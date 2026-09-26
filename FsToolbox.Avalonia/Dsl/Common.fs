namespace FsToolbox.Avalonia.Dsl

open Avalonia
open Avalonia.Controls
open Avalonia.Controls.Primitives

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

