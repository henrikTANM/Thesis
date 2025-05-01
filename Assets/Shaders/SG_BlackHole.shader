Shader "SG_BlackHole"
{
    Properties
    {
        _SpherePercentage("SpherePercentage", Range(0, 1)) = 0.25
        _DistortionExponent("DistortionExponent", Range(1, 16)) = 1
        _OuterGlowColor("OuterGlowColor", Color) = (1, 1, 1, 0)
        _OuterGlowExponent("OuterGlowExponent", Float) = 4
        _OuterGlowMultiplier("OuterGlowMultiplier", Float) = 1
        _ScreenSpaceScale("ScreenSpaceScale", Float) = 1
        [HideInInspector]_QueueOffset("_QueueOffset", Float) = 0
        [HideInInspector]_QueueControl("_QueueControl", Float) = -1
        [HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "UniversalMaterialType" = "Unlit"
            "Queue"="Transparent"
            "DisableBatching"="False"
            "ShaderGraphShader"="true"
            "ShaderGraphTargetId"="UniversalUnlitSubTarget"
        }
        Pass
        {
            Name "Universal Forward"
            Tags
            {
                "LightMode"="UseColorTexture"
            }
        
        // Render State
        Cull Back
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite Off
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 2.0
        #pragma multi_compile_instancing
        #pragma multi_compile_fog
        #pragma instancing_options renderinglayer
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        #pragma multi_compile _ LIGHTMAP_ON
        #pragma multi_compile _ DIRLIGHTMAP_COMBINED
        #pragma shader_feature _ _SAMPLE_GI
        #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
        #pragma multi_compile_fragment _ DEBUG_DISPLAY
        #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
        // GraphKeywords: <None>
        
        // Defines
        
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_NORMAL_WS
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_UNLIT
        #define _FOG_FRAGMENT 1
        #define _SURFACE_TYPE_TRANSPARENT 1
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float3 normalWS;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 WorldSpaceNormal;
             float3 WorldSpaceViewDirection;
             float3 WorldSpacePosition;
             float2 NDCPosition;
             float2 PixelPosition;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS : INTERP0;
             float3 normalWS : INTERP1;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.positionWS.xyz = input.positionWS;
            output.normalWS.xyz = input.normalWS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.positionWS = input.positionWS.xyz;
            output.normalWS = input.normalWS.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float _SpherePercentage;
        float _DistortionExponent;
        float4 _OuterGlowColor;
        float _OuterGlowExponent;
        float _OuterGlowMultiplier;
        float _ScreenSpaceScale;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_GrabbedTexture);
        SAMPLER(sampler_GrabbedTexture);
        float4 _GrabbedTexture_TexelSize;
        
        // Graph Includes
        #include "Assets/Shaders/SF_Raycast.hlsl"
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_FresnelEffect_float(float3 Normal, float3 ViewDir, float Power, out float Out)
        {
            Out = pow((1.0 - saturate(dot(normalize(Normal), normalize(ViewDir)))), Power);
        }
        
        void Unity_InvertColors_float(float In, float InvertColors, out float Out)
        {
            Out = abs(InvertColors - In);
        }
        
        void Unity_Subtract_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A - B;
        }
        
        void Unity_Normalize_float3(float3 In, out float3 Out)
        {
            Out = normalize(In);
        }
        
        void Unity_DotProduct_float3(float3 A, float3 B, out float Out)
        {
            Out = dot(A, B);
        }
        
        void Unity_Saturate_float(float In, out float Out)
        {
            Out = saturate(In);
        }
        
        void Unity_Arccosine_float(float In, out float Out)
        {
            Out = acos(In);
        }
        
        void Unity_Divide_float(float A, float B, out float Out)
        {
            Out = A / B;
        }
        
        void Unity_OneMinus_float(float In, out float Out)
        {
            Out = 1 - In;
        }
        
        void Unity_Power_float(float A, float B, out float Out)
        {
            Out = pow(A, B);
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_DegreesToRadians_float(float In, out float Out)
        {
            Out = radians(In);
        }
        
        void Unity_Tangent_float(float In, out float Out)
        {
            Out = tan(In);
        }
        
        void Unity_Distance_float3(float3 A, float3 B, out float Out)
        {
            Out = distance(A, B);
        }
        
        void Unity_Divide_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A / B;
        }
        
        void Unity_Multiply_float3_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A * B;
        }
        
        void Unity_Subtract_float2(float2 A, float2 B, out float2 Out)
        {
            Out = A - B;
        }
        
        void Unity_Normalize_float2(float2 In, out float2 Out)
        {
            Out = normalize(In);
        }
        
        void Unity_Multiply_float2_float2(float2 A, float2 B, out float2 Out)
        {
            Out = A * B;
        }
        
        void Unity_Add_float2(float2 A, float2 B, out float2 Out)
        {
            Out = A + B;
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        void Unity_Add_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A + B;
        }
        
        void Unity_Lerp_float4(float4 A, float4 B, float4 T, out float4 Out)
        {
            Out = lerp(A, B, T);
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float3 BaseColor;
            float Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            UnityTexture2D _Property_69d77899d4a84dff9e0ba0c4a044b9d8_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_GrabbedTexture);
            float _FresnelEffect_0f0943dc3a564f91956808e178c3998e_Out_3_Float;
            Unity_FresnelEffect_float(IN.WorldSpaceNormal, IN.WorldSpaceViewDirection, 0.2, _FresnelEffect_0f0943dc3a564f91956808e178c3998e_Out_3_Float);
            float _InvertColors_f7e477496c31462c87eb912600ed00cd_Out_1_Float;
            float _InvertColors_f7e477496c31462c87eb912600ed00cd_InvertColors = float (1);
            Unity_InvertColors_float(_FresnelEffect_0f0943dc3a564f91956808e178c3998e_Out_3_Float, _InvertColors_f7e477496c31462c87eb912600ed00cd_InvertColors, _InvertColors_f7e477496c31462c87eb912600ed00cd_Out_1_Float);
            float3 _Subtract_fad7af201293423d8122499f95b1b7b9_Out_2_Vector3;
            Unity_Subtract_float3(_WorldSpaceCameraPos, SHADERGRAPH_OBJECT_POSITION, _Subtract_fad7af201293423d8122499f95b1b7b9_Out_2_Vector3);
            float3 _Normalize_5abd8b5118ce435d9139716e6d139c3b_Out_1_Vector3;
            Unity_Normalize_float3(_Subtract_fad7af201293423d8122499f95b1b7b9_Out_2_Vector3, _Normalize_5abd8b5118ce435d9139716e6d139c3b_Out_1_Vector3);
            float _DotProduct_0cde307a59a14b2a93ca6522bd6ef385_Out_2_Float;
            Unity_DotProduct_float3(IN.WorldSpaceNormal, _Normalize_5abd8b5118ce435d9139716e6d139c3b_Out_1_Vector3, _DotProduct_0cde307a59a14b2a93ca6522bd6ef385_Out_2_Float);
            float _Saturate_eec3574569164c51be3c825c4bba17ee_Out_1_Float;
            Unity_Saturate_float(_DotProduct_0cde307a59a14b2a93ca6522bd6ef385_Out_2_Float, _Saturate_eec3574569164c51be3c825c4bba17ee_Out_1_Float);
            float _Arccosine_986c5d2f1a964026840b69446e689f86_Out_1_Float;
            Unity_Arccosine_float(_Saturate_eec3574569164c51be3c825c4bba17ee_Out_1_Float, _Arccosine_986c5d2f1a964026840b69446e689f86_Out_1_Float);
            float _Divide_8bb22013ba8044899d4e09c3738480f7_Out_2_Float;
            Unity_Divide_float(_Arccosine_986c5d2f1a964026840b69446e689f86_Out_1_Float, 1.5, _Divide_8bb22013ba8044899d4e09c3738480f7_Out_2_Float);
            float _OneMinus_10ed0c7c31934669a324bae3231cfe06_Out_1_Float;
            Unity_OneMinus_float(_Divide_8bb22013ba8044899d4e09c3738480f7_Out_2_Float, _OneMinus_10ed0c7c31934669a324bae3231cfe06_Out_1_Float);
            float _Property_a426d4ed7bcf4846afd70efd5477f242_Out_0_Float = _SpherePercentage;
            float _OneMinus_71ce242022d44156828b253d2d059462_Out_1_Float;
            Unity_OneMinus_float(_Property_a426d4ed7bcf4846afd70efd5477f242_Out_0_Float, _OneMinus_71ce242022d44156828b253d2d059462_Out_1_Float);
            float _Divide_b4db269f1d354e75a0e6467b97884ad9_Out_2_Float;
            Unity_Divide_float(_OneMinus_10ed0c7c31934669a324bae3231cfe06_Out_1_Float, _OneMinus_71ce242022d44156828b253d2d059462_Out_1_Float, _Divide_b4db269f1d354e75a0e6467b97884ad9_Out_2_Float);
            float _Property_0c314eb8683a42c293aacfbaf35c2da4_Out_0_Float = _DistortionExponent;
            float _Power_9477134c44f94f89b0cb619fbf210836_Out_2_Float;
            Unity_Power_float(_Divide_b4db269f1d354e75a0e6467b97884ad9_Out_2_Float, _Property_0c314eb8683a42c293aacfbaf35c2da4_Out_0_Float, _Power_9477134c44f94f89b0cb619fbf210836_Out_2_Float);
            float _Float_95c4524b111041dd89db1ed6d4ca4746_Out_0_Float = 60;
            float _Multiply_4c3dca3470804b8dbcd2b0762d3dee92_Out_2_Float;
            Unity_Multiply_float_float(_Float_95c4524b111041dd89db1ed6d4ca4746_Out_0_Float, 0.5, _Multiply_4c3dca3470804b8dbcd2b0762d3dee92_Out_2_Float);
            float _DegreesToRadians_dfd87932145c4301990ee69e3ec61f18_Out_1_Float;
            Unity_DegreesToRadians_float(_Multiply_4c3dca3470804b8dbcd2b0762d3dee92_Out_2_Float, _DegreesToRadians_dfd87932145c4301990ee69e3ec61f18_Out_1_Float);
            float _Tangent_21484ba704ed4646854adb637505b365_Out_1_Float;
            Unity_Tangent_float(_DegreesToRadians_dfd87932145c4301990ee69e3ec61f18_Out_1_Float, _Tangent_21484ba704ed4646854adb637505b365_Out_1_Float);
            float _Float_113cd539138a4222bfce6d620dcf208f_Out_0_Float = 2;
            float _Distance_c996b1aa1861486c81054136b341f9a8_Out_2_Float;
            Unity_Distance_float3(SHADERGRAPH_OBJECT_POSITION, _WorldSpaceCameraPos, _Distance_c996b1aa1861486c81054136b341f9a8_Out_2_Float);
            float _Multiply_28186d21cf78489e84ac995c8b61a63b_Out_2_Float;
            Unity_Multiply_float_float(_Float_113cd539138a4222bfce6d620dcf208f_Out_0_Float, _Distance_c996b1aa1861486c81054136b341f9a8_Out_2_Float, _Multiply_28186d21cf78489e84ac995c8b61a63b_Out_2_Float);
            float _Multiply_f2054904daa44fe0a8b914f24c5a2d7a_Out_2_Float;
            Unity_Multiply_float_float(_Tangent_21484ba704ed4646854adb637505b365_Out_1_Float, _Multiply_28186d21cf78489e84ac995c8b61a63b_Out_2_Float, _Multiply_f2054904daa44fe0a8b914f24c5a2d7a_Out_2_Float);
            float3 _Divide_c0917e1dffc2444ba1d06fd4058878a8_Out_2_Vector3;
            Unity_Divide_float3(float3(length(float3(UNITY_MATRIX_M[0].x, UNITY_MATRIX_M[1].x, UNITY_MATRIX_M[2].x)),
                                     length(float3(UNITY_MATRIX_M[0].y, UNITY_MATRIX_M[1].y, UNITY_MATRIX_M[2].y)),
                                     length(float3(UNITY_MATRIX_M[0].z, UNITY_MATRIX_M[1].z, UNITY_MATRIX_M[2].z))), (_Multiply_f2054904daa44fe0a8b914f24c5a2d7a_Out_2_Float.xxx), _Divide_c0917e1dffc2444ba1d06fd4058878a8_Out_2_Vector3);
            float _Property_482e3cabae6e418cb0d42afc2a5ee85f_Out_0_Float = _ScreenSpaceScale;
            float3 _Multiply_b63b20e9b09145879163b62bfc49a54a_Out_2_Vector3;
            Unity_Multiply_float3_float3(_Divide_c0917e1dffc2444ba1d06fd4058878a8_Out_2_Vector3, (_Property_482e3cabae6e418cb0d42afc2a5ee85f_Out_0_Float.xxx), _Multiply_b63b20e9b09145879163b62bfc49a54a_Out_2_Vector3);
            float2 _GetScreenPositionCustomFunction_6ce49d91c7ad499b8354d539f57bf9fe_ScreenPosition_1_Vector2;
            float2 _GetScreenPositionCustomFunction_6ce49d91c7ad499b8354d539f57bf9fe_ScreenPositionAspectRatio_2_Vector2;
            GetScreenPosition_float(SHADERGRAPH_OBJECT_POSITION, _GetScreenPositionCustomFunction_6ce49d91c7ad499b8354d539f57bf9fe_ScreenPosition_1_Vector2, _GetScreenPositionCustomFunction_6ce49d91c7ad499b8354d539f57bf9fe_ScreenPositionAspectRatio_2_Vector2);
            float2 _GetScreenPositionCustomFunction_d895c8e6953846d19c19ce30dbfdd612_ScreenPosition_1_Vector2;
            float2 _GetScreenPositionCustomFunction_d895c8e6953846d19c19ce30dbfdd612_ScreenPositionAspectRatio_2_Vector2;
            GetScreenPosition_float(IN.WorldSpacePosition, _GetScreenPositionCustomFunction_d895c8e6953846d19c19ce30dbfdd612_ScreenPosition_1_Vector2, _GetScreenPositionCustomFunction_d895c8e6953846d19c19ce30dbfdd612_ScreenPositionAspectRatio_2_Vector2);
            float2 _Subtract_30dd0da3baed49f199aa3557d75532a4_Out_2_Vector2;
            Unity_Subtract_float2(_GetScreenPositionCustomFunction_6ce49d91c7ad499b8354d539f57bf9fe_ScreenPositionAspectRatio_2_Vector2, _GetScreenPositionCustomFunction_d895c8e6953846d19c19ce30dbfdd612_ScreenPositionAspectRatio_2_Vector2, _Subtract_30dd0da3baed49f199aa3557d75532a4_Out_2_Vector2);
            float2 _Normalize_88a5507e0b48445f9294ffa92002d2c9_Out_1_Vector2;
            Unity_Normalize_float2(_Subtract_30dd0da3baed49f199aa3557d75532a4_Out_2_Vector2, _Normalize_88a5507e0b48445f9294ffa92002d2c9_Out_1_Vector2);
            float2 _Multiply_a59f86dd5cf2401bac61902a4c56c06c_Out_2_Vector2;
            Unity_Multiply_float2_float2(_Normalize_88a5507e0b48445f9294ffa92002d2c9_Out_1_Vector2, float2(1, 5), _Multiply_a59f86dd5cf2401bac61902a4c56c06c_Out_2_Vector2);
            float2 _Multiply_58091cb1ddbf4fe391cc229a231bbe46_Out_2_Vector2;
            Unity_Multiply_float2_float2((_Multiply_b63b20e9b09145879163b62bfc49a54a_Out_2_Vector3.xy), _Multiply_a59f86dd5cf2401bac61902a4c56c06c_Out_2_Vector2, _Multiply_58091cb1ddbf4fe391cc229a231bbe46_Out_2_Vector2);
            float2 _Multiply_737819d141724c58bea7e13ea8f2ac3d_Out_2_Vector2;
            Unity_Multiply_float2_float2((_Power_9477134c44f94f89b0cb619fbf210836_Out_2_Float.xx), _Multiply_58091cb1ddbf4fe391cc229a231bbe46_Out_2_Vector2, _Multiply_737819d141724c58bea7e13ea8f2ac3d_Out_2_Vector2);
            float2 _Multiply_496e5802406f425186371299d2b3d574_Out_2_Vector2;
            Unity_Multiply_float2_float2((_InvertColors_f7e477496c31462c87eb912600ed00cd_Out_1_Float.xx), _Multiply_737819d141724c58bea7e13ea8f2ac3d_Out_2_Vector2, _Multiply_496e5802406f425186371299d2b3d574_Out_2_Vector2);
            float4 _ScreenPosition_2b4708c4b586457b915926ad7068e9c1_Out_0_Vector4 = float4(IN.NDCPosition.xy, 0, 0);
            float2 _Add_7a43b140e2cf457db51b5ec47b24cfb7_Out_2_Vector2;
            Unity_Add_float2(_Multiply_496e5802406f425186371299d2b3d574_Out_2_Vector2, (_ScreenPosition_2b4708c4b586457b915926ad7068e9c1_Out_0_Vector4.xy), _Add_7a43b140e2cf457db51b5ec47b24cfb7_Out_2_Vector2);
            float2 _MirrorUVCoordinatesCustomFunction_5747d402c6fc421a88e20db12162dc15_NewUVs_1_Vector2;
            MirrorUVCoordinates_float(_Add_7a43b140e2cf457db51b5ec47b24cfb7_Out_2_Vector2, _MirrorUVCoordinatesCustomFunction_5747d402c6fc421a88e20db12162dc15_NewUVs_1_Vector2);
            float4 _SampleTexture2D_29dddbabc94e4a2b940a6d1c21dc6b10_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_69d77899d4a84dff9e0ba0c4a044b9d8_Out_0_Texture2D.tex, _Property_69d77899d4a84dff9e0ba0c4a044b9d8_Out_0_Texture2D.samplerstate, _Property_69d77899d4a84dff9e0ba0c4a044b9d8_Out_0_Texture2D.GetTransformedUV(_MirrorUVCoordinatesCustomFunction_5747d402c6fc421a88e20db12162dc15_NewUVs_1_Vector2) );
            float _SampleTexture2D_29dddbabc94e4a2b940a6d1c21dc6b10_R_4_Float = _SampleTexture2D_29dddbabc94e4a2b940a6d1c21dc6b10_RGBA_0_Vector4.r;
            float _SampleTexture2D_29dddbabc94e4a2b940a6d1c21dc6b10_G_5_Float = _SampleTexture2D_29dddbabc94e4a2b940a6d1c21dc6b10_RGBA_0_Vector4.g;
            float _SampleTexture2D_29dddbabc94e4a2b940a6d1c21dc6b10_B_6_Float = _SampleTexture2D_29dddbabc94e4a2b940a6d1c21dc6b10_RGBA_0_Vector4.b;
            float _SampleTexture2D_29dddbabc94e4a2b940a6d1c21dc6b10_A_7_Float = _SampleTexture2D_29dddbabc94e4a2b940a6d1c21dc6b10_RGBA_0_Vector4.a;
            float3 _Normalize_3ab1dec6bb6f4731a06525593a132b3f_Out_1_Vector3;
            Unity_Normalize_float3(IN.WorldSpaceViewDirection, _Normalize_3ab1dec6bb6f4731a06525593a132b3f_Out_1_Vector3);
            float _Property_20b7d20ea5e34144a790e1b6f4da7739_Out_0_Float = _SpherePercentage;
            float3 _Multiply_d9233314746a4308a1e14cf64e740ceb_Out_2_Vector3;
            Unity_Multiply_float3_float3(float3(length(float3(UNITY_MATRIX_M[0].x, UNITY_MATRIX_M[1].x, UNITY_MATRIX_M[2].x)),
                                     length(float3(UNITY_MATRIX_M[0].y, UNITY_MATRIX_M[1].y, UNITY_MATRIX_M[2].y)),
                                     length(float3(UNITY_MATRIX_M[0].z, UNITY_MATRIX_M[1].z, UNITY_MATRIX_M[2].z))), (_Property_20b7d20ea5e34144a790e1b6f4da7739_Out_0_Float.xxx), _Multiply_d9233314746a4308a1e14cf64e740ceb_Out_2_Vector3);
            float _RaycastCustomFunction_63a2b127565c4949b44451478d341c25_Hit_4_Float;
            float3 _RaycastCustomFunction_63a2b127565c4949b44451478d341c25_HitPosition_5_Vector3;
            float3 _RaycastCustomFunction_63a2b127565c4949b44451478d341c25_HitNormal_6_Vector3;
            Raycast_float(_WorldSpaceCameraPos, _Normalize_3ab1dec6bb6f4731a06525593a132b3f_Out_1_Vector3, SHADERGRAPH_OBJECT_POSITION, (_Multiply_d9233314746a4308a1e14cf64e740ceb_Out_2_Vector3).x, _RaycastCustomFunction_63a2b127565c4949b44451478d341c25_Hit_4_Float, _RaycastCustomFunction_63a2b127565c4949b44451478d341c25_HitPosition_5_Vector3, _RaycastCustomFunction_63a2b127565c4949b44451478d341c25_HitNormal_6_Vector3);
            float _OneMinus_72b930eb3dee44909e5dcc56d0b6ff9c_Out_1_Float;
            Unity_OneMinus_float(_RaycastCustomFunction_63a2b127565c4949b44451478d341c25_Hit_4_Float, _OneMinus_72b930eb3dee44909e5dcc56d0b6ff9c_Out_1_Float);
            float4 _Property_80800af487674c8ba9a503a7b8b3004d_Out_0_Vector4 = _OuterGlowColor;
            float _Property_376f2c2bdc874238918c02741df7c808_Out_0_Float = _OuterGlowExponent;
            float _FresnelEffect_5444077bc5a8462c89ee3c8ad6f76bbf_Out_3_Float;
            Unity_FresnelEffect_float(_RaycastCustomFunction_63a2b127565c4949b44451478d341c25_HitNormal_6_Vector3, IN.WorldSpaceViewDirection, _Property_376f2c2bdc874238918c02741df7c808_Out_0_Float, _FresnelEffect_5444077bc5a8462c89ee3c8ad6f76bbf_Out_3_Float);
            float _Property_fe4645a506b6401d803892d77c7ccd49_Out_0_Float = _OuterGlowMultiplier;
            float _Multiply_363070eeffcd486cbc8edbd798c77bad_Out_2_Float;
            Unity_Multiply_float_float(_FresnelEffect_5444077bc5a8462c89ee3c8ad6f76bbf_Out_3_Float, _Property_fe4645a506b6401d803892d77c7ccd49_Out_0_Float, _Multiply_363070eeffcd486cbc8edbd798c77bad_Out_2_Float);
            float4 _Multiply_faa4b722b1dc4c2d8f9a283c725e5bd0_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Property_80800af487674c8ba9a503a7b8b3004d_Out_0_Vector4, (_Multiply_363070eeffcd486cbc8edbd798c77bad_Out_2_Float.xxxx), _Multiply_faa4b722b1dc4c2d8f9a283c725e5bd0_Out_2_Vector4);
            float4 _Add_9e051cf94d8a40cf9f53da10df5d6671_Out_2_Vector4;
            Unity_Add_float4((_OneMinus_72b930eb3dee44909e5dcc56d0b6ff9c_Out_1_Float.xxxx), _Multiply_faa4b722b1dc4c2d8f9a283c725e5bd0_Out_2_Vector4, _Add_9e051cf94d8a40cf9f53da10df5d6671_Out_2_Vector4);
            float4 _Lerp_36a8bbf1a73c464e9fdd5f91966fc8dd_Out_3_Vector4;
            Unity_Lerp_float4(_SampleTexture2D_29dddbabc94e4a2b940a6d1c21dc6b10_RGBA_0_Vector4, _Add_9e051cf94d8a40cf9f53da10df5d6671_Out_2_Vector4, (_RaycastCustomFunction_63a2b127565c4949b44451478d341c25_Hit_4_Float.xxxx), _Lerp_36a8bbf1a73c464e9fdd5f91966fc8dd_Out_3_Vector4);
            surface.BaseColor = (_Lerp_36a8bbf1a73c464e9fdd5f91966fc8dd_Out_3_Vector4.xyz);
            surface.Alpha = 1;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
            // must use interpolated tangent, bitangent and normal before they are normalized in the pixel shader.
            float3 unnormalizedNormalWS = input.normalWS;
            const float renormFactor = 1.0 / length(unnormalizedNormalWS);
        
        
            output.WorldSpaceNormal = renormFactor * input.normalWS.xyz;      // we want a unit length Normal Vector node in shader graph
        
        
            output.WorldSpaceViewDirection = GetWorldSpaceNormalizeViewDir(input.positionWS);
            output.WorldSpacePosition = input.positionWS;
        
            #if UNITY_UV_STARTS_AT_TOP
            output.PixelPosition = float2(input.positionCS.x, (_ProjectionParams.x < 0) ? (_ScaledScreenParams.y - input.positionCS.y) : input.positionCS.y);
            #else
            output.PixelPosition = float2(input.positionCS.x, (_ProjectionParams.x > 0) ? (_ScaledScreenParams.y - input.positionCS.y) : input.positionCS.y);
            #endif
        
            output.NDCPosition = output.PixelPosition.xy / _ScaledScreenParams.xy;
            output.NDCPosition.y = 1.0f - output.NDCPosition.y;
        
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/UnlitPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "DepthNormalsOnly"
            Tags
            {
                "LightMode" = "DepthNormalsOnly"
            }
        
        // Render State
        Cull Back
        ZTest LEqual
        ZWrite On
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 2.0
        #pragma multi_compile_instancing
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
        // GraphKeywords: <None>
        
        // Defines
        
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define VARYINGS_NEED_NORMAL_WS
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_DEPTHNORMALSONLY
        #define _SURFACE_TYPE_TRANSPARENT 1
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 normalWS;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float3 normalWS : INTERP0;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.normalWS.xyz = input.normalWS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.normalWS = input.normalWS.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float _SpherePercentage;
        float _DistortionExponent;
        float4 _OuterGlowColor;
        float _OuterGlowExponent;
        float _OuterGlowMultiplier;
        float _ScreenSpaceScale;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_GrabbedTexture);
        SAMPLER(sampler_GrabbedTexture);
        float4 _GrabbedTexture_TexelSize;
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        // GraphFunctions: <None>
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            surface.Alpha = 1;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
        
        
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/DepthNormalsOnlyPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }
        
        // Render State
        Cull Back
        ZTest LEqual
        ZWrite On
        ColorMask 0
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 2.0
        #pragma multi_compile_instancing
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
        // GraphKeywords: <None>
        
        // Defines
        
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define VARYINGS_NEED_NORMAL_WS
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_SHADOWCASTER
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 normalWS;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float3 normalWS : INTERP0;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.normalWS.xyz = input.normalWS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.normalWS = input.normalWS.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float _SpherePercentage;
        float _DistortionExponent;
        float4 _OuterGlowColor;
        float _OuterGlowExponent;
        float _OuterGlowMultiplier;
        float _ScreenSpaceScale;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_GrabbedTexture);
        SAMPLER(sampler_GrabbedTexture);
        float4 _GrabbedTexture_TexelSize;
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        // GraphFunctions: <None>
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            surface.Alpha = 1;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
        
        
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShadowCasterPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "GBuffer"
            Tags
            {
                "LightMode" = "UniversalGBuffer"
            }
        
        // Render State
        Cull Back
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite Off
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 4.5
        #pragma exclude_renderers gles gles3 glcore
        #pragma multi_compile_instancing
        #pragma multi_compile_fog
        #pragma instancing_options renderinglayer
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
        #pragma multi_compile _ LOD_FADE_CROSSFADE
        #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
        // GraphKeywords: <None>
        
        // Defines
        
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_NORMAL_WS
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_GBUFFER
        #define _SURFACE_TYPE_TRANSPARENT 1
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float3 normalWS;
            #if !defined(LIGHTMAP_ON)
             float3 sh;
            #endif
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 WorldSpaceNormal;
             float3 WorldSpaceViewDirection;
             float3 WorldSpacePosition;
             float2 NDCPosition;
             float2 PixelPosition;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
            #if !defined(LIGHTMAP_ON)
             float3 sh : INTERP0;
            #endif
             float3 positionWS : INTERP1;
             float3 normalWS : INTERP2;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            #if !defined(LIGHTMAP_ON)
            output.sh = input.sh;
            #endif
            output.positionWS.xyz = input.positionWS;
            output.normalWS.xyz = input.normalWS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            #if !defined(LIGHTMAP_ON)
            output.sh = input.sh;
            #endif
            output.positionWS = input.positionWS.xyz;
            output.normalWS = input.normalWS.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float _SpherePercentage;
        float _DistortionExponent;
        float4 _OuterGlowColor;
        float _OuterGlowExponent;
        float _OuterGlowMultiplier;
        float _ScreenSpaceScale;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_GrabbedTexture);
        SAMPLER(sampler_GrabbedTexture);
        float4 _GrabbedTexture_TexelSize;
        
        // Graph Includes
        #include "Assets/Shaders/SF_Raycast.hlsl"
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_FresnelEffect_float(float3 Normal, float3 ViewDir, float Power, out float Out)
        {
            Out = pow((1.0 - saturate(dot(normalize(Normal), normalize(ViewDir)))), Power);
        }
        
        void Unity_InvertColors_float(float In, float InvertColors, out float Out)
        {
            Out = abs(InvertColors - In);
        }
        
        void Unity_Subtract_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A - B;
        }
        
        void Unity_Normalize_float3(float3 In, out float3 Out)
        {
            Out = normalize(In);
        }
        
        void Unity_DotProduct_float3(float3 A, float3 B, out float Out)
        {
            Out = dot(A, B);
        }
        
        void Unity_Saturate_float(float In, out float Out)
        {
            Out = saturate(In);
        }
        
        void Unity_Arccosine_float(float In, out float Out)
        {
            Out = acos(In);
        }
        
        void Unity_Divide_float(float A, float B, out float Out)
        {
            Out = A / B;
        }
        
        void Unity_OneMinus_float(float In, out float Out)
        {
            Out = 1 - In;
        }
        
        void Unity_Power_float(float A, float B, out float Out)
        {
            Out = pow(A, B);
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_DegreesToRadians_float(float In, out float Out)
        {
            Out = radians(In);
        }
        
        void Unity_Tangent_float(float In, out float Out)
        {
            Out = tan(In);
        }
        
        void Unity_Distance_float3(float3 A, float3 B, out float Out)
        {
            Out = distance(A, B);
        }
        
        void Unity_Divide_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A / B;
        }
        
        void Unity_Multiply_float3_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A * B;
        }
        
        void Unity_Subtract_float2(float2 A, float2 B, out float2 Out)
        {
            Out = A - B;
        }
        
        void Unity_Normalize_float2(float2 In, out float2 Out)
        {
            Out = normalize(In);
        }
        
        void Unity_Multiply_float2_float2(float2 A, float2 B, out float2 Out)
        {
            Out = A * B;
        }
        
        void Unity_Add_float2(float2 A, float2 B, out float2 Out)
        {
            Out = A + B;
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        void Unity_Add_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A + B;
        }
        
        void Unity_Lerp_float4(float4 A, float4 B, float4 T, out float4 Out)
        {
            Out = lerp(A, B, T);
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float3 BaseColor;
            float Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            UnityTexture2D _Property_69d77899d4a84dff9e0ba0c4a044b9d8_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_GrabbedTexture);
            float _FresnelEffect_0f0943dc3a564f91956808e178c3998e_Out_3_Float;
            Unity_FresnelEffect_float(IN.WorldSpaceNormal, IN.WorldSpaceViewDirection, 0.2, _FresnelEffect_0f0943dc3a564f91956808e178c3998e_Out_3_Float);
            float _InvertColors_f7e477496c31462c87eb912600ed00cd_Out_1_Float;
            float _InvertColors_f7e477496c31462c87eb912600ed00cd_InvertColors = float (1);
            Unity_InvertColors_float(_FresnelEffect_0f0943dc3a564f91956808e178c3998e_Out_3_Float, _InvertColors_f7e477496c31462c87eb912600ed00cd_InvertColors, _InvertColors_f7e477496c31462c87eb912600ed00cd_Out_1_Float);
            float3 _Subtract_fad7af201293423d8122499f95b1b7b9_Out_2_Vector3;
            Unity_Subtract_float3(_WorldSpaceCameraPos, SHADERGRAPH_OBJECT_POSITION, _Subtract_fad7af201293423d8122499f95b1b7b9_Out_2_Vector3);
            float3 _Normalize_5abd8b5118ce435d9139716e6d139c3b_Out_1_Vector3;
            Unity_Normalize_float3(_Subtract_fad7af201293423d8122499f95b1b7b9_Out_2_Vector3, _Normalize_5abd8b5118ce435d9139716e6d139c3b_Out_1_Vector3);
            float _DotProduct_0cde307a59a14b2a93ca6522bd6ef385_Out_2_Float;
            Unity_DotProduct_float3(IN.WorldSpaceNormal, _Normalize_5abd8b5118ce435d9139716e6d139c3b_Out_1_Vector3, _DotProduct_0cde307a59a14b2a93ca6522bd6ef385_Out_2_Float);
            float _Saturate_eec3574569164c51be3c825c4bba17ee_Out_1_Float;
            Unity_Saturate_float(_DotProduct_0cde307a59a14b2a93ca6522bd6ef385_Out_2_Float, _Saturate_eec3574569164c51be3c825c4bba17ee_Out_1_Float);
            float _Arccosine_986c5d2f1a964026840b69446e689f86_Out_1_Float;
            Unity_Arccosine_float(_Saturate_eec3574569164c51be3c825c4bba17ee_Out_1_Float, _Arccosine_986c5d2f1a964026840b69446e689f86_Out_1_Float);
            float _Divide_8bb22013ba8044899d4e09c3738480f7_Out_2_Float;
            Unity_Divide_float(_Arccosine_986c5d2f1a964026840b69446e689f86_Out_1_Float, 1.5, _Divide_8bb22013ba8044899d4e09c3738480f7_Out_2_Float);
            float _OneMinus_10ed0c7c31934669a324bae3231cfe06_Out_1_Float;
            Unity_OneMinus_float(_Divide_8bb22013ba8044899d4e09c3738480f7_Out_2_Float, _OneMinus_10ed0c7c31934669a324bae3231cfe06_Out_1_Float);
            float _Property_a426d4ed7bcf4846afd70efd5477f242_Out_0_Float = _SpherePercentage;
            float _OneMinus_71ce242022d44156828b253d2d059462_Out_1_Float;
            Unity_OneMinus_float(_Property_a426d4ed7bcf4846afd70efd5477f242_Out_0_Float, _OneMinus_71ce242022d44156828b253d2d059462_Out_1_Float);
            float _Divide_b4db269f1d354e75a0e6467b97884ad9_Out_2_Float;
            Unity_Divide_float(_OneMinus_10ed0c7c31934669a324bae3231cfe06_Out_1_Float, _OneMinus_71ce242022d44156828b253d2d059462_Out_1_Float, _Divide_b4db269f1d354e75a0e6467b97884ad9_Out_2_Float);
            float _Property_0c314eb8683a42c293aacfbaf35c2da4_Out_0_Float = _DistortionExponent;
            float _Power_9477134c44f94f89b0cb619fbf210836_Out_2_Float;
            Unity_Power_float(_Divide_b4db269f1d354e75a0e6467b97884ad9_Out_2_Float, _Property_0c314eb8683a42c293aacfbaf35c2da4_Out_0_Float, _Power_9477134c44f94f89b0cb619fbf210836_Out_2_Float);
            float _Float_95c4524b111041dd89db1ed6d4ca4746_Out_0_Float = 60;
            float _Multiply_4c3dca3470804b8dbcd2b0762d3dee92_Out_2_Float;
            Unity_Multiply_float_float(_Float_95c4524b111041dd89db1ed6d4ca4746_Out_0_Float, 0.5, _Multiply_4c3dca3470804b8dbcd2b0762d3dee92_Out_2_Float);
            float _DegreesToRadians_dfd87932145c4301990ee69e3ec61f18_Out_1_Float;
            Unity_DegreesToRadians_float(_Multiply_4c3dca3470804b8dbcd2b0762d3dee92_Out_2_Float, _DegreesToRadians_dfd87932145c4301990ee69e3ec61f18_Out_1_Float);
            float _Tangent_21484ba704ed4646854adb637505b365_Out_1_Float;
            Unity_Tangent_float(_DegreesToRadians_dfd87932145c4301990ee69e3ec61f18_Out_1_Float, _Tangent_21484ba704ed4646854adb637505b365_Out_1_Float);
            float _Float_113cd539138a4222bfce6d620dcf208f_Out_0_Float = 2;
            float _Distance_c996b1aa1861486c81054136b341f9a8_Out_2_Float;
            Unity_Distance_float3(SHADERGRAPH_OBJECT_POSITION, _WorldSpaceCameraPos, _Distance_c996b1aa1861486c81054136b341f9a8_Out_2_Float);
            float _Multiply_28186d21cf78489e84ac995c8b61a63b_Out_2_Float;
            Unity_Multiply_float_float(_Float_113cd539138a4222bfce6d620dcf208f_Out_0_Float, _Distance_c996b1aa1861486c81054136b341f9a8_Out_2_Float, _Multiply_28186d21cf78489e84ac995c8b61a63b_Out_2_Float);
            float _Multiply_f2054904daa44fe0a8b914f24c5a2d7a_Out_2_Float;
            Unity_Multiply_float_float(_Tangent_21484ba704ed4646854adb637505b365_Out_1_Float, _Multiply_28186d21cf78489e84ac995c8b61a63b_Out_2_Float, _Multiply_f2054904daa44fe0a8b914f24c5a2d7a_Out_2_Float);
            float3 _Divide_c0917e1dffc2444ba1d06fd4058878a8_Out_2_Vector3;
            Unity_Divide_float3(float3(length(float3(UNITY_MATRIX_M[0].x, UNITY_MATRIX_M[1].x, UNITY_MATRIX_M[2].x)),
                                     length(float3(UNITY_MATRIX_M[0].y, UNITY_MATRIX_M[1].y, UNITY_MATRIX_M[2].y)),
                                     length(float3(UNITY_MATRIX_M[0].z, UNITY_MATRIX_M[1].z, UNITY_MATRIX_M[2].z))), (_Multiply_f2054904daa44fe0a8b914f24c5a2d7a_Out_2_Float.xxx), _Divide_c0917e1dffc2444ba1d06fd4058878a8_Out_2_Vector3);
            float _Property_482e3cabae6e418cb0d42afc2a5ee85f_Out_0_Float = _ScreenSpaceScale;
            float3 _Multiply_b63b20e9b09145879163b62bfc49a54a_Out_2_Vector3;
            Unity_Multiply_float3_float3(_Divide_c0917e1dffc2444ba1d06fd4058878a8_Out_2_Vector3, (_Property_482e3cabae6e418cb0d42afc2a5ee85f_Out_0_Float.xxx), _Multiply_b63b20e9b09145879163b62bfc49a54a_Out_2_Vector3);
            float2 _GetScreenPositionCustomFunction_6ce49d91c7ad499b8354d539f57bf9fe_ScreenPosition_1_Vector2;
            float2 _GetScreenPositionCustomFunction_6ce49d91c7ad499b8354d539f57bf9fe_ScreenPositionAspectRatio_2_Vector2;
            GetScreenPosition_float(SHADERGRAPH_OBJECT_POSITION, _GetScreenPositionCustomFunction_6ce49d91c7ad499b8354d539f57bf9fe_ScreenPosition_1_Vector2, _GetScreenPositionCustomFunction_6ce49d91c7ad499b8354d539f57bf9fe_ScreenPositionAspectRatio_2_Vector2);
            float2 _GetScreenPositionCustomFunction_d895c8e6953846d19c19ce30dbfdd612_ScreenPosition_1_Vector2;
            float2 _GetScreenPositionCustomFunction_d895c8e6953846d19c19ce30dbfdd612_ScreenPositionAspectRatio_2_Vector2;
            GetScreenPosition_float(IN.WorldSpacePosition, _GetScreenPositionCustomFunction_d895c8e6953846d19c19ce30dbfdd612_ScreenPosition_1_Vector2, _GetScreenPositionCustomFunction_d895c8e6953846d19c19ce30dbfdd612_ScreenPositionAspectRatio_2_Vector2);
            float2 _Subtract_30dd0da3baed49f199aa3557d75532a4_Out_2_Vector2;
            Unity_Subtract_float2(_GetScreenPositionCustomFunction_6ce49d91c7ad499b8354d539f57bf9fe_ScreenPositionAspectRatio_2_Vector2, _GetScreenPositionCustomFunction_d895c8e6953846d19c19ce30dbfdd612_ScreenPositionAspectRatio_2_Vector2, _Subtract_30dd0da3baed49f199aa3557d75532a4_Out_2_Vector2);
            float2 _Normalize_88a5507e0b48445f9294ffa92002d2c9_Out_1_Vector2;
            Unity_Normalize_float2(_Subtract_30dd0da3baed49f199aa3557d75532a4_Out_2_Vector2, _Normalize_88a5507e0b48445f9294ffa92002d2c9_Out_1_Vector2);
            float2 _Multiply_a59f86dd5cf2401bac61902a4c56c06c_Out_2_Vector2;
            Unity_Multiply_float2_float2(_Normalize_88a5507e0b48445f9294ffa92002d2c9_Out_1_Vector2, float2(1, 5), _Multiply_a59f86dd5cf2401bac61902a4c56c06c_Out_2_Vector2);
            float2 _Multiply_58091cb1ddbf4fe391cc229a231bbe46_Out_2_Vector2;
            Unity_Multiply_float2_float2((_Multiply_b63b20e9b09145879163b62bfc49a54a_Out_2_Vector3.xy), _Multiply_a59f86dd5cf2401bac61902a4c56c06c_Out_2_Vector2, _Multiply_58091cb1ddbf4fe391cc229a231bbe46_Out_2_Vector2);
            float2 _Multiply_737819d141724c58bea7e13ea8f2ac3d_Out_2_Vector2;
            Unity_Multiply_float2_float2((_Power_9477134c44f94f89b0cb619fbf210836_Out_2_Float.xx), _Multiply_58091cb1ddbf4fe391cc229a231bbe46_Out_2_Vector2, _Multiply_737819d141724c58bea7e13ea8f2ac3d_Out_2_Vector2);
            float2 _Multiply_496e5802406f425186371299d2b3d574_Out_2_Vector2;
            Unity_Multiply_float2_float2((_InvertColors_f7e477496c31462c87eb912600ed00cd_Out_1_Float.xx), _Multiply_737819d141724c58bea7e13ea8f2ac3d_Out_2_Vector2, _Multiply_496e5802406f425186371299d2b3d574_Out_2_Vector2);
            float4 _ScreenPosition_2b4708c4b586457b915926ad7068e9c1_Out_0_Vector4 = float4(IN.NDCPosition.xy, 0, 0);
            float2 _Add_7a43b140e2cf457db51b5ec47b24cfb7_Out_2_Vector2;
            Unity_Add_float2(_Multiply_496e5802406f425186371299d2b3d574_Out_2_Vector2, (_ScreenPosition_2b4708c4b586457b915926ad7068e9c1_Out_0_Vector4.xy), _Add_7a43b140e2cf457db51b5ec47b24cfb7_Out_2_Vector2);
            float2 _MirrorUVCoordinatesCustomFunction_5747d402c6fc421a88e20db12162dc15_NewUVs_1_Vector2;
            MirrorUVCoordinates_float(_Add_7a43b140e2cf457db51b5ec47b24cfb7_Out_2_Vector2, _MirrorUVCoordinatesCustomFunction_5747d402c6fc421a88e20db12162dc15_NewUVs_1_Vector2);
            float4 _SampleTexture2D_29dddbabc94e4a2b940a6d1c21dc6b10_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_69d77899d4a84dff9e0ba0c4a044b9d8_Out_0_Texture2D.tex, _Property_69d77899d4a84dff9e0ba0c4a044b9d8_Out_0_Texture2D.samplerstate, _Property_69d77899d4a84dff9e0ba0c4a044b9d8_Out_0_Texture2D.GetTransformedUV(_MirrorUVCoordinatesCustomFunction_5747d402c6fc421a88e20db12162dc15_NewUVs_1_Vector2) );
            float _SampleTexture2D_29dddbabc94e4a2b940a6d1c21dc6b10_R_4_Float = _SampleTexture2D_29dddbabc94e4a2b940a6d1c21dc6b10_RGBA_0_Vector4.r;
            float _SampleTexture2D_29dddbabc94e4a2b940a6d1c21dc6b10_G_5_Float = _SampleTexture2D_29dddbabc94e4a2b940a6d1c21dc6b10_RGBA_0_Vector4.g;
            float _SampleTexture2D_29dddbabc94e4a2b940a6d1c21dc6b10_B_6_Float = _SampleTexture2D_29dddbabc94e4a2b940a6d1c21dc6b10_RGBA_0_Vector4.b;
            float _SampleTexture2D_29dddbabc94e4a2b940a6d1c21dc6b10_A_7_Float = _SampleTexture2D_29dddbabc94e4a2b940a6d1c21dc6b10_RGBA_0_Vector4.a;
            float3 _Normalize_3ab1dec6bb6f4731a06525593a132b3f_Out_1_Vector3;
            Unity_Normalize_float3(IN.WorldSpaceViewDirection, _Normalize_3ab1dec6bb6f4731a06525593a132b3f_Out_1_Vector3);
            float _Property_20b7d20ea5e34144a790e1b6f4da7739_Out_0_Float = _SpherePercentage;
            float3 _Multiply_d9233314746a4308a1e14cf64e740ceb_Out_2_Vector3;
            Unity_Multiply_float3_float3(float3(length(float3(UNITY_MATRIX_M[0].x, UNITY_MATRIX_M[1].x, UNITY_MATRIX_M[2].x)),
                                     length(float3(UNITY_MATRIX_M[0].y, UNITY_MATRIX_M[1].y, UNITY_MATRIX_M[2].y)),
                                     length(float3(UNITY_MATRIX_M[0].z, UNITY_MATRIX_M[1].z, UNITY_MATRIX_M[2].z))), (_Property_20b7d20ea5e34144a790e1b6f4da7739_Out_0_Float.xxx), _Multiply_d9233314746a4308a1e14cf64e740ceb_Out_2_Vector3);
            float _RaycastCustomFunction_63a2b127565c4949b44451478d341c25_Hit_4_Float;
            float3 _RaycastCustomFunction_63a2b127565c4949b44451478d341c25_HitPosition_5_Vector3;
            float3 _RaycastCustomFunction_63a2b127565c4949b44451478d341c25_HitNormal_6_Vector3;
            Raycast_float(_WorldSpaceCameraPos, _Normalize_3ab1dec6bb6f4731a06525593a132b3f_Out_1_Vector3, SHADERGRAPH_OBJECT_POSITION, (_Multiply_d9233314746a4308a1e14cf64e740ceb_Out_2_Vector3).x, _RaycastCustomFunction_63a2b127565c4949b44451478d341c25_Hit_4_Float, _RaycastCustomFunction_63a2b127565c4949b44451478d341c25_HitPosition_5_Vector3, _RaycastCustomFunction_63a2b127565c4949b44451478d341c25_HitNormal_6_Vector3);
            float _OneMinus_72b930eb3dee44909e5dcc56d0b6ff9c_Out_1_Float;
            Unity_OneMinus_float(_RaycastCustomFunction_63a2b127565c4949b44451478d341c25_Hit_4_Float, _OneMinus_72b930eb3dee44909e5dcc56d0b6ff9c_Out_1_Float);
            float4 _Property_80800af487674c8ba9a503a7b8b3004d_Out_0_Vector4 = _OuterGlowColor;
            float _Property_376f2c2bdc874238918c02741df7c808_Out_0_Float = _OuterGlowExponent;
            float _FresnelEffect_5444077bc5a8462c89ee3c8ad6f76bbf_Out_3_Float;
            Unity_FresnelEffect_float(_RaycastCustomFunction_63a2b127565c4949b44451478d341c25_HitNormal_6_Vector3, IN.WorldSpaceViewDirection, _Property_376f2c2bdc874238918c02741df7c808_Out_0_Float, _FresnelEffect_5444077bc5a8462c89ee3c8ad6f76bbf_Out_3_Float);
            float _Property_fe4645a506b6401d803892d77c7ccd49_Out_0_Float = _OuterGlowMultiplier;
            float _Multiply_363070eeffcd486cbc8edbd798c77bad_Out_2_Float;
            Unity_Multiply_float_float(_FresnelEffect_5444077bc5a8462c89ee3c8ad6f76bbf_Out_3_Float, _Property_fe4645a506b6401d803892d77c7ccd49_Out_0_Float, _Multiply_363070eeffcd486cbc8edbd798c77bad_Out_2_Float);
            float4 _Multiply_faa4b722b1dc4c2d8f9a283c725e5bd0_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Property_80800af487674c8ba9a503a7b8b3004d_Out_0_Vector4, (_Multiply_363070eeffcd486cbc8edbd798c77bad_Out_2_Float.xxxx), _Multiply_faa4b722b1dc4c2d8f9a283c725e5bd0_Out_2_Vector4);
            float4 _Add_9e051cf94d8a40cf9f53da10df5d6671_Out_2_Vector4;
            Unity_Add_float4((_OneMinus_72b930eb3dee44909e5dcc56d0b6ff9c_Out_1_Float.xxxx), _Multiply_faa4b722b1dc4c2d8f9a283c725e5bd0_Out_2_Vector4, _Add_9e051cf94d8a40cf9f53da10df5d6671_Out_2_Vector4);
            float4 _Lerp_36a8bbf1a73c464e9fdd5f91966fc8dd_Out_3_Vector4;
            Unity_Lerp_float4(_SampleTexture2D_29dddbabc94e4a2b940a6d1c21dc6b10_RGBA_0_Vector4, _Add_9e051cf94d8a40cf9f53da10df5d6671_Out_2_Vector4, (_RaycastCustomFunction_63a2b127565c4949b44451478d341c25_Hit_4_Float.xxxx), _Lerp_36a8bbf1a73c464e9fdd5f91966fc8dd_Out_3_Vector4);
            surface.BaseColor = (_Lerp_36a8bbf1a73c464e9fdd5f91966fc8dd_Out_3_Vector4.xyz);
            surface.Alpha = 1;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
            // must use interpolated tangent, bitangent and normal before they are normalized in the pixel shader.
            float3 unnormalizedNormalWS = input.normalWS;
            const float renormFactor = 1.0 / length(unnormalizedNormalWS);
        
        
            output.WorldSpaceNormal = renormFactor * input.normalWS.xyz;      // we want a unit length Normal Vector node in shader graph
        
        
            output.WorldSpaceViewDirection = GetWorldSpaceNormalizeViewDir(input.positionWS);
            output.WorldSpacePosition = input.positionWS;
        
            #if UNITY_UV_STARTS_AT_TOP
            output.PixelPosition = float2(input.positionCS.x, (_ProjectionParams.x < 0) ? (_ScaledScreenParams.y - input.positionCS.y) : input.positionCS.y);
            #else
            output.PixelPosition = float2(input.positionCS.x, (_ProjectionParams.x > 0) ? (_ScaledScreenParams.y - input.positionCS.y) : input.positionCS.y);
            #endif
        
            output.NDCPosition = output.PixelPosition.xy / _ScaledScreenParams.xy;
            output.NDCPosition.y = 1.0f - output.NDCPosition.y;
        
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/UnlitGBufferPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "SceneSelectionPass"
            Tags
            {
                "LightMode" = "SceneSelectionPass"
            }
        
        // Render State
        Cull Off
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 2.0
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        // PassKeywords: <None>
        // GraphKeywords: <None>
        
        // Defines
        
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_DEPTHONLY
        #define SCENESELECTIONPASS 1
        #define ALPHA_CLIP_THRESHOLD 1
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float _SpherePercentage;
        float _DistortionExponent;
        float4 _OuterGlowColor;
        float _OuterGlowExponent;
        float _OuterGlowMultiplier;
        float _ScreenSpaceScale;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_GrabbedTexture);
        SAMPLER(sampler_GrabbedTexture);
        float4 _GrabbedTexture_TexelSize;
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        // GraphFunctions: <None>
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            surface.Alpha = 1;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
        
        
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/SelectionPickingPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "ScenePickingPass"
            Tags
            {
                "LightMode" = "Picking"
            }
        
        // Render State
        Cull Back
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 2.0
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        // PassKeywords: <None>
        // GraphKeywords: <None>
        
        // Defines
        
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_DEPTHONLY
        #define SCENEPICKINGPASS 1
        #define ALPHA_CLIP_THRESHOLD 1
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float _SpherePercentage;
        float _DistortionExponent;
        float4 _OuterGlowColor;
        float _OuterGlowExponent;
        float _OuterGlowMultiplier;
        float _ScreenSpaceScale;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_GrabbedTexture);
        SAMPLER(sampler_GrabbedTexture);
        float4 _GrabbedTexture_TexelSize;
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        // GraphFunctions: <None>
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            surface.Alpha = 1;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
        
        
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/SelectionPickingPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
    }
    CustomEditor "UnityEditor.ShaderGraph.GenericShaderGraphMaterialGUI"
    CustomEditorForRenderPipeline "UnityEditor.ShaderGraphUnlitGUI" "UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset"
    FallBack "Hidden/Shader Graph/FallbackError"
}