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
    private SerializedProperty _knobType;
    private SerializedProperty _minRotation;
    private SerializedProperty _maxRotation;
    private SerializedProperty _steps;
    private SerializedProperty _currentStep;
    private SerializedProperty _startPosition;


    public void OnEnable()
    {
        _knobType = serializedObject.FindProperty("knobType");
        _minRotation = serializedObject.FindProperty("minRotation");
        _maxRotation = serializedObject.FindProperty("maxRotation");
        _steps = serializedObject.FindProperty("steps");
        _currentStep = serializedObject.FindProperty("currentStep");
        _startPosition = serializedObject.FindProperty("startPosition");
    }

    private void DrawSimpleFields()
    {
        NaughtyEditorGUI.PropertyField_Layout(_knobType, true);
        NaughtyEditorGUI.PropertyField_Layout(_minRotation, true);
        NaughtyEditorGUI.PropertyField_Layout(_maxRotation, true);
        NaughtyEditorGUI.PropertyField_Layout(_steps, true); 
        EditorGUI.BeginDisabledGroup(true);
        NaughtyEditorGUI.PropertyField_Layout(_currentStep, true);
        EditorGUI.EndDisabledGroup();
    }

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
        DrawSimpleFields();

        if (dial.TryGetComponent(out AudioParameterDial audioParameterDial))
        {
            var audioParameterDialSo = new SerializedObject(audioParameterDial);
            audioParameterDialSo.Update();
            var targetParameterProp = audioParameterDialSo
                .FindProperty(nameof(audioParameterDial.targetParameter));
            if (targetParameterProp.objectReferenceValue != null)
            {
                var targetParameter = new SerializedObject(targetParameterProp.objectReferenceValue);
                if (targetParameter.targetObject != null)
                {
                    var minValue =
                        targetParameter.FindProperty(nameof(audioParameterDial.targetParameter.minValue));
                    var maxValue =
                        targetParameter.FindProperty(nameof(audioParameterDial.targetParameter.maxValue));
                    // Only enable start position settings when not in the play in editor mode
                    if (EditorApplication.isPlaying)
                        EditorGUI.BeginDisabledGroup(true);
                    var position = EditorGUILayout.GetControlRect(true, EditorGUI.GetPropertyHeight(_startPosition));
                    var newStartPosition = EditorGUI.Slider(
                            position,
                            "StartPosition",
                            _startPosition.floatValue.Map(0f, 1f, minValue.floatValue, maxValue.floatValue),
                            minValue.floatValue,
                            maxValue.floatValue)
                        .Map(minValue.floatValue, maxValue.floatValue, 0f, 1f);
                    if (Math.Abs(_startPosition.floatValue - newStartPosition) > 0.00001f ||
                        float.IsNaN(_startPosition.floatValue))
                    {
                        _startPosition.floatValue = newStartPosition;
                    }

                    if (EditorApplication.isPlaying)
                        EditorGUI.EndDisabledGroup();

                    if (GUI.changed)
                    {
                        PropertyUtility.CallOnValueChangedCallbacks(_startPosition);
                    }

                    targetParameter.ApplyModifiedProperties();
                    EditorUtility.SetDirty(targetParameter.targetObject);
                }
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}