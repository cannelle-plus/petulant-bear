module PetulantBear.Tests.Games.API
//
//open Fuchu
//
//open System
//open System.IO
//open System.Net
//open System.Net.Http
//open System.Net.Http.Headers
//open System.Text
//
//
//open Akka.FSharp
//open Akka.Configuration
//
//open Suave // always open suave
//open Suave.Logging
//open Suave.State.CookieStateStore
//open Suave.Http
//
//open Suave.Cookie
//open Suave.Http.Files
//open Suave.Http.Writers
//open Suave.Http.Successful // for OK-result
//open Suave.Http.Applicatives
//open Suave.Http.RequestErrors
//open Suave.Web // for config
//open Suave.Types
//
//open PetulantBear.Tests.TestUtilities
//open Suave.Testing
//open PetulantBear.Games.Contracts
//
//open System.Runtime.Serialization
//open System.Collections.Generic
//
//let runWithConfig = runWith {  defaultConfig with  bindings = [HttpBinding.mk' HTTP "127.0.0.1" 8090]  }
//
//type Assert with
//  static member Null (msg : string, o : obj) =
//    if o <> null then Tests.failtest msg
//    else ()
//
//  static member Contains (msg : string, f_expected : 'a -> bool, xs : seq<'a>) =
//    if Seq.isEmpty xs then Tests.failtest "empty seq"
//    match Seq.tryFind f_expected xs with
//    | None -> Tests.failtest msg
//    | Some v ->
//      // printfn "found %A" v
//      ()
//
//let reqResp
//  (methd : HttpMethod)
//  (resource : string)
//  ( msg : option<_>)
//  (cookies : CookieContainer option)
//  (f_request : HttpRequestMessage -> HttpRequestMessage)
//  f_result
//  ctx =
//
//  let log = Suave.Log.info ctx.suaveConfig.logger "Suave.Tests" TraceHeader.empty
//  log (sprintf "%A %s" methd resource)
//
//  let default_timeout = TimeSpan.FromSeconds 5.
//
//  let reqContent = 
//    match msg with
//    | Some(m) -> Some <| new ByteArrayContent(toJson m)
//    | None -> None
//
//  use handler = mkHandler DecompressionMethods.None cookies
//  use client = mkClient handler
//  use request = mkRequest methd resource "" reqContent (endpointUri ctx.suaveConfig) |> f_request
//
//  for h in request.Headers do
//    log (sprintf "%s: %s" h.Key (String.Join(", ", h.Value)))
//
//  // use -> let!!!
//  let result = request |> send client default_timeout ctx
//  f_result result
//
//let setConnectionKeepAlive (r : HttpRequestMessage) =
//  r.Headers.ConnectionClose <- Nullable(false)
//  r
//
///// Test a request by looking at the cookies alone.
//let req_cookies cookies ctx methd resource msg f_req =
//  reqResp methd resource msg (Some cookies)
//           setConnectionKeepAlive
//           f_req
//           ctx
//
//let cookies suaveConfig (container : CookieContainer) =
//  container.GetCookies(endpointUri suaveConfig)
//
//let interaction ctx f_ctx = withContext f_ctx ctx
//
//let interactWithContainer methd resource  msg container ctx =
//  let response = req_cookies container ctx methd resource msg id
//  match response.Headers.TryGetValues("Set-Cookie") with
//  | false, _ -> ()
//  | true, values -> values |> Seq.iter (fun cookie -> container.SetCookies(endpointUri ctx.suaveConfig, cookie))
//  response
//
//let sessionState f =
//  context( fun r ->
//    match HttpContext.state r with
//    | None ->  RequestErrors.BAD_REQUEST "damn it"
//    | Some store -> f store )
//
//let gameId =Guid.NewGuid()
//let bearId = Guid.NewGuid()
//
//let createCommand i v c = { id = i; version = v; payLoad = c; }:Command<'T>
//let createRequest i v  = { id = i; version = v; } : Request
//
//let scheduleCmd:Command<ScheduleGame> =  
//    createCommand gameId 1 {
//        name= "test";
//        startDate = DateTime.Now.AddDays(float 52);
//        location = "playSoccer";
//        players = "julien";
//        nbPlayers = 2;
//        maxPlayers = 8;
//    }
//  
//
//let cancelCmd =  createRequest gameId 1 
//let abandonCmd  =  createRequest gameId 1 
//let markBearCmd  = createCommand gameId 1 12
//let commentBearCmd = createCommand gameId 1 "some comment"
//
//let allesGut cmd =  Success(cmd)
//let queryList filter = Some(new List<PetulantBear.Games.Contracts.Game>())
//let queryDetail filter  = 
//    let result: PetulantBear.Games.Contracts.GameDetail = {
//        id = gameId;
//        name= "test";
//        ownerId=bearId;
//        ownerUserName = "jason";
//        startDate = DateTime.Now;
//        location = "playSoccer";
//        players = new System.Collections.Generic.List<BearPlayer>();
//        nbPlayers = 0;
//        maxPlayers = 8;
//    } 
//    Some(result)
//
//let events = new List<Event<PetulantBear.Games.Events>>()
//let saveEvents name (id ,version, evt) =
//    events.Add({ id= id; version=version;payLoad= evt })
//let fakeSaveToDB (id,version,bearId) cmd=  ()
//
//
//[<Tests>]
//let games_api rootPath =
//    let runWithConfig = runWith defaultConfig
//
//    testList "games command" [
//        testCase "schedule new game requires authentication" <| fun _ ->
//            
//            let system = System.create "System" akkaConfig
//            let ctx = choose (PetulantBear.Games.authRoutes system saveEvents queryList queryDetail fakeSaveToDB) |> runWithConfig 
//
//            // mutability bonanza here:
//            let container = CookieContainer()
//            let interact methd resource msg = interactWithContainer methd resource msg container ctx
//            let cookies = cookies ctx.suaveConfig container
//
//            // when
//            interaction ctx <| fun _ -> 
//                use res = interact HttpMethod.POST "/api/games/schedule" (Some(scheduleCmd))
//                Assert.Equal("returns correct json representation", "{\"msg\":\"OK\"}", contentString res)
//                Assert.Equal("code 200 OK", HttpStatusCode.OK, statusCode res)
//
//        testCase "cancel scheduled game" <| fun _ ->
//
//            let system = System.create "System" akkaConfig
//
//            let ctx = choose (PetulantBear.Games.authRoutes system saveEvents queryList queryDetail fakeSaveToDB) |> runWithConfig 
//
//            // mutability bonanza here:
//            let container = CookieContainer()
//            let interact methd resource msg = interactWithContainer methd resource msg container ctx
//            let cookies = cookies ctx.suaveConfig container
//
//            // when
//            interaction ctx <| fun _ -> 
//                use res = interact HttpMethod.POST "/api/games/cancel" (Some(cancelCmd))
//                Assert.Equal("returns correct json representation", "{\"msg\":\"OK\"}", contentString res)
//                Assert.Equal("code 200 OK", HttpStatusCode.OK, statusCode res)
//
//        testCase "abandon scheduled game" <| fun _ ->
//
//            let system = System.create "System" akkaConfig
//
//            let ctx = choose (PetulantBear.Games.authRoutes system saveEvents queryList queryDetail fakeSaveToDB) |> runWithConfig 
//
//            // mutability bonanza here:
//            let container = CookieContainer()
//            let interact methd resource msg = interactWithContainer methd resource msg container ctx
//            let cookies = cookies ctx.suaveConfig container
//
//            // when
//            interaction ctx <| fun _ -> 
//                use res = interact HttpMethod.POST "/api/games/abandon" (Some(abandonCmd))
//                Assert.Equal("returns correct json representation", "{\"msg\":\"OK\"}", contentString res)
//                Assert.Equal("code 200 OK", HttpStatusCode.OK, statusCode res)
//        
//        testCase "mark bear in game" <| fun _ ->
//
//            let system = System.create "System" akkaConfig
//
//            let ctx = choose (PetulantBear.Games.authRoutes system saveEvents queryList queryDetail fakeSaveToDB) |> runWithConfig 
//
//            // mutability bonanza here:
//            let container = CookieContainer()
//            let interact methd resource msg = interactWithContainer methd resource msg container ctx
//            let cookies = cookies ctx.suaveConfig container
//
//            // when
//            interaction ctx <| fun _ -> 
//                use res = interact HttpMethod.POST "/api/games/markBear" (Some(markBearCmd))
//                Assert.Equal("returns correct json representation", "{\"msg\":\"OK\"}", contentString res)
//                Assert.Equal("code 200 OK", HttpStatusCode.OK, statusCode res)                
//
//        testCase "comment bear in game" <| fun _ ->
//
//            let system = System.create "System" akkaConfig
//
//            let ctx = choose (PetulantBear.Games.authRoutes system saveEvents queryList queryDetail fakeSaveToDB) |> runWithConfig 
//
//            // mutability bonanza here:
//            let container = CookieContainer()
//            let interact methd resource msg = interactWithContainer methd resource msg container ctx
//            let cookies = cookies ctx.suaveConfig container
//
//            // when
//            interaction ctx <| fun _ -> 
//                use res = interact HttpMethod.POST "/api/games/commentBear" (Some(commentBearCmd))
//                Assert.Equal("returns correct json representation", "{\"msg\":\"OK\"}", contentString res)
//                Assert.Equal("code 200 OK", HttpStatusCode.OK, statusCode res)                
//    ]
//
//
//
