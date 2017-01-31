// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace TwoMGFX.EffectParsing
{
    [Serializable]
    public class MismatchedBlockDelimiterException : Exception
    {
        public int Line { get; }
        public int Column { get; }

        private MismatchedBlockDelimiterException(int line, int column, string message)
            : base(message)
        {
            Line = line;
            Column = column;
        }

        internal static MismatchedBlockDelimiterException UnexpectedEof(OpenedBlockInfo e, int line, int column)
        {
            return new MismatchedBlockDelimiterException(line, column,
                    $"Parsing Error: Unexpected end of file, expected '{e.OpenChar}' first to close the block starting at (Ln: {e.Line}; Col: {e.Column})");
        }

        internal static MismatchedBlockDelimiterException UnexpectedClosingChar(char actual, int line, int column)
        {
            return new MismatchedBlockDelimiterException(line, column,
                    $"Parsing Error: Unexpected closing character '{actual}', there are no open blocks.");
        }

        internal static MismatchedBlockDelimiterException WrongClosingChar(OpenedBlockInfo expected, char actual, int line, int column)
        {
            return new MismatchedBlockDelimiterException(line, column,
                $"Parsing Error: Unexpected block closing character '{actual}', expected " +
                $"'{expected.OpenChar}' to close the block starting at (Ln: {expected.Line}; Col: {expected.Column})");
        }
    }
}