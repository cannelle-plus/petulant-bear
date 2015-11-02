module PetulantBear.ServingFiles

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

type FileType =
    | Html
    | Js
    | Css

let ReadTextAsync filePath =
    async {
        use sourceStream  = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read,  4096, true)
        let sb = new StringBuilder()
        let count = ref 1024
        let buffer = Array.zeroCreate 1024

        while !count>0 do
             let! n = Async.AwaitTask (sourceStream.ReadAsync(buffer, 0, buffer.Length))
             sb.Append(Encoding.UTF8.GetString(buffer, 0, n)) |> ignore
             count := n
        return sb.ToString()
    }

let getPathFromFile rootPath fileType name =
    match fileType with
        | Html -> sprintf @"%s\%swwwroot\%s.html" Environment.CurrentDirectory rootPath name
        | Js -> sprintf @"%s\%swwwroot\scripts\%s.js" Environment.CurrentDirectory rootPath name
        | Css -> sprintf @"%s\%swwwroot\css\%s.css" Environment.CurrentDirectory rootPath name

let serving rootPath fileType name :WebPart = 
    fun (x : HttpContext) -> async {
      let! fileContent = ReadTextAsync <| getPathFromFile rootPath fileType name
      return! OK fileContent x
    }


