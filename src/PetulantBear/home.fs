module PetulantBear.Home

open System
open Suave // always open suave
open Suave.Logging
open Suave.Types
open Suave.Http
open Suave.Http.Applicatives
open Suave.Http.Files
open Suave.Http.Writers
open Suave.Http.Successful // for OK-result
open Suave.Web // for config
open System.Text
open System.IO
open Suave.State.CookieStateStore

module Navigation =
    let root = "/"
    let home = "/home"
    let welcome = "/welcome"
    let signIn = "/signInHome"
    let signUp = "/signUp"

let authRoutes = 
    [
        path Navigation.root  >>= noCache  >>= browseFileHome "index.html"
    ]

let routes = 
    [
        path Navigation.home  >>= noCache >>= statefulForSession >>= browseFileHome "home.html"
        path Navigation.welcome  >>= noCache >>= browseFileHome "welcome.html"
        path Navigation.signIn  >>= noCache >>= browseFileHome "signin.html"
        path Navigation.signUp  >>= noCache >>= browseFileHome "signin.html"
        
        browseHome >>= noCache //used only for dev purposes
    ]

