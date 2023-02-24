namespace FsToolbox.Core

module Compression =
        open System.IO.Compression
        
        let zip (path: string) (output: string) =
            ZipFile.CreateFromDirectory(path, output, CompressionLevel.Optimal, false)
            
        let unzip (path: string) (output: string) =
            ZipFile.ExtractToDirectory(path, output)

