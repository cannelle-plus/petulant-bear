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
    let close = "/api/games/close"
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
      version : Nullable<int>;
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
      version : Nullable<int>
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

    type Join() = class end
    type Abandon() = class end
    type Cancel() = class end
    type Close() = class end

    type GameAbandonned() = class end
    type GameJoined() = class end
    type GameCancelled() = class end
    type GameClosed() = class end

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
  | Abandon of Contracts.Abandon
  | Cancel of Contracts.Cancel
  | Close of Contracts.Close
  | Join of Contracts.Join
  | ChangeName of Contracts.ChangeName
  | ChangeStartDate of Contracts.ChangeStartDate
  | ChangeLocation of Contracts.ChangeLocation
  | KickPlayer of Contracts.KickPlayer
  | ChangeMaxPlayer of Contracts.ChangeMaxPlayer



type Events =
  | Scheduled of Contracts.GameScheduled
  | Abandonned  of Contracts.GameAbandonned
  | Cancelled  of Contracts.GameCancelled
  | Closed  of Contracts.GameClosed
  | Joined  of Contracts.GameJoined
  | NameChanged of Contracts.NameChanged
  | StartDateChanged of Contracts.StartDateChanged
  | LocationChanged of Contracts.LocationChanged
  | PlayerKicked of Contracts.PlayerKicked
  | MaxPlayerChanged of Contracts.MaxPlayerChanged


type State = {
    nbPlayer : int;
    maxPlayer : int;
    startDate : DateTime;
    isOpenned : bool;
}
with static member Initial = { nbPlayer = 0; maxPlayer=0; startDate = DateTime.MaxValue; isOpenned= false}


module private Assert =

    let validAction = validator (fun s-> s.isOpenned ) [GamesText.gameClosedforModifications] 

    let validScheduleGame (command:Contracts.ScheduleGame) state =
        validator (fun (cmd:Contracts.ScheduleGame) -> cmd.location <> ""  ) [GamesText.locationUnknow] command
        <* validator (fun g -> not<| g.isOpenned ) [GamesText.gameAlreadyScheduled] state
    
    let validJoinGame state = 
        validAction state
        <* validator (fun g -> g.startDate>DateTime.Now    ) ["err:the game has already started"] state

    let validAbandonGame state = 
        validAction state
        <* validator (fun g->  g.startDate>DateTime.Now ) ["err:the game has already started"] state

    let validChangeStartDate (cmd:Contracts.ChangeStartDate) state = 
        validAction state
        <* validator (fun g-> cmd.startDate>DateTime.Now) ["It is not possible to reschedule a game in the past"] state

let exec state = function
    | Schedule (cmd) -> Assert.validScheduleGame cmd state <?> [Scheduled({ name = cmd.name; startDate = cmd.startDate ; location = cmd.location; players = cmd.players;  nbPlayers = cmd.nbPlayers; maxPlayers = cmd.maxPlayers; });Joined(Contracts.GameJoined())]
    | Abandon(cmd)  -> Assert.validAbandonGame state <?> [Abandonned(Contracts.GameAbandonned())]
    | Cancel(cmd) -> Assert.validAbandonGame state <?> [Cancelled(Contracts.GameCancelled())]
    | Close(cmd) -> Assert.validAction state <?> [Closed(Contracts.GameClosed())]
    | Join(cmd) -> Assert.validAbandonGame state <?> [Joined(Contracts.GameJoined())]
    | ChangeName(cmd) -> Assert.validAction state <?> [NameChanged({ name=cmd.name})]
    | ChangeStartDate(cmd) -> Assert.validChangeStartDate cmd state <?> [StartDateChanged({ startDate= cmd.startDate})]
    | ChangeLocation(cmd) -> Assert.validAction state <?> [LocationChanged({ location=cmd.location})]
    | KickPlayer(cmd) ->  Assert.validAction state <?> [PlayerKicked({ kickedBearId=cmd.kickedBearId})]
    | ChangeMaxPlayer(cmd) -> Assert.validAction state <?> [MaxPlayerChanged({ maxPlayers=cmd.maxPlayers})]


let applyEvts state = function
    | Scheduled(evt) -> { state with isOpenned= true; startDate= evt.startDate}
    | Abandonned(evt) -> { state with nbPlayer= state.nbPlayer-1 }
    | Cancelled(evt) -> { state with isOpenned = false }
    | Closed(evt) -> { state with isOpenned = false }
    | Joined(evt)  -> { state with nbPlayer= state.nbPlayer+1 }
    | PlayerKicked(evt) -> { state with nbPlayer= state.nbPlayer-1 }
    | _ -> state


let routes = []

//should I ever use the actors.. I wonder:: may be when the situations will become more complex
//
//let gameActor id saveEvents  saveToDB=
//    fun (mailbox:Actor<Guid*int*Guid*Commands>) ->
//        let rec loop (currentVersion, state:State) = actor {
//            let! (id,version,bearId,cmd) = mailbox.Receive()
//            let evts = cmd |> exec state
//            match evts with
//            | Choice1Of2(evts) ->
//                let newState = List.fold applyEvts state evts
//                saveEvents "game" (id, currentVersion, evts)
//                saveToDB (id,version,bearId) cmd |> ignore
//                mailbox.Sender() <! ResultFound(Success(cmd))
//                return! loop(currentVersion+ evts.Length,newState)
//            | Choice2Of2(failure) ->
//                return! loop(currentVersion,state)
//        }
//        loop(0,State.Initial)


//open the contracts for simple use in the definition of the routes
open Contracts

    

let authRoutes (system:ActorSystem) repo getGameList getGame  saveToDB=

    let executeCmd map = 
        processingCommand repo "game" applyEvts State.Initial exec map
    [
        POST >>= choose [
            path Navigation.list  >>=   getInSession (fun bearId ->
                getGameList bearId
                |> mapJson<Contracts.GamesFilter,List<Contracts.Game>>  );
            path Navigation.detail  >>=  getInSession (fun bearId ->
                getGame bearId
                |> mapJson<Contracts.GamesFilter,Contracts.GameDetail> );

            path Navigation.schedule 
                >>= withBear 
                >>= withCommand<Contracts.ScheduleGame>
                >>= executeCmd Schedule
                //>>= processing saveToDB Schedule

            path Navigation.join >>=   withBear >>= withCommand<Contracts.Join>  >>= executeCmd Commands.Join //>>= processing saveToDB Commands.Join
            path Navigation.abandon >>=  withBear >>= withCommand<Contracts.Abandon> >>= executeCmd  Commands.Abandon  //>>=  processing saveToDB Commands.Abandon
            path Navigation.cancel >>=  withBear >>= withCommand<Contracts.Cancel> >>= executeCmd  Commands.Cancel //>>=  processing saveToDB Commands.Cancel
            path Navigation.close >>=  withBear >>= withCommand<Contracts.Close> >>= executeCmd  Commands.Close 
            path Navigation.changeName >>=  withBear >>= withCommand<Contracts.ChangeName> >>= executeCmd  ChangeName //>>=  processing saveToDB ChangeName
            path Navigation.changeStartDate >>=  withBear >>= withCommand<Contracts.ChangeStartDate> >>= executeCmd  ChangeStartDate //>>=  processing saveToDB ChangeStartDate
            path Navigation.changeLocation >>=  withBear >>= withCommand<Contracts.ChangeLocation> >>= executeCmd  ChangeLocation //>>=  processing saveToDB ChangeLocation
            path Navigation.kickPlayer >>=  withBear >>= withCommand<Contracts.KickPlayer> >>= executeCmd  KickPlayer //>>=  processing saveToDB KickPlayer
            path Navigation.changeMaxPlayer >>=  withBear >>= withCommand<Contracts.ChangeMaxPlayer>  >>= executeCmd  ChangeMaxPlayer //>>=  processing saveToDB ChangeMaxPlayer

        ]
    ]
    