namespace FsToolbox.Extensions

[<AutoOpen>]
module Strings =
    
    open System
    open System.Text
    open FsToolbox.Core
    
    type String with
    
        static member FromUtfBytes(bytes: byte array) = Conversions.toUtf8 bytes
        
        member str.ToSnakeCase() =            
            str
            |> List.ofSeq
            |> List.fold (fun (acc, i) c ->
                let newAcc =
                    match Char.IsUpper c, i = 0 with
                    | false, _ -> acc @ [ c ]
                    | true, true -> acc @ [ Char.ToLower(c) ]
                    | true, false -> acc @ [ '_'; Char.ToLower(c) ]
                (newAcc, i + 1)) ([], 0)
            |> (fun (chars, _) -> String(chars |> Array.ofList))
        
        member str.ToPascalCase() =
            let isMatch c = [ '_'; '-'; ' ' ] |> List.contains c
        
            str
            |> List.ofSeq
            |> List.fold (fun (acc, i) c ->
                let newAcc =
                    //match c =
                    match i - 1 >= 0 && isMatch str.[i - 1], i = 0, isMatch c with
                    | true, _, false -> acc @ [ Char.ToUpper c ]
                    | true, _, true -> acc
                    | false, false, false -> acc @ [ c ]
                    | false, true, _ -> acc @ [ Char.ToUpper c ]
                    | false, false, true -> acc
                        
                    //match Char.IsUpper c, i = 0 with
                    //| false, _ -> acc @ [ c ]
                    //| true, true -> acc @ [ Char.ToLower(c) ]
                    //| true, false -> acc @ [ '_'; Char.ToLower(c) ]
                (newAcc, i + 1)) ([], 0)
            |> (fun (chars, _) -> String(chars |> Array.ofList))
        
        member str.ToCamelCase() =
            let isMatch c = [ '_'; '-'; ' ' ] |> List.contains c
        
            str
            |> List.ofSeq
            |> List.fold (fun (acc, i) c ->
                let newAcc =
                    //match c =
                    match i - 1 >= 0 && isMatch str.[i - 1], i = 0, isMatch c with
                    | true, _, false -> acc @ [ Char.ToUpper c ]
                    | true, _, true -> acc
                    | false, false, false -> acc @ [ c ]
                    | false, true, _ -> acc @ [ Char.ToUpper c ]
                    | false, false, true -> acc
                        
                    //match Char.IsUpper c, i = 0 with
                    //| false, _ -> acc @ [ c ]
                    //| true, true -> acc @ [ Char.ToLower(c) ]
                    //| true, false -> acc @ [ '_'; Char.ToLower(c) ]
                (newAcc, i + 1)) ([], 0)
            |> (fun (chars, _) ->
                match chars.Length > 0 with
                | true -> String([ Char.ToLower chars.Head ] @ chars.Tail |> Array.ofList)
                | false -> "")
            
        member str.ToUtf8Bytes() = Conversions.fromUtf8 str

