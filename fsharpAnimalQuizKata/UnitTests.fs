 
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

    [<Test>]
      let ``before starting will not have any conversation token``()=
        // arrange
        let playStart = defaultInitState (AnimalName "monkey")

        let noToken = match playStart.conversationToken with | None -> true | Some X -> false

        Assert.IsTrue(noToken)

    [<Test>]
      let ``at first interaction will receive a new token``()=
        // arrange
        let playStart = defaultInitState (AnimalName "monkey")

        // act
        let result = consoleInteract playStart

        // assert
        let hasToken = match result.conversationToken with | None -> false | Some _ -> true
        Assert.IsTrue(hasToken)



    [<Test>]
     let ``at the initial state, Welcome, the user receive the message "please, think about an animal", and state becomes 'invitethinkaboutanimal ``()=
        // arrange
        let playStart = defaultInitState (AnimalName "monkey")

        // act
        let result = consoleInteract playStart

        // assert
        Assert.AreEqual(InviteToThinkAboutAnAnimal,  result.currentState)
        Assert.AreEqual("think about an animal", result.messageFromEngine)
               
                     
   [<Test>]
     let ``when the state is InviteToThinkAboutAnAnimal, and the current node is leaf will ask if it is the animal on the current note, and the state is Guessingfromcurrentnode``()=
        // arrange
        let playStart = {(defaultInitState (AnimalName "cat")) with currentState = InviteToThinkAboutAnAnimal;  conversationToken=Some "token"}

        // act
        let resultUsingStructure = consoleInteract playStart

        // assert
        Assert.AreEqual("is it a cat?",resultUsingStructure.messageFromEngine)
        Assert.AreEqual(GuessingFromCurrentNode,  resultUsingStructure.currentState)

   [<Test>]
     let ``when the state is InviteToThinkAboutAnAnimal, and the current node is not leaf, will ask the node question, and the state is guessingfromcurrentnode``()=
        // arrange
        let tree = SubTree {Question="is it big?"; YesBranch = AnimalName "elephant"; NoBranch = AnimalName "cat"}
        let playStart = {(defaultInitState  tree) with currentState = InviteToThinkAboutAnAnimal; conversationToken = Some "token"}

        // act
        let resultUsingStructure = consoleInteract playStart

        // assert
        Assert.AreEqual("is it big?",resultUsingStructure.messageFromEngine)
        Assert.AreEqual(GuessingFromCurrentNode,  resultUsingStructure.currentState)

   [<Test>]
     let ``when is guessing from current non leaf node, a yes answer will make the current node being the yesBranch, and the yesNo history list will remember the "yes" answer ``()=
        // arrange
        let tree = SubTree {Question="is it big?"; YesBranch = AnimalName "elephant"; NoBranch = AnimalName "cat"}
        let playStart = {(defaultInitState  tree) with currentState = GuessingFromCurrentNode; messageFromPlayer = Some "yes"}

        // act
        let resultUsingStructure = consoleInteract playStart

        // assert
        Assert.AreEqual("is it a elephant?",resultUsingStructure.messageFromEngine)
        Assert.AreEqual(GuessingFromCurrentNode,  resultUsingStructure.currentState)
        Assert.AreEqual(AnimalName "elephant",resultUsingStructure.currentNode)
        Assert.AreEqual(["yes"],resultUsingStructure.yesNoList)

   [<Test>]
     let ``if the current node is leaf, and we are in GuessingFromCurrentNode, a "yes" answer will make the system cheers with a "yeah!"``()=
        // arrange
        let playStart = {(defaultInitState (AnimalName "cat")) with messageFromPlayer = Some "yes"; currentState=GuessingFromCurrentNode}

        // act
        let result = consoleInteract playStart

        // assert
        Assert.AreEqual(Welcome,  result.currentState)
        Assert.AreEqual("yeah!", result.messageFromEngine)


   [<Test>]
     let ``When the state is GuessingFromCurrentNode, and the current node is leaf, if I say no, it will ask me what animal was``()=
        // arrange
        let playStart = {(defaultInitState (AnimalName "cat")) with messageFromPlayer = Some "no"; currentState=GuessingFromCurrentNode}

        // act
        let result = consoleInteract playStart

        // assert
        Assert.AreEqual(AskWhatAnimalWas,  result.currentState)
        Assert.AreEqual("what animal was?", result.messageFromEngine)

   [<Test>]
     let ``after asking what animal was, will ask me to suggest the yes/no question that can be used to distinguish the new animal with the anmial of the current node``()=
        // arrange
        let playStart = {(defaultInitState (AnimalName "hummingbird")) with messageFromPlayer = Some "cat"; currentState=AskWhatAnimalWas}


        // act
        let result = consoleInteract playStart

        // assert
        Assert.AreEqual(ExpectingDiscriminatingQuestion,  result.currentState)
        Assert.AreEqual(AnimalName "hummingbird", result.currentNode)
        Assert.AreEqual("please, write a yes/no question to distinguish a cat from a hummingbird", result.messageFromEngine)


   [<Test>]
     let ``will ask the answer to the quesiton that separates the curent leaf node with the animal that the user was thinking of``()=
        // arrange

        let playStart = {(defaultInitState (AnimalName "hummingbird")) with messageFromPlayer = Some "does it fly?"; currentState=ExpectingDiscriminatingQuestion; animalToBeLearned = "cat"}


        let tree = AnimalName "hummingbird"

        // act
        let result = consoleInteract playStart

        // assert
        Assert.AreEqual(AnsweringDiscriminatingQuestion,  result.currentState)
        Assert.AreEqual("what is the answer to the question \"does it fly?\" to distinguish a cat from a hummingbird?", result.messageFromEngine)



   [<Test>]
     let ``after  third interaction, when the state is GuessingFromCurrentNode, and it asked if it is the animal of the root, I anser different from yes/no, and it will remind me to use yes/no``()=
        // arrange

        let playStart = {(defaultInitState (AnimalName "hummingbird" )) with currentState = GuessingFromCurrentNode; messageFromPlayer = Some "ni/yeou"}
        let tree = AnimalName "hummingbird"

        // act
        let result = consoleInteract playStart

        // assert
        Assert.AreEqual(GuessingFromCurrentNode,  result.currentState)
        Assert.AreEqual("please answer only yes or no. is it a hummingbird?", result.messageFromEngine)


   [<Test>]
     let ``will ask the root question when there is a non leaf tree``()=
        // arrange
        let tree = {Question="is it big?";YesBranch=AnimalName "elephant";NoBranch=AnimalName "cat"}
        let playStart = {(defaultInitState (SubTree tree)) with currentState = GuessingFromCurrentNode; messageFromPlayer = Some "ni/yeou"}

        // act
        let result = consoleInteract playStart

        // assert
        Assert.AreEqual("please answer only yes or no. is it big?", result.messageFromEngine)


 [<Test>]
    let ``a long interaction 1``() =
        let tree = AnimalName "cat"

        let playStart = {(defaultInitState (AnimalName "cat")) with currentState = Welcome; messageFromPlayer = Some ""}

        let mutable result = consoleInteract playStart 

        Assert.AreEqual("think about an animal",result.messageFromEngine)
        Assert.AreEqual(InviteToThinkAboutAnAnimal,result.currentState)

        result <- consoleInteract result
        Assert.AreEqual(GuessingFromCurrentNode,result.currentState)
        Assert.AreEqual("is it a cat?",result.messageFromEngine)

        result.messageFromPlayer <- Some "no" 
        result <- consoleInteract result

        Assert.AreEqual("what animal was?",result.messageFromEngine)
        Assert.AreEqual(AskWhatAnimalWas,result.currentState)

        let playStart = { conversationToken = None;  messageFromEngine="";messageFromPlayer= Some "elephant";currentState=AskWhatAnimalWas;animalToBeLearned="";rootTree= tree;currentNode= tree ; yesNoList=[];newDiscriminatingQuestion=None} 
        result.messageFromPlayer <- Some "elephant"
        result <- consoleInteract result

        Assert.AreEqual(ExpectingDiscriminatingQuestion,result.currentState )
        Assert.AreEqual("please, write a yes/no question to distinguish a elephant from a cat",result.messageFromEngine)
        Assert.AreEqual("elephant",result.animalToBeLearned)


 [<Test>]
    let ``a cycle of learning a new node from a single leaf``() =
        let tree = AnimalName "cat" 

        let playStart = {(defaultInitState tree) with currentState = Welcome; messageFromPlayer = Some ""}

        let mutable result = consoleInteract playStart 

        Assert.AreEqual("think about an animal",result.messageFromEngine)
        Assert.AreEqual(InviteToThinkAboutAnAnimal,result.currentState)

        result <- consoleInteract result
        Assert.AreEqual(GuessingFromCurrentNode,result.currentState)
        Assert.AreEqual("is it a cat?",result.messageFromEngine)

        result <- consoleInteract { result with messageFromPlayer = Some "no" }

        Assert.AreEqual("what animal was?",result.messageFromEngine)
        Assert.AreEqual(AskWhatAnimalWas,result.currentState)

        result <- consoleInteract { result with messageFromPlayer = Some "elephant" }

        Assert.AreEqual(ExpectingDiscriminatingQuestion,result.currentState )
        Assert.AreEqual("please, write a yes/no question to distinguish a elephant from a cat",result.messageFromEngine)
        Assert.AreEqual("elephant",result.animalToBeLearned)

        result <- consoleInteract { result with messageFromPlayer = Some "is it big?" }

        Assert.AreEqual("what is the answer to the question \"is it big?\" to distinguish a elephant from a cat?",result.messageFromEngine)
        Assert.AreEqual(AnsweringDiscriminatingQuestion,result.currentState)

        result <- consoleInteract  { result with messageFromPlayer = Some "yes" }

        Assert.AreEqual(Welcome,result.currentState)    
        Assert.AreEqual("ok",result.messageFromEngine)

        Assert.AreEqual(SubTree {Question="is it big?"; YesBranch = AnimalName "elephant"; NoBranch = AnimalName "cat"},result.rootTree)



 [<Test>]
    let ``a cycle of learning a new node from a two leafes tree``() =
        let tree = SubTree {Question="is it big?";
                                YesBranch = AnimalName "elephant"; 
                                NoBranch = AnimalName "cat"}

        let playStart = {(defaultInitState tree) with currentState = Welcome} 

        let mutable result = consoleInteract playStart

        Assert.AreEqual("think about an animal",result.messageFromEngine)
        Assert.AreEqual(InviteToThinkAboutAnAnimal,result.currentState)

        result <- consoleInteract result

        Assert.AreEqual(GuessingFromCurrentNode,result.currentState)
        Assert.AreEqual("is it big?",result.messageFromEngine)

        result <- consoleInteract { result with messageFromPlayer = Some "no" }

        Assert.AreEqual(GuessingFromCurrentNode,result.currentState)
        Assert.AreEqual("is it a cat?",result.messageFromEngine)
        Assert.AreEqual(["no"],result.yesNoList)

        result <- consoleInteract { result with messageFromPlayer = Some "no" }

        Assert.AreEqual("what animal was?",result.messageFromEngine)
        Assert.AreEqual(AskWhatAnimalWas,result.currentState)

        result <- consoleInteract { result with messageFromPlayer = Some "mouse" }

        Assert.AreEqual(ExpectingDiscriminatingQuestion,result.currentState )
        Assert.AreEqual("please, write a yes/no question to distinguish a mouse from a cat",result.messageFromEngine)
        Assert.AreEqual("mouse",result.animalToBeLearned)
        Assert.AreEqual(["no"],result.yesNoList)

        result <- consoleInteract { result with messageFromPlayer = Some "is it a rodent?" }

        Assert.AreEqual("what is the answer to the question \"is it a rodent?\" to distinguish a mouse from a cat?",result.messageFromEngine)
        Assert.AreEqual(AnsweringDiscriminatingQuestion,result.currentState)

        result <- consoleInteract  { result with messageFromPlayer = Some "yes" }

        Assert.AreEqual(Welcome,result.currentState)    
        Assert.AreEqual("ok",result.messageFromEngine)

        Assert.AreEqual(SubTree {Question="is it big?"; YesBranch = AnimalName "elephant"; NoBranch = SubTree {Question="is it a rodent?"; YesBranch= AnimalName "mouse";NoBranch=AnimalName "cat"}},result.rootTree)


 [<Test>]
    let ``a cycle of learning a new node from a two leafes tree expanding the yesBranch``() =
        let tree = SubTree {Question="is it big?"; 
                                YesBranch = AnimalName "elephant";
                                NoBranch = AnimalName "cat"}

        let playStart = {(defaultInitState tree)  with currentState = Welcome} 

        let mutable result = consoleInteract playStart

        Assert.AreEqual("think about an animal",result.messageFromEngine)
        Assert.AreEqual(InviteToThinkAboutAnAnimal,result.currentState)

        result <- consoleInteract result

        Assert.AreEqual(GuessingFromCurrentNode,result.currentState)
        Assert.AreEqual("is it big?",result.messageFromEngine)

        result <- consoleInteract { result with messageFromPlayer = Some "yes" }

        Assert.AreEqual(GuessingFromCurrentNode,result.currentState)
        Assert.AreEqual("is it a elephant?",result.messageFromEngine)
        Assert.AreEqual(["yes"],result.yesNoList)

        result <- consoleInteract { result with messageFromPlayer = Some "no" }

        Assert.AreEqual("what animal was?",result.messageFromEngine)
        Assert.AreEqual(AskWhatAnimalWas,result.currentState)

        result <- consoleInteract { result with messageFromPlayer = Some "whale" }

        Assert.AreEqual(ExpectingDiscriminatingQuestion,result.currentState )
        Assert.AreEqual("please, write a yes/no question to distinguish a whale from a elephant",result.messageFromEngine)
        Assert.AreEqual("whale",result.animalToBeLearned)
        Assert.AreEqual(["yes"],result.yesNoList)

        result <- consoleInteract { result with messageFromPlayer = Some "is it a cetacea?" }

        Assert.AreEqual("what is the answer to the question \"is it a cetacea?\" to distinguish a whale from a elephant?",result.messageFromEngine)
        Assert.AreEqual(AnsweringDiscriminatingQuestion,result.currentState)

        result <- consoleInteract  { result with messageFromPlayer = Some "yes" }

        Assert.AreEqual(Welcome,result.currentState)    
        Assert.AreEqual("ok",result.messageFromEngine)

        Assert.AreEqual(SubTree {Question="is it big?"; YesBranch = SubTree {Question="is it a cetacea?"; YesBranch= AnimalName "whale";NoBranch=AnimalName "elephant"};NoBranch=AnimalName "cat"},result.rootTree)

        result <- consoleInteract { result with messageFromPlayer = None }

        Assert.AreEqual("think about an animal",result.messageFromEngine)
        Assert.AreEqual(InviteToThinkAboutAnAnimal,result.currentState)

        result <- consoleInteract { result with messageFromPlayer = Some "" }

        Assert.AreEqual("is it big?",result.messageFromEngine)
        Assert.AreEqual(GuessingFromCurrentNode,result.currentState)



 [<Test>]
    let ``navigate through in the no part of the yes/no branch``() =
        let tree = SubTree {Question="is it big?"; YesBranch = AnimalName "elephant"; NoBranch = AnimalName "cat"}

        let playStart = {(defaultInitState tree)  with currentState = Welcome} 

        let mutable result = consoleInteract playStart 

        Assert.AreEqual("think about an animal",result.messageFromEngine)
        Assert.AreEqual(InviteToThinkAboutAnAnimal,result.currentState)

        result <- consoleInteract result
        Assert.AreEqual(GuessingFromCurrentNode,result.currentState)
        Assert.AreEqual("is it big?",result.messageFromEngine)

        result <- consoleInteract { result with messageFromPlayer = Some "no" }

        Assert.AreEqual(GuessingFromCurrentNode,result.currentState)
        Assert.AreEqual(AnimalName "cat",result.currentNode)
        Assert.AreEqual(["no"],result.yesNoList)



    [<Test>]
    let ``navigate through in the yes part of the yes/no branch``() =
        let tree = SubTree {Question="is it big?"; 
                                YesBranch = AnimalName "elephant"; 
                                NoBranch = AnimalName "cat"}

        let playStart = {(defaultInitState tree)  with currentState = Welcome} 

        let mutable result = consoleInteract playStart 

        Assert.AreEqual("think about an animal",result.messageFromEngine)
        Assert.AreEqual(InviteToThinkAboutAnAnimal,result.currentState)

        result <- consoleInteract result
        Assert.AreEqual(GuessingFromCurrentNode,result.currentState)
        Assert.AreEqual("is it big?",result.messageFromEngine)

        result <- consoleInteract { result with messageFromPlayer = Some "yes" }

        Assert.AreEqual(GuessingFromCurrentNode,result.currentState)
        Assert.AreEqual(AnimalName "elephant",result.currentNode)
        Assert.AreEqual(["yes"],result.yesNoList)

 [<Test>]
    let ``learning new animal when discriminating question is yes``() =
        let tree = AnimalName "cat"

        let playStart = {(defaultInitState tree)  with currentState = Welcome} 

        let mutable result = consoleInteract playStart 

        Assert.AreEqual("think about an animal",result.messageFromEngine)
        Assert.AreEqual(InviteToThinkAboutAnAnimal,result.currentState)

        result <- consoleInteract result
        Assert.AreEqual(GuessingFromCurrentNode,result.currentState)
        Assert.AreEqual("is it a cat?",result.messageFromEngine)

        result <- consoleInteract { result with messageFromPlayer = Some "no" }

        Assert.AreEqual("what animal was?",result.messageFromEngine)
        Assert.AreEqual(AskWhatAnimalWas,result.currentState)

        result <- consoleInteract { result with messageFromPlayer = Some "elephant" }

        Assert.AreEqual(ExpectingDiscriminatingQuestion,result.currentState )
        Assert.AreEqual("please, write a yes/no question to distinguish a elephant from a cat",result.messageFromEngine)
        Assert.AreEqual("elephant",result.animalToBeLearned)

        result <- consoleInteract { result with messageFromPlayer = Some "is it big?" }

        Assert.AreEqual("what is the answer to the question \"is it big?\" to distinguish a elephant from a cat?",result.messageFromEngine)
        Assert.AreEqual(AnsweringDiscriminatingQuestion,result.currentState)
        Assert.AreEqual(Some "is it big?",result.newDiscriminatingQuestion)

        result <- consoleInteract  { result with messageFromPlayer = Some "yes" }

        Assert.AreEqual(Welcome,result.currentState) 
        Assert.AreEqual("ok",result.messageFromEngine)
        Assert.AreEqual(SubTree {Question="is it big?"; YesBranch = AnimalName "elephant"; NoBranch = AnimalName "cat"},result.rootTree)

        Assert.IsTrue([] = result.yesNoList)


 [<Test>]
    let ``descending the tree in making questions``() =
        let tree = SubTree {Question="is it small?"; YesBranch = AnimalName "cat"; NoBranch = AnimalName "elephant"}
        let playStart = {(defaultInitState tree)  with currentState = Welcome} 

        let mutable result = consoleInteract playStart 

        Assert.AreEqual("think about an animal",result.messageFromEngine)
        Assert.AreEqual(InviteToThinkAboutAnAnimal,result.currentState)

        result <- consoleInteract result
        Assert.AreEqual(GuessingFromCurrentNode,result.currentState)
        Assert.AreEqual("is it small?",result.messageFromEngine)

        Assert.AreEqual(SubTree {Question="is it small?"; YesBranch = AnimalName "cat"; NoBranch = AnimalName "elephant"},result.currentNode)

        result <- consoleInteract { result with messageFromPlayer = Some "no" }

        Assert.AreEqual("is it a elephant?",result.messageFromEngine)
        Assert.AreEqual(GuessingFromCurrentNode,result.currentState)

        result <- consoleInteract { result  with messageFromPlayer = Some "no" }

        Assert.AreEqual(AskWhatAnimalWas,result.currentState)
        Assert.AreEqual("what animal was?",result.messageFromEngine)


        result <- consoleInteract { result with messageFromPlayer = Some "whale" }

        Assert.AreEqual("please, write a yes/no question to distinguish a whale from a elephant",result.messageFromEngine)

        result.messageFromPlayer <- Some "is it a cetaceus?"

        result <- consoleInteract result

        Assert.AreEqual(AnsweringDiscriminatingQuestion,result.currentState)    

        Assert.AreEqual("what is the answer to the question \"is it a cetaceus?\" to distinguish a whale from a elephant?",result.messageFromEngine)

        Assert.AreEqual(["no"],result.yesNoList)
        Assert.AreEqual(AnsweringDiscriminatingQuestion,result.currentState)    

        result <- consoleInteract { result with messageFromPlayer = Some "yes"}

        let expectedResultTree = SubTree {Question="is it small?"; YesBranch = AnimalName "cat"; NoBranch = SubTree {Question="is it a cetaceus?"; YesBranch = AnimalName "whale"; NoBranch = AnimalName "elephant"}}

        Assert.AreEqual(expectedResultTree,result.rootTree)



[<Test>]
    let ``deep descending the tree (reproducing bug)``() =
        let tree = AnimalName "elephant"
        
        //SubTree {Question="is it small?"; yesBranch = AnimalName "cat"; noBranch = AnimalName "elephant"}
        let playStart = {(defaultInitState tree)  with currentState = Welcome} 

        let mutable result = consoleInteract playStart 

        Assert.AreEqual("think about an animal",result.messageFromEngine)
        Assert.AreEqual(InviteToThinkAboutAnAnimal,result.currentState)

        result <- consoleInteract result
        Assert.AreEqual(GuessingFromCurrentNode,result.currentState)
        Assert.AreEqual("is it a elephant?",result.messageFromEngine)

        result <- consoleInteract { result with messageFromPlayer = Some "no" }

        Assert.AreEqual(AskWhatAnimalWas,result.currentState)
        Assert.AreEqual("what animal was?",result.messageFromEngine)

        result <- consoleInteract { result with messageFromPlayer = Some "cat" }

        Assert.AreEqual("please, write a yes/no question to distinguish a cat from a elephant",result.messageFromEngine)


        //result <- consoleInteract result
        result <- consoleInteract {result with messageFromPlayer = Some "is it small?"}

        Assert.AreEqual(AnsweringDiscriminatingQuestion,result.currentState)    

        Assert.AreEqual("what is the answer to the question \"is it small?\" to distinguish a cat from a elephant?",result.messageFromEngine)

        //Assert.AreEqual([],result.yesNoList)
        Assert.AreEqual(AnsweringDiscriminatingQuestion,result.currentState)    

        result <- consoleInteract { result with messageFromPlayer = Some "yes"}

        let expectedResultTree = SubTree {Question="is it small?"; YesBranch = AnimalName "cat"; NoBranch = AnimalName "elephant"}

        // check this one:
        //Assert.AreEqual(["yes"],result.yesNoList)

        Assert.AreEqual(expectedResultTree,result.rootTree)

        Assert.AreEqual(Welcome,result.currentState)

        result <- consoleInteract result 

        Assert.AreEqual("think about an animal",result.messageFromEngine)
        Assert.AreEqual(InviteToThinkAboutAnAnimal,result.currentState)

        result <- consoleInteract result
        Assert.AreEqual(GuessingFromCurrentNode,result.currentState)
        Assert.AreEqual("is it small?",result.messageFromEngine)

        result <- consoleInteract { result with messageFromPlayer = Some "yes"}

        //result <- consoleInteract result
        Assert.AreEqual(GuessingFromCurrentNode,result.currentState)
        Assert.AreEqual("is it a cat?",result.messageFromEngine)

        result <- consoleInteract { result with messageFromPlayer = Some "no"}

        Assert.AreEqual(AskWhatAnimalWas,result.currentState)
        Assert.AreEqual("what animal was?",result.messageFromEngine)

        result <- consoleInteract { result with messageFromPlayer = Some "mouse"}

        Assert.AreEqual("please, write a yes/no question to distinguish a mouse from a cat",result.messageFromEngine)

        result <- consoleInteract {result with messageFromPlayer = Some "is it clean?"}

        Assert.AreEqual(AnsweringDiscriminatingQuestion,result.currentState)    

        Assert.AreEqual("what is the answer to the question \"is it clean?\" to distinguish a mouse from a cat?",result.messageFromEngine)

        Assert.AreEqual(AnsweringDiscriminatingQuestion,result.currentState)    

        result <- consoleInteract { result with messageFromPlayer = Some "no"}

        let expectedResultTree = SubTree {Question="is it small?"; YesBranch =  SubTree {Question="is it clean?"; YesBranch= AnimalName "cat";NoBranch = AnimalName "mouse"}; NoBranch = AnimalName "elephant"}

        Assert.AreEqual(expectedResultTree,result.rootTree)

        Assert.AreEqual(Welcome,result.currentState)

        result <- consoleInteract result 

        Assert.AreEqual("think about an animal",result.messageFromEngine)
        Assert.AreEqual(InviteToThinkAboutAnAnimal,result.currentState)

        result <- consoleInteract result
        Assert.AreEqual(GuessingFromCurrentNode,result.currentState)
        Assert.AreEqual("is it small?",result.messageFromEngine)

        result <- consoleInteract { result with messageFromPlayer = Some "yes"}

        Assert.AreEqual(GuessingFromCurrentNode,result.currentState)
        Assert.AreEqual("is it clean?",result.messageFromEngine)

        result <- consoleInteract { result with messageFromPlayer = Some "yes"}

        Assert.AreEqual(GuessingFromCurrentNode,result.currentState)
        Assert.AreEqual("is it a cat?",result.messageFromEngine)

        result <- consoleInteract { result with messageFromPlayer = Some "no"}

        Assert.AreEqual(AskWhatAnimalWas,result.currentState)
        Assert.AreEqual("what animal was?",result.messageFromEngine)

        result <- consoleInteract { result with messageFromPlayer = Some "ant"}

        Assert.AreEqual("please, write a yes/no question to distinguish a ant from a cat",result.messageFromEngine)

        result <- consoleInteract { result with messageFromPlayer = Some "is it an insect?"}

        Assert.AreEqual(AnsweringDiscriminatingQuestion,result.currentState)    

        Assert.AreEqual("what is the answer to the question \"is it an insect?\" to distinguish a ant from a cat?",result.messageFromEngine)

        result <- consoleInteract { result with messageFromPlayer = Some "yes"}

        let expectedResultTree = SubTree { Question="is it small?"; YesBranch =  
           SubTree {Question="is it clean?"; YesBranch= SubTree {Question="is it an insect?";YesBranch= AnimalName "ant";NoBranch= AnimalName "cat"};
           NoBranch= AnimalName "mouse"};NoBranch= AnimalName "elephant"}

        Assert.AreEqual(expectedResultTree,result.rootTree)

        printf "%s\n" ("actual: " + (printTree result.rootTree) )

        printf "%s\n" ("expected: " + (printTree expectedResultTree) )
        Assert.IsTrue(true)


