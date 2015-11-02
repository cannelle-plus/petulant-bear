module PetulantBear.Tests.HtmlPages


open Fuchu

open System
open System.IO
open System.Net.Http
open System.Net.Http.Headers
open System.Text
open System.Runtime.Serialization

open Suave
open Suave.Types
open Suave.Http


open PetulantBear.Tests.TestUtilities
open Suave.Testing

open PetulantBear.ServingFiles



[<Tests>]
let getting_the_application_page rootPath  =
  

  let readFile fileType name =
    getPathFromFile rootPath fileType name
    |> System.IO.File.ReadAllText

  testList "get the basic pages" [
    testCase "index page" <| fun _ ->
      
      Assert.Equal("/ returns index page as defined in the wwwroot", (readFile Html "index"), 
        runWithConfig PetulantBear.Home.authRoutes |> req HttpMethod.GET PetulantBear.Home.Navigation.root None ) ]

    



