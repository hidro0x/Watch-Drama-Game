using UnityEngine;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector;
using UnityEditor;

public class DialogueEditorWindow : OdinEditorWindow
{
    [InlineEditor(Expanded = true)]
    public DialogueDatabase dialogueDatabase;

    [MenuItem("Kahin/Dialogue Editor")]
    private static void OpenWindow()
    {
        var window = GetWindow<DialogueEditorWindow>("Dialogue Editor");
        window.Show();
    }
} 