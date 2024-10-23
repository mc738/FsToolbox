namespace FsToolbox.ProcessWrappers

[<AutoOpen>]
module Common =
    
    let wrapString (str: string) =
        match str.Contains(' ') with
        | true -> $"\"{str}\""
        | false -> str
        
    let forceWrapString (str: string) = $"\"{str}\""

    let concatStrings (separator: string) (values: string seq) =
        values
        //|> Seq.map wrapString
        |> String.concat separator