module PetulantBear.Games

open System
open System.Collections.Generic
open System.Linq
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

    type EmptyClass() =
        override x.Equals(yobj) =
            match yobj with
            | :? EmptyClass as y -> true
            | _ -> false
 
        override x.GetHashCode() = hash 7
        interface System.IComparable with
          member x.CompareTo yobj =
              match yobj with
              | :? EmptyClass as y -> 0
              | _ -> invalidArg "yobj" "cannot compare values of different types"

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

    type Join = EmptyClass
    type Abandon = EmptyClass
    type Cancel = EmptyClass
    type Close = EmptyClass

    type GameAbandonned = 
      {
      bearId : Guid;
      }
    type GameJoined = 
      {
      bearId : Guid;
      }
    type GameCancelled = EmptyClass
    type GameClosed = EmptyClass

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
    type PlayerAddedToTheLineUp =
      {
        bearId : Guid;
      }
    type PlayerRemovedFromTheLineUp =
      {
        bearId : Guid;
      }
    type PlayerAddedToTheBench =
      {
        bearId : Guid;
      }
    type PlayerRemovedFromTheBench =
      {
        bearId : Guid;
        isRemovedFromTheGame : bool;
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
  | PlayerAddedToTheLineUp of Contracts.PlayerAddedToTheLineUp
  | PlayerRemovedFromTheLineUp of Contracts.PlayerRemovedFromTheLineUp
  | PlayerAddedToTheBench of Contracts.PlayerAddedToTheBench
  | PlayerRemovedFromTheBench of Contracts.PlayerRemovedFromTheBench

type State = {
    nbPlayer : int;
    maxPlayer : int;
    startDate : DateTime;
    isOpenned : bool;
    lineUp : Guid list;
    bench : Guid list;
    }
with static member Initial = { nbPlayer = 0; maxPlayer=0; startDate = DateTime.MaxValue; isOpenned= false; lineUp= []; bench = []}

module private Assert =

    let validAction = validator (fun s-> s.isOpenned ) [GamesText.gameClosedforModifications] 

    let validScheduleGame (command:Contracts.ScheduleGame) state =
        validator (fun (cmd:Contracts.ScheduleGame) -> cmd.location <> ""  ) [GamesText.locationUnknow] command
        <* validator (fun g -> not<| g.isOpenned ) [GamesText.gameAlreadyScheduled] state
    
    let validJoinGame bear state = 
        validAction state
        <* validator (fun g -> g.startDate>DateTime.Now    ) ["err:the game has already started"] state
        <* validator (fun (b,g) -> not <| (g.lineUp |> Seq.append g.bench |> Seq.exists (fun x -> x=b.bearId))) ["err:this bear is already part of the game"] (bear,state)

    let validAbandonGame bear state = 
        validAction state
        <* validator (fun g->  g.startDate>DateTime.Now ) ["err:the game has already started"] state
        <* validator (fun (b,g) -> g.lineUp |> Seq.append g.bench |> Seq.exists (fun x -> x=b.bearId)) ["err:this bear is not part of the game"] (bear,state)

    let validChangeStartDate (cmd:Contracts.ChangeStartDate) state = 
        validAction state
        <* validator (fun g-> cmd.startDate>DateTime.Now) ["It is not possible to reschedule a game in the past"] state

let exec state bear = function
    | Schedule (cmd) -> 
        let evts = [
            Scheduled({ name = cmd.name; startDate = cmd.startDate ; location = cmd.location; players = cmd.players;  nbPlayers = cmd.nbPlayers; maxPlayers = cmd.maxPlayers; });
            Joined({bearId=bear.bearId})
            PlayerAddedToTheLineUp({ bearId=bear.bearId})
        ]
        Assert.validScheduleGame cmd state <?> evts
    | Abandon(cmd)  -> 
        let evts =
            if (state.lineUp.Contains(bear.bearId)) then  
                let playerOnTheBench = if (state.bench.Any())  then [PlayerAddedToTheLineUp({ bearId=state.bench.First()});PlayerRemovedFromTheBench({bearId= state.bench.First(); isRemovedFromTheGame=false })] else []
                 
                playerOnTheBench
                |> List.append [Abandonned({bearId=bear.bearId});PlayerRemovedFromTheLineUp({bearId= bear.bearId})] 
            else 
                [Abandonned({bearId=bear.bearId});PlayerRemovedFromTheBench({bearId= bear.bearId;isRemovedFromTheGame= true })]
            
        Assert.validAbandonGame bear state <?> evts
    | Cancel(cmd) -> Assert.validAction state <?> [Cancelled(Contracts.GameCancelled())]
    | Close(cmd) -> Assert.validAction state <?> [Closed(Contracts.GameClosed())]
    | Join(cmd) -> 
        if (state.lineUp.Length = state.maxPlayer ) then  Assert.validJoinGame bear state <?> [Joined({bearId=bear.bearId});PlayerAddedToTheBench({ bearId=bear.bearId})]
        else Assert.validJoinGame bear state <?> [Joined({bearId=bear.bearId});PlayerAddedToTheLineUp({ bearId=bear.bearId})] 
    | ChangeName(cmd) -> Assert.validAction state <?> [NameChanged({ name=cmd.name})]
    | ChangeStartDate(cmd) -> Assert.validChangeStartDate cmd state <?> [StartDateChanged({ startDate= cmd.startDate})]
    | ChangeLocation(cmd) -> Assert.validAction state <?> [LocationChanged({ location=cmd.location})]
    | KickPlayer(cmd) ->  
        let evts =
            if (state.lineUp.Contains(bear.bearId)) then  
                let playerOnTheBench = if (state.bench.Any())  then [PlayerAddedToTheLineUp({ bearId=state.bench.Last()});PlayerRemovedFromTheBench({bearId=state.bench.Last(); isRemovedFromTheGame=false })] else []

                playerOnTheBench
                |> List.append [PlayerKicked({ kickedBearId=cmd.kickedBearId});PlayerRemovedFromTheLineUp({bearId= bear.bearId})]
            else 
                [PlayerKicked({ kickedBearId=cmd.kickedBearId});PlayerRemovedFromTheBench({bearId= bear.bearId;isRemovedFromTheGame= true })]
        Assert.validAction state <?> evts
    | ChangeMaxPlayer(cmd) -> Assert.validAction state <?> [MaxPlayerChanged({ maxPlayers=cmd.maxPlayers})]

let applyEvts state = function
    | Scheduled(evt) -> { state with isOpenned= true; startDate= evt.startDate; maxPlayer= evt.maxPlayers}
    | Abandonned(evt) -> { state with nbPlayer= state.nbPlayer-1 }
    | Cancelled(evt) -> { state with isOpenned = false }
    | Closed(evt) -> { state with isOpenned = false }
    | Joined(evt)  ->  { state with nbPlayer= state.nbPlayer+1;}
    | PlayerKicked(evt) ->  { state with nbPlayer= state.nbPlayer-1 ; }
    | PlayerAddedToTheLineUp(evt) -> {state with lineUp= evt.bearId::state.lineUp}
    | PlayerRemovedFromTheLineUp(evt) -> 
        let newLineUp =
            state.lineUp
            |> Seq.filter (fun x-> x<>evt.bearId)
            |> Seq.toList
        {state with lineUp= newLineUp}
    | PlayerAddedToTheBench(evt) ->  {state with bench= evt.bearId::state.bench}
    | PlayerRemovedFromTheBench(evt) -> 
        let newBench =
            state.bench
            |> Seq.filter (fun x-> x<>evt.bearId)
            |> Seq.toList
        {state with bench= newBench}
    | _ -> state

let routes = []

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
    