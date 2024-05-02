using System;
using System.Collections.Generic;
using System.Reflection;
using Assets.Scripts.Player.Actions;
using UnityEditor;
using UnityEngine;

namespace Assets.Editor
{
    [CustomPropertyDrawer(typeof(ActionContainer))]
    public class ActionContainerDrawer : PropertyDrawer
    {
        private const float INT_FIELD_WIDTH = 20f;

        private static List<Type> _actionTypes = new List<Type>(20);

        private void UpdateTypes()
        {
            if (_actionTypes.Count > 0)
                return;

            var actionType = typeof(IAction);
            var types = Assembly.GetAssembly(actionType).GetTypes();

            _actionTypes.Clear();
            foreach (var type in types)
            {
                if (!type.IsAbstract && actionType.IsAssignableFrom(type))
                    _actionTypes.Add(type);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var actionsProperty = property.FindPropertyRelative("_actions");
            var height = EditorGUI.GetPropertyHeight(property) + EditorGUIUtility.standardVerticalSpacing;

            for (var i = 0; i < actionsProperty.arraySize; i++)
            {
                var arrayElement = actionsProperty.GetArrayElementAtIndex(i);

                height += EditorGUI.GetPropertyHeight(arrayElement);
            }

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            UpdateTypes();

            var actionsProperty = property.FindPropertyRelative("_actions");

            Rect labelRect = position;
            labelRect.width = EditorGUIUtility.labelWidth;
            labelRect.height = EditorGUIUtility.singleLineHeight;

            Rect intRect = position;
            intRect.x += EditorGUIUtility.labelWidth + 3;
            intRect.width = INT_FIELD_WIDTH;
            intRect.height = EditorGUIUtility.singleLineHeight;

            EditorGUI.LabelField(labelRect, "Actions", EditorStyles.boldLabel);
            var style = new GUIStyle(EditorStyles.numberField);
            style.alignment = TextAnchor.MiddleRight;
            actionsProperty.arraySize = EditorGUI.IntField(intRect, actionsProperty.arraySize, style);

            DrawArrayField(position, actionsProperty);
        }

        private void DrawArrayField(Rect position, SerializedProperty property)
        {
            var count = property.arraySize;
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            for (var i = 0; i < count; i++)
            {
                var arrayElement = property.GetArrayElementAtIndex(i);

                var height = EditorGUI.GetPropertyHeight(arrayElement);

                DrawField(position, arrayElement);

                position.y += height;
            }
        }

        private void DrawField(Rect position, SerializedProperty property)
        {
            string typeName = property.managedReferenceValue?.GetType().Name ?? "Not set";

            Rect dropdownRect = position;
            dropdownRect.x += EditorGUIUtility.labelWidth + 2;
            dropdownRect.width -= EditorGUIUtility.labelWidth + 2;
            dropdownRect.height = EditorGUIUtility.singleLineHeight;

            if (EditorGUI.DropdownButton(dropdownRect, new GUIContent(typeName), FocusType.Keyboard))
            {
                GenericMenu menu = new GenericMenu();

                // null
                menu.AddItem(new GUIContent("None"), property.managedReferenceValue == null, () =>
                {
                    property.managedReferenceValue = null;
                    property.serializedObject.ApplyModifiedProperties();
                });

                // inherited types
                foreach (Type type in _actionTypes)
                {
                    menu.AddItem(new GUIContent(type.Name), typeName == type.Name, () =>
                    {
                        property.managedReferenceValue = type.GetConstructor(Type.EmptyTypes).Invoke(null);
                        property.serializedObject.ApplyModifiedProperties();
                    });
                }
                menu.ShowAsContext();
            }

            EditorGUI.PropertyField(position, property, new GUIContent(typeName), true);
        }
    }
}