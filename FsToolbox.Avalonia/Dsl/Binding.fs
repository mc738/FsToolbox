namespace FsToolbox.Avalonia.Dsl

open Avalonia.Data

[<RequireQualifiedAccess>]
module Binding =
    
    let create () =
        Binding()
    
    let createFromPath (path) =
        Binding(path)
        
    let withSource (source: obj) (binding: Binding) =
        binding.Source <- source
        binding

    let withMode (mode: BindingMode) (binding: Binding) =
        binding.Mode <- mode
        binding