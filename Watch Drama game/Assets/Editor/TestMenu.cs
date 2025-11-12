using UnityEditor;

public class TestMenu
{
    [MenuItem("Kahin/Test Menu Item")]
    public static void TestMenuItem()
    {
        UnityEngine.Debug.Log("Test menu item works!");
        EditorUtility.DisplayDialog("Test", "Menu system is working!", "OK");
    }
}

