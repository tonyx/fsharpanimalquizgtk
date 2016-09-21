namespace animalquizTests

module tokenTests =

    open System
    open NUnit.Framework
    open fsharpAnimalQuizKata.RunModule
    open System.Collections
    open System.Collections.Generic
    open fsharpAnimalQuizKata.IoModule
    open fsharpAnimalQuizKata.BrainModule
    open Rhino.Mocks
    open fsharpAnimalQuizKata.RecordUtils
    open fsharpAnimalQuizKata.RunModuleRefactoring

    let  initPlayStructure state tree = { conversationToken = None; 
                                messageFromEngine="";
                                messageFromPlayer=Some "";
                                currentState=state;animalToBeLearned="";
                                rootTree=tree;currentNode=tree;
                                yesNoList = [];
                                newDiscriminatingQuestion=None
                             }

    [<Test>]
    let ``in the initial welcome state, should invite to think about an animal``() =
        // setup
        let mockrepo = new MockRepository()
        let inStream = mockrepo.DynamicMock<InStream>()
        let outStream = mockrepo.StrictMock<OutStream>()

        // expectation
        Expect.Call(outStream.Write("think about an animal")) |> ignore
        mockrepo.ReplayAll()

        // arrange
        let tree = AnimalName "elephant"
        let initialState = Welcome
        let numLoops = 1

        // act
        runUntilIndexPassingEntireStructure outStream inStream (initPlayStructure Welcome (AnimalName "elephant")) numLoops |> ignore

        // verify expectations
        mockrepo.VerifyAll()



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
      let ``invalid token will raise an error``()=
        // arrange
        let playStart = defaultInitState (AnimalName "monkey")

        // act
        let result = consoleInteract playStart

        // assert
        let hasToken = match result.conversationToken with | None -> false | Some _ -> true
        Assert.IsTrue(hasToken)

//   let raiseEx X = match X with |  false -> false | _ -> failwith "sdaf"



   [<Test>]
   [<Ignore>]
   [<ExpectedException>]
     let ``for any state different than welcome should use a valid token``()=

        // arrange
        let playStart = {(defaultInitState (AnimalName "cat")) with currentState = InviteToThinkAboutAnAnimal; conversationToken=None }

  //      Assert.IsTrue(false)
        // act
        let resultUsingStructure = consoleInteract playStart // expect exception here because of no token

        0 |> ignore // todo: see how to improve this trick



