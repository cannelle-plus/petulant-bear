module PetulantBear.Tests.Games.Domain
//
//open Fuchu
//open PetulantBear
//open PetulantBear.Games
//
//open System
//open System.IO
//open PetulantBear.Games.Contracts
//
//open Akka
//open Akka.Actor
//open Akka.FSharp
//
//open Swensen.Unquote
//
//open System.Threading
//open System.Threading.Tasks
//
//let mapResultToOptionEvent = function Choice1Of2 x -> Some x | _ -> None
//let mapResultToOptionFailure = function Choice2Of2 x -> Some x | _ -> None
//
//[<Tests>]
//let gamesDomainTests =
//    testList "Commands based test" [
//        testCase "ScheduleGame -> GameScheduled" <| fun _ ->
//            
//            let ownerId = Guid.NewGuid()
//            let startDate = DateTime.Now.AddHours(float 25)
//            let cmd:ScheduleGame =  {
//                name= "test";
//                startDate = startDate;
//                location = "playSoccer";
//                players = "julien";
//                nbPlayers = 2;
//                maxPlayers = 8;
//            }
//
//            let gameScheduled:GameScheduled =  {
//                name= "test";
//                startDate = startDate;
//                location = "playSoccer";
//                players = "julien";
//                nbPlayers = 2;
//                maxPlayers = 8;
//            }
//
//
//            test <@ exec State.Initial <| Schedule(cmd)
//                    |> mapResultToOptionEvent
//                    |> Option.map (function Scheduled x -> Some(x) | _ -> None)
//                    |> Option.exists ((=) (Some(gameScheduled))) @>
//        
//        testCase "ScheduleGame -> GameScheduled twisted" <| fun _ ->
//            
//            let ownerId = Guid.NewGuid()
//            let startDate = DateTime.Now.AddHours(float 25)
//            let cmd:ScheduleGame =  {
//                name= "test";
//                startDate = startDate;
//                location = "playSoccer";
//                players = "julien";
//                nbPlayers = 2;
//                maxPlayers = 8;
//            }
//
//            let gameScheduled:GameScheduled =  {
//                name= "test";
//                startDate = startDate;
//                location = "playSoccers";
//                players = "julien";
//                nbPlayers = 2;
//                maxPlayers = 8;
//            }
//
//
//            test <@ exec State.Initial <| Schedule(cmd)
//                    |> mapResultToOptionEvent
//                    |> Option.map (function Scheduled x -> Some(x) | _ -> None)
//                    |> Option.exists ((<>) (Some(gameScheduled))) @>
//        
//        testCase "scheduling a game before 24 hours in not allowed" <| fun _ ->
//            
//            let ownerId = Guid.NewGuid()
//            let startDate = DateTime.Now.AddDays(float -1.0)
//            let cmd:ScheduleGame =  {
//                name= "test";
//                startDate = startDate;
//                location = "playSoccer";
//                players = "julien";
//                nbPlayers = 2;
//                maxPlayers = 8;
//            }
//
//            test <@ exec State.Initial <| Schedule(cmd)
//                    |> mapResultToOptionFailure
//                    |> Option.exists ((=) [gamesText.gamesIn24Hours]) @>
//
//        testCase "scheduling a game already scheduled is not allowed" <| fun _ ->
//            
//            let ownerId = Guid.NewGuid()
//            let startDate = DateTime.Now.AddDays(float 25)
//            let cmd:ScheduleGame =  {
//                name= "test";
//                startDate = startDate;
//                location = "playSoccer";
//                players = "julien";
//                nbPlayers = 2;
//                maxPlayers = 8;
//            }
//
//            test <@ exec { State.Initial with isScheduled=true} <| Schedule(cmd)
//                    |> mapResultToOptionFailure
//                    |> Option.exists ((=) [gamesText.gameAlreadyScheduled]) @>
//
//            
//    ]
//
