namespace FsToolbox.OpenGL.Materials

open System.Numerics
open FsToolbox.OpenGL.Shaders

[<AbstractClass>]
type OpenGLMaterial(shader: OpenGLShader) as this =
    
    /// An abstract method that will be called each time a model is bound.
    abstract member OnModelBind: unit -> unit
        
    member _.Use() = shader.Use()
    
    member _.BindViewProjection(view: Matrix4x4, projection: Matrix4x4) =
        shader.SetUniform("uView", view)
        shader.SetUniform("uProjection", projection)
    
    member _.BindModel(modelMatrix: Matrix4x4) =
        shader.SetUniform("uModel", modelMatrix)
        this.OnModelBind()