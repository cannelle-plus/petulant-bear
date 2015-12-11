module PetulantBear.Rooms


open System
open System.Runtime.Serialization

open Suave // always open suave
open Suave.Http
open Suave.Http.Applicatives

open Suave.Http.Successful // for OK-result
open Suave.Web // for config


module Navigation =
    let detail = "/api/rooms/detail"
    let postMessage = "/api/rooms/postmessage"

module Contracts =

    open PetulantBear.Bears.Contracts


    [<DataContract>]
    type RoomFilter =
      { 
      [<field: DataMember(Name = "roomId")>]
      roomId : Guid;
      }

    [<DataContract>]
    type RoomMessageDetail =
      {
      [<field: DataMember(Name = "roomId")>]
      roomId : Guid;  
      [<field: DataMember(Name = "bear")>]
      bear : BearDetail;  
      [<field: DataMember(Name = "message")>]
      message : string;
      }

    [<DataContract>]
    type RoomDetail =
      { 
      [<field: DataMember(Name = "roomId")>]
      roomId : Guid;
      [<field: DataMember(Name = "name")>]
      name : string;
      [<field: DataMember(Name = "messages")>]
      messages : System.Collections.Generic.List<RoomMessageDetail>;
      }    

    [<DataContract>]
    type PostMessage = 
      { 
      [<field: DataMember(Name = "roomId")>]
      roomId : Guid;
      [<field: DataMember(Name = "message")>]
      message : string;
      }



type Commands = 
    | PostMessage of Contracts.PostMessage

let getRoomDetail getroomDB (filter: Contracts.RoomFilter) = getroomDB filter


let routes = []
let authRoutes getRoom save = 
    [
        POST >>= choose [ 
            path Navigation.detail >>=  mapJson (getRoomDetail getRoom );
            path Navigation.postMessage >>=  withBear >>= withCommand<Contracts.PostMessage> >>= processing  save PostMessage
        ]
    ]    
