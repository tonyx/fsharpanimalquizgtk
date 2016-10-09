
module fsharpAnimalQuizKata.KnowledgeTreeXmlSchema
open System
open Gtk;
open FSharp.Data
open System.Xml.Linq
open System.Data

         
    type KnowledgeBaseXmlSchema = XmlProvider<Schema="""
        <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema"
            elementFormDefault="qualified" attributeFormDefault="unqualified">
            <xs:element name = "node">
                <xs:complexType>            
                    <xs:sequence>            
                        <xs:choice>
                            <xs:element name = "animal" type="xs:string"/>
                            <xs:sequence>
                                <xs:element name = "question" type="xs:string"/> 
                                <xs:element name = "yesBranch">
                                    <xs:complexType>                        
                                        <xs:sequence>
                                            <xs:element ref = "node"/>                        
                                        </xs:sequence>
                                    </xs:complexType>
                                </xs:element>
                                <xs:element name = "noBranch">
                                    <xs:complexType>                        
                                        <xs:sequence>
                                            <xs:element ref = "node"/>                        
                                        </xs:sequence>
                                    </xs:complexType>
                                </xs:element>
                            </xs:sequence>
                        </xs:choice>
                    </xs:sequence>
                </xs:complexType>
            </xs:element>
        </xs:schema>""">