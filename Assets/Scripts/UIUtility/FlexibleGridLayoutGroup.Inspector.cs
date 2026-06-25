#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public partial class FlexibleGridLayoutGroup
{
    [CustomEditor(typeof(FlexibleGridLayoutGroup))]
    public class Inspector : Editor
    {
        private SerializedProperty m_Padding;
        private SerializedProperty m_ChildAlignment;

        private SerializedProperty m_OrderPriority;
        private SerializedProperty m_GridSize;

        private SerializedProperty m_RowSpacing;
        private SerializedProperty m_ColumnSpacing;

        private SerializedProperty m_ReverseArrangement;

        private SerializedProperty m_ChildControlWidth;
        private SerializedProperty m_ChildControlHeight;

        private SerializedProperty m_ChildForceExpandWidth;
        private SerializedProperty m_ChildForceExpandHeight;

        private SerializedProperty m_ChildScaleWidth;
        private SerializedProperty m_ChildScaleHeight;

        private void OnEnable()
        {
            m_Padding = serializedObject.FindProperty("m_Padding");
            m_ChildAlignment = serializedObject.FindProperty("m_ChildAlignment");

            m_OrderPriority = serializedObject.FindProperty("m_OrderPriority");
            m_GridSize = serializedObject.FindProperty("m_GridSize");

            m_RowSpacing = serializedObject.FindProperty("m_RowSpacing");
            m_ColumnSpacing = serializedObject.FindProperty("m_ColumnSpacing");

            m_ReverseArrangement = serializedObject.FindProperty("m_ReverseArrangement");

            m_ChildControlWidth = serializedObject.FindProperty("m_ChildControlWidth");
            m_ChildControlHeight = serializedObject.FindProperty("m_ChildControlHeight");

            m_ChildForceExpandWidth = serializedObject.FindProperty("m_ChildForceExpandWidth");
            m_ChildForceExpandHeight = serializedObject.FindProperty("m_ChildForceExpandHeight");

            m_ChildScaleWidth = serializedObject.FindProperty("m_ChildScaleWidth");
            m_ChildScaleHeight = serializedObject.FindProperty("m_ChildScaleHeight");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Padding);
            EditorGUILayout.PropertyField(m_ChildAlignment);

            EditorGUILayout.PropertyField(m_OrderPriority);
            EditorGUILayout.PropertyField(m_GridSize);

            EditorGUILayout.PropertyField(m_RowSpacing);
            EditorGUILayout.PropertyField(m_ColumnSpacing);

            EditorGUILayout.PropertyField(m_ReverseArrangement);

            DrawInlineTogglePair("Control Child Size", m_ChildControlWidth, m_ChildControlHeight);
            DrawInlineTogglePair("Use Child Scale", m_ChildScaleWidth, m_ChildScaleHeight);
            DrawInlineTogglePair("Child Force Expand", m_ChildForceExpandWidth, m_ChildForceExpandHeight);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawInlineTogglePair(string label, SerializedProperty widthProperty, SerializedProperty heightProperty)
        {
            var totalRect = EditorGUILayout.GetControlRect();
            var fieldRect = EditorGUI.PrefixLabel(totalRect, new GUIContent(label));

            var savedLabelWidth = EditorGUIUtility.labelWidth;
            var savedIndentLevel = EditorGUI.indentLevel;

            EditorGUIUtility.labelWidth = 50f;
            EditorGUI.indentLevel = 0;

            var halfWidth = fieldRect.width * 0.5f;
            var widthToggleRect = new Rect(fieldRect.x, fieldRect.y, halfWidth, fieldRect.height);
            var heightToggleRect = new Rect(fieldRect.x + halfWidth, fieldRect.y, halfWidth, fieldRect.height);

            EditorGUI.PropertyField(widthToggleRect, widthProperty, new GUIContent("Width"));
            EditorGUI.PropertyField(heightToggleRect, heightProperty, new GUIContent("Height"));

            EditorGUIUtility.labelWidth = savedLabelWidth;
            EditorGUI.indentLevel = savedIndentLevel;
        }
    }
}
#endif
