module PetulantBear.Tests.Program

open Fuchu
open System
open Fuchu
open PetulantBear.Tests.HtmlPages
open PetulantBear.Tests.bearsAPI
open PetulantBear.ServingFiles

open PetulantBear.Tests.Games.Domain
open PetulantBear.Tests.Games.Actor
open PetulantBear.Tests.Games.API
open PetulantBear.Tests


let rootPath = @"src\"

[<EntryPoint>]
let main args =
    System.Net.ServicePointManager.Expect100Continue <- false
    let result =
        [
//          getting_the_application_page(rootPath);
//          bears_api(rootPath);
          //games tests
           gamesDomainTests
//           EventStoreRepo.eventStoreRepoTests
//          gamesActorBasedTests;
//          games_api(rootPath);
        ]
        |> List.map run
    //|> Test.filter (fun n -> n.Contains "simple")
    Console.WriteLine("test finished...")
    Console.ReadKey() |> ignore
    0
