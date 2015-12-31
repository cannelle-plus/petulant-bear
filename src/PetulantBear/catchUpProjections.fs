module PetulantBear.Projections.CatchUp

open System
open System.Net
open System.IO
open System.Reflection
open EventStore.ClientAPI
open EventStore.ClientAPI.Exceptions
open EventStore.ClientAPI.Projections

open System.Data.SQLite

type ProjectionsCtx =
  { logger  : ILogger
    ep      : IPEndPoint
    timeout : TimeSpan
    creds   : SystemData.UserCredentials
    connection : SQLiteConnection 
    projectionRepo : IEventStoreProjection }

type HttpStatus =
    | OK
    | NotOK

let rec handleHttpResponse (logger : ILogger) v =
        async {
            try
                let! status = v
                return match status with
                        | "{}" -> OK
                        | _ -> NotOK
                with e -> return! onHttpError logger v e e
            } 
    and onHttpError logger v orig_ex (ex : exn) =
        async {
            match ex with
            | :? ProjectionCommandFailedException as pcfe when pcfe.Message.Contains "404 (Not Found)" -> return NotOK
            | :? AggregateException as ae -> return! onHttpError logger v orig_ex (ae.InnerException)
            | :? WebException as we when we.Message.Contains "Aborted" -> return! handleHttpResponse logger v
            | e ->
                logger.Error (sprintf "unhandled exception from handle_http_codes:\n%O" e)
                return raise (Exception("unhandled exception from handle_http_codes", e))
        }

let mkManager ctx = 
    let manager = new ProjectionsManager(ctx.logger,ctx.ep,ctx.timeout)
    manager
    

let getStateAsync ctx name =
    let pm = mkManager ctx

    pm.GetStateAsync(name,ctx.creds)
    |> Async.AwaitTask
    |> handleHttpResponse ctx.logger
let projectionsDir = 
        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
        |> sprintf "%s/projections" 

let createContinuousAsync ctx name =
    let pm = mkManager ctx
    let projScripts = 
            sprintf "%s/%s.js" projectionsDir name
            |> System.IO.File.ReadAllText
    pm.CreateContinuousAsync(name, projScripts, ctx.creds)
    |> Async.AwaitIAsyncResult 
    |> Async.Ignore

    
let createProjectionAsync ctx name = async {
    let! state = getStateAsync ctx name
    match state with 
    | NotOK -> 

        let sql = "delete from Projections where projectionName=@name; insert into Projections (projectionName) VALUES (@name)"
        use sqlCmd = new SQLiteCommand(sql, ctx.connection) 

        let add (name:string, value: string) = 
            sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

        add("@name", name)

        do! sqlCmd.ExecuteNonQueryAsync() |> Async.AwaitTask |> Async.Ignore

        sqlCmd.Dispose()
            
            

        do! createContinuousAsync ctx name 
    | OK -> ()

    }
        
let startProjection ctx name projection =
    let sql = " select lastCheckPoint from Projections where projectionName=@name"
    use sqlCmd = new SQLiteCommand(sql, ctx.connection) 

    let add (name:string, value: string) = 
        sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

    add("@name", name)

    use reader = sqlCmd.ExecuteReader() 

    if not <| reader.Read() then 
        sprintf "subscribing to unknown projection %s in db, though it exists in eventstore " name
        |> Logary.LogLine.error 
        |> Logary.Logging.getCurrentLogger().Log

        let sql = "delete from Projections where projectionName=@name;delete from eventsProcessed where projectionName=@name; insert into Projections (projectionName) VALUES (@name)"
        use sqlCmd = new SQLiteCommand(sql, ctx.connection) 

        let add (name:string, value: string) = 
            sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

        add("@name", name)

        sqlCmd.ExecuteNonQuery() |> ignore
        projection.resetProjection ctx.connection
        ctx.projectionRepo.SubscribeToStreamFrom name (Nullable<int>()) true  projection
    else
        match Int32.TryParse(reader.[0].ToString()) with
        | true,lastCheckPoint ->
            sprintf "subscribing to projection %s starting at position %i" name lastCheckPoint
            |> Logary.LogLine.error 
            |> Logary.Logging.getCurrentLogger().Log

            ctx.projectionRepo.SubscribeToStreamFrom name  (Nullable(lastCheckPoint)) true projection
        | false, _ -> 
            sprintf "subscribing to starting projection %s" name 
            |> Logary.LogLine.error 
            |> Logary.Logging.getCurrentLogger().Log
                
            ctx.projectionRepo.SubscribeToStreamFrom name (Nullable<int>()) true projection

            
let create (projectionRepo:IEventStoreProjection ) (connection:SQLiteConnection) (httpEndPoint:IPEndPoint) = 
    let log = new EventStore.ClientAPI.Common.Log.ConsoleLogger()
    let defaultUserCredentials = new SystemData.UserCredentials("admin","changeit")
    let ctx = { 
        logger  = log
        ep      = httpEndPoint
        timeout = new TimeSpan(0,0,10)
        creds   = defaultUserCredentials
        connection = connection
        projectionRepo = projectionRepo }
    ctx