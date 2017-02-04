// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using EffectParse;

namespace TwoMGFX.EffectParsing
{
    internal class ParseResult
    {
        public string Content => TopLevelStatement.Text;
        public ShaderProfile ShaderProfile { get; }
        public StatementCollection TopLevelStatement { get; }
        public ShaderInfo ShaderInfo { get; }
        public List<ParseError> Errors { get; }

        public ParseResult(ShaderProfile shaderProfile, StatementCollection topLevelStatement, ShaderInfo shaderInfo, List<ParseError> errors)
        {
            ShaderProfile = shaderProfile;
            TopLevelStatement = topLevelStatement;
            ShaderInfo = shaderInfo;
            Errors = errors;
        }
    }
}