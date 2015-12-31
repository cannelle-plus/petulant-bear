module PetulantBear.EventSourceRepo

open System
open System.Threading
open System.Text
open System.Threading.Tasks

open Newtonsoft.Json
open EventStore.ClientAPI
open EventStore.ClientAPI.Common.Log

open System.Data.SQLite
open System.Collections.Generic

type Repository(connection:SQLiteConnection, connectionString) =
    let defaultUserCredentials = new SystemData.UserCredentials("admin","changeit")
    let settingsBuilder = ConnectionSettings.Create()
                                     .SetDefaultUserCredentials(defaultUserCredentials)
                                     .KeepRetrying()
                                     .KeepReconnecting()

    let createNameAgg streamName id = sprintf "%s-%s" streamName (id.ToString())
    let conn = EventStore.ClientAPI.EventStoreConnection.Create(settingsBuilder.Build(),new Uri(connectionString),"Bear2BearESConnection")

    let catchupSubscriptions = new Dictionary<Guid, EventStoreCatchUpSubscription>()
    let activeSubscriptions = new Dictionary<Guid, EventStoreSubscription>()
        
    interface IEventStoreRepository with 

        member this.Connect()=
            conn.ConnectAsync().Wait()

        member this.IsCommandProcessed idCommand =
            

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

                (exists>0)
            else
                reader.Dispose() 
                sqlCmd.Dispose()
                false

        member this.SaveCommandProcessedAsync (cmd:Command<_>) = async {

            let sql = " insert into  commandProcessed (commandId) values(@cmdId);"
            use sqlCmd = new SQLiteCommand(sql, connection) 

            let add (name:string, value: string) = 
                sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

            add("@cmdId", cmd.idCommand.ToString())

            sqlCmd.ExecuteNonQuery()  |> ignore

            sqlCmd.Dispose()
        }
        
        member this.SaveEvtsAsync  name  enveloppe evts =  async {
                
                let envInitial = enveloppe 0;

                let events = evts
                                |> List.mapi (fun index e ->
                                            let typeEvts = e.GetType().Name
                                            let data=  toJson e 
                                            let enveloppeMetadata = toJson(enveloppe(index))
                                            new EventData(envInitial.aggregateId,typeEvts,true, data,enveloppeMetadata)
                                            )


                let expectedVersion = envInitial.version
                let streamName = createNameAgg name (envInitial.aggregateId.ToString())


                printfn "uncommitted events have been produced according to version %i" expectedVersion
                let! writeResult = conn.AppendToStreamAsync(streamName,expectedVersion,events) |> Async.AwaitTask

                printfn "events appended to %s, next expected version : %i"  streamName writeResult.NextExpectedVersion

            }

        member this.HydrateAggAsync<'TAgg,'TEvent>  streamName (applyTo:'TAgg -> 'TEvent ->'TAgg) (initialState:'TAgg)  (id:Guid) =

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
        

        member this.SubscribeToLiveStream name (resolveLinkTo:bool) eventAppeared subscriptionDropped =

            let subscriptionId = Guid.NewGuid()
            
            let createSubscription() = 
                let ea = Action<EventStoreSubscription,ResolvedEvent> eventAppeared
                let sd = Action<EventStoreSubscription,SubscriptionDropReason,exn> subscriptionDropped
                
                let subscription =
                    conn.SubscribeToStreamAsync(name,  resolveLinkTo, ea, sd,  defaultUserCredentials) 
                    |> Async.AwaitTask 
                    |> Async.RunSynchronously
            
                activeSubscriptions.Add(subscriptionId,subscription) 

            createSubscription()

            //deals with reconnection
            let onReconnection (evArgs:ClientReconnectingEventArgs) = 
                sprintf "SubscribeToLiveStream: onReconnection... name : %s subscriptionId :%A " name subscriptionId
                |> Logary.LogLine.create' Logary.LogLevel.Info
                |> Logary.Logging.getCurrentLogger().Log

                if activeSubscriptions.ContainsKey subscriptionId then
                    activeSubscriptions.[subscriptionId].Close()
                    activeSubscriptions.Remove(subscriptionId)|> ignore
                createSubscription()

            conn.Reconnecting.Add onReconnection
            

        member this.SubscribeToStreamFrom name (lastCheckPoint:Nullable<int>) (resolveLinkTo:bool) (projection:Projection) =

            let subscriptionId = Guid.NewGuid()

            let createSubscription() = 
                let evtAppeared =projection.eventAppeared connection
                let evtAppeared = Action<EventStoreCatchUpSubscription,ResolvedEvent> evtAppeared
                let catchUp = Action<EventStoreCatchUpSubscription> projection.catchup
                let onError = Action<EventStoreCatchUpSubscription, SubscriptionDropReason, Exception> projection.onError

                let subscription = conn.SubscribeToStreamFrom(name, lastCheckPoint, resolveLinkTo, evtAppeared, catchUp, onError, defaultUserCredentials, 500) 

                catchupSubscriptions.Add(subscriptionId,subscription) 

            createSubscription()

            //deals with reconnection
            let onReconnection (evArgs:ClientReconnectingEventArgs) = 
                sprintf "SubscribeToStreamFrom: onReconnection... name : %s subscriptionId :%A " name subscriptionId
                |> Logary.LogLine.create' Logary.LogLevel.Info
                |> Logary.Logging.getCurrentLogger().Log

                if activeSubscriptions.ContainsKey subscriptionId then
                    activeSubscriptions.[subscriptionId].Close()
                    activeSubscriptions.Remove(subscriptionId)|> ignore
                createSubscription()

            conn.Reconnecting.Add onReconnection
    
    interface IDisposable with 
        member this.Dispose()  =
            for entry in activeSubscriptions do entry.Value.Dispose()
            for entry in catchupSubscriptions do entry.Value.Stop()
            ()

let create dbConnection cs  = (new Repository(dbConnection,cs))
