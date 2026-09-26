namespace FsToolbox.Avalonia.Dsl

open Avalonia.Controls
open Avalonia.Layout
open FsToolbox.Avalonia.Dsl

[<RequireQualifiedAccess>]
module ComboBox =

    let create (style: ControlStyle) =
        let c = ComboBox()

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

    let createDefault () = create ControlStyle.Default

    let withItems (items: ComboBoxItem list) (clearCurrentItems: bool) (cb: ComboBox) =
        if clearCurrentItems then
            cb.Items.Clear()

        for item in items do
            cb.Items.Add(item) |> ignore

    let withSelectionChanged (fn: SelectionChangedEventArgs -> unit) (cb: ComboBox) =
        cb.SelectionChanged.Add fn
        cb

    let onSelectionChanged (fn: SelectionChangedEventArgs -> unit) (cb: ComboBox) = cb.SelectionChanged.Add fn
