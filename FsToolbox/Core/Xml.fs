namespace FsToolbox.Core

[<RequireQualifiedAccess>]
module Xml =

    open System
    open System.IO
    open System.Xml.Linq

    let xName (ns: string) (name: string)  = XName.Get(name, ns)

    
    
    let getElements (name: XName) (parent: XElement) =
        parent.Elements name
        |> List.ofSeq
        
    let tryGetElement (name: XName) (parent: XElement) =
        parent.Element(name)
        |> fun r ->
            match r <> null with
            | true -> Some r
            | false -> None

    let setValue (element: XElement) (value: string) = element.Value <- value

    let tryGetAttribute (name: string) (element: XElement) =
        element.Attribute name
        |> fun r ->
            match r <> null with
            | true -> Some r
            | false -> None

    let tryGetAttributeValue (name: string) (element: XElement) =
        tryGetAttribute name element
        |> Option.bind (fun att -> Some att.Value)

    let getAttributeValueWithDefault (name: string) (defaultValue: string) (element: XElement) =
        tryGetAttributeValue name element
        |> Option.defaultValue defaultValue
    
    let getAttributeValue (name: string) (element: XElement) =
        getAttributeValueWithDefault name String.Empty element
    
    let tryGetGuidAttributeValue (name: string) (element: XElement) =
        tryGetAttributeValue name element
        |> Option.bind
            (fun v ->
                match Guid.TryParse v with
                | true, r -> Some r
                | false, _ -> None)
    
    let getGuidAttributeValueWithDefault (name: string) (defaultValue: Guid) (element: XElement) =
        tryGetGuidAttributeValue name element
        |> Option.defaultValue defaultValue
            
    let getGuidAttributeValue (name: string) (element: XElement) =
        getGuidAttributeValueWithDefault name (Guid()) element
            
    let tryGetDateTimeAttributeValue (name: string) (element: XElement) =
        tryGetAttributeValue name element
        |> Option.bind
            (fun v ->
                match DateTime.TryParse v with
                | true, r -> Some r
                | false, _ -> None)

    let getDateTimeAttributeValueWithDefault (name: string) (defaultValue: DateTime) (element: XElement) =
        tryGetDateTimeAttributeValue name element
        |> Option.defaultValue defaultValue
        
    let getDateTimeAttribute (name: string) (element: XElement) =
        getDateTimeAttributeValueWithDefault name (DateTime()) element
        
    let tryGetTimespanAttributeValue (name: string) (element: XElement) =
        tryGetAttributeValue name element
        |> Option.bind
            (fun v ->
                match TimeSpan.TryParse v with
                | true, r -> Some r
                | false, _ -> None)

    let getTimespanAttributeValueWithDefault (name: string) (defaultValue: TimeSpan) (element: XElement) =
        tryGetTimespanAttributeValue name element
        |> Option.defaultValue defaultValue
    
    let getTimespanAttribute (name: string) (element: XElement) =
        getTimespanAttributeValueWithDefault name (TimeSpan()) element
    
    let tryGetIntAttributeValue (name: string) (element: XElement) =
        tryGetAttributeValue name element
        |> Option.bind
            (fun v ->
                match Int32.TryParse v with
                | true, r -> Some r
                | false, _ -> None) 
    
    let getIntAttributeValueWithDefault (name: string) (defaultValue: int) (element: XElement) =
        tryGetIntAttributeValue name element
        |> Option.defaultValue defaultValue
    
    let getIntAttributeValue (name: string) (element: XElement) =
        getIntAttributeValueWithDefault name 0 element
    
    let parse (xml: string) =
        try
            XDocument.Parse xml |> Ok
        with
        | exn ->
            Error
                { Message = exn.Message
                  Exception = Some exn }

    let save (path: string) (xDoc: XDocument) = xDoc.Save(path)


