// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TwoMGFX.EffectParsing
{
    internal partial class EffectParser
    {
        #region Classification

        private void AddTechniqueClasses()
        {
            _parser.AddClass(StatementClass.Technique, s => s.FirstWordIs("technique"));
            _parser.AddClass(StatementClass.Pass, s => s.FirstWordIs("pass"), StatementClass.Technique);
            _parser.AddClass(StatementClass.Illegal, s => true, StatementClass.Technique);

            _parser.AddClass(StatementClass.VertexShader, s => s.FirstWordIs("VertexShader"), StatementClass.Pass);
            _parser.AddClass(StatementClass.PixelShader, s => s.FirstWordIs("PixelShader"), StatementClass.Pass);

            // render state
            _parser.AddClass(StatementClass.AlphaBlendEnable, s => s.BeforeEqualsIs("AlphaBlendEnable"), StatementClass.Pass);
            _parser.AddClass(StatementClass.SrcBlend, s => s.BeforeEqualsIs("SrcBlend"), StatementClass.Pass);
            _parser.AddClass(StatementClass.DestBlend, s => s.BeforeEqualsIs("DestBlend"), StatementClass.Pass);
            _parser.AddClass(StatementClass.BlendOp, s => s.BeforeEqualsIs("BlendOp"), StatementClass.Pass);
            _parser.AddClass(StatementClass.ColorWriteEnable, s => s.BeforeEqualsIs("ColorWriteEnable"), StatementClass.Pass);
            _parser.AddClass(StatementClass.ZEnable, s => s.BeforeEqualsIs("ZEnable"), StatementClass.Pass);
            _parser.AddClass(StatementClass.ZWriteEnable, s => s.BeforeEqualsIs("ZWriteEnable"), StatementClass.Pass);
            _parser.AddClass(StatementClass.ZFunc, s => s.BeforeEqualsIs("ZFunc"), StatementClass.Pass);
            _parser.AddClass(StatementClass.DepthBias, s => s.BeforeEqualsIs("DepthBias"), StatementClass.Pass);
            _parser.AddClass(StatementClass.CullMode, s => s.BeforeEqualsIs("CullMode"), StatementClass.Pass);
            _parser.AddClass(StatementClass.FillMode, s => s.BeforeEqualsIs("FillMode"), StatementClass.Pass);
            _parser.AddClass(StatementClass.MultiSampleAntiAlias, s => s.BeforeEqualsIs("MultiSampleAntiAlias"), StatementClass.Pass);
            _parser.AddClass(StatementClass.ScissorTestEnable, s => s.BeforeEqualsIs("ScissorTestEnable"), StatementClass.Pass);
            _parser.AddClass(StatementClass.SlopeScaleDepthBias, s => s.BeforeEqualsIs("SlopeScaleDepthBias"), StatementClass.Pass);
            _parser.AddClass(StatementClass.StencilEnable, s => s.BeforeEqualsIs("StencilEnable"), StatementClass.Pass);
            _parser.AddClass(StatementClass.StencilFail, s => s.BeforeEqualsIs("StencilFail"), StatementClass.Pass);
            _parser.AddClass(StatementClass.StencilFunc, s => s.BeforeEqualsIs("StencilFunc"), StatementClass.Pass);
            _parser.AddClass(StatementClass.StencilMask, s => s.BeforeEqualsIs("StencilMask"), StatementClass.Pass);
            _parser.AddClass(StatementClass.StencilPass, s => s.BeforeEqualsIs("StencilPass"), StatementClass.Pass);
            _parser.AddClass(StatementClass.StencilRef, s => s.BeforeEqualsIs("StencilRef"), StatementClass.Pass);
            _parser.AddClass(StatementClass.StencilWriteMask, s => s.BeforeEqualsIs("StencilWriteMask"), StatementClass.Pass);
            _parser.AddClass(StatementClass.StencilZFail, s => s.BeforeEqualsIs("StencilZFail"), StatementClass.Pass);
            // any other statement is considered illegal
            _parser.AddClass(StatementClass.Illegal, s => true, StatementClass.Pass);
        }

        private void AddSamplerClasses()
        {
            _parser.AddClass(StatementClass.Sampler, s =>
                (s.FirstWordIs("sampler1D") ||
                 s.FirstWordIs("sampler2D") ||
                 s.FirstWordIs("sampler3D") ||
                 s.FirstWordIs("samplerCUBE") ||
                 s.FirstWordIs("SamplerState") ||
                 s.FirstWordIs("sampler")) &&
                s.AfterEqualsIs("sampler_state"));

            // sampler state
            _parser.AddClass(StatementClass.MinFilter, s => s.BeforeEqualsIs("MinFilter"), StatementClass.Sampler);
            _parser.AddClass(StatementClass.MagFilter, s => s.BeforeEqualsIs("MagFilter"), StatementClass.Sampler);
            _parser.AddClass(StatementClass.MipFilter, s => s.BeforeEqualsIs("MipFilter"), StatementClass.Sampler);
            _parser.AddClass(StatementClass.Filter, s => s.BeforeEqualsIs("Filter"), StatementClass.Sampler);
            _parser.AddClass(StatementClass.Texture, s => s.BeforeEqualsIs("Texture"), StatementClass.Sampler);
            _parser.AddClass(StatementClass.AddressU, s => s.BeforeEqualsIs("AddressU"), StatementClass.Sampler);
            _parser.AddClass(StatementClass.AddressV, s => s.BeforeEqualsIs("AddressV"), StatementClass.Sampler);
            _parser.AddClass(StatementClass.AddressW, s => s.BeforeEqualsIs("AddressW"), StatementClass.Sampler);
            _parser.AddClass(StatementClass.BorderColor, s => s.BeforeEqualsIs("BorderColor"), StatementClass.Sampler);
            _parser.AddClass(StatementClass.MaxAnisotropy, s => s.BeforeEqualsIs("MaxAnisotropy"), StatementClass.Sampler);
            _parser.AddClass(StatementClass.MaxMipLevel, s => s.BeforeEqualsIs("MaxMipLevel") || s.BeforeEqualsIs("MaxLod"), StatementClass.Sampler);
            _parser.AddClass(StatementClass.MipLodBias, s => s.BeforeEqualsIs("MipLodBias"), StatementClass.Sampler);
            // any other statement is considered illegal
            _parser.AddClass(StatementClass.Illegal, s => true, StatementClass.Sampler);
        }

        #endregion

        #region Processing

        #region Technique

        private TechniqueInfo ProcessTechnique(BlockStatement ts)
        {
            var tinfo = new TechniqueInfo();
            var parts = ts.HeaderText.Split();
            if (!Check(parts.Length > 2, ts, "Syntax Error: Expected 'technique [<techniqueName>]'"))
                return null;
            if (parts.Length == 2)
            {
                if (!Check(parts[1].IsValidIdentifier(), ts, $"Invalid technique name {parts[1]}"))
                    return null;
                tinfo.name = parts[1];
            }
            else
                tinfo.name = string.Empty;


            foreach (var s in ts)
            {
                switch (s.Class)
                {
                    case StatementClass.Pass:
                        BlockStatement bs;
                        if (SafeToBlockStatement(s, out bs))
                        {
                            var pinfo = ProcessPass(bs);
                            tinfo.Passes.Add(pinfo);
                        }
                        break;
                    case StatementClass.Illegal:
                        AddError(s, "Syntax Error: Expected pass declaration");
                        break;
                }
            }
            ts.ToWhitespace();
            return tinfo;
        }

        private PassInfo ProcessPass(BlockStatement ps)
        {
            var pinfo = new PassInfo();

            var parts = ps.HeaderText.Split();
            if (!Check(parts.Length > 2, ps, "Syntax Error: Expected 'pass [<passName>]'"))
                return null;
            if (parts.Length == 2)
            {
                if (!Check(parts[1].IsValidIdentifier(), ps, $"Invalid pass name {parts[1]}"))
                    return null;
                pinfo.name = parts[1];
            }
            else
                pinfo.name = string.Empty;

            foreach (var s in ps)
            {
                var success = true;
                switch (s.Class)
                {
                    case StatementClass.VertexShader:
                        if (!TryParseShaderAssignment(s, out pinfo.vsFunction, out pinfo.vsModel))
                            AddError(s, "Syntax Error: Expected 'compile <VsName> <VsModel>'");
                        break;
                    case StatementClass.PixelShader:
                        if (!TryParseShaderAssignment(s, out pinfo.psFunction, out pinfo.psModel))
                            AddError(s, "Syntax Error: Expected 'compile <PsName> <PsModel>'");
                        break;

                    // Render State
                    case StatementClass.AlphaBlendEnable:
                        bool alphaBlendEnable;
                        if (success = TryParseBoolAssignment(s, out alphaBlendEnable))
                            pinfo.AlphaBlendEnable = alphaBlendEnable;
                        break;
                    case StatementClass.SrcBlend:
                        Blend srcBlend;
                        if (success = TryParseBlendAssignment(s, out srcBlend))
                            pinfo.SrcBlend = srcBlend;
                        break;
                    case StatementClass.DestBlend:
                        Blend destBlend;
                        if (success = TryParseBlendAssignment(s, out destBlend))
                            pinfo.DestBlend = destBlend;
                        break;
                    case StatementClass.BlendOp:
                        BlendFunction blendOp;
                        if (success = TryParseBlendFuncAssignment(s, out blendOp))
                            pinfo.BlendOp = blendOp;
                        break;
                    case StatementClass.ColorWriteEnable:
                        ColorWriteChannels colorWriteEnable;
                        if (success = TryParseEnumAssignment(s, out colorWriteEnable))
                            pinfo.ColorWriteEnable = colorWriteEnable;
                        break;
                    case StatementClass.ZEnable:
                        bool zEnable;
                        if (success = TryParseBoolAssignment(s, out zEnable))
                            pinfo.ZEnable = zEnable;
                        break;
                    case StatementClass.ZWriteEnable:
                        bool zWriteEnable;
                        if (success = TryParseBoolAssignment(s, out zWriteEnable))
                            pinfo.ZWriteEnable = zWriteEnable;
                        break;
                    case StatementClass.ZFunc:
                        CompareFunction zFunc;
                        if (success = TryParseEnumAssignment(s, out zFunc))
                            pinfo.DepthBufferFunction = zFunc;
                        break;
                    case StatementClass.DepthBias:
                        float depthBias;
                        if (success = TryParseFloatAssignment(s, out depthBias))
                            pinfo.DepthBias = depthBias;
                        break;
                    case StatementClass.CullMode:
                        CullMode cullMode;
                        if (success = TryParseCullModeAssignment(s, out cullMode))
                            pinfo.CullMode = cullMode;
                        break;
                    case StatementClass.FillMode:
                        FillMode fillMode;
                        if (success = TryParseEnumAssignment(s, out fillMode))
                            pinfo.FillMode = fillMode;
                        break;
                    case StatementClass.MultiSampleAntiAlias:
                        bool multiSampleAntiAlias;
                        if (success = TryParseBoolAssignment(s, out multiSampleAntiAlias))
                            pinfo.MultiSampleAntiAlias = multiSampleAntiAlias;
                        break;
                    case StatementClass.ScissorTestEnable:
                        bool scissorTestEnable;
                        if (success = TryParseBoolAssignment(s, out scissorTestEnable))
                            pinfo.ScissorTestEnable = scissorTestEnable;
                        break;
                    case StatementClass.SlopeScaleDepthBias:
                        float slopeScaleDepthBias;
                        if (success = TryParseFloatAssignment(s, out slopeScaleDepthBias))
                            pinfo.SlopeScaleDepthBias = slopeScaleDepthBias;
                        break;
                    case StatementClass.StencilEnable:
                        bool stencilEnable;
                        if (success = TryParseBoolAssignment(s, out stencilEnable))
                            pinfo.StencilEnable = stencilEnable;
                        break;
                    case StatementClass.StencilFail:
                        StencilOperation stencilFail;
                        if (success = TryParseStencilOpAssignment(s, out stencilFail))
                            pinfo.StencilFail = stencilFail;
                        break;
                    case StatementClass.StencilFunc:
                        CompareFunction stencilFunc;
                        if (success = TryParseEnumAssignment(s, out stencilFunc))
                            pinfo.StencilFunc = stencilFunc;
                        break;
                    case StatementClass.StencilMask:
                        int stencilMask;
                        if (success = TryParseIntAssignment(s, out stencilMask))
                            pinfo.StencilMask = stencilMask;
                        break;
                    case StatementClass.StencilPass:
                        StencilOperation stencilPass;
                        if (success = TryParseStencilOpAssignment(s, out stencilPass))
                            pinfo.StencilPass = stencilPass;
                        break;
                    case StatementClass.StencilRef:
                        int stencilRef;
                        if (success = TryParseIntAssignment(s, out stencilRef))
                            pinfo.StencilRef = stencilRef;
                        break;
                    case StatementClass.StencilWriteMask:
                        int stencilWriteMask;
                        if (success = TryParseIntAssignment(s, out stencilWriteMask))
                            pinfo.StencilWriteMask = stencilWriteMask;
                        break;
                    case StatementClass.StencilZFail:
                        StencilOperation stencilZFail;
                        if (success = TryParseStencilOpAssignment(s, out stencilZFail))
                            pinfo.StencilZFail = stencilZFail;
                        break;
                    case StatementClass.Illegal:
                        break;
                }

                if (!success)
                    AddError(s, "Failed to parse render state assignment");
            }
            return pinfo;
        }

        #endregion

        #region Sampler

        private SamplerStateInfo ProcessSampler(BlockStatement ss)
        {
            var sinfo = new SamplerStateInfo();

            foreach (var s in ss)
            {
                var success = true;
                switch (s.Class)
                {
                    case StatementClass.Texture:
                        string textureName;
                        if (success = TryParseTextureAssignment(s, out textureName))
                            sinfo.TextureName = textureName;
                        break;
                    case StatementClass.MinFilter:
                        TextureFilterType minFilter;
                        if (success = TryParseEnumAssignment(s, out minFilter))
                            sinfo.MinFilter = minFilter;
                        break;
                    case StatementClass.MagFilter:
                        TextureFilterType magFilter;
                        if (success = TryParseEnumAssignment(s, out magFilter))
                            sinfo.MagFilter = magFilter;
                        break;
                    case StatementClass.MipFilter:
                        TextureFilterType mipFilter;
                        if (success = TryParseEnumAssignment(s, out mipFilter))
                            sinfo.MipFilter = mipFilter;
                        break;
                    case StatementClass.Filter:
                        TextureFilterType filter;
                        if (success = TryParseEnumAssignment(s, out filter))
                            sinfo.Filter = filter;
                        break;
                    case StatementClass.AddressU:
                        TextureAddressMode addressU;
                        if (success = TryParseEnumAssignment(s, out addressU))
                            sinfo.AddressU = addressU;
                        break;
                    case StatementClass.AddressV:
                        TextureAddressMode addressV;
                        if (success = TryParseEnumAssignment(s, out addressV))
                            sinfo.AddressV = addressV;
                        break;
                    case StatementClass.AddressW:
                        TextureAddressMode addressW;
                        if (success = TryParseEnumAssignment(s, out addressW))
                            sinfo.AddressW = addressW;
                        break;
                    case StatementClass.BorderColor:
                        Color borderColor;
                        if (success = TryParseColorAssignment(s, out borderColor))
                            sinfo.BorderColor = borderColor;
                        break;
                    case StatementClass.MaxAnisotropy:
                        int maxAnisotropy;
                        if (success = TryParseIntAssignment(s, out maxAnisotropy))
                            sinfo.MaxAnisotropy = maxAnisotropy;
                        break;
                    case StatementClass.MaxMipLevel:
                        int maxMipLevel;
                        if (success = TryParseIntAssignment(s, out maxMipLevel))
                            sinfo.MaxMipLevel = maxMipLevel;
                        break;
                    case StatementClass.MipLodBias:
                        int mipLodBias;
                        success = TryParseIntAssignment(s, out mipLodBias);
                        sinfo.MipMapLevelOfDetailBias = mipLodBias;
                        break;
                    case StatementClass.Illegal:
                        AddError(s, $"Unknown sampler state variable {s.Text.GetFirstIdentifier()}");
                        break;
                }

                if (!success)
                    AddError(s, "Failed to parse sampler state assignment");
            }

            return sinfo;
        }

        #endregion

        #endregion // Processing
    }
}