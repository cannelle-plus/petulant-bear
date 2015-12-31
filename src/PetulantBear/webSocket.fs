module PetulantBear.WebSockets

open System

open Suave // always open suave
open Suave.Logging
open Suave.Http
open Suave.Http.Applicatives
open Suave.Http.Files
open Suave.Http.Writers
open Suave.Http.Successful // for OK-result
open Suave.Web // for config
open Suave.Http.RequestErrors
open Suave.Types
open Suave.WebSocket
open Suave.Sockets.Control



module Navigation =
    let games = "/api/websocket"
    

let echo  (subscribe,unsubscribe) (webSocket : WebSocket) ctx= 
    let webSocketId = Guid.NewGuid()
    let loop = ref true

    let closeWebSocket() = 
         loop := false
         unsubscribe(webSocketId)

    //subscribe to the events
    subscribe webSocketId (fun (msg :string)-> 
         let data = System.Text.UTF8Encoding.UTF8.GetBytes msg
         let result = webSocket.send Text data true |> Async.RunSynchronously
         match  result with
         | Choice2Of2(error) ->
            //log error ?
            closeWebSocket()
            false
         | _ -> true

    )
    socket {
        while !loop do
            let! message = webSocket.read() 
            match message with
            | Text, data, true->
                let str = System.Text.Encoding.UTF8.GetString data
                let responseData = System.Text.UTF8Encoding.UTF8.GetBytes str
                do! webSocket.send Text responseData true
            | Ping, _, _ ->
                do! webSocket.send Pong [||] true
            | Close, _, _ ->
                do! webSocket.send Close [||] true
                closeWebSocket()
            |  _ -> ()
    }

let authRoutes =  []
let routes liveSubscribe= 
    [
        GET >>= choose
            [ 
                path Navigation.games >>=  handShake (echo liveSubscribe)
            ]
    ]