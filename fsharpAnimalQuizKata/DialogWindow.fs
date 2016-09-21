
namespace fsharpAnimalQuizKata
open fsharpAnimalQuizKata.IoModule
open fsharpAnimalQuizKata.RecordUtils
open fsharpAnimalQuizKata.BrainModule
module DialogWindow =

        open System
        open Gtk;

        type MainDialog() as this =
            inherit Window("Animal Quiz")

            let mutable statusStructure = consoleInteract templateInitStructure

            let textBox = new Entry()
            let outerv = new VBox()
            do outerv.BorderWidth <- (uint32)12
            do outerv.Spacing <-14 
            do this.Add(outerv)

            let label = new Label(("<span weight=\"bold\" size=\"larger\">Animal Quiz</span>"))

            do outerv.Add(label)

            do label.Xalign <- (float32)0
            do label.UseMarkup <- true

            let h  = new HBox()
            do h.Spacing <- 6
            do outerv.Add(h)

            let v =  new VBox()
            do h.Spacing <- 6
            do h.PackStart(v,false,false,(uint32)0)

            let l1 = new Label(statusStructure.messageFromEngine)
            do l1.Xalign <- (float32)0
            do v.PackStart(l1,true,false,(uint32)0)
            do l1.MnemonicWidget <- textBox

            let v = new VBox()
            do v.Spacing <- 6
            do h.PackStart(v,true,true,(uint32)0)

            do v.PackStart(textBox,true,true,(uint32)0)

            do this.DeleteEvent.AddHandler(fun o e -> this.OnDeleteEvent(o,e))
            do textBox.Activated.AddHandler(fun o e -> this.EnterPressed(o,e))

            do this.ShowAll()

            member this.OnDeleteEvent(o,e:DeleteEventArgs) = 
                Application.Quit ()
                e.RetVal <- true

            member this.EnterPressed(i,e:EventArgs) =  
                do statusStructure <- consoleInteract {statusStructure with messageFromPlayer = Some textBox.Text}
                do l1.Text <- statusStructure.messageFromEngine
                do textBox.Text <- ""

