using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Graph")]
public class DialogueGraph : ScriptableObject
{
    public List<DialogueNode> m_nodes = new List<DialogueNode>();
}
