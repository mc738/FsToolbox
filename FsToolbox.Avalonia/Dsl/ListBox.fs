namespace FsToolbox.Avalonia.Dsl

open Avalonia.Controls
open Avalonia.Interactivity
open Avalonia.Layout
open FsToolbox.Avalonia.Dsl

[<RequireQualifiedAccess>]
module ListBox =

    let create (style: ControlStyle) =
        let c = ListBox()

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

    let withItems (items: ListBoxItem list) (clearCurrentItems: bool) (lb: ListBox) =
        if clearCurrentItems then
            lb.Items.Clear()

        for item in items do
            lb.Items.Add(item) |> ignore

    let withContextMenu (cm: ContextMenu) (lb: ListBox) =
        lb.ContextMenu <- cm
        lb

    let onSelectionChanged (fn: RoutedEventArgs -> unit) (lb: ListBox) = lb.SelectionChanged.Add fn
