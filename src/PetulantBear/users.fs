module PetulantBear.Users

open System
open Suave

open Suave.Cookie
open Suave.Types
open Suave.Http 
open Suave.Http.Successful
open Suave.Http.Applicatives 
open Suave.Http.RequestErrors 
open Suave.Auth
open Suave.State.CookieStateStore
open PetulantBear

open Facebook
open FSharp.Data

module Navigation =
    let bearLogin = "/bearLogin"
    let adminAuth = "/adminAuth"
    let facebookAuth = "/facebookAuth"
    let facebookCallbackAuth = "/facebookCallbackAuth"
    let googleAuth = "/googleAuth"
    let googleCallbackAuth = "/googleCallbackAuth"
    let logout = "/logout"


type oAuthProvider =
    abstract member GetRedirectionAuthUrl : unit -> string
    abstract member AuthenticateWithCode : HttpContext ->  Async<HttpContext option>


type facebookRequest = {
    client_id : string;
    client_secret : string;
    redirect_uri : string;
    code : string;
}
type facebookResponse = {
    id : string;
    name : string;
}

type facebookAccessTokenResponse = JsonProvider<""" {"access_token":"Cm8ZD","expires":5181471} """>
type facebookSocialIdResponse = JsonProvider<""" {"id":"1388221448136137"} """>

type facebookProvider (urlSite:string, absoluteRedirectUri:string, appId:string, appSecret:string) =
    let redirectUri = sprintf "%s%s" urlSite absoluteRedirectUri 
    interface oAuthProvider with 
        member __.GetRedirectionAuthUrl() =
            //facebook stuff
            let redirect  = sprintf "https://www.facebook.com/dialog/oauth?client_id=%s&redirect_uri=%s&scope=email,email,rsvp_event,read_stream,user_likes,user_birthday&response_type=code" appId redirectUri
            redirect
        member __.AuthenticateWithCode(ctx:HttpContext)= 
            context  (fun x ->
                match HttpContext.state x with
                | None ->
                    // restarted server without keeping the key; set key manually?
                    let msg = "Server Key, Cookie Serialiser reset, or Cookie Data Corrupt, "
                                + "if you refresh the browser page, you'll have gotten a new cookie."
                    OK msg
                | Some store ->
                    match ctx.request.queryParam  "code" with
                    | Choice1Of2(code) -> 
                        //facebook stuff 
                        let client = new FacebookClient()
                        
                        let fbRequest = {
                            client_id     = appId;
                            client_secret = appSecret;
                            redirect_uri  = redirectUri;
                            code          =  code ;
                        }

                        //retrieving accessToken and socialId                            
                        let resultAccessToken = facebookAccessTokenResponse.Parse(client.Get("oauth/access_token",fbRequest ).ToString() )
                        client.AccessToken <- resultAccessToken.AccessToken
                        let resultSocialId = facebookSocialIdResponse.Parse(client.Get("me?fields=id").ToString())
                        store.set accessTokenStore resultAccessToken.AccessToken 
                        >>= store.set socialIdStore resultSocialId.Id 
                    | Choice2Of2(failureMsg) -> Http.RequestErrors.BAD_REQUEST "code not found" 
            ) ctx
            
            

type googleProvider(urlSite:string,absoluteRedirectUri:string) =
      interface oAuthProvider with 
        member __.GetRedirectionAuthUrl() =
            //google stuff
            absoluteRedirectUri
        member __.AuthenticateWithCode(x:HttpContext) =
            async {
                return match x.request.queryParam  "state" with
                        | Choice1Of2(code) -> 
                            //google stuff 
                            let accessToken = Guid.NewGuid().ToString()
                            //save accessToken and other values if needed
//                            setSession x accessTokenStore accessToken  |> ignore
                            Some(x)
                        | Choice2Of2(failureMsg) -> None
            }
            
let idClient ="603076340798-2ld1gf9atoefkoo0a3sorl97df9qlgir.apps.googleusercontent.com"
let secretCode = "vMp2aIvxmXoas5D0TO_ONkDM"

let authenticateWithLogin fSuccess : WebPart= 
    authenticate Session false
               (fun () -> Choice2Of2(Redirection.FOUND Home.Navigation.home))
               (fun (f) -> Choice2Of2(Redirection.FOUND Home.Navigation.home))
               fSuccess

let sessionState f =
  context( fun r ->
    match HttpContext.state r with
    | None ->  RequestErrors.BAD_REQUEST "damn it"
    | Some store -> f store )

let oAuth (provider:oAuthProvider) (getBearFromSocialId: string -> BearSession option)=
    statefulForSession
    >>= provider.AuthenticateWithCode 
    >>= context (fun x ->
            match HttpContext.state x with
                | None ->
                    // restarted server without keeping the key; set key manually?
                    let msg = "Server Key, Cookie Serialiser reset, or Cookie Data Corrupt, "
                                + "if you refresh the browser page, you'll have gotten a new cookie."
                    OK msg
                | Some store ->
                    match store.get socialIdStore with 
                    | Some(socialId) ->
                        match getBearFromSocialId (socialId) with
                        | Some(bear) ->
                            store.set  bearStore bear.bearId 
                            >>= store.set  userNameStore bear.username 
                        | None -> OK "no Bear found with this socialId"
                    | None -> Redirection.found PetulantBear.Home.Navigation.home //bad login from facebook what to do here??
        )
    >>= inSession (fun s ->
        match s with
        | NoSession -> Http.RequestErrors.FORBIDDEN "bad login password"
        | NewBear n -> Redirection.found PetulantBear.Home.Navigation.signIn
        | Bear b -> Auth.authenticated Session false >>= Redirection.found PetulantBear.Home.Navigation.root
    )




let socialAuth (provider:oAuthProvider) = Redirection.redirect <|  provider.GetRedirectionAuthUrl()


let userPassAuth (login: string-> string -> PetulantBear.Bears.Contracts.BearDetail option) =
    context  (fun x ->
                match HttpContext.state x with
                | None ->
                    // restarted server without keeping the key; set key manually?
                    let msg = "Server Key, Cookie Serialiser reset, or Cookie Data Corrupt, "
                                + "if you refresh the browser page, you'll have gotten a new cookie."
                    OK msg
                | Some store ->
                    let a = x.request.formData  "username"
                    let b = x.request.formData  "password"
                    match a,b with
                    | Choice1Of2(username),Choice1Of2(password) -> 
                        match login username password with
                        | Some(bear) -> 
                            store.set socialIdStore bear.socialId
                            >>= store.set  bearStore bear.bearId 
                            >>= store.set  userNameStore bear.bearUsername 
                        | None -> Http.RequestErrors.FORBIDDEN "unknown login and/or password"
                    | _,_ -> Http.RequestErrors.BAD_REQUEST "login attempt not correct" 
            ) 
    >>= inSession (fun s ->
        match s with
        | NoSession -> Http.RequestErrors.FORBIDDEN "bad login password"
        | NewBear n -> Http.RequestErrors.FORBIDDEN "Incoherent data, not possible, bearId missing"
        | Bear b -> Auth.authenticated Session false >>= Redirection.found PetulantBear.Home.Navigation.root
    )
    
    

let logout =  
    unsetCookie  Auth.SessionAuthCookie
    >>= unsetCookie Suave.State.CookieStateStore.StateCookie
    >>= OK "logged out <br> <a href='/' > return to home  </a>"


let routes  urlSite getBearFromSocialId bearLogin = 
    let fb = facebookProvider(urlSite, Navigation.facebookCallbackAuth,"1536474526596015","97f0961242c1f94abf45d3eaeb243399")
    let gplus = googleProvider(urlSite, Navigation.googleCallbackAuth)

    [
        path Navigation.bearLogin >>= statefulForSession >>=  userPassAuth bearLogin
        path Navigation.adminAuth >>= Auth.authenticated Session false >>= OK "authed"
        path Navigation.facebookAuth >>= statefulForSession >>= socialAuth fb 
        path Navigation.googleAuth  >>= statefulForSession >>= socialAuth gplus
        path Navigation.facebookCallbackAuth   >>= oAuth fb getBearFromSocialId
        path Navigation.googleCallbackAuth >>= oAuth gplus getBearFromSocialId


        path Navigation.logout >>= logout
    ]
let authRoutes = []




