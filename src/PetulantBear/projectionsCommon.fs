module PetulantBear.Projections.Common



open System
open System.Text
open System.Runtime.Serialization
open System.Data.SQLite


open EventStore.ClientAPI  
open Newtonsoft.Json

[<DataContract>]
type JsonResolvedEvent =
    {
        [<field: DataMember(Name = "Case")>]
        case :string
    }

[<DataContract>]
type JsonEvent<'T>=
    {
        [<field: DataMember(Name = "Case")>]
        case :string
        [<field: DataMember(Name = "Fields")>]
        events : 'T []
    }





let deserializeEvt<'T> (e:ResolvedEvent) =
    let json = System.Text.Encoding.UTF8.GetString( e.Event.Data)
    let jsonEvt = JsonConvert.DeserializeObject<JsonEvent<'T>>(json)
    let evt = jsonEvt.events.[0]
    evt
    


let updateProjection name eventId=
    UseConnectionToDB (fun connection ->
        let sql = "Update Projections set lastCheckPoint=IFNULL(lastCheckPoint,-1)+1 where projectionName=@name; Insert into eventsProcessed (projectionName,eventId) VALUES (@name,@eventId)"
        use sqlCmd = new SQLiteCommand(sql, connection) 

        let add (name:string, value: string) = 
            sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

        add("@name", name)
        add("@eventId", eventId.ToString())

        sqlCmd.ExecuteNonQuery() |> ignore
    ) |> ignore

let isEventProcessed name eventId =
    UseConnectionToDB ( fun connection ->
        let sql = " select count(*) from eventsProcessed where projectionName=@name and eventId=@eventId"
        use sqlCmd = new SQLiteCommand(sql, connection) 

        let add (name:string, value: string) = 
            sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

        add("@name", name)
        add("@eventId", eventId.ToString())

        use reader = sqlCmd.ExecuteReader()

        if (reader.Read()) && Int32.Parse(reader.[0].ToString()) >0 then true
        else false
    )
    

   
let withEvent name f escus (resolvedEvent:ResolvedEvent)  =
    let json = System.Text.Encoding.UTF8.GetString( resolvedEvent.Event.Data);
    let jsonEvent = JsonConvert.DeserializeObject<JsonResolvedEvent>(json)

    let jsonMeta = System.Text.Encoding.UTF8.GetString( resolvedEvent.Event.Metadata);
    let meta = JsonConvert.DeserializeObject<Enveloppe>(jsonMeta);

    if not <| isEventProcessed name meta.messageId then
        f meta jsonEvent 
        updateProjection name meta.messageId

let logProjection name  (resolvedEvent:ResolvedEvent) m evt =
    let streamId = resolvedEvent.OriginalEvent.EventStreamId 
    let eventId = resolvedEvent.Event.EventId
    let eventType  = resolvedEvent.Event.EventType
    sprintf "Projection : %s  --> stream : %A, evtAppeared type:%A id:%A metaData = %A , data:%A" name streamId eventType eventId m evt
    |> Logary.LogLine.error 
    |> Logary.Logging.getCurrentLogger().Log
    