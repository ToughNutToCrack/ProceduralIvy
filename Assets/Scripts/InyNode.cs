using UnityEngine;

public class IvyNode {
    Vector3 position;
    Vector3 normal;
    Vector3 tangent;
    string name;

    public IvyNode(Vector3 position, Vector3 normal) {
        this.position = position;
        this.normal = normal;
    }

    public IvyNode(Vector3 position, Vector3 normal, Vector3 tangent, string name) {
        this.position = position;
        this.normal = normal;
        this.tangent = tangent;
        this.name = name;
    }

    public Vector3 getPosition() => position;
    public Vector3 getNormal() => normal;
    public Vector3 getTangent() => tangent;
    public string getName() => name;
}