module PetulantBear.FinishedGames

open System 

open Suave.Http
open Suave.Http.Applicatives

open Akka.Actor
open Akka.FSharp

module Navigation =
    let giveScore = "/api/finishedGames/giveScore"
    let markBear = "/api/finishedGames/markBear"
    let commentBear = "/api/finishedGames/commentBear"

module Contracts =

    type GiveScore =
      { 
      teamAId : Guid;
      teamAScore : int;
      teamBId : Guid;
      teamBScore : int;
      }
    
    type MarkBear =
      { 
      bearId : Guid;
      mark : int;
      }

    type CommentBear =
      { 
      bearId : Guid;
      comment : string;
      } 

    type BearMarked =
      { 
      bearId : Guid;
      mark : int;
      }

    type BearCommented =
      { 
      bearId : Guid;
      comment : string;
      }

    type ScoreGiven =
      { 
      teamAId : Guid;
      teamAScore : int;
      teamBId : Guid;
      teamBScore : int;
      }

open Contracts

type Commands = 
    | GiveScore  of GiveScore
    | MarkBear  of MarkBear
    | CommentBear of CommentBear

type Events =
    | ScoreGiven of ScoreGiven
    | BearMarked  of BearMarked
    | BearCommented of BearCommented


type State = {
    isScoreGiven : bool;
}
with static member Initial = { isScoreGiven = false;}

module private Assert =
    let validMarkBear (mark:MarkBear) state = validator (fun m -> state.isScoreGiven  ) [GamesText.locationUnknow] mark
    let validCommentBear (comment:CommentBear) state = validator (fun m -> state.isScoreGiven  ) [GamesText.locationUnknow] comment
        

let exec state bear = function
    | GiveScore(s)-> Choice1Of2([ScoreGiven({teamAId = s.teamAId; teamAScore = s.teamAScore; teamBId = s.teamBId; teamBScore = s.teamBScore;})])
    | MarkBear(m) -> Assert.validMarkBear m state <?> [BearMarked({bearId= m.bearId; mark = m.mark })]
    | CommentBear(c) -> Assert.validCommentBear c state <?> [BearCommented({bearId= c.bearId; comment = c.comment })]

let applyEvts state  = function
    | ScoreGiven(s) ->  {state with isScoreGiven = true}
    | BearMarked (m) -> state
    | BearCommented(c) ->state


let routes = []

let authRoutes (system:ActorSystem) repo = 
    let executeCmd map = 
        processingCommand repo "finishedGame" applyEvts State.Initial exec map

    [
        POST >>= choose [
            path Navigation.giveScore >>=  withBear >>= withCommand<Contracts.GiveScore> >>= executeCmd GiveScore
            path Navigation.commentBear >>=  withBear >>= withCommand<Contracts.CommentBear> >>= executeCmd CommentBear
            path Navigation.markBear >>=  withBear >>= withCommand<Contracts.MarkBear> >>= executeCmd MarkBear
        ] 
    ]
    