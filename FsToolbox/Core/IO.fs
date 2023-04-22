namespace FsToolbox.Core


open System
open System.IO

[<RequireQualifiedAccess>]
module FileIO =
    
    let writeBytes (path: string) (bytes: byte array) =
        try 
            File.WriteAllBytes(path, bytes) |> Ok
        with
        | exn -> Error exn.Message
    
    let readBytes (path: string) =
        try
            File.ReadAllBytes(path) |> Ok
        with
        | exn -> Error exn.Message
                
    let readText (path: string) =
        try
            File.ReadAllText path |> Ok
        with
        | exn -> Error { Message = exn.Message; Exception = Some exn }

    let writeText (path: string, content: string) =
        try
            File.WriteAllText(path, content) |> Ok
        with
        | exn -> Error { Message = exn.Message; Exception = Some exn }

module ConsoleIO =
        
    let cprintfn (color: ConsoleColor) message =
        Console.ForegroundColor <- color
        printfn $"{message}"
        Console.ResetColor()
            
    let printSuccess message = cprintfn ConsoleColor.Green message
    
    let printError message = cprintfn ConsoleColor.Red message
    
    let printWarning message = cprintfn ConsoleColor.DarkYellow message
    
    let printDebug message =
        #if DEBUG
        cprintfn ConsoleColor.DarkMagenta message
        #else
        ()
        #endif

