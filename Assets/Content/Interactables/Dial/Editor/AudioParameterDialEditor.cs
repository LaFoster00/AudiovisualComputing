using System.Linq;
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
        audioParameterDial.targetProvider = (AudioProvider)EditorGUILayout.ObjectField(
            "Target Audio Provider",
            audioParameterDial.targetProvider,
            typeof(AudioProvider),
            true);


        if (audioParameterDial.targetProvider)
        {
            var audioParameters = audioParameterDial.targetProvider
                .GetType()
                .GetFields()
                .Where(info => info.FieldType == typeof(AudioParameter))
                .Select(info => (AudioParameter)info.GetValue(audioParameterDial.targetProvider))
                .ToArray();
            var audioParameterNames = audioParameters.Select(parameter => parameter.name).ToArray();
            var targetParameterIndexProp =
                serializedObject.FindProperty(nameof(AudioParameterDial.targetParameterIndex));
            targetParameterIndexProp.intValue = EditorGUILayout.Popup("Target Parameter",
                targetParameterIndexProp.intValue, audioParameterNames);

            var targetParameterProp = serializedObject.FindProperty(nameof(audioParameterDial.targetParameter));
            targetParameterProp.managedReferenceValue = audioParameters[audioParameterDial.targetParameterIndex];
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(targetParameterProp, true);
            EditorGUI.EndDisabledGroup();
            
            serializedObject.ApplyModifiedProperties();
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(audioParameterDial);
        }
    }
}