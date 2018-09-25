// Learn more about F# at http://fsharp.org

open System
open FireScratch.Core.FireScratch

[<EntryPoint>]
let main argv =
    printfn "Start Firebase Scratch Server"
    Server(5411)
    Console.ReadKey() |> ignore   
    0 
