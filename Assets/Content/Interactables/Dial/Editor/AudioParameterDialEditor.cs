using System;
using System.Collections.Generic;
using System.Linq;
using MackySoft.SerializeReferenceExtensions.Editor;
using NaughtyAttributes.Editor;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AudioParameterDial))]
public class AudioParameterDialEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Disable editing
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((AudioParameterDial)target),
            typeof(AudioParameterDial), false);
        EditorGUILayout.ObjectField("Editor", MonoScript.FromScriptableObject(this),
            typeof(AudioParameterDialEditor), false);
        EditorGUI.EndDisabledGroup();

        AudioParameterDial audioParameterDial = (AudioParameterDial)target;
        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(audioParameterDial.targetProvider)));


        if (audioParameterDial.targetProvider)
        {
            var targetProvider = serializedObject.FindProperty(nameof(audioParameterDial.targetProvider));
            var enumerator = new SerializedObject(targetProvider.objectReferenceValue).GetIterator();
            enumerator.Next(true);

            var audioParameters = new List<SerializedObject>();
            while (enumerator.Next(false))
            {
                if (enumerator.propertyType == SerializedPropertyType.ObjectReference &&
                    enumerator.objectReferenceValue != null &&
                    enumerator.objectReferenceValue.GetType() == typeof(AudioParameter))
                {
                    audioParameters.Add(new SerializedObject(enumerator.objectReferenceValue));
                }
            }

            if (audioParameters.Count != 0)
            {
                var audioParameterNames = audioParameters.Select(
                    parameter => parameter.FindProperty(nameof(AudioParameter.parameterName)).stringValue).ToList();
                var targetParameterName =
                    serializedObject.FindProperty(nameof(AudioParameterDial.targetParameterName));
                
                var targetParameterIndex =
                    audioParameterNames.FindIndex(parameterName => parameterName == targetParameterName.stringValue);
                targetParameterIndex = targetParameterIndex == -1 ? 0 : targetParameterIndex;
                
                targetParameterIndex = EditorGUILayout.Popup("Target Parameter", targetParameterIndex,
                    audioParameterNames.ToArray());
                
                targetParameterName.stringValue = audioParameterNames[targetParameterIndex];

                var targetParameterProp = serializedObject.FindProperty(nameof(audioParameterDial.targetParameter));

                if (targetParameterProp.objectReferenceValue !=
                    audioParameters[targetParameterIndex].targetObject)
                    targetParameterProp.objectReferenceValue =
                        audioParameters[targetParameterIndex].targetObject;
                EditorGUI.BeginDisabledGroup(true);
                targetParameterProp.DrawDefaultInspector();
                EditorGUI.EndDisabledGroup();
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}