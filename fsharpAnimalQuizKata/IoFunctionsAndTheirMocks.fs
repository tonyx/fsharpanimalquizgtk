
module fsharpAnimalQuizKata.IoModule

open System
open System.Collections.Generic

type OutStream =
    abstract Write: string -> unit

type InStream =
    abstract Input: unit -> string

type ConsoleOutput() =
    interface OutStream with
       member this.Write(x) = printf "%s\n" x

type ConsoleInput() =
    interface InStream with
        member this.Input() = Console.ReadLine()

