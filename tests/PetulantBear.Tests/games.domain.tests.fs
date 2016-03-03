module PetulantBear.Tests.Games.Domain

open Fuchu
open PetulantBear
open PetulantBear.Games

open System
open System.IO
open PetulantBear.Games.Contracts
open Akka
open Akka.Actor
open Akka.FSharp

open Swensen.Unquote

open System.Threading
open System.Threading.Tasks


let isEvent expected = function
    | Choice1Of2 evts -> evts= expected
    | _ -> false

let mapResultToOptionFailure = function 
    | Choice2Of2 x -> Some x 
    | _ -> None


[<Tests>]
let gamesDomainTests =
    testList "Commands based test" [
        testCase "ScheduleGame -> GameScheduled -> GameJoined -> PlayerAddedToTheLineUp" <| fun _ ->
            
            let bear = {
                bearId=Guid.NewGuid()
                socialId="testSocialId"
                username = "toto"
            }
            
            let startDate = DateTime.Now.AddHours(float 25)
            let cmd =  Schedule({
                name= "test";
                startDate = startDate;
                location = "playSoccer";
                players = "julien";
                nbPlayers = 0;
                maxPlayers = 8;
            })

            let expectedEvents = 
                [
                    Scheduled({
                        name= "test";
                        startDate = startDate;
                        location = "playSoccer";
                        players = "julien";
                        nbPlayers = 0;
                        maxPlayers = 8;
                    })
                    Joined({bearId=bear.bearId}) 
                    PlayerAddedToTheLineUp({bearId=bear.bearId})
                ]   

            test <@ exec State.Initial bear <| cmd
                    |> isEvent expectedEvents @>

        testCase "When Scheduled then JoinGame -> GameJoined" <| fun _ ->
            
            let bear = {
                bearId=Guid.NewGuid()
                socialId="testSocialId"
                username = "toto"
            }
            
            let startDate = DateTime.Now.AddHours(float 25)
            let someId = Guid.NewGuid();
                
            let pastEvents = 
                [
                    Scheduled({
                        name= "test";
                        startDate = startDate;
                        location = "playSoccer";
                        players = "julien";
                        nbPlayers = 0;
                        maxPlayers = 8;
                    })
                    Joined({bearId=someId}) 
                    PlayerAddedToTheLineUp({bearId=someId})
                ]

            let cmd = Commands.Join(Join())

            let expectedEvents = 
                [
                    Joined({bearId=bear.bearId})
                    PlayerAddedToTheLineUp({bearId=bear.bearId})
                ]  
                
            let state = List.fold applyEvts State.Initial pastEvents 

            test <@ exec state bear <| cmd
                    |> isEvent expectedEvents @>

        testCase "When Scheduled then AbandonGame -> GameAbandonned" <| fun _ ->
            
            let bear = {
                bearId=Guid.NewGuid()
                socialId="testSocialId"
                username = "toto"
            }
            
            let startDate = DateTime.Now.AddHours(float 25)
            let someId = Guid.NewGuid();

            let pastEvents = 
                [
                    Scheduled({
                        name= "test";
                        startDate = startDate;
                        location = "playSoccer";
                        players = "julien";
                        nbPlayers = 0;
                        maxPlayers = 8;
                    })
                    Joined({bearId=bear.bearId})
                    PlayerAddedToTheLineUp({bearId=bear.bearId})
                ]

            let cmd = Commands.Abandon(Abandon())

            let expectedEvents = 
                [
                    Abandonned({bearId=bear.bearId})
                    PlayerRemovedFromTheLineUp({bearId=bear.bearId})
                ]  
                
            let state = List.fold applyEvts State.Initial pastEvents 

            test <@ exec state bear <| cmd
                    |> isEvent expectedEvents @>

        testCase "When Scheduled,Joined until full then JoinGame -> GameJoined,PlayerAddedToTheBench" <| fun _ ->
            
            let bear = {
                bearId=Guid.NewGuid()
                socialId="testSocialId"
                username = "toto"
            }
            
            let startDate = DateTime.Now.AddHours(float 25)
            let bear1 = Guid.NewGuid()
            let bear2 = Guid.NewGuid()
            let bear3 = Guid.NewGuid()

            let pastEvents = 
                [
                    Scheduled({
                        name= "test";
                        startDate = startDate;
                        location = "playSoccer";
                        players = "julien";
                        nbPlayers = 0;
                        maxPlayers = 3;
                    })
                    Joined({bearId=bear1})
                    PlayerAddedToTheLineUp({bearId=bear1})
                    Joined({bearId=bear2})
                    PlayerAddedToTheLineUp({bearId=bear2})
                    Joined({bearId=bear3})
                    PlayerAddedToTheLineUp({bearId=bear3})
                ]


            let cmd = Commands.Join(Join())

            let expectedEvents = 
                [
                    Joined({bearId=bear.bearId})
                    PlayerAddedToTheBench({bearId= bear.bearId})
                ]  
                
            let state = List.fold applyEvts State.Initial pastEvents 

            test <@ exec state bear <| cmd
                    |> isEvent expectedEvents @>

        testCase "When Scheduled,Joined until full, joined, PlayerAddedToTheBench then AbandonGame -> GameAbandonned,PlayerRemovedFromTheBench" <| fun _ ->
            
            let bear = {
                bearId=Guid.NewGuid()
                socialId="testSocialId"
                username = "toto"
            }
            
            let startDate = DateTime.Now.AddHours(float 25)

            let bearIdAddedTotheBench = Guid.NewGuid()
            let bear1 = Guid.NewGuid()
            let bear2 = Guid.NewGuid()
            let bear3 = Guid.NewGuid()

            let pastEvents = 
                [
                    Scheduled({
                        name= "test";
                        startDate = startDate;
                        location = "playSoccer";
                        players = "julien";
                        nbPlayers = 0;
                        maxPlayers = 3;
                    })
                    Joined({bearId=bear1})
                    PlayerAddedToTheLineUp({bearId=bear1})
                    Joined({bearId=bear.bearId}) //->  future leaving bear
                    PlayerAddedToTheLineUp({bearId=bear.bearId})
                    Joined({bearId=bear2})
                    PlayerAddedToTheLineUp({bearId=bear2})
                    Joined({bearId=bearIdAddedTotheBench})
                    PlayerAddedToTheBench({bearId=bearIdAddedTotheBench})
                ]

            let cmd = Commands.Abandon(Abandon())

            let expectedEvents = 
                [
                    Abandonned({bearId=bear.bearId})
                    PlayerRemovedFromTheLineUp({bearId= bear.bearId})
                    PlayerAddedToTheLineUp({bearId=bearIdAddedTotheBench})
                    PlayerRemovedFromTheBench({bearId= bearIdAddedTotheBench; isRemovedFromTheGame=false})
                ]  
                
            let state = List.fold applyEvts State.Initial pastEvents 

            test <@ exec state bear <| cmd
                    |> isEvent expectedEvents @>

        testCase "When Scheduled,Joined until full,  and the same Join then AbandonGame -> GameAbandonned" <| fun _ ->
            
            let bear = {
                bearId=Guid.NewGuid()
                socialId="testSocialId"
                username = "toto"
            }
            
            let startDate = DateTime.Now.AddHours(float 25)
            let bear1 = Guid.NewGuid()
            let bear2 = Guid.NewGuid()
            let bear3 = Guid.NewGuid()
            let bearIdAddedTotheBench = Guid.NewGuid()

            let pastEvents = 
                [
                    Scheduled({
                        name= "test";
                        startDate = startDate;
                        location = "playSoccer";
                        players = "julien";
                        nbPlayers = 0;
                        maxPlayers = 2;
                    })
                    Joined({bearId=bear1})
                    PlayerAddedToTheLineUp({bearId=bear1})
                    Joined({bearId=bear2})
                    PlayerAddedToTheLineUp({bearId=bear2})
                    Joined({bearId = bear.bearId})
                    PlayerAddedToTheBench({bearId = bear.bearId})
                ]

            let cmd = Commands.Abandon(Abandon())

            let expectedEvents = 
                [
                    Abandonned({bearId=bear.bearId})
                    PlayerRemovedFromTheBench({bearId=bear.bearId; isRemovedFromTheGame=true})
                ]  
                
            let state = List.fold applyEvts State.Initial pastEvents 

            test <@ exec state bear <| cmd
                    |> isEvent expectedEvents @>
        
        testCase "When Scheduled,Joined until almost full then Join -> GameJoined" <| fun _ ->
            
            let bear = {
                bearId=Guid.NewGuid()
                socialId="testSocialId"
                username = "toto"
            }
            
            let startDate = DateTime.Now.AddHours(float 25)
            let bear1 = Guid.NewGuid()
            let bear2 = Guid.NewGuid()
            let bearIdAddedTotheBench = Guid.NewGuid()

            let pastEvents = 
                [
                    Scheduled({
                        name= "test";
                        startDate = startDate;
                        location = "playSoccer";
                        players = "julien";
                        nbPlayers = 0;
                        maxPlayers = 3;
                    })
                    Joined({bearId=bear1})
                    PlayerAddedToTheLineUp({bearId=bear1})
                    Joined({bearId=bear2})
                    PlayerAddedToTheLineUp({bearId=bear2})
                ]

            let cmd = Commands.Join(Join())

            let expectedEvents = 
                [
                    Joined({bearId=bear.bearId})
                    PlayerAddedToTheLineUp({bearId=bear.bearId})
                ]  
                
            let state = List.fold applyEvts State.Initial pastEvents 

            test <@ exec state bear <| cmd
                    |> isEvent expectedEvents @>
        
    ]