// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using EffectParse;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TwoMGFX.EffectParsing
{
    internal partial class EffectParser
    {
        /// <summary>
        /// Parse effect syntax in the given string and fill a parseResult instance with the required information.
        /// Returns true if there were no errors, false otherwise.
        /// </summary>
        public static bool Parse(ShaderProfile profile, string text, out ParseResult parseResult)
        {
            var parser = new EffectParser(profile, text);
            return parser.Parse(out parseResult);
        }

        public static string GetCleanedFiled(ParseResult parseResult)
        {
            foreach (var s in parseResult.TopLevelStatement)
            {
                if (s.Class == StatementClass.Technique)
                {
                    s.ToWhitespace();
                }
                else if (s.Class == StatementClass.Sampler)
                {
                    var bs = s as BlockStatement;
                    if (bs == null)
                        continue;

                    var spaceLoc = bs.HeaderText.IndexOf("=", StringComparison.InvariantCulture);
                    if (spaceLoc != -1)
                        bs.ToWhitespace(spaceLoc);
                    else
                        bs.BodyToWhitespace();
                }
            }
            return parseResult.Content;
        }

        private readonly ShaderProfile _profile;
        private readonly StringBuilder _text;
        private readonly Parser _parser;
        private readonly List<ParseError> _errors;

        private EffectParser(ShaderProfile profile, string text)
        {
            _profile = profile;
            _text = new StringBuilder(text);
            _parser = new Parser();
            _errors = new List<ParseError>();
        }

        public bool Parse(out ParseResult parseResult)
        {
            _parser.AddSeparator(';');
            _parser.AddSeparator(new BlockDelimiter('{', '}'));

            AddTechniqueClasses();
            AddSamplerClasses();

            if (_profile == ShaderProfile.DirectX_11)
                AddHlslClasses();

            StatementCollection statements;
            try
            {
                statements = _parser.Parse(_text);
            }
            catch (MismatchedBlockDelimiterException e)
            {
                _errors.Add(new ParseError(e.Line, e.Column, e.Message));
                parseResult =  new ParseResult(_profile, null, null, _errors);
                return false;
            }

            var shaderInfo = ProcessStatements(statements);

            parseResult = new ParseResult(_profile, statements, shaderInfo, _errors);
            return !_errors.Any();
        }

        #region Error Helpers

        private void AddError(int line, int column, string message)
        {
            var error = new ParseError(line, column, message);
            _errors.Add(error);
        }

        private void AddError(Statement s, string message)
        {
            AddError(s.Line, s.Column, message);
        }

        private bool Check(bool assertion, Statement s, string errorMessage)
        {
            if (!assertion)
                AddError(s.Line, s.Column, errorMessage);
            return assertion;
        }

        private bool SafeToBlockStatement(Statement s, out BlockStatement bs)
        {
            bs = s as BlockStatement;
            if (bs == null)
            {
                AddError(s.Line, s.Column, "Illegal syntax: Expected block instead of statement.");
                return false;
            }
            return true;
        }

        #endregion

        #region Parsing Helpers

        private static bool TryParseBoolAssignment(Statement s, out bool result)
        {
            var boolText = s.Text.GetAfterEquals();
            if (boolText.IsEqualTo("true") || boolText.IsEqualTo("1"))
				result =  true;
		    else if (boolText.IsEqualTo("false") || boolText.IsEqualTo("0"))
		        result = false;
            else
            {
                result = false;
                return false;
            }

            return true;
        }

        private static bool TryParseFloatAssignment(Statement s, out float result)
        {
            var floatText = s.Text.GetAfterEquals();

            floatText = floatText.Replace(" ", "");
            floatText = floatText.TrimEnd('f', 'F');
            return float.TryParse(floatText, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
        }

        private static bool TryParseIntAssignment(Statement s, out int result)
        {
            // We read it as a float and cast it down to
            // an integer to match Microsoft FX behavior.
            float f;
            if (!TryParseFloatAssignment(s, out f))
            {
                result = 0;
                return false;
            }
            result = (int) Math.Floor(f);
            return true;
        }

        private static bool TryParseColorAssignment(Statement s, out Color result)
        {
            var colorText = s.Text.GetAfterEquals();

            result = Color.White;
            uint hexValue;
            if (!uint.TryParse(colorText, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out hexValue))
                return false;

            if (colorText.Length == 8)
            {
                result.R = (byte) ((hexValue >> 16) & 0xFF);
                result.G = (byte) ((hexValue >> 8) & 0xFF);
                result.B = (byte) ((hexValue >> 0) & 0xFF);
            }
            else if (colorText.Length == 10)
            {
                result.R = (byte) ((hexValue >> 24) & 0xFF);
                result.G = (byte) ((hexValue >> 16) & 0xFF);
                result.B = (byte) ((hexValue >> 8) & 0xFF);
                result.A = (byte) ((hexValue >> 0) & 0xFF);
            }

            return true;
        }

        private static bool TryParseEnumAssignment<T>(Statement s, out T result) where T : struct
        {
            var rhs = s.Text.GetAfterEquals();
            return Enum.TryParse(rhs, true, out result);
        }

        private static bool TryParseBlendAssignment(Statement s, out Blend blend)
        {
            if (TryParseEnumAssignment(s, out blend))
                return true;

            // shorthands
            var blendText = s.Text.GetAfterEquals();
            if (blendText.IsEqualTo("SrcColor"))
                blend = Blend.SourceColor;
            else if (blendText.IsEqualTo("InvSrcColor"))
                blend = Blend.InverseSourceColor;
            else if (blendText.IsEqualTo("SrcAlpha"))
                blend = Blend.SourceAlpha;
            else if (blendText.IsEqualTo("InvSrcAlpha"))
                blend = Blend.InverseSourceAlpha;
            else if (blendText.IsEqualTo("DestAlpha"))
                blend = Blend.DestinationAlpha;
            else if (blendText.IsEqualTo("InvDestAlpha"))
                blend = Blend.InverseDestinationAlpha;
            else if (blendText.IsEqualTo("DestColor"))
                blend = Blend.DestinationColor;
            else if (blendText.IsEqualTo("InvDestColor"))
                blend = Blend.InverseDestinationColor;
            else if (blendText.IsEqualTo("SrcAlphaSat"))
                blend = Blend.SourceAlphaSaturation;
            else if (blendText.IsEqualTo("InvBlendFactor"))
                blend = Blend.InverseBlendFactor;
            else
                return false;

            return true;
        }

        private static bool TryParseBlendFuncAssignment(Statement s, out BlendFunction blendFunc)
        {
            if (TryParseEnumAssignment(s, out blendFunc))
                return true;

            // shorthands
            var bfText = s.Text.GetAfterEquals();
            if (bfText.IsEqualTo("RevSubtract"))
                blendFunc = BlendFunction.ReverseSubtract;
            else
                return false;

            return true;
        }

        private static bool TryParseCullModeAssignment(Statement s, out CullMode cullMode)
        {
            if (TryParseEnumAssignment(s, out cullMode))
                return true;

            //shorthands
            var cmText = s.Text.GetAfterEquals();
            if (cmText.IsEqualTo("cw"))
                cullMode = CullMode.CullClockwiseFace;
            else if (cmText.IsEqualTo("ccw"))
                cullMode = CullMode.CullCounterClockwiseFace;
            else if (cmText.IsEqualTo("None"))
                cullMode = CullMode.None;
            else
                return false;

            return true;
        }

        private static bool TryParseStencilOpAssignment(Statement s, out StencilOperation stencilOp)
        {
            if (TryParseEnumAssignment(s, out stencilOp))
                return true;

            //shorthands
            var opText = s.Text.GetAfterEquals();
            if (opText.IsEqualTo("IncrSat"))
                stencilOp = StencilOperation.IncrementSaturation;
            else if (opText.IsEqualTo("DecrSat"))
                stencilOp = StencilOperation.DecrementSaturation;
            else if (opText.IsEqualTo("Incr"))
                stencilOp = StencilOperation.Increment;
            else if (opText.IsEqualTo("Decr"))
                stencilOp = StencilOperation.Decrement;
            else
                return false;

            return true;
        }

        private static bool TryParseTextureAssignment(Statement s, out string textureName)
        {
            var t = s.Text.GetAfterEquals();
            if (t.IsValidIdentifier())
            {
                textureName = t;
                return true;
            }
            var tname = t.TrimStart('(').TrimEnd(')');
            if (tname.IsValidIdentifier())
            {
                textureName = tname;
                return true;
            }
            tname = t.TrimStart('<').TrimEnd('>');
            if (tname.IsValidIdentifier())
            {
                textureName = tname;
                return true;
            }
            textureName = string.Empty;
            return false;
        }

        private static bool TryParseShaderAssignment(Statement s, out string func, out string target)
        {
            var parts = s.Text.GetAfterEquals().TrimEnd(')').TrimEnd().TrimEnd('(').Split((char[]) null, StringSplitOptions.RemoveEmptyEntries);
            func = string.Empty;
            target = string.Empty;

            if (parts.Length == 3 && parts[0].IsEqualTo("compile"))
            {
                target = parts[1];
                func = parts[2];
                return true;
            }

            return false;
        }

        #endregion

        private ShaderInfo ProcessStatements(ParentStatement statements)
        {
            var shaderInfo = new ShaderInfo();
            foreach (var s in statements)
            {
                BlockStatement bs;
                if (!SafeToBlockStatement(s, out bs))
                    continue;

                switch (s.Class)
                {
                    case StatementClass.Technique:
                        var technique = ProcessTechnique(bs);
                        if (technique != null)
                            shaderInfo.Techniques.Add(technique);
                        break;
                    case StatementClass.Sampler:
                        var sampler = ProcessSampler(bs);
                        if (sampler != null)
                            shaderInfo.SamplerStates.Add(sampler.Name, sampler);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return shaderInfo;
        }
    }
}
