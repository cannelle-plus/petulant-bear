module PetulantBear.EventSourceRepo
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

open System
open System.Threading
open System.Text
open System.Threading.Tasks

open Newtonsoft.Json

type Enveloppe = {
  version : int
}


open EventStore.ClientAPI
open EventStore.ClientAPI.Common.Log



type Repository(connectionString) =
    
    let connectionSettings =  EventStore.ClientAPI.ConnectionString.GetConnectionSettings(sprintf "ConnectTo=%s" <|connectionString)
    let conn = EventStore.ClientAPI.EventStoreConnection.Create(connectionSettings,new Uri(connectionString),"Bear2BearESConnection")
    

    member this.Connect()=
        conn.ConnectAsync().RunSynchronously()
        
    member this.SaveEvtsAsync<'T>  streamName  (id, v, evts) =  async {
            let typeEvts = (typedefof<'T>).ToString() 
            let enveloppeMetadata = toJson<Enveloppe>({ version= v })

            let events = evts
                            |> List.mapi (fun index e ->
                                        let data=  toJson<'T> e 
                                        let enveloppeMetadata = toJson<Enveloppe>({ version= v })
                                        new EventData(id,typeEvts,true, data,enveloppeMetadata)
                                        )


            let expectedVersion = v
            let n = sprintf "%s - %s" streamName (id.ToString())


            printfn "uncommitted events have been produced according to version %i" expectedVersion
            let! writeResult = conn.AppendToStreamAsync(n,expectedVersion,events) |> Async.AwaitTask

            printfn "events appended to %s, next expected version : %i"  n writeResult.NextExpectedVersion

        }

    member this.hydrateAggAsync<'T>  streamName applyTo initialState  id =

        let rec applyRemainingEvents evts =
            match evts with
            | [] -> initialState
            | head :: tail ->  applyTo (applyRemainingEvents tail) head


        async {

        printfn "reading stream events..."
        let n = sprintf "%s - %s" streamName (id.ToString())
        let! slice = conn.ReadStreamEventsForwardAsync(n,0,99,false) |> Async.AwaitTask

        let evts =
            slice.Events
            |> Seq.map (fun (e:ResolvedEvent) -> fromJson<'T> e.Event.Data) //'
            |> Seq.toList
        printfn "%i events read" evts.Length
        evts |> List.iteri (fun i e-> printfn "applying event %i" i)
        let aggregate = evts |> applyRemainingEvents

        return aggregate
        }



let create cs = new Repository(cs)
