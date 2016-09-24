module fsharpAnimalQuizKata.BrainModule

open fsharpAnimalQuizKata.RecordUtils
open System

let rec learn playStructure = 
    let currentAnimal  = 
        match playStructure.CurrentNode with 
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
        match playStructure.NewDiscriminatingQuestion with 
            | Some X -> X
            | None -> failwith "discriminating question cannot be empty"

    match playStructure.YesNoList with
     | [] -> match playStructure.MessageFromPlayer with
          | Some "yes" -> SubTree { Question=thisIsTheQuestion; 
                                    YesBranch = AnimalName playStructure.AnimalToBeLearned; 
                                    NoBranch = AnimalName currentAnimal }
          | Some "no" -> SubTree {  Question=thisIsTheQuestion;
                                    YesBranch = AnimalName currentAnimal;
                                    NoBranch = AnimalName playStructure.AnimalToBeLearned }

          | _ -> failwith "called learn when user interaction is different from yes or no"

     | "yes"::T -> SubTree {Question = question playStructure.RootTree; 
                            YesBranch = learn (substituteYesNoList  {playStructure with RootTree = yesBranch playStructure.RootTree} T);     
                            NoBranch= noBranch playStructure.RootTree
                           }
     | "no"::T -> SubTree {Question = question playStructure.RootTree;
                           YesBranch = yesBranch playStructure.RootTree;
                           NoBranch = learn  (substituteYesNoList {playStructure with RootTree = noBranch playStructure.RootTree} T)
                          }
                                                                                                                                   
let consoleInteract playStructure =
    let currentAnimal = match playStructure.CurrentNode with | AnimalName name -> name | _ -> "ERROR: expected  leaf node actual non leaf node!"
    let messageFromPlayer = match playStructure.MessageFromPlayer with | Some X -> X | None -> ""

    match playStructure.CurrentState with
        | InviteToThinkAboutAnAnimal -> initState playStructure
        // todo: will match later
//            match playStructure.conversationToken with | Some X ->  initState playStructure | None -> failwith "no token is given"
        | GuessingFromCurrentNode ->   (
            match playStructure.MessageFromPlayer with
          | Some "yes" -> (
            match playStructure.CurrentNode with
                            | AnimalName _ ->     sayYeah playStructure 
                            | SubTree subTree  -> yesSubTreeNavigation playStructure subTree
                          )
          | Some "no" ->  (
            match playStructure.CurrentNode with 
                            | AnimalName _ -> askWhatAnimalWas playStructure
                            | SubTree subTree -> noSubTreeNavigation playStructure subTree
                          )
          | _ -> askToEnterYesOrNot playStructure 
         )
        | Welcome -> welcomeMessage playStructure
        | AskWhatAnimalWas ->  askDiscriminatingQuestion playStructure messageFromPlayer currentAnimal
        | ExpectingDiscriminatingQuestion -> askAnswerToDiscriminatingQuestion playStructure messageFromPlayer currentAnimal
        | AnsweringDiscriminatingQuestion ->  match playStructure.MessageFromPlayer with
            | (Some "yes"| Some "no") -> { 
                                            templateInitStructure with 
                                                RootTree = learn playStructure;
                                                CurrentNode = learn playStructure;
                                                MessageFromEngine="ok"}        
            | _ ->  {playStructure with MessageFromEngine= "please answer only yes or no."} 
