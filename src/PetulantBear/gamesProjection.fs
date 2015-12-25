module PetulantBear.Projections.Games

open System
open System.Text
open System.Runtime.Serialization
open System.Data.SQLite
open System.Configuration   

open EventStore.ClientAPI  
open Newtonsoft.Json

open PetulantBear.Games
open PetulantBear.Games.Contracts
open PetulantBear.sqliteBear2bearDB
open PetulantBear.Projections.Common


let name= "gameListProjection"

let resetProjection() =
    UseConnectionToDB (fun connection -> 
        let sql = "delete from GamesBears where GameId in ( Select id from GamesList where  GamesList.version IS NOT NULL) ;delete from GamesList where GamesList.version IS NOT NULL;"
        use sqlCmd = new SQLiteCommand(sql, connection) 

        sqlCmd.ExecuteNonQuery() |> ignore
    )


let saveScheduledGame m (e:GameScheduled)=
    UseConnectionToDB (fun connection -> 
        //Insert into GamesBears (gameId,bearId)  VALUES (@id,@ownerId); Insert into Rooms (roomId,name)  VALUES (@id,'salle de ' + @name);    
        let sql = "Insert into GamesList (id,name,ownerId,ownerBearName,startDate,location,maxPlayers,version) VALUES (@id, @name,@ownerId,@ownerUserName, @begins, @location, @maxPlayers,@version); "
        use sqlCmd = new SQLiteCommand(sql, connection) 

        let add (name:string, value: string) = 
            sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

        add("@id", m.aggregateId.ToString())
        add("@name", e.name)
        add("@ownerId",  m.bear.bearId.ToString())
        add("@ownerUserName",  m.bear.username)
        add("@begins", e.startDate.ToString("yyyy-MM-ddTHH:mm:ssZ"))
        add("@location", e.location.ToString())
        add("@maxPlayers", e.maxPlayers.ToString())
        add("@version", m.version.ToString())
    

        sqlCmd.ExecuteNonQuery() |> ignore
    ) |> ignore

let saveJoined m  =
    UseConnectionToDB (fun connection -> 
        let sql = "delete from GamesBears  where gameId=@id and bearId=@bearId; Insert into GamesBears (gameId,bearId)  VALUES (@id,@bearId); Update GamesList set version =@version where id=@id;"
        use sqlCmd = new SQLiteCommand(sql, connection) 

        let add (name:string, value: string) = 
            sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

        add("@id", m.aggregateId.ToString())
        add("@bearId",  m.bear.bearId.ToString())
        add("@version", m.version.ToString())

        sqlCmd.ExecuteNonQuery() |> ignore
    ) |> ignore

let saveAbandonned m =
    UseConnectionToDB (fun connection -> 
        let sql = "delete from GamesBears  where gameId=@id and bearId=@bearId; Update GamesList set version =@version where id=@id;"
        use sqlCmd = new SQLiteCommand(sql, connection) 

        let add (name:string, value: string) = 
            sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

        add("@id", m.aggregateId.ToString())
        add("@bearId",  m.bear.bearId.ToString())
        add("@version", m.version.ToString())

        sqlCmd.ExecuteNonQuery() |> ignore
    ) |> ignore

let saveCancelled m =
    UseConnectionToDB (fun connection -> 
        let sql = "update GamesList set currentState=0,version =@version where id=@id;"
        use sqlCmd = new SQLiteCommand(sql, connection) 

        let add (name:string, value: string) = 
            sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

        add("@id", m.aggregateId.ToString())
        add("@version", m.version.ToString())
        
        sqlCmd.ExecuteNonQuery() |> ignore    
    ) |> ignore

let savePlayerKicked m e =
    UseConnectionToDB (fun connection -> 
        let sql = "delete from GamesBears  where gameId=@id and bearId=@bearId;Update GamesList set version =@version where id=@id;"
        use sqlCmd = new SQLiteCommand(sql, connection) 

        let add (name:string, value: string) = 
            sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

        add("@id", m.aggregateId.ToString())
        add("@version", m.version.ToString())
        add("@bearId", e.kickedBearId.ToString())   

        sqlCmd.ExecuteNonQuery() |> ignore    
    ) 

let saveNameChanged m e =
    UseConnectionToDB (fun connection -> 
        let sql = "Update  GamesList set name= @name where id=@id;Update GamesList set version =@version where id=@id;"
        use sqlCmd = new SQLiteCommand(sql, connection) 

        let add (name:string, value: string) = 
            sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

        add("@id", m.aggregateId.ToString())
        add("@version", m.version.ToString())
        add("@name", e.name.ToString())   

        sqlCmd.ExecuteNonQuery() |> ignore    
    ) 

let saveStartDateChanged m e =
    UseConnectionToDB (fun connection -> 
        let sql = "Update  GamesList set startDate= @startDate where id=@id;Update GamesList set version =@version where id=@id;"
        use sqlCmd = new SQLiteCommand(sql, connection) 

        let add (name:string, value: string) = 
            sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

        add("@id", m.aggregateId.ToString())
        add("@version", m.version.ToString())
        add("@startDate", e.startDate.ToString("yyyy-MM-ddTHH:mm:ssZ"))   

        sqlCmd.ExecuteNonQuery() |> ignore    
    ) 

let saveLocationChanged m e =
    UseConnectionToDB (fun connection -> 
        let sql = "Update  GamesList set location= @location where id=@id;Update GamesList set version =@version where id=@id;"
        use sqlCmd = new SQLiteCommand(sql, connection) 

        let add (name:string, value: string) = 
            sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

        add("@id", m.aggregateId.ToString())
        add("@version", m.version.ToString())
        add("@location", e.location)  
        
        sqlCmd.ExecuteNonQuery() |> ignore     
    ) 

let saveMaxPlayerChanged m e =
    UseConnectionToDB (fun connection -> 
        let sql = "Update  GamesList set maxPlayers= @maxPlayers where id=@id;Update GamesList set version =@version where id=@id;"
        use sqlCmd = new SQLiteCommand(sql, connection) 

        let add (name:string, value: string) = 
            sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

        add("@id", m.aggregateId.ToString())
        add("@version", m.version.ToString())
        add("@maxPlayers", e.maxPlayers.ToString())   

        sqlCmd.ExecuteNonQuery() |> ignore    
    ) 
    

let evtAppeared  escus (resolvedEvent:ResolvedEvent)= 
    withEvent name (fun m jsonEvent ->
        match  jsonEvent.case with
        | "Scheduled" -> 
            let e = deserializeEvt<GameScheduled> resolvedEvent
            logProjection name resolvedEvent m e
            saveScheduledGame m e
        | "Abandonned" -> 
            let e = deserializeEvt<GameAbandonned> resolvedEvent
            logProjection name  resolvedEvent m e
            saveAbandonned m 
        | "Joined" -> 
            let e = deserializeEvt<GameJoined> resolvedEvent
            logProjection name resolvedEvent m e
            saveJoined m
        | "Cancelled" -> 
            let e = deserializeEvt<GameCancelled> resolvedEvent
            logProjection name resolvedEvent m e
            saveCancelled m
        | "PlayerKicked" -> 
            let e = deserializeEvt<PlayerKicked> resolvedEvent
            logProjection name resolvedEvent m e
            savePlayerKicked m e
        | "NameChanged" -> 
            let e = deserializeEvt<NameChanged> resolvedEvent
            logProjection name resolvedEvent m e
            saveNameChanged m e
         | "StartDateChanged" -> 
            let e = deserializeEvt<StartDateChanged> resolvedEvent
            logProjection name resolvedEvent m e
            saveStartDateChanged m e
         | "LocationChanged" -> 
            let e = deserializeEvt<LocationChanged> resolvedEvent
            logProjection name resolvedEvent m e
            saveLocationChanged m e
         | "MaxPlayerChanged" -> 
            let e = deserializeEvt<MaxPlayerChanged> resolvedEvent
            logProjection name resolvedEvent m e
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



