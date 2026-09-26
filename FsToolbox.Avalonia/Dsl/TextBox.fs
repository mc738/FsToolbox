namespace FsToolbox.Avalonia.Dsl

open Avalonia.Controls
open Avalonia.Layout

[<RequireQualifiedAccess>]
module TextBox =

    let create (style: ControlStyle) =
        let c = TextBox()

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

    let withPlaceholderText (placeholderText: string) (c: TextBox) =
        c.PlaceholderText <- placeholderText
        c
