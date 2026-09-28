namespace FsToolbox.Avalonia

open System
open System.Windows.Input

module Commands =

    type RelayCommand(action: unit -> unit, canExecute: unit -> bool) =
        interface ICommand with
            member this.CanExecute(parameter) = canExecute ()

            member this.add_CanExecuteChanged(value: EventHandler) = ()


            member this.remove_CanExecuteChanged(value: EventHandler) = ()

            member this.Execute(parameter) = action ()


    type DelegateCommand<'T>(action: 'T -> unit, canExecute: 'T -> bool) =
        interface ICommand with
            member this.CanExecute(parameter) =
                match parameter with
                | :? 'T as v -> canExecute v
                | _ -> false

            member this.add_CanExecuteChanged(value: EventHandler) = ()
            member this.remove_CanExecuteChanged(value: EventHandler) = ()

            member this.Execute(parameter) =
                match parameter with
                | :? 'T as v -> action v
                | _ -> ()
