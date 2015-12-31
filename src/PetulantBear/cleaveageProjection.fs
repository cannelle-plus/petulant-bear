module PetulantBear.Projections.Cleaveage

open System
open System.Text
open System.Data.SQLite

open EventStore.ClientAPI

open PetulantBear.Cleaveage
open PetulantBear.Cleaveage.Contracts
open PetulantBear.Projections.Common


let name= "cleaveageProjection"

let resetProjection connection =
    let sql = "delete from teamPlayers where teamId in ( Select teamId from team where cleaveageId in( select cleaveageId from cleaveage where  cleaveage.version IS NOT NULL)) ; delete from team where cleaveageId in( select cleaveageId from cleaveage where  cleaveage.version IS NOT NULL);delete from cleaveage where  cleaveage.version IS NOT NULL"
    use sqlCmd = new SQLiteCommand(sql, connection) 

    sqlCmd.ExecuteNonQuery() |> ignore
    

let updateVersion connection id version =
    let sql = "update cleaveage set version=@version where cleaveageId=@id; "
    use sqlCmd = new SQLiteCommand(sql, connection) 

    let add (name:string, value: string) = 
        sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

    add("@id", id.ToString())
    add("@version", version.ToString())

    sqlCmd.ExecuteNonQuery() |> ignore

let saveCleaveageProposed connection m (e:CleaveageProposed)=
    
    let sql = "Insert into cleaveage (cleaveageId,gameId,isOpenned) VALUES (@id,@gameId, 1);insert into team (cleaveageId,teamId,name) VALUES (@id,@teamAId,@nameTeamA);insert into team (cleaveageId,teamId,name) VALUES (@id,@teamBId,@nameTeamB)  "
    use sqlCmd = new SQLiteCommand(sql, connection) 

    let add (name:string, value: string) = 
        sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

    add("@id", m.aggregateId.ToString())
    add("@gameId", e.gameId.ToString())
    add("@teamAId", e.teamAId.ToString())
    add("@nameTeamA",  e.nameTeamA)
    add("@teamBId",  e.teamBId.ToString())
    add("@nameTeamB", e.nameTeamB)

    sqlCmd.ExecuteNonQuery() |> ignore
    
    updateVersion connection m.aggregateId m.version


let saveCleaveageClosed connection m  =
    
    let sql = "update  cleaveage set isOpenned=0 where cleaveageId=@id;"
    use sqlCmd = new SQLiteCommand(sql, connection) 

    let add (name:string, value: string) = 
        sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

    add("@id", m.aggregateId.ToString())
    add("@bearId",  m.bear.bearId.ToString())
    add("@version", m.version.ToString())

    sqlCmd.ExecuteNonQuery() |> ignore
     
    updateVersion connection m.aggregateId m.version

let saveCleaveageOpenned connection m  =
    
    let sql = "update  cleaveage set isOpenned=1 where cleaveageId=@id;"
    use sqlCmd = new SQLiteCommand(sql, connection) 

    let add (name:string, value: string) = 
        sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

    add("@id", m.aggregateId.ToString())
    add("@bearId",  m.bear.bearId.ToString())
    add("@version", m.version.ToString())

    sqlCmd.ExecuteNonQuery() |> ignore
     
    updateVersion connection m.aggregateId m.version

let saveTeamJoined connection m (e:TeamJoined) =
    
    let sql = "delete from teamPlayers  where teamId=@teamId and bearId=@bearId;Insert into teamPlayers (teamId,bearId) VALUES (@teamId,@bearId);"
    use sqlCmd = new SQLiteCommand(sql, connection) 

    let add (name:string, value: string) = 
        sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

    add("@teamId", e.teamId.ToString())
    add("@bearId", m.bear.bearId.ToString())

    sqlCmd.ExecuteNonQuery() |> ignore
    
    updateVersion connection m.aggregateId m.version

let saveTeamLeaved connection m (e:TeamLeaved) =
    
    let sql = "delete from teamPlayers  where teamId=@teamId and bearId=@bearId;"
    use sqlCmd = new SQLiteCommand(sql, connection) 

    let add (name:string, value: string) = 
        sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

    add("@teamId", e.teamId.ToString())
    add("@bearId", m.bear.bearId.ToString())

    sqlCmd.ExecuteNonQuery() |> ignore
     
    updateVersion connection m.aggregateId m.version

let saveNameTeamChanged connection m (e:NameTeamChanged) =
    
    let sql = "update team set name=@nameTeam  where teamId=@teamId ;"
    use sqlCmd = new SQLiteCommand(sql, connection) 

    let add (name:string, value: string) = 
        sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

    add("@teamId", e.teamId.ToString())
    add("@nameTeam", e.nameTeam)

    sqlCmd.ExecuteNonQuery() |> ignore
    
    updateVersion connection m.aggregateId m.version

let savePlayerSwitched connection m (e:PlayerSwitched) =
    
    let sql = "Update  teamPlayers  set teamId=@toTteamId  where teamId=@fromTeamId and bearId=@bearId   "
    use sqlCmd = new SQLiteCommand(sql, connection) 

    let add (name:string, value: string) = 
        sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

    add("@bearId", e.playerId.ToString())
    add("@fromTeamId", e.fromTeamId.ToString())
    add("@toTteamId", e.toTeamId.ToString())

    sqlCmd.ExecuteNonQuery() |> ignore
     
    updateVersion connection m.aggregateId m.version



let savePlayerKickedFromTeam connection m (e:PlayerKickedFromTeam) =
    
    let sql = "delete  from teamPlayers  where teamId=@teamId and bearId=@bearId   "
    use sqlCmd = new SQLiteCommand(sql, connection) 

    let add (name:string, value: string) = 
        sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

    add("@bearId", e.playerId.ToString())
    add("@teamId", e.teamId.ToString())

    sqlCmd.ExecuteNonQuery() |> ignore
    
    updateVersion connection  m.aggregateId m.version


let evtAppeared connection escus (resolvedEvent:ResolvedEvent)= 
    withEvent connection name (fun m jsonEvent ->
        match  jsonEvent.case with
        | "CleaveageProposed" -> 
            let e = deserializeEvt<CleaveageProposed> resolvedEvent
            logProjection name resolvedEvent  m e
            saveCleaveageProposed connection m e
        | "CleaveageClosed" -> 
            let e = deserializeEvt<CleaveageClosed> resolvedEvent
            logProjection name resolvedEvent  m e
            saveCleaveageClosed connection m 
        | "CleaveageOpenned" -> 
            let e = deserializeEvt<CleaveageOpenned> resolvedEvent
            logProjection name resolvedEvent  m e
            saveCleaveageOpenned connection m 
        | "TeamJoined" -> 
            let e = deserializeEvt<TeamJoined> resolvedEvent
            logProjection name resolvedEvent  m e
            saveTeamJoined connection m e
        | "TeamLeaved" -> 
            let e = deserializeEvt<TeamLeaved> resolvedEvent
            logProjection name resolvedEvent  m e
            saveTeamLeaved connection m e
        | "NameTeamChanged" -> 
            let e = deserializeEvt<NameTeamChanged> resolvedEvent
            logProjection name resolvedEvent  m e
            saveNameTeamChanged connection m e
         | "PlayerSwitched" -> 
            let e = deserializeEvt<PlayerSwitched> resolvedEvent
            logProjection name resolvedEvent  m e
            savePlayerSwitched connection m e
         | "PlayerKickedFromTeam" -> 
            let e = deserializeEvt<PlayerKickedFromTeam> resolvedEvent
            logProjection name resolvedEvent  m e
            savePlayerKickedFromTeam connection m e
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



