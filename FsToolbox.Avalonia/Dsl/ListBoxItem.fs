namespace FsToolbox.Avalonia.Dsl

open Avalonia.Controls
open Avalonia.Layout

[<RequireQualifiedAccess>]
module ListBoxItem =
    let create (style: ControlStyle) =
        let c = ListBoxItem()

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
