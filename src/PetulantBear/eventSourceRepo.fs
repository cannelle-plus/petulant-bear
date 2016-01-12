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
open Logary

type Status =
    | Connected
    | Closed

type Repository(connection:SQLiteConnection, eventStoreConnectionString) =

    let mutable status = Closed
    let defaultUserCredentials = new SystemData.UserCredentials("admin","changeit")
    let settingsBuilder = ConnectionSettings.Create()
                                     .KeepRetrying()
                                     .KeepReconnecting()
//                                     .SetDefaultUserCredentials(defaultUserCredentials)
//                                     .SetOperationTimeoutTo(TimeSpan.FromSeconds(5 |> float))
//                                     .EnableVerboseLogging()
//                                     .FailOnNoServerResponse()
//                                     .SetReconnectionDelayTo(TimeSpan.FromSeconds(15 |> float))
                                     
    
    let conn =  EventStore.ClientAPI.EventStoreConnection.Create(settingsBuilder.Build(),new Uri(eventStoreConnectionString),"Bear2BearESConnection")
    let createNameAgg streamName id = sprintf "%s-%s" streamName (id.ToString())

    let subscriptionsFactory = new Dictionary<Guid, unit -> EventStoreStreamCatchUpSubscription>()
    let activeSubscriptions = new Dictionary<Guid, EventStoreStreamCatchUpSubscription>()
    
    let onClosed (evtArgs:ClientClosedEventArgs) = 
        status <- Closed
        sprintf "closed connection %A" evtArgs
        |> LogLine.create' LogLevel.Info 
        |> Logging.getCurrentLogger().Log

    let onConnected (evtArgs:ClientConnectionEventArgs) = 
        status <- Connected
        sprintf "conected %A" evtArgs
        |> LogLine.create' LogLevel.Info 
        |> Logging.getCurrentLogger().Log

    let onDisconnected (evtArgs:ClientConnectionEventArgs) = 
        status <- Closed
        sprintf "disconected %A" evtArgs
        |> LogLine.create' LogLevel.Info 
        |> Logging.getCurrentLogger().Log

    let onErrorOccurred (evtArgs:ClientErrorEventArgs) = 
        status <- Closed
        sprintf "error occured %A" evtArgs
        |> LogLine.create' LogLevel.Info 
        |> Logging.getCurrentLogger().Log

    let onReconnecting (evtArgs:ClientReconnectingEventArgs) = 
        
        sprintf "reconnecting occured %A" evtArgs
        |> LogLine.create' LogLevel.Info 
        |> Logging.getCurrentLogger().Log

    let runSubscription subscriptionId = 
        if activeSubscriptions.ContainsKey subscriptionId then
            activeSubscriptions.[subscriptionId].Stop()
            activeSubscriptions.Remove(subscriptionId)|> ignore

        if subscriptionsFactory.ContainsKey(subscriptionId) then
            activeSubscriptions.Add(subscriptionId,subscriptionsFactory.[subscriptionId]()) 
    
        
    let onError subscriptionId onerror (escus:EventStoreCatchUpSubscription) (sdr:SubscriptionDropReason) (e:Exception) = 
            
        let logError msgError =
            sprintf "error! - SubscriptionDropReason= %s, exception : %A" msgError e
            |> Logary.LogLine.error 
            |> Logary.Logging.getCurrentLogger().Log

        onerror escus sdr e
    
        match sdr with
            | SubscriptionDropReason.AccessDenied -> logError "AccessDenied" 
            | SubscriptionDropReason.CatchUpError ->  
                logError "CatchUpError" 
            | SubscriptionDropReason.ConnectionClosed -> logError "ConnectionClosed" 
            | SubscriptionDropReason.EventHandlerException -> 
                logError "EventHandlerException" 
            | SubscriptionDropReason.MaxSubscribersReached -> logError "MaxSubscribersReached" 
            | SubscriptionDropReason.UserInitiated -> logError "UserInitiated" 
            | SubscriptionDropReason.NotAuthenticated -> logError "NotAuthenticated" 
            | SubscriptionDropReason.NotFound -> logError "NotFound" 
            | SubscriptionDropReason.PersistentSubscriptionDeleted -> logError "PersistentSubscriptionDeleted" 
            | SubscriptionDropReason.ProcessingQueueOverflow -> logError "ProcessingQueueOverflow" 
            | SubscriptionDropReason.ServerError -> logError "ServerError" 
            | SubscriptionDropReason.SubscribingError -> logError "SubscribingError" 
            | _ -> logError "Unknown" 

        
    interface IEventStoreRepository with 

        member this.Connect()=
            conn.Closed.Add(onClosed)
            conn.Connected.Add(onConnected)
            conn.Disconnected.Add(onDisconnected)
            conn.ErrorOccurred.Add(onErrorOccurred)
            conn.Reconnecting.Add(onReconnecting)

            conn.ConnectAsync().Wait()
            status <- Connected
            conn

        member this.IsCommandProcessed idCommand =
            

            let sql = " select count(*) from commandProcessed where commandId=@cmdId"
            use sqlCmd = new SQLiteCommand(sql, connection) 

            let add (name:string, value: string) = 
                sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

            add("@cmdId", idCommand.ToString())

            use reader = sqlCmd.ExecuteReader() 

            (reader.Read() && Int32.Parse(reader.[0].ToString())>0)

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
        
        member this.SubscribeToStreamFrom name (lastCheckPoint:Nullable<int>) (resolveLinkTo:bool) (projection:Projection) =

            let subscriptionId = Guid.NewGuid()
            
            let evtAppeared =projection.eventAppeared connection
            let evtAppeared = Action<EventStoreCatchUpSubscription,ResolvedEvent> evtAppeared
            let catchUp = Action<EventStoreCatchUpSubscription> projection.catchup
            let error = Action<EventStoreCatchUpSubscription, SubscriptionDropReason, Exception> (onError subscriptionId projection.onError)

            subscriptionsFactory.Add(subscriptionId , (fun () -> 
                conn.SubscribeToStreamFrom(name, lastCheckPoint, resolveLinkTo, evtAppeared, catchUp, error, defaultUserCredentials, 500) )
            )

            //deals with reconnection
            let onReconnection (evArgs:ClientReconnectingEventArgs) = 
                sprintf "SubscribeToStreamFrom: onReconnection... name : %s subscriptionId :%A " name subscriptionId
                |> Logary.LogLine.create' Logary.LogLevel.Info
                |> Logary.Logging.getCurrentLogger().Log

            conn.Reconnecting.Add onReconnection

            runSubscription subscriptionId
    
    interface IDisposable with 
        member this.Dispose()  =
            for entry in activeSubscriptions do entry.Value.Stop()
            

let create dbConnection cs = (new Repository(dbConnection,cs))
