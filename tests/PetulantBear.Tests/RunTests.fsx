



#I @"..\..\packages\FsCheck\lib\net45"
#I @"..\..\packages\FsPickler\lib\net45"
#I @"..\..\packages\Fuchu\lib"
#I @"..\..\packages\Fuchu-suave\lib\net40"
#I @"..\..\packages\Suave\lib\net40"
#I @"..\..\packages\Suave.Testing\lib\net40"
#I @"..\..\packages\Akka\lib\net45"
#I @"..\..\packages\Akka.FSharp\lib\net45"
#I @"..\..\packages\Newtonsoft.Json\lib\net45"
#I @"..\..\packages\Unquote\lib\net45"


#r "FsCheck.dll"
#r "FsPickler.dll"
#r "Fuchu.dll"
#r "Suave.dll"
#r "Suave.Testing.dll"
#r "System.net.http"
#r "System.runtime.serialization"
#r "Akka"
#r "Akka.FSharp"
#r "Newtonsoft.Json"
#r "Unquote.dll"


#load @"..\..\src\PetulantBear\staticTexts.fs"
#load @"..\..\src\PetulantBear\validator.fs"
#load @"..\..\src\PetulantBear\dataContracts.fs"
#load @"..\..\src\PetulantBear\types.fs"
#load @"..\..\src\PetulantBear\perRequest.fs"
#load @"..\..\src\PetulantBear\aggregateCoordinator.fs"
#load @"..\..\src\PetulantBear\servingFiles.fs"
#load @"..\..\src\PetulantBear\rooms.fs"
#load @"..\..\src\PetulantBear\games.fs"
#load @"..\..\src\PetulantBear\bears.fs"
#load @"..\..\src\PetulantBear\home.fs"
#load @"..\..\src\PetulantBear\monApp.fs"
#load @"..\..\src\PetulantBear\staticResources.fs"


#load @"TestUtilities.fs"
#load @"htmlPages.fs"
#load @"jsScripts.fs"

#load @"bearsAPI.fs"
#load @"games.domain.tests.fs"
#load @"games.actor.tests.fs"
#load @"games.API.tests.fs"

open System

Environment.CurrentDirectory <- __SOURCE_DIRECTORY__

open Fuchu
open PetulantBear.Tests.HtmlPages
open PetulantBear.Tests.bearsAPI



open PetulantBear.Tests.Games.Domain
open PetulantBear.Tests.Games.Actor
open PetulantBear.Tests.Games.API
open PetulantBear.ServingFiles


let rootPath = @"..\..\src\"

[
  getting_the_application_page(rootPath);

//  bears_api(rootPath);
  //games tests
//  games_api(rootPath);
  gamesActorBasedTests;
  gamesDomainTests;
]
//|> List.filter (fun x -> x.Contains("Actor"))
|> List.map run
|> ignore
