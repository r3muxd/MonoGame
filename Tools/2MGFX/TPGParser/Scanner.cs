// Generated by TinyPG v1.3 available at www.codeproject.com

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace TwoMGFX.TPGParser
{
    #region Scanner

    public partial class Scanner
    {
        public string Input;
        public int StartPos = 0;
        public int EndPos = 0;
        public string CurrentFile;
        public int CurrentLine;
        public int CurrentColumn;
        public int CurrentPosition;
        public List<Token> Skipped; // tokens that were skipped
        public Dictionary<TokenType, Regex> Patterns;

        private Token LookAheadToken;
        private List<TokenType> Tokens;
        private List<TokenType> SkipList; // tokens to be skipped
        private readonly TokenType FileAndLine;

        public Scanner()
        {
            Regex regex;
            Patterns = new Dictionary<TokenType, Regex>();
            Tokens = new List<TokenType>();
            LookAheadToken = null;
            Skipped = new List<Token>();

            SkipList = new List<TokenType>();
            SkipList.Add(TokenType.BlockComment);
            SkipList.Add(TokenType.Comment);
            SkipList.Add(TokenType.Whitespace);
            SkipList.Add(TokenType.LinePragma);
            FileAndLine = TokenType.LinePragma;

            regex = new Regex(@"/\*([^*]|\*[^/])*\*/", RegexOptions.Compiled);
            Patterns.Add(TokenType.BlockComment, regex);
            Tokens.Add(TokenType.BlockComment);

            regex = new Regex(@"//[^\n\r]*", RegexOptions.Compiled);
            Patterns.Add(TokenType.Comment, regex);
            Tokens.Add(TokenType.Comment);

            regex = new Regex(@"[ \t\n\r]+", RegexOptions.Compiled);
            Patterns.Add(TokenType.Whitespace, regex);
            Tokens.Add(TokenType.Whitespace);

            regex = new Regex(@"^[ \t]*#line[ \t]*(?<Line>\d*)[ \t]*(\""(?<File>[^\""\\]*(?:\\.[^\""\\]*)*)\"")?\n", RegexOptions.Compiled);
            Patterns.Add(TokenType.LinePragma, regex);
            Tokens.Add(TokenType.LinePragma);

            regex = new Regex(@"pass", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Pass, regex);
            Tokens.Add(TokenType.Pass);

            regex = new Regex(@"technique", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Technique, regex);
            Tokens.Add(TokenType.Technique);

            regex = new Regex(@"sampler1D|sampler2D|sampler3D|samplerCUBE|SamplerState|sampler", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Sampler, regex);
            Tokens.Add(TokenType.Sampler);

            regex = new Regex(@"sampler_state", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.SamplerState, regex);
            Tokens.Add(TokenType.SamplerState);

            regex = new Regex(@"VertexShader", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.VertexShader, regex);
            Tokens.Add(TokenType.VertexShader);

            regex = new Regex(@"PixelShader", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.PixelShader, regex);
            Tokens.Add(TokenType.PixelShader);

            regex = new Regex(@"register", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Register, regex);
            Tokens.Add(TokenType.Register);

            regex = new Regex(@"true|false|0|1", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Boolean, regex);
            Tokens.Add(TokenType.Boolean);

            regex = new Regex(@"[+-]? ?[0-9]?\.?[0-9]+[fF]?", RegexOptions.Compiled);
            Patterns.Add(TokenType.Number, regex);
            Tokens.Add(TokenType.Number);

            regex = new Regex(@"0x[0-9a-f]{6}([0-9a-f][0-9a-f])?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.HexColor, regex);
            Tokens.Add(TokenType.HexColor);

            regex = new Regex(@"[A-Za-z_][A-Za-z0-9_]*", RegexOptions.Compiled);
            Patterns.Add(TokenType.Identifier, regex);
            Tokens.Add(TokenType.Identifier);

            regex = new Regex(@"{", RegexOptions.Compiled);
            Patterns.Add(TokenType.OpenBracket, regex);
            Tokens.Add(TokenType.OpenBracket);

            regex = new Regex(@"}", RegexOptions.Compiled);
            Patterns.Add(TokenType.CloseBracket, regex);
            Tokens.Add(TokenType.CloseBracket);

            regex = new Regex(@"=", RegexOptions.Compiled);
            Patterns.Add(TokenType.Equals, regex);
            Tokens.Add(TokenType.Equals);

            regex = new Regex(@":", RegexOptions.Compiled);
            Patterns.Add(TokenType.Colon, regex);
            Tokens.Add(TokenType.Colon);

            regex = new Regex(@",", RegexOptions.Compiled);
            Patterns.Add(TokenType.Comma, regex);
            Tokens.Add(TokenType.Comma);

            regex = new Regex(@";", RegexOptions.Compiled);
            Patterns.Add(TokenType.Semicolon, regex);
            Tokens.Add(TokenType.Semicolon);

            regex = new Regex(@"\|", RegexOptions.Compiled);
            Patterns.Add(TokenType.Or, regex);
            Tokens.Add(TokenType.Or);

            regex = new Regex(@"\(", RegexOptions.Compiled);
            Patterns.Add(TokenType.OpenParenthesis, regex);
            Tokens.Add(TokenType.OpenParenthesis);

            regex = new Regex(@"\)", RegexOptions.Compiled);
            Patterns.Add(TokenType.CloseParenthesis, regex);
            Tokens.Add(TokenType.CloseParenthesis);

            regex = new Regex(@"\[", RegexOptions.Compiled);
            Patterns.Add(TokenType.OpenSquareBracket, regex);
            Tokens.Add(TokenType.OpenSquareBracket);

            regex = new Regex(@"\]", RegexOptions.Compiled);
            Patterns.Add(TokenType.CloseSquareBracket, regex);
            Tokens.Add(TokenType.CloseSquareBracket);

            regex = new Regex(@"<", RegexOptions.Compiled);
            Patterns.Add(TokenType.LessThan, regex);
            Tokens.Add(TokenType.LessThan);

            regex = new Regex(@">", RegexOptions.Compiled);
            Patterns.Add(TokenType.GreaterThan, regex);
            Tokens.Add(TokenType.GreaterThan);

            regex = new Regex(@"compile", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Compile, regex);
            Tokens.Add(TokenType.Compile);

            regex = new Regex(@"[A-Za-z_][A-Za-z0-9_]*", RegexOptions.Compiled);
            Patterns.Add(TokenType.ShaderModel, regex);
            Tokens.Add(TokenType.ShaderModel);

            regex = new Regex(@"in|attribute", RegexOptions.Compiled);
            Patterns.Add(TokenType.GlslIn, regex);
            Tokens.Add(TokenType.GlslIn);

            regex = new Regex(@"void", RegexOptions.Compiled);
            Patterns.Add(TokenType.Void, regex);
            Tokens.Add(TokenType.Void);

            regex = new Regex(@"[\S]+", RegexOptions.Compiled);
            Patterns.Add(TokenType.Code, regex);
            Tokens.Add(TokenType.Code);

            regex = new Regex(@"^$", RegexOptions.Compiled);
            Patterns.Add(TokenType.EndOfFile, regex);
            Tokens.Add(TokenType.EndOfFile);

            regex = new Regex(@"MinFilter", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.MinFilter, regex);
            Tokens.Add(TokenType.MinFilter);

            regex = new Regex(@"MagFilter", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.MagFilter, regex);
            Tokens.Add(TokenType.MagFilter);

            regex = new Regex(@"MipFilter", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.MipFilter, regex);
            Tokens.Add(TokenType.MipFilter);

            regex = new Regex(@"Filter", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Filter, regex);
            Tokens.Add(TokenType.Filter);

            regex = new Regex(@"Texture", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Texture, regex);
            Tokens.Add(TokenType.Texture);

            regex = new Regex(@"AddressU", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.AddressU, regex);
            Tokens.Add(TokenType.AddressU);

            regex = new Regex(@"AddressV", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.AddressV, regex);
            Tokens.Add(TokenType.AddressV);

            regex = new Regex(@"AddressW", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.AddressW, regex);
            Tokens.Add(TokenType.AddressW);

            regex = new Regex(@"BorderColor", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.BorderColor, regex);
            Tokens.Add(TokenType.BorderColor);

            regex = new Regex(@"MaxAnisotropy", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.MaxAnisotropy, regex);
            Tokens.Add(TokenType.MaxAnisotropy);

            regex = new Regex(@"MaxMipLevel|MaxLod", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.MaxMipLevel, regex);
            Tokens.Add(TokenType.MaxMipLevel);

            regex = new Regex(@"MipLodBias", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.MipLodBias, regex);
            Tokens.Add(TokenType.MipLodBias);

            regex = new Regex(@"Clamp", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Clamp, regex);
            Tokens.Add(TokenType.Clamp);

            regex = new Regex(@"Wrap", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Wrap, regex);
            Tokens.Add(TokenType.Wrap);

            regex = new Regex(@"Mirror", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Mirror, regex);
            Tokens.Add(TokenType.Mirror);

            regex = new Regex(@"Border", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Border, regex);
            Tokens.Add(TokenType.Border);

            regex = new Regex(@"None", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.None, regex);
            Tokens.Add(TokenType.None);

            regex = new Regex(@"Linear", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Linear, regex);
            Tokens.Add(TokenType.Linear);

            regex = new Regex(@"Point", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Point, regex);
            Tokens.Add(TokenType.Point);

            regex = new Regex(@"Anisotropic", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Anisotropic, regex);
            Tokens.Add(TokenType.Anisotropic);

            regex = new Regex(@"AlphaBlendEnable", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.AlphaBlendEnable, regex);
            Tokens.Add(TokenType.AlphaBlendEnable);

            regex = new Regex(@"SrcBlend", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.SrcBlend, regex);
            Tokens.Add(TokenType.SrcBlend);

            regex = new Regex(@"DestBlend", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.DestBlend, regex);
            Tokens.Add(TokenType.DestBlend);

            regex = new Regex(@"BlendOp", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.BlendOp, regex);
            Tokens.Add(TokenType.BlendOp);

            regex = new Regex(@"ColorWriteEnable", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.ColorWriteEnable, regex);
            Tokens.Add(TokenType.ColorWriteEnable);

            regex = new Regex(@"ZEnable", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.ZEnable, regex);
            Tokens.Add(TokenType.ZEnable);

            regex = new Regex(@"ZWriteEnable", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.ZWriteEnable, regex);
            Tokens.Add(TokenType.ZWriteEnable);

            regex = new Regex(@"ZFunc", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.ZFunc, regex);
            Tokens.Add(TokenType.ZFunc);

            regex = new Regex(@"DepthBias", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.DepthBias, regex);
            Tokens.Add(TokenType.DepthBias);

            regex = new Regex(@"CullMode", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.CullMode, regex);
            Tokens.Add(TokenType.CullMode);

            regex = new Regex(@"FillMode", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.FillMode, regex);
            Tokens.Add(TokenType.FillMode);

            regex = new Regex(@"MultiSampleAntiAlias", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.MultiSampleAntiAlias, regex);
            Tokens.Add(TokenType.MultiSampleAntiAlias);

            regex = new Regex(@"ScissorTestEnable", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.ScissorTestEnable, regex);
            Tokens.Add(TokenType.ScissorTestEnable);

            regex = new Regex(@"SlopeScaleDepthBias", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.SlopeScaleDepthBias, regex);
            Tokens.Add(TokenType.SlopeScaleDepthBias);

            regex = new Regex(@"StencilEnable", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.StencilEnable, regex);
            Tokens.Add(TokenType.StencilEnable);

            regex = new Regex(@"StencilFail", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.StencilFail, regex);
            Tokens.Add(TokenType.StencilFail);

            regex = new Regex(@"StencilFunc", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.StencilFunc, regex);
            Tokens.Add(TokenType.StencilFunc);

            regex = new Regex(@"StencilMask", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.StencilMask, regex);
            Tokens.Add(TokenType.StencilMask);

            regex = new Regex(@"StencilPass", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.StencilPass, regex);
            Tokens.Add(TokenType.StencilPass);

            regex = new Regex(@"StencilRef", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.StencilRef, regex);
            Tokens.Add(TokenType.StencilRef);

            regex = new Regex(@"StencilWriteMask", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.StencilWriteMask, regex);
            Tokens.Add(TokenType.StencilWriteMask);

            regex = new Regex(@"StencilZFail", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.StencilZFail, regex);
            Tokens.Add(TokenType.StencilZFail);

            regex = new Regex(@"Never", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Never, regex);
            Tokens.Add(TokenType.Never);

            regex = new Regex(@"Less", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Less, regex);
            Tokens.Add(TokenType.Less);

            regex = new Regex(@"Equal", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Equal, regex);
            Tokens.Add(TokenType.Equal);

            regex = new Regex(@"LessEqual", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.LessEqual, regex);
            Tokens.Add(TokenType.LessEqual);

            regex = new Regex(@"Greater", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Greater, regex);
            Tokens.Add(TokenType.Greater);

            regex = new Regex(@"NotEqual", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.NotEqual, regex);
            Tokens.Add(TokenType.NotEqual);

            regex = new Regex(@"GreaterEqual", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.GreaterEqual, regex);
            Tokens.Add(TokenType.GreaterEqual);

            regex = new Regex(@"Always", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Always, regex);
            Tokens.Add(TokenType.Always);

            regex = new Regex(@"Keep", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Keep, regex);
            Tokens.Add(TokenType.Keep);

            regex = new Regex(@"Zero", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Zero, regex);
            Tokens.Add(TokenType.Zero);

            regex = new Regex(@"Replace", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Replace, regex);
            Tokens.Add(TokenType.Replace);

            regex = new Regex(@"IncrSat", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.IncrSat, regex);
            Tokens.Add(TokenType.IncrSat);

            regex = new Regex(@"DecrSat", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.DecrSat, regex);
            Tokens.Add(TokenType.DecrSat);

            regex = new Regex(@"Invert", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Invert, regex);
            Tokens.Add(TokenType.Invert);

            regex = new Regex(@"Incr", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Incr, regex);
            Tokens.Add(TokenType.Incr);

            regex = new Regex(@"Decr", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Decr, regex);
            Tokens.Add(TokenType.Decr);

            regex = new Regex(@"Red", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Red, regex);
            Tokens.Add(TokenType.Red);

            regex = new Regex(@"Green", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Green, regex);
            Tokens.Add(TokenType.Green);

            regex = new Regex(@"Blue", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Blue, regex);
            Tokens.Add(TokenType.Blue);

            regex = new Regex(@"Alpha", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Alpha, regex);
            Tokens.Add(TokenType.Alpha);

            regex = new Regex(@"All", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.All, regex);
            Tokens.Add(TokenType.All);

            regex = new Regex(@"Cw", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Cw, regex);
            Tokens.Add(TokenType.Cw);

            regex = new Regex(@"Ccw", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Ccw, regex);
            Tokens.Add(TokenType.Ccw);

            regex = new Regex(@"Solid", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Solid, regex);
            Tokens.Add(TokenType.Solid);

            regex = new Regex(@"WireFrame", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.WireFrame, regex);
            Tokens.Add(TokenType.WireFrame);

            regex = new Regex(@"Add", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Add, regex);
            Tokens.Add(TokenType.Add);

            regex = new Regex(@"Subtract", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Subtract, regex);
            Tokens.Add(TokenType.Subtract);

            regex = new Regex(@"RevSubtract", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.RevSubtract, regex);
            Tokens.Add(TokenType.RevSubtract);

            regex = new Regex(@"Min", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Min, regex);
            Tokens.Add(TokenType.Min);

            regex = new Regex(@"Max", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Max, regex);
            Tokens.Add(TokenType.Max);

            regex = new Regex(@"One", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.One, regex);
            Tokens.Add(TokenType.One);

            regex = new Regex(@"SrcColor", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.SrcColor, regex);
            Tokens.Add(TokenType.SrcColor);

            regex = new Regex(@"InvSrcColor", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.InvSrcColor, regex);
            Tokens.Add(TokenType.InvSrcColor);

            regex = new Regex(@"SrcAlpha", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.SrcAlpha, regex);
            Tokens.Add(TokenType.SrcAlpha);

            regex = new Regex(@"InvSrcAlpha", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.InvSrcAlpha, regex);
            Tokens.Add(TokenType.InvSrcAlpha);

            regex = new Regex(@"DestAlpha", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.DestAlpha, regex);
            Tokens.Add(TokenType.DestAlpha);

            regex = new Regex(@"InvDestAlpha", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.InvDestAlpha, regex);
            Tokens.Add(TokenType.InvDestAlpha);

            regex = new Regex(@"DestColor", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.DestColor, regex);
            Tokens.Add(TokenType.DestColor);

            regex = new Regex(@"InvDestColor", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.InvDestColor, regex);
            Tokens.Add(TokenType.InvDestColor);

            regex = new Regex(@"SrcAlphaSat", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.SrcAlphaSat, regex);
            Tokens.Add(TokenType.SrcAlphaSat);

            regex = new Regex(@"BlendFactor", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.BlendFactor, regex);
            Tokens.Add(TokenType.BlendFactor);

            regex = new Regex(@"InvBlendFactor", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.InvBlendFactor, regex);
            Tokens.Add(TokenType.InvBlendFactor);


        }

        public void Init(string input)
        {
            Init(input, "");
        }

        public void Init(string input, string fileName)
        {
            this.Input = input;
            StartPos = 0;
            EndPos = 0;
            CurrentFile = fileName;
            CurrentLine = 1;
            CurrentColumn = 1;
            CurrentPosition = 0;
            LookAheadToken = null;
        }

        public Token GetToken(TokenType type)
        {
            Token t = new Token(this.StartPos, this.EndPos);
            t.Type = type;
            return t;
        }

         /// <summary>
        /// executes a lookahead of the next token
        /// and will advance the scan on the input string
        /// </summary>
        /// <returns></returns>
        public Token Scan(params TokenType[] expectedtokens)
        {
            Token tok = LookAhead(expectedtokens); // temporarely retrieve the lookahead
            LookAheadToken = null; // reset lookahead token, so scanning will continue
            StartPos = tok.EndPos;
            EndPos = tok.EndPos; // set the tokenizer to the new scan position
            CurrentLine = tok.Line + (tok.Text.Length - tok.Text.Replace("\n", "").Length);
            CurrentFile = tok.File;
            return tok;
        }

        /// <summary>
        /// returns token with longest best match
        /// </summary>
        /// <returns></returns>
        public Token LookAhead(params TokenType[] expectedtokens)
        {
            int i;
            int startpos = StartPos;
            int endpos = EndPos;
            int currentline = CurrentLine;
            string currentFile = CurrentFile;
            Token tok = null;
            List<TokenType> scantokens;


            // this prevents double scanning and matching
            // increased performance
            if (LookAheadToken != null 
                && LookAheadToken.Type != TokenType._UNDETERMINED_ 
                && LookAheadToken.Type != TokenType._NONE_) return LookAheadToken;

            // if no scantokens specified, then scan for all of them (= backward compatible)
            if (expectedtokens.Length == 0)
                scantokens = Tokens;
            else
            {
                scantokens = new List<TokenType>(expectedtokens);
                scantokens.AddRange(SkipList);
            }

            do
            {

                int len = -1;
                TokenType index = (TokenType)int.MaxValue;
                string input = Input.Substring(startpos);

                tok = new Token(startpos, endpos);

                for (i = 0; i < scantokens.Count; i++)
                {
                    Regex r = Patterns[scantokens[i]];
                    Match m = r.Match(input);
                    if (m.Success && m.Index == 0 && ((m.Length > len) || (scantokens[i] < index && m.Length == len )))
                    {
                        len = m.Length;
                        index = scantokens[i];  
                    }
                }

                if (index >= 0 && len >= 0)
                {
                    tok.EndPos = startpos + len;
                    tok.Text = Input.Substring(tok.StartPos, len);
                    tok.Type = index;
                }
                else if (tok.StartPos == tok.EndPos)
                {
                    if (tok.StartPos < Input.Length)
                        tok.Text = Input.Substring(tok.StartPos, 1);
                    else
                        tok.Text = "EOF";
                }

                // Update the line and column count for error reporting.
                tok.File = currentFile;
                tok.Line = currentline;
                if (tok.StartPos < Input.Length)
                    tok.Column = tok.StartPos - Input.LastIndexOf('\n', tok.StartPos);

                if (SkipList.Contains(tok.Type))
                {
                    startpos = tok.EndPos;
                    endpos = tok.EndPos;
                    currentline = tok.Line + (tok.Text.Length - tok.Text.Replace("\n", "").Length);
                    currentFile = tok.File;
                    Skipped.Add(tok);
                }
                else
                {
                    // only assign to non-skipped tokens
                    tok.Skipped = Skipped; // assign prior skips to this token
                    Skipped = new List<Token>(); //reset skips
                }

                // Check to see if the parsed token wants to 
                // alter the file and line number.
                if (tok.Type == FileAndLine)
                {
                    var match = Patterns[tok.Type].Match(tok.Text);
                    var fileMatch = match.Groups["File"];
                    if (fileMatch.Success)
                        currentFile = fileMatch.Value.Replace("\\\\", "\\");
                    var lineMatch = match.Groups["Line"];
                    if (lineMatch.Success)
                        currentline = int.Parse(lineMatch.Value, NumberStyles.Integer, CultureInfo.InvariantCulture);
                }
            }
            while (SkipList.Contains(tok.Type));

            LookAheadToken = tok;
            return tok;
        }
    }

    #endregion

    #region Token

    public enum TokenType
    {

            //Non terminal tokens:
            _NONE_  = 0,
            _UNDETERMINED_= 1,

            //Non terminal tokens:
            Start   = 2,
            Technique_Declaration= 3,
            FillMode_Solid= 4,
            FillMode_WireFrame= 5,
            FillModes= 6,
            CullMode_None= 7,
            CullMode_Cw= 8,
            CullMode_Ccw= 9,
            CullModes= 10,
            Colors_None= 11,
            Colors_Red= 12,
            Colors_Green= 13,
            Colors_Blue= 14,
            Colors_Alpha= 15,
            Colors_All= 16,
            Colors_Boolean= 17,
            Colors  = 18,
            ColorsMasks= 19,
            Blend_Zero= 20,
            Blend_One= 21,
            Blend_SrcColor= 22,
            Blend_InvSrcColor= 23,
            Blend_SrcAlpha= 24,
            Blend_InvSrcAlpha= 25,
            Blend_DestAlpha= 26,
            Blend_InvDestAlpha= 27,
            Blend_DestColor= 28,
            Blend_InvDestColor= 29,
            Blend_SrcAlphaSat= 30,
            Blend_BlendFactor= 31,
            Blend_InvBlendFactor= 32,
            Blends  = 33,
            BlendOp_Add= 34,
            BlendOp_Subtract= 35,
            BlendOp_RevSubtract= 36,
            BlendOp_Min= 37,
            BlendOp_Max= 38,
            BlendOps= 39,
            CmpFunc_Never= 40,
            CmpFunc_Less= 41,
            CmpFunc_Equal= 42,
            CmpFunc_LessEqual= 43,
            CmpFunc_Greater= 44,
            CmpFunc_NotEqual= 45,
            CmpFunc_GreaterEqual= 46,
            CmpFunc_Always= 47,
            CmpFunc = 48,
            StencilOp_Keep= 49,
            StencilOp_Zero= 50,
            StencilOp_Replace= 51,
            StencilOp_IncrSat= 52,
            StencilOp_DecrSat= 53,
            StencilOp_Invert= 54,
            StencilOp_Incr= 55,
            StencilOp_Decr= 56,
            StencilOp= 57,
            Render_State_CullMode= 58,
            Render_State_FillMode= 59,
            Render_State_AlphaBlendEnable= 60,
            Render_State_SrcBlend= 61,
            Render_State_DestBlend= 62,
            Render_State_BlendOp= 63,
            Render_State_ColorWriteEnable= 64,
            Render_State_DepthBias= 65,
            Render_State_SlopeScaleDepthBias= 66,
            Render_State_ZEnable= 67,
            Render_State_ZWriteEnable= 68,
            Render_State_ZFunc= 69,
            Render_State_MultiSampleAntiAlias= 70,
            Render_State_ScissorTestEnable= 71,
            Render_State_StencilEnable= 72,
            Render_State_StencilFail= 73,
            Render_State_StencilFunc= 74,
            Render_State_StencilMask= 75,
            Render_State_StencilPass= 76,
            Render_State_StencilRef= 77,
            Render_State_StencilWriteMask= 78,
            Render_State_StencilZFail= 79,
            Render_State_Expression= 80,
            Pass_Declaration= 81,
            VertexShader_Pass_Expression= 82,
            PixelShader_Pass_Expression= 83,
            AddressMode_Clamp= 84,
            AddressMode_Wrap= 85,
            AddressMode_Mirror= 86,
            AddressMode_Border= 87,
            AddressMode= 88,
            TextureFilter_None= 89,
            TextureFilter_Linear= 90,
            TextureFilter_Point= 91,
            TextureFilter_Anisotropic= 92,
            TextureFilter= 93,
            Sampler_State_Texture= 94,
            Sampler_State_MinFilter= 95,
            Sampler_State_MagFilter= 96,
            Sampler_State_MipFilter= 97,
            Sampler_State_Filter= 98,
            Sampler_State_AddressU= 99,
            Sampler_State_AddressV= 100,
            Sampler_State_AddressW= 101,
            Sampler_State_BorderColor= 102,
            Sampler_State_MaxMipLevel= 103,
            Sampler_State_MaxAnisotropy= 104,
            Sampler_State_MipLodBias= 105,
            Sampler_State_Expression= 106,
            Sampler_Register_Expression= 107,
            Sampler_Declaration_States= 108,
            Sampler_Declaration= 109,
            Semantic_Variable= 110,
            Semantic= 111,
            GlslParam= 112,
            Function_Header= 113,

            //Terminal tokens:
            BlockComment= 114,
            Comment = 115,
            Whitespace= 116,
            LinePragma= 117,
            Pass    = 118,
            Technique= 119,
            Sampler = 120,
            SamplerState= 121,
            VertexShader= 122,
            PixelShader= 123,
            Register= 124,
            Boolean = 125,
            Number  = 126,
            HexColor= 127,
            Identifier= 128,
            OpenBracket= 129,
            CloseBracket= 130,
            Equals  = 131,
            Colon   = 132,
            Comma   = 133,
            Semicolon= 134,
            Or      = 135,
            OpenParenthesis= 136,
            CloseParenthesis= 137,
            OpenSquareBracket= 138,
            CloseSquareBracket= 139,
            LessThan= 140,
            GreaterThan= 141,
            Compile = 142,
            ShaderModel= 143,
            GlslIn  = 144,
            Void    = 145,
            Code    = 146,
            EndOfFile= 147,
            MinFilter= 148,
            MagFilter= 149,
            MipFilter= 150,
            Filter  = 151,
            Texture = 152,
            AddressU= 153,
            AddressV= 154,
            AddressW= 155,
            BorderColor= 156,
            MaxAnisotropy= 157,
            MaxMipLevel= 158,
            MipLodBias= 159,
            Clamp   = 160,
            Wrap    = 161,
            Mirror  = 162,
            Border  = 163,
            None    = 164,
            Linear  = 165,
            Point   = 166,
            Anisotropic= 167,
            AlphaBlendEnable= 168,
            SrcBlend= 169,
            DestBlend= 170,
            BlendOp = 171,
            ColorWriteEnable= 172,
            ZEnable = 173,
            ZWriteEnable= 174,
            ZFunc   = 175,
            DepthBias= 176,
            CullMode= 177,
            FillMode= 178,
            MultiSampleAntiAlias= 179,
            ScissorTestEnable= 180,
            SlopeScaleDepthBias= 181,
            StencilEnable= 182,
            StencilFail= 183,
            StencilFunc= 184,
            StencilMask= 185,
            StencilPass= 186,
            StencilRef= 187,
            StencilWriteMask= 188,
            StencilZFail= 189,
            Never   = 190,
            Less    = 191,
            Equal   = 192,
            LessEqual= 193,
            Greater = 194,
            NotEqual= 195,
            GreaterEqual= 196,
            Always  = 197,
            Keep    = 198,
            Zero    = 199,
            Replace = 200,
            IncrSat = 201,
            DecrSat = 202,
            Invert  = 203,
            Incr    = 204,
            Decr    = 205,
            Red     = 206,
            Green   = 207,
            Blue    = 208,
            Alpha   = 209,
            All     = 210,
            Cw      = 211,
            Ccw     = 212,
            Solid   = 213,
            WireFrame= 214,
            Add     = 215,
            Subtract= 216,
            RevSubtract= 217,
            Min     = 218,
            Max     = 219,
            One     = 220,
            SrcColor= 221,
            InvSrcColor= 222,
            SrcAlpha= 223,
            InvSrcAlpha= 224,
            DestAlpha= 225,
            InvDestAlpha= 226,
            DestColor= 227,
            InvDestColor= 228,
            SrcAlphaSat= 229,
            BlendFactor= 230,
            InvBlendFactor= 231
    }

    public class Token
    {
        private string file;
        private int line;
        private int column;
        private int startpos;
        private int endpos;
        private string text;
        private object value;

        // contains all prior skipped symbols
        private List<Token> skipped;

        public string File { 
            get { return file; } 
            set { file = value; }
        }

        public int Line { 
            get { return line; } 
            set { line = value; }
        }

        public int Column {
            get { return column; } 
            set { column = value; }
        }

        public int StartPos { 
            get { return startpos;} 
            set { startpos = value; }
        }

        public int Length { 
            get { return endpos - startpos;} 
        }

        public int EndPos { 
            get { return endpos;} 
            set { endpos = value; }
        }

        public string Text { 
            get { return text;} 
            set { text = value; }
        }

        public List<Token> Skipped { 
            get { return skipped;} 
            set { skipped = value; }
        }
        public object Value { 
            get { return value;} 
            set { this.value = value; }
        }

        [XmlAttribute]
        public TokenType Type;

        public Token()
            : this(0, 0)
        {
        }

        public Token(int start, int end)
        {
            Type = TokenType._UNDETERMINED_;
            startpos = start;
            endpos = end;
            Text = ""; // must initialize with empty string, may cause null reference exceptions otherwise
            Value = null;
        }

        public void UpdateRange(Token token)
        {
            if (token.StartPos < startpos) startpos = token.StartPos;
            if (token.EndPos > endpos) endpos = token.EndPos;
        }

        public override string ToString()
        {
            if (Text != null)
                return Type.ToString() + " '" + Text + "'";
            else
                return Type.ToString();
        }
    }

    #endregion
}
