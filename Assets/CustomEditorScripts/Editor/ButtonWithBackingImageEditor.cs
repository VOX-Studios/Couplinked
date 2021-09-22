using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

[CustomEditor(typeof(ButtonWithBackingImage))]
public class ButtonWithBackingImageEditor : Editor
{
    //public override void OnInspectorGUI()
    //{
    //    ButtonWithBackingImage targetButton = (ButtonWithBackingImage)target;

    //    targetButton.BackingImage = (Image)EditorGUILayout.ObjectField("Backing Image", targetButton.BackingImage, typeof(Image), true);

    //    if (GUI.changed)
    //    {
    //        EditorUtility.SetDirty(targetButton);
    //        EditorSceneManager.MarkSceneDirty(targetButton.gameObject.scene);
    //    }

    //    // Show default inspector property editor
    //    DrawDefaultInspector();
    //}
}