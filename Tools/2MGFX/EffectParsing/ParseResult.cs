using System.Collections.Generic;
using System.Linq;
using System.Text;
using EffectParse;

namespace TwoMGFX.EffectParsing
{
    internal class ParseResult
    {
        public StringBuilder Text { get; }
        public ShaderProfile ShaderProfile { get; }
        public StatementCollection TopLevelStatement { get; }
        public ShaderInfo ShaderInfo { get; }
        public List<ParseError> Errors { get; }

        public ParseResult(StringBuilder text, ShaderProfile shaderProfile, StatementCollection topLevelStatement, ShaderInfo shaderInfo, List<ParseError> errors)
        {
            Text = text;
            ShaderProfile = shaderProfile;
            TopLevelStatement = topLevelStatement;
            ShaderInfo = shaderInfo;
            Errors = errors;
        }
    }
}