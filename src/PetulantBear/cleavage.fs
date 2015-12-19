module PetulantBear.Cleavage 

open System

open Suave.Http
open Suave.Http.Applicatives

open PetulantBear.Games.Contracts

open Akka.Actor
open Akka.FSharp

module Navigation =
    let propose = "/api/cleavage/propose"
//    let vote = "/api/cleavage/vote"
    let joinTeam = "/api/cleavage/joinTeam"
    let leaveTeam = "/api/cleavage/leaveTeam"
    let kickPlayerFromTeam = "/api/cleavage/kickPlayerFromTeam"
    let switchPlayer = "/api/cleavage/switchPlayer"
    let changeNameTeam = "/api/cleavage/changeNameTeam"
    let list = "/api/cleavage/list"
    let detail = "/api/cleavage/detail"
    

module Contracts =
  

    type CleaveageFilter = class end

    type ProposeCleavage =
      {
      gameId : Guid;
      nameTeamA : string;
      nameTeamB : string;
      }

//    type UpVoteCleavage = class end
//    type downVoteCleavage = class end
    type JoinTeam = 
      {
      teamId : Guid;
      }
    type LeaveTeam = 
      {
      teamId : Guid;
      }
    
    type ChangeNameTeam =
      {
      teamId : Guid;
      nameTeam : string;
      }
    
    type SwitchPlayer =
      {
      playerId : Guid;
      fromTeamId : Guid;
      toTeamId: Guid;
      }
    
    type KickPlayerFromTeam =
      {
      teamId : Guid;
      playerId : Guid;
      }

    type CloseCleavage() = class end
    type OpenCleavage() = class end
      

    type Team = {
        TeamId : Guid;
        name: string;
        players : System.Collections.Generic.List<BearPlayer>;
        canJoin : bool;
        CanLeave : bool;
    }

    type CleavageDetail =
      {
      id : Guid;
      name : string;
      TeamA : Team;
      TeamB : Team;
      version : Nullable<int>;
      }

    type CleavageProposed =
      {
      gameId : Guid;
      teamAId : Guid;
      nameTeamA : string;
      teamBId : Guid;
      nameTeamB : string;
      }

    type CleavageClosed() = class end
    type CleavageOpenned() = class end

//    type CleavageVoted = 
//      {
//      vote : int;
//      }

    type TeamJoined = 
      {
        teamId : Guid;
      }
    type TeamLeaved = 
      {
        teamId : Guid;
      }
      
    type NameTeamChanged =
      {
        teamId : Guid;
        nameTeam : string;
      }
    
    type PlayerSwitched =
      {
      fromTeamId : Guid;
      toTeamId: Guid;
      playerId : Guid;
      }
    
    type PlayerKickedFromTeam =
      {
      teamId : Guid;
      playerId : Guid;
      }

type Commands =
  | ProposeCleavage of Contracts.ProposeCleavage
  | ChangeNameTeam of Contracts.ChangeNameTeam
  | JoinTeam of Contracts.JoinTeam
  | LeaveTeam of Contracts.LeaveTeam
  | SwitchPlayer of Contracts.SwitchPlayer
  | KickPlayerFromTeam of Contracts.KickPlayerFromTeam
  | CloseCleavage of Contracts.CloseCleavage
  | OpenCleavage of Contracts.OpenCleavage
 

type Events =
  | CleavageProposed of Contracts.CleavageProposed
  | TeamJoined  of Contracts.TeamJoined
  | TeamLeaved  of Contracts.TeamLeaved
  | NameTeamChanged  of Contracts.NameTeamChanged
  | PlayerSwitched of Contracts.PlayerSwitched
  | PlayerKickedFromTeam of Contracts.PlayerKickedFromTeam
  | CleavageClosed of Contracts.CleavageClosed
  | CleavageOpenned of Contracts.CleavageOpenned


type State = {
    isOpenned : bool;
}
with static member Initial = { isOpenned = false}



module private Assert =
    let validActionCleavage state = validator (fun s -> s.isOpenned  )  ["err:the cleavage is closed"] state

let exec state = function
    | ProposeCleavage (cmd) -> Choice1Of2([CleavageProposed({ gameId= cmd.gameId; teamAId = Guid.NewGuid(); nameTeamA = cmd.nameTeamA; teamBId = Guid.NewGuid();nameTeamB = cmd.nameTeamB; })])
    | ChangeNameTeam(cmd) -> Assert.validActionCleavage state <?> [NameTeamChanged({ teamId = cmd.teamId; nameTeam= cmd.nameTeam })]
    | JoinTeam (cmd) -> Assert.validActionCleavage state <?> [TeamJoined({ teamId = cmd.teamId })]
    | LeaveTeam (cmd) -> Assert.validActionCleavage  state <?> [TeamLeaved({ teamId = cmd.teamId })]
    | SwitchPlayer (cmd) -> Assert.validActionCleavage state <?> [PlayerSwitched({ playerId = cmd.playerId;fromTeamId=cmd.fromTeamId; toTeamId= cmd.toTeamId })]
    | KickPlayerFromTeam (cmd) -> Assert.validActionCleavage  state <?> [PlayerKickedFromTeam({ teamId = cmd.teamId; playerId = cmd.playerId })]
    | CloseCleavage (cmd) -> Choice1Of2([CleavageClosed(new Contracts.CleavageClosed())])
    | OpenCleavage (cmd) -> Choice1Of2([CleavageOpenned(new Contracts.CleavageOpenned())])


let applyEvts state = function
    | CleavageProposed(evt) -> { state with isOpenned=true}
    | CleavageClosed(evt) ->{ state with isOpenned=false}
    | CleavageOpenned(evt) ->{ state with isOpenned=true}
    | _ -> state



//open the contracts for simple use in the definition of the routes
open Contracts

let routes = []


let authRoutes (system:ActorSystem) repo getGameCleavageList getCleavage  saveToDB=

    let executeCmd map = 
        processingCommand repo "cleavage" applyEvts State.Initial exec map
    [
        POST >>= choose [
            path Navigation.list  >>=   getInSession (fun gameId ->
                getGameCleavageList gameId
                |> mapJson<CleaveageFilter,List<CleavageDetail>>  );
            path Navigation.detail  >>=  getInSession (fun cleavageId ->
                getCleavage cleavageId
                |> mapJson<CleaveageFilter,CleavageDetail> );

            path Navigation.propose >>= withBear >>= withCommand<ProposeCleavage>  >>= executeCmd ProposeCleavage
            path Navigation.joinTeam >>=   withBear >>= withCommand<JoinTeam>  >>= executeCmd JoinTeam
            path Navigation.leaveTeam >>=  withBear >>= withCommand<LeaveTeam> >>= executeCmd  LeaveTeam
            path Navigation.kickPlayerFromTeam >>=  withBear >>= withCommand<KickPlayerFromTeam> >>= executeCmd  KickPlayerFromTeam
            path Navigation.switchPlayer >>=  withBear >>= withCommand<SwitchPlayer> >>= executeCmd  SwitchPlayer
            path Navigation.changeNameTeam >>=  withBear >>= withCommand<ChangeNameTeam> >>= executeCmd  ChangeNameTeam

        ]
    ]
    