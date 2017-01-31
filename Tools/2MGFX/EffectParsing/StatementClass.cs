// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace TwoMGFX.EffectParsing
{
    public enum StatementClass
    {
        Skipped                            = 0,
        TopLevel                           = 1,
        Illegal                            = 2,

        #region Technique

        Technique                          = 10,
        Pass                               = 11,
        VertexShader                       = 12,
        PixelShader                        = 13,

        #endregion

        #region Sampler State

        Sampler                            = 20,
        MinFilter                          = 21,
        MagFilter                          = 22,
        MipFilter                          = 23,
        Filter                             = 24,
        Texture                            = 25,
        AddressU                           = 26,
        AddressV                           = 27,
        AddressW                           = 28,
        BorderColor                        = 29,
        MaxAnisotropy                      = 30,
        MaxMipLevel                        = 31,
        MipLodBias                         = 32,

        #endregion

        #region Render State

        AlphaBlendEnable                   = 50,
        SrcBlend                           = 51,
        DestBlend                          = 52,
        BlendOp                            = 53,
        ColorWriteEnable                   = 54,
        ZEnable                            = 55,
        ZWriteEnable                       = 56,
        ZFunc                              = 57,
        DepthBias                          = 58,
        CullMode                           = 59,
        FillMode                           = 60,
        MultiSampleAntiAlias               = 61,
        ScissorTestEnable                  = 62,
        SlopeScaleDepthBias                = 63,
        StencilEnable                      = 64,
        StencilFail                        = 65,
        StencilFunc                        = 66,
        StencilMask                        = 67,
        StencilPass                        = 68,
        StencilRef                         = 69,
        StencilWriteMask                   = 70,
        StencilZFail                       = 71,

        #endregion

        #region HLSL Only

        Tbuffer,
        Cbuffer,

        #endregion

        #region GLFX Only

        Function                           = 200,
        InputChannel                       = 201,

        #endregion
    }
}