module Program

open Suave // always open suave
open Suave.Logging
open Suave.Http
open Suave.Http.Applicatives
open Suave.Http.Successful // for OK-result
open Suave.Web // for config
open System.Configuration

open Akka.Configuration.Hocon
open Akka.FSharp

//*********************
open Suave
open Suave.Types
open Suave.Cookie
open Suave.Auth

open PetulantBear


open System
open System.Collections.Generic
open System.Text.RegularExpressions


let events = new List<Event<Games.Events>>()
let saveEvents name (id, expectedVersion, evt) =
    events.Add({ id= id; version=expectedVersion;payLoad= evt })


//    serverKey             = Utils.Crypto.generateKey HttpRuntime.ServerKeyLength
//    errorHandler          = defaultErrorHandler
//    listenTimeout         = TimeSpan.FromMilliseconds 2000.
//    cancellationToken     = Async.DefaultCancellationToken
//    bufferSize            = 2048
//    maxOps                = 100
//    mimeTypesMap          = Writers.defaultMimeTypesMap
//    homeFolder            = None
//    compressedFilesFolder = None
//    logger                = logger
//    cookieSerialiser = 



open Suave.Logging
open Logary
open Logary.Configuration
open Logary.Targets
open Logary.Metrics
open NodaTime


[<EntryPoint>]
let main args =

    let rootPath = ConfigurationManager.AppSettings.["rootPath"]
    let ipAddress = ConfigurationManager.AppSettings.["IPAddress"]
    let port = Int32.Parse( ConfigurationManager.AppSettings.["Port"])
    let urlSite = ConfigurationManager.AppSettings.["urlSite"]


    let confElmah :Logary.Targets.ElmahIO.ElmahIOConf =
        match Guid.TryParse(ConfigurationManager.AppSettings.Get("elmah.io")) with
        | true, logId ->{ logId = logId; }
        | false, _->{ logId = Guid.Empty; }
     
    
    use logary =
      withLogary' "logibit.web" (
        withTargets [
          Console.create Console.empty "console"
          Logary.Targets.ElmahIO.create  confElmah "elmah"

        ] >>
          withRules [
            Rule.create (Regex(".*", RegexOptions.Compiled)) "console" (fun _ -> true) (fun _ -> true) Info
            Rule.create (Regex(".*", RegexOptions.Compiled)) "elmah" (fun _ -> true) (fun _ -> true) Error
          ]
        )
    
    
    let section = ConfigurationManager.GetSection("akka"):?> AkkaConfigurationSection
    
    let system = System.create "System" ( section.AkkaConfig)
    let config = 
        { defaultConfig with 
            logger = SuaveAdapter(logary.GetLogger "suave")
            bindings = [ HttpBinding.mk' HTTP ipAddress port ] 
            homeFolder = Some(rootPath)
        }

    (PetulantBear.Application.app rootPath urlSite system saveEvents Users.authenticateWithLogin)
    |> startWebServer config
    0
