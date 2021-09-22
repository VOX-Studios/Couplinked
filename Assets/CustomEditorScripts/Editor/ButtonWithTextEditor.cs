using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

[CustomEditor(typeof(ButtonWithText))]
public class ButtonWithTextEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ButtonWithText targetButton = (ButtonWithText)target;

        targetButton.text = (Text)EditorGUILayout.ObjectField("Text", targetButton.text, typeof(Text), true);

        if (GUI.changed)
        {
            EditorUtility.SetDirty(targetButton);
            EditorSceneManager.MarkSceneDirty(targetButton.gameObject.scene);
        }

        // Show default inspector property editor
        DrawDefaultInspector();
    }
}