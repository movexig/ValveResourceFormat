using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using GUI.Types.Renderer;
using GUI.Utils;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace GUI.Types
{
    internal class GLRenderControl
    {
        public class RenderEventArgs
        {
            public float FrameTime { get; set; }
            public Camera Camera { get; set; }
        }

        public Camera Camera { get; set; }

        public Control Control { get; }

        private Label fpsLabel;
        private GLControl glControl;

        public event EventHandler<RenderEventArgs> Paint;
        public event EventHandler Load;

        private readonly Stopwatch stopwatch;

        public GLRenderControl()
        {
            Camera = new Camera();
            Control = InitializeControl();

            stopwatch = new Stopwatch();
        }

        private Control InitializeControl()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
            };

            fpsLabel = new Label
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                AutoSize = true,
                Dock = DockStyle.Top,
            };
            panel.Controls.Add(fpsLabel);

#if DEBUG
            glControl = new GLControl(new GraphicsMode(32, 24, 0, 8), 3, 3, GraphicsContextFlags.Debug);
#else
            glControl = new GLControl(new GraphicsMode(32, 24, 0, 8), 3, 3, GraphicsContextFlags.Default);
#endif
            glControl.Dock = DockStyle.Fill;
            glControl.AutoSize = true;
            glControl.Load += OnLoad;
            glControl.Paint += OnPaint;
            glControl.Resize += OnResize;
            glControl.MouseEnter += OnMouseEnter;
            glControl.MouseLeave += OnMouseLeave;
            glControl.GotFocus += OnGotFocus;
            glControl.VisibleChanged += OnVisibleChanged;
            glControl.Disposed += OnDisposed;

            panel.Controls.Add(glControl);
            return panel;
        }

        private void OnDisposed(object sender, EventArgs e)
        {
            glControl.Load -= OnLoad;
            glControl.Paint -= OnPaint;
            glControl.Resize -= OnResize;
            glControl.MouseEnter -= OnMouseEnter;
            glControl.MouseLeave -= OnMouseLeave;
            glControl.GotFocus -= OnGotFocus;
            glControl.VisibleChanged -= OnVisibleChanged;
            glControl.Disposed -= OnDisposed;
        }

        private void OnVisibleChanged(object sender, EventArgs e)
        {
            if (glControl.Visible)
            {
                glControl.Focus();
            }
        }

        private void OnMouseLeave(object sender, EventArgs e)
        {
            Camera.MouseOverRenderArea = false;
        }

        private void OnMouseEnter(object sender, EventArgs e)
        {
            Camera.MouseOverRenderArea = true;
        }

        private void OnLoad(object sender, EventArgs e)
        {
            glControl.MakeCurrent();

            Console.WriteLine("OpenGL version: " + GL.GetString(StringName.Version));
            Console.WriteLine("OpenGL vendor: " + GL.GetString(StringName.Vendor));
            Console.WriteLine("GLSL version: " + GL.GetString(StringName.ShadingLanguageVersion));

            CheckOpenGL();

            stopwatch.Start();

            Load?.Invoke(this, e);

            HandleResize();
            Draw();
        }

        private void OnPaint(object sender, EventArgs e)
        {
            Draw();
        }

        private void Draw()
        {
            if (glControl.Visible)
            {
                var frameTime = stopwatch.ElapsedMilliseconds / 1000f;
                stopwatch.Restart();

                Camera.Tick(frameTime);
                Camera.HandleInput(Mouse.GetState(), Keyboard.GetState());

                fpsLabel.Text = $"FPS: {Math.Round(1f / frameTime)}";

                GL.ClearColor(Settings.BackgroundColor);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                Paint?.Invoke(this, new RenderEventArgs { FrameTime = frameTime, Camera = Camera });

                glControl.SwapBuffers();
                glControl.Invalidate();
            }
        }

        private void OnResize(object sender, EventArgs e)
        {
            HandleResize();
            Draw();
        }

        private void HandleResize()
        {
            Camera.SetViewportSize(Control.Width, Control.Height);
        }

        private void OnGotFocus(object sender, EventArgs e)
        {
            glControl.MakeCurrent();
            Draw();
        }

        private void CheckOpenGL()
        {
            var extensions = new Dictionary<string, bool>();
            var count = GL.GetInteger(GetPName.NumExtensions);
            for (var i = 0; i < count; i++)
            {
                var extension = GL.GetString(StringNameIndexed.Extensions, i);
                extensions.Add(extension, true);
            }

            if (extensions.ContainsKey("GL_EXT_texture_filter_anisotropic"))
            {
                MaterialLoader.MaxTextureMaxAnisotropy = GL.GetInteger((GetPName)ExtTextureFilterAnisotropic.MaxTextureMaxAnisotropyExt);
            }
            else
            {
                Console.Error.WriteLine("GL_EXT_texture_filter_anisotropic is not supported");
            }
        }
    }
}
