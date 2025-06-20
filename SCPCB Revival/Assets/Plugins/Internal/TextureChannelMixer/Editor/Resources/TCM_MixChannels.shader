Shader "Hidden/TCM_MixChannels"
{
    Properties
    {
        _RedTex("Red Texture", 2D) = "black"
        _GreenTex("Green Texture", 2D) = "black"
        _BlueTex("Blue Texture", 2D) = "black"
        _AlphaTex("Alpha Texture", 2D) = "black"
    }
        SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _RedTex;
            sampler2D _GreenTex;
            sampler2D _BlueTex;
            sampler2D _AlphaTex;
            float4 _RedMask;
            float4 _GreenMask;
            float4 _BlueMask;
            float4 _AlphaMask;
            int _RedOperationCount;
            int _GreenOperationCount;
            int _BlueOperationCount;
            int _AlphaOperationCount;
            float4 _IsChannelNormalMap;
            float _RedOperations[64];
            float _GreenOperations[64];
            float _BlueOperations[64];
            float _AlphaOperations[64];
            float4 _RedOperationsData[64];
            float4 _GreenOperationsData[64];
            float4 _BlueOperationsData[64];
            float4 _AlphaOperationsData[64];

            float add(float valueA, float valueB) {
                return valueA + valueB;
            }

            float subtract(float valueA, float valueB) {
                return valueA - valueB;
            }

            float multiply(float valueA, float valueB) {
                return valueA * valueB;
            }

            float divide(float valueA, float valueB) {
                return valueA - valueB;
            }

            float oneMinus(float value) {
                return 1.0f - value;
            }

            float negate(float value) {
                return value * -1.0f;
            }

            float clampValue(float value, float minValue, float maxValue) {
                return clamp(value, minValue, maxValue);
            }

            float power(float value, float power) {
                return pow(value, power);
            }

            float absolute(float value) {
                return abs(value);
            }

            float remap(float value, float oldx, float oldy, float newx, float newy) {
                return newx + (value - oldx) * (newy - newx) / (oldy - oldx);
            }

            float modulo(float valueA, float valueB) {
                return valueA % valueB;
            }

            float minValue(float valueA, float valueB) {
                return min(valueA, valueB);
            }

            float maxValue(float valueA, float valueB) {
                return max(valueA, valueB);
            }

            float fraction(float value) {
                return frac(value);
            }

            float squareRoot(float value) {
                return sqrt(value);
            }

            float sine(float value) {
                return sin(value);
            }

            float cosine(float value) {
                return cos(value);
            }

            float tangent(float value) {
                return tan(value);
            }

            float linearInterpolate(float valueA, float valueB, float alpha) {
                return lerp(valueA, valueB, alpha);
            }

            float applyModifiers(float initialValue, int operationCount, float operations[64], float4 operationsData[64]) {
                float result = initialValue;
                for (int i = 0; i < operationCount; ++i) {
                    int operation = round(operations[i]);
                    if (operation == 0) result = add(result, operationsData[i].x);
                    else if (operation == 1) result = operationsData[i].y < 0.5f ? subtract(operationsData[i].x, result) : subtract(result, operationsData[i].x);
                    else if (operation == 2) result = multiply(result, operationsData[i].x);
                    else if (operation == 3) result = operationsData[i].y < 0.5f ? divide(operationsData[i].x, result) : divide(result, operationsData[i].x);
                    else if (operation == 4) result = oneMinus(result);
                    else if (operation == 5) result = negate(result);
                    else if (operation == 6) result = clampValue(result, operationsData[i].x, operationsData[i].y);
                    else if (operation == 7) result = operationsData[i].y < 0.5f ? power(operationsData[i].x, result) : power(result, operationsData[i].x);
                    else if (operation == 8) result = absolute(result);
                    else if (operation == 9) result = remap(result, operationsData[i].x, operationsData[i].y, operationsData[i].z, operationsData[i].w);
                    else if (operation == 10) result = operationsData[i].y < 0.5f ? modulo(operationsData[i].x, result) : modulo(result, operationsData[i].x);
                    else if (operation == 11) result = minValue(result, operationsData[i].x);
                    else if (operation == 12) result = maxValue(result, operationsData[i].x);
                    else if (operation == 13) result = fraction(result);
                    else if (operation == 14) result = squareRoot(result);
                    else if (operation == 15) result = sine(result);
                    else if (operation == 16) result = cosine(result);
                    else if (operation == 17) result = tangent(result);
                    else if (operation == 18) result = operationsData[i].y < 0.5f ? lerp(operationsData[i].x, result, operationsData[i].z) : lerp(result, operationsData[i].x, operationsData[i].z);
                    else if (operation == 19) result = GammaToLinearSpaceExact(result);
                    else if (operation == 20) result = LinearToGammaSpaceExact(result);
                }
                return result;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float r = length(tex2D(_RedTex, i.uv) * _RedMask);
                float g = length(tex2D(_GreenTex, i.uv) * _GreenMask);
                float b = length(tex2D(_BlueTex, i.uv) * _BlueMask);
                float a = length(tex2D(_AlphaTex, i.uv) * _AlphaMask);
                float4 result = float4(r, g, b, a);

#ifndef UNITY_COLORSPACE_GAMMA
                if (_IsChannelNormalMap.x < 0.5f)
                    result.r = LinearToGammaSpaceExact(result.r);
                if (_IsChannelNormalMap.y < 0.5f)
                result.g = LinearToGammaSpaceExact(result.g);
                if (_IsChannelNormalMap.z < 0.5f)
                    result.b = LinearToGammaSpaceExact(result.b);
#endif

                result.r = applyModifiers(result.r, _RedOperationCount, _RedOperations, _RedOperationsData);
                result.g = applyModifiers(result.g, _GreenOperationCount, _GreenOperations, _GreenOperationsData);
                result.b = applyModifiers(result.b, _BlueOperationCount, _BlueOperations, _BlueOperationsData);
                result.a = applyModifiers(result.a, _AlphaOperationCount, _AlphaOperations, _AlphaOperationsData);

#ifndef UNITY_COLORSPACE_GAMMA
                if (_IsChannelNormalMap.x < 0.5f)
                    result.r = GammaToLinearSpaceExact(result.r);
                if (_IsChannelNormalMap.y < 0.5f)
                    result.g = GammaToLinearSpaceExact(result.g);
                if (_IsChannelNormalMap.z < 0.5f)
                    result.b = GammaToLinearSpaceExact(result.b);
#endif

                return result;
            }
            ENDCG
        }
    }
}
