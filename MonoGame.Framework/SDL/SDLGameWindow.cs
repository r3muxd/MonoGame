// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OpenGL;

namespace Microsoft.Xna.Framework
{
    internal class SdlGameWindow : GameWindow, IDisposable
    {
        public override bool AllowUserResizing
        {
            get { return !IsBorderless && _resizable; }
            set
            {
                if (_handle != IntPtr.Zero)
                {
                if (Sdl.Patch > 4)
                    Sdl.Window.SetResizable(_handle, value);
                else
                        throw new Exception(
                            "SDL 2.0.4 does not support changing resizable parameter of the window after it's already been created, please use a newer version of it.");
                }

                _resizable = value;
            }
        }

        public override Rectangle ClientBounds
        {
            get
            {
                int x, y, w, h;
                Sdl.Window.GetPosition(Handle, out x, out y);
                Sdl.Window.GetSize(Handle, out w, out h);
                return new Rectangle(x, y, w, h);
            }
        }

        public override Point Position
        {
            get
            {
                if (_handle == IntPtr.Zero)
                    return _position;

                int x = 0, y = 0;

                if (!IsFullScreen)
                    Sdl.Window.GetPosition(Handle, out x, out y);

                return new Point(x, y);
            }
            set
            {
                _position = value;

                if (_handle != IntPtr.Zero)
                Sdl.Window.SetPosition(Handle, value.X, value.Y);

                _wasMoved = true;
            }
        }

        public override DisplayOrientation CurrentOrientation
        {
            get { return DisplayOrientation.Default; }
        }

        public override IntPtr Handle
        {
            get { return _handle; }
        }

        public override string ScreenDeviceName
        {
            get { return _screenDeviceName; }
        }

        public override bool IsBorderless
        {
            get { return _borderless; }
            set
            {
                Sdl.Window.SetBordered(_handle, value ? 0 : 1);
                _borderless = value;
            }
        }

        public static GameWindow Instance;
        public bool IsFullScreen { get; private set; }

        private readonly Game _game;
        private IntPtr _handle, _icon;
        private bool _disposed;
        private bool _resizable, _borderless, _mouseVisible, _hardwareSwitch;
        private string _screenDeviceName;
        private Point _position;
        private int _storedWidth, _storedHeight;
        private bool _wasMoved;
        private bool _supressMoved;

        private ColorFormat _surfaceFormat;
        private DepthFormat _depthStencilFormat;
        private int _multisampleCount;

        public SdlGameWindow(Game game)
        {
            _game = game;
            Instance = this;

                var display = GetMouseDisplay();
            if (display != -1)
            {
                _screenDeviceName = Sdl.Display.GetDisplayName(display);

                Sdl.Rectangle bounds;
                Sdl.Display.GetBounds(display, out bounds);

                var x = bounds.X + (bounds.Width - GraphicsDeviceManager.DefaultBackBufferWidth) / 2;
                var y = bounds.Y + (bounds.Height - GraphicsDeviceManager.DefaultBackBufferHeight) / 2;
                _position = new Point(x, y);
            }
            
            Sdl.SetHint("SDL_VIDEO_MINIMIZE_ON_FOCUS_LOSS", "0");
            Sdl.SetHint("SDL_JOYSTICK_ALLOW_BACKGROUND_EVENTS", "1");

            using (
                var stream =
                    Assembly.GetEntryAssembly().GetManifestResourceStream(Assembly.GetEntryAssembly().EntryPoint.DeclaringType.Namespace + ".Icon.bmp") ??
                    Assembly.GetEntryAssembly().GetManifestResourceStream("Icon.bmp") ??
                    Assembly.GetExecutingAssembly().GetManifestResourceStream("MonoGame.bmp"))
            {
                if (stream != null)
                    using (var br = new BinaryReader(stream))
                    {
                        try
                        {
                            var src = Sdl.RwFromMem(br.ReadBytes((int)stream.Length), (int)stream.Length);
                            _icon = Sdl.LoadBMP_RW(src, 1);
                        }
                        catch { }
                    }
            }
        }

        internal void CreateWindow(PresentationParameters pp)
        {
            if (_handle != IntPtr.Zero)
                Sdl.Window.Destroy(_handle);

            _surfaceFormat = pp.BackBufferFormat.GetColorFormat();
            _depthStencilFormat = pp.DepthStencilFormat;
            _multisampleCount = pp.MultiSampleCount;

            var initflags =
                Sdl.Window.State.OpenGL |
                Sdl.Window.State.InputFocus |
                Sdl.Window.State.MouseFocus;

            if (pp.IsFullScreen)
            {
                if (pp.HardwareModeSwitch)
                    initflags |= Sdl.Window.State.Fullscreen;
                else
                    initflags |= Sdl.Window.State.FullscreenDesktop;

                IsFullScreen = true;
            }

            _hardwareSwitch = pp.HardwareModeSwitch;

            _handle = Sdl.Window.Create(Title,
                _position.X, _position.Y,
                pp.BackBufferWidth, pp.BackBufferHeight, initflags);

            // The GraphicsDevice needs to recreate its GraphicsContext for the new window
            if (_game.GraphicsDevice != null)
                _game.GraphicsDevice.RecreateContext(Handle);

            if (_icon != IntPtr.Zero)
                Sdl.Window.SetIcon(_handle, _icon);

            Sdl.Window.SetBordered(_handle, _borderless ? 0 : 1);
            Sdl.Window.SetResizable(_handle, _resizable);

            SetCursorVisible(_mouseVisible);

            if (!_wasMoved)
                CenterWindow();

            _supressMoved = true;
        }

        ~SdlGameWindow()
        {
            Dispose(false);
        }

        private static int GetMouseDisplay()
        {
            int x, y;
            Sdl.Mouse.GetGlobalState(out x, out y);

            var displayCount = Sdl.Display.GetNumVideoDisplays();
            for (var i = 0; i < displayCount; i++)
            {
                Sdl.Rectangle rect;
                Sdl.Display.GetBounds(i, out rect);

                if (x >= rect.X && x < rect.X + rect.Width &&
                    y >= rect.Y && y < rect.Y + rect.Height)
                    return i;
            }

            return -1;
        }

        public void SetCursorVisible(bool visible)
        {
            _mouseVisible = visible;
            Sdl.Mouse.ShowCursor(visible ? 1 : 0);
        }

        public override void BeginScreenDeviceChange(bool willBeFullScreen)
        {
        }

        public override void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight)
        {
            _screenDeviceName = screenDeviceName;

            var pp = _game.GraphicsDevice.PresentationParameters;

            // if depth format, back buffer format or multisample count of the back buffer
            // changed we need to recreate the window
            if (pp.DepthStencilFormat != _depthStencilFormat ||
                pp.BackBufferFormat.GetColorFormat() != _surfaceFormat ||
                pp.MultiSampleCount != _multisampleCount)
            {
                CreateWindow(pp);
                return;
            }

            if (!IsFullScreen && pp.IsFullScreen ||
                IsFullScreen && pp.IsFullScreen && _hardwareSwitch != pp.HardwareModeSwitch)
            {
                IsFullScreen = true;
                // store width and height for recovering later
                _storedWidth = pp.BackBufferWidth;
                _storedHeight = pp.BackBufferHeight;
                // enter full screen
                var fullscreenFlag = pp.HardwareModeSwitch ? Sdl.Window.State.Fullscreen : Sdl.Window.State.FullscreenDesktop;
                Sdl.Window.SetFullscreen(Handle, fullscreenFlag);
                _hardwareSwitch = pp.HardwareModeSwitch;
            }
            else if (IsFullScreen && ! pp.IsFullScreen)
            {
                // exit full screen
                IsFullScreen = false;
                Sdl.Window.SetFullscreen(Handle, 0);
                Sdl.Window.SetPosition(Handle, _position.X, _position.Y);
                Sdl.Window.SetSize(Handle, _storedWidth, _storedHeight);
            }
            else if (!IsFullScreen)
            {
                Sdl.Window.SetSize(Handle, pp.BackBufferWidth, pp.BackBufferHeight);
                if (!_wasMoved)
                    CenterWindow();
            }
            _supressMoved = true;
        }

        private void CenterWindow()
        {
            // If this window is resizable, there is a bug in SDL 2.0.4 where
            // after the window gets resized, window position information
            // becomes wrong (for me it always returned 10 8). Solution is
            // to not try and set the window position because it will be wrong.
            if (Sdl.Patch <= 4 && AllowUserResizing)
                return;

            int x, y;
            Sdl.Window.GetPosition(Handle, out x, out y);
            if (_position.X == x && _position.Y == y)
                return;

            var di = Sdl.Window.GetDisplayIndex(_handle);
            Sdl.Rectangle displayBounds;
            Sdl.Display.GetBounds(di, out displayBounds);

            int w, h;
            Sdl.Window.GetSize(_handle, out w, out h);

            x = displayBounds.X + (displayBounds.Width - w) / 2;
            y = displayBounds.Y + (displayBounds.Height - h) / 2;
            _position = new Point(x, y);
            Sdl.Window.SetPosition(Handle, x, y);
        }

        internal void Moved()
        {
            if (_supressMoved)
            {
                _supressMoved = false;
                return;
            }

            // SDL raises a lot of these events, sometimes when switching to full screen
            if (!IsFullScreen)
            {
                _wasMoved = true;
                int x, y;
                Sdl.Window.GetPosition(Handle, out x, out y);
                _position = new Point(x, y);
            }
        }

        public void ClientResize(int width, int height)
        {
            // TODO We need to detect somehow if user resizing ended, if not don't do anything yet,
            //      that's how XNA and the DX implementation do it.
            //      This hoewever does not seem to be possible with the SDL API :(

            if (_game.GraphicsDevice.PresentationParameters.BackBufferWidth == width &&
                _game.GraphicsDevice.PresentationParameters.BackBufferHeight == height) {
                return;
            }

            Console.WriteLine($"Resizing backbuffer ({width}, {height})");

            _game.graphicsDeviceManager.PreferredBackBufferWidth = width;
            _game.graphicsDeviceManager.PreferredBackBufferHeight = height;
            _game.GraphicsDevice.Viewport = new Viewport(0, 0, width, height);

            _game.graphicsDeviceManager.ApplyChanges();

            OnClientSizeChanged();
        }

        public void CallTextInput(char c, Keys key = Keys.None)
        {
            OnTextInput(this, new TextInputEventArgs(c, key));
        }

        protected internal override void SetSupportedOrientations(DisplayOrientation orientations)
        {
            // Nothing to do here
        }

        protected override void SetTitle(string title)
        {
            if (_handle != IntPtr.Zero)
            Sdl.Window.SetTitle(_handle, title);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            Sdl.Window.Destroy(_handle);
            _handle = IntPtr.Zero;

            if (_icon != IntPtr.Zero)
                Sdl.FreeSurface(_icon);

            _disposed = true;
        }

        public IntPtr GetDummyWindowHandle()
        {
            return Sdl.Window.Create(string.Empty, 0, 0, 1, 1, Sdl.Window.State.Hidden | Sdl.Window.State.OpenGL);
        }
    }
}
