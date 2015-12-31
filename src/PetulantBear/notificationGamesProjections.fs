module PetulantBear.Projections.NotificationGames

open System
open System.Text
open System.IO
open System.Reflection
open System.Runtime.Serialization
open System.Data.SQLite
open System.Configuration

open EventStore.ClientAPI
open Newtonsoft.Json

open PetulantBear.Games
open PetulantBear.Games.Contracts
open PetulantBear.Bears.Contracts
open PetulantBear.SqliteBear2bearDB
open PetulantBear.Projections.Common


let name= "emailGamesProjections"

let templatesDir = 
    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
    |> sprintf "%s/templates" 

let getTemplate name = 
    sprintf "%s/%s.txt" templatesDir name 
    |> System.IO.File.ReadAllText

let resetProjection connection =
    let sql = "delete from notifications ;delete from emailToSend;delete from emailSent;delete from deadQueue;"
    use sqlCmd = new SQLiteCommand(sql, connection) 

    sqlCmd.ExecuteNonQuery() |> ignore
    

let saveScheduledGame connection (m:Enveloppe) (e:GameScheduled)=

    let sqlNotif = "select count(*) from notifications where eventId=@eventId and notificationType= 'email'"
    use sqlCmdNotif = new SQLiteCommand(sqlNotif, connection) 

    let addparamNotif (name:string, value: string) = 
            sqlCmdNotif.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

    addparamNotif("@eventId", m.messageId.ToString())

    use readerNotif = sqlCmdNotif.ExecuteReader()

    let isAlreadyNotified = 
        if (readerNotif.Read()) then 
            (Int32.Parse(readerNotif.[0].ToString())<>0)
        else 
            false
                
    if not  <| isAlreadyNotified then
        let sqlPlayers = "select b.bearId,b.bearUsername,b.email,u.socialId,b.bearAvatarId from Bears as b inner join users as u on b.bearId=u.bearId where b.email IS NOT NULL"
        use sqlCmdPlayers = new SQLiteCommand(sqlPlayers, connection) 

        let recipients : BearDetail list= 
            [ use readerPlayers = sqlCmdPlayers.ExecuteReader()
              while (readerPlayers.Read()) do
                yield { bearId = Guid.Parse(readerPlayers.["bearId"].ToString())
                        bearUsername = readerPlayers.["bearUsername"].ToString()
                        bearEmail = readerPlayers.["email"].ToString()
                        socialId = readerPlayers.["socialId"].ToString()
                        bearAvatarId = Int32.Parse(readerPlayers.["bearAvatarId"].ToString()) 
                }
            ]
    

        use sqlCmd = new SQLiteCommand(connection) 
            
        let add (name:string, value: string) = 
            sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

        let notificationId = Guid.NewGuid()
        let sqlNotification = ["Insert into notifications (notificationId,notificationType,eventId) VALUES (@notificationId, 'email',@eventId);"]

        add("@notificationId", notificationId.ToString())
        add("@eventId", m.messageId.ToString())
        
        let sql = recipients
                    |> List.fold (fun (agg,i) b  ->
                
                        let sql = sprintf "insert into emailToSend (notificationId,subject,body,recipient) VALUES (@notificationId%i,@subject%i,@body%i,@recipient%i);" i i i i

                        let subject = getTemplate "scheduledGameSubject";
                        let body = getTemplate "scheduledGameBody";
            
                        add(sprintf "@notificationId%i" i, notificationId.ToString())
                        add(sprintf "@eventId%i" i, m.messageId.ToString())
                        add(sprintf "@subject%i" i, subject)
                        add(sprintf "@body%i" i,  body)
                        add(sprintf "@recipient%i" i,  b.bearEmail)
                        let newIndex = i+1
                        sql::agg,newIndex
                    )  (sqlNotification,0)
                    
        
        
        sqlCmd.CommandText <- fst(sql) |> List.fold (+) ""
         
        sqlCmd.ExecuteNonQuery() |> ignore


//let saveCancelled m =
//    UseConnectionToDB (fun connection -> 
//        let sql = "update GamesList set currentState=0,version =@version where id=@id;"
//        use sqlCmd = new SQLiteCommand(sql, connection) 
//
//        let add (name:string, value: string) = 
//            sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore
//
//        add("@id", m.aggregateId.ToString())
//        add("@version", m.version.ToString())
//        
//        sqlCmd.ExecuteNonQuery() |> ignore
//    ) |> ignore
//
//
//let saveStartDateChanged m e =
//    UseConnectionToDB (fun connection -> 
//        let sql = "Update  GamesList set startDate= @startDate where id=@id;Update GamesList set version =@version where id=@id;"
//        use sqlCmd = new SQLiteCommand(sql, connection) 
//
//        let add (name:string, value: string) = 
//            sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore
//
//        add("@id", m.aggregateId.ToString())
//        add("@version", m.version.ToString())
//        add("@startDate", e.startDate.ToString("yyyy-MM-ddTHH:mm:ssZ"))   
//
//        sqlCmd.ExecuteNonQuery() |> ignore    
//    ) 


let evtAppeared connection escus (resolvedEvent:ResolvedEvent)= 
    withEvent connection name (fun m jsonEvent ->
        match  jsonEvent.case with
        | "Scheduled" -> 
            let e = deserializeEvt<GameScheduled> resolvedEvent
            logProjection name resolvedEvent m e
            saveScheduledGame connection m e
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


let projection =  {
        resetProjection = resetProjection;
        eventAppeared = evtAppeared;
        catchup = catchup;
        onError = onError ;
    }