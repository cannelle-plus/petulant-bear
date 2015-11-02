module PetulantBear.AfterGames

open Suave.Http
open Suave.Http.Applicatives

open Akka.Actor
open Akka.FSharp

module Navigation =
    let markBear = "/api/afterGames/markBear"
    let commentBear = "/api/afterGames/commentBear"


type Commands = 
    | FinishGame 
    | MarkBear  of int
    | CommentBear of string    

type Events =
    | GameFinished
    | BearMarked  of int
    | BearCommented of string


type State = {
    isPlayed : bool;
}
with static member Initial = { isPlayed = false;}

module private Assert =
    let validMarkBear (mark:int) state = validator (fun m -> state.isPlayed  ) [gamesText.locationUnknow] mark
    let validCommentBear (comment:string) state = validator (fun m -> state.isPlayed  ) [gamesText.locationUnknow] comment
        

let exec state = function
    | FinishGame -> Choice1Of2(GameFinished)
    | MarkBear(m) -> Assert.validMarkBear m state <?> BearMarked(m)
    | CommentBear(c) -> Assert.validCommentBear c state <?> BearCommented(c)

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
    