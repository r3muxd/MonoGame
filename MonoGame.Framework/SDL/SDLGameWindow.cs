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
                int x, y, width, height;
                Sdl.Window.GetPosition(Handle, out x, out y);
                Sdl.Window.GetSize(Handle, out width, out height);
                return new Rectangle(x, y, width, height);
            }
        }

        public override Point Position
        {
            get
            {
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

                _userMoved = true;
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

        public bool IsFullScreen { get; private set; }

        private IntPtr _handle, _icon;
        private bool _disposed;
        private bool _resizable, _borderless, _mouseVisible, _hardwareSwitch;
        private string _screenDeviceName = string.Empty;
        private int _storedWidth;
        private int _storedHeight;
        private Point _position;
        private bool _supressMove;
        private bool _userMoved;

        private ColorFormat _surfaceFormat;
        private DepthFormat _depthStencilFormat;

        public SdlGameWindow()
        {
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

        protected override void CreateWindow(PresentationParameters pp)
        {
            _supressMove = true;

            if (_handle != IntPtr.Zero)
                Sdl.Window.Destroy(_handle);

            _surfaceFormat = pp.BackBufferFormat.GetColorFormat();
            _depthStencilFormat = pp.DepthStencilFormat;

            Sdl.GL.SetAttribute(Sdl.GL.Attribute.RedSize, _surfaceFormat.R);
            Sdl.GL.SetAttribute(Sdl.GL.Attribute.GreenSize, _surfaceFormat.G);
            Sdl.GL.SetAttribute(Sdl.GL.Attribute.BlueSize, _surfaceFormat.B);
            Sdl.GL.SetAttribute(Sdl.GL.Attribute.AlphaSize, _surfaceFormat.A);

            switch (_depthStencilFormat)
            {
                case DepthFormat.None:
                    Sdl.GL.SetAttribute(Sdl.GL.Attribute.DepthSize, 0);
                    Sdl.GL.SetAttribute(Sdl.GL.Attribute.StencilSize, 0);
                    break;
                case DepthFormat.Depth16:
                    Sdl.GL.SetAttribute(Sdl.GL.Attribute.DepthSize, 16);
                    Sdl.GL.SetAttribute(Sdl.GL.Attribute.StencilSize, 0);
                    break;
                case DepthFormat.Depth24:
                    Sdl.GL.SetAttribute(Sdl.GL.Attribute.DepthSize, 24);
                    Sdl.GL.SetAttribute(Sdl.GL.Attribute.StencilSize, 0);
                    break;
                case DepthFormat.Depth24Stencil8:
                    Sdl.GL.SetAttribute(Sdl.GL.Attribute.DepthSize, 24);
                    Sdl.GL.SetAttribute(Sdl.GL.Attribute.StencilSize, 8);
                    break;
            }

            Sdl.GL.SetAttribute(Sdl.GL.Attribute.DoubleBuffer, 1);
            Sdl.GL.SetAttribute(Sdl.GL.Attribute.ContextMajorVersion, 2);
            Sdl.GL.SetAttribute(Sdl.GL.Attribute.ContextMinorVersion, 1);

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
                _hardwareSwitch = pp.HardwareModeSwitch;
            }

            _handle = Sdl.Window.Create(Title,
                                        _position.X, _position.Y,
                                        pp.BackBufferWidth, pp.BackBufferHeight,
                                        initflags);

            if (_icon != IntPtr.Zero)
                Sdl.Window.SetIcon(_handle, _icon);

            Sdl.Window.SetBordered(_handle, _borderless ? 0 : 1);
            Sdl.Window.SetResizable(_handle, _resizable);

            SetCursorVisible(_mouseVisible);
            if (!_userMoved)
                CenterWindow();

            _supressMove = true;
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
                {
                    return i;
                }
            }

            return -1;
        }

        public void SetCursorVisible(bool visible)
        {
            _mouseVisible = visible;
            Sdl.Mouse.ShowCursor(visible ? 1 : 0);
        }

        public override void OnPresentationChanged(PresentationParameters pp)
        {
            _supressMove = true;
            // TODO we need to check if multisamplecount
            // changed and recreate the window if it did

            var width = pp.BackBufferWidth;
            var height = pp.BackBufferHeight;

            if (pp.DepthStencilFormat != _depthStencilFormat ||
                pp.BackBufferFormat.GetColorFormat() != _surfaceFormat)
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
            else if (IsFullScreen && !pp.IsFullScreen)
            {
                // exit full screen
                IsFullScreen = false;
                Sdl.Window.SetFullscreen(Handle, 0);
                Sdl.Window.SetPosition(Handle, _position.X, _position.Y);
                width = _storedWidth;
                height = _storedHeight;
            }

            if (!IsFullScreen)
            {
                Sdl.Window.SetSize(Handle, width, height);
                if (!_userMoved)
                    CenterWindow();
            }
            _supressMove = true;
        }

        private void CenterWindow()
        {
            // If this window is resizable, there is a bug in SDL 2.0.4 where
            // after the window gets resized, window position information
            // becomes wrong (for me it always returned 10 8). Solution is
            // to not try and set the window position because it will be wrong.
            if (Sdl.Patch <= 4 && AllowUserResizing)
                return;

            var di = Sdl.Window.GetDisplayIndex(_handle);
            Sdl.Rectangle displayBounds;
            Sdl.Display.GetBounds(di, out displayBounds);

            int w, h;
            Sdl.Window.GetSize(_handle, out w, out h);

            var x = displayBounds.X + (displayBounds.Width - w) / 2;
            var y = displayBounds.Y + (displayBounds.Height - h) / 2;
            _supressMove = true;
            Sdl.Window.SetPosition(Handle, x, y);
        }

        internal void Moved()
        {
            if (_supressMove)
            {
                _supressMove = false;
                return;
            }
            _userMoved = true;
            int x, y;
            Sdl.Window.GetPosition(_handle, out x, out y);
            _position = new Point(x, y);
        }

        public void ClientResize(int width, int height)
        {
            // TODO detect somehow if the user resize ended, if not don't do anything yet
            //      this however does not seem to be possible with the SDL API :(

            var pp = GraphicsDeviceManager.GraphicsDevice.PresentationParameters;
            if (pp.BackBufferWidth == width && pp.BackBufferHeight == height)
                return;

            GraphicsDeviceManager.PreferredBackBufferWidth = width;
            GraphicsDeviceManager.PreferredBackBufferHeight = height;
            GraphicsDeviceManager.GraphicsDevice.Viewport = new Viewport(0, 0, width, height);
            GraphicsDeviceManager.ApplyChanges();

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
    }
}
