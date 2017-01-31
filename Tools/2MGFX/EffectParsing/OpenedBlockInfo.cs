// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace TwoMGFX.EffectParsing
{
    internal struct OpenedBlockInfo
    {
        public OpenedBlockInfo(char openChar, char closeChar, int line, int column)
        {
            OpenChar = openChar;
            CloseChar = closeChar;
            Line = line;
            Column = column;
        }

        public char OpenChar { get; }
        public char CloseChar { get; }
        public int Line { get; }
        public int Column { get; }
    }
}