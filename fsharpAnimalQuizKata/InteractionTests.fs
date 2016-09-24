namespace animalquizInteractionTests

module tests =

    open System
    open NUnit.Framework
    open fsharpAnimalQuizKata.RunModule
    open System.Collections
    open System.Collections.Generic
    open fsharpAnimalQuizKata.IoModule
    open fsharpAnimalQuizKata.BrainModule
    open Rhino.Mocks
    open fsharpAnimalQuizKata.RecordUtils

    let  initPlayStructure  = { ConversationToken = None; 
                                MessageFromEngine="";
                                MessageFromPlayer=Some "";
                                CurrentState=Welcome;AnimalToBeLearned="";
                                RootTree=AnimalName "elephant";CurrentNode=AnimalName "elephant";
                                YesNoList = [];
                                NewDiscriminatingQuestion=None
                              }

    [<Test>]
        let first_test() =
            Assert.IsTrue(true)

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
        runUntilIndex outStream inStream tree initialState numLoops |> ignore

        // verify expectations
        mockrepo.VerifyAll()


                                  
    [<Test>]
    let ``in the state InviteToThinkAboutAnAnimal, given a only one leaf node root should guess that it is the animal in the root``() =
        // setup
        let mockrepo = new MockRepository()
        let inStream = mockrepo.DynamicMock<InStream>()
        let outStream = mockrepo.StrictMock<OutStream>()

        // expectations
        Expect.Call(outStream.Write("is it a cat?")) |> ignore
        mockrepo.ReplayAll()

        // arrange
        let tree = AnimalName "cat"
        let initialState = InviteToThinkAboutAnAnimal
        let numLoops = 1

        // act
        runUntilIndex outStream inStream tree InviteToThinkAboutAnAnimal numLoops |> ignore

        // verify expectations
        mockrepo.VerifyAll()




[<Test>]
    let ``with a one leaf node knowledge tree, engine asks if it is the animal on that node, the user says yes, and so the engine answer with "yeah!"``() =
        // setup 
        let mockrepo = new MockRepository()
        let inStream = mockrepo.StrictMock<InStream>()

        let outStream = mockrepo.StrictMock<OutStream>()

        // setup expectations
        Expect.Call(outStream.Write("think about an animal")) |> ignore
        Expect.Call(inStream.Input()).Return("whatever") |> ignore

        Expect.Call(outStream.Write("is it a cat?")) |> ignore     
        Expect.Call(inStream.Input()).Return("yes") |> ignore

        Expect.Call(outStream.Write("yeah!")) |> ignore 
        Expect.Call(inStream.Input()).Return("anything") |> ignore
          
        mockrepo.ReplayAll()

        // arrange
        let tree = AnimalName "cat"
        let initState = Welcome
        let numOfLoops = 3

        // act
        runUntilIndex outStream inStream tree initState numOfLoops |> ignore

        // verify expectations
        mockrepo.VerifyAll()


    [<Test>]
    let ``given one node knowledge tree, the engine asks if it is such animal, and when the user says no, then the engine asks "what animal was"``() =
        // setup stubs
        let mockrepo = new MockRepository()
        let inStream = mockrepo.StrictMock<InStream>()
        let outStream = mockrepo.StrictMock<OutStream>()

        // mocking
        Expect.Call(outStream.Write("think about an animal")) |> ignore
        Expect.Call(inStream.Input()).Return("anything") |> ignore

        Expect.Call(outStream.Write("is it a cat?")) |> ignore     
        Expect.Call(inStream.Input()).Return("no") |> ignore

        Expect.Call(outStream.Write("what animal was?")) |> ignore     
        Expect.Call(inStream.Input()).Return("any") |> ignore
          
        mockrepo.ReplayAll()
          
        // arrange
        let tree = AnimalName "cat"
        let initState = Welcome

        // act
        runUntilIndex outStream inStream tree initState 3 |> ignore

        // assert
        mockrepo.VerifyAll()





    [<Test>]
    let ``the system will ask if is the animal on the root, and the use says no, and so the system will ask a question do distinguish them``() =
        // setup stubs
        let mockrepo = new MockRepository()
        let inStream = mockrepo.StrictMock<InStream>()
        let outStream = mockrepo.StrictMock<OutStream>()

        Expect.Call(outStream.Write("think about an animal")) |> ignore
        Expect.Call(inStream.Input()).Return("") |> ignore

        Expect.Call(outStream.Write("is it a dog?")) |> ignore
        Expect.Call(inStream.Input()).Return("no") |> ignore

        Expect.Call(outStream.Write("what animal was?")) |> ignore
        Expect.Call(inStream.Input()).Return("cat") |> ignore

        Expect.Call(outStream.Write("please, write a yes/no question to distinguish a cat from a dog")) |> ignore
        Expect.Call(inStream.Input()).Return("does it bark?") |> ignore

        Expect.Call(outStream.Write("what is the answer to the question \"does it bark?\" to distinguish a cat from a dog?")) |> ignore
        Expect.Call(inStream.Input()).Return("no") |> ignore
          
        mockrepo.ReplayAll()

        // arrange
        let tree = AnimalName "dog"

        // act
        runUntilIndex outStream inStream tree Welcome 5 |> ignore

        // assert
        mockrepo.VerifyAll()


    [<Test>]
    let ``if the user specifies an empty string as animal, then the question about what animal was should be asked again``() =
        // setup stubs
        let mockrepo = new MockRepository()
        let inStream = mockrepo.StrictMock<InStream>()
        let outStream = mockrepo.StrictMock<OutStream>()

        Expect.Call(outStream.Write("think about an animal")) |> ignore
        Expect.Call(inStream.Input()).Return("") |> ignore

        Expect.Call(outStream.Write("is it a dog?")) |> ignore
        Expect.Call(inStream.Input()).Return("no") |> ignore

        Expect.Call(outStream.Write("what animal was?")) |> ignore
        Expect.Call(inStream.Input()).Return("") |> ignore

        Expect.Call(outStream.Write("what animal was?")) |> ignore
        Expect.Call(inStream.Input()).Return("cat") |> ignore

        Expect.Call(outStream.Write("please, write a yes/no question to distinguish a cat from a dog")) |> ignore
        Expect.Call(inStream.Input()).Return("does it bark?") |> ignore

        Expect.Call(outStream.Write("what is the answer to the question \"does it bark?\" to distinguish a cat from a dog?")) |> ignore
        Expect.Call(inStream.Input()).Return("no") |> ignore
          
        mockrepo.ReplayAll()

        // arrange
        let tree = AnimalName "dog"

        // act
        runUntilIndex outStream inStream tree Welcome 6 |> ignore

        // assert
        mockrepo.VerifyAll()




    [<Test>]
    let ``update the knowledge tree``() =
        // setup
        let mockrepo = new MockRepository()
        let inStream = mockrepo.StrictMock<InStream>()
        let outStream = mockrepo.StrictMock<OutStream>()


        // mocking
        Expect.Call(outStream.Write("think about an animal")) |> ignore
        Expect.Call(inStream.Input()).Return("") |> ignore

        Expect.Call(outStream.Write("is it a dog?")) |> ignore
        Expect.Call(inStream.Input()).Return("no") |> ignore

        Expect.Call(outStream.Write("what animal was?")) |> ignore
        Expect.Call(inStream.Input()).Return("cat") |> ignore

        Expect.Call(outStream.Write("please, write a yes/no question to distinguish a cat from a dog")) |> ignore
        Expect.Call(inStream.Input()).Return("does it bark?") |> ignore

        Expect.Call(outStream.Write("what is the answer to the question \"does it bark?\" to distinguish a cat from a dog?")) |> ignore
        Expect.Call(inStream.Input()).Return("no") |> ignore

        Expect.Call(outStream.Write("ok")) |> ignore
        Expect.Call(inStream.Input()).Return("") |> ignore

        Expect.Call(outStream.Write("think about an animal")) |> ignore
        Expect.Call(inStream.Input()).Return("") |> ignore

        Expect.Call(outStream.Write("does it bark?")) |> ignore
        Expect.Call(inStream.Input()).Return("no") |> ignore

        mockrepo.ReplayAll()


        // arrange
        let tree = AnimalName "dog"
        let initState = Welcome

        // act
        runUntilIndex outStream inStream tree initState 8 |> ignore

        // assert
        mockrepo.VerifyAll()




    [<Test>]
    let ``update the knowledge tree starting from elephant (reproducing bug)``() =
        // setup
        let mockrepo = new MockRepository()
        let inStream = mockrepo.StrictMock<InStream>()
        let outStream = mockrepo.StrictMock<OutStream>()

        // mocking
        Expect.Call(outStream.Write("think about an animal")) |> ignore
        Expect.Call(inStream.Input()).Return("") |> ignore
      
        Expect.Call(outStream.Write("is it a elephant?")) |> ignore
        Expect.Call(inStream.Input()).Return("no") |> ignore
        
        Expect.Call(outStream.Write("what animal was?")) |> ignore
        Expect.Call(inStream.Input()).Return("cat") |> ignore
        
        Expect.Call(outStream.Write("please, write a yes/no question to distinguish a cat from a elephant")) |> ignore
        Expect.Call(inStream.Input()).Return("is it small?") |> ignore
       
        Expect.Call(outStream.Write("what is the answer to the question \"is it small?\" to distinguish a cat from a elephant?")) |> ignore
        Expect.Call(inStream.Input()).Return("yes") |> ignore
       
        Expect.Call(outStream.Write("ok")) |> ignore
        Expect.Call(inStream.Input()).Return("") |> ignore
        
        Expect.Call(outStream.Write("think about an animal")) |> ignore
        Expect.Call(inStream.Input()).Return("") |> ignore
        
        Expect.Call(outStream.Write("is it small?")) |> ignore
        Expect.Call(inStream.Input()).Return("yes") |> ignore
        
        Expect.Call(outStream.Write("is it a cat?")) |> ignore
        Expect.Call(inStream.Input()).Return("no") |> ignore
        
        Expect.Call(outStream.Write("what animal was?")) |> ignore
        Expect.Call(inStream.Input()).Return("mouse") |> ignore

        Expect.Call(outStream.Write("please, write a yes/no question to distinguish a mouse from a cat")) |> ignore
        Expect.Call(inStream.Input()).Return("is it clean?") |> ignore

        Expect.Call(outStream.Write("what is the answer to the question \"is it clean?\" to distinguish a mouse from a cat?")) |> ignore
        Expect.Call(inStream.Input()).Return("no") |> ignore

        Expect.Call(outStream.Write("ok")) |> ignore
        Expect.Call(inStream.Input()).Return("") |> ignore

        Expect.Call(outStream.Write("think about an animal")) |> ignore
        Expect.Call(inStream.Input()).Return("") |> ignore

        Expect.Call(outStream.Write("is it small?")) |> ignore
        Expect.Call(inStream.Input()).Return("yes") |> ignore

        Expect.Call(outStream.Write("is it clean?")) |> ignore
        Expect.Call(inStream.Input()).Return("yes") |> ignore

        Expect.Call(outStream.Write("is it a cat?")) |> ignore
        Expect.Call(inStream.Input()).Return("no") |> ignore

        Expect.Call(outStream.Write("what animal was?")) |> ignore
        Expect.Call(inStream.Input()).Return("ant") |> ignore

        Expect.Call(outStream.Write("please, write a yes/no question to distinguish a ant from a cat")) |> ignore
        Expect.Call(inStream.Input()).Return("is it an insect?") |> ignore

        Expect.Call(outStream.Write("what is the answer to the question \"is it an insect?\" to distinguish a ant from a cat?")) |> ignore
        Expect.Call(inStream.Input()).Return("yes") |> ignore

        Expect.Call(outStream.Write("ok")) |> ignore
        Expect.Call(inStream.Input()).Return("") |> ignore

        Expect.Call(outStream.Write("think about an animal")) |> ignore
        Expect.Call(inStream.Input()).Return("") |> ignore

        Expect.Call(outStream.Write("is it small?")) |> ignore
        Expect.Call(inStream.Input()).Return("yes") |> ignore

        Expect.Call(outStream.Write("is it clean?")) |> ignore
        Expect.Call(inStream.Input()).Return("no") |> ignore

        mockrepo.ReplayAll()

        // arrange
        let tree = AnimalName "elephant"
        let initState = Welcome
        
        // act
        runUntilIndex outStream inStream tree initState 24 |> ignore
        
        // assert
        mockrepo.VerifyAll()


    [<Test>]
    let ``when write uncorrectly the yes or no, the question will be repeated``() =
        // setup
        let mockrepo = new MockRepository()
        let inStream = mockrepo.StrictMock<InStream>()
        let outStream = mockrepo.StrictMock<OutStream>()


        // mocking
        Expect.Call(outStream.Write("think about an animal")) |> ignore
        Expect.Call(inStream.Input()).Return("") |> ignore

        Expect.Call(outStream.Write("is it a dog?")) |> ignore
        Expect.Call(inStream.Input()).Return("nou") |> ignore

        Expect.Call(outStream.Write("please answer only yes or no. is it a dog?")) |> ignore
        Expect.Call(inStream.Input()).Return("yes") |> ignore
        Expect.Call(outStream.Write("yeah!")) |> ignore

        Expect.Call(inStream.Input()).Return("") |> ignore


//        Expect.Call(outStream.Write("what animal was?")) |> ignore
//        Expect.Call(inStream.Input()).Return("cat") |> ignore
//
//        Expect.Call(outStream.Write("please, write a yes/no question to distinguish a cat from a dog")) |> ignore
//        Expect.Call(inStream.Input()).Return("does it bark?") |> ignore
//
//        Expect.Call(outStream.Write("what is the answer to the question \"does it bark?\" to distinguish a cat from a dog?")) |> ignore
//        Expect.Call(inStream.Input()).Return("no") |> ignore
//
//        Expect.Call(outStream.Write("ok")) |> ignore
//        Expect.Call(inStream.Input()).Return("") |> ignore
//
//        Expect.Call(outStream.Write("think about an animal")) |> ignore
//        Expect.Call(inStream.Input()).Return("") |> ignore
//
//        Expect.Call(outStream.Write("does it bark?")) |> ignore
//        Expect.Call(inStream.Input()).Return("no") |> ignore

        mockrepo.ReplayAll()


        // arrange
        let tree = AnimalName "dog"
        let initState = Welcome

        // act
        runUntilIndex outStream inStream tree initState 4 |> ignore

        // assert
        mockrepo.VerifyAll()














    
