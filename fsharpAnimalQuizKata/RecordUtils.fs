
module fsharpAnimalQuizKata.RecordUtils
open System

type KnowledgeTree = AnimalName of string | SubTree of Tree
and Tree = {Question: string; yesBranch: KnowledgeTree; noBranch: KnowledgeTree}
type state = | Welcome | InviteToThinkAboutAnAnimal |  GuessingFromCurrentNode |AskWhatAnimalWas | ExpectingDiscriminatingQuestion | AnsweringDiscriminatingQuestion

let rec printTree tree = 
    match tree with
        | AnimalName name -> name
        | SubTree {Question=question; yesBranch=yBranch; noBranch=nBranch } ->  "[ Question = " + question  + "; YesBranch = " + printTree yBranch +  "; NoBranch = "+printTree nBranch + "]"

type playingStructure = {       conversationToken: string option;
                                messageFromEngine: string; 
                                mutable messageFromPlayer: string option; 
                                currentState: state; 
                                animalToBeLearned: string; 
                                rootTree: KnowledgeTree; 
                                currentNode: KnowledgeTree; 
                                yesNoList: string list; 
                                newDiscriminatingQuestion : string option
                            }

let templateInitStructure  = {
                    conversationToken = None;
                    messageFromEngine="";
                    messageFromPlayer = None;
                    currentState = Welcome;
                    animalToBeLearned="";
                    rootTree=AnimalName "elephant";
                    currentNode=AnimalName "elephant";
                    yesNoList=[];
                    newDiscriminatingQuestion=None
                 }


let defaultInitState tree =  { templateInitStructure with rootTree=tree; currentNode=tree}

let currentAnimal playStructure = match playStructure.currentNode with | AnimalName name -> name | _ -> "ERROR!"

let question tree  =
    match tree with
        | AnimalName name -> "is it a "+name+"?"
        | SubTree {Question=question} -> question 

let substituteYesNoList playStructure newYesNoList = {playStructure with yesNoList=newYesNoList}
                            
let initState playStructure = 
                            { playStructure with 
                                 messageFromEngine = question playStructure.currentNode;
                                 currentState = GuessingFromCurrentNode }

let sayYeah playStructure = {playStructure with messageFromEngine="yeah!" ; currentState = Welcome}  

let yesSubTreeNavigation playStructure subTree =  
                  { playStructure with 
                     messageFromEngine = question subTree.yesBranch; 
                     currentNode = subTree.yesBranch; 
                     yesNoList = playStructure.yesNoList@["yes"]}

let noSubTreeNavigation playStructure subTree = 
                  { playStructure with 
                     messageFromEngine = question subTree.noBranch; 
                     currentNode = subTree.noBranch; 
                     yesNoList = playStructure.yesNoList@["no"]}

let askWhatAnimalWas playStructure  = 
                    {playStructure with 
                        messageFromEngine="what animal was?"; 
                        currentState = AskWhatAnimalWas}
  

let askToEnterYesOrNot playStructure = 
                    {playStructure with 
                        messageFromEngine = "please answer only yes or no. "+question  playStructure.currentNode;
                        currentState = GuessingFromCurrentNode}

let welcomeMessage playStructure =
                  { playStructure with 
                        messageFromEngine = "think about an animal";
                        currentState=InviteToThinkAboutAnAnimal 
                        conversationToken=Some (Guid.NewGuid().ToString())}

let askDiscriminatingQuestion playStructure messageFromPlayer currentAnimal = 
                    match messageFromPlayer with
                     | "" -> playStructure
                     | _ ->  { playStructure with 
                                messageFromEngine="please, write a yes/no question to distinguish a "+messageFromPlayer+" from a "+currentAnimal;
                                currentState=ExpectingDiscriminatingQuestion;
                                animalToBeLearned=messageFromPlayer }
                    

let askAnswerToDiscriminatingQuestion playStructure messageFromPlayer currentAnimal = 
                     { playStructure with
                         messageFromEngine="what is the answer to the question \""+messageFromPlayer+"\" to distinguish a "+playStructure.animalToBeLearned+" from a "+currentAnimal+"?";
                         currentState=AnsweringDiscriminatingQuestion;
                         newDiscriminatingQuestion=Some messageFromPlayer }
                     

