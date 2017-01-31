// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Text;

namespace TwoMGFX.EffectParsing
{
    public sealed class StatementCollection : ParentStatement
    {
        public StatementCollection(StringBuilder sb) : base(sb, 0, 1, 1, null, StatementClass.TopLevel)
        {
            End = sb.Length;
        }
    }
}