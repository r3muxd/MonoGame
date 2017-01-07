// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

namespace MonoGame.Tests
{
	partial class GameTest
    {
		public static class Methods
        {
			[TestFixture]
			public class Run : FixtureBase
            {
				[Test]
                [Ignore("MG needs a GraphicsDeviceManager created before calling run," +
                        "We need to fix this so a run succeeds without a GDM")]
				public void Can_only_be_called_once ()
				{
					Game.Run ();
					Assert.Throws<InvalidOperationException> (() => Game.Run ());
				}
			}
		}
	}
}
