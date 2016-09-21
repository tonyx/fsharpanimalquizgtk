
namespace fsharpAnimalQuizKata

module MainModule =
    open fsharpAnimalQuizKata.RunModule
    open fsharpAnimalQuizKata.IoModule
    open fsharpAnimalQuizKata.BrainModule
    open System
    open fsharpAnimalQuizKata.RecordUtils
    open Gtk


    [<EntryPoint>]
    let Main(args) = 
        if (args.Length > 0 && args.[0] = "gui" ) then
            Application.Init()
            let win = new DialogWindow.MainDialog()
            win.Show()
            Application.Run()
            0
        else 
            let myOutputStream = new ConsoleOutput() :> OutStream
            let myInputStream = new ConsoleInput() :> InStream

            let tree = AnimalName "elephant"

            runForever myOutputStream myInputStream tree Welcome

