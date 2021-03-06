﻿
module fsharpAnimalQuizKata.RecordUtils
open System
open Gtk;
open FSharp.Data
open System.Xml.Linq
open System.Data
open fsharpAnimalQuizKata.KnowledgeTreeXmlSchema



type KnowledgeTree = AnimalName of string | SubTree of Tree
and Tree = {Question: string; YesBranch: KnowledgeTree; NoBranch: KnowledgeTree}

type State = | Welcome | InviteToThinkAboutAnAnimal |  GuessingFromCurrentNode |AskWhatAnimalWas | ExpectingDiscriminatingQuestion | AnsweringDiscriminatingQuestion

let rec printTree tree = 
    match tree with
        | AnimalName name -> name
        | SubTree {Question=question; YesBranch=yBranch; NoBranch=nBranch } ->  "[ Question = " + question  + "; YesBranch = " + printTree yBranch +  "; NoBranch = "+printTree nBranch + "]"



let rec treeToTreeStore tree (store: TreeStore) iter  prefix =
    match tree with 
        | AnimalName name -> 
             do store.AppendValues(iter,[|prefix + name|]) |> ignore 
        | SubTree {Question=quest; YesBranch=yBranch; NoBranch=nBranch } -> 
             let innerIter = store.AppendValues(iter,[|prefix + quest|])
             do treeToTreeStore yBranch store innerIter "YES: " 
             do treeToTreeStore nBranch store innerIter "NO:  "


let rec treeToXml tree =
    match tree with 
        | AnimalName name -> "<node><animal>" + name + "</animal></node>"
        | SubTree {Question=question; YesBranch=yBranch; NoBranch=nBranch } ->  
           "<node><question>" + question + "</question>" + "<yesBranch>" + treeToXml yBranch + "</yesBranch><noBranch>" + treeToXml nBranch + "</noBranch></node>"

      
let rec xmlToTree (tree:KnowledgeBaseXmlSchema.Node) =      
    match tree.Animal with
    | Some x -> AnimalName x
    | None ->  match (tree.Question,tree.YesBranch,tree.NoBranch) with 
      | (Some y1,Some y2,Some y3) -> SubTree {Question=y1; YesBranch=xmlToTree y2.Node ; NoBranch = xmlToTree y3.Node }
      | _ -> failwith "error wrapping xml tree"  
    

 


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