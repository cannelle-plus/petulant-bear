module PetulantBear.Tests.Games.Actor
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
//open System.Threading
//open System.Threading.Tasks
//
//let AllesGut cmd =  Success(cmd)
//let fakeSaveToDB (id,version,bearId) cmd=  ()
//
//let buildSaveEventAsync<'T> (timeOut:int) = 
//  let tcs = new TaskCompletionSource<'T>() //'
//
//  let ct = new CancellationTokenSource(timeOut)
//  ct.Token.Register(fun () -> tcs.TrySetCanceled() |> ignore) |> ignore
//
//  let saveEvt streamName (id, v, evts)  = 
//    tcs.SetResult(evts)
//
//  tcs.Task |> Async.AwaitTask, saveEvt
//    
//[<Tests>]
//let gamesActorBasedTests =
//    testList "Async Actor msg based test" [
//        testCase "ScheduleGame -> GameScheduled" <| fun _ ->
//
//            let savingTask,saveEvent = buildSaveEventAsync<Games.Events>(4000)
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
//            let gameScheduled:GameScheduled =  {
//                name= "test";
//                startDate = startDate;
//                location = "playSoccer";
//                players = "julien";
//                nbPlayers = 2;
//                maxPlayers = 8;
//            }
//            let id,version,bearId = Guid.NewGuid(),1,Guid.NewGuid()
//
//            let system = System.create "System" (Configuration.defaultConfig() )
//
//            let gameActor = spawn system "gameAgg" <| gameActor id saveEvent fakeSaveToDB
//            //it is going to produce a dead letter -> this test should be part of an actor to handle the message back
//            gameActor <! (Guid.NewGuid(),1,bearId,Schedule(cmd))
//                
//            let eventProduced = savingTask |> Async.RunSynchronously
//
//            match eventProduced with
//            | Scheduled(evt) -> Assert.Equal("should produce GameScheduled Event",gameScheduled, evt)
//            | _ -> Assert.None("has not produced GameScheduled Event",None)
//
//    ]
