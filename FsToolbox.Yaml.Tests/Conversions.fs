namespace FsToolbox.Yaml.Tests

open System.Collections.Generic
open System.Text.Json
open Microsoft.VisualStudio.TestTools.UnitTesting
open YamlDotNet.RepresentationModel

[<TestClass>]
type ConversionTests() =

    [<TestMethod>]
    member _.``Json object to yaml``() =
        let obj = JsonDocument.Parse("{ ").RootElement

        let expected =
            YamlMappingNode(seq { KeyValuePair(YamlScalarNode "test", YamlScalarNode "Hello, World!") })
        

        ()
