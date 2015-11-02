open System 

Environment.CurrentDirectory <- __SOURCE_DIRECTORY__

#I @"..\..\packages\EventStore.Client.PreRelease.3.1.0\lib\net45"
#I @"..\..\packages\EventStore.Client.FSharp.4.2.0\lib\net40"

#r "EventStore.ClientAPI.dll"
#r "EventStore.ClientAPI.FSharp.dll"

open EventStore.ClientAPI

let IPAddress = "127.0.0.1"


"tcp://admin:changeit@179.108.3.204:1113"
EventStore.ClientAPI.Conn.connect 
    


    
    