module PetulantBear.Projections.Cleavage

open System
open System.Text
open System.Runtime.Serialization
open System.Data.SQLite
open System.Configuration   

open EventStore.ClientAPI  
open Newtonsoft.Json

open PetulantBear.Cleavage
open PetulantBear.Cleavage.Contracts
open PetulantBear.sqliteBear2bearDB
open PetulantBear.Projections.Common


let name= "cleavageProjection"

let resetProjection() =
    UseConnectionToDB (fun connection -> 
        let sql = "delete from teamPlayers where teamId in ( Select teamId from team where cleavageId in( select cleavageId from cleavage where  cleavage.version IS NOT NULL)) ; delete from team where cleavageId in( select cleavageId from cleavage where  cleavage.version IS NOT NULL);delete from cleavage where  cleavage.version IS NOT NULL"
        use sqlCmd = new SQLiteCommand(sql, connection) 

        sqlCmd.ExecuteNonQuery() |> ignore
    )

let updateVersion id version =
    UseConnectionToDB (fun connection -> 
        let sql = "update cleavage set version=@version where cleavageId=@id; "
        use sqlCmd = new SQLiteCommand(sql, connection) 

        let add (name:string, value: string) = 
            sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

        add("@id", id.ToString())
        add("@version", version.ToString())

        sqlCmd.ExecuteNonQuery() |> ignore
    )
let saveCleavageProposed m (e:CleavageProposed)=
    UseConnectionToDB (fun connection -> 
        let sql = "Insert into cleavage (cleavageId,isOpenned) VALUES (@id, 1);insert into team (cleavageId,teamId,name) VALUES (@id,@teamAId,@nameTeamA);insert into team (cleavageId,teamId,name) VALUES (@id,@teamBId,@nameTeamB)  "
        use sqlCmd = new SQLiteCommand(sql, connection) 

        let add (name:string, value: string) = 
            sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

        add("@id", m.aggregateId.ToString())
        add("@teamAId", e.teamAId.ToString())
        add("@nameTeamA",  e.nameTeamA)
        add("@teamBId",  e.teamBId.ToString())
        add("@nameTeamB", e.nameTeamB)

        sqlCmd.ExecuteNonQuery() |> ignore
    ) 
    updateVersion m.aggregateId m.version



let saveCleavageClosed m  =
    UseConnectionToDB (fun connection -> 
        let sql = "update  cleavage set isOpenned=0 where cleavageId=@id;"
        use sqlCmd = new SQLiteCommand(sql, connection) 

        let add (name:string, value: string) = 
            sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

        add("@id", m.aggregateId.ToString())
        add("@bearId",  m.bear.bearId.ToString())
        add("@version", m.version.ToString())

        sqlCmd.ExecuteNonQuery() |> ignore
    ) 
    updateVersion m.aggregateId m.version




let saveCleavageOpenned m  =
    UseConnectionToDB (fun connection -> 
        let sql = "update  cleavage set isOpenned=1 where cleavageId=@id;"
        use sqlCmd = new SQLiteCommand(sql, connection) 

        let add (name:string, value: string) = 
            sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

        add("@id", m.aggregateId.ToString())
        add("@bearId",  m.bear.bearId.ToString())
        add("@version", m.version.ToString())

        sqlCmd.ExecuteNonQuery() |> ignore
    ) 
    updateVersion m.aggregateId m.version

let saveTeamJoined m (e:TeamJoined) =
    UseConnectionToDB (fun connection -> 
        let sql = "delete from teamPlayers  where teamId=@teamId and bearId=@bearId;Insert into teamPlayers (teamId,bearId) VALUES (@teamId,@bearId);"
        use sqlCmd = new SQLiteCommand(sql, connection) 

        let add (name:string, value: string) = 
            sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

        add("@teamId", e.teamId.ToString())
        add("@bearId", m.bear.bearId.ToString())   

        sqlCmd.ExecuteNonQuery() |> ignore    
    ) 
    updateVersion m.aggregateId m.version

let saveTeamLeaved m (e:TeamLeaved) =
    UseConnectionToDB (fun connection -> 
        let sql = "delete from teamPlayers  where teamId=@teamId and bearId=@bearId;"
        use sqlCmd = new SQLiteCommand(sql, connection) 

        let add (name:string, value: string) = 
            sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

        add("@teamId", e.teamId.ToString())
        add("@bearId", m.bear.bearId.ToString())   

        sqlCmd.ExecuteNonQuery() |> ignore    
    ) 
    updateVersion m.aggregateId m.version

let saveNameTeamChanged m (e:NameTeamChanged) =
    UseConnectionToDB (fun connection -> 
        let sql = "update team set name=@name  where teamId=@teamId ;"
        use sqlCmd = new SQLiteCommand(sql, connection) 

        let add (name:string, value: string) = 
            sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

        add("@teamId", e.teamId.ToString())
        add("@nameTeam", e.nameTeam)   

        sqlCmd.ExecuteNonQuery() |> ignore    
    ) 
    updateVersion m.aggregateId m.version

let savePlayerSwitched m (e:PlayerSwitched) =
    UseConnectionToDB (fun connection -> 
        let sql = "Update  teamPlayers  set teamId=@toTteamId  where teamId=@fromTeamId and bearId=@bearId   "
        use sqlCmd = new SQLiteCommand(sql, connection) 

        let add (name:string, value: string) = 
            sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

        add("@bearId", e.playerId.ToString())
        add("@fromTeamId", e.fromTeamId.ToString())   
        add("@toTteamId", e.toTeamId.ToString())   

        sqlCmd.ExecuteNonQuery() |> ignore    
    ) 
    updateVersion m.aggregateId m.version



let savePlayerKickedFromTeam m (e:PlayerKickedFromTeam) =
    UseConnectionToDB (fun connection -> 
        let sql = "delete  teamPlayers  where teamId=@teamId and bearId=@bearId   "
        use sqlCmd = new SQLiteCommand(sql, connection) 

        let add (name:string, value: string) = 
            sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

        add("@bearId", e.playerId.ToString())
        add("@teamId", e.teamId.ToString())   

        sqlCmd.ExecuteNonQuery() |> ignore    
    ) 
    updateVersion m.aggregateId m.version


let evtAppeared  escus (resolvedEvent:ResolvedEvent)= 
    withEvent name (fun m jsonEvent ->
        match  jsonEvent.case with
        | "CleavageProposed" -> 
            let e = deserializeEvt<CleavageProposed> resolvedEvent
            logProjection name resolvedEvent.OriginalEvent.EventStreamId resolvedEvent.Event.EventId  m e
            saveCleavageProposed m e
        | "CleavageClosed" -> 
            let e = deserializeEvt<CleavageClosed> resolvedEvent
            logProjection name resolvedEvent.OriginalEvent.EventStreamId resolvedEvent.Event.EventId  m e
            saveCleavageClosed m 
        | "CleavageOpenned" -> 
            let e = deserializeEvt<CleavageOpenned> resolvedEvent
            logProjection name resolvedEvent.OriginalEvent.EventStreamId resolvedEvent.Event.EventId  m e
            saveCleavageOpenned m 
        | "TeamJoined" -> 
            let e = deserializeEvt<TeamJoined> resolvedEvent
            logProjection name resolvedEvent.OriginalEvent.EventStreamId resolvedEvent.Event.EventId  m e
            saveTeamJoined m e
        | "TeamLeaved" -> 
            let e = deserializeEvt<TeamLeaved> resolvedEvent
            logProjection name resolvedEvent.OriginalEvent.EventStreamId resolvedEvent.Event.EventId  m e
            saveTeamLeaved m e
        | "NameTeamChanged" -> 
            let e = deserializeEvt<NameTeamChanged> resolvedEvent
            logProjection name resolvedEvent.OriginalEvent.EventStreamId resolvedEvent.Event.EventId  m e
            saveNameTeamChanged m e
         | "PlayerSwitched" -> 
            let e = deserializeEvt<PlayerSwitched> resolvedEvent
            logProjection name resolvedEvent.OriginalEvent.EventStreamId resolvedEvent.Event.EventId  m e
            savePlayerSwitched m e
         | "PlayerKickedFromTeam" -> 
            let e = deserializeEvt<PlayerKickedFromTeam> resolvedEvent
            logProjection name resolvedEvent.OriginalEvent.EventStreamId resolvedEvent.Event.EventId  m e
            savePlayerKickedFromTeam m e
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



