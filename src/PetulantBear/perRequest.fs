[<AutoOpen>]
module PerRequest

open Suave // always open suave
open Suave.Http
open Suave.Http.Applicatives

open Suave.Http.Successful // for OK-result
open Suave.Web // for config
open Suave.Types

open Akka.Actor
open Akka.FSharp

open System
open System.Threading.Tasks

let toMessage<'a> (result:Result<'a>) = 
    match result with 
    | Success(cmd) -> { msg = "OK"  }    
    | Failure(reason) -> { msg = reason  }    


type StateRequest = {
    isHandled : bool;
}
with static member Initial = { isHandled= false }

//let getCmdFromContract<'TContract,'TCommands> (toCmd:'TContract -> 'TCommands) ctx =
//     //get the command part of the request
//    let cmd = fromJson<Command<'TContract>> ctx.request.rawForm
//    //decompose the pack in id,version, cmd
//    cmd.id, cmd.version:>obj, (toCmd(cmd.payLoad)):> obj

//let getCmdFromRequest<'TCommands> toCmd ctx =
//     //get the command part of the request
//    let cmd = fromJson<Command<'TCommands>> ctx.request.rawForm
//    //decompose the pack in id,version, cmd
//    cmd.id, cmd.version:>obj, toCmd:> obj

let  handleRequest<'TAggCommand> system (aggCoordinator :IActorRef) getCmdFromJson =

//    // The per requestActor handle a request ie : id, version , command
//    // it asks the aggCoordinator the reference to the aggregate, and then tells this aggregate to try to execute the command for this version
//    // when the aggregate has finished computing it tells it back the this perrequest actor that triggers the task setting its result and gracefully stops
//    let perRequestActor (tcs :TaskCompletionSource<Result<'TAggCommand>>) (aggCoordinator :IActorRef)  id version  bearId cmd  =
//        fun (mailbox:Actor<httpActorMsg<'TAggCommand>>) ->
//            let rec loop state = actor {
//                let! message = mailbox.Receive()
//                let newState = 
//                    match message with 
//                    | HandleRequest ->
//                        aggCoordinator <! Hydrate (id,mailbox.Self) 
//                        state
//                    | Hydrated(aggActor)->
//                        aggActor <! (id,version,bearId, cmd)
//                        state
//                    | ResultFound(result) ->
//                        if not <| state.isHandled then 
//                            tcs.SetResult(result)
//                            mailbox.Self <! PoisonPill.Instance
//                        { state with isHandled= true}
//                return! loop newState 
//            }
//            loop StateRequest.Initial  
//
//    // async function returning a task to allow let! of the result from the per request actor
//    let tellActorAsync system (aggCoordinator :IActorRef) id version bearId command =
//        //create a new task
//        let tcs = new TaskCompletionSource<Result<'TAggCommand>>()
//        //spawn an actor for this request
//        let requestName = sprintf "requestActor-%s" (Guid.NewGuid().ToString())
//        let requestActor = spawn system requestName <| perRequestActor tcs aggCoordinator id version bearId command
//        //tell the actor to start the computation
//        requestActor <! (HandleRequest:httpActorMsg<'TAggCommand>)
//        //return the task that should be completed by the request actor just spawned
//        tcs.Task

    // suave web part handler
    // aggCoordinator is the actor responsible for creating unique aggregate actor 
    // toCmd is a function to transform the payLoad of the command contract into a command type 
    fun (ctx:HttpContext)->
        async {
            let id,version,cmd= getCmdFromJson ctx
            let currentBear : BearSession option = Some({ bearId = Guid.NewGuid(); socialId="fddfd"; username ="toto"})
            match currentBear with 
            | Some(bear) ->
                //we wait the for the per request actor to handle gracefully the computation
//                let! result = Async.AwaitTask <| tellActorAsync system aggCoordinator id version bear.bearId cmd
                // the result might be success or failure , we then transform it in a 
                // standardized uniform kind of message not relying on 
                // http code 
                let responsePayLoad = 
//                    result
//                    |> toMessage
                    { msg = "OK"  }
                    |> toJson
                return! Successful.ok responsePayLoad ctx
            | None  -> return! Suave.Http.RequestErrors.METHOD_NOT_ALLOWED "action not allowed" ctx
        } 


