#r @"packages\Suave\lib\net40\Suave.dll"
#r @"packages\FsPickler.1.5.2\lib\net45\FsPickler.dll"

open Suave                 // always open suave
open Suave.Http.Successful // for OK-result
open Suave.Web            // for config
open Suave.Http
open Suave.State.CookieStateStore
open Suave.Http.Applicatives
open Suave.Types

[
    path "/home" >>= statefulForSession >>=  OK "home <a href='/secondPage'>second page</a>"
    path "/secondPage" >>= statefulForSession >>= context  (fun x ->
      match HttpContext.state x with
      | None ->
        // restarted server without keeping the key; set key manually?
        let msg = "Server Key, Cookie Serialiser reset, or Cookie Data Corrupt, "
                  + "if you refresh the browser page, you'll have gotten a new cookie."
        OK msg
      | Some store ->
        match store.get "counter" with
        | Some y ->
          store.set "counter" (y + 1)
          >>= OK (sprintf "Hello %d time(s) <a href=thirdPage>third</a> " (y + 1) )
        | None ->
          store.set "counter" 1
          >>= OK "First time <a href=thirdPage>third</a>"
      )
    path "/thirdPage" >>= statefulForSession >>= context  (fun x ->
        match HttpContext.state x with
        | None ->
            // restarted server without keeping the key; set key manually?
            let msg = "Server Key, Cookie Serialiser reset, or Cookie Data Corrupt, "
                        + "if you refresh the browser page, you'll have gotten a new cookie."
            OK msg
        | Some store ->
            match store.get "counter2" with
            | Some y ->
                store.set "counter2" (y + 1)
                >>= OK (sprintf "bye %d time(s) <a href=secondPage>secondPage</a>" (y + 1) )
            | None ->
                store.set "counter2" 1
                >>= OK "First time  <a href=secondPage>secondPage</a>"
        )
    ]
|> choose
|> startWebServer defaultConfig
