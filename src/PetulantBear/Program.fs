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

    let logger = Suave.Logging.Loggers.ConsoleWindowLogger Suave.Logging.LogLevel.Verbose

    let section = ConfigurationManager.GetSection("akka"):?> AkkaConfigurationSection
    let system = System.create "System" ( section.AkkaConfig)
    let config = 
        { defaultConfig with  
            bindings = [ HttpBinding.mk' HTTP ipAddress port ] 
            homeFolder = Some(rootPath)
        }

    (PetulantBear.Application.app rootPath urlSite system saveEvents Users.authenticateWithLogin)
    >>= Suave.Http.Applicatives.log logger logFormat
    |> startWebServer config
    0
