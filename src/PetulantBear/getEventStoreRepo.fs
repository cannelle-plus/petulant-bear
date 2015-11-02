module EventSourceRepo
//
//open System
//open EventStore.Core
//open EventStore.Core.Messages
//open EventStore.Core.Bus
//open EventStore.ClientAPI
//open EventStore.ClientAPI.Embedded
//open EventStore.ClientAPI.Common.Log
//
//open EventStore.ClientAPI
//
//let IPAddress = "127.0.0.1"
//
///// Execute f in a context where you can bind to the event store
//let withEmbeddedEs f =
//    
//    let node = EmbeddedVNodeBuilder.AsSingleNode()
//                                   .OnDefaultEndpoints()
//                                   .RunInMemory()
//                                   .RunProjections(ProjectionsMode.All)
//                                   .WithWorkerThreads(16)
//                                   .Build()
//    try
//        printfn "starting embedded EventStore"
//        node.Start()
//        f()
//    finally
//        printfn "stopping embedded EventStore"
//        use stopped = new AutoResetEvent(false)
//        node.MainBus.Subscribe(
//        new AdHocHandler<SystemMessage.BecomeShutdown>(
//        fun m -> stopped.Set() |> ignore))
//        node.Stop() |> ignore
//        if not (stopped.WaitOne(20000)) then
//        Tests.failtest "couldn't stop ES within 20000 ms"
//        else
//        printfn "stopped embedded EventStore"

//let  saveEvents  name id version evt = 
    //let endPoint = new IPEndPoint(IPAddress,1113)
    
//    EventStore.ClientAPI.Conn.connect <| (EventStore.ClientAPI.EventStoreConnection.Create "tcp://admin:changeit@mydomain:1113"):>EventStore.ClientAPI.Connection

    
//    ()
    