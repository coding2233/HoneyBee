using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using ImGuiNET;
using ImPlotNET;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

using static ImGuiNET.ImGuiNative;

namespace HoneyBee.Diff.Gui
{
    class DiffProgram
    {
        private static Sdl2Window _window;
        private static GraphicsDevice _gd;
        private static CommandList _cl;
        private static ImGuiController _controller;

        private static Vector3 _clearColor = new Vector3(0.45f, 0.55f, 0.6f);

        private static MainWindow _mainWindow;

        private static CompositionContainer _container = null;


        static void Main(string[] args)
        {
            // Create window, GraphicsDevice, and all resources necessary for the demo.
            VeldridStartup.CreateWindowAndGraphicsDevice(
                new WindowCreateInfo(50, 50, 1280, 720, WindowState.Maximized, "Honey Bee - Diff"),
                new GraphicsDeviceOptions(true, null, true, ResourceBindingModel.Improved, true, true),
                GraphicsBackend.OpenGL,
                out _window,
                out _gd);
            _window.Resized += () =>
            {
                _gd.MainSwapchain.Resize((uint)_window.Width, (uint)_window.Height);
                _controller.WindowResized(_window.Width, _window.Height);
            };
            _cl = _gd.ResourceFactory.CreateCommandList();
            _controller = new ImGuiController(_gd, _gd.MainSwapchain.Framebuffer.OutputDescription, _window.Width, _window.Height);

            //ImGui.StyleColorsLight();

            _mainWindow = new MainWindow();

            // Main application loop
            while (_window.Exists)
            {
                InputSnapshot snapshot = _window.PumpEvents();
                if (!_window.Exists) { break; }
                _controller.Update(1f / 60f, snapshot); // Feed the input events to our ImGui controller, which passes them through to ImGui.

                _mainWindow?.OnDraw();

                _cl.Begin();
                _cl.SetFramebuffer(_gd.MainSwapchain.Framebuffer);
                _cl.ClearColorTarget(0, new RgbaFloat(_clearColor.X, _clearColor.Y, _clearColor.Z, 1f));
                _controller.Render(_gd, _cl);
                _cl.End();
                _gd.SubmitCommands(_cl);
                _gd.SwapBuffers(_gd.MainSwapchain);
            }

            _mainWindow?.Dispose();
            _mainWindow = null;

            // Clean up Veldrid resources
            _gd.WaitForIdle();
            _controller.Dispose();
            _cl.Dispose();
            _gd.Dispose();
        }


        public static void ComposeParts(params object[] attributedParts)
        {
            if (_container == null)
            {
                var catalog = new AssemblyCatalog(Assembly.GetExecutingAssembly());
                _container = new CompositionContainer(catalog);
            }
            _container.ComposeParts(attributedParts);
        }

    }
}
