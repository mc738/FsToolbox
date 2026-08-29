namespace FsToolbox.OpenGL.Materials

open System.Numerics
open FsToolbox.OpenGL.Shaders

[<AbstractClass>]
type OpenGLMaterial(shader: OpenGLShader) =
    
    abstract member Bind: View: Matrix4x4 * Projection: Matrix4x4 * ModelMatrix: Matrix4x4 -> unit
        
    member _.Use() = shader.Use()
    
    member _.BindViewProjection(view: Matrix4x4, projection: Matrix4x4) =
        
        shader.SetUniform("uView", view)
        shader.SetUniform("uProjection", projection)
        ()
    
    member _.BindModel(modelMatrix: Matrix4x4) =
        shader.SetUniform("uModel", modelMatrix)
        ()