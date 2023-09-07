using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using static TycheFighters.Program;

namespace TycheFighters
{
    public partial class Game : GameWindow
    {
        private float[] vertices = {};

        Shader shader;

        private int VertexBufferObject;
        private int VertexArrayObject;
        public Game(int width, int height, string title) : base(GameWindowSettings.Default, new NativeWindowSettings() { Size = (width, height), Title = title }) { }

        public int screenWidth = 256;
        public int screenHeight = 256;

        public float shakeStrength = 0f;
        public byte shakeFrame = 0;

        private List<Triangle> triangles = new List<Triangle>();

        public void MapTriangles()
        {
            int minDimension = Math.Min(screenWidth, screenHeight);
            float scale = minDimension / 255f;

            vertices = new float[triangles.Count * 9];

            for (int i = 0; i < triangles.Count; i++)
            {
                vertices[i*9] = (triangles[i].worldCoords[0] - 128f - shakeStrength * MathF.Cos(shakeFrame)) * scale / (screenWidth / 2.0f);
                vertices[i*9 + 1] = (triangles[i].worldCoords[1] - 128f - shakeStrength * MathF.Cos(shakeFrame)) * scale / (screenHeight / 2.0f);
                vertices[i*9 + 2] = triangles[i].color / 255f;

                vertices[i*9 + 3] = (triangles[i].worldCoords[2] - 128f - shakeStrength * MathF.Cos(shakeFrame)) * scale / (screenWidth / 2.0f);
                vertices[i*9 + 4] = (triangles[i].worldCoords[3] - 128f - shakeStrength * MathF.Cos(shakeFrame)) * scale / (screenHeight / 2.0f);
                vertices[i*9 + 5] = triangles[i].color / 255f;

                vertices[i*9 + 6] = (triangles[i].worldCoords[4] - 128f - shakeStrength * MathF.Cos(shakeFrame)) * scale / (screenWidth / 2.0f);
                vertices[i*9 + 7] = (triangles[i].worldCoords[5] - 128f - shakeStrength * MathF.Cos(shakeFrame)) * scale / (screenHeight / 2.0f);
                vertices[i*9 + 8] = triangles[i].color / 255f;
            }
        }

        public void AddTrianglesToDraw(List<Triangle> trianglesToAdd)
        {
            triangles = Enumerable.Concat(triangles, trianglesToAdd).ToList();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            if (KeyboardState.IsKeyPressed(Keys.Escape))
            {
                if (currentScene == 1)
                {
                    gamePaused = !gamePaused;
                }
            }
            else if (KeyboardState.IsKeyPressed(Keys.Space))
            {
                currentScene = (byte)(1 - currentScene);
            }

            switch(currentScene)
            {
                case 0:
                    UpdateMenuScene();
                    break;
                case 1:
                    if (!gamePaused) UpdateFightScene();
                    break;
            }
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);

            VertexBufferObject = GL.GenBuffer();

            VertexArrayObject = GL.GenVertexArray();

            shader = new Shader("shader.vert", "shader.frag");

            shader.Use();
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            shader.Use();
            triangles.Clear();

            //

            switch (currentScene)
            {
                case 0:
                    DrawMenuScene();
                    break;
                case 1:
                    DrawFightScene();
                    break;
            }

            //
            
            MapTriangles();
            
            //
            
            HandleRender();
        }

        private void HandleRender()
        {
            GL.BindVertexArray(VertexArrayObject);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.DynamicDraw);

            // Vertex Position
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.DrawArrays(PrimitiveType.Triangles, 0, triangles.Count * 3);
            SwapBuffers();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            screenWidth = e.Width;
            screenHeight = e.Height;

            GL.Viewport(0, 0, screenWidth, screenHeight);
        }

        protected override void OnUnload()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            // Delete all the resources.
            GL.DeleteBuffer(VertexBufferObject);
            GL.DeleteVertexArray(VertexArrayObject);

            GL.DeleteProgram(shader.Handle);

            base.OnUnload();

            shader.Dispose();
        }
    }
}