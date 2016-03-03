module PetulantBear.Tests.EventStoreRepo

open System
open Fuchu
open PetulantBear.EventSourceRepo
open System.Data.SQLite
open EventStore.ClientAPI
open EventStore.ClientAPI.Embedded
open System.Threading
open EventStore.Core
open EventStore.Core.Bus
open EventStore.Core.Messages

[<Tests>]
let eventStoreRepoTests =
    testList "test" [
        testCase "saving Event" <| fun _ ->
            let node = EmbeddedVNodeBuilder.AsSingleNode()
                                   .OnDefaultEndpoints()
                                   .WithExternalTcpOn(new Net.IPEndPoint(Net.IPAddress.Parse("127.0.0.1"),1789))
                                   .WithInternalTcpOn(new Net.IPEndPoint(Net.IPAddress.Parse("127.0.0.1"),1790))
                                   .WithExternalHttpOn(new Net.IPEndPoint(Net.IPAddress.Parse("127.0.0.1"),1790))
                                   .WithInternalHttpOn(new Net.IPEndPoint(Net.IPAddress.Parse("127.0.0.1"),1790))
                                   .RunInMemory()
                                   .RunProjections(ProjectionsMode.All)
                                   .WithWorkerThreads(16)
                                   .Build()
            node.Start()
            use connection = new SQLiteConnection("Data Source=:memory:;Version=3;New=True;")

            let defaultUserCredentials = new SystemData.UserCredentials("admin","changeit")
            //connect to ges
            let settingsBuilder =
              ConnectionSettings.Create()
                        .KeepRetrying()
                        .KeepReconnecting()
                        .SetDefaultUserCredentials(defaultUserCredentials)
            
            let repo = PetulantBear.EventSourceRepo.create connection "tcp://127.0.0.1:1789""admin" "changeit"

            

            let stopped = new AutoResetEvent(false)
            node.MainBus.Subscribe( new AdHocHandler<SystemMessage.BecomeShutdown>(fun m -> stopped.Set() |> ignore))
            node.Stop() |> ignore
            stopped.WaitOne(20000) |> ignore
    ]

