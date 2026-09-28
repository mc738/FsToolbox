namespace FsToolbox.Avalonia.Dsl

open Avalonia.Controls
open Avalonia.Layout

[<RequireQualifiedAccess>]
module TabControl =
    
    let create (style: ControlStyle) =
        let c = TabControl()

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

    let setTabs (tabs: TabItem seq) (tc: TabControl) =
        for tab in tabs do
            tc.Items.Add(tab) |> ignore
          
    let withTabs (tabs: TabItem seq) (tc: TabControl) =
        setTabs tabs tc
        tc
