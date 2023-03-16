using UnityEditor;

[CustomEditor(typeof(UL_FastLight)), CanEditMultipleObjects]
public class eUL_FastLight : Editor
{
    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    public override void OnInspectorGUI()
    {
        var fastLight = (UL_FastLight)target;
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(UL_FastLight.range)));
        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(UL_FastLight.color)));
        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(UL_FastLight.intensity)));
        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(UL_FastLight.subtractive)));

        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(UL_FastLight.softCullingDistance)));
        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(UL_FastLight.hardCullingDistance)));

        if (fastLight.softCullingDistance > fastLight.hardCullingDistance - 0.1f) EditorGUILayout.HelpBox("Culling is disabled", MessageType.Info);

        serializedObject.ApplyModifiedProperties();
    }

    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
}
