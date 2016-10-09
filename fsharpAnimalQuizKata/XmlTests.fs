
namespace animalquizTests

module XmlTests =

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
     let ``can load the new refactored structure single node animal``()=
       let expected = AnimalName "monkey"
       let xmlVersion = "<node><animal>monkey</animal></node>"
       let xmlWrapped = KnowledgeBaseXmlSchema.Parse xmlVersion
       let actual = xmlToTree xmlWrapped
       Assert.IsTrue(true)


    [<Test>]
     let ``can load the new refactored structure complex node animal``()=
       let expected =  SubTree {Question="is it big?"; YesBranch=AnimalName "elephant"; NoBranch = AnimalName "cat" }

       let xmlVersion = """<node><question>is it big?</question>
        <yesBranch><node><animal>elephant</animal></node></yesBranch>
        <noBranch><node><animal>cat</animal></node></noBranch>
       
        </node>"""
       let xmlWrapped = KnowledgeBaseXmlSchema.Parse xmlVersion
       let actual = xmlToTree xmlWrapped
       Assert.AreEqual(expected,actual)


    [<Test>]
     let ``can load the new refactored structure more complex node animal``()=
       let expected =  SubTree {Question="is it big?"; YesBranch=AnimalName "elephant"; NoBranch = SubTree {Question = "is it an insect?";YesBranch=AnimalName "ant";NoBranch=AnimalName "cat" }}

       let xmlVersion = """
       <node><question>is it big?</question>
         <yesBranch><node><animal>elephant</animal></node></yesBranch>
         <noBranch>
            <node>
                <question>is it an insect?</question>
                <yesBranch><node><animal>ant</animal></node></yesBranch>
                <noBranch><node><animal>cat</animal></node> </noBranch>
            </node>
         </noBranch>
       </node>"""
       let xmlWrapped = KnowledgeBaseXmlSchema.Parse xmlVersion

       let actual = xmlToTree xmlWrapped
       Assert.AreEqual(expected,actual)
         