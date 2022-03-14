using System.Collections.Generic;
using UnityEngine;

public class Branch : MonoBehaviour {
    const string AMOUNT = "_Amount";
    const string RADIUS = "_Radius";
    const float MAX = 0.5f;

    List<IvyNode> branchNodes;

    Mesh mesh;
    Material material;
    Material leafMaterial;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;

    float branchRadius = 0.02f;
    int meshFaces = 8;

    bool animate;
    float growthSpeed = 2;
    float currentAmount = -1;

    public void init(List<IvyNode> branchNodes, float branchRadius, Material material) {
        this.branchNodes = branchNodes;
        this.branchRadius = branchRadius;
        this.material = new Material(material);
        mesh = createMesh(branchNodes);
    }

    void Start() {
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        if (material == null) {
            material = new Material(Shader.Find("Specular"));
        }

        leafMaterial = material;
        meshRenderer.material = material;
        if (mesh != null) {
            meshFilter.mesh = mesh;
        }

        material.SetFloat(RADIUS, branchRadius);
        material.SetFloat(AMOUNT, currentAmount);
        animate = true;
    }

    void Update() {
        if (animate) {
            currentAmount += Time.deltaTime * growthSpeed;
            material.SetFloat(AMOUNT, currentAmount);
            if (currentAmount >= MAX) {
                animate = false;
            }
        }
    }

    float remap(float input, float oldLow, float oldHigh, float newLow, float newHigh) {
        float t = Mathf.InverseLerp(oldLow, oldHigh, input);
        return Mathf.Lerp(newLow, newHigh, t);
    }

    Mesh createMesh(List<IvyNode> nodes) {
        Mesh branchMesh = new Mesh();

        Vector3[] vertices = new Vector3[(nodes.Count) * meshFaces * 4];
        Vector3[] normals = new Vector3[nodes.Count * meshFaces * 4];
        Vector2[] uv = new Vector2[nodes.Count * meshFaces * 4];
        int[] triangles = new int[(nodes.Count - 1) * meshFaces * 6];

        for (int i = 0; i < nodes.Count; i++) {
            float vStep = (2f * Mathf.PI) / meshFaces;

            var fw = Vector3.zero;
            if (i > 0) {
                fw = branchNodes[i - 1].getPosition() - branchNodes[i].getPosition();
            }

            if (i < branchNodes.Count - 1) {
                fw += branchNodes[i].getPosition() - branchNodes[i + 1].getPosition();
            }

            if (fw == Vector3.zero) {
                fw = Vector3.forward;
            }

            // fw.z = 0;
            fw.Normalize();

            var up = branchNodes[i].getNormal();
            up.Normalize();

            for (int v = 0; v < meshFaces; v++) {



                var orientation = Quaternion.LookRotation(fw, up);
                Vector3 xAxis = Vector3.up;
                Vector3 yAxis = Vector3.right;
                Vector3 pos = branchNodes[i].getPosition();
                // var radius = (1 - remap(i, 0, nodes.Count - 1, 0, 0.95f)) * branchRadius;
                pos += orientation * xAxis * (branchRadius * Mathf.Sin(v * vStep));
                pos += orientation * yAxis * (branchRadius * Mathf.Cos(v * vStep));



                var diff = pos - branchNodes[i].getPosition();

                // if (i == 0 || i + 1 >= nodes.Count) {
                //     pos = branchNodes[i].getPosition();
                // }
                vertices[i * meshFaces + v] = pos;

                normals[i * meshFaces + v] = diff / diff.magnitude;
                float uvID = remap(i, 0, nodes.Count - 1, 0, 1);


                uv[i * meshFaces + v] = new Vector2((float)v / meshFaces, uvID);
            }

            if (i + 1 < nodes.Count) {
                for (int v = 0; v < meshFaces; v++) {
                    triangles[i * 48 + v * 6] = ((v + 1) % meshFaces) + i * meshFaces;
                    triangles[i * 48 + v * 6 + 1] = triangles[i * 48 + v * 6 + 4] = v + i * meshFaces;
                    triangles[i * 48 + v * 6 + 2] = triangles[i * 48 + v * 6 + 3] = ((v + 1) % meshFaces + meshFaces) + i * meshFaces;
                    triangles[i * 48 + v * 6 + 5] = (meshFaces + v % meshFaces) + i * meshFaces;
                }
            }
        }

        branchMesh.vertices = vertices;
        branchMesh.triangles = triangles;
        branchMesh.normals = normals;
        branchMesh.uv = uv;
        return branchMesh;
    }



    void OnDrawGizmosSelected() {

        if (branchNodes != null) {
            for (int i = 0; i < branchNodes.Count; i++) {
                Gizmos.color = Constants.getColor(branchNodes[i].getName());
                Gizmos.DrawSphere(branchNodes[i].getPosition(), .002f);
                Gizmos.color = Color.white;
                Gizmos.DrawLine(branchNodes[i].getPosition(), branchNodes[i].getPosition() + branchNodes[i].getNormal() * .1f);
                // Gizmos.color = Color.blue;
                // Gizmos.DrawLine(branchNodes[i].getPosition(), branchNodes[i].getPosition() + Vector3.Cross(branchNodes[i].getTangent(), branchNodes[i].getNormal()) * .1f);

                if (i > 0) {
                    // Gizmos.color = Color.green;
                    // Gizmos.DrawLine(branchNodes[i - 1].getPosition(), branchNodes[i].getPosition());
                }

                Gizmos.color = Color.blue;
                // var fw = Vector3.zero;
                // if (i + 1 < branchNodes.Count) {
                //     var h = branchNodes[i + 1].getPosition() - branchNodes[i].getPosition();
                //     var d = h.magnitude;
                //     fw = h / d;
                // } else {
                //     Gizmos.color = Color.yellow;
                //     var h = branchNodes[i].getPosition() - branchNodes[i - 1].getPosition();
                //     var d = h.magnitude;
                //     fw = h / d;
                // }

                var fw = Vector3.zero;
                if (i > 0) {
                    fw = branchNodes[i - 1].getPosition() - branchNodes[i].getPosition();
                }

                if (i < branchNodes.Count - 1) {
                    fw += branchNodes[i].getPosition() - branchNodes[i + 1].getPosition();
                }

                // fw.z = 0;
                fw.Normalize();

                var up = branchNodes[i].getNormal();
                up.Normalize();

                Vector3.OrthoNormalize(ref up, ref fw);

                float vStep = (2f * Mathf.PI) / meshFaces;
                for (int v = 0; v < meshFaces; v++) {

                    Gizmos.DrawLine(branchNodes[i].getPosition(), branchNodes[i].getPosition() + fw * .05f);

                    var orientation = Quaternion.LookRotation(fw, up);
                    Vector3 xAxis = Vector3.up;
                    Vector3 yAxis = Vector3.right;
                    Vector3 pos = branchNodes[i].getPosition();
                    pos += orientation * xAxis * (branchRadius * Mathf.Sin(v * vStep));
                    pos += orientation * yAxis * (branchRadius * Mathf.Cos(v * vStep));


                    // Vector3 pos = branchNodes[i].getPosition() + orientation * new Vector3(branchRadius * Mathf.Sin(v * vStep), branchRadius * Mathf.Cos(v * vStep), 0);

                    Gizmos.color = new Color(
                        (float)v / meshFaces,
                        (float)v / meshFaces,
                        1f
                    );
                    Gizmos.DrawSphere(pos, .002f);
                }
                // }
            }
        }

    }
}