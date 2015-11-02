[<AutoOpen>]
module dataContracts

open System
open System.Runtime.Serialization



[<DataContract>]
type ResultResponse = 
  {
  [<field: DataMember(Name = "msg")>]
  msg : string;
  }
