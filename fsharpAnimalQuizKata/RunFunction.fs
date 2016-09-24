module fsharpAnimalQuizKata.RunModule

open fsharpAnimalQuizKata.IoModule
open fsharpAnimalQuizKata.RecordUtils
open System
open System.Collections.Generic
open fsharpAnimalQuizKata.BrainModule

    let  initPlayStructure state tree = { ConversationToken = None; 
                                MessageFromEngine="";
                                MessageFromPlayer=Some "";
                                CurrentState=state;AnimalToBeLearned="";
                                RootTree=tree;CurrentNode=tree;
                                YesNoList = [];
                                NewDiscriminatingQuestion=None
                             }

let runUntilIndex (outputStream:OutStream) (inputStream:InStream) tree state index =  
    let mutable playStart = initPlayStructure state tree
    for i = 1 to index do
        playStart <- consoleInteract playStart   
        outputStream.Write(playStart.MessageFromEngine) 
        playStart.MessageFromPlayer <- (Some (inputStream.Input())) 
        0
    0 


let runForever (outputStream:OutStream) (inputStream:InStream) tree state =
    let mutable playStart = initPlayStructure state tree
    while true do
        playStart <- consoleInteract playStart
        outputStream.Write(playStart.MessageFromEngine)
        playStart.MessageFromPlayer <- (Some (inputStream.Input()))
        0
    0
