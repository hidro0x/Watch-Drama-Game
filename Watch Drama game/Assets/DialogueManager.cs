using UnityEngine;
using System.Linq;

public class DialogueManager : MonoBehaviour
{
    public DialogueDatabase dialogueDatabase;
    private DialogueNode currentNode;

    public void StartDialogue(int turn, float trust, float faith, float hostility)
    {
        // Uygun diyalogları bul
        foreach (var node in dialogueDatabase.dialogueNodes)
        {
            if (IsAvailable(node, turn, trust, faith, hostility))
            {
                currentNode = node;
                ShowDialogue(node);
                break;
            }
        }
    }

    bool IsAvailable(DialogueNode node, int turn, float trust, float faith, float hostility)
    {
        if (node.minTurn.HasValue && turn < node.minTurn.Value) return false;
        if (node.maxTurn.HasValue && turn > node.maxTurn.Value) return false;
        if (node.minTrust.HasValue && trust < node.minTrust.Value) return false;
        if (node.maxTrust.HasValue && trust > node.maxTrust.Value) return false;
        if (node.minFaith.HasValue && faith < node.minFaith.Value) return false;
        if (node.maxFaith.HasValue && faith > node.maxFaith.Value) return false;
        if (node.minHostility.HasValue && hostility < node.minHostility.Value) return false;
        if (node.maxHostility.HasValue && hostility > node.maxHostility.Value) return false;
        return true;
    }

    void ShowDialogue(DialogueNode node)
    {
        Debug.Log(node.text);
        // UIManager ile entegre edilecek
    }

    public void MakeChoice(int choiceIndex)
    {
        var choice = currentNode.choices[choiceIndex];
        // Barları güncelle (GameManager üzerinden)
        // Sonraki düğüme geç
        if (!string.IsNullOrEmpty(choice.nextNodeId))
        {
            var nextNode = dialogueDatabase.dialogueNodes.FirstOrDefault(n => n.id == choice.nextNodeId);
            if (nextNode != null)
            {
                currentNode = nextNode;
                ShowDialogue(nextNode);
            }
        }
    }
} 