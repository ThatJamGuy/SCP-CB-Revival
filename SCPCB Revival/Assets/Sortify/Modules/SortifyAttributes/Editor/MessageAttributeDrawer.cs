#if UNITY_EDITOR && SORTIFY_ATTRIBUTES
using UnityEditor;
using UnityEngine;

namespace Sortify
{
    [CustomPropertyDrawer(typeof(MessageAttribute))]
    public class MessageAttributeDrawer : PropertyDrawer
    {
        private const float _closeButtonSize = 22f;
        private const float _closeButtonPadding = 2f;
        private const float _helpButtonSize = 20f;
        private const float _helpButtonPadding = 2f;

        private string _playerPrefsKey;
        private bool _infoBoxVisible;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            MessageAttribute infoAttribute = attribute as MessageAttribute;
            _playerPrefsKey = property.propertyPath + "_InfoMessageVisible";
            _infoBoxVisible = EditorPrefs.GetInt(_playerPrefsKey, 1) == 1;

            float infoHeight = _infoBoxVisible ? GetInfoHeight(infoAttribute) : 0f;
            Rect propertyPosition = new Rect(position.x, position.y + infoHeight, position.width - (!_infoBoxVisible ? _helpButtonSize - _helpButtonPadding : 0f), EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(propertyPosition, property, label);

            if (!string.IsNullOrEmpty(infoAttribute?.Message))
            {
                Rect helpButtonRect = new Rect(propertyPosition.xMax + _helpButtonPadding, position.y + (EditorGUIUtility.singleLineHeight - _helpButtonSize) * 0.5f, _helpButtonSize, _helpButtonSize);

                if (!_infoBoxVisible && GUI.Button(helpButtonRect, EditorGUIUtility.IconContent("_Help"), GUIStyle.none))
                    _infoBoxVisible = true;

                if (_infoBoxVisible)
                {
                    float infoWidth = position.width - _closeButtonSize;
                    Rect infoRect = new Rect(position.x, position.y, infoWidth, infoHeight - EditorGUIUtility.standardVerticalSpacing);

                    MessageType messageType = MessageType.Info;
                    switch (infoAttribute.Type)
                    {
                        case MessageAttribute.MessageType.Info:
                            messageType = MessageType.Info;
                            break;
                        case MessageAttribute.MessageType.Warning:
                            messageType = MessageType.Warning;
                            break;
                        case MessageAttribute.MessageType.Error:
                            messageType = MessageType.Error;
                            break;
                    }

                    EditorGUI.HelpBox(infoRect, infoAttribute.Message, messageType);

                    Rect closeButtonRect = new Rect(infoRect.xMax + _closeButtonPadding, infoRect.y + _closeButtonPadding, _closeButtonSize, _closeButtonSize);
                    GUIStyle closeButtonStyle = new GUIStyle(GUI.skin.button)
                    {
                        normal = 
                        {
                            textColor = Color.gray, 
                            background = EditorGUIUtility.whiteTexture
                        }
                    };
                    Color bgColor = GUI.backgroundColor;
                    GUI.backgroundColor = Color.clear;

                    if (GUI.Button(closeButtonRect, "X", closeButtonStyle))
                        _infoBoxVisible = false;

                    GUI.backgroundColor = bgColor;
                }
            }

            EditorPrefs.SetInt(_playerPrefsKey, _infoBoxVisible ? 1 : 0);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            MessageAttribute infoAttribute = attribute as MessageAttribute;

            float infoHeight = _infoBoxVisible ? GetInfoHeight(infoAttribute) : 0f;
            float propertyHeight = EditorGUI.GetPropertyHeight(property, label, true);

            return propertyHeight + infoHeight;
        }

        private float GetInfoHeight(MessageAttribute infoAttribute)
        {
            if (string.IsNullOrEmpty(infoAttribute?.Message))
                return 0f;

            GUIStyle style = new GUIStyle(EditorStyles.helpBox);
            GUIContent content = new GUIContent(infoAttribute.Message);

            float infoHeight = style.CalcHeight(content, EditorGUIUtility.currentViewWidth);
            float padding = EditorGUIUtility.singleLineHeight * 0.5f;

            return infoHeight + padding;
        }
    }
}
#endif
