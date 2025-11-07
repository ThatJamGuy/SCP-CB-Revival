using UnityEngine;

namespace SickDev.DevConsole {
    //Extracted from UnityEngine.dll to provide backwards compatibility for older versions of Unity
    public static class ColorUtils {
        public static void RGBToHSV(Color rgbColor, out float H, out float S, out float V) {
            if(rgbColor.b > rgbColor.g && rgbColor.b > rgbColor.r)
                RGBToHSVHelper(4f, rgbColor.b, rgbColor.r, rgbColor.g, out H, out S, out V);
            else if(rgbColor.g > rgbColor.r)
                RGBToHSVHelper(2f, rgbColor.g, rgbColor.b, rgbColor.r, out H, out S, out V);
            else
                RGBToHSVHelper(0.0f, rgbColor.r, rgbColor.g, rgbColor.b, out H, out S, out V);
        }

        static void RGBToHSVHelper(float offset, float dominantcolor, float colorone, float colortwo, out float H, out float S, out float V) {
            V = dominantcolor;
            if(V != 0.0) {
                float num1 = colorone <= colortwo ? colorone : colortwo;
                float num2 = V - num1;
                if(num2 != 0.0) {
                    S = num2 / V;
                    H = offset + (colorone - colortwo) / num2;
                }
                else {
                    S = 0.0f;
                    H = offset + (colorone - colortwo);
                }
                H = H / 6f;
                if(H >= 0.0)
                    return;
                H = H + 1f;
            }
            else {
                S = 0.0f;
                H = 0.0f;
            }
        }

        public static Color HSVToRGB(float H, float S, float V) {
            return HSVToRGB(H, S, V, true);
        }

        public static Color HSVToRGB(float H, float S, float V, bool hdr) {
            Color white = Color.white;
            if(S == 0.0) {
                white.r = V;
                white.g = V;
                white.b = V;
            }
            else if(V == 0.0) {
                white.r = 0.0f;
                white.g = 0.0f;
                white.b = 0.0f;
            }
            else {
                white.r = 0.0f;
                white.g = 0.0f;
                white.b = 0.0f;
                float num1 = S;
                float num2 = V;
                float f = H * 6f;
                int num3 = (int)Mathf.Floor(f);
                float num4 = f - (float)num3;
                float num5 = num2 * (1f - num1);
                float num6 = num2 * (float)(1.0 - num1 * num4);
                float num7 = num2 * (float)(1.0 - num1 * (1.0 - num4));
                switch(num3 + 1) {
                case 0:
                    white.r = num2;
                    white.g = num5;
                    white.b = num6;
                    break;
                case 1:
                    white.r = num2;
                    white.g = num7;
                    white.b = num5;
                    break;
                case 2:
                    white.r = num6;
                    white.g = num2;
                    white.b = num5;
                    break;
                case 3:
                    white.r = num5;
                    white.g = num2;
                    white.b = num7;
                    break;
                case 4:
                    white.r = num5;
                    white.g = num6;
                    white.b = num2;
                    break;
                case 5:
                    white.r = num7;
                    white.g = num5;
                    white.b = num2;
                    break;
                case 6:
                    white.r = num2;
                    white.g = num5;
                    white.b = num6;
                    break;
                case 7:
                    white.r = num2;
                    white.g = num7;
                    white.b = num5;
                    break;
                }
                if(!hdr) {
                    white.r = Mathf.Clamp(white.r, 0.0f, 1f);
                    white.g = Mathf.Clamp(white.g, 0.0f, 1f);
                    white.b = Mathf.Clamp(white.b, 0.0f, 1f);
                }
            }
            return white;
        }
    }
}
