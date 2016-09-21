
module fsharpAnimalQuizKata.RunModuleRefactoring


open fsharpAnimalQuizKata.IoModule
open fsharpAnimalQuizKata.RecordUtils
open System
open System.Collections.Generic
open fsharpAnimalQuizKata.BrainModule


let runUntilIndexPassingEntireStructure (outputStream:OutStream) (inputStream:InStream) playStructure index=
    let mutable playStart = playStructure
    for i = 1 to index do
        playStart <- consoleInteract playStart
        outputStream.Write(playStart.messageFromEngine)
        playStart.messageFromPlayer <- (Some (inputStream.Input()))

        0
    0


   