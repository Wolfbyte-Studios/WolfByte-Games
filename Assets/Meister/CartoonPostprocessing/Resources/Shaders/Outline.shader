Shader "Hidden/Shader/Outline"
{
    Properties
    {
        // This property is necessary to make the CommandBuffer.Blit bind the source texture to _MainTex
        _MainTex("Main Texture", 2DArray) = "grey" {}
    }

    HLSLINCLUDE

    #pragma target 4.5
    #pragma only_renderers d3d11 playstation xboxone xboxseries vulkan metal switch

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/FXAA.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/RTUpscale.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/NormalBuffer.hlsl"
    
    struct Attributes
    {
        uint vertexID : SV_VertexID;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float2 texcoord   : TEXCOORD0;
        UNITY_VERTEX_OUTPUT_STEREO
    };

    Varyings Vert(Attributes input)
    {
        Varyings output;
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
        output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
        output.texcoord = GetFullScreenTriangleTexCoord(input.vertexID);
        return output;
    }

    // List of properties to control your post process effect
    float _ColorIntensity = 0;
    float _NormalIntensity = 0;
    float _DepthIntensity = 0;
    int _Delta = 0;
    float4 _Color = float4(0.0f, 0.0f, 0.0f, 0.0f);
    TEXTURE2D_X(_MainTex);
    TEXTURE2D_X(_GBufferTexture1);

    float3 LoadCameraNormal(uint2 positionSS)
    {
        if(LoadCameraDepth(positionSS).x <= 0.0f)
        {
            return 0.0f;
        }
        NormalData normalData;
        DecodeFromNormalBuffer(positionSS, normalData);
        return normalData.normalWS;
    }

    bool normalRobertsCross(uint2 texcoord)
    {
        if(_NormalIntensity == 0) return false;
        float3 normalHorizontal = LoadCameraNormal(texcoord + uint2(-1, -1)*_Delta);
        normalHorizontal -= LoadCameraNormal(texcoord + uint2(1, 1)*_Delta);

        float3 normalVertical = LoadCameraNormal(texcoord + uint2(-1, 1)*_Delta);
        normalVertical -= LoadCameraNormal(texcoord + uint2(1, -1)*_Delta);

        float normalOutline = dot(normalHorizontal, normalHorizontal) + dot(normalVertical, normalVertical); 
        bool ret = (normalOutline > (1/_NormalIntensity)) ? false : true;
        return !ret;
    }

    bool colorRobertsCross(uint2 texcoord)
    {
        if(_ColorIntensity == 0) return false;
        float3 colorHorizontal = LoadCameraColor(texcoord + uint2(-1, -1)*_Delta);
        colorHorizontal -= LoadCameraColor(texcoord + uint2(1, 1)*_Delta);

        float3 colorVertical = LoadCameraColor(texcoord + uint2(-1, 1)*_Delta);
        colorVertical -= LoadCameraColor(texcoord + uint2(1, -1)*_Delta);

        float colorOutline = sqrt(dot(colorHorizontal, colorHorizontal) + dot(colorVertical, colorVertical)); 
        bool ret = (colorOutline > (1/_ColorIntensity)) ? false : true;
        return !ret;
    }

    bool depthRobertsCross(uint2 texcoord)
    {
        if(_DepthIntensity == 0) return false;
        float depthHorizontal = LoadCameraDepth(texcoord + uint2(-1, -1)*_Delta).x;
        depthHorizontal -= LoadCameraDepth(texcoord + uint2(1, 1)*_Delta).x;

        float depthVertical = LoadCameraDepth(texcoord + uint2(-1, 1)*_Delta).x;
        depthVertical -= LoadCameraDepth(texcoord + uint2(1, -1)*_Delta).x;

        float depthOutline = sqrt(dot(depthHorizontal, depthHorizontal) + dot(depthVertical, depthVertical)); 
        float depthBias = (1/_DepthIntensity) * LoadCameraDepth(texcoord).x;
        bool ret = (depthOutline > depthBias) ? false : true;
        return !ret;
    }


    float4 CustomPostProcess(Varyings input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

        bool outline = depthRobertsCross(input.positionCS) || normalRobertsCross(input.positionCS) || colorRobertsCross(input.positionCS); 
        float3 color;
        if(!outline)
            color = SAMPLE_TEXTURE2D_X(_MainTex, s_linear_clamp_sampler, input.texcoord).xyz;
        else color =  _Color;

        return float4(color, 1);
    }

    ENDHLSL

    SubShader
    {
        Tags{ "RenderPipeline" = "HDRenderPipeline" }
        Pass
        {
            Name "Outline"

            ZWrite Off
            ZTest Always
            Blend Off
            Cull Off

            HLSLPROGRAM
                #pragma fragment CustomPostProcess
                #pragma vertex Vert
            ENDHLSL
        }
    }
    Fallback Off
}
