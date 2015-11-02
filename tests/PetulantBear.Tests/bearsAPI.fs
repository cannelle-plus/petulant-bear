module PetulantBear.Tests.bearsAPI


open Fuchu

open System
open System.IO
open System.Net
open System.Net.Http
open System.Net.Http.Headers
open System.Text


open Suave // always open suave
open Suave.Logging
open Suave.State.CookieStateStore
open Suave.Http

open Suave.Cookie
open Suave.Http.Files
open Suave.Http.Writers
open Suave.Http.Successful // for OK-result
open Suave.Http.Applicatives
open Suave.Http.RequestErrors
open Suave.Web // for config
open Suave.Types

open PetulantBear.Tests.TestUtilities
open Suave.Testing
open PetulantBear.Bears.Contracts

open System.Runtime.Serialization

let runWithConfig = runWith { defaultConfig with logger = Loggers.saneDefaultsFor LogLevel.Warn }

type Assert with
  static member Null (msg : string, o : obj) =
    if o <> null then Tests.failtest msg
    else ()

  static member Contains (msg : string, f_expected : 'a -> bool, xs : seq<'a>) =
    if Seq.isEmpty xs then Tests.failtest "empty seq"
    match Seq.tryFind f_expected xs with
    | None -> Tests.failtest msg
    | Some v ->
      // printfn "found %A" v
      ()

let reqResp
  (methd : HttpMethod)
  (resource : string)
  ( msg : option<_>)
  (cookies : CookieContainer option)
  (f_request : HttpRequestMessage -> HttpRequestMessage)
  f_result
  ctx =

  let log = Suave.Log.info ctx.suaveConfig.logger "Suave.Tests" TraceHeader.empty
  log (sprintf "%A %s" methd resource)

  let default_timeout = TimeSpan.FromSeconds 5.

  let reqContent = 
    match msg with
    | Some(m) -> Some <| new ByteArrayContent(toJson m)
    | None -> None

  use handler = mkHandler DecompressionMethods.None cookies
  use client = mkClient handler
  use request = mkRequest methd resource "" reqContent (endpointUri ctx.suaveConfig) |> f_request

  for h in request.Headers do
    log (sprintf "%s: %s" h.Key (String.Join(", ", h.Value)))

  // use -> let!!!
  let result = request |> send client default_timeout ctx
  f_result result

let setConnectionKeepAlive (r : HttpRequestMessage) =
  r.Headers.ConnectionClose <- Nullable(false)
  r

/// Test a request by looking at the cookies alone.
let req_cookies cookies ctx methd resource msg f_req =
  reqResp methd resource msg (Some cookies)
           setConnectionKeepAlive
           f_req
           ctx

let cookies suaveConfig (container : CookieContainer) =
  container.GetCookies(endpointUri suaveConfig)

let interaction ctx f_ctx = withContext f_ctx ctx

let interactWithContainer methd resource  msg container ctx =
  let response = req_cookies container ctx methd resource msg id
  match response.Headers.TryGetValues("Set-Cookie") with
  | false, _ -> ()
  | true, values -> values |> Seq.iter (fun cookie -> container.SetCookies(endpointUri ctx.suaveConfig, cookie))
  response

let sessionState f =
  context( fun r ->
    match HttpContext.state r with
    | None ->  RequestErrors.BAD_REQUEST "damn it"
    | Some store -> f store )

let bearId =Guid.NewGuid()
let socialId = "facebookId"
let avatarId = 5
let userName = "jason"


let signInCmd:SignIn =  {
    bearUsername = userName;
    bearAvatarId = avatarId;
}

let fakeSave (id,version,bearId) (socialid,cmd ) = 
    Success(socialId, cmd)

//[<Tests>]
//let bears_api rootPath =
//    let runWithConfig = runWith defaultConfig
//
//    testList "sign in bears" [
//        testCase "sign in new bear does not requires authentication" <| fun _ ->
//
//            let ctx =
//                [ path "/adminAuth" >>= Auth.authenticated Session false >>= OK "authed" ]
//                |> List.append (PetulantBear.Bears.routes fakeSave)
//                |> choose
//                |> runWithConfig 
//
//            // mutability bonanza here:
//            let container = CookieContainer()
//            let interact methd resource msg = interactWithContainer methd resource msg container ctx
//            let cookies = cookies ctx.suaveConfig container
//
//            // when
//            interaction ctx <| fun _ -> 
//                use res = interact HttpMethod.POST "/api/bears/signin" (Some(signInCmd))
//
//                Assert.Equal("returns correct json representation", "{\"msg\":\"OK\"}", contentString res)
//                Assert.Equal("code 200 OK", HttpStatusCode.OK, statusCode res)
//    ]



