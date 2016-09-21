
module fsharpAnimalQuizKata.RunModule

open fsharpAnimalQuizKata.IoModule
open fsharpAnimalQuizKata.RecordUtils
open System
open System.Collections.Generic
open fsharpAnimalQuizKata.BrainModule

    let  initPlayStructure state tree = { conversationToken = None; 
                                messageFromEngine="";
                                messageFromPlayer=Some "";
                                currentState=state;animalToBeLearned="";
                                rootTree=tree;currentNode=tree;
                                yesNoList = [];
                                newDiscriminatingQuestion=None
                             }

let runUntilIndex (outputStream:OutStream) (inputStream:InStream) tree state index =  
    let mutable playStart = initPlayStructure state tree
    for i = 1 to index do
        playStart <- consoleInteract playStart   
        outputStream.Write(playStart.messageFromEngine) 
        playStart.messageFromPlayer <- (Some (inputStream.Input())) 
        0
    0 


let runForever (outputStream:OutStream) (inputStream:InStream) tree state =
    let mutable playStart = initPlayStructure state tree
    while true do
        playStart <- consoleInteract playStart
        outputStream.Write(playStart.messageFromEngine)
        playStart.messageFromPlayer <- (Some (inputStream.Input()))
        0
    0

