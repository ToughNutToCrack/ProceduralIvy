using UnityEngine;

public class DebugNode {
    public Vector3 position;
    public Color color;
    public string label;

    public DebugNode(Vector3 position, Color color, string label) {
        this.position = position;
        this.color = color;
        this.label = label;
    }
}
