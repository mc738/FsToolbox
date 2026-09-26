namespace FsToolbox.Avalonia.Dsl

open Avalonia.Controls
open Avalonia.Layout
open FsToolbox.Avalonia.Commands

[<RequireQualifiedAccess>]
module MenuItem =
    let create (style: ControlStyle) =
        let c = MenuItem()

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

    let createDefault () =
        let style = ControlStyle.Default
        create style

    let withHeader (header: string) (mi: MenuItem) =
        mi.Header <- header
        mi

    let withCommand (fn: unit -> unit) (mi: MenuItem) =
        mi.Command <- RelayCommand(fn, (fun () -> true))
        mi
