/*
 * Created :    Spring 2022
 * Author :     SeungGeon Kim (keithrek@hanmail.net)
 * Project :    HomebrewIK
 * Filename :   editorHomebrewIK.cs (custom editor module)
 * 
 * All Content (C) 2022 Unlimited Fischl Works, all rights reserved.
 */



using System;       // AttributeUsage
using UnityEngine;  // GUILayout
using UnityEditor;  // Editor



namespace FischlWorks
{



    [CustomEditor(typeof(csHomebrewIK))]
    [CanEditMultipleObjects]
    public class editorHomebrewIK : Editor
    {
        private csHomebrewIK homebrewIK = null;

        private Rect glDisplay = new Rect();

        private Material glMaterial = null;

        private float viewpointOrientation = 0;

        private readonly float gVectorScaleMultiplier = 450;

        private readonly float gFloorHeightMultiplier = 4 / 5f;



        // OnEnable is being used here because the unity manual does so
        void OnEnable()
        {
            homebrewIK = (csHomebrewIK)target;

            glMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));
        }



        public override void OnInspectorGUI()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);

            // Reserving a screen space inside the inspector for display
            glDisplay = GUILayoutUtility.GetAspectRect(3);

            if (Event.current.type == EventType.Repaint)
            {
                GUI.BeginClip(glDisplay);

                glMaterial.SetPass(0);

                /* Background */

                GLDrawShape(GL.QUADS, () =>
                {
                    GL.Color(new Color32(5, 15, 25, 255));

                    GL.Vertex3(0, 0, 0);
                    GL.Vertex3(0, glDisplay.height, 0);
                    GL.Vertex3(glDisplay.width, glDisplay.height, 0);
                    GL.Vertex3(glDisplay.width, 0, 0);
                });

                /* Ground */

                GLDrawShape(GL.QUADS, () =>
                {
                    GL.Color(new Color32(25, 35, 45, 255));

                    GL.Vertex3(0, glDisplay.height * gFloorHeightMultiplier, 0);
                    GL.Vertex3(0, glDisplay.height, 0);
                    GL.Vertex3(glDisplay.width, glDisplay.height, 0);
                    GL.Vertex3(glDisplay.width, glDisplay.height * gFloorHeightMultiplier, 0);
                });

                /* Foot */

                viewpointOrientation = Mathf.Sign(
                    Vector3.ProjectOnPlane(
                    SceneView.lastActiveSceneView.rotation * Vector3.forward,
                    homebrewIK.transform.forward).x);

                /* There's no real computational meaning in world vectors below, they're just there for intuition */

                Vector3 ankleHeightOffsetVector = Vector3.up * homebrewIK._AnkleHeightOffset * gVectorScaleMultiplier;
                Vector3 raySphereRadiusVector = Vector3.up * homebrewIK._RaySphereRadius * gVectorScaleMultiplier;
                Vector3 soleVector = Vector3.right * homebrewIK._LengthFromHeelToToes * gVectorScaleMultiplier;

                if (viewpointOrientation > 0)
                {
                    RotateToAngle(ref ankleHeightOffsetVector, 90 + homebrewIK._LeftFootProjectedAngle * -1);
                    RotateToAngle(ref raySphereRadiusVector, 90 + homebrewIK._LeftFootProjectedAngle * -1);
                    RotateToAngle(ref soleVector, 180 + homebrewIK._LeftFootProjectedAngle * -1);

                    DrawFootByVector(
                        new Vector2(glDisplay.width * 2 / 5, glDisplay.height * gFloorHeightMultiplier),
                        ankleHeightOffsetVector,
                        raySphereRadiusVector,
                        soleVector,
                        homebrewIK._LeftFootIKPositionTarget,
                        homebrewIK._LeftFootProjectedAngle);

                    /* We can reuse the initial vector because this function does not rotate by, but towards */

                    RotateToAngle(ref ankleHeightOffsetVector, 90 + homebrewIK._RightFootProjectedAngle * -1);
                    RotateToAngle(ref raySphereRadiusVector, 90 + homebrewIK._RightFootProjectedAngle * -1);
                    RotateToAngle(ref soleVector, 180 + homebrewIK._RightFootProjectedAngle * -1);

                    DrawFootByVector(
                      new Vector2(glDisplay.width * 4 / 5, glDisplay.height * gFloorHeightMultiplier),
                      ankleHeightOffsetVector,
                      raySphereRadiusVector,
                      soleVector,
                      homebrewIK._RightFootIKPositionTarget,
                      homebrewIK._RightFootProjectedAngle);
                }
                else
                {
                    RotateToAngle(ref ankleHeightOffsetVector, 90 + homebrewIK._LeftFootProjectedAngle);
                    RotateToAngle(ref raySphereRadiusVector, 90 + homebrewIK._LeftFootProjectedAngle);
                    RotateToAngle(ref soleVector, homebrewIK._LeftFootProjectedAngle);

                    DrawFootByVector(
                        new Vector2(glDisplay.width * 1 / 5, glDisplay.height * gFloorHeightMultiplier),
                        ankleHeightOffsetVector,
                        raySphereRadiusVector,
                        soleVector,
                        homebrewIK._LeftFootIKPositionTarget,
                        homebrewIK._LeftFootProjectedAngle);

                    /* We can reuse the initial vector because this function does not rotate by, but towards */

                    RotateToAngle(ref ankleHeightOffsetVector, 90 + homebrewIK._RightFootProjectedAngle);
                    RotateToAngle(ref raySphereRadiusVector, 90 + homebrewIK._RightFootProjectedAngle);
                    RotateToAngle(ref soleVector, homebrewIK._RightFootProjectedAngle);

                    DrawFootByVector(
                        new Vector2(glDisplay.width * 3 / 5, glDisplay.height * gFloorHeightMultiplier),
                        ankleHeightOffsetVector,
                        raySphereRadiusVector,
                        soleVector,
                        homebrewIK._RightFootIKPositionTarget,
                        homebrewIK._RightFootProjectedAngle);
                }

                GUI.EndClip();
            }

            GUILayout.EndVertical();

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            // Draw other script-provided elements
            base.OnInspectorGUI();

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            if (GUILayout.Button("Open Documentation", GUILayout.Height(30)))
            {
                Application.OpenURL("https://nonstop-marigold-de3.notion.site/Docs-HomebrewIK-6455d28e2e184f649e88a429c23047ff");
            }
        }



        private void DrawFootByVector(
            Vector3 anklePosition,
            Vector3 ankleHeightOffsetVector,
            Vector3 raySphereRadiusVector,
            Vector3 soleVector,
            Vector3 IKPositionTarget,
            float projectedAngle)
        {
            anklePosition.y -= IKPositionTarget.y * gVectorScaleMultiplier;

            Vector3 ankleVertex = new Vector3(anklePosition.x, anklePosition.y, 0);
            Vector3 ankleHeightOffsetVertex = new Vector3();
            Vector3 heelVertex = new Vector3();

            if (homebrewIK._AnkleHeightOffset < 0)
            {
                ankleHeightOffsetVertex = ankleVertex - ankleHeightOffsetVector;
                heelVertex = ankleVertex + (raySphereRadiusVector - ankleHeightOffsetVector);
            }
            else
            {
                ankleHeightOffsetVertex = ankleVertex + ankleHeightOffsetVector;
                heelVertex = ankleHeightOffsetVertex + raySphereRadiusVector;
            }

            Vector3 toesVertex = heelVertex + soleVector;
            Vector3 midVertex = (ankleHeightOffsetVertex + heelVertex) / 2 + soleVector;

            GLDrawShape(GL.QUADS, () =>
            {
                GL.Color(new Color32(255, 155, 55, 255));

                PassVertexByVector(ankleVertex);
                PassVertexByVector(heelVertex, 0, -3);
                PassVertexByVector(toesVertex, 0, -3);
                PassVertexByVector(midVertex);
            });

            GLDrawShape(GL.TRIANGLES, () =>
            {
                GL.Color(new Color32(255, 200, 55, 255));

                if (homebrewIK._AnkleHeightOffset < 0)
                {
                    PassVertexByVector(ankleVertex);
                    PassVertexByVector(heelVertex, 0, -3);
                    PassVertexByVector(toesVertex, 0, -3);
                }
                else
                {
                    PassVertexByVector(ankleHeightOffsetVertex);
                    PassVertexByVector(heelVertex, 0, -3);
                    PassVertexByVector(toesVertex, 0, -3);
                }
            });

            GLDrawShape(GL.LINES, () =>
            {
                GL.Color(Color.yellow);

                PassVertexByVector(ankleVertex, 2 * viewpointOrientation);
                PassVertexByVector(ankleHeightOffsetVertex, 2 * viewpointOrientation);
            });

            GLDrawShape(GL.LINES, () =>
            {
                GL.Color(Color.green);

                if (homebrewIK._AnkleHeightOffset < 0)
                {
                    PassVertexByVector(ankleVertex, 2 * viewpointOrientation);
                    PassVertexByVector(heelVertex, 2 * viewpointOrientation);
                }
                else
                {
                    PassVertexByVector(ankleHeightOffsetVertex, 2 * viewpointOrientation);
                    PassVertexByVector(heelVertex, 2 * viewpointOrientation);
                }
            });

            GLDrawShape(GL.LINES, () =>
            {
                if (projectedAngle < 0)
                {
                    GL.Color(new Color32(255, 88, 55, 255));
                }
                else
                {
                    GL.Color(new Color32(55, 100, 255, 255));
                }

                PassVertexByVector(heelVertex, 0, -1);
                PassVertexByVector(toesVertex, 0, -1);
            });

            GLDrawShape(GL.LINES, () =>
            {
                if (projectedAngle < 0)
                {
                    GL.Color(new Color32(255, 88, 55, 255));
                }
                else
                {
                    GL.Color(new Color32(55, 100, 255, 255));
                }

                PassVertexByVector(heelVertex, 0, -2);
                PassVertexByVector(toesVertex, 0, -2);
            });
        }



        private void PassVertexByVector(Vector3 vector, float xOffset = 0, float yOffset = 0)
        {
            if (vector.x < 0 - xOffset)
            {
                vector.x = 0 - xOffset;
            }
            else if (vector.x > glDisplay.width - xOffset - 1)
            {
                vector.x = glDisplay.width - xOffset - 1;
            }

            if (vector.y < 0 - yOffset)
            {
                vector.y = 0 - yOffset;
            }
            else if (vector.y > glDisplay.height - yOffset - 1)
            {
                vector.y = glDisplay.height - yOffset - 1;
            }

            GL.Vertex3(vector.x + xOffset, vector.y + yOffset, 0);
        }



        private void GLDrawShape(int mode, Action function)
        {
            GL.Begin(mode);
            GL.PushMatrix();

            function.Invoke();

            GL.End();
            GL.PopMatrix();
        }



        // Note that when angle is zero, the resulting vector points to Vector3.right
        private void RotateToAngle(ref Vector3 target, float angle)
        {
            target = new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad) * target.magnitude,
                Mathf.Sin(angle * Mathf.Deg2Rad) * target.magnitude,
                0);
        }
    }



    [CustomPropertyDrawer(typeof(ShowIfAttribute))]
    // This attribute is intended to be used with a property field to show / hide it following a bool variable
    public class ShowIfAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ShowIfAttribute attributeHandle = (ShowIfAttribute)attribute;

            SerializedProperty baseProperty = property.serializedObject.FindProperty(attributeHandle._BaseCondition);

            if (baseProperty != null)
            {
                GUI.enabled = baseProperty.boolValue;

                EditorGUI.PropertyField(position, property, label, true);
            }
            else
            {
                Debug.LogError("Designated property was not found : " + attributeHandle._BaseCondition);
            }
        }
    }



    [CustomPropertyDrawer(typeof(BigHeaderAttribute))]
    // DecoratorDrawer must be inherited instead of PropertyDrawer in order not to affect any property field beneath
    public class BigHeaderAttributeDrawer : DecoratorDrawer
    {
        public override void OnGUI(Rect position)
        {
            BigHeaderAttribute attributeHandle = (BigHeaderAttribute)attribute;

            position.yMin += EditorGUIUtility.singleLineHeight * 0.5f;

            // This line of code was fetched from the internal unity header attribute implementation
            position = EditorGUI.IndentedRect(position);

            GUIStyle headerTextStyle = new GUIStyle()
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };

            headerTextStyle.normal.textColor = new Color32(255, 200, 55, 255);

            GUI.Label(position, attributeHandle._Text, headerTextStyle);

            EditorGUI.DrawRect(new Rect(position.xMin, position.yMin, position.width, 1), new Color32(255, 155, 55, 255));
        }

        public override float GetHeight()
        {
            return EditorGUIUtility.singleLineHeight * 2;
        }
    }



}