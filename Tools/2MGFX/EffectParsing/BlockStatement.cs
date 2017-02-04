// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Text;

namespace TwoMGFX.EffectParsing
{
    public sealed class BlockStatement : ParentStatement
    {
        public int Curly { get; }

        public string HeaderText => StringBuilder.ToString(Start, Curly - Start).Trim();

        public bool HasHeader => !string.IsNullOrEmpty(HeaderText);

        public BlockStatement(StringBuilder stringBuilder, int start, int line, int column, int headerEnd, ParentStatement parent, StatementClass cls)
            : base(stringBuilder, start, line, column, parent, cls)
        {
            Curly = headerEnd;
            End = headerEnd;
        }

        public override string ToString()
        {
            return $"{HeaderText} {{";
        }

        public void BodyToWhitespace()
        {
            ToWhitespace(Curly);
        }
    }
}