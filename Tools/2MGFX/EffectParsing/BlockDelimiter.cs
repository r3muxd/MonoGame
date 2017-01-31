// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace TwoMGFX.EffectParsing
{
    public struct BlockDelimiter
    {
        public char Open { get; }
        public char Close { get; }

        public BlockDelimiter(char open, char close)
        {
            Open = open;
            Close = close;
        }
    }
}