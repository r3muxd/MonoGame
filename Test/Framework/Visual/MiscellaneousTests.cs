// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using NUnit.Framework;

using MonoGame.Tests.Components;

namespace MonoGame.Tests.Visual {
	[TestFixture]
	class MiscellaneousTests : VisualTestFixtureBase
    {
		[Test]
		public void DrawOrder_falls_back_to_order_of_addition_to_Game ()
		{
			Game.PreDrawWith += (sender, e) => {
				Game.GraphicsDevice.Clear (Color.CornflowerBlue);
			};

			Game.Components.Add (new ImplicitDrawOrderComponent (Game));
			RunMultiFrameTest (captureCount: 5);
		}
		
		[TestCase(true)]
		[TestCase(false)]
        [Ignore("FIx this test")] // TODO
		public void TexturedQuad_lighting (bool enableLighting)
		{
			Game.Components.Add (new TexturedQuadComponent (Game, enableLighting));
			RunSingleFrameTest ();
		}

		[Test]
        [Ignore("FIx this test")] // TODO
		public void SpaceshipModel ()
		{
			Game.Components.Add (new SpaceshipModelDrawComponent(Game));
			RunMultiFrameTest (captureCount: 10, captureStride: 2);
		}
	}
}
