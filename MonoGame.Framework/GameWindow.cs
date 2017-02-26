// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.ComponentModel;

namespace Microsoft.Xna.Framework {
    public abstract partial class GameWindow
    {
        #region Properties

        [DefaultValue(false)]
        public abstract bool AllowUserResizing { get; set; }

        public abstract Rectangle ClientBounds { get; }

        internal bool _allowAltF4 = true;

        /// <summary>
        /// Gets or sets a bool that enables usage of Alt+F4 for window closing on desktop platforms. Value is true by default.
        /// </summary>
        public virtual bool AllowAltF4 { get { return _allowAltF4; } set { _allowAltF4 = value; } }

#if (WINDOWS && !WINRT) || DESKTOPGL
        /// <summary>
        /// The location of this window on the desktop, eg: global coordinate space
        /// which stretches across all screens.
        /// </summary>
        public abstract Point Position { get; set; }
#endif

        public abstract DisplayOrientation CurrentOrientation { get; }

        public abstract IntPtr Handle { get; }

        protected bool CreatedWindow
        {
            get { return Handle != IntPtr.Zero; }
        }

        public abstract string ScreenDeviceName { get; }

        private string _title = MonoGame.Utilities.AssemblyHelper.GetDefaultWindowTitle();

        /// <summary>
        /// Gets or sets the title of the game window.
        /// </summary>
        /// <remarks>
        /// For Windows 8 and Windows 10 UWP this has no effect. For these platforms the title should be
        /// set by using the DisplayName property found in the app manifest file.
        /// </remarks>
        public string Title
        {
            get { return _title; }
            set
            {
                if (_title != value)
                {
                    SetTitle(value);
                    _title = value;
                }
            }
        }

        /// <summary>
        /// Determines whether the border of the window is visible. Currently only supported on the WinDX and WinGL/Linux platforms.
        /// </summary>
        /// <exception cref="System.NotImplementedException">
        /// Thrown when trying to use this property on a platform other than the WinDX and WinGL/Linux platforms.
        /// </exception>
        public virtual bool IsBorderless
        {
            get
            {
                return false;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        protected GraphicsDeviceManager GraphicsDeviceManager { get; private set; }

        internal MouseState MouseState;
	    internal TouchPanelState TouchPanelState;

        protected GameWindow()
        {
            TouchPanelState = new TouchPanelState(this);
        }

        #endregion Properties

        #region Events

        public event EventHandler<EventArgs> ClientSizeChanged;
		public event EventHandler<EventArgs> OrientationChanged;
		public event EventHandler<EventArgs> ScreenDeviceNameChanged;

#if WINDOWS || WINDOWS_UAP || DESKTOPGL|| ANGLE

        /// <summary>
		/// Use this event to retrieve text for objects like textbox's.
		/// This event is not raised by noncharacter keys.
		/// This event also supports key repeat.
		/// For more information this event is based off:
		/// http://msdn.microsoft.com/en-AU/library/system.windows.forms.control.keypress.aspx
		/// </summary>
		/// <remarks>
		/// This event is only supported on the Windows DirectX, Windows OpenGL and Linux platforms.
		/// </remarks>
		public event EventHandler<TextInputEventArgs> TextInput;
#endif

        #endregion Events

        /// <summary>
        /// Binds the given graphics device manager to this <see cref="GameWindow"/>.
        /// </summary>
        public void BindGraphicsDeviceManager(GraphicsDeviceManager gdm)
        {
            if (GraphicsDeviceManager != null)
            {
                GraphicsDeviceManager.CreatingDevice -= CreatingDeviceHandler;
                GraphicsDeviceManager.PresentationChanged -= PresentationChangedHandler;
            }

            GraphicsDeviceManager = gdm;
            GraphicsDeviceManager.CreatingDevice += CreatingDeviceHandler;
            GraphicsDeviceManager.PresentationChanged += PresentationChangedHandler;
        }

        private void CreatingDeviceHandler(object sender, PresentationChangedEventArgs e)
        {
            if (CreatedWindow)
                return;

            CreateWindow(e.Parameters);
            ((GraphicsDeviceManager) sender).WindowHandle = Handle;
            e.Parameters.DeviceWindowHandle = Handle;
        }

        /// <summary>
        /// Create the native window.
        /// </summary>
        /// <param name="pp">The PresentationParameters for the window creation.</param>
        protected abstract void CreateWindow(PresentationParameters pp);

        private void PresentationChangedHandler(object sender, PresentationChangedEventArgs args)
        {
            OnPresentationChanged(args.Parameters);
        }

        /// <summary>
        /// Called when <see cref="GraphicsDevice"/> <see cref="PresentationParameters"/> changed
        /// to update the <see cref="GameWindow"/> accordingly.
        /// </summary>
        /// <param name="pp">The updated <see cref="PresentationParameters"/>.</param>
        public abstract void OnPresentationChanged(PresentationParameters pp);

		protected void OnActivated ()
		{
		}

		protected void OnClientSizeChanged ()
		{
			if (ClientSizeChanged != null)
				ClientSizeChanged (this, EventArgs.Empty);
		}

		protected void OnDeactivated ()
		{
		}
         
		protected void OnOrientationChanged ()
		{
			if (OrientationChanged != null)
				OrientationChanged (this, EventArgs.Empty);
		}

		protected void OnPaint ()
		{
		}

		protected void OnScreenDeviceNameChanged ()
		{
			if (ScreenDeviceNameChanged != null)
				ScreenDeviceNameChanged (this, EventArgs.Empty);
		}

#if WINDOWS || WINDOWS_UAP || DESKTOPGL || ANGLE
		protected void OnTextInput(object sender, TextInputEventArgs e)
		{
			if (TextInput != null)
				TextInput(sender, e);
		}
#endif

		protected internal abstract void SetSupportedOrientations (DisplayOrientation orientations);
		protected abstract void SetTitle (string title);

#if DIRECTX && WINDOWS
        public static GameWindow Create(Game game, int width, int height)
        {
            var window = new MonoGame.Framework.WinFormsGameWindow((MonoGame.Framework.WinFormsGamePlatform)game.Platform);
            window.Initialize(width, height);

            return window;
        }
#endif
    }
}
