namespace FsToolbox.Core

module Strings =
    
    open System
    open System.Text

    let bytesToHex (bytes: byte array) =
        Convert.ToHexString bytes
        
        (*
        bytes
        |> Array.fold (fun (sb: StringBuilder) b -> sb.AppendFormat("{0:x2}", b)) (StringBuilder(bytes.Length * 2))
        |> fun sb -> sb.ToString()
        *)
    
    let equalOrdinal a b = String.Equals(a, b, StringComparison.Ordinal)
    
    let equalOrdinalIgnoreCase a b = String.Equals(a, b, StringComparison.OrdinalIgnoreCase)

