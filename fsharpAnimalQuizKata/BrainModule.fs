module fsharpAnimalQuizKata.BrainModule

open fsharpAnimalQuizKata.RecordUtils
open System

let rec learn playStructure = 
    let currentAnimal  = 
        match playStructure.currentNode with 
            | AnimalName name -> name 
            | _ -> failwith "ERROR, current node structure should be a leaf"

    let question tree  =
        match tree with
            | AnimalName name -> "is it a "+name+"?"
            | SubTree {Question=question} -> question 

    let noBranch tree = 
         match tree with
            |SubTree X -> X.NoBranch
            | _ -> failwith "consider the consistency of the yes no list"

    let yesBranch tree = 
         match tree with
            |SubTree X -> X.YesBranch
            | _ -> failwith "consider the consistency of the yes no list"

    let thisIsTheQuestion = 
        match playStructure.newDiscriminatingQuestion with 
            | Some X -> X
            | None -> failwith "discriminating question cannot be empty"

    match playStructure.yesNoList with
     | [] -> match playStructure.messageFromPlayer with
          | Some "yes" -> SubTree { Question=thisIsTheQuestion; 
                                    YesBranch = AnimalName playStructure.animalToBeLearned; 
                                    NoBranch = AnimalName currentAnimal }
          | Some "no" -> SubTree {  Question=thisIsTheQuestion;
                                    YesBranch = AnimalName currentAnimal;
                                    NoBranch = AnimalName playStructure.animalToBeLearned }

          | _ -> failwith "called learn when user interaction is different from yes or no"

     | "yes"::T -> SubTree {Question = question playStructure.rootTree; 
                            YesBranch = learn (substituteYesNoList  {playStructure with rootTree = yesBranch playStructure.rootTree} T);     
                            NoBranch= noBranch playStructure.rootTree
                           }
     | "no"::T -> SubTree {Question = question playStructure.rootTree;
                           YesBranch = yesBranch playStructure.rootTree;
                           NoBranch = learn  (substituteYesNoList {playStructure with rootTree = noBranch playStructure.rootTree} T)
                          }
                                                                                                                                   
let consoleInteract playStructure =
    let currentAnimal = match playStructure.currentNode with | AnimalName name -> name | _ -> "ERROR: expected  leaf node actual non leaf node!"
    let messageFromPlayer = match playStructure.messageFromPlayer with | Some X -> X | None -> ""

    match playStructure.currentState with
        | InviteToThinkAboutAnAnimal -> initState playStructure
        // todo: will match later
//            match playStructure.conversationToken with | Some X ->  initState playStructure | None -> failwith "no token is given"
        | GuessingFromCurrentNode ->   (
            match playStructure.messageFromPlayer with
          | Some "yes" -> (
            match playStructure.currentNode with
                            | AnimalName _ ->     sayYeah playStructure 
                            | SubTree subTree  -> yesSubTreeNavigation playStructure subTree
                          )
          | Some "no" ->  (
            match playStructure.currentNode with 
                            | AnimalName _ -> askWhatAnimalWas playStructure
                            | SubTree subTree -> noSubTreeNavigation playStructure subTree
                          )
          | _ -> askToEnterYesOrNot playStructure 
         )
        | Welcome -> welcomeMessage playStructure
        | AskWhatAnimalWas ->  askDiscriminatingQuestion playStructure messageFromPlayer currentAnimal
        | ExpectingDiscriminatingQuestion -> askAnswerToDiscriminatingQuestion playStructure messageFromPlayer currentAnimal
        | AnsweringDiscriminatingQuestion ->  match playStructure.messageFromPlayer with
            | (Some "yes"| Some "no") -> { 
                                            templateInitStructure with 
                                                rootTree = learn playStructure;
                                                currentNode = learn playStructure;
                                                messageFromEngine="ok"}        
            | _ ->  {playStructure with messageFromEngine= "please answer only yes or no."} 




