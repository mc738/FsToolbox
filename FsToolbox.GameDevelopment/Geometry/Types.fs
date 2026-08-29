namespace FsToolbox.GameDevelopment.Geometry

module Types =

    type VertexLayout = { Items: VertexLayoutItem list }

    and VertexLayoutItem =
        { Name: string
          ShaderName: string
          Size: int }
