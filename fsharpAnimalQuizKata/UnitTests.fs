namespace animalquizTests

module unitTests =

    open System
    open NUnit.Framework
    open fsharpAnimalQuizKata.RunModule
    open System.Collections
    open System.Collections.Generic
    open fsharpAnimalQuizKata.IoModule
    open fsharpAnimalQuizKata.BrainModule
    open Rhino.Mocks
    open fsharpAnimalQuizKata.RecordUtils
    open FSharp.Data
    open fsharpAnimalQuizKata.KnowledgeTreeXmlSchema



    [<Test>]
      let ``before starting will not have any conversation token``()=
        // arrange
        let playStart = defaultInitState (AnimalName "monkey")

        let noToken = match playStart.ConversationToken with | None -> true | Some X -> false

        Assert.IsTrue(noToken)

    [<Test>]
      let ``at first interaction will receive a new token``()=
        // arrange
        let playStart = defaultInitState (AnimalName "monkey")

        // act
        let result = consoleInteract playStart

        // assert
        let hasToken = match result.ConversationToken with | None -> false | Some _ -> true
        Assert.IsTrue(hasToken)



    [<Test>]
     let ``at the initial state, Welcome, the user receive the message "please, think about an animal", and state becomes 'invitethinkaboutanimal ``()=
        // arrange
        let playStart = defaultInitState (AnimalName "monkey")

        // act
        let result = consoleInteract playStart

        // assert
        Assert.AreEqual(InviteToThinkAboutAnAnimal,  result.CurrentState)
        Assert.AreEqual("think about an animal", result.MessageFromEngine)
               
                     
   [<Test>]
     let ``when the state is InviteToThinkAboutAnAnimal, and the current node is leaf will ask if it is the animal on the current note, and the state is Guessingfromcurrentnode``()=
        // arrange
        let playStart = {(defaultInitState (AnimalName "cat")) with CurrentState = InviteToThinkAboutAnAnimal;  ConversationToken=Some "token"}

        // act
        let resultUsingStructure = consoleInteract playStart

        // assert
        Assert.AreEqual("is it a cat?",resultUsingStructure.MessageFromEngine)
        Assert.AreEqual(GuessingFromCurrentNode,  resultUsingStructure.CurrentState)

   [<Test>]
     let ``when the state is InviteToThinkAboutAnAnimal, and the current node is not leaf, will ask the node question, and the state is guessingfromcurrentnode``()=
        // arrange
        let tree = SubTree {Question="is it big?"; YesBranch = AnimalName "elephant"; NoBranch = AnimalName "cat"}
        let playStart = {(defaultInitState  tree) with CurrentState = InviteToThinkAboutAnAnimal; ConversationToken = Some "token"}

        // act
        let resultUsingStructure = consoleInteract playStart

        // assert
        Assert.AreEqual("is it big?",resultUsingStructure.MessageFromEngine)
        Assert.AreEqual(GuessingFromCurrentNode,  resultUsingStructure.CurrentState)

   [<Test>]
     let ``when is guessing from current non leaf node, a yes answer will make the current node being the yesBranch, and the yesNo history list will remember the "yes" answer ``()=
        // arrange
        let tree = SubTree {Question="is it big?"; YesBranch = AnimalName "elephant"; NoBranch = AnimalName "cat"}
        let playStart = {(defaultInitState  tree) with CurrentState = GuessingFromCurrentNode; MessageFromPlayer = Some "yes"}

        // act
        let resultUsingStructure = consoleInteract playStart

        // assert
        Assert.AreEqual("is it a elephant?",resultUsingStructure.MessageFromEngine)
        Assert.AreEqual(GuessingFromCurrentNode,  resultUsingStructure.CurrentState)
        Assert.AreEqual(AnimalName "elephant",resultUsingStructure.CurrentNode)
        Assert.AreEqual(["yes"],resultUsingStructure.YesNoList)

   [<Test>]
     let ``if the current node is leaf, and we are in GuessingFromCurrentNode, a "yes" answer will make the system cheers with a "yeah!"``()=
        // arrange
        let playStart = {(defaultInitState (AnimalName "cat")) with MessageFromPlayer = Some "yes"; CurrentState=GuessingFromCurrentNode}

        // act
        let result = consoleInteract playStart

        // assert
        Assert.AreEqual(Welcome,  result.CurrentState)
        Assert.AreEqual("yeah!", result.MessageFromEngine)


   [<Test>]
     let ``When the state is GuessingFromCurrentNode, and the current node is leaf, if I say no, it will ask me what animal was``()=
        // arrange
        let playStart = {(defaultInitState (AnimalName "cat")) with MessageFromPlayer = Some "no"; CurrentState=GuessingFromCurrentNode}

        // act
        let result = consoleInteract playStart

        // assert
        Assert.AreEqual(AskWhatAnimalWas,  result.CurrentState)
        Assert.AreEqual("what animal was?", result.MessageFromEngine)

   [<Test>]
     let ``after asking what animal was, will ask me to suggest the yes/no question that can be used to distinguish the new animal with the anmial of the current node``()=
        // arrange
        let playStart = {(defaultInitState (AnimalName "hummingbird")) with MessageFromPlayer = Some "cat"; CurrentState=AskWhatAnimalWas}


        // act
        let result = consoleInteract playStart

        // assert
        Assert.AreEqual(ExpectingDiscriminatingQuestion,  result.CurrentState)
        Assert.AreEqual(AnimalName "hummingbird", result.CurrentNode)
        Assert.AreEqual("please, write a yes/no question to distinguish a cat from a hummingbird", result.MessageFromEngine)


   [<Test>]
     let ``will ask the answer to the quesiton that separates the curent leaf node with the animal that the user was thinking of``()=
        // arrange

        let playStart = {(defaultInitState (AnimalName "hummingbird")) with MessageFromPlayer = Some "does it fly?"; CurrentState=ExpectingDiscriminatingQuestion; AnimalToBeLearned = "cat"}


        let tree = AnimalName "hummingbird"

        // act
        let result = consoleInteract playStart

        // assert
        Assert.AreEqual(AnsweringDiscriminatingQuestion,  result.CurrentState)
        Assert.AreEqual("what is the answer to the question \"does it fly?\" to distinguish a cat from a hummingbird?", result.MessageFromEngine)



   [<Test>]
     let ``after  third interaction, when the state is GuessingFromCurrentNode, and it asked if it is the animal of the root, I anser different from yes/no, and it will remind me to use yes/no``()=
        // arrange

        let playStart = {(defaultInitState (AnimalName "hummingbird" )) with CurrentState = GuessingFromCurrentNode; MessageFromPlayer = Some "ni/yeou"}
        let tree = AnimalName "hummingbird"

        // act
        let result = consoleInteract playStart

        // assert
        Assert.AreEqual(GuessingFromCurrentNode,  result.CurrentState)
        Assert.AreEqual("please answer only yes or no. is it a hummingbird?", result.MessageFromEngine)


   [<Test>]
     let ``will ask the root question when there is a non leaf tree``()=
        // arrange
        let tree = {Question="is it big?";YesBranch=AnimalName "elephant";NoBranch=AnimalName "cat"}
        let playStart = {(defaultInitState (SubTree tree)) with CurrentState = GuessingFromCurrentNode; MessageFromPlayer = Some "ni/yeou"}

        // act
        let result = consoleInteract playStart

        // assert
        Assert.AreEqual("please answer only yes or no. is it big?", result.MessageFromEngine)


 [<Test>]
    let ``a long interaction 1``() =
        let tree = AnimalName "cat"

        let playStart = {(defaultInitState (AnimalName "cat")) with CurrentState = Welcome; MessageFromPlayer = Some ""}

        let mutable result = consoleInteract playStart 

        Assert.AreEqual("think about an animal",result.MessageFromEngine)
        Assert.AreEqual(InviteToThinkAboutAnAnimal,result.CurrentState)

        result <- consoleInteract result
        Assert.AreEqual(GuessingFromCurrentNode,result.CurrentState)
        Assert.AreEqual("is it a cat?",result.MessageFromEngine)

        result.MessageFromPlayer <- Some "no" 
        result <- consoleInteract result

        Assert.AreEqual("what animal was?",result.MessageFromEngine)
        Assert.AreEqual(AskWhatAnimalWas,result.CurrentState)

        let playStart = { ConversationToken = None;  MessageFromEngine="";MessageFromPlayer= Some "elephant";CurrentState=AskWhatAnimalWas;AnimalToBeLearned="";RootTree= tree;CurrentNode= tree ; YesNoList=[];NewDiscriminatingQuestion=None} 
        result.MessageFromPlayer <- Some "elephant"
        result <- consoleInteract result

        Assert.AreEqual(ExpectingDiscriminatingQuestion,result.CurrentState )
        Assert.AreEqual("please, write a yes/no question to distinguish a elephant from a cat",result.MessageFromEngine)
        Assert.AreEqual("elephant",result.AnimalToBeLearned)


 [<Test>]
    let ``a cycle of learning a new node from a single leaf``() =
        let tree = AnimalName "cat" 

        let playStart = {(defaultInitState tree) with CurrentState = Welcome; MessageFromPlayer = Some ""}

        let mutable result = consoleInteract playStart 

        Assert.AreEqual("think about an animal",result.MessageFromEngine)
        Assert.AreEqual(InviteToThinkAboutAnAnimal,result.CurrentState)

        result <- consoleInteract result
        Assert.AreEqual(GuessingFromCurrentNode,result.CurrentState)
        Assert.AreEqual("is it a cat?",result.MessageFromEngine)

        result <- consoleInteract { result with MessageFromPlayer = Some "no" }

        Assert.AreEqual("what animal was?",result.MessageFromEngine)
        Assert.AreEqual(AskWhatAnimalWas,result.CurrentState)

        result <- consoleInteract { result with MessageFromPlayer = Some "elephant" }

        Assert.AreEqual(ExpectingDiscriminatingQuestion,result.CurrentState )
        Assert.AreEqual("please, write a yes/no question to distinguish a elephant from a cat",result.MessageFromEngine)
        Assert.AreEqual("elephant",result.AnimalToBeLearned)

        result <- consoleInteract { result with MessageFromPlayer = Some "is it big?" }

        Assert.AreEqual("what is the answer to the question \"is it big?\" to distinguish a elephant from a cat?",result.MessageFromEngine)
        Assert.AreEqual(AnsweringDiscriminatingQuestion,result.CurrentState)

        result <- consoleInteract  { result with MessageFromPlayer = Some "yes" }

        Assert.AreEqual(Welcome,result.CurrentState)    
        Assert.AreEqual("ok",result.MessageFromEngine)

        Assert.AreEqual(SubTree {Question="is it big?"; YesBranch = AnimalName "elephant"; NoBranch = AnimalName "cat"},result.RootTree)



 [<Test>]
    let ``a cycle of learning a new node from a two leafes tree``() =
        let tree = SubTree {Question="is it big?";
                                YesBranch = AnimalName "elephant"; 
                                NoBranch = AnimalName "cat"}

        let playStart = {(defaultInitState tree) with CurrentState = Welcome} 

        let mutable result = consoleInteract playStart

        Assert.AreEqual("think about an animal",result.MessageFromEngine)
        Assert.AreEqual(InviteToThinkAboutAnAnimal,result.CurrentState)

        result <- consoleInteract result

        Assert.AreEqual(GuessingFromCurrentNode,result.CurrentState)
        Assert.AreEqual("is it big?",result.MessageFromEngine)

        result <- consoleInteract { result with MessageFromPlayer = Some "no" }

        Assert.AreEqual(GuessingFromCurrentNode,result.CurrentState)
        Assert.AreEqual("is it a cat?",result.MessageFromEngine)
        Assert.AreEqual(["no"],result.YesNoList)

        result <- consoleInteract { result with MessageFromPlayer = Some "no" }

        Assert.AreEqual("what animal was?",result.MessageFromEngine)
        Assert.AreEqual(AskWhatAnimalWas,result.CurrentState)

        result <- consoleInteract { result with MessageFromPlayer = Some "mouse" }

        Assert.AreEqual(ExpectingDiscriminatingQuestion,result.CurrentState )
        Assert.AreEqual("please, write a yes/no question to distinguish a mouse from a cat",result.MessageFromEngine)
        Assert.AreEqual("mouse",result.AnimalToBeLearned)
        Assert.AreEqual(["no"],result.YesNoList)

        result <- consoleInteract { result with MessageFromPlayer = Some "is it a rodent?" }

        Assert.AreEqual("what is the answer to the question \"is it a rodent?\" to distinguish a mouse from a cat?",result.MessageFromEngine)
        Assert.AreEqual(AnsweringDiscriminatingQuestion,result.CurrentState)

        result <- consoleInteract  { result with MessageFromPlayer = Some "yes" }

        Assert.AreEqual(Welcome,result.CurrentState)    
        Assert.AreEqual("ok",result.MessageFromEngine)

        Assert.AreEqual(SubTree {Question="is it big?"; YesBranch = AnimalName "elephant"; NoBranch = SubTree {Question="is it a rodent?"; YesBranch= AnimalName "mouse";NoBranch=AnimalName "cat"}},result.RootTree)


 [<Test>]
    let ``a cycle of learning a new node from a two leafes tree expanding the yesBranch``() =
        let tree = SubTree {Question="is it big?"; 
                                YesBranch = AnimalName "elephant";
                                NoBranch = AnimalName "cat"}

        let playStart = {(defaultInitState tree)  with CurrentState = Welcome} 

        let mutable result = consoleInteract playStart

        Assert.AreEqual("think about an animal",result.MessageFromEngine)
        Assert.AreEqual(InviteToThinkAboutAnAnimal,result.CurrentState)

        result <- consoleInteract result

        Assert.AreEqual(GuessingFromCurrentNode,result.CurrentState)
        Assert.AreEqual("is it big?",result.MessageFromEngine)

        result <- consoleInteract { result with MessageFromPlayer = Some "yes" }

        Assert.AreEqual(GuessingFromCurrentNode,result.CurrentState)
        Assert.AreEqual("is it a elephant?",result.MessageFromEngine)
        Assert.AreEqual(["yes"],result.YesNoList)

        result <- consoleInteract { result with MessageFromPlayer = Some "no" }

        Assert.AreEqual("what animal was?",result.MessageFromEngine)
        Assert.AreEqual(AskWhatAnimalWas,result.CurrentState)

        result <- consoleInteract { result with MessageFromPlayer = Some "whale" }

        Assert.AreEqual(ExpectingDiscriminatingQuestion,result.CurrentState )
        Assert.AreEqual("please, write a yes/no question to distinguish a whale from a elephant",result.MessageFromEngine)
        Assert.AreEqual("whale",result.AnimalToBeLearned)
        Assert.AreEqual(["yes"],result.YesNoList)

        result <- consoleInteract { result with MessageFromPlayer = Some "is it a cetacea?" }

        Assert.AreEqual("what is the answer to the question \"is it a cetacea?\" to distinguish a whale from a elephant?",result.MessageFromEngine)
        Assert.AreEqual(AnsweringDiscriminatingQuestion,result.CurrentState)

        result <- consoleInteract  { result with MessageFromPlayer = Some "yes" }

        Assert.AreEqual(Welcome,result.CurrentState)    
        Assert.AreEqual("ok",result.MessageFromEngine)

        Assert.AreEqual(SubTree {Question="is it big?"; YesBranch = SubTree {Question="is it a cetacea?"; YesBranch= AnimalName "whale";NoBranch=AnimalName "elephant"};NoBranch=AnimalName "cat"},result.RootTree)

        result <- consoleInteract { result with MessageFromPlayer = None }

        Assert.AreEqual("think about an animal",result.MessageFromEngine)
        Assert.AreEqual(InviteToThinkAboutAnAnimal,result.CurrentState)

        result <- consoleInteract { result with MessageFromPlayer = Some "" }

        Assert.AreEqual("is it big?",result.MessageFromEngine)
        Assert.AreEqual(GuessingFromCurrentNode,result.CurrentState)



 [<Test>]
    let ``navigate through in the no part of the yes/no branch``() =
        let tree = SubTree {Question="is it big?"; YesBranch = AnimalName "elephant"; NoBranch = AnimalName "cat"}

        let playStart = {(defaultInitState tree)  with CurrentState = Welcome} 

        let mutable result = consoleInteract playStart 

        Assert.AreEqual("think about an animal",result.MessageFromEngine)
        Assert.AreEqual(InviteToThinkAboutAnAnimal,result.CurrentState)

        result <- consoleInteract result
        Assert.AreEqual(GuessingFromCurrentNode,result.CurrentState)
        Assert.AreEqual("is it big?",result.MessageFromEngine)

        result <- consoleInteract { result with MessageFromPlayer = Some "no" }

        Assert.AreEqual(GuessingFromCurrentNode,result.CurrentState)
        Assert.AreEqual(AnimalName "cat",result.CurrentNode)
        Assert.AreEqual(["no"],result.YesNoList)



    [<Test>]
    let ``navigate through in the yes part of the yes/no branch``() =
        let tree = SubTree {Question="is it big?"; 
                                YesBranch = AnimalName "elephant"; 
                                NoBranch = AnimalName "cat"}

        let playStart = {(defaultInitState tree)  with CurrentState = Welcome} 

        let mutable result = consoleInteract playStart 

        Assert.AreEqual("think about an animal",result.MessageFromEngine)
        Assert.AreEqual(InviteToThinkAboutAnAnimal,result.CurrentState)

        result <- consoleInteract result
        Assert.AreEqual(GuessingFromCurrentNode,result.CurrentState)
        Assert.AreEqual("is it big?",result.MessageFromEngine)

        result <- consoleInteract { result with MessageFromPlayer = Some "yes" }

        Assert.AreEqual(GuessingFromCurrentNode,result.CurrentState)
        Assert.AreEqual(AnimalName "elephant",result.CurrentNode)
        Assert.AreEqual(["yes"],result.YesNoList)

 [<Test>]
    let ``learning new animal when discriminating question is yes``() =
        let tree = AnimalName "cat"

        let playStart = {(defaultInitState tree)  with CurrentState = Welcome} 

        let mutable result = consoleInteract playStart 

        Assert.AreEqual("think about an animal",result.MessageFromEngine)
        Assert.AreEqual(InviteToThinkAboutAnAnimal,result.CurrentState)

        result <- consoleInteract result
        Assert.AreEqual(GuessingFromCurrentNode,result.CurrentState)
        Assert.AreEqual("is it a cat?",result.MessageFromEngine)

        result <- consoleInteract { result with MessageFromPlayer = Some "no" }

        Assert.AreEqual("what animal was?",result.MessageFromEngine)
        Assert.AreEqual(AskWhatAnimalWas,result.CurrentState)

        result <- consoleInteract { result with MessageFromPlayer = Some "elephant" }

        Assert.AreEqual(ExpectingDiscriminatingQuestion,result.CurrentState )
        Assert.AreEqual("please, write a yes/no question to distinguish a elephant from a cat",result.MessageFromEngine)
        Assert.AreEqual("elephant",result.AnimalToBeLearned)

        result <- consoleInteract { result with MessageFromPlayer = Some "is it big?" }

        Assert.AreEqual("what is the answer to the question \"is it big?\" to distinguish a elephant from a cat?",result.MessageFromEngine)
        Assert.AreEqual(AnsweringDiscriminatingQuestion,result.CurrentState)
        Assert.AreEqual(Some "is it big?",result.NewDiscriminatingQuestion)

        result <- consoleInteract  { result with MessageFromPlayer = Some "yes" }

        Assert.AreEqual(Welcome,result.CurrentState) 
        Assert.AreEqual("ok",result.MessageFromEngine)
        Assert.AreEqual(SubTree {Question="is it big?"; YesBranch = AnimalName "elephant"; NoBranch = AnimalName "cat"},result.RootTree)

        Assert.IsTrue([] = result.YesNoList)


 [<Test>]
    let ``descending the tree in making questions``() =
        let tree = SubTree {Question="is it small?"; YesBranch = AnimalName "cat"; NoBranch = AnimalName "elephant"}
        let playStart = {(defaultInitState tree)  with CurrentState = Welcome} 

        let mutable result = consoleInteract playStart 

        Assert.AreEqual("think about an animal",result.MessageFromEngine)
        Assert.AreEqual(InviteToThinkAboutAnAnimal,result.CurrentState)

        result <- consoleInteract result
        Assert.AreEqual(GuessingFromCurrentNode,result.CurrentState)
        Assert.AreEqual("is it small?",result.MessageFromEngine)

        Assert.AreEqual(SubTree {Question="is it small?"; YesBranch = AnimalName "cat"; NoBranch = AnimalName "elephant"},result.CurrentNode)

        result <- consoleInteract { result with MessageFromPlayer = Some "no" }

        Assert.AreEqual("is it a elephant?",result.MessageFromEngine)
        Assert.AreEqual(GuessingFromCurrentNode,result.CurrentState)

        result <- consoleInteract { result  with MessageFromPlayer = Some "no" }

        Assert.AreEqual(AskWhatAnimalWas,result.CurrentState)
        Assert.AreEqual("what animal was?",result.MessageFromEngine)


        result <- consoleInteract { result with MessageFromPlayer = Some "whale" }

        Assert.AreEqual("please, write a yes/no question to distinguish a whale from a elephant",result.MessageFromEngine)

        result.MessageFromPlayer <- Some "is it a cetaceus?"

        result <- consoleInteract result

        Assert.AreEqual(AnsweringDiscriminatingQuestion,result.CurrentState)    

        Assert.AreEqual("what is the answer to the question \"is it a cetaceus?\" to distinguish a whale from a elephant?",result.MessageFromEngine)

        Assert.AreEqual(["no"],result.YesNoList)
        Assert.AreEqual(AnsweringDiscriminatingQuestion,result.CurrentState)    

        result <- consoleInteract { result with MessageFromPlayer = Some "yes"}

        let expectedResultTree = SubTree {Question="is it small?"; YesBranch = AnimalName "cat"; NoBranch = SubTree {Question="is it a cetaceus?"; YesBranch = AnimalName "whale"; NoBranch = AnimalName "elephant"}}

        Assert.AreEqual(expectedResultTree,result.RootTree)





[<Test>]
    let ``deep descending the tree (reproducing bug)``() =
        let tree = AnimalName "elephant"
        
        //SubTree {Question="is it small?"; yesBranch = AnimalName "cat"; noBranch = AnimalName "elephant"}
        let playStart = {(defaultInitState tree)  with CurrentState = Welcome} 

        let mutable result = consoleInteract playStart 

        Assert.AreEqual("think about an animal",result.MessageFromEngine)
        Assert.AreEqual(InviteToThinkAboutAnAnimal,result.CurrentState)

        result <- consoleInteract result
        Assert.AreEqual(GuessingFromCurrentNode,result.CurrentState)
        Assert.AreEqual("is it a elephant?",result.MessageFromEngine)

        result <- consoleInteract { result with MessageFromPlayer = Some "no" }

        Assert.AreEqual(AskWhatAnimalWas,result.CurrentState)
        Assert.AreEqual("what animal was?",result.MessageFromEngine)

        result <- consoleInteract { result with MessageFromPlayer = Some "cat" }

        Assert.AreEqual("please, write a yes/no question to distinguish a cat from a elephant",result.MessageFromEngine)


        //result <- consoleInteract result
        result <- consoleInteract {result with MessageFromPlayer = Some "is it small?"}

        Assert.AreEqual(AnsweringDiscriminatingQuestion,result.CurrentState)    

        Assert.AreEqual("what is the answer to the question \"is it small?\" to distinguish a cat from a elephant?",result.MessageFromEngine)

        //Assert.AreEqual([],result.yesNoList)
        Assert.AreEqual(AnsweringDiscriminatingQuestion,result.CurrentState)    

        result <- consoleInteract { result with MessageFromPlayer = Some "yes"}

        let expectedResultTree = SubTree {Question="is it small?"; YesBranch = AnimalName "cat"; NoBranch = AnimalName "elephant"}

        // check this one:
        //Assert.AreEqual(["yes"],result.yesNoList)

        Assert.AreEqual(expectedResultTree,result.RootTree)

        Assert.AreEqual(Welcome,result.CurrentState)

        result <- consoleInteract result 

        Assert.AreEqual("think about an animal",result.MessageFromEngine)
        Assert.AreEqual(InviteToThinkAboutAnAnimal,result.CurrentState)

        result <- consoleInteract result
        Assert.AreEqual(GuessingFromCurrentNode,result.CurrentState)
        Assert.AreEqual("is it small?",result.MessageFromEngine)

        result <- consoleInteract { result with MessageFromPlayer = Some "yes"}

        //result <- consoleInteract result
        Assert.AreEqual(GuessingFromCurrentNode,result.CurrentState)
        Assert.AreEqual("is it a cat?",result.MessageFromEngine)

        result <- consoleInteract { result with MessageFromPlayer = Some "no"}

        Assert.AreEqual(AskWhatAnimalWas,result.CurrentState)
        Assert.AreEqual("what animal was?",result.MessageFromEngine)

        result <- consoleInteract { result with MessageFromPlayer = Some "mouse"}

        Assert.AreEqual("please, write a yes/no question to distinguish a mouse from a cat",result.MessageFromEngine)

        result <- consoleInteract {result with MessageFromPlayer = Some "is it clean?"}

        Assert.AreEqual(AnsweringDiscriminatingQuestion,result.CurrentState)    

        Assert.AreEqual("what is the answer to the question \"is it clean?\" to distinguish a mouse from a cat?",result.MessageFromEngine)

        Assert.AreEqual(AnsweringDiscriminatingQuestion,result.CurrentState)    

        result <- consoleInteract { result with MessageFromPlayer = Some "no"}

        let expectedResultTree = SubTree {Question="is it small?"; YesBranch =  SubTree {Question="is it clean?"; YesBranch= AnimalName "cat";NoBranch = AnimalName "mouse"}; NoBranch = AnimalName "elephant"}

        Assert.AreEqual(expectedResultTree,result.RootTree)

        Assert.AreEqual(Welcome,result.CurrentState)

        result <- consoleInteract result 

        Assert.AreEqual("think about an animal",result.MessageFromEngine)
        Assert.AreEqual(InviteToThinkAboutAnAnimal,result.CurrentState)

        result <- consoleInteract result
        Assert.AreEqual(GuessingFromCurrentNode,result.CurrentState)
        Assert.AreEqual("is it small?",result.MessageFromEngine)

        result <- consoleInteract { result with MessageFromPlayer = Some "yes"}

        Assert.AreEqual(GuessingFromCurrentNode,result.CurrentState)
        Assert.AreEqual("is it clean?",result.MessageFromEngine)

        result <- consoleInteract { result with MessageFromPlayer = Some "yes"}

        Assert.AreEqual(GuessingFromCurrentNode,result.CurrentState)
        Assert.AreEqual("is it a cat?",result.MessageFromEngine)

        result <- consoleInteract { result with MessageFromPlayer = Some "no"}

        Assert.AreEqual(AskWhatAnimalWas,result.CurrentState)
        Assert.AreEqual("what animal was?",result.MessageFromEngine)

        result <- consoleInteract { result with MessageFromPlayer = Some "ant"}

        Assert.AreEqual("please, write a yes/no question to distinguish a ant from a cat",result.MessageFromEngine)

        result <- consoleInteract { result with MessageFromPlayer = Some "is it an insect?"}

        Assert.AreEqual(AnsweringDiscriminatingQuestion,result.CurrentState)    

        Assert.AreEqual("what is the answer to the question \"is it an insect?\" to distinguish a ant from a cat?",result.MessageFromEngine)

        result <- consoleInteract { result with MessageFromPlayer = Some "yes"}

        let expectedResultTree = SubTree { Question="is it small?"; YesBranch =  
           SubTree {Question="is it clean?"; YesBranch= SubTree {Question="is it an insect?";YesBranch= AnimalName "ant";NoBranch= AnimalName "cat"};
           NoBranch= AnimalName "mouse"};NoBranch= AnimalName "elephant"}

        Assert.AreEqual(expectedResultTree,result.RootTree)

        printf "%s\n" ("actual: " + (printTree result.RootTree) )

        printf "%s\n" ("expected: " + (printTree expectedResultTree) )
        Assert.IsTrue(true)



     
    