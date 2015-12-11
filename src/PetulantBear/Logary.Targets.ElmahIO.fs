module Logary.Targets.ElmahIO


open System
open System.Collections.Generic
open System.Text.RegularExpressions


open Suave.Logging
open FSharp.Actor

open Logary
open Logary.Target
open Logary.Internals
open Logary.Configuration
open Logary.Metrics
open NodaTime

open Elmah
type ErrorLog = Elmah.Io.ErrorLog

/// Configuration for the Elmah.IO target, see
/// https://github.com/elmahio/elmah.io and
/// https://elmah.io/
type ElmahIOConf =
    { logId : Guid }

module internal Impl =

    type State = { log : ErrorLog }
    // http://blog.elmah.io/logging-custom-errors-to-elmah-io/
    let loop (conf : ElmahIOConf) (ri : RuntimeInfo) (inbox : IActor<_>) =
        let rec init () = async {
            let config = new Dictionary<string, string>()
            config.Add( "LogId", conf.logId.ToString())
          
            let log = new Elmah.Io.ErrorLog(config)
          
            return! running { log = log }
            }
        and running state = async {
            let! msg, _ = inbox.Receive()
            match msg with
            | Log l ->
                match l.``exception`` with
                | None -> return! running state
                | Some ex ->
                    
                    let err = Elmah.Error ex
                    err.ApplicationName <- ri.serviceName
                    err.Time <- l.timestamp.ToDateTimeUtc()
                    
                    err.Message <- Map.fold (fun agg key item->
                                        match key with
                                        | "bear" -> sprintf "%s , bear: %A" agg item
                                        | "url" -> sprintf "%s , url: %A" agg item
                                        | _ -> agg
                                    ) l.message l.data
                    
                    let! entryId = Async.FromBeginEnd(err, state.log.BeginLog, state.log.EndLog)
                    // do nothing with entry id, yes tutorial?
                    return! running state
            | Measure msr ->
                // Elmah IO doesn't care
                return! running state
            | Flush ackChan ->
                ackChan.Reply Ack
                return! running state
            | Shutdown ackChan ->
                ackChan.Reply Ack
                return shutdown state
            }
        and shutdown state =
            ()

        init ()

/// Create a new Elmah.IO target
let create conf = TargetUtils.stdNamedTarget (Impl.loop conf)

/// C# interop: Create a new Elmah.IO target
[<CompiledName "Create">]
let create' (conf, name) =
    create conf name

/// Use with LogaryFactory.New( s => s.Target<ElmahIO.Builder>().WithLogId("MY GUID HERE") )
type Builder(conf, callParent : FactoryApi.ParentCallback<Builder>) =
    member x.WithLogId(logId : Guid) =
        ! (callParent <| Builder({ conf with logId = logId }, callParent))

    new(callParent : FactoryApi.ParentCallback<_>) =
        Builder({ logId = Guid.Empty }, callParent)

    interface Logary.Target.FactoryApi.SpecificTargetConf with
        member x.Build name = create conf name

