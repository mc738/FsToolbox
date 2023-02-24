namespace FsToolbox.Core

open System.IO

[<RequireQualifiedAccess>]
module Streams =
    /// Read a stream into a buffer.
    let readToBuffer (stream: Stream) bufferSize =
        async {
            // TODO What if more data than buffer size?
            let buffer =
                [| for i in [ 0 .. bufferSize ] -> 0uy |]

            stream.ReadAsync(buffer, 0, bufferSize)
            |> Async.AwaitTask
            |> ignore

            return buffer
        }
        
    let readAllBytes (s : Stream) = 
        use ms = new MemoryStream()
        s.CopyTo(ms)
        ms.ToArray()

