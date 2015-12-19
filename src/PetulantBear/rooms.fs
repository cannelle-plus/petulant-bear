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
      [<field: DataMember(Name = "typeMessage")>]
      typeMessage : string;
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
      [<field: DataMember(Name = "version")>]
      version : Nullable<int>;
      }    

    [<DataContract>]
    type PostMessage = 
      { 
      [<field: DataMember(Name = "roomId")>]
      roomId : Guid;
      [<field: DataMember(Name = "message")>]
      message : string;
      }

    type MessagePosted =
      {
          [<field: DataMember(Name = "message")>]
          message : string;
      }


type Commands = 
    | PostMessage of Contracts.PostMessage

type Events =
    | MessagePosted of Contracts.MessagePosted

type State = {
    NbMessages : int
}
with static member Initial = { NbMessages = 0}

let exec state = function
    | PostMessage(cmd) -> Choice1Of2([MessagePosted({ message=cmd.message})])
    

let applyEvts state = function
    | MessagePosted(evt) -> state

let getRoomDetail getroomDB (filter: Contracts.RoomFilter) = getroomDB filter


let routes = []
let authRoutes repo getRoom save = 
    let executeCmd map = 
        processingCommand repo "room" applyEvts State.Initial exec map
    [
        POST >>= choose [ 
            path Navigation.detail >>=  mapJson (getRoomDetail getRoom );
            path Navigation.postMessage >>=  withBear >>= withCommand<Contracts.PostMessage>  >>= executeCmd  PostMessage  >>= processing save PostMessage
        ]
    ]    
