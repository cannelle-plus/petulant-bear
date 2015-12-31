/// Documentation for petulant Bear
///
/// ## Example
///
///     let h = Library.hello 1
///     printfn "%d" h
///
module PetulantBear.Application

open System
open System.IO
open System.Net.Http
open System.Net.Http.Headers
open System.Text
open System.Runtime.Serialization
open System.Data.SQLite

open Suave // always open suave
open Suave.Http
open Suave.Http.Applicatives
open Suave.Http.Files
open Suave.Http.Writers
open Suave.Http.Successful // for OK-result
open Suave.Web // for config

open Akka.Actor

open Suave.Cookie
open PetulantBear.SqliteBear2bearDB
open PetulantBear


let addRoutes routes authRoutes existing =
    let (existingRoutes, existingAuthRoutes) = existing
    let newTuple = ( existingRoutes |> List.append routes, existingAuthRoutes |> List.append authRoutes)
    newTuple


     
let app root urlSite (connection:SQLiteConnection) (system:ActorSystem) repo auth (liveSubscribe,liveUnsubscribe) = 
    //list the available routes for each module
    let (routes, authRoutes) = 
        (Users.routes urlSite (getBearFromSocialId connection) (login connection),Users.authRoutes)
        |> addRoutes (WebSockets.routes (liveSubscribe,liveUnsubscribe)) WebSockets.authRoutes
        |> addRoutes Games.routes (Games.authRoutes system repo (getGames connection) (getGame connection) (saveToDB connection mapGameCmds))
        |> addRoutes Cleaveage.routes (Cleaveage.authRoutes system repo (getCleaveages connection) )
        |> addRoutes CurrentBear.routes (CurrentBear.authRoutes (getBear connection) system repo (getGames connection) (getGame connection) (saveToDB connection mapCurrentBearCmds))
        |> addRoutes FinishedGames.routes (FinishedGames.authRoutes system repo)
        |> addRoutes (Bears.routes (signinToDB connection) (signinBearToDB connection)) (Bears.authRoutes (getBears connection) (getBear connection))
        |> addRoutes Rooms.routes (Rooms.authRoutes repo (getRoomDetail connection) (saveToDB connection mapRoomCmds))
        |> addRoutes Home.routes Home.authRoutes
        
    
    authRoutes 
    |> List.append [ path "/resetDB" >>=  (PetulantBear.SqliteBear2bearDB.resetDB connection) ]
    |> List.map (fun route -> auth(route))
    |> List.append routes
    |> choose
    




    


    


    
    

    

    
