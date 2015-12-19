module PetulantBear.EventSourceRepo

open System
open System.Threading
open System.Text
open System.Threading.Tasks

open Newtonsoft.Json
open EventStore.ClientAPI
open EventStore.ClientAPI.Common.Log

open System.Data.SQLite

type Repository(dbConnection:string, connectionString) =
    let defaultUserCredentials = new SystemData.UserCredentials("admin","changeit")
    let settingsBuilder = ConnectionSettings.Create()
                                     .SetDefaultUserCredentials(defaultUserCredentials)
                                     .FailOnNoServerResponse()
                                     .KeepReconnecting()

    let createNameAgg streamName id = sprintf "%s-%s" streamName (id.ToString())
    let conn = EventStore.ClientAPI.EventStoreConnection.Create(settingsBuilder.Build(),new Uri(connectionString),"Bear2BearESConnection")
    
    interface IEventStoreRepository with 

        member this.connect()=
            conn.ConnectAsync().Wait()

        member this.isCommandProcessed idCommand =
            use connection = new SQLiteConnection(dbConnection)
            connection.Open()

            let sql = " select count(*) from commandProcessed where commandId=@cmdId"
            use sqlCmd = new SQLiteCommand(sql, connection) 

            let add (name:string, value: string) = 
                sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

            add("@cmdId", idCommand.ToString())

            use reader = sqlCmd.ExecuteReader() 

            if reader.Read() then
                let exists = Int32.Parse(reader.[0].ToString())
                
                reader.Dispose()
                sqlCmd.Dispose()
                connection.Dispose()
                GC.Collect()
                if exists>0 then true else false
            else   
                reader.Dispose() 
                sqlCmd.Dispose()
                connection.Dispose()
                GC.Collect() 
                false

        member this.saveCommandProcessedAsync (cmd:Command<_>) = async {
            use connection = new SQLiteConnection(dbConnection)
            connection.Open()

            let sql = " insert into  commandProcessed (commandId) values(@cmdId);"
            use sqlCmd = new SQLiteCommand(sql, connection) 

            let add (name:string, value: string) = 
                sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

            add("@cmdId", cmd.idCommand.ToString())

            sqlCmd.ExecuteNonQuery()  |> ignore

            sqlCmd.Dispose()
            connection.Dispose()
            GC.Collect()
        }
        
        member this.saveEvtsAsync  name  enveloppe evts =  async {
                
                let envInitial = enveloppe 0;

                let events = evts
                                |> List.mapi (fun index e ->
                                            let typeEvts = e.GetType().Name
                                            let data=  toJson e 
                                            let enveloppeMetadata = toJson<Enveloppe>(enveloppe(index))
                                            new EventData(envInitial.aggregateId,typeEvts,true, data,enveloppeMetadata)
                                            )


                let expectedVersion = envInitial.version
                let streamName = createNameAgg name (envInitial.aggregateId.ToString())


                printfn "uncommitted events have been produced according to version %i" expectedVersion
                let! writeResult = conn.AppendToStreamAsync(streamName,expectedVersion,events) |> Async.AwaitTask

                printfn "events appended to %s, next expected version : %i"  streamName writeResult.NextExpectedVersion

            }

        member this.hydrateAggAsync<'TAgg,'TEvent>  streamName (applyTo:'TAgg -> 'TEvent ->'TAgg) (initialState:'TAgg)  (id:Guid) =

            let rec applyRemainingEvents evts =
                match evts with
                | [] ->  initialState
                | head :: tail ->  applyTo (applyRemainingEvents tail) head


            async {

                printfn "reading stream events..."
                let n = createNameAgg streamName id
                let! slice = conn.ReadStreamEventsForwardAsync(n,0,99,false) |> Async.AwaitTask

                let evts =
                    slice.Events
                    |> Seq.map (fun (e:ResolvedEvent) -> JsonConvert.DeserializeObject<'TEvent>(System.Text.Encoding.UTF8.GetString(e.Event.Data)))
                    |> Seq.toList
                printfn "%i events read" evts.Length
                evts |> List.iteri (fun i e-> printfn "applying event %i" i)
                let (aggregate:'TAgg) = evts |> applyRemainingEvents 

                return aggregate,evts.Length-1
            }
            
    interface IEventStoreProjection with 
        member this.SubscribeToStreamFrom name (lastCheckPoint:Nullable<int>) (resolveLinkTo:bool) (projection:Projection) =
            let evtAppeared = Action<EventStoreCatchUpSubscription,ResolvedEvent> projection.eventAppeared
            let catchUp = Action<EventStoreCatchUpSubscription> projection.catchup
            let onError = Action<EventStoreCatchUpSubscription, SubscriptionDropReason, Exception> projection.onError
            conn.SubscribeToStreamFrom(name, lastCheckPoint, resolveLinkTo, evtAppeared, catchUp, onError, defaultUserCredentials, 500) 

let create dbConnection cs = (new Repository(dbConnection,cs))
