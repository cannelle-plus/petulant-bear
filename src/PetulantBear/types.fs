[<AutoOpen>]
module types

open System
open System.Configuration   
open System.Collections.Generic
open System.Runtime.Serialization
open System.Data.SQLite
open Akka.Actor

open Suave // always open suave
open Suave.Types
open Suave.Http
open Suave.Http.Applicatives
open Suave.Http.Writers

open Suave.Http.Successful // for OK-result
open Suave.Web // for config
open Suave.State.CookieStateStore


open Newtonsoft.Json


type BearSession = {
    bearId : Guid;
    socialId : string;
    username : string;
}


type NewBearNotLoggedOnSession = {
    socialId : string
}
type Session =
    | NoSession
    | NewBear of NewBearNotLoggedOnSession
    | Bear of BearSession

[<DataContract>]
type Command<'a> = {
    [<field: DataMember(Name = "id")>]
    id : Guid;
    [<field: DataMember(Name = "idCommand")>]
    idCommand : Guid;
    [<field: DataMember(Name = "version")>]
    version : Nullable<int>;
    [<field: DataMember(Name = "payLoad")>]
    payLoad : 'a; 
}

[<DataContract>]
type Request = {
    [<field: DataMember(Name = "id")>]
    id : Guid;
    [<field: DataMember(Name = "version")>]
    version : Nullable<int>;
}

//[<DataContract>]
//type Event<'a> = {
//    [<field: DataMember(Name = "id")>]
//    id : Guid;
//    [<field: DataMember(Name = "version")>]
//    version : int;
//    [<field: DataMember(Name = "payLoad")>]
//    payLoad : 'a; 
//}

[<DataContract>]
type Enveloppe = {
    [<field: DataMember(Name = "messageId")>]
    messageId : Guid;
    [<field: DataMember(Name = "correlationId")>]
    correlationId : Guid;
    [<field: DataMember(Name = "aggregateId")>]
    aggregateId: Guid;
    [<field: DataMember(Name = "version")>]
    version : int;
    [<field: DataMember(Name = "bear")>]
    bear : BearSession;
}

let createEnveloppe c a b v = 
    (fun i -> 
        { 
            messageId = Guid.NewGuid();
            correlationId = c;
            aggregateId = a;
            version = v+i;
            bear = b;
        }
    )



type Result<'TSuccess> =
    | Success of 'TSuccess
    | Failure of string

let toMessage = function
    | Success(cmd) -> { msg = "OK"  }    
    | Failure(reason) -> { msg = reason  }


type PubSubMsg<'a> = 
    | Subscribe of IActorRef
    | Unsubscribe of IActorRef
    | Publish of Guid*int*'a

type AggregateCoordinatorMsg =
    | Hydrate of Guid*IActorRef
    | Dehydrate of Guid

type httpActorMsg<'TAggCommand> =
    | HandleRequest 
    | Hydrated of IActorRef
    | ResultFound of Result<'TAggCommand>



let tryCatch f x =
    match x with  
    | Success(cmd) -> 
        try
            f cmd  |> Success
        with
        | ex -> Failure(ex.Message)    
    | Failure(msg) -> x



let ApplyCmdToActor buildGameActor (actorsDic :Dictionary<Guid, IActorRef>) (cmd:Command<'a>) =
    if not <| actorsDic.ContainsKey(cmd.id) then
        actorsDic.Add(cmd.id, buildGameActor cmd.id)
    actorsDic.[cmd.id].Tell(cmd)


type DeserializingResult<'T> =
| Serialized of 'T
| DeserializingException of string*Newtonsoft.Json.JsonException

let fromJson<'a> byteArray= 
    let json = System.Text.Encoding.UTF8.GetString(byteArray) 
    try
        Serialized(JsonConvert.DeserializeObject<'a>(json))
    with
    | :?Newtonsoft.Json.JsonException as ex ->  DeserializingException(json,ex)
    | _ -> reraise()




let toJson obj = 
    JsonConvert.SerializeObject(obj)
    |> System.Text.Encoding.UTF8.GetBytes

let toJsonString obj = 
    JsonConvert.SerializeObject(obj)

let deserializingRawForm<'T> req fSuccess =
    match fromJson<'T> req.rawForm with
        | Serialized(cmd) -> fSuccess cmd
        | DeserializingException(rawText,ex) ->
            let t = typedefof<'T>
            let elmahLogger = Logary.Logging.getLoggerByName "elmah"
            ex.ToString()
            |> sprintf "raw form sent : '%s', serialization type : '%s', err trace : '%s'" rawText ((typedefof<'T>).FullName)
            |> Logary.LogLine.error 
            |> elmahLogger.Log 
            RequestErrors.BAD_REQUEST "body not understood"

let deserializingCmd<'T> req fSuccess =
    match fromJson<Command<'T>> req.rawForm with
        | Serialized(cmd) -> 
            fSuccess cmd
        | DeserializingException(rawText,ex) ->
            let logger = Logary.Logging.getLoggerByName "Logary.Targets.ElmahIO"
            
            sprintf "raw form sent : '%s', serialization type : '%s'" rawText ((typedefof<'T>).FullName)
            |> Logary.LogLine.error
            |> Logary.LogLine.setExn ex
            |> logger.Log
            
            RequestErrors.BAD_REQUEST "body not understood"




    
let socialIdStore = "socialId"
let accessTokenStore = "accessToken"
let bearStore = "bearInSession"
let userNameStore = "username"


let noCache = 
  setHeader "Cache-Control" "no-cache, no-store, must-revalidate"
  >>= setHeader "Pragma" "no-cache"
  >>= setHeader "Expires" "0"
  


let sessionCmd<'Tcontract> f=
    statefulForSession 
    >>= context (fun x ->
        deserializingCmd<'Tcontract> x.request (fun cmd ->
            let store = x |> HttpContext.state
            match  store with
            | None -> f NoSession cmd
            | Some state ->
                match state.get bearStore,state.get socialIdStore, state.get userNameStore  with
                | Some bearId, Some socialId, Some username -> f (Bear { bearId= bearId; socialId=socialId; username = username} ) cmd
                | None ,  Some socialId , None-> f (NewBear {socialId = socialId}) cmd
                | _ , _,_ -> f NoSession cmd
            )   

        )


let session f=
    statefulForSession 
    >>= context (fun x ->
            let store = x |> HttpContext.state
            match  store with
            | None -> f NoSession 
            | Some state ->
                match state.get bearStore,state.get socialIdStore, state.get userNameStore  with
                | Some bearId, Some socialId, Some username -> f (Bear { bearId= bearId; socialId=socialId; username = username} ) 
                | None ,  Some socialId , None-> f (NewBear {socialId = socialId}) 
                | _ , _,_ -> f NoSession 
        )

let inSession f =
    context (fun x ->
        let store = x |> HttpContext.state
        match  store with
        | None -> f NoSession
        | Some state ->
            let b = state.get bearStore
            let s = state.get socialIdStore
            let u = state.get userNameStore
            match b,s , u with
            | Some bearId, Some socialId , Some username-> f (Bear { bearId= bearId; socialId=socialId; username = username} )
            | None ,  Some socialId , None-> f (NewBear {socialId = socialId})
            | _ , _ , _-> f NoSession 
        )

let apply<'TContract,'TCommand> save (mapCmd:'TContract->'TCommand) =
    sessionCmd<'TContract>(fun s cmd ->
        match s  with
        | NoSession -> Http.RequestErrors.BAD_REQUEST "no session found" 
        | NewBear nb -> Http.RequestErrors.BAD_REQUEST "new bear found" 
        | Bear b -> 
            try
                Success(mapCmd(cmd.payLoad))
                |> tryCatch (save (cmd.id,cmd.version,b)) 
                |> toMessage
                |> toJson
                |> Successful.ok 
                >>= Writers.setMimeType "application/json"
            with
            | ex -> 
                Failure(ex.Message) 
                |> toMessage
                |> toJson
                |> Successful.ok 
                >>= Writers.setMimeType "application/json"
    )


let toJsonFromOptions = function 
| Some(a) -> toJson(a) 
| None ->  toJson("")

let mapJson<'a,'b> (f:'a->'b option) = 
    Types.request(fun r ->
        deserializingRawForm<'a> r (fun cmd ->
            f cmd
            |> toJsonFromOptions
            |> Successful.ok
            >>= Writers.setMimeType "application/json"
        )
    ) 



let passHash (pass: string) =
    use sha = Security.Cryptography.SHA256.Create()
    Text.Encoding.UTF8.GetBytes(pass)
    |> sha.ComputeHash
    |> Array.map (fun b -> b.ToString("x2"))
    |> String.concat ""


let getInSession f =
    session (fun s ->
        match s with 
        | NoSession -> Http.RequestErrors.BAD_REQUEST "no session found" 
        | NewBear nb -> Http.RequestErrors.BAD_REQUEST "new bear found" 
    
        | Bear b -> f b.bearId
    )
    
let withBear  =
    statefulForSession 
    >>= context (fun x ->
            let store = x |> HttpContext.state
            match  store with
            | None -> Http.RequestErrors.BAD_REQUEST "no state found"
            | Some state ->
                match state.get bearStore,state.get socialIdStore, state.get userNameStore  with
                | Some bearId, Some socialId, Some username -> 
                    Writers.setUserData "bear"  { bearId= bearId; socialId=socialId; username = username}
                | None ,  Some (_) , None->Http.RequestErrors.BAD_REQUEST "new bear found"
                | _ , _,_ -> Http.RequestErrors.BAD_REQUEST "no session found" 
        )

let logErrorWithBear x bear name  =
    let logger = Logary.Logging.getLoggerByName "Logary.Targets.ElmahIO"
            
    Logary.LogLine.warn name
    |> Logary.LogLine.setData "bear" bear
    |> Logary.LogLine.setData "url" x.request.url
    |> Logary.LogLine.setData "headers" x.request.headers
    |> logger.Log

let withCommand<'T> = 
    context (fun x ->
        match fromJson<Command<'T>> x.request.rawForm with
            | Serialized(cmd) -> Writers.setUserData "cmd"  cmd
            | DeserializingException(rawText,ex) ->

                let bear = x.userState.["bear"] :?> BearSession
                let logger = Logary.Logging.getLoggerByName "Logary.Targets.ElmahIO"
            
                Logary.LogLine.error "DeserializingException"
                |> Logary.LogLine.setExn ex
                |> Logary.LogLine.setData "bear" bear
                |> Logary.LogLine.setData "url" x.request.url
                |> Logary.LogLine.setData "url" x.request.url
                |> Logary.LogLine.setData "rawForm" rawText
                |> Logary.LogLine.setData "serializationType" (typedefof<'T>).FullName
                |> Logary.LogLine.setData "headers" x.request.headers
                
                |> logger.Log
            
                RequestErrors.BAD_REQUEST "body not understood"
    )

open EventStore.ClientAPI

type Projection ={
    resetProjection : SQLiteConnection -> unit
    eventAppeared : SQLiteConnection -> EventStoreCatchUpSubscription->ResolvedEvent->unit
    catchup : EventStoreCatchUpSubscription -> unit
    onError : EventStoreCatchUpSubscription -> SubscriptionDropReason -> Exception ->unit
}


type IEventStoreRepository =
    abstract member Connect: unit->unit 
    abstract member IsCommandProcessed : Guid -> bool
    abstract member SaveCommandProcessedAsync : Command<'a> -> Async<unit>
    abstract member SaveEvtsAsync<'T> : string -> (int ->Enveloppe) -> 'T list -> Async<unit>
    abstract member HydrateAggAsync<'TAgg,'TEvents> : string -> ('TAgg -> 'TEvents -> 'TAgg) -> 'TAgg -> Guid -> Async<'TAgg*int>


type IEventStoreProjection =
    inherit IDisposable
    abstract member SubscribeToLiveStream:  string ->  bool -> ( EventStoreSubscription ->ResolvedEvent->unit) -> (EventStoreSubscription -> SubscriptionDropReason -> exn -> unit) ->unit
    abstract member SubscribeToStreamFrom:  string -> Nullable<int> -> bool -> Projection -> unit

let processingCommand<'TAgg, 'TContract,'TCommand, 'TEvents>   (repo:IEventStoreRepository) streamName apply initialState (exec:'TAgg -> 'TCommand -> Choice<'TEvents list,string list>)  (mapCmd:'TContract->'TCommand) x =
     async {
        // cehck global context first
        if not <| x.userState.ContainsKey "cmd" || not <| x.userState.ContainsKey "bear" then 
            return Some(x)
        else
            let cmd = x.userState.["cmd"] :?> Command<'TContract>
            let bear = x.userState.["bear"] :?> BearSession
            let idAggregate = cmd.id
        
            // if there is no version it means the aggregate does not support event sourcing
            if (not <| repo.IsCommandProcessed cmd.idCommand) && cmd.version.HasValue then
                let! state,v = repo.HydrateAggAsync<'TAgg,'TEvents> streamName  apply initialState idAggregate

                match cmd.payLoad |> mapCmd |> exec state  with
                | Choice1Of2(evts) ->
                    let newState = List.fold apply state evts
                    let enveloppe = createEnveloppe  cmd.idCommand cmd.id bear v
                    do! repo.SaveEvtsAsync<'TEvents> streamName enveloppe evts
                    do! repo.SaveCommandProcessedAsync cmd

                    return! (Success("toto")
                        |> toMessage
                        |> toJson
                        |> Successful.ok 
                        >>= Writers.setMimeType "application/json") x
                | Choice2Of2(errors) ->
                    // do comething here like log errors and return errors codes to warn the user
                    let msg = errors |> List.fold (fun agg err -> sprintf "%s;%s" err agg ) ""
                    logErrorWithBear x bear msg
                    return! RequestErrors.BAD_REQUEST msg x
            else
                let msg = "command already processed"
                logErrorWithBear x bear msg
                return! RequestErrors.BAD_REQUEST msg x
    }


let processingHttpContext<'TContract,'TCommand> save (mapCmd:'TContract->'TCommand) x =
    try
            let cmd = x.userState.["cmd"] :?> Command<'TContract>
            let bear = x.userState.["bear"] :?> BearSession

            if not <| cmd.version.HasValue then
                cmd.payLoad
                |> mapCmd
                |> Success
                |> tryCatch (save (cmd.id,cmd.version,bear)) 
                |> toMessage
                |> toJson
                |> Successful.ok 
                >>= Writers.setMimeType "application/json"
            else
                Success(cmd)
                |> toMessage
                |> toJson
                |> Successful.ok 
                >>= Writers.setMimeType "application/json"
        with
        | ex -> 
            Failure(ex.Message) 
            |> toMessage
            |> toJson
            |> Successful.ok 
            >>= Writers.setMimeType "application/json"

let processing<'TContract,'TCommand> save (mapCmd:'TContract->'TCommand) = 
    processingHttpContext save mapCmd
    |> context
    