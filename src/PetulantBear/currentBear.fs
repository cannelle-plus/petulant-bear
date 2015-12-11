module PetulantBear.CurrentBear


open System
open System.Runtime.Serialization

open Suave // always open suave
open Suave.Http
open Suave.Http.Applicatives

open Suave.Http.Successful // for OK-result
open Suave.Web // for config
open Suave.State.CookieStateStore
open Suave.Types
open Suave.Cookie


open Akka.Actor
open Akka.FSharp

module Navigation = 
    let detail = "/api/bear/detail"
    let changeUserName = "/api/bear/changeUserName"
    let changeAvatarId = "/api/bear/changeAvatarId"
    let changePassword = "/api/bear/changePassword"

module Contracts = 


    [<DataContract>]
    type ChangeUserName =
      { 
      [<field: DataMember(Name = "bearUsername")>]
      bearUsername: string;
      }
    
    [<DataContract>]
    type ChangeAvatarId =
      { 
      [<field: DataMember(Name = "bearAvatarId")>]
      bearAvatarId: int;
      }

    [<DataContract>]
    type ChangePassword =
      { 
      [<field: DataMember(Name = "bearPassword")>]
      bearPassword: string;
      }

type Commands = 
    | ChangeAvatarId of Contracts.ChangeAvatarId
    | ChangePassword of Contracts.ChangePassword
    | ChangeUserName of Contracts.ChangeUserName

            
let routes = []

let authRoutes  findBear (system:ActorSystem) saveEvents getGameList getGame  saveToDB   =
    [
        path Navigation.changeAvatarId >>=  withBear >>= withCommand<Contracts.ChangeAvatarId> >>= processing saveToDB ChangeAvatarId
        path Navigation.changeUserName >>= withBear >>= withCommand<Contracts.ChangeUserName> >>= processing saveToDB ChangeUserName
         >>= context (fun x ->
            let cmd = x.userState.["cmd"] :?> Command<Contracts.ChangeUserName> 
            match HttpContext.state x with
            | None ->  Http.RequestErrors.BAD_REQUEST "no store found"
            | Some store -> store.set userNameStore cmd.payLoad.bearUsername
        ) 
        path Navigation.changePassword >>= apply saveToDB ChangePassword
        //the following method might leak into the bear module with BearDetail... beware!!!
        GET >>= choose [ 
            path Navigation.detail >>= session ( fun s ->
                    match s with 
                    | NoSession -> Http.RequestErrors.BAD_REQUEST "no session found" 
                    | NewBear nb -> nb |> toJson |> Successful.ok  >>= Writers.setMimeType "application/json" 
                    | Bear b -> 
                        match findBear b.bearId with 
                        | Some(bear) -> bear |> toJson |> Successful.ok  >>= Writers.setMimeType "application/json" 
                        | None -> Http.ServerErrors.INTERNAL_ERROR "bear not known in db"
                )
        ]
    ]


