// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TwoMGFX.EffectParsing
{
    public partial class Parser
    {
        public bool Parsed { get; private set; }

        private readonly Dictionary<StatementClass, List<ClassFilter>> _matchingRules = new Dictionary<StatementClass, List<ClassFilter>>();

        public void AddSeparator(char c)
        {
            _lineSeparators.Add(c);
        }

        public void AddSeparator(BlockDelimiter cs)
        {
            _endBlockChars.Add(cs.Open, cs.Close);
            _openBlockSeparators.Add(cs.Open);
            _closeBlockSeparators.Add(cs.Close);
        }

        public void AddClass(StatementClass cls, Func<string, bool> filter, params StatementClass[] parents)
        {
            if (!parents.Any())
                parents = new[] { StatementClass.TopLevel };

            foreach (var parent in parents)
            {
                if (_matchingRules.ContainsKey(parent))
                {
                    _matchingRules[parent].Add(new ClassFilter(cls, filter));
                }
                else
                {
                    var ls = new List<ClassFilter> {new ClassFilter(cls, filter)};
                    _matchingRules.Add(parent, ls);
                }
            }
        }

        #region Parsing State

        private ParentStatement _parentStatement;
        private readonly Stack<OpenedBlockInfo> _openedBlocks = new Stack<OpenedBlockInfo>();

        private readonly Dictionary<char, char> _endBlockChars = new Dictionary<char, char>();
        private readonly List<char> _lineSeparators = new List<char>();
        private readonly List<char> _openBlockSeparators = new List<char>();
        private readonly List<char> _closeBlockSeparators = new List<char>();

        private int _head;
        private int _line;
        private int _column;
        private int _nextLine;
        private int _nextColumn;

        private StringBuilder _text;

        private char CurrentChar => _text[_head];
        private StatementClass CurrentBlockClass => _parentStatement.Class;
        private bool InSkippedBlock => _parentStatement.Class == StatementClass.Skipped;

        #endregion

        #region Parsing

        public StatementCollection Parse(StringBuilder text)
        {
            if (Parsed)
                throw new InvalidOperationException("Already parsed.");

            _text = text;
            var sc = new StatementCollection(_text);

            _parentStatement = sc;

            _line = 1;
            _column = 1;
            _nextLine = _line;
            _nextColumn = _column;

            SkipWhitespace(_text);

            int nextHead;
            SeparatorType separatorType;
            while (GetNextSeparator(out nextHead, out separatorType))
            {
                var statementText = text.ToString(_head, nextHead - _head);
                var cls = InSkippedBlock ? StatementClass.Skipped : Classify(statementText);

                if (separatorType == SeparatorType.Line)
                {
                    // don't add statements from the 'Skipped' class and empty statements
                    if (cls != StatementClass.Skipped && nextHead - _head > 1)
                    {
                        var statement = new LineStatement(_text, _head, _line, _column, nextHead, _parentStatement, cls);
                        _parentStatement.Push(statement);
                    }
                }
                else if (separatorType == SeparatorType.OpenBlock)
                {
                    OpenBlock(_text[nextHead]);

                    var newParentStatement = new BlockStatement(_text, _head, _line, _column, nextHead, _parentStatement, cls);
                    if (cls != StatementClass.Skipped)
                        _parentStatement.Push(newParentStatement);

                    _parentStatement = newParentStatement;
                }
                else if (separatorType == SeparatorType.CloseBlock)
                {
                    _parentStatement.Close(nextHead);

                    // throw a sensible exception when the first char is a closing block character
                    if (_head == 0)
                        throw MismatchedBlockDelimiterException.UnexpectedClosingChar(text[0], 1, 1);

                    CloseBlock(_text[_head - 1]);

                    _parentStatement = _parentStatement.Parent;
                }

                _head = nextHead;
                IncrementHead();
                SkipWhitespace(_text);

                _line = _nextLine;
                _column = _nextColumn;
            }

            // we can't have any open blocks left
            if (_openedBlocks.Any())
                throw MismatchedBlockDelimiterException.UnexpectedEof(_openedBlocks.Peek(), _nextLine, _nextColumn);

            return sc;
        }

        private StatementClass Classify(string text)
        {
            if (!_matchingRules.ContainsKey(CurrentBlockClass))
                return StatementClass.Skipped;

            return _matchingRules[CurrentBlockClass]
                .Where(r => r.Matches(text))
                .Select(r => r.Class).FirstOrDefault();
        }

        private void SkipWhitespace(StringBuilder sb)
        {
            while (_head < sb.Length)
            {
                if (!char.IsWhiteSpace(CurrentChar))
                    break;

                IncrementHead();
            }
        }

        private void IncrementHead()
        {
            if (CurrentChar == '\n')
            {
                _nextLine++;
                _nextColumn = 1;
            }
            else
                _nextColumn++;

            _head++;
        }

        private void OpenBlock(char openChar)
        {
            var info = new OpenedBlockInfo(openChar, _endBlockChars[openChar], _nextLine, _nextColumn);
            _openedBlocks.Push(info);
        }

        private void CloseBlock(char closeChar)
        {
            if (!_openedBlocks.Any())
                throw MismatchedBlockDelimiterException.UnexpectedClosingChar(closeChar, _nextLine, _nextColumn);

            var openCharInfo = _openedBlocks.Pop();
            if (openCharInfo.CloseChar != CurrentChar)
                throw MismatchedBlockDelimiterException.WrongClosingChar(openCharInfo, closeChar, _nextColumn, _nextLine);
        }

        private bool GetNextSeparator(out int separatorPos, out SeparatorType separatorType)
        {
            
            for (separatorPos = _head; separatorPos < _text.Length; separatorPos++)
            {
                var c = _text[separatorPos];
                if (_lineSeparators.Contains(c))
                {
                    separatorType = SeparatorType.Line;
                    return true;
                }
                if (_openBlockSeparators.Contains(c))
                {
                    separatorType = SeparatorType.OpenBlock;
                    return true;
                }
                if (_closeBlockSeparators.Contains(c))
                {
                    separatorType = SeparatorType.CloseBlock;
                    return true;
                }
                if (c == '\n')
                {
                    _nextLine++;
                    _nextColumn = 1;
                }
                else
                {
                    _nextColumn++;
                }
            }
            separatorType = SeparatorType.EOF;

            return false;
        }

        private enum SeparatorType
        {
            Line,
            OpenBlock,
            CloseBlock,
            EOF
        }

        private class ClassFilter
        {
            public StatementClass Class { get; }
            public Func<string, bool> Matches { get; }

            public ClassFilter(StatementClass cls, Func<string, bool> matches)
            {
                Class = cls;
                Matches = matches;
            }
        }

        #endregion
    }
}
