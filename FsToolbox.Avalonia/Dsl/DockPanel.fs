namespace FsToolbox.Avalonia.Dsl

open Avalonia.Controls
open Avalonia.Layout

[<RequireQualifiedAccess>]
module DockPanel =

    let create (style: ControlStyle) =
        let dockPanel = DockPanel()

        match style.StretchType with
        | StretchType.None -> ()
        | StretchType.Vertical -> dockPanel.VerticalAlignment <- VerticalAlignment.Stretch
        | StretchType.Horizontal -> dockPanel.HorizontalAlignment <- HorizontalAlignment.Stretch
        | StretchType.Both ->
            dockPanel.VerticalAlignment <- VerticalAlignment.Stretch
            dockPanel.HorizontalAlignment <- HorizontalAlignment.Stretch

        dockPanel

    let withChild (child: Control) (dock: Dock option) (stackPanel: StackPanel) =
        match dock with
        | None -> ()
        | Some d -> DockPanel.SetDock(child, d)

        stackPanel.Children.Add(child)
