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

open Suave // always open suave
open Suave.Http
open Suave.Http.Applicatives
open Suave.Http.Files
open Suave.Http.Writers
open Suave.Http.Successful // for OK-result
open Suave.Web // for config

open Akka.Actor

open Suave.Cookie
open PetulantBear.sqliteBear2bearDB
open PetulantBear


let addRoutes routes authRoutes existing =
    let (existingRoutes, existingAuthRoutes) = existing
    let newTuple = ( existingRoutes |> List.append routes, existingAuthRoutes |> List.append authRoutes)
    newTuple


     
let app root urlSite (system:ActorSystem) saveEvents auth = 
    //list the available routes for each module
    let (routes, authRoutes) = 
        (Users.routes urlSite getBearFromSocialId login,Users.authRoutes)
        |> addRoutes Games.routes (Games.authRoutes system saveEvents getGames getGame (saveToDB mapGameCmds))
        |> addRoutes CurrentBear.routes (CurrentBear.authRoutes getBear system saveEvents getGames getGame (saveToDB mapCurrentBearCmds))
        |> addRoutes AfterGames.routes (AfterGames.authRoutes system saveEvents (saveToDB mapAfterGamesCmds))
        |> addRoutes (Bears.routes signinToDB signinBearToDB) (Bears.authRoutes getBears getBear)
        |> addRoutes Rooms.routes (Rooms.authRoutes getRoomDetail (saveToDB mapRoomCmds))
        |> addRoutes Home.routes Home.authRoutes
        
    
    authRoutes 
    |> List.append [ path "/resetDB" >>=  PetulantBear.sqliteBear2bearDB.resetDB ]
    |> List.map (fun route -> auth(route))  
    |> List.append routes
    |> choose
    




    


    


    
    

    

    
