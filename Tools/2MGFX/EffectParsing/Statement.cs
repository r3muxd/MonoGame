// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Text;

namespace TwoMGFX.EffectParsing
{
    public abstract class Statement
    {
        protected internal readonly StringBuilder StringBuilder;

        public int Start { get; }
        public int Line { get; }
        public int Column { get; }
        public int End { get; protected set; }

        public int Length => End - Start;

        public string Text => StringBuilder.ToString(Start, Length).Trim();

        public ParentStatement Parent { get; }
        public bool HasParent => Parent != null;

        public int ParentCount
        {
            get
            {
                if (!HasParent)
                    return 0;

                var count = 1;
                var current = Parent;
                while ((current = current.Parent) != null)
                    count++;
                return count;
            }
        }

        public StatementClass Class { get; private set; }

        protected internal Statement(StringBuilder stringBuilder, int start, int line, int column, ParentStatement parent, StatementClass cls)
        {
            StringBuilder = stringBuilder;
            Start = start;
            Line = line;
            Column = column;
            Parent = parent;
            Class = cls;
        }

        public override string ToString()
        {
            return Text;
        }

        public void ToWhitespace()
        {
            for (var i = Start; i <= End; i++)
            {
                if (!char.IsWhiteSpace(StringBuilder[i]))
                    StringBuilder[i] = ' ';
            }
                
        }
    }
}