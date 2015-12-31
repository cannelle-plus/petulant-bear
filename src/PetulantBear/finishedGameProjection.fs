module PetulantBear.Projections.FinishedGame

open System
open System.Text
open System.Runtime.Serialization
open System.Data.SQLite
open System.Configuration

open EventStore.ClientAPI
open Newtonsoft.Json

open PetulantBear.FinishedGames
open PetulantBear.FinishedGames.Contracts

open PetulantBear.SqliteBear2bearDB
open PetulantBear.Projections.Common

let name= "finishedGameProjection"

let resetProjection connection =
    
        let sql = "delete from gameFinished;"
        use sqlCmd = new SQLiteCommand(sql, connection) 

        sqlCmd.ExecuteNonQuery() |> ignore
  
//this should be a an event of the room, but for now it will do
let saveScoreGiven connection m (e:ScoreGiven)=
    let sql = "Delete from gameFinished where gameId=@gameId; Insert into gameFinished (gameId,version,teamAId,teamAScore,teamBId,teamBScore)  VALUES (@gameId,@version,@teamAId,@teamAScore,@teamBId,@teamBScore)"
    use sqlCmd = new SQLiteCommand(sql, connection) 

    let add (name:string, value: string) = 
        sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

    add("@gameId", m.aggregateId.ToString())
    add("@version", m.version.ToString())
    add("@teamAId", e.teamAId.ToString())
    add("@teamAScore", e.teamAScore.ToString())
    add("@teamBId", e.teamBId.ToString())
    add("@teamBScore", e.teamBScore.ToString())

    sqlCmd.ExecuteNonQuery() |> ignore

let evtAppeared  connection escus (resolvedEvent:ResolvedEvent)= 
    withEvent connection name (fun m jsonEvent ->
        match  jsonEvent.case with
        | "Scheduled" -> 
            let e = deserializeEvt<ScoreGiven> resolvedEvent
            logProjection name resolvedEvent  m e
            saveScoreGiven connection m e
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



