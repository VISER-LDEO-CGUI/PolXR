Shader "Unlit/Fader"
{
    Properties
    {
        // https://docs.unity3d.com/Manual/SL-Properties.html
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        // Tags https://docs.unity3d.com/Manual/SL-SubShaderTags.html
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        // Default Unlit/Transparent LOD. See https://docs.unity3d.com/Manual/SL-ShaderLOD.html
        LOD 100

        // Commands
        // Legacy (but shorter) fixed function shader command for color
        Color[_Color]
        // Depth testing: we don't want to limit to closest pixels, as the fader screen would be the only visible then
        ZTest Always
        // Alpha blending: Traditional transparency (https://docs.unity3d.com/Manual/SL-Blend.html)
        Blend SrcAlpha OneMinusSrcAlpha

        // Pass
        Pass 
        {
            // No actual instructions
        }
    }
}
