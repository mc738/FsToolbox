namespace FsToolbox.Yaml.Tests

open System.Collections.Generic
open System.Text.Json
open Microsoft.VisualStudio.TestTools.UnitTesting
open YamlDotNet.RepresentationModel

[<TestClass>]
type ConversionTests() =

    [<TestMethod>]
    member _.``Json object to yaml``() =
        let obj = JsonDocument.Parse("""{ "test": "Hello, World!" }""").RootElement

        let expected =
            YamlMappingNode(
                seq { KeyValuePair(YamlScalarNode "test" :> YamlNode, YamlScalarNode "Hello, World!" :> YamlNode) }
            )
            :> YamlNode
            |> Some

        let actual =
            FsToolbox.Yaml.Conversion.jsonElementToYamlNode obj
        
        Assert.AreEqual(expected, actual)
