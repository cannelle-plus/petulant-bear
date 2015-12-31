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
open PetulantBear.SqliteBear2bearDB
open PetulantBear.Projections.Common



let name= "gameRoomProjection"

let resetProjection connection =
    
        let sql = "delete from RoomMessages where roomId in ( Select roomId from Rooms where  Rooms.version IS NOT NULL) ;delete from Rooms where Rooms.version IS NOT NULL;"
        use sqlCmd = new SQLiteCommand(sql, connection) 

        sqlCmd.ExecuteNonQuery() |> ignore
    

let writeMessage connection roomId version bearId message typeMessage=
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
    
    
  
//this should be a an event of the room, but for now it will do
let saveScheduledGame connection m (e:GameScheduled)=
    let sql = "Insert into Rooms (roomId,name)  VALUES (@id,'salle de ' + @name)"
    use sqlCmd = new SQLiteCommand(sql, connection) 

    let add (name:string, value: string) = 
        sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

    add("@id", m.aggregateId.ToString())
    add("@name", e.name)
    add("@version", m.version.ToString())
    

    sqlCmd.ExecuteNonQuery() |> ignore

    //this message add the versionning too, so if it was to be removed add it above
    let message = sprintf "Bienvenue dans la salle %s" e.name
    writeMessage connection m.aggregateId m.version m.bear.bearId message "welcome"

let saveJoined connection (m:Enveloppe)  =
    let message = sprintf "%s vient de joindre le match!" m.bear.username
    writeMessage connection m.aggregateId m.version m.bear.bearId message "joined"

let saveAbandonned connection (m:Enveloppe) =
    let message = sprintf "%s vient de quitter le match!" m.bear.username
    writeMessage connection m.aggregateId m.version m.bear.bearId message "abandonned"

let saveCancelled connection (m:Enveloppe) =
    let message = sprintf "Le match vient d'etre annulé!" 
    writeMessage connection m.aggregateId m.version m.bear.bearId message "cancelled"

let saveMessagePosted connection m e =
    writeMessage connection m.aggregateId m.version m.bear.bearId e.message "text"

let saveNameChanged connection (m:Enveloppe) (e:NameChanged) =
    let message = sprintf "Le match s'appele maintenant : %s!"  e.name
    writeMessage connection m.aggregateId m.version m.bear.bearId message "nameChanged"

let saveLocationChanged connection (m:Enveloppe) e =
    let message = sprintf "Changement de lieu! Le match se deroulera maintenant à maintenant à %s "  e.location
    writeMessage connection m.aggregateId m.version m.bear.bearId message "locationChanged"

let saveStartDateChanged connection (m:Enveloppe) e =
    let message = sprintf "Changement de date! Le match se deroulera maintenant le %s "  (e.startDate.ToString("yyyy-MM-ddTHH:mm:ssZ"))
    writeMessage connection m.aggregateId m.version m.bear.bearId message "locationChanged"

let saveMaxPlayerChanged connection (m:Enveloppe) e =
    let message = sprintf "Changement de max joueurs! Le match puet avoir maintenant jusqu'à %s "  (e.maxPlayers.ToString())
    writeMessage connection m.aggregateId m.version m.bear.bearId message "locationChanged"


let evtAppeared  connection escus (resolvedEvent:ResolvedEvent)= 
    withEvent connection name (fun m jsonEvent ->
        match  jsonEvent.case with
        | "Scheduled" -> 
            let e = deserializeEvt<GameScheduled> resolvedEvent
            logProjection name resolvedEvent  m e
            saveScheduledGame connection m e
        | "Abandonned" | "PlayerKicked" -> 
            let e = deserializeEvt<GameAbandonned> resolvedEvent
            logProjection name resolvedEvent  m e
            saveAbandonned connection m 
        | "Joined" -> 
            let e = deserializeEvt<GameJoined> resolvedEvent
            logProjection name resolvedEvent  m e
            saveJoined connection m
        | "Cancelled" -> 
            let e = deserializeEvt<GameCancelled> resolvedEvent
            logProjection name resolvedEvent  m e
            saveCancelled connection m
        | "MessagePosted" -> 
            let e = deserializeEvt<MessagePosted> resolvedEvent
            logProjection name resolvedEvent  m e
            saveMessagePosted connection m e
        | "NameChanged" ->
            let e = deserializeEvt<NameChanged> resolvedEvent
            logProjection name resolvedEvent  m e
            saveNameChanged connection m e
        | "LocationChanged" ->
            let e = deserializeEvt<LocationChanged> resolvedEvent
            logProjection name resolvedEvent  m e
            saveLocationChanged connection m e
        | "StartDateChanged" ->
            let e = deserializeEvt<StartDateChanged> resolvedEvent
            logProjection name resolvedEvent  m e
            saveStartDateChanged connection m e
        | "MaxPlayerChanged" ->
            let e = deserializeEvt<MaxPlayerChanged> resolvedEvent
            logProjection name resolvedEvent  m e
            saveMaxPlayerChanged connection m e
            
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



