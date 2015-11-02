module PetulantBear.Bears


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

module Navigation = 
    let list = "/api/bears/list"
    let detail = "/api/bears/detail"
    let signin = "/api/bears/signin"
    

module Contracts = 

    [<DataContract>]
    type BearsFilter =
      { 
      [<field: DataMember(Name = "bearId")>]
      bearId : Guid;
      [<field: DataMember(Name = "fromBearId")>]
      fromBearId : Guid;
      }


    [<DataContract>]
    type SignIn =
      { 
      [<field: DataMember(Name = "bearAvatarId")>]
      bearAvatarId: int;
      [<field: DataMember(Name = "bearUsername")>]
      bearUsername: string;
      }
  
    [<DataContract>]
    type Bear =
      { 
      [<field: DataMember(Name = "bearId")>]
      bearId : Guid;
      [<field: DataMember(Name = "bearUsername")>]
      bearUsername : string;
      [<field: DataMember(Name = "bearAvatarId")>]
      bearAvatarId : int; 
      }


type Commands = 
    | SignIn of Contracts.SignIn

let getBears getBearList (filter:Contracts.BearsFilter) = getBearList filter
let getBear findBear (filter:Contracts.BearsFilter) = findBear filter.bearId

let signIn (store:StateStore) saveSignin bearId socialId  = 
    Types.request(fun r ->
        try
            let cmd:Command<Contracts.SignIn> = fromJson r.rawForm
            let id,version = (cmd.id,cmd.version)

            saveSignin (id,version,bearId) (socialId,cmd.payLoad)  |> ignore
            
            { msg = PetulantBear.Home.Navigation.root  }     
            |> toJson
            |> Successful.ok 
            >>= Writers.setMimeType "application/json"
            >>= store.set bearStore bearId
            >>= store.set userNameStore cmd.payLoad.bearUsername
            >>= Auth.authenticated Session false

        with | ex -> 
            Failure(ex.Message) 
            |> toMessage
            |> toJson
            |> Http.RequestErrors.bad_request 
            >>= Writers.setMimeType "application/json"


    )

let signin save =   
    statefulForSession  
    >>= context (fun x -> 
            match HttpContext.state x with
            | None ->
                // restarted server without keeping the key; set key manually?
                let msg = "Server Key, Cookie Serialiser reset, or Cookie Data Corrupt, "
                            + "if you refresh the browser page, you'll have gotten a new cookie."
                OK msg
            | Some store ->
                match (store.get socialIdStore) with
                | Some(socialId) ->
                    let bearId =Guid.NewGuid()
                    signIn store save bearId socialId 
                     
                    
                | None -> Http.RequestErrors.BAD_REQUEST "no socialId found"
                
                
    ) 


let routes save = 
    [
        path Navigation.signin >>= signin save

    ]

let authRoutes  findBears findBear   =
    [
        POST >>= choose [ 
            path Navigation.list >>=  mapJson (getBears findBears)
            path Navigation.detail >>=  mapJson (getBear  findBear)
        ]
    ]


