// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    internal class PresentationChangedEventArgs : EventArgs
    {
        public PresentationParameters Parameters { get; }

        public PresentationChangedEventArgs(PresentationParameters parameters)
        {
            Parameters = parameters;
        }
    }
}