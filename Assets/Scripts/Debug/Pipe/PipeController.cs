using System.Collections.Generic;
using UnityEngine;

public class PipeController : MonoBehaviour {

    List<Vector3> nodes;

    Material material;
    PipeMeshGenerator pmg;

    void Start() {
        nodes = new List<Vector3>();
        foreach (Transform t in transform) {
            nodes.Add(t.localPosition);
        }

        material = new Material(Shader.Find("Specular"));

        pmg = gameObject.AddComponent<PipeMeshGenerator>();
        pmg.points = nodes;
        pmg.pipeMaterial = material;
        pmg.RenderPipe();
    }

    void Update() {
        nodes = new List<Vector3>();
        foreach (Transform t in transform) {
            nodes.Add(t.localPosition);
        }

        pmg.points = nodes;
        pmg.RenderPipe();
    }
}