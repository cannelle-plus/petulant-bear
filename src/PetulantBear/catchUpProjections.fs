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
    dbConnection : string 
    projectionRepo : IEventStoreProjection }

type HttpStatus =
    | OK
    | NotOK

type CatchupProjection  (ctx:ProjectionsCtx) =

    let projectionsDir = 
        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
        |> sprintf "%s/projections" 

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

    let mkManager() = 
        let manager = new ProjectionsManager(ctx.logger,ctx.ep,ctx.timeout)
        manager
    

    let getStateAsync name =
        let pm = mkManager() 

        pm.GetStateAsync(name,ctx.creds)
        |> Async.AwaitTask
        |> handleHttpResponse ctx.logger

    let createContinuousAsync name =
        let pm = mkManager() 
        let projScripts = 
                sprintf "%s/%s.js" projectionsDir name
                |> System.IO.File.ReadAllText
        pm.CreateContinuousAsync(name, projScripts, ctx.creds)
        |> Async.AwaitIAsyncResult 
        |> Async.Ignore
    
    member this.createProjectionAsync(name) = async {
        let! state = getStateAsync  name
        match state with 
        | NotOK -> 
            use connection = new SQLiteConnection(ctx.dbConnection)
            connection.Open()

            let sql = "delete from Projections where projectionName=@name; insert into Projections (projectionName) VALUES (@name)"
            use sqlCmd = new SQLiteCommand(sql, connection) 

            let add (name:string, value: string) = 
                sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

            add("@name", name)

            do! sqlCmd.ExecuteNonQueryAsync() |> Async.AwaitTask |> Async.Ignore

            sqlCmd.Dispose()
            connection.Dispose()
            GC.Collect()

            do! createContinuousAsync name 
        | OK -> ()

        }
        
    member this.startProjection(name, projection) =
        use connection = new SQLiteConnection(ctx.dbConnection)
        connection.Open()

        let sql = " select lastCheckPoint from Projections where projectionName=@name"
        use sqlCmd = new SQLiteCommand(sql, connection) 

        let add (name:string, value: string) = 
            sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

        add("@name", name)

        use reader = sqlCmd.ExecuteReader() 

        if not <| reader.Read() then 
            sprintf "subscribing to unknown projection %s in db, though it exists in eventstore " name
            |> Logary.LogLine.error 
            |> Logary.Logging.getCurrentLogger().Log

            let sql = "delete from Projections where projectionName=@name;delete from eventsProcessed where projectionName=@name; insert into Projections (projectionName) VALUES (@name)"
            use sqlCmd = new SQLiteCommand(sql, connection) 

            let add (name:string, value: string) = 
                sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

            add("@name", name)

            sqlCmd.ExecuteNonQuery() |> ignore

            sqlCmd.Dispose()
            connection.Dispose()
            GC.Collect()

            projection.resetProjection()

            
            ctx.projectionRepo.SubscribeToStreamFrom name (Nullable<int>()) true  projection             
        else
            match Int32.TryParse(reader.[0].ToString()) with
            | true,lastCheckPoint ->
                sprintf "subscribing to projection %s starting at position %i" name lastCheckPoint
                |> Logary.LogLine.error 
                |> Logary.Logging.getCurrentLogger().Log
                
                sqlCmd.Dispose()
                connection.Dispose()
                GC.Collect()
                ctx.projectionRepo.SubscribeToStreamFrom name  (Nullable(lastCheckPoint)) true projection             
            | false, _ -> 
                sprintf "subscribing to starting projection %s" name 
                |> Logary.LogLine.error 
                |> Logary.Logging.getCurrentLogger().Log
                
                sqlCmd.Dispose() 
                connection.Dispose()
                GC.Collect()

                ctx.projectionRepo.SubscribeToStreamFrom name (Nullable<int>()) true projection             

    
//    member this.reStartAllProjection(projections) =
//        use connection = new SQLiteConnection(ctx.dbConnection)
//        connection.Open()
//
//        let sql = " select projectionName,lastChackPoint from Projections"
//        use sqlCmd = new SQLiteCommand(sql, connection) 
//
//        use reader = sqlCmd.ExecuteReader() 
//
//        while reader.Read() do
//            let name = reader.["projectionName"].ToString()
            
            
            
let create (projectionRepo:IEventStoreProjection ) (dbConnection:string) (httpEndPoint:IPEndPoint) :CatchupProjection= 
    let log = new EventStore.ClientAPI.Common.Log.ConsoleLogger()
    let defaultUserCredentials = new SystemData.UserCredentials("admin","changeit")
    let ctx = { 
        logger  = log
        ep      = httpEndPoint
        timeout = new TimeSpan(0,0,10)
        creds   = defaultUserCredentials
        dbConnection = dbConnection
        projectionRepo = projectionRepo }
    new CatchupProjection (ctx )