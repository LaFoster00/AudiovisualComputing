using System.Linq;
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
            audioParameterDial.targetParameterIndex = EditorGUILayout.Popup("Target Parameter",
                audioParameterDial.targetParameterIndex, audioParameterNames);

            audioParameterDial.targetParameter = audioParameters[audioParameterDial.targetParameterIndex];

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AudioParameterDial.targetParameter)), true);
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(audioParameterDial);
        }
    }
}