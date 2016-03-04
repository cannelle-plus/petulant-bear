// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open System
open System.Configuration
open System.Threading

open Quartz
open Quartz.Impl

open Logary
open Logary.Targets
open Logary.Configuration.Config

open System.Text.RegularExpressions



[<EntryPoint>]
let main argv = 
    

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

        let job = JobBuilder.Create<EmailJob.MyJob>()
                            .WithIdentity("job1","groupe1")
                            .Build()
        let trigger = TriggerBuilder.Create()
                                    .WithIdentity("trigger1","group1")
                                    .StartNow()
                                    .WithSimpleSchedule(fun x -> x.WithIntervalInMinutes(10).RepeatForever()|> ignore)
                                    .Build()
        
                
        Console.WriteLine( scheduler.ScheduleJob(job,trigger))

        


        

    with  ex -> Console.WriteLine(ex.ToString())

    
    0 // return an integer exit code
