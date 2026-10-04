namespace FsToolbox.Avalonia.Dsl

open Avalonia.Controls
open Avalonia.Layout

[<RequireQualifiedAccess>]
module Grid =

    let create (style: ControlStyle) (columns: ColumnDefinitions option) (rows: RowDefinitions option) =
        let grid = Grid()

        match style.StretchType with
        | StretchType.None -> ()
        | StretchType.Vertical -> grid.VerticalAlignment <- VerticalAlignment.Stretch
        | StretchType.Horizontal -> grid.HorizontalAlignment <- HorizontalAlignment.Stretch
        | StretchType.Both ->
            grid.VerticalAlignment <- VerticalAlignment.Stretch
            grid.HorizontalAlignment <- HorizontalAlignment.Stretch

        columns |> Option.iter (fun cd -> grid.ColumnDefinitions <- cd)
        rows |> Option.iter (fun rd -> grid.RowDefinitions <- rd)

        grid
        
    let withRows<'T when 'T :> Grid> (rows: RowDefinition seq) (grid: 'T) =
        let rds = RowDefinitions()
        rds.AddRange(rows)
        grid.RowDefinitions <- rds
        grid

    let withColumns<'T when 'T :> Grid> (columns: ColumnDefinition seq) (grid: 'T) =
        let cds = ColumnDefinitions()
        cds.AddRange(columns)
        grid.ColumnDefinitions <- cds
        grid
    
    let withChild (child: Control) (row: int option) (column: int option) (grid: Grid) =
        match column with
        | None -> ()
        | Some c -> Grid.SetColumn(child, c)

        match row with
        | None -> ()
        | Some r -> Grid.SetRow(child, r)

        grid.Children.Add(child)
