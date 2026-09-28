namespace FsToolbox.Avalonia.Dsl

open Avalonia
open Avalonia.Controls
open Avalonia.Controls.Primitives
open Avalonia.Data
open Avalonia.Layout
open Avalonia.Media

[<AutoOpen>]
module Common =


    [<RequireQualifiedAccess>]
    type StretchType =
        | None
        | Vertical
        | Horizontal
        | Both

    type ControlStyle =
        { StretchType: StretchType
          Classes: string list }


        static member Fill =
            { StretchType = StretchType.Both
              Classes = [] }

        static member Default =
            { StretchType = StretchType.None
              Classes = [] }

    [<AutoOpen>]
    module Control =

        let withDataContext<'T, 'TControl when 'TControl :> StyledElement> (data: 'T) (c: 'TControl) =

            c.DataContext <- data
            c

        let tryGetDataContext<'T> (c: Control) =
            try
                c.DataContext :?> 'T |> Ok
            with ex ->
                Error ex.Message

        let withGridRow<'T when 'T :> Control> (row: int) (c: 'T) =
            Grid.SetRow(c, row)
            c


        let withGridRowSpan<'T when 'T :> Control> (rowSpan: int) (c: 'T) =
            Grid.SetRowSpan(c, rowSpan)
            c

        let withGridColumn<'T when 'T :> Control> (column: int) (c: 'T) =
            Grid.SetColumn(c, column)
            c

        let withGridColumnSpan<'T when 'T :> Control> (columnSpan: int) (c: 'T) =
            Grid.SetColumnSpan(c, columnSpan)
            c

        let withDock<'T when 'T :> Control> (dock: Dock) (c: 'T) =
            DockPanel.SetDock(c, dock)
            c



        let setBindings<'T when 'T :> Control> (bindings: (AvaloniaProperty * BindingBase) seq) (c: 'T) =
            for prop, binding in bindings do
                c.Bind(prop, binding) |> ignore


        let withBindings<'T when 'T :> Control> (bindings: (AvaloniaProperty * BindingBase) seq) (c: 'T) =
            setBindings bindings c
            c

        let bind<'T when 'T :> Control> (property: AvaloniaProperty) (binding: BindingBase) (c: 'T) =
            c.Bind(property, binding) |> ignore
            c


    [<AutoOpen>]
    module Layoutable =
        
        let withHeight<'T when 'T :> Layoutable> (height: double) (c: 'T) =
            c.Height <- height
            c
        
        let withWidth<'T when 'T :> Layoutable> (width: double) (c: 'T) =
            c.Width <- width
            c
        
        ()
    
    [<AutoOpen>]
    module SelectingItemsControl =

        let withItems<'T when 'T :> SelectingItemsControl> (clearCurrent: bool) (items: Control seq) (c: 'T) =
            if clearCurrent then
                c.Items.Clear()

            for item in items do
                c.Items.Add(item) |> ignore

            c

        let setItems<'T when 'T :> SelectingItemsControl> (clearCurrent: bool) (items: Control seq) (c: 'T) =
            if clearCurrent then
                c.Items.Clear()

            for item in items do
                c.Items.Add(item) |> ignore

    [<AutoOpen>]
    module Panel =

        let withChildren<'T when 'T :> Panel> (children: Control seq) (p: 'T) =
            p.Children.AddRange(children)
            p

        let setChildren<'T when 'T :> Panel> (children: Control seq) (p: 'T) = p.Children.AddRange(children)

        let withBackground<'T when 'T :> Panel> (background: IBrush) (p: 'T) =
            p.Background <- background
            p
