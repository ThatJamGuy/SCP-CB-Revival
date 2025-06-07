using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TCM_TextureChannelMixer
{
    public class ModifierNameAttribute : System.Attribute
    {
        public string name;
        public int ID;

        public ModifierNameAttribute(string name, int ID)
        {
            this.name = name;
            this.ID = ID;
        }
    }

    public class ModifierHiddenAttribute : System.Attribute
    {
        public bool hideAttribute = true;

        public ModifierHiddenAttribute(bool showAttribute)
        {
            this.hideAttribute = showAttribute;
        }
    }

    [ModifierName("No Name", -1)]
    public interface IResultModifier
    {
        Color ChannelColor { get; set; }
        void Initialize();
        void Draw();
        void GetShaderData(out int operationID, out Vector4 operationData);
        void SetShaderData(Vector4 operationData);
    }

    [ModifierHidden(true)]
    public class ResultModifierNumericOperation : IResultModifier
    {
        public Color ChannelColor { get; set; }
        public float value = 0;
        public bool leftSideValue = false;
        protected string operationString = "?";
        GUIContent swapContent;

        public virtual void Initialize()
        {
            swapContent = new GUIContent("<->", "Swaps the input and value");
        }


        public virtual void Draw()
        {
            GUILayout.BeginHorizontal();
            if (!leftSideValue)
            {
                Color originalColor = GUI.color;
                GUI.color = ChannelColor;
                GUILayout.Label("Input");
                GUI.color = originalColor;
                GUILayout.Label(operationString);
            }
            value = EditorGUILayout.FloatField(value);
            if (leftSideValue)
            {
                GUILayout.Label(operationString);
                Color originalColor = GUI.color;
                GUI.color = ChannelColor;
                GUILayout.Label("Input");
                GUI.color = originalColor;
            }
            if (GUILayout.Button(swapContent, GUILayout.Width(30)))
                leftSideValue = !leftSideValue;
            GUILayout.EndHorizontal();
        }

        public virtual void GetShaderData(out int operationID, out Vector4 operationData)
        {
            operationID = -1;
            operationData = Vector4.one * -1;
        }

        public virtual void SetShaderData(Vector4 operationData)
        {
            value = operationData.x;
            leftSideValue = operationData.y > 0.5f;
        }
    }

    [ModifierName("Basic/Add Value", 0), ModifierHidden(false)]
    public class ResultModifierAddValue : ResultModifierNumericOperation
    {
        public override void Initialize()
        {
            base.Initialize();
            operationString = "+";
            value = 0;
        }

        public override void GetShaderData(out int operationID, out Vector4 operationData)
        {
            operationID = 0;
            operationData = new Vector4(value, 0, 0, 0);
        }
    }

    [ModifierName("Basic/Subtract Value", 1), ModifierHidden(false)]
    public class ResultModifierSubtractValue : ResultModifierNumericOperation
    {
        public override void Initialize()
        {
            base.Initialize();
            operationString = "-";
            value = 0;
        }

        public override void GetShaderData(out int operationID, out Vector4 operationData)
        {
            operationID = 1;
            operationData = new Vector4(value, leftSideValue ? 0 : 1, 0, 0);
        }
    }

    [ModifierName("Basic/Multiply Value", 2), ModifierHidden(false)]
    public class ResultModifierMutiplyValue : ResultModifierNumericOperation
    {
        public override void Initialize()
        {
            base.Initialize();
            operationString = "*";
            value = 1;
        }

        public override void GetShaderData(out int operationID, out Vector4 operationData)
        {
            operationID = 2;
            operationData = new Vector4(value, 0, 0, 0);
        }
    }

    [ModifierName("Basic/Divide Value", 3), ModifierHidden(false)]
    public class ResultModifierDivideValue : ResultModifierNumericOperation
    {
        public override void Initialize()
        {
            base.Initialize();
            operationString = "/";
            value = 1;
        }

        public override void GetShaderData(out int operationID, out Vector4 operationData)
        {
            operationID = 3;
            operationData = new Vector4(value, leftSideValue ? 0 : 1, 0, 0);
        }
    }

    [ModifierName("Range/1-x", 4)]
    public class ResultModifierOneMinus : IResultModifier
    {
        public Color ChannelColor { get; set; }

        public void Initialize()
        {

        }

        public void Draw()
        {
            GUILayout.Label("1  -", GUILayout.Width(24));
            Color originalColor = GUI.color;
            GUI.color = ChannelColor;
            GUILayout.Label("Input");
            GUI.color = originalColor;
        }

        public void GetShaderData(out int operationID, out Vector4 operationData)
        {
            operationID = 4;
            operationData = Vector4.zero;
        }

        public void SetShaderData(Vector4 operationData)
        {
            
        }
    }

    [ModifierName("Advanced/Negate", 5)]
    public class ResultModifierNegate : IResultModifier
    {
        public Color ChannelColor { get; set; }

        public void Initialize()
        {

        }

        public void Draw()
        {
            GUILayout.Label("Negate");
        }

        public void GetShaderData(out int operationID, out Vector4 operationData)
        {
            operationID = 5;
            operationData = Vector4.zero;
        }

        public void SetShaderData(Vector4 operationData)
        {

        }
    }

    [ModifierName("Range/Clamp", 6)]
    public class ResultModifierClamp : IResultModifier
    {
        public Color ChannelColor { get; set; }
        public float minvalue = 0;
        public float maxvalue = 1;

        public virtual void Initialize()
        {

        }

        public void Draw()
        {
            GUILayout.BeginHorizontal();
            float originalWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 25;
            minvalue = EditorGUILayout.FloatField("Min", minvalue, GUILayout.Width(65));
            GUILayout.FlexibleSpace();
            maxvalue = EditorGUILayout.FloatField("Max", maxvalue, GUILayout.Width(65));
            EditorGUIUtility.labelWidth = originalWidth;
            GUILayout.EndHorizontal();
        }

        public void GetShaderData(out int operationID, out Vector4 operationData)
        {
            operationID = 6;
            operationData = new Vector4(minvalue, maxvalue, 0, 0);
        }

        public void SetShaderData(Vector4 operationData)
        {
            minvalue = operationData.x;
            maxvalue = operationData.y;
        }
    }

    [ModifierName("Basic/Power", 7), ModifierHidden(false)]
    public class ResultModifierPower : ResultModifierNumericOperation
    {
        public override void Initialize()
        {
            base.Initialize();
            operationString = "^";
            value = 2;
        }

        public override void GetShaderData(out int operationID, out Vector4 operationData)
        {
            operationID = 7;
            operationData = new Vector4(value, leftSideValue ? 0 : 1, 0, 0);
        }
    }

    [ModifierName("Advanced/Absolute", 8)]
    public class ResultModifierAbsolute : IResultModifier
    {
        public Color ChannelColor { get; set; }

        public void Initialize()
        {

        }

        public void Draw()
        {
            GUILayout.Label("Abs");
        }

        public void GetShaderData(out int operationID, out Vector4 operationData)
        {
            operationID = 8;
            operationData = Vector4.zero;
        }

        public void SetShaderData(Vector4 operationData)
        {

        }
    }

    [ModifierName("Range/Remap", 9)]
    public class ResultModifierRemap : IResultModifier
    {
        public Color ChannelColor { get; set; }
        public Vector2 oldRange;
        public Vector2 newRange;

        public void Initialize()
        {
            oldRange = new Vector2(0, 1);
            newRange = new Vector2(0, 1);
        }

        public void Draw()
        {
            float originalWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 25;
            GUILayout.BeginVertical();
            oldRange = EditorGUILayout.Vector2Field("Old Range", oldRange);
            newRange = EditorGUILayout.Vector2Field("New Range", newRange);
            GUILayout.EndVertical();
            EditorGUIUtility.labelWidth = originalWidth;
        }

        public void GetShaderData(out int operationID, out Vector4 operationData)
        {
            operationID = 9;
            operationData = new Vector4(oldRange.x, oldRange.y, newRange.x, newRange.y);
        }

        public void SetShaderData(Vector4 operationData)
        {
            oldRange = new Vector2(operationData.x, operationData.y);
            newRange = new Vector2(operationData.z, operationData.w);
        }
    }

    [ModifierName("Advanced/Modulo", 10), ModifierHidden(false)]
    public class ResultModifierModulo : ResultModifierNumericOperation
    {
        public override void Initialize()
        {
            base.Initialize();
            operationString = "%";
            value = 2;
        }

        public override void GetShaderData(out int operationID, out Vector4 operationData)
        {
            operationID = 10;
            operationData = new Vector4(value, leftSideValue ? 0 : 1, 0, 0);
        }
    }

    [ModifierName("Range/Min", 11), ModifierHidden(false)]
    public class ResultModifierMín : ResultModifierNumericOperation
    {
        public override void Initialize()
        {
            base.Initialize();
            operationString = "Min";
            value = 1;
        }

        public override void GetShaderData(out int operationID, out Vector4 operationData)
        {
            operationID = 11;
            operationData = new Vector4(value, leftSideValue ? 0 : 1, 0, 0);
        }
    }

    [ModifierName("Range/Max", 12), ModifierHidden(false)]
    public class ResultModifierMax : ResultModifierNumericOperation
    {
        public override void Initialize()
        {
            base.Initialize();
            operationString = "Max";
            value = 0;
        }

        public override void GetShaderData(out int operationID, out Vector4 operationData)
        {
            operationID = 12;
            operationData = new Vector4(value, leftSideValue ? 0 : 1, 0, 0);
        }
    }

    [ModifierName("Range/Fraction", 13)]
    public class ResultModifierFraction : IResultModifier
    {
        public Color ChannelColor { get; set; }

        public void Initialize()
        {

        }

        public void Draw()
        {
            GUILayout.Label("Frac");
        }

        public void GetShaderData(out int operationID, out Vector4 operationData)
        {
            operationID = 13;
            operationData = Vector4.zero;
        }

        public void SetShaderData(Vector4 operationData)
        {

        }
    }

    [ModifierName("Basic/Square root", 14)]
    public class ResultModifierSqrt : IResultModifier
    {
        public Color ChannelColor { get; set; }

        public void Initialize()
        {

        }

        public void Draw()
        {
            GUILayout.Label("Sqrt");
        }

        public void GetShaderData(out int operationID, out Vector4 operationData)
        {
            operationID = 14;
            operationData = Vector4.zero;
        }

        public void SetShaderData(Vector4 operationData)
        {

        }
    }

    [ModifierName("Trigonometry/Sin", 15)]
    public class ResultModifierSin : IResultModifier
    {
        public Color ChannelColor { get; set; }

        public void Initialize()
        {

        }

        public void Draw()
        {
            GUILayout.Label("Sin");
        }

        public void GetShaderData(out int operationID, out Vector4 operationData)
        {
            operationID = 15;
            operationData = Vector4.zero;
        }

        public void SetShaderData(Vector4 operationData)
        {

        }
    }

    [ModifierName("Trigonometry/Cos", 166)]
    public class ResultModifierCos : IResultModifier
    {
        public Color ChannelColor { get; set; }

        public void Initialize()
        {

        }

        public void Draw()
        {
            GUILayout.Label("Cos");
        }

        public void GetShaderData(out int operationID, out Vector4 operationData)
        {
            operationID = 16;
            operationData = Vector4.zero;
        }

        public void SetShaderData(Vector4 operationData)
        {

        }
    }

    [ModifierName("Trigonometry/Tan", 17)]
    public class ResultModifierTan : IResultModifier
    {
        public Color ChannelColor { get; set; }

        public void Initialize()
        {

        }

        public void Draw()
        {
            GUILayout.Label("Tan");
        }

        public void GetShaderData(out int operationID, out Vector4 operationData)
        {
            operationID = 17;
            operationData = Vector4.zero;
        }

        public void SetShaderData(Vector4 operationData)
        {

        }
    }

    [ModifierName("Interpolation/Lerp", 18), ModifierHidden(false)]
    public class ResultModifierLerp : ResultModifierNumericOperation
    {
        public float lerpValue;

        public override void Initialize()
        {
            base.Initialize();
            operationString = ",";
            value = 0;
        }

        public override void Draw()
        {
            GUILayout.BeginVertical();
            base.Draw();
            float originalWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 35;
            lerpValue = EditorGUILayout.Slider("Alpha", lerpValue, 0.0f, 1.0f);
            EditorGUIUtility.labelWidth = originalWidth;
            GUILayout.EndVertical();
        }

        public override void GetShaderData(out int operationID, out Vector4 operationData)
        {
            operationID = 18;
            operationData = new Vector4(value, leftSideValue ? 0 : 1, lerpValue, 0);
        }

        public override void SetShaderData(Vector4 operationData)
        {
            base.SetShaderData(operationData);
            lerpValue = operationData.z;
        }
    }

    [ModifierName("Advanced/Gamma to Linear", 19)]
    public class ResultModifierGammaToLinear : IResultModifier
    {
        public Color ChannelColor { get; set; }

        public void Initialize()
        {

        }

        public void Draw()
        {
            GUILayout.Label("Gamma to Linear");
        }

        public void GetShaderData(out int operationID, out Vector4 operationData)
        {
            operationID = 19;
            operationData = Vector4.zero;
        }

        public void SetShaderData(Vector4 operationData)
        {

        }
    }

    [ModifierName("Advanced/Linear to Gamma", 20)]
    public class ResultModifierLinearToGamma : IResultModifier
    {
        public Color ChannelColor { get; set; }

        public void Initialize()
        {

        }

        public void Draw()
        {
            GUILayout.Label("Linear to Gamma");
        }

        public void GetShaderData(out int operationID, out Vector4 operationData)
        {
            operationID = 20;
            operationData = Vector4.zero;
        }

        public void SetShaderData(Vector4 operationData)
        {

        }
    }
}