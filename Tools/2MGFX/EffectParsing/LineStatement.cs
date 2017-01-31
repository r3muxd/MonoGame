// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Text;

namespace TwoMGFX.EffectParsing
{
    public sealed class LineStatement : Statement
    {
        public LineStatement(StringBuilder stringBuilder, int start, int line, int column, int end, ParentStatement parent, StatementClass cls)
            : base(stringBuilder, start, line, column, parent, cls)
        {
            End = end;
        }
    }
}