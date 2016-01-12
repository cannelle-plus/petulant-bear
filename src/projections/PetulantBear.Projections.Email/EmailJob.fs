module EmailJob


open System
open System.Configuration
open System.Net
open System.Net.Mail
open System.Threading
open System.Data.SQLite

open Quartz
open Quartz.Impl

open Logary
open Logary.Targets
open Logary.Configuration.Config
open Logary.Metrics
open NodaTime

open System.Text.RegularExpressions

[<DisallowConcurrentExecution>]
type MyJob()=
    

    
    let sendMail (smtpClient:SmtpClient) connection from nbAttemptMax notification =
        let send() =
            Console.WriteLine("sending mail...")
            let mailMessage = new MailMessage()
            mailMessage.From <- new MailAddress(from)
            mailMessage.To.Add (new MailAddress(notification.Recipient))
            mailMessage.Subject <-  notification.Subject
            mailMessage.Body <- notification.Body

            let client = new SmtpClient()
            smtpClient.Send(mailMessage)

        let saveSentEmail () =
            Console.WriteLine("saving mail to sent queue...")
            let sql = "Insert into emailSent (notificationId , subject , body , recipient , nbAttempt,sentDate) VALUES (@notificationId , @subject , @body , @recipient , @nbAttempt,@now) ; delete from emailToSend where notificationId=@notificationId and recipient=@recipient;"

            use sqlCmd = new SQLiteCommand(sql,connection)
            sqlCmd.Parameters.Add(new SQLiteParameter("@notificationId",notification.NotificationId.ToString())) |> ignore
            sqlCmd.Parameters.Add(new SQLiteParameter("@subject", notification.Subject)) |> ignore
            sqlCmd.Parameters.Add(new SQLiteParameter("@body", notification.Body)) |> ignore
            sqlCmd.Parameters.Add(new SQLiteParameter("@recipient", notification.Recipient)) |> ignore
            sqlCmd.Parameters.Add(new SQLiteParameter("@nbAttempt", notification.NbAttempt.ToString())) |> ignore
            sqlCmd.Parameters.Add(new SQLiteParameter("@now", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ"))) |> ignore

            sqlCmd.ExecuteNonQuery() |> ignore
            
        try
            send()
            saveSentEmail()
        with 
            | :? System.Net.Mail.SmtpException as smtpEx-> 
                if notification.NbAttempt<=nbAttemptMax then
                    //increment the counter of attempt to send the email
                    let sqlNbAttempt = "Update emailToSend set nbAttempt = nbAttempt +1 where notificationId=@notificationId and recipient=@recipient;"

                    use sqlCmdNbAttempt = new SQLiteCommand(sqlNbAttempt,connection)
                    sqlCmdNbAttempt.Parameters.Add(new SQLiteParameter("@notificationId",Data.DbType.String, notification.NotificationId.ToString())) |> ignore
                    sqlCmdNbAttempt.Parameters.Add(new SQLiteParameter("@recipient", notification.Recipient.ToString())) |> ignore

                    sqlCmdNbAttempt.ExecuteNonQuery() |> ignore
                else
                    //pass the email to the deadQueue
                    let sql = "Insert into deadQueue (notificationId , subject , body , recipient , nbAttempt,scheduledDate) select notificationId , subject , body , recipient , nbAttempt, scheduledDate from emailToSend where notificationId=@notificationId and recipient=@recipient; delete from emailToSend where notificationId=@notificationId and recipient=@recipient"

                    use sqlCmd = new SQLiteCommand(sql,connection)
                    sqlCmd.Parameters.Add(new SQLiteParameter("@notificationId",Data.DbType.String, notification.NotificationId.ToString())) |> ignore
                    sqlCmd.Parameters.Add(new SQLiteParameter("@recipient", notification.Recipient.ToString())) |> ignore

                    sqlCmd.ExecuteNonQuery() |> ignore
                
            | :? System.Data.SQLite.SQLiteException as dbEx-> 
                sprintf "error occured %A" dbEx
                |> LogLine.create' LogLevel.Error 
                |> Logging.getCurrentLogger().Log 
                raise dbEx 
    

    interface  IJob with
        member this.Execute ctx =

            let dbConnection = ConfigurationManager.ConnectionStrings.["bear2bearDB"].ConnectionString
            let host =ConfigurationManager.AppSettings.["host"]
            let userName =ConfigurationManager.AppSettings.["userName"]
            let password =ConfigurationManager.AppSettings.["password"]
            let from =ConfigurationManager.AppSettings.["from"]
            let port = Int32.Parse( ConfigurationManager.AppSettings.["port"])
            let nbAttemptMax = Int32.Parse( ConfigurationManager.AppSettings.["NbAttemptMax"])

            Console.WriteLine("checking in...")
            use connection = new SQLiteConnection(dbConnection)
            connection.Open()
            let smtpClient = new SmtpClient()
            smtpClient.Host <- host
            smtpClient.EnableSsl <- true
//            smtpClient.UseDefaultCredentials <- true
            smtpClient.Credentials <- new NetworkCredential(userName, password)
            smtpClient.Port <- port;
            
            let sql= "select notificationId, subject,body,recipient,scheduledDate,nbAttempt from emailToSend where ScheduledDate<@now"
            use sqlCmd = new SQLiteCommand(sql,connection)

            sqlCmd.Parameters.Add(new SQLiteParameter("@now", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ"))) |> ignore

            let mailsToSend =
                [ use reader = sqlCmd.ExecuteReader();
                  while reader.Read() do
                    yield {Recipient=reader.["recipient"].ToString(); Subject=reader.["subject"].ToString();Body=reader.["body"].ToString(); NotificationId=Guid.Parse(reader.["notificationId"].ToString()); ScheduledDate=DateTime.Parse( reader.["scheduledDate"].ToString()) ; NbAttempt=Int32.Parse( reader.["nbAttempt"].ToString())}
                ]
            mailsToSend
            |> List.iter (sendMail smtpClient connection from nbAttemptMax)
            Console.WriteLine("checking out...")