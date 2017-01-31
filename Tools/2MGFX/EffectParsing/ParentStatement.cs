// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace TwoMGFX.EffectParsing
{
    public class ParentStatement : Statement, IEnumerable<Statement>
    {
        public List<Statement> Statements { get; }

        public ParentStatement(StringBuilder sb, int start, int line, int column, ParentStatement parent, StatementClass cls)
            : base(sb, start, line, column, parent, cls)
        {
            Statements = new List<Statement>();
        }

        public void Push(Statement statement)
        {
            Statements.Add(statement);
        }

        public void Close(int end)
        {
            End = end;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<Statement> GetEnumerator()
        {
            return Statements.GetEnumerator();
        }
    }
}