module PetulantBear.Runner

open PetulantBear.Formatter   
open PetulantBear.Parser 

let rec private eval (ctx : Context) = function 
    | Bag list -> 
        match ctx.resolve list with
            | Some item -> item
            | None -> [Parser.reconstructLiteral list]

    | Literals l -> [l]
    | ForLoop (alias, bag, contents) -> 
        [for value in (eval ctx bag) do
            ctx.runtimeAdd (alias, value)
            for elem in contents do
                yield! eval ctx elem
            ctx.runtimeRemove alias]


let run ctx text = 
    Parser.get text 
        |> List.map (eval ctx)
        |> List.reduce List.append
        |> List.reduce (+)