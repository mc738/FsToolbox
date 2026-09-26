namespace FsToolbox.Avalonia.Dsl

open Avalonia.Controls
open Avalonia.Input
open Avalonia.Interactivity
open Avalonia.Layout
open Avalonia.Media

module Presets =

        [<RequireQualifiedAccess>]
        module Titles =

            let dialog text =
                TextBlock(Text = text, FontWeight = FontWeight.Bold)


        module Labels =

            let input (content: obj) (target: IInputElement) =
                Label.createDefault () |> Label.withContent content |> Label.withTarget target

        module Buttons =

            let group (buttons: Button seq) =
                StackPanel.createDefault ()
                |> StackPanel.withOrientation Orientation.Horizontal
                |> withChildren (buttons |> Seq.cast<Control>)

            let general (content: obj) (fn: RoutedEventArgs -> unit) =
                Button.create ControlStyle.Default
                |> Button.withContent content
                |> Button.onClick fn
            
            let success (content: obj) (fn: RoutedEventArgs -> unit) =
                Button.create
                    { ControlStyle.Default with
                        Classes = [ "ok" ] }
                |> Button.withContent content
                |> Button.onClick fn

            let cancel (content: obj) (fn: RoutedEventArgs -> unit) =
                Button.create
                    { ControlStyle.Default with
                        Classes = [ "cancel" ] }
                |> Button.withContent content
                |> Button.onClick fn
