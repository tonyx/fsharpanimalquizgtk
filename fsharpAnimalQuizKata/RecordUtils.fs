
module fsharpAnimalQuizKata.RecordUtils
open System
open Gtk;

type KnowledgeTree = AnimalName of string | SubTree of Tree
and Tree = {Question: string; YesBranch: KnowledgeTree; NoBranch: KnowledgeTree}
type State = | Welcome | InviteToThinkAboutAnAnimal |  GuessingFromCurrentNode |AskWhatAnimalWas | ExpectingDiscriminatingQuestion | AnsweringDiscriminatingQuestion

let rec printTree tree = 
    match tree with
        | AnimalName name -> name
        | SubTree {Question=question; YesBranch=yBranch; NoBranch=nBranch } ->  "[ Question = " + question  + "; YesBranch = " + printTree yBranch +  "; NoBranch = "+printTree nBranch + "]"


let noBranch tree = 
        match tree with
        |SubTree X -> X.NoBranch
        | _ -> failwith "consider the consistency of the yes no list"

let yesBranch tree = 
        match tree with
        |SubTree X -> X.YesBranch
        | _ -> failwith "consider the consistency of the yes no list"



let rec treeToTreeStore tree (store: TreeStore) iter  =
    match tree with 
        | AnimalName name -> 
             do store.AppendValues(iter,[|name|]) |> ignore 
         
        | SubTree {Question=quest; YesBranch=yBranch; NoBranch=nBranch } -> 

             let innerIter = store.AppendValues(iter,[|quest|])
             do treeToTreeStore (yesBranch tree) store innerIter
             do treeToTreeStore (noBranch tree) store innerIter

let rec treeToTreeStoreOk tree (store: TreeStore) iter  prefix =
    match tree with 
        | AnimalName name -> 
             do store.AppendValues(iter,[|prefix + name|]) |> ignore 
         
        | SubTree {Question=quest; YesBranch=yBranch; NoBranch=nBranch } -> 

             let innerIter = store.AppendValues(iter,[|prefix + quest|])
             do treeToTreeStoreOk (yesBranch tree) store innerIter "YES: " 
             do treeToTreeStoreOk (noBranch tree) store innerIter "NO:  "




type PlayingStructure = {       ConversationToken: string option;
                                MessageFromEngine: string; 
                                mutable MessageFromPlayer: string option; 
                                CurrentState: State; 
                                AnimalToBeLearned: string; 
                                RootTree: KnowledgeTree; 
                                CurrentNode: KnowledgeTree; 
                                YesNoList: string list; 
                                NewDiscriminatingQuestion : string option
                            }


let templateInitStructure  = {
                    ConversationToken = None;
                    MessageFromEngine="";
                    MessageFromPlayer = None;
                    CurrentState = Welcome;
                    AnimalToBeLearned="";
                    RootTree=AnimalName "elephant";
                    CurrentNode=AnimalName "elephant";
                    YesNoList=[];
                    NewDiscriminatingQuestion=None
                 }


let defaultInitState tree =  { templateInitStructure with RootTree=tree; CurrentNode=tree}

let currentAnimal playStructure = match playStructure.CurrentNode with | AnimalName name -> name | _ -> "ERROR!"

let question tree  =
    match tree with
        | AnimalName name -> "is it a "+name+"?"
        | SubTree {Question=question} -> question 

let substituteYesNoList playStructure newYesNoList = {playStructure with YesNoList=newYesNoList}
                            
let initState playStructure = 
                            { playStructure with 
                                 MessageFromEngine = question playStructure.CurrentNode;
                                 CurrentState = GuessingFromCurrentNode }

let sayYeah playStructure = {playStructure with MessageFromEngine="yeah!" ; CurrentState = Welcome}  

let yesSubTreeNavigation playStructure subTree =  
                  { playStructure with 
                     MessageFromEngine = question subTree.YesBranch; 
                     CurrentNode = subTree.YesBranch; 
                     YesNoList = playStructure.YesNoList@["yes"]}

let noSubTreeNavigation playStructure subTree = 
                  { playStructure with 
                     MessageFromEngine = question subTree.NoBranch; 
                     CurrentNode = subTree.NoBranch; 
                     YesNoList = playStructure.YesNoList@["no"]}

let askWhatAnimalWas playStructure  = 
                    {playStructure with 
                        MessageFromEngine="what animal was?"; 
                        CurrentState = AskWhatAnimalWas}
  

let askToEnterYesOrNot playStructure = 
                    {playStructure with 
                        MessageFromEngine = "please answer only yes or no. "+question  playStructure.CurrentNode;
                        CurrentState = GuessingFromCurrentNode}

let welcomeMessage playStructure =
                  { playStructure with 
                        MessageFromEngine = "think about an animal";
                        CurrentState=InviteToThinkAboutAnAnimal 
                        ConversationToken=Some (Guid.NewGuid().ToString())}

let askDiscriminatingQuestion playStructure messageFromPlayer currentAnimal = 
                    match messageFromPlayer with
                     | "" -> playStructure
                     | _ ->  { playStructure with 
                                MessageFromEngine="please, write a yes/no question to distinguish a "+messageFromPlayer+" from a "+currentAnimal;
                                CurrentState=ExpectingDiscriminatingQuestion;
                                AnimalToBeLearned=messageFromPlayer }
                    

let askAnswerToDiscriminatingQuestion playStructure messageFromPlayer currentAnimal = 
                     { playStructure with
                         MessageFromEngine="what is the answer to the question \""+messageFromPlayer+"\" to distinguish a "+playStructure.AnimalToBeLearned+" from a "+currentAnimal+"?";
                         CurrentState=AnsweringDiscriminatingQuestion;
                         NewDiscriminatingQuestion=Some messageFromPlayer }