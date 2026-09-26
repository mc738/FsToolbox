namespace FsToolbox.Avalonia.Dsl

open Avalonia.Controls
open Avalonia.Layout

[<RequireQualifiedAccess>]
module StackPanel =

    let create (style: ControlStyle) =
        let stackPanel = StackPanel()

        match style.StretchType with
        | StretchType.None -> ()
        | StretchType.Vertical -> stackPanel.VerticalAlignment <- VerticalAlignment.Stretch
        | StretchType.Horizontal -> stackPanel.HorizontalAlignment <- HorizontalAlignment.Stretch
        | StretchType.Both ->
            stackPanel.VerticalAlignment <- VerticalAlignment.Stretch
            stackPanel.HorizontalAlignment <- HorizontalAlignment.Stretch

        stackPanel

    let createDefault () = create ControlStyle.Default

    let withOrientation (o: Orientation) (sp: StackPanel) =
        sp.Orientation <- o
        sp

    let withChild (child: Control) (stackPanel: StackPanel) =
        stackPanel.Children.Add(child)
        stackPanel
