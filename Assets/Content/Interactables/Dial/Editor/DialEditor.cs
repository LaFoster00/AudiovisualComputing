using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes.Editor;
using UnityEditor;
using UnityEngine;
using USCSL;

[CustomEditor(typeof(Dial))]
public class DialEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Disable editing
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((Dial)target),
            typeof(Dial), false);
        EditorGUILayout.ObjectField("Editor", MonoScript.FromScriptableObject(this),
            typeof(DialEditor), false);
        EditorGUI.EndDisabledGroup();

        Dial dial = (Dial)target;
        NaughtyEditorGUI.PropertyField_Layout(serializedObject.FindProperty("knobType"), true);
        NaughtyEditorGUI.PropertyField_Layout(serializedObject.FindProperty("minRotation"), true);
        NaughtyEditorGUI.PropertyField_Layout(serializedObject.FindProperty("maxRotation"), true);
        NaughtyEditorGUI.PropertyField_Layout(serializedObject.FindProperty("steps"), true);
        NaughtyEditorGUI.PropertyField_Layout(serializedObject.FindProperty("angleTolerance"), true);
        EditorGUI.BeginDisabledGroup(true);
        NaughtyEditorGUI.PropertyField_Layout(serializedObject.FindProperty("currentStep"), true);
        EditorGUI.EndDisabledGroup();

        if (dial.TryGetComponent(out AudioParameterDial audioParameterDial))
        {
            var minValue = audioParameterDial.targetParameter.minValue;
            var maxValue = audioParameterDial.targetParameter.maxValue;
            var startPosition = serializedObject.FindProperty("startPosition");
            var startPositionNormalized = startPosition.floatValue;
            if (EditorApplication.isPlaying)
                EditorGUI.BeginDisabledGroup(true);
            var position = EditorGUILayout.GetControlRect(true, EditorGUI.GetPropertyHeight(startPosition));
            var newStartPosition = EditorGUI.Slider(
                    position, 
                    "StartPosition",
                    startPositionNormalized.Map(0f, 1f, minValue, maxValue),
                    minValue,
                    maxValue)
                .Map(minValue, maxValue, 0f, 1f);
            if (Math.Abs(startPosition.floatValue - newStartPosition) > 0.00001f || float.IsNaN(startPosition.floatValue))
            {
                startPosition.floatValue = newStartPosition;
            }
            serializedObject.ApplyModifiedProperties();
            if (EditorApplication.isPlaying)
                EditorGUI.EndDisabledGroup();
            if (EditorGUI.EndChangeCheck())
                PropertyUtility.CallOnValueChangedCallbacks(startPosition);
        }
    }
}