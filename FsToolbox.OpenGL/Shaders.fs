namespace FsToolbox.OpenGL.Shaders

open System
open System.IO
open System.Numerics
open System.Runtime.InteropServices
open Silk.NET.OpenGL



type OpenGLShader(gl: GL, vertexCode: string, fragmentCode: string) as this =

    let mutable handle = 0u

    do
        let vertex = this.LoadShader(ShaderType.VertexShader, vertexCode)
        let fragment = this.LoadShader(ShaderType.FragmentShader, fragmentCode)

        handle <- gl.CreateProgram()

        // Attach shaders to program

        gl.AttachShader(handle, vertex)
        gl.AttachShader(handle, fragment)
        gl.LinkProgram(handle)

        let status = gl.GetProgram(handle, ProgramPropertyARB.LinkStatus)

        if status <> (GLEnum.True |> int) then

            failwith $"Failed to link program: {gl.GetProgramInfoLog(handle)}"

        // We can remove shaders now.
        gl.DetachShader(handle, vertex)
        gl.DetachShader(handle, fragment)
        gl.DeleteShader(vertex)
        gl.DeleteShader(fragment)

    interface IDisposable with
        member this.Dispose() = gl.DeleteProgram(handle)

    static member CreateFromFile(gl: GL, vertexCodePath: string, fragmentCodePath: string) =
        new OpenGLShader(gl, File.ReadAllText(vertexCodePath), File.ReadAllText(fragmentCodePath))

    member _.Handle = handle

    member _.Use() = gl.UseProgram(handle)


    member _.SetUniform(name: string, value: int) =
        let location = gl.GetUniformLocation(handle, name)

        if location = -1 then
            failwith $"{name} not found on shader"

        gl.Uniform1(location, value)


    member _.SetUniform(name: string, value: float32) =
        let location = gl.GetUniformLocation(handle, name)

        if location = -1 then
            failwith $"{name} not found on shader"

        gl.Uniform1(location, value)

    member _.SetUniform(name: string, value: Matrix4x4) =
        let location = gl.GetUniformLocation(handle, name)

        if location = -1 then
            failwith $"{name} not found on shader"

        // No Alloc!!!!!!!
        // Using memory marsh and pointing to the first element
        let span = MemoryMarshal.CreateReadOnlySpan(&value.M11, 16)
        gl.UniformMatrix4(location, false, span)

    member _.SetUniform(name: string, value: Vector3) =
        let location = gl.GetUniformLocation(handle, name)

        if location = -1 then
            failwith $"{name} not found on shader"

        gl.Uniform3(location, value)

    member private _.LoadShader(shaderType: ShaderType, shaderCode: string) =
        //To load a single shader we need to:
        //1) Load the shader from a file.
        //2) Create the handle.
        //3) Upload the source to opengl.
        //4) Compile the shader.
        //5) Check for errors.
        let shaderHandler = gl.CreateShader(shaderType)
        gl.ShaderSource(shaderHandler, shaderCode)

        gl.CompileShader(shaderHandler)

        let status = gl.GetShader(shaderHandler, ShaderParameterName.CompileStatus)

        if status <> (GLEnum.True |> int) then

            failwith $"Failed to vertex compile shader: {gl.GetShaderInfoLog(shaderHandler)}"

        shaderHandler
