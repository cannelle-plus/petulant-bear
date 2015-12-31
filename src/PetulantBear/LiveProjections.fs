module PetulantBear.Projections.Live

open System
open System.Net
open System.IO
open System.Reflection
open EventStore.ClientAPI
open EventStore.ClientAPI.Exceptions
open EventStore.ClientAPI.Projections

open PetulantBear.Projections.CatchUp
open PetulantBear.Projections.Common

open System.Data.SQLite
open System.Collections.Generic
open Newtonsoft.Json

type LiveEvent<'a> = {
    Enveloppe : Enveloppe;
    PayLoad : JsonEvent<'a>
}

let liveProjection  (ctx:ProjectionsCtx) =

    
    let runningSubscriptions = new Dictionary<Guid,(string -> bool)>()

    let subscribe idWebSocket push =
        if not <| runningSubscriptions.ContainsKey idWebSocket then
            runningSubscriptions.Add(idWebSocket, push)
        else () //log something ??

    let unsubscribe id = runningSubscriptions.Remove(id) |> ignore

    let eventAppeared ess (re:ResolvedEvent)= 
        let json = System.Text.Encoding.UTF8.GetString( re.Event.Data);
        let jsonEvent = JsonConvert.DeserializeObject<JsonEvent<_>>(json)

        let jsonMeta = System.Text.Encoding.UTF8.GetString( re.Event.Metadata);
        let meta = JsonConvert.DeserializeObject<Enveloppe>(jsonMeta);

        sprintf "eventAppeared live subscription:, jsonMeta: %A, json %A"  jsonMeta json 
        |> ctx.logger.Info

        let  evt = {
            Enveloppe = meta;
            PayLoad = jsonEvent;
        }
        for entry in runningSubscriptions do
            let k = entry.Key
            let f = entry.Value
            if not <| f ( toJsonString(evt)) then unsubscribe k
        
    let subscriptionDropped  (ess:EventStoreSubscription) (sdr:SubscriptionDropReason) (exn:exn) = 
        //what to do here ? Log?
        sprintf "EventStoreSubscription %A:, SubscriptionDropReason: %A, exn %A" ess sdr exn
        |> ctx.logger.Error
        ()
    
    Logary.LogLine.error "subscribing to live projection"
    |> Logary.Logging.getCurrentLogger().Log

    ctx.projectionRepo.SubscribeToLiveStream "petulantBearProjection" true eventAppeared subscriptionDropped

    subscribe,unsubscribe
            
let create (projectionRepo:IEventStoreProjection ) (connection:SQLiteConnection) (httpEndPoint:IPEndPoint) = 
    let log = new EventStore.ClientAPI.Common.Log.ConsoleLogger()
    let defaultUserCredentials = new SystemData.UserCredentials("admin","changeit")
    let ctx = { 
        logger  = log
        ep      = httpEndPoint
        timeout = new TimeSpan(0,0,10)
        creds   = defaultUserCredentials
        connection = connection
        projectionRepo = projectionRepo }
    liveProjection (ctx )