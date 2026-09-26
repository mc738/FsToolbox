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

