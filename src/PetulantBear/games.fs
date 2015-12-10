module PetulantBear.Games

open System
open System.Collections.Generic
open Newtonsoft.Json

open Suave // always open suave
open Suave.Http
open Suave.Http.Applicatives

open Suave.Http.Successful // for OK-result
open Suave.Web // for config
open Suave.Types

open System.Threading.Tasks

open Akka.Actor
open Akka.FSharp

module Navigation =
    let list = "/api/games/list"
    let detail = "/api/games/detail"
    let schedule = "/api/games/schedule"
    let join = "/api/games/join"
    let cancel = "/api/games/cancel"
    let abandon = "/api/games/abandon"
    let changeName = "/api/games/changeName"
    let changeStartDate = "/api/games/changeStartDate"
    let changeLocation = "/api/games/changeLocation"
    let kickPlayer = "/api/games/kickPlayer"
    let changeMaxPlayer = "/api/games/changeMaxPlayer"

module Contracts =
  

    type GamesFilter =
      {
      id : Guid;
      From : DateTime;
      To: DateTime;
      }


    type BearPlayer =
      {
      bearId : Guid;
      bearUsername : string;
      bearAvatarId : int;
      mark :int;
      comment : string
      isWaitingList: bool
      }


    type GameDetail =
      {
      id : Guid;
      name : string;
      ownerId : Guid;
      ownerUserName : string;
      startDate : DateTime;
      location : string;
      players : System.Collections.Generic.List<BearPlayer>;
      nbPlayers : int;
      maxPlayers : int;
      isJoinable : bool;
      isCancellable : bool;
      isAbandonnable : bool;
      isOwner: bool;
      }


    type Game =
      {
      id : Guid;
      name : string;
      ownerId : Guid;
      ownerUserName : string;
      startDate : DateTime;
      location : string;
      players : string;
      nbPlayers : int;
      maxPlayers : int;
      isJoinable : bool;
      isCancellable : bool;
      isAbandonnable : bool;
      isOwner : bool;
      }

    type ScheduleGame =
      {
      name : string;
      startDate : DateTime;
      location : string;
      players : string;
      nbPlayers : int;
      maxPlayers : int;
      }
    type ChangeName =
      {
      name : string;
      }

    type ChangeStartDate =
      {
      startDate : DateTime;
      }

    type ChangeLocation =
      {
      location : string;
      }

    type ChangeMaxPlayer =
      {
      maxPlayers : int;
      }

    type KickPlayer =
      {
      kickedBearId : Guid;
      }


    type GameScheduled =
      {
      name : string;
      startDate : DateTime;
      location : string;
      players : string;
      nbPlayers : int;
      maxPlayers : int;
      }
    type NameChanged =
      {
      name : string;
      }

    type StartDateChanged =
      {
      startDate : DateTime;
      }

    type LocationChanged =
      {
      location : string;
      }

    type PlayerKicked =
      {
      kickedBearId : Guid;
      }

    type MaxPlayerChanged =
      {
      maxPlayers : int;
      }

type Commands =
  | Schedule of Contracts.ScheduleGame
  | Abandon
  | Cancel
  | Join
  | ChangeName of Contracts.ChangeName
  | ChangeStartDate of Contracts.ChangeStartDate
  | ChangeLocation of Contracts.ChangeLocation
  | KickPlayer of Contracts.KickPlayer
  | ChangeMaxPlayer of Contracts.ChangeMaxPlayer



type Events =
  | Scheduled of Contracts.GameScheduled
  | Abandonned
  | Cancelled
  | Joined
  | NameChanged of Contracts.NameChanged
  | StartDateChanged of Contracts.StartDateChanged
  | LocationChanged of Contracts.LocationChanged
  | PlayerKicked of Contracts.PlayerKicked
  | MaxPlayerChanged of Contracts.MaxPlayerChanged


type State = {
    nbPlayer : int;
    maxPlayer : int;
    isScheduled : bool;
    isCancelled : bool;
}
with static member Initial = { nbPlayer = 0; maxPlayer=0; isScheduled = false; isCancelled= false}


module private Assert =

    let validScheduleGame (command:Contracts.ScheduleGame) state =
        validator (fun cmd -> true  ) [gamesText.locationUnknow] command
        <* validator (fun (cmd:Contracts.ScheduleGame) -> cmd.startDate> (DateTime.Now.AddHours(float 24.0))) [gamesText.gamesIn24Hours] command
        <* validator (fun g -> not g.isScheduled ) [gamesText.gameAlreadyScheduled] state
    let validJoinGame state = validator (fun g -> g.nbPlayer <10   ) ["err:the Nb max of player is 10"] state
    let validAbandonGame state = validator (fun g-> true) ["It is not allowed to withdraw from a game 48 hrs before the beginning"] state
    let validChangeStartDate (cmd:Contracts.ChangeStartDate) state = validator (fun g-> cmd.startDate<DateTime.Now) ["It is not rescheduled a game in the past"] state

let exec state = function
    | Schedule (cmd) -> Assert.validScheduleGame cmd state <?> Scheduled({ name = cmd.name; startDate = cmd.startDate ; location = cmd.location; players = cmd.players;  nbPlayers = cmd.nbPlayers; maxPlayers = cmd.maxPlayers; })
    | Abandon  -> Assert.validAbandonGame state <?> Abandonned
    | Cancel -> Assert.validAbandonGame state <?> Cancelled
    | Join -> Assert.validAbandonGame state <?> Joined
    | ChangeName(cmd) -> Choice1Of2(NameChanged({ name=cmd.name}))
    | ChangeStartDate(cmd) -> Assert.validChangeStartDate cmd state <?> StartDateChanged({ startDate= cmd.startDate})
    | ChangeLocation(cmd) -> Choice1Of2(LocationChanged({ location=cmd.location}))
    | KickPlayer(cmd) -> Choice1Of2(PlayerKicked({ kickedBearId=cmd.kickedBearId}))
    | ChangeMaxPlayer(cmd) -> Choice1Of2(MaxPlayerChanged({ maxPlayers=cmd.maxPlayers}))


let applyEvts state version = function
    | Scheduled(evt) -> version+1,{ state with isScheduled= true}
    | Abandonned -> version+1,{ state with nbPlayer= state.nbPlayer-1 }
    | Cancelled -> version+1,{ state with isCancelled = true }
    | Joined  -> version+1,{ state with nbPlayer= state.nbPlayer+1 }
    | PlayerKicked(evt) -> version+1,{ state with nbPlayer= state.nbPlayer-1 }
    | _ -> version,state


let routes = []


let gameActor id saveEvents  saveToDB=
    fun (mailbox:Actor<Guid*int*Guid*Commands>) ->
        let rec loop (currentVersion, state:State) = actor {
            let! (id,version,bearId,cmd) = mailbox.Receive()
            let evts = cmd |> exec state
            match evts with
            | Choice1Of2(evts) ->
                let newVersion,newState = evts |> applyEvts state currentVersion
                saveEvents "game" (id, currentVersion, evts)
                saveToDB (id,version,bearId) cmd |> ignore
                mailbox.Sender() <! ResultFound(Success(cmd))
                return! loop(newVersion,newState)
            | Choice2Of2(failure) ->
                return! loop(currentVersion,state)
        }
        loop(0,State.Initial)


//open the contracts for simple use in the definition of the routes
open Contracts

   

let authRoutes (system:ActorSystem) saveEvents getGameList getGame  saveToDB=

//    let handle =
//        let gameActorBuilder aggId =   gameActor aggId  saveEvents saveToDB
//
//        AggregateCoordinator.actor<Commands> gameActorBuilder
//        |> spawn system "gameActorCoordinator"
//        |> handleRequest<Commands> system

    [
        POST >>= choose [
            path Navigation.list  >>=   getInSession (fun bearId ->
                getGameList bearId
                |> mapJson<Contracts.GamesFilter,List<Contracts.Game>>  );
            path Navigation.detail  >>=  getInSession (fun bearId ->
                getGame bearId
                |> mapJson<Contracts.GamesFilter,Contracts.GameDetail> );

//            path Navigation.schedule >>=  handle  (getCmdFromContract<Contracts.ScheduleGame,Commands> Schedule)
//            path Navigation.abandon >>=  handle (getCmdFromRequest Abandon)
//            path Navigation.cancel >>=  handle (getCmdFromRequest Cancel)
//            path Navigation.commentBear >>=  handle (getCmdFromRequest CommentBear)
//            path Navigation.markBear >>=  handle (getCmdFromRequest MarkBear)

            path Navigation.schedule >>=  apply saveToDB Schedule
            path Navigation.join >>=  apply saveToDB (fun () -> Join)
            path Navigation.abandon >>=  apply saveToDB (fun () -> Abandon)
            path Navigation.cancel >>=  apply saveToDB (fun () -> Cancel)
            path Navigation.changeName >>=  apply saveToDB ChangeName
            path Navigation.changeStartDate >>=  apply saveToDB ChangeStartDate
            path Navigation.changeLocation >>=  apply saveToDB ChangeLocation
            path Navigation.kickPlayer >>=  apply saveToDB KickPlayer
            path Navigation.changeMaxPlayer >>=  apply saveToDB ChangeMaxPlayer
        ]
    ]
    