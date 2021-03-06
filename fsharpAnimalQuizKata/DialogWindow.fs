﻿
namespace fsharpAnimalQuizKata
open fsharpAnimalQuizKata.IoModule
open fsharpAnimalQuizKata.RecordUtils
open fsharpAnimalQuizKata.BrainModule
open FSharp.Data
open System.IO
open System.Xml
open System.Text
open fsharpAnimalQuizKata.KnowledgeTreeXmlSchema

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


// menu load save
            let mb = new MenuBar()
            let agrp = new AccelGroup()
            
            let file_menu = new Menu()
            let item = new MenuItem("_File")
            do item.Submenu <- file_menu
            do mb.Append(item)

            let item = new ImageMenuItem(Stock.Open,agrp)
            do file_menu.Append(item)

            do outerv.PackStart(mb,false,false,(uint32)0)

            let item2 = new ImageMenuItem(Stock.Save,agrp)
            do file_menu.Append(item2)
            do file_menu.Append(new SeparatorMenuItem())


            let label = new Label(("<span weight=\"bold\" size=\"larger\">Animal Quiz</span>"))

            do outerv.Add(label)

            let hBox = new HBox()

            let button = new Button("show tree")
            do hBox.Add(button)

            let buttonHide = new Button("hide tree")
            do hBox.Add(buttonHide)

            let buttonExpandTree = new Button("expand tree")
            do hBox.Add(buttonExpandTree)
            do outerv.Add(hBox)

            let tv = new TreeView()
            do tv.HeadersVisible <- true
            do outerv.Add(tv)

            let col = new TreeViewColumn()
            let colr = new CellRendererText()
            do col.Title <- "Knowledge Tree"
            do col.PackStart(colr,true)
            do col.AddAttribute(colr,"text",0)
            do tv.AppendColumn(col) |> ignore

            let store = new TreeStore(typeof<string>)
            do tv.Model <- store

            let mutable iter = new TreeIter()

            let mutable innerIter = new TreeIter()

            do iter <- store.AppendValues("Knowledge Tree")  

            do label.Xalign <- (float32)0
            do label.UseMarkup <- true

            let h  = new HBox()
            do h.Spacing <- 6
            do outerv.Add(h)

            let v =  new VBox()
            do h.Spacing <- 6


            do h.PackStart(v,false,false,(uint32)0)

            let l1 = new Label(statusStructure.MessageFromEngine)
            do l1.Xalign <- (float32)0
            do v.PackStart(l1,true,false,(uint32)0)
            do l1.MnemonicWidget <- textBox

            let v = new VBox()
            do v.Spacing <- 6
            do h.PackStart(v,true,true,(uint32)0)

            do v.PackStart(textBox,true,true,(uint32)0)



            do this.DeleteEvent.AddHandler(fun o e -> this.OnDeleteEvent(o,e))
            do textBox.Activated.AddHandler(fun o e -> this.EnterPressed(o,e))
            do button.Clicked.AddHandler(fun o e -> this.ShowKnowledgeTree(o,e))
            do buttonHide.Clicked.AddHandler(fun o e -> this.ResetTreeView(o,e))

            do item.Activated.AddHandler(fun o e -> this.OpenActivated(o,e))
            do item2.Activated.AddHandler(fun o e -> this.SaveActivated(o,e))

            do buttonExpandTree.Clicked.AddHandler(fun o e -> this.ExpandTreeView(o,e))

            do this.ShowAll()

            member this.OnDeleteEvent(o,e:DeleteEventArgs) = 
                Application.Quit ()
                e.RetVal <- true

            member this.EnterPressed(i,e:EventArgs) =  
                do statusStructure <- consoleInteract {statusStructure with MessageFromPlayer = Some textBox.Text}
                do l1.Text <- statusStructure.MessageFromEngine
                do textBox.Text <- ""

            member this.OpenOFD() =
                let filechooser = new Gtk.FileChooserDialog("chose the file to open",this,FileChooserAction.Open, "Cancel",ResponseType.Cancel,"Open",ResponseType.Accept)
                0

            member this.OpenActivated(o,e:EventArgs) =
                let filechooser = new Gtk.FileChooserDialog("chose the file to open",this,FileChooserAction.Open, "Cancel",ResponseType.Cancel,"Open",ResponseType.Accept)
                if filechooser.Run() = (int) ResponseType.Accept then
                    try 
                        let file = System.IO.File.OpenRead(filechooser.Filename)
                        let content2 = KnowledgeBaseXmlSchema.Load(file)
                        statusStructure <- {templateInitStructure with RootTree = (xmlToTree content2); CurrentNode = (xmlToTree content2)}
                        do file.Close()
                    with 
                     |  :? Exception as ex -> ( let w1 = new Window("error loading xml, see console for details") 
                                                let outerv = new VBox()
                                                outerv.BorderWidth <- (uint32)12
                                                outerv.Spacing <- 12
                                                w1.Add(outerv)
                                                printf "%s\n" (ex.ToString())
                                                let l = new Label("error loading xml: "+ex.ToString().Substring(0,30)+". See console output for details ")
                                                //l.Xalign <- (uint32)0
                                                outerv.PackStart(l,false,false,(uint32)0)
                                                w1.ShowAll())
                filechooser.Destroy() 

            member this.SaveActivated(o,e:EventArgs) =
                let filechooser = new Gtk.FileChooserDialog("save file",this,FileChooserAction.Save, "Cancel",ResponseType.Cancel,"Save",ResponseType.Accept)
                if filechooser.Run() = (int) ResponseType.Accept then
                
                
                // fix this
                    let knowledgeBaseTree =  treeToXml statusStructure.RootTree
                    let mStream = new MemoryStream()
                    let writer = new XmlTextWriter(mStream,Encoding.Unicode)
                    let document = new XmlDocument()
                    
                    document.WriteContentTo(writer)
                    writer.Flush()
                    mStream.Flush()
                    do mStream.Position <- (int64)0 
                    let sReader = new StreamReader(mStream)
                    let formmattedXml = sReader.ReadToEnd()
                    
               //     File.WriteAllText(filechooser.Filename,formmattedXml)

                    mStream.Close();
                    writer.Close();
                    // end fix this
                    
                    

                    File.WriteAllText(filechooser.Filename,knowledgeBaseTree)
                    filechooser.Destroy() 


// MemoryStream mStream = new MemoryStream();
// XmlTextWriter writer = new XmlTextWriter(mStream, Encoding.Unicode);
// XmlDocument document   = new XmlDocument();





            member this.ShowKnowledgeTree(i,e:EventArgs) =
                do this.ResetTreeView(i,e)
                do treeToTreeStore  statusStructure.RootTree store iter ""

            member this.ResetTreeView(i,e:EventArgs) =
                do store.Clear()
                do iter <- store.AppendValues("Knowledge Tree") 

            member this.ExpandTreeView(i,e:EventArgs) =
                do this.ShowKnowledgeTree(i,e)
                do tv.ExpandAll()

                 
