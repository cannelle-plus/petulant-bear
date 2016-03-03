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
    let postImage = "/api/rooms/postImage"
    let postVideo = "/api/rooms/postVideo"
    let postMusic = "/api/rooms/postMusic"
    

module Contracts =


    [<DataContract>]
    type BearDetail =
      { 
      [<field: DataMember(Name = "bearId")>]
      bearId : Guid;
      [<field: DataMember(Name = "bearUsername")>]
      bearUsername : string;
      [<field: DataMember(Name = "socialId")>]
      socialId : string;
      [<field: DataMember(Name = "bearAvatarId")>]
      bearAvatarId : int; 
      }


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

    [<DataContract>]
    type PostImage = 
      { 
      [<field: DataMember(Name = "roomId")>]
      roomId : Guid;
      [<field: DataMember(Name = "urlImage")>]
      urlImage : string;
      }

    [<DataContract>]
    type PostVideo = 
      { 
      [<field: DataMember(Name = "roomId")>]
      roomId : Guid;
      [<field: DataMember(Name = "urlVideo")>]
      urlVideo : string;
      }

    [<DataContract>]
    type PostMusic = 
      { 
      [<field: DataMember(Name = "roomId")>]
      roomId : Guid;
      [<field: DataMember(Name = "urlMusic")>]
      urlMusic : string;
      }
    type MessagePosted =
      {
          [<field: DataMember(Name = "message")>]
          message : string;
      }

    type ImagePosted =
      {
          [<field: DataMember(Name = "urlImage")>]
          urlImage : string;
      }

    type VideoPosted =
      {
          [<field: DataMember(Name = "urlVideo")>]
          urlVideo : string;
      }

    type MusicPosted =
      {
          [<field: DataMember(Name = "urlMusic")>]
          urlMusic : string;
      }

type Commands = 
    | PostMessage of Contracts.PostMessage
    | PostImage of Contracts.PostImage
    | PostVideo of Contracts.PostVideo
    | PostMusic of Contracts.PostMusic

type Events =
    | MessagePosted of Contracts.MessagePosted
    | ImagePosted of Contracts.ImagePosted
    | VideoPosted of Contracts.VideoPosted
    | MusicPosted of Contracts.MusicPosted

type State = {
    NbMessages : int
}
with static member Initial = { NbMessages = 0}

let exec state bear = function
    | PostMessage(cmd) -> Choice1Of2([MessagePosted({ message=cmd.message})])
    | PostImage(cmd) -> Choice1Of2([ImagePosted({ urlImage=cmd.urlImage})])
    | PostVideo(cmd) -> Choice1Of2([VideoPosted({ urlVideo=cmd.urlVideo})])
    | PostMusic(cmd) -> Choice1Of2([MusicPosted({ urlMusic=cmd.urlMusic})])
    

let applyEvts state = function
    | MessagePosted(evt) -> state
    | ImagePosted(evt) -> state
    | MusicPosted(evt) -> state
    | VideoPosted(evt) -> state

let getRoomDetail getroomDB (filter: Contracts.RoomFilter) = getroomDB filter


let routes = []
let authRoutes repo getRoom save = 
    let executeCmd map = 
        processingCommand repo "room" applyEvts State.Initial exec map
    [
        POST >>= choose [ 
            path Navigation.detail >>=  mapJson (getRoomDetail getRoom );
            path Navigation.postMessage >>=  withBear >>= withCommand<Contracts.PostMessage>  >>= executeCmd  PostMessage
            path Navigation.postImage >>=  withBear >>= withCommand<Contracts.PostImage>  >>= executeCmd  PostImage
            path Navigation.postVideo >>=  withBear >>= withCommand<Contracts.PostVideo>  >>= executeCmd  PostVideo
            path Navigation.postMusic >>=  withBear >>= withCommand<Contracts.PostMusic>  >>= executeCmd  PostMusic
        ]
    ]
