namespace FsToolbox.Avalonia.Dsl

open Avalonia.Controls
open Avalonia.Layout
open FsToolbox.Avalonia.Dsl

[<RequireQualifiedAccess>]
module ContextMenu =
    let create (style: ControlStyle) =
        let c = ContextMenu()

        match style.StretchType with
        | StretchType.None -> ()
        | StretchType.Vertical -> c.VerticalAlignment <- VerticalAlignment.Stretch
        | StretchType.Horizontal -> c.HorizontalAlignment <- HorizontalAlignment.Stretch
        | StretchType.Both ->
            c.VerticalAlignment <- VerticalAlignment.Stretch
            c.HorizontalAlignment <- HorizontalAlignment.Stretch

        for cl in style.Classes do
            c.Classes.Add(cl)

        c

    let withItems (items: MenuItem seq) (cm: ContextMenu) =
        for item in items do
            cm.Items.Add(item) |> ignore

        cm
