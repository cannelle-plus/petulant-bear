module PetulantBear.Projections.Room

open System
open System.Text
open System.Runtime.Serialization
open System.Data.SQLite
open System.Configuration   

open EventStore.ClientAPI  
open Newtonsoft.Json

open PetulantBear.Games
open PetulantBear.Games.Contracts
open PetulantBear.Rooms
open PetulantBear.Rooms.Contracts
open PetulantBear.sqliteBear2bearDB
open PetulantBear.Projections.Common



let name= "gameRoomProjection"

let resetProjection() =
    UseConnectionToDB (fun connection -> 
        let sql = "delete from RoomMessages where roomId in ( Select roomId from Rooms where  Rooms.version IS NOT NULL) ;delete from Rooms where Rooms.version IS NOT NULL;"
        use sqlCmd = new SQLiteCommand(sql, connection) 

        sqlCmd.ExecuteNonQuery() |> ignore
    )

let writeMessage roomId version bearId message typeMessage=
    UseConnectionToDB (fun connection -> 
        let sql  = "Insert into RoomMessages (roomId,bearId,message,typeMessage) VALUES (@roomId,@bearId, @message,@typeMessage);Update Rooms set version =@version where roomId=@roomId;"
        use sqlCmd = new SQLiteCommand(sql, connection) 

        let add (name:string, value: string) = 
            sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

        add("@roomId", roomId.ToString())
        add("@version", version.ToString())
        add("@bearId", bearId.ToString())
        add("@message", message)
        add("@typeMessage", typeMessage)

        sqlCmd.ExecuteNonQuery() |> ignore    
    )  
    
  
//this should be a an event of the room, but for now it will do
let saveScheduledGame m (e:GameScheduled)=
    UseConnectionToDB (fun connection -> 
        let sql = "Insert into Rooms (roomId,name)  VALUES (@id,'salle de ' + @name)"
        use sqlCmd = new SQLiteCommand(sql, connection) 

        let add (name:string, value: string) = 
            sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

        add("@id", m.aggregateId.ToString())
        add("@name", e.name)
        add("@version", m.version.ToString())
    

        sqlCmd.ExecuteNonQuery() |> ignore
    ) 
    //this message add the versionning too, so if it was to be removed add it above
    let message = sprintf "Bienvenue dans la salle %s" e.name
    writeMessage m.aggregateId m.version m.bear.bearId message "welcome"

let saveJoined (m:Enveloppe)  =
    let message = sprintf "%s vient de joindre le match!" m.bear.username
    writeMessage m.aggregateId m.version m.bear.bearId message "joined"

let saveAbandonned (m:Enveloppe) =
    let message = sprintf "%s vient de quitter le match!" m.bear.username
    writeMessage m.aggregateId m.version m.bear.bearId message "abandonned"

let saveCancelled (m:Enveloppe) =
    let message = sprintf "Le match vient d'etre annulé!" 
    writeMessage m.aggregateId m.version m.bear.bearId message "cancelled"

let saveMessagePosted m e =
    writeMessage m.aggregateId m.version m.bear.bearId e.message "text"

let saveNameChanged (m:Enveloppe) (e:NameChanged) =
    let message = sprintf "Le match s'appele maintenant : %s!"  e.name
    writeMessage m.aggregateId m.version m.bear.bearId message "nameChanged"

let saveLocationChanged (m:Enveloppe) e =
    let message = sprintf "Changement de lieu! Le match se deroulera maintenant à maintenant à %s "  e.location
    writeMessage m.aggregateId m.version m.bear.bearId message "locationChanged"

let saveStartDateChanged (m:Enveloppe) e =
    let message = sprintf "Changement de date! Le match se deroulera maintenant le %s "  (e.startDate.ToString("yyyy-MM-ddTHH:mm:ssZ"))
    writeMessage m.aggregateId m.version m.bear.bearId message "locationChanged"

let saveMaxPlayerChanged (m:Enveloppe) e =
    let message = sprintf "Changement de max joueurs! Le match puet avoir maintenant jusqu'à %s "  (e.maxPlayers.ToString())
    writeMessage m.aggregateId m.version m.bear.bearId message "locationChanged"


let evtAppeared  escus (resolvedEvent:ResolvedEvent)= 
    withEvent name (fun m jsonEvent ->
        match  jsonEvent.case with
        | "Scheduled" -> 
            let e = deserializeEvt<GameScheduled> resolvedEvent
            logProjection name resolvedEvent.OriginalEvent.EventStreamId resolvedEvent.Event.EventId  m e
            saveScheduledGame m e
        | "Abandonned" | "PlayerKicked" -> 
            let e = deserializeEvt<GameAbandonned> resolvedEvent
            logProjection name resolvedEvent.OriginalEvent.EventStreamId resolvedEvent.Event.EventId  m e
            saveAbandonned m 
        | "Joined" -> 
            let e = deserializeEvt<GameJoined> resolvedEvent
            logProjection name resolvedEvent.OriginalEvent.EventStreamId resolvedEvent.Event.EventId  m e
            saveJoined m
        | "Cancelled" -> 
            let e = deserializeEvt<GameCancelled> resolvedEvent
            logProjection name resolvedEvent.OriginalEvent.EventStreamId resolvedEvent.Event.EventId  m e
            saveCancelled m
        | "MessagePosted" -> 
            let e = deserializeEvt<MessagePosted> resolvedEvent
            logProjection name resolvedEvent.OriginalEvent.EventStreamId resolvedEvent.Event.EventId  m e
            saveMessagePosted m e
        | "NameChanged" ->
            let e = deserializeEvt<NameChanged> resolvedEvent
            logProjection name resolvedEvent.OriginalEvent.EventStreamId resolvedEvent.Event.EventId  m e
            saveNameChanged m e
        | "LocationChanged" ->
            let e = deserializeEvt<LocationChanged> resolvedEvent
            logProjection name resolvedEvent.OriginalEvent.EventStreamId resolvedEvent.Event.EventId  m e
            saveLocationChanged m e
        | "StartDateChanged" ->
            let e = deserializeEvt<StartDateChanged> resolvedEvent
            logProjection name resolvedEvent.OriginalEvent.EventStreamId resolvedEvent.Event.EventId  m e
            saveStartDateChanged m e
        | "MaxPlayerChanged" ->
            let e = deserializeEvt<MaxPlayerChanged> resolvedEvent
            logProjection name resolvedEvent.OriginalEvent.EventStreamId resolvedEvent.Event.EventId  m e
            saveMaxPlayerChanged m e
            
        | unknown -> 
            sprintf "unknown event %s" unknown
            |> Logary.LogLine.error  
            |> Logary.Logging.getCurrentLogger().Log
    )  escus (resolvedEvent:ResolvedEvent)
    
        
        
let catchup escus = 
    sprintf "catchup!"
    |> Logary.LogLine.error 
    |> Logary.Logging.getCurrentLogger().Log
let onError escus sdr e = 
    sprintf "error!"
    |> Logary.LogLine.error 
    |> Logary.Logging.getCurrentLogger().Log


let projection = {
    resetProjection = resetProjection
    eventAppeared = evtAppeared
    catchup = catchup
    onError = onError 
}



