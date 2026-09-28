namespace FsToolbox.Avalonia.Dsl

open Avalonia.Controls


[<RequireQualifiedAccess>]
module Window =

    let createDialog () =
        let window = Window()
        
        window.WindowDecorations <- WindowDecorations.None
        
        window.WindowStartupLocation <- WindowStartupLocation.CenterScreen
        
        window
        
    let withWindowDecorations (decorations: WindowDecorations) (window: Window) =
        window.WindowDecorations <- decorations
        window
        
    let withStartupLocation (startupLocation: WindowStartupLocation) (window: Window) =
        window.WindowStartupLocation <- startupLocation
        window
    
    let withMinWidth (minWidth: double) (window: Window) =
        window.MinWidth <- minWidth
        window
        
    let withWidth (width: double) (window: Window) =
        window.Width <- width
        window
        
    let withMinHeight (minHeight: double) (window: Window) =
        window.MinHeight <- minHeight
        window
        
    let withHeight (height: double) (window: Window) =
        window.Height <- height
        window
        
    let withTitle (title: string) (window: Window) =
        window.Title <- title
        window
    
    
    let setContent (content: obj) (window: Window) =
        window.Content <- content
        
    ()

