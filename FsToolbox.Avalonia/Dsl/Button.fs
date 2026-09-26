namespace FsToolbox.Avalonia.Dsl

open Avalonia.Controls
open Avalonia.Interactivity
open Avalonia.Layout


[<RequireQualifiedAccess>]
module Button =
    let create (style: ControlStyle) =
        let c = Button()

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

    let withContent (content: obj) (btn: Button) =
        btn.Content <- content
        btn

    let onClick (fn: RoutedEventArgs -> unit) (btn: Button) =
        btn.Click.Add fn
        btn
