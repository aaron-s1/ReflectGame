// using UnityEditor;
// using UnityEngine;

// [CustomPropertyDrawer(typeof(PlayerController.KeySequenceItem))]
// public class KeySequenceItemDrawer : PropertyDrawer
// {
//     public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//     {
//         EditorGUI.BeginProperty(position, label, property);

//         SerializedProperty key = property.FindPropertyRelative("key");
//         SerializedProperty needsHeldDown = property.FindPropertyRelative("needsHeldDown");
//         SerializedProperty holdDuration = property.FindPropertyRelative("holdDuration");
//         SerializedProperty requiresNextKey = property.FindPropertyRelative("requiresNextKey");
//         SerializedProperty timeHeld = property.FindPropertyRelative("timeHeld");

//         EditorGUI.PropertyField(position, key);

//         position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

//         EditorGUI.PropertyField(position, needsHeldDown);
//         position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

//         // Draw the 'requiresNextKey' field if not the last element and 'needsHeldDown' is true
//         if (property.GetArrayElementAtIndex(property.propertyPath.LastIndexOf(']') - 1).boolValue && property.GetArrayElementAtIndex(property.propertyPath.LastIndexOf(']') + 1).boolValue)
//         {
//             EditorGUI.PropertyField(position, requiresNextKey);
//             position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
//         }
//         else
//         {
//             EditorGUI.BeginDisabledGroup(true);
//             EditorGUI.PropertyField(position, requiresNextKey);
//             EditorGUI.EndDisabledGroup();
//             position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
//         }

//         EditorGUI.PropertyField(position, holdDuration);
//         position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

//         EditorGUI.PropertyField(position, timeHeld);

//         EditorGUI.EndProperty();
//     }

//     public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
//     {
//         int fieldCount = 5; // 'key', 'needsHeldDown', 'requiresNextKey', 'holdDuration', 'timeHeld'

//         // Add one for 'requiresNextKey' if not the last element and 'needsHeldDown' is true
//         if (property.GetArrayElementAtIndex(property.propertyPath.LastIndexOf(']') - 1).boolValue && property.GetArrayElementAtIndex(property.propertyPath.LastIndexOf(']') + 1).boolValue)
//         {
//             fieldCount++;
//         }

//         return fieldCount * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
//     }
// }
