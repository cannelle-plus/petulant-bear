module AggregateCoordinator


open System
open System.Collections.Generic


open Akka.Actor
open Akka.FSharp

type State = {
    children : Dictionary<Guid,IActorRef>
}
with static member Initial = { children = new Dictionary<Guid,IActorRef>() }

let actor<'TAggCommand> (buildActor:Guid->Actor<Guid*int*Guid*'TAggCommand>->Cont<Guid*int*Guid*'TAggCommand,obj>)= 
    fun (mailbox:Actor<AggregateCoordinatorMsg>) ->
        let rec loop state = actor {
            let! message = mailbox.Receive()
            
            match message with
            | Hydrate(id,requestActor) ->
                let nameActor = sprintf "gameActor%s" (id.ToString())
                if not <| state.children.ContainsKey(id) then
                    let newActorRef = spawn mailbox.Context.System nameActor <| buildActor id
                    state.children.Add(id, newActorRef)
                requestActor <! (Hydrated(state.children.[id]):httpActorMsg<'TAggCommand>)
            | Dehydrate(id) ->
                state.children.[id].GracefulStop(TimeSpan(0,0,0,0,1)) |> ignore
                state.children.Remove(id)  |> ignore
            return! loop state
        }
        loop State.Initial