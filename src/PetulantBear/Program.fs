module Program

open Suave // always open suave
open Suave.Logging
open Suave.Types
open Suave.Web // for config
open System.Configuration

open Logary
open Logary.Configuration
open Logary.Targets

open Akka.Configuration.Hocon
open Akka.FSharp

open PetulantBear

open System
open System.Net
open System.Text.RegularExpressions
open System.Data.SQLite

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

type PetulantConfig = {
    RootPath:string;
    IpAddress :string;
    Port :int;
    UrlSite: string;
    EventStoreConnectionString :string;
    EventStoreClientIp :string;
    EventStoreClientPort :int;
    EventStoreClientUsername :string;
    EventStoreClientPassword :string;
    DbConnection :string;
    Elmah:Logary.Targets.ElmahIO.ElmahIOConf;
}
    
with static member Initial = {RootPath=""; IpAddress =""; Port =0; UrlSite= ""; EventStoreConnectionString =""; EventStoreClientIp=""; EventStoreClientPort=0; EventStoreClientUsername=""; EventStoreClientPassword=""; DbConnection =""; Elmah={ logId = Guid.Empty; }}

let parseArg conf (arg:string) =
    match arg.Split([|'='|],2) with
    | [|a;b|] ->
        match a with
        | "--help" ->
            Console.WriteLine("parameters accepted : --help, --rootPath,--ipAddress,--port,--urlSite,--eventStoreConnectionString,--dbConnection,--elmah")
            conf
        | "--rootPath" -> { conf with RootPath=b}
        | "--ipAddress" -> { conf with IpAddress=b}
        | "--port" ->
            match Int32.TryParse( b) with
            | true,x ->  { conf with Port=x}
            | false,_ -> conf
        | "--urlSite" -> { conf with UrlSite=b}
        | "--eventStoreConnectionString" -> { conf with EventStoreConnectionString=b}
        | "--eventStoreClientIp" -> { conf with EventStoreClientIp=b}
        | "--eventStoreClientPort" ->
          match Int32.TryParse( b) with
          | true,x ->  { conf with EventStoreClientPort=x}
          | false,_ -> conf
        | "--eventStoreConnectionUsername" -> { conf with EventStoreClientUsername=b}
        | "--eventStoreConnectionPassword" -> { conf with EventStoreClientPassword=b}
        | "--dbConnection" -> { conf with DbConnection=b}
        | "--elmah" ->
            match Guid.TryParse( b) with
            | true,x ->  { conf with Elmah={ logId = x; }}
            | false,_ -> conf
        | x ->
            sprintf "unknown parameter %s" x
            |> failwith
    | [|a|] ->
        sprintf "unrecognised parameter format %s, --myParameter=value expected" a
        |> failwith
    | _ ->
        sprintf "bouh! non ho capito!!" 
        |> failwith



[<EntryPoint>]
let main args =

    // parse the app config to extract values
    let rootPath = ConfigurationManager.AppSettings.["rootPath"]
    let ipAddress = ConfigurationManager.AppSettings.["IPAddress"]
    let coukdParsePort,port = Int32.TryParse( ConfigurationManager.AppSettings.["Port"])
    let urlSite = ConfigurationManager.AppSettings.["urlSite"]
    let eventStoreConnectionString = ConfigurationManager.AppSettings.["eventStoreConnectionString"]
    let dbConnection = ConfigurationManager.ConnectionStrings.["bear2bearDB"].ConnectionString
    let couldParseElmah,elmah = Guid.TryParse(ConfigurationManager.AppSettings.Get("elmah.io"))
    let eventStoreClientIp = ConfigurationManager.AppSettings.["eventStoreClientIp"]
    let couldParseClientPort, eventStoreClientPort = Int32.TryParse( ConfigurationManager.AppSettings.["eventStoreClientPort"])
    let eventStoreUsername = ConfigurationManager.AppSettings.["eventStoreUserName"]
    let eventStorePassword = ConfigurationManager.AppSettings.["eventStorePassword"]

    // apply default values to config over appConfig
    let initialConfig = {
        PetulantConfig.Initial with
            DbConnection = dbConnection
            RootPath = if (rootPath <>"") then rootPath else "wwwwroot"
            IpAddress = if (ipAddress <>"") then ipAddress else "127.0.0.1"
            Port = if coukdParsePort then port else 8084
            UrlSite = if (urlSite <>"") then urlSite else "http://localhost:8084"
            EventStoreConnectionString = if (eventStoreConnectionString <>"") then eventStoreConnectionString else "tcp://127.0.0.1:1113"
            Elmah = if couldParseElmah then { logId = elmah; } else { logId = Guid.Empty; }
            EventStoreClientIp = if eventStoreClientIp <> "" then eventStoreClientIp else "127.0.0.1"
            EventStoreClientPort = if couldParseClientPort then eventStoreClientPort else 2113
            EventStoreClientUsername = if eventStoreUsername<>"" then eventStoreUsername else "admin"
            EventStoreClientPassword = if eventStorePassword<>"" then eventStorePassword else "changeit"
    }

    // apply args values to config
    let confPetulant = args |> Seq.fold parseArg initialConfig

    use connection = new SQLiteConnection(confPetulant.DbConnection)
    connection.Open()

    let repo = EventSourceRepo.create connection confPetulant.EventStoreConnectionString confPetulant.EventStoreClientUsername confPetulant.EventStoreClientPassword
    let conn =(repo:>IEventStoreRepository).Connect()

    use logary =
        withLogary' "bear2bear.web" (
            withTargets [
                Console.create Console.empty "console"
                Logary.Targets.ElmahIO.create  confPetulant .Elmah "elmah"

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
            bindings = [ HttpBinding.mk' HTTP confPetulant.IpAddress confPetulant.Port ]
            homeFolder = Some(confPetulant.RootPath)
        }




    //create the subscription to live events
    let httpendPoint = new IPEndPoint(System.Net.IPAddress.Parse(confPetulant.EventStoreClientIp), confPetulant.EventStoreClientPort);
    let wsSubscribe,wsUnsubscribe,nameLiveProjection,liveProjection = PetulantBear.Projections.Live.create (repo:> IEventStoreProjection) connection httpendPoint (confPetulant.EventStoreClientUsername) (confPetulant.EventStoreClientPassword)

    //create the subscription to catchup projections
    let ctx = Projections.CatchUp.create (repo:>IEventStoreProjection)  connection httpendPoint (confPetulant.EventStoreClientUsername) (confPetulant.EventStoreClientPassword)
    [
        (nameLiveProjection,liveProjection)
        (PetulantBear.Projections.Games.name,PetulantBear.Projections.Games.projection)
        (PetulantBear.Projections.Room.name,PetulantBear.Projections.Room.projection)
        (PetulantBear.Projections.Cleaveage.name,PetulantBear.Projections.Cleaveage.projection)
        (PetulantBear.Projections.NotificationGames.name,PetulantBear.Projections.NotificationGames.projection)
        (PetulantBear.Projections.FinishedGame.name,PetulantBear.Projections.FinishedGame.projection)
    ]
    |> List.iter (fun (name,projection) ->
            Projections.CatchUp.createProjectionAsync ctx name |> Async.RunSynchronously
            Projections.CatchUp.startProjection ctx name projection
        )

    (PetulantBear.Application.app confPetulant.RootPath confPetulant.UrlSite connection system repo Users.authenticateWithLogin (wsSubscribe,wsUnsubscribe))
    |> startWebServer config

    //close the eventStore repo and its subscriptions
    (repo:>IEventStoreProjection).Dispose()

    connection.Dispose()
    GC.Collect()

    Console.ReadLine() |> ignore
    0
