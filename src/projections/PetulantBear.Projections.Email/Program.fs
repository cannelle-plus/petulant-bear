// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

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

let dbConnection = ConfigurationManager.ConnectionStrings.["bear2bearDB"].ConnectionString
let host =ConfigurationManager.AppSettings.["host"]
let userName =ConfigurationManager.AppSettings.["userName"]
let password =ConfigurationManager.AppSettings.["password"]
let from =ConfigurationManager.AppSettings.["from"]
let port = Int32.Parse( ConfigurationManager.AppSettings.["port"])

type MailNotification = 
    {
        NotificationId:Guid;
        Recipient:string;
        Subject :string;
        Body :string;
    } 

type MyJob()=
    
    let sendMail (smtpClient:SmtpClient) connection notification=
        let send() =
            let mailMessage = new MailMessage()
            mailMessage.From <- new MailAddress(from)
            mailMessage.To.Add (new MailAddress(notification.Recipient))
            mailMessage.Subject <-  notification.Subject
            mailMessage.Body <- notification.Body

            let client = new SmtpClient()
            smtpClient.Send(mailMessage)

        let saveSentEmail () =
            let sql = "Insert into emailSent (notificationId , subject , body , recipient , nbAttempt) select notificationId , subject , body , recipient , nbAttempt from emailToSend where notificationId=@notificationId and recipient=@recipient; delete from emailToSend where notificationId=@notificationId and recipient=@recipient"

            use sqlCmd = new SQLiteCommand(sql,connection)
            sqlCmd.Parameters.Add(new SQLiteParameter("@notificationId",Data.DbType.String, notification.NotificationId.ToString())) |> ignore
            sqlCmd.Parameters.Add(new SQLiteParameter("@recipient", notification.Recipient.ToString())) |> ignore

            sqlCmd.ExecuteNonQuery() |> ignore

        try
            send()
            saveSentEmail()
        with 
            | :? System.Net.Mail.SmtpException as smtpEx-> () //retry send?
            | :? System.Data.SQLite.SQLiteException as dbEx-> () //retry save? log error?
    
    
    

    interface  IJob with
        member this.Execute ctx =
            use connection = new SQLiteConnection(dbConnection)
            connection.Open()
            let smtpClient = new SmtpClient()
            smtpClient.Host <- host
            smtpClient.EnableSsl <- true
            smtpClient.UseDefaultCredentials <- true
            smtpClient.Credentials <- new NetworkCredential(userName, password)
            smtpClient.Port <- port;
            
            let sql= "select notificationId, subject,body,recipient from emailToSend"
            use sqlCmd = new SQLiteCommand(sql,connection)

            let mailsToSend =
                [ use reader = sqlCmd.ExecuteReader();
                  while reader.Read() do
                    yield {Recipient=reader.["recipient"].ToString(); Subject=reader.["subject"].ToString();Body=reader.["body"].ToString(); NotificationId=Guid.Parse(reader.["notificationId"].ToString()); }
                ]
            mailsToSend
            |> List.iter (sendMail smtpClient connection)
            Console.WriteLine("test")
            
    

[<EntryPoint>]
let main argv = 
    printfn "%A" argv
    
    

    let confElmah :Logary.Targets.ElmahIO.ElmahIOConf =
        match Guid.TryParse(ConfigurationManager.AppSettings.Get("elmah.io")) with
        | true, logId ->{ logId = logId; }
        | false, _->{ logId = Guid.Empty; }

    
    
    use logary =
        withLogary' "bear2bear.mail" (
            withTargets [
                Console.create Console.empty "console"
                Logary.Targets.ElmahIO.create  confElmah "elmah"

            ] >>
                withRules [
                Rule.create (Regex(".*", RegexOptions.Compiled)) "console" (fun _ -> true) (fun _ -> true) Info
                Rule.create (Regex(".*", RegexOptions.Compiled)) "elmah" (fun _ -> true) (fun _ -> true) Error
                ]
        )


    try
        let scheduler = StdSchedulerFactory.GetDefaultScheduler()
        scheduler.Start() |> ignore

        let job = JobBuilder.Create<MyJob>()
                            .WithIdentity("job1","groupe1")
                            .Build()
        let trigger = TriggerBuilder.Create()
                                    .WithIdentity("trigger1","group1")
                                    .StartNow()
                                    .WithSimpleSchedule(fun x -> x.WithIntervalInSeconds(10).RepeatForever()|> ignore)
                                    .Build()
        
        scheduler.ScheduleJob(job,trigger) |> ignore

        Thread.Sleep(TimeSpan.FromSeconds(60 |> float))


        scheduler.Shutdown()

    with  ex -> Console.WriteLine(ex.ToString())

    0 // return an integer exit code
