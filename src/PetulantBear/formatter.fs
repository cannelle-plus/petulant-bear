﻿module PetulantBear.Formatter 

open PetulantBear.Parser 
open System.Collections.Generic

type Context () =    
    let ctxs = new Dictionary<string, ContextType>()
    let runtime = new Dictionary<string, string>()

    member x.add (key, values) = ctxs.[key] <- List values
    member x.add (key, value)  = ctxs.[key] <- Value value
    member x.add (key, ctx)    = ctxs.[key] <- More ctx

    member x.runtimeAdd (key, value) = runtime.[key] <- value
    member x.runtimeRemove key = runtime.Remove key |> ignore
    
    member x.add (dict:Dictionary<string, string>) = 
        for keys in dict do
            ctxs.[keys.Key] <- Value keys.Value

    member x.resolve list = 
        match list with 
            | [] -> None
            | h::t -> 
                if runtime.ContainsKey h then
                    Some [runtime.[h]]
                else if ctxs.ContainsKey h then
                    ctxs.[h].resolve t
                else 
                    None            

and private ContextType = 
    | Value of string
    | List of string list
    | More of Context
    member x.resolve list = 
        match x with 
            | Value str -> Some [str]
            | List strs -> Some strs
            | More ctx -> ctx.resolve list
