using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Tests.Framework
{

// TODO: Mac implements its own GameWindow class that cannot 
// be overloaded...  if you hate this hack, go fix it.
#if !MONOMAC

    internal class MockWindow : GameWindow
    {
        public override bool AllowUserResizing { get; set; }

        public override Rectangle ClientBounds
        {
            get { throw new NotImplementedException(); }
        }

        // TODO: Make this common so that all platforms have it!
#if (WINDOWS && !WINRT) || LINUX
        public override Point Position { get; set; }
#endif

        public override DisplayOrientation CurrentOrientation
        {
            get { throw new NotImplementedException(); }
        }

        public override IntPtr Handle
        {
            get { throw new NotImplementedException(); }
        }

        public override string ScreenDeviceName
        {
            get { throw new NotImplementedException(); }
        }

        protected internal override void SetSupportedOrientations(DisplayOrientation orientations)
        {
            throw new NotImplementedException();
        }

        protected override void SetTitle(string title)
        {
            throw new NotImplementedException();
        }

        public override void CreateWindow(PresentationParameters pp)
        {
            throw new NotImplementedException();
        }

        public override void OnPresentationChanged(PresentationParameters pp)
        {
            throw new NotImplementedException();
        }
    }

#endif // !MONOMAC

}