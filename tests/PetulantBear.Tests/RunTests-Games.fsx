//open System
//
////System.IO.Directory.SetCurrentDirectory (__SOURCE_DIRECTORY__)
//Environment.CurrentDirectory <- __SOURCE_DIRECTORY__
//
//#I @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5"
//#I @"..\..\packages\FSharp.Formatting\lib\net40"
//#I @"..\..\packages\Akka\lib\net45"
//#I @"..\..\packages\Akka.FSharp\lib\net45"
//#I @"..\..\packages\FsCheck\lib\net45"
//#I @"..\..\packages\FsPickler\lib\net45"
//#I @"..\..\packages\Fuchu\lib"
//#I @"..\..\packages\Suave\lib\net40"
//#I @"..\..\packages\Suave.Testing\lib\net40"
//#I @"..\..\packages\Akka\lib\net45"
//#I @"..\..\packages\Akka.FSharp\lib\net45"
//#I @"..\..\packages\Newtonsoft.Json\lib\net45"
//#I @"..\..\packages\Unquote\lib\net45"
//#I @"..\..\packages\System.Data.SQLite.Core\lib\net45"
//#I @"..\..\packages\Logary\lib\net40"
//#I @"..\..\packages\EventStore.Client\lib\net40"
//#I @"..\..\packages\Newtonsoft.Json.FSharp\lib\net40"
//#I @"..\..\packages\FSharp.Core\lib\net40"
//#I @"..\..\packages\FSharp.Compiler.Service\lib\net45"
//#I @"..\..\packages\FSharp.Data\lib\net40"
//#I @"..\..\packages\NodaTime\lib\net35-Client"
//#I @"..\..\packages\NodaTime.Serialization.JsonNet\lib\net35-Client"
//
//#r "System.dll"
//#r "Akka.dll"
//#r "CSharpFormat.dll"
//#r "Akka.FSharp.dll"
//#r "FsCheck.dll"
//#r "FsPickler.dll"
//#r "Fuchu.dll"
//#r "Suave.dll"
//#r "Suave.Testing.dll"
//#r "System.net.http"
//#r "System.runtime.serialization"
//#r "Akka"
//#r "Akka.FSharp"
//#r "Newtonsoft.Json"
//#r "Newtonsoft.Json.FSharp.dll"
//#r "Unquote.dll"
//#r "System.Data.SQLite.dll"
//#r "Logary.dll"
//#r "EventStore.ClientAPI.dll"
//#r "FSharp.Core.dll"
//#r "FSharp.Compiler.Service.dll"
//#r "FSharp.Data.dll"
//#r "NodaTime.dll"
//#r "NodaTime.Serialization.JsonNet.dll"
// 
//
//type DeserializingResult<'T> =
//| Serialized of 'T
//| DeserializingException of string*Newtonsoft.Json.JsonException
//
//#load @"..\..\src\PetulantBear\staticTexts.fs"
//#load @"..\..\src\PetulantBear\validator.fs"
//#load @"..\..\src\PetulantBear\dataContracts.fs"
//#load @"..\..\src\PetulantBear\types.fs"
//#load @"..\..\src\PetulantBear\perRequest.fs"
//#load @"..\..\src\PetulantBear\aggregateCoordinator.fs"
//#load @"..\..\src\PetulantBear\games.fs"
//
//
//#load @"TestUtilities.fs"
//#load @"games.domain.tests.fs"
//#load @"games.actor.tests.fs"
//#load @"games.API.tests.fs"
//
//
//open Fuchu
//open PetulantBear.Tests.Games.Domain
//open PetulantBear.Tests.Games.Actor
//open PetulantBear.Tests.Games.API
//
//
//let rootPath = @"..\..\src\"
//
//[
////  games_api(rootPath);
////  gamesActorBasedTests;
//  gamesDomainTests;
//]
////|> List.filter (fun x -> x.Contains("Actor"))
//|> List.map run
////|> ignore
