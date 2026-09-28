namespace FsToolbox.Avalonia.Dsl

open Avalonia
open Avalonia.Controls
open Avalonia.Layout

module Border =
    
    
    let createFill (child: Control) =
        let border = Border()
        
        border.HorizontalAlignment <- HorizontalAlignment.Stretch
        border.VerticalAlignment <- VerticalAlignment.Stretch
        
        border.Child <- child
        
        border

    let withMargin (b: Border) =
        b.Margin <- Thickness(10.)
        b
