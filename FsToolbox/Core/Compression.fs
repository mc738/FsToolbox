namespace FsToolbox.Core

open System.IO.Compression

module Compression =

    let zip (path: string) (output: string) =
        ZipFile.CreateFromDirectory(path, output, CompressionLevel.Optimal, false)

    let unzip (path: string) (output: string) =
        ZipFile.ExtractToDirectory(path, output)
