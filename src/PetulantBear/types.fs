[<AutoOpen>]
module types

open System
open System.Collections.Generic
open System.Runtime.Serialization
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



[<DataContract>]
type Command<'a> = {
    [<field: DataMember(Name = "id")>]
    id : Guid;
    [<field: DataMember(Name = "version")>]
    version : int;
    [<field: DataMember(Name = "payLoad")>]
    payLoad : 'a; 
}

[<DataContract>]
type Request = {
    [<field: DataMember(Name = "id")>]
    id : Guid;
    [<field: DataMember(Name = "version")>]
    version : int;
}

[<DataContract>]
type Event<'a> = {
    [<field: DataMember(Name = "id")>]
    id : Guid;
    [<field: DataMember(Name = "version")>]
    version : int;
    [<field: DataMember(Name = "payLoad")>]
    payLoad : 'a; 
}

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
| DeserializingException of string*Newtonsoft.Json.JsonSerializationException

let fromJson<'a> byteArray= 
    let json = System.Text.Encoding.UTF8.GetString(byteArray) 
    try
        Serialized(JsonConvert.DeserializeObject<'a>(json))
    with
    | :?Newtonsoft.Json.JsonSerializationException as ex ->  
        DeserializingException(json,ex)
    | _ -> reraise()
                

let toJson<'a> (obj:'a) = 
    JsonConvert.SerializeObject(obj)
    |> System.Text.Encoding.UTF8.GetBytes

let toJsonString<'a> (obj:'a) = 
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
        | Serialized(cmd) -> fSuccess cmd
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

type BearSession = {
    bearId : Guid;
    socialId : string;
    username : string;
}


type NewBearNotLoggedOnSession = {
    socialId : string
}

let noCache = 
  setHeader "Cache-Control" "no-cache, no-store, must-revalidate"
  >>= setHeader "Pragma" "no-cache"
  >>= setHeader "Expires" "0"
  

type Session =
    | NoSession
    | NewBear of NewBearNotLoggedOnSession
    | Bear of BearSession

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


let toJsonFromOptions<'T> = function 
| Some(a) -> toJson<'T>(a) 
| None ->  toJson("")

let mapJson<'a,'b> f = 
    Types.request(fun r ->
        deserializingRawForm<'a> r (fun cmd ->
            f cmd
            |> toJsonFromOptions<'b>
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
    