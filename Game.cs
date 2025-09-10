using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace FirstOpenTK
{
    public class Game : GameWindow
    {
        private int _vbo;     // vertex buffer
        private int _vao;     // vertex array
        private int _ebo;     // element/index buffer
        private int _shader;  // shader program

        public Game(GameWindowSettings gws, NativeWindowSettings nws) : base(gws, nws) { }

        // --- Setup ---
        protected override void OnLoad()
        {
            base.OnLoad();
            GL.ClearColor(0.12f, 0.13f, 0.16f, 1f);

            // 4 rectangle corners (x, y, z)
            float[] vertices =
            {
                //  x      y     z
                 0.5f,  0.5f, 0.0f, // 0: top-right
                 0.5f, -0.5f, 0.0f, // 1: bottom-right
                -0.5f, -0.5f, 0.0f, // 2: bottom-left
                -0.5f,  0.5f, 0.0f  // 3: top-left
            };


            // Two triangles: (0,1,3) and (1,2,3)
            uint[] indices = { 0, 1, 3, 1, 2, 3 };

            // VBO
            _vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            // VAO
            _vao = GL.GenVertexArray();
            GL.BindVertexArray(_vao);

            // EBO
            _ebo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            // Vertex layout: location 0 → vec3 position
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            // Shaders
            const string vert = @"#version 330 core
layout (location = 0) in vec3 aPos;
void main() {
    gl_Position = vec4(aPos, 1.0);
}";
            const string frag = @"#version 330 core
out vec4 FragColor;
void main() {
    FragColor = vec4(0.98, 0.50, 0.45, 1); // rectangle color
}";
            _shader = CreateProgram(vert, frag);

            // Unbind to keep state clean
            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            // Do NOT unbind the ElementArrayBuffer while VAO is bound (we already unbound VAO).
        }

        // --- Per-frame update ---
        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
            if (KeyboardState.IsKeyDown(Keys.Escape))
                Close();
        }

        // --- Per-frame draw ---
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.UseProgram(_shader);
            GL.BindVertexArray(_vao);

            // Filled rectangle (two triangles via indices)
            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);

            SwapBuffers();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, Size.X, Size.Y);
        }

        protected override void OnUnload()
        {
            base.OnUnload();
            GL.DeleteBuffer(_vbo);
            GL.DeleteBuffer(_ebo);
            GL.DeleteVertexArray(_vao);
            GL.DeleteProgram(_shader);
        }

        // --- Helpers ---
        private static int CreateProgram(string vertexSrc, string fragmentSrc)
        {
            int vs = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vs, vertexSrc);
            GL.CompileShader(vs);
            GL.GetShader(vs, ShaderParameter.CompileStatus, out int vOk);
            if (vOk == 0) throw new System.Exception(GL.GetShaderInfoLog(vs));

            int fs = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fs, fragmentSrc);
            GL.CompileShader(fs);
            GL.GetShader(fs, ShaderParameter.CompileStatus, out int fOk);
            if (fOk == 0) throw new System.Exception(GL.GetShaderInfoLog(fs));

            int prog = GL.CreateProgram();
            GL.AttachShader(prog, vs);
            GL.AttachShader(prog, fs);
            GL.LinkProgram(prog);
            GL.GetProgram(prog, GetProgramParameterName.LinkStatus, out int pOk);
            if (pOk == 0) throw new System.Exception(GL.GetProgramInfoLog(prog));

            GL.DetachShader(prog, vs);
            GL.DetachShader(prog, fs);
            GL.DeleteShader(vs);
            GL.DeleteShader(fs);
            return prog;
        }
    }
}
