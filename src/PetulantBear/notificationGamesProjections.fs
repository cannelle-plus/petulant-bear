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

let saveNotifications connection (m:Enveloppe) recipients (notificationDate:DateTime) subject body =

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
        
    

        use sqlCmd = new SQLiteCommand(connection) 
            
        let add (name:string, value: string) = 
            sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

        let notificationId = Guid.NewGuid()
        let sqlNotification = ["Insert into notifications (notificationId,notificationType,eventId) VALUES (@notificationId, 'email',@eventId);"]

        add("@notificationId", notificationId.ToString())
        add("@eventId", m.messageId.ToString())
        add("@scheduledDate",  notificationDate.ToString("yyyy-MM-ddTHH:mm:ssZ"))        
        
        let sql = recipients
                    |> List.fold (fun (agg,i) b  ->
                
                        let sql = sprintf "insert into emailToSend (notificationId,subject,body,recipient,scheduledDate,nbAttempt) VALUES (@notificationId%i,@subject%i,@body%i,@recipient%i,@scheduledDate,0);" i i i i
            
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

let saveScheduledGame connection (m:Enveloppe) (e:GameScheduled)=
    let subject = getTemplate "scheduledGameSubject";
    let bodyTemplate = getTemplate "scheduledGameBody";
    
    let sqlPlayers = "select b.bearId,b.bearUsername,b.email,u.socialId,b.bearAvatarId from Bears as b inner join users as u on b.bearId=u.bearId where b.email IS NOT NULL"
    use sqlCmdPlayers = new SQLiteCommand(sqlPlayers, connection) 

    let recipients : BearDetail list = [
        use readerPlayers = sqlCmdPlayers.ExecuteReader()
        while (readerPlayers.Read()) do
        yield { bearId = Guid.Parse(readerPlayers.["bearId"].ToString())
                bearUsername = readerPlayers.["bearUsername"].ToString()
                bearEmail = readerPlayers.["email"].ToString()
                socialId = readerPlayers.["socialId"].ToString()
                bearAvatarId = Int32.Parse(readerPlayers.["bearAvatarId"].ToString()) 
        }
    ]

    let root = new PetulantBear.Formatter.Context()
    let game = new PetulantBear.Formatter.Context()    

    game.add("startDate", e.startDate.ToString());
    game.add("location", e.location);
    game.add("maxPlayer", e.maxPlayers.ToString());
    game.add("nbPlayer", e.nbPlayers.ToString());
    game.add("owner",m.bear.username);
    
    root.add("game", game );
    let body = PetulantBear.Runner.run root bodyTemplate
    saveNotifications connection m recipients (e.startDate.AddDays(float <| -7)) subject body

let savePlayerRemovedFromTheBench connection (m:Enveloppe) (e:PlayerRemovedFromTheBench)=
    
    let subject = getTemplate "playerRemovedFromTheBenchSubject";
    let body = getTemplate "playerRemovedFromTheBenchBody";

    let sqlPlayers = "select b.bearId,b.bearUsername,b.email,u.socialId,b.bearAvatarId from Bears as b inner join users as u on b.bearId=u.bearId where b.email IS NOT NULL and b.bearId=@bearId"
    use sqlCmdPlayers = new SQLiteCommand(sqlPlayers, connection) 

    sqlCmdPlayers.Parameters.Add(new SQLiteParameter( "@bearId", e.bearId.ToString())) |> ignore

    let recipients : BearDetail list = [
        use readerPlayers = sqlCmdPlayers.ExecuteReader()
        while (readerPlayers.Read()) do
        yield { bearId = Guid.Parse(readerPlayers.["bearId"].ToString())
                bearUsername = readerPlayers.["bearUsername"].ToString()
                bearEmail = readerPlayers.["email"].ToString()
                socialId = readerPlayers.["socialId"].ToString()
                bearAvatarId = Int32.Parse(readerPlayers.["bearAvatarId"].ToString()) 
        }
    ]
    saveNotifications connection m recipients DateTime.Now subject body

let evtAppeared connection escus (resolvedEvent:ResolvedEvent)= 
    withEvent connection name (fun m jsonEvent ->
        match  jsonEvent.case with
        | "Scheduled" -> 
            let e = deserializeEvt<GameScheduled> resolvedEvent
            logProjection name resolvedEvent m e
            saveScheduledGame connection m e
        | "PlayerRemovedFromTheBench" -> 
            let e = deserializeEvt<PlayerRemovedFromTheBench> resolvedEvent
            logProjection name resolvedEvent m e
            savePlayerRemovedFromTheBench connection m e
        | unknown -> 
            sprintf "unknown event %s" unknown
            |> Logary.LogLine.error
            |> Logary.Logging.getCurrentLogger().Log
    )  escus (resolvedEvent:ResolvedEvent)
        
let catchup escus = 
    sprintf "catchup!"
    |> Logary.LogLine.error 
    |> Logary.Logging.getCurrentLogger().Log

let onError (escus:EventStoreCatchUpSubscription) (sdr:SubscriptionDropReason) (e:Exception) = 
    let msgError = 
        match sdr with
        | SubscriptionDropReason.AccessDenied -> "AccessDenied" 
        | SubscriptionDropReason.CatchUpError -> "CatchUpError" 
        | SubscriptionDropReason.ConnectionClosed -> "ConnectionClosed" 
        | SubscriptionDropReason.EventHandlerException -> "EventHandlerException" 
        | SubscriptionDropReason.MaxSubscribersReached -> "MaxSubscribersReached" 
        | SubscriptionDropReason.UserInitiated -> "UserInitiated" 
        | SubscriptionDropReason.NotAuthenticated -> "NotAuthenticated" 
        | SubscriptionDropReason.NotFound -> "NotFound" 
        | SubscriptionDropReason.PersistentSubscriptionDeleted -> "PersistentSubscriptionDeleted" 
        | SubscriptionDropReason.ProcessingQueueOverflow -> "ProcessingQueueOverflow" 
        | SubscriptionDropReason.ServerError -> "ServerError" 
        | SubscriptionDropReason.SubscribingError -> "SubscribingError" 
        | SubscriptionDropReason.Unknown -> "Unknown" 

    sprintf "error! - SubscriptionDropReason= %s, exception : %A" msgError e
    |> Logary.LogLine.error 
    |> Logary.Logging.getCurrentLogger().Log


let projection =  {
        resetProjection = resetProjection;
        eventAppeared = evtAppeared;
        catchup = catchup;
        onError = onError ;
    }