module PetulantBear.AfterGames

open System 

open Suave.Http
open Suave.Http.Applicatives

open Akka.Actor
open Akka.FSharp

module Navigation =
    let markBear = "/api/afterGames/markBear"
    let commentBear = "/api/afterGames/commentBear"

module Contracts =
    
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

open Contracts

type Commands = 
    | FinishGame 
    | MarkBear  of MarkBear
    | CommentBear of CommentBear    

type Events =
    | GameFinished
    | BearMarked  of BearMarked
    | BearCommented of BearCommented


type State = {
    isPlayed : bool;
}
with static member Initial = { isPlayed = false;}

module private Assert =
    let validMarkBear (mark:MarkBear) state = validator (fun m -> state.isPlayed  ) [gamesText.locationUnknow] mark
    let validCommentBear (comment:CommentBear) state = validator (fun m -> state.isPlayed  ) [gamesText.locationUnknow] comment
        

let exec state = function
    | FinishGame -> Choice1Of2(GameFinished)
    | MarkBear(m) -> Assert.validMarkBear m state <?> BearMarked({bearId= m.bearId; mark = m.mark })
    | CommentBear(c) -> Assert.validCommentBear c state <?> BearCommented({bearId= c.bearId; comment = c.comment })

let applyEvts state version = function
    | GameFinished ->  version+1,{state with isPlayed = true}
    | BearMarked (m) -> version+1,state
    | BearCommented(c) -> version+1,state


let routes = []  

let authRoutes (system:ActorSystem) saveEvents  saveToDB= 
    [
        POST >>= choose [
            path Navigation.commentBear >>=  apply saveToDB CommentBear
            path Navigation.markBear >>=  apply saveToDB MarkBear
        ] 
    ]
    