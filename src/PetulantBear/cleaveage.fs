
module PetulantBear.Cleaveage

open System
open System.Collections.Generic

open Suave.Http
open Suave.Http.Applicatives

open PetulantBear.Games.Contracts

open Akka.Actor
open Akka.FSharp

module Navigation =
    let propose = "/api/cleaveage/propose"
//    let vote = "/api/cleaveage/vote"
    let joinTeam = "/api/cleaveage/joinTeam"
    let leaveTeam = "/api/cleaveage/leaveTeam"
    let kickPlayerFromTeam = "/api/cleaveage/kickPlayerFromTeam"
    let switchPlayer = "/api/cleaveage/switchPlayer"
    let changeNameTeam = "/api/cleaveage/changeNameTeam"
    let list = "/api/cleaveage/list"
    let detail = "/api/cleaveage/detail"
    

module Contracts =
  

    type CleaveageFilter = 
      {
      gameId : Guid;
      }

    

    

    type ProposeCleaveage =
      {
      gameId : Guid;
      nameTeamA : string;
      nameTeamB : string;
      }

//    type UpVoteCleaveage = class end
//    type downVoteCleaveage = class end
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

    type CloseCleaveage() = class end
    type OpenCleaveage() = class end
      
    type CleaveagePlayer =
      {
        bearId : Guid;
        bearUsername : string;
        bearAvatarId : int;
      }

    type TeamDetail = 
      {
      teamId : Guid;
      name : string;
      players : System.Collections.Generic.List<CleaveagePlayer>
      }

    type CleaveageDetail =
      {
      cleaveageId : Guid;
      gameId : Guid;
      isOpenned: bool;
      teamA : TeamDetail;
      teamB : TeamDetail;
      version : int;
      }

    type CleaveageProposed =
      {
      gameId : Guid;
      teamAId : Guid;
      nameTeamA : string;
      teamBId : Guid;
      nameTeamB : string;
      }

    type CleaveageClosed() = class end
    type CleaveageOpenned() = class end

//    type CleaveageVoted = 
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
  | ProposeCleaveage of Contracts.ProposeCleaveage
  | ChangeNameTeam of Contracts.ChangeNameTeam
  | JoinTeam of Contracts.JoinTeam
  | LeaveTeam of Contracts.LeaveTeam
  | SwitchPlayer of Contracts.SwitchPlayer
  | KickPlayerFromTeam of Contracts.KickPlayerFromTeam
  | CloseCleaveage of Contracts.CloseCleaveage
  | OpenCleaveage of Contracts.OpenCleaveage
 

type Events =
  | CleaveageProposed of Contracts.CleaveageProposed
  | TeamJoined  of Contracts.TeamJoined
  | TeamLeaved  of Contracts.TeamLeaved
  | NameTeamChanged  of Contracts.NameTeamChanged
  | PlayerSwitched of Contracts.PlayerSwitched
  | PlayerKickedFromTeam of Contracts.PlayerKickedFromTeam
  | CleaveageClosed of Contracts.CleaveageClosed
  | CleaveageOpenned of Contracts.CleaveageOpenned


type State = {
    isOpenned : bool;
}
with static member Initial = { isOpenned = false}



module private Assert =
    let validActionCleaveage state = validator (fun s -> s.isOpenned  )  ["err:the cleaveage is closed"] state
    let validProposeCleaveage cmd state = validator (fun (cmd:Contracts.ProposeCleaveage) -> cmd.gameId <> Guid.Empty) ["err:the gameId is empty"] cmd

let exec state bear = function
    | ProposeCleaveage (cmd) -> Assert.validProposeCleaveage cmd state <?> [CleaveageProposed({ gameId= cmd.gameId; teamAId = Guid.NewGuid(); nameTeamA = cmd.nameTeamA; teamBId = Guid.NewGuid();nameTeamB = cmd.nameTeamB; })]
    | ChangeNameTeam(cmd) -> Assert.validActionCleaveage state <?> [NameTeamChanged({ teamId = cmd.teamId; nameTeam= cmd.nameTeam })]
    | JoinTeam (cmd) -> Assert.validActionCleaveage state <?> [TeamJoined({ teamId = cmd.teamId })]
    | LeaveTeam (cmd) -> Assert.validActionCleaveage  state <?> [TeamLeaved({ teamId = cmd.teamId })]
    | SwitchPlayer (cmd) -> Assert.validActionCleaveage state <?> [PlayerSwitched({ playerId = cmd.playerId;fromTeamId=cmd.fromTeamId; toTeamId= cmd.toTeamId })]
    | KickPlayerFromTeam (cmd) -> Assert.validActionCleaveage  state <?> [PlayerKickedFromTeam({ teamId = cmd.teamId; playerId = cmd.playerId })]
    | CloseCleaveage (cmd) -> Choice1Of2([CleaveageClosed(new Contracts.CleaveageClosed())])
    | OpenCleaveage (cmd) -> Choice1Of2([CleaveageOpenned(new Contracts.CleaveageOpenned())])


let applyEvts state = function
    | CleaveageProposed(evt) -> { state with isOpenned=true}
    | CleaveageClosed(evt) ->{ state with isOpenned=false}
    | CleaveageOpenned(evt) ->{ state with isOpenned=true}
    | _ -> state



//open the contracts for simple use in the definition of the routes
open Contracts

let routes = []


let authRoutes (system:ActorSystem) repo getGameCleaveageList   =

    let executeCmd map = 
        processingCommand repo "cleaveage" applyEvts State.Initial exec map
    [
        POST >>= choose [
            path Navigation.list  >>=   getInSession (fun bearId ->
                getGameCleaveageList bearId
                |> mapJson<CleaveageFilter,List<CleaveageDetail>>  );
            path Navigation.detail  >>=  getInSession (fun bearId ->
                getGameCleaveageList bearId
                |> mapJson<CleaveageFilter,List<CleaveageDetail>> );

            path Navigation.propose >>= withBear >>= withCommand<ProposeCleaveage>  >>= executeCmd ProposeCleaveage
            path Navigation.joinTeam >>=   withBear >>= withCommand<JoinTeam>  >>= executeCmd JoinTeam
            path Navigation.leaveTeam >>=  withBear >>= withCommand<LeaveTeam> >>= executeCmd  LeaveTeam
            path Navigation.kickPlayerFromTeam >>=  withBear >>= withCommand<KickPlayerFromTeam> >>= executeCmd  KickPlayerFromTeam
            path Navigation.switchPlayer >>=  withBear >>= withCommand<SwitchPlayer> >>= executeCmd  SwitchPlayer
            path Navigation.changeNameTeam >>=  withBear >>= withCommand<ChangeNameTeam> >>= executeCmd  ChangeNameTeam

        ]
    ]
    