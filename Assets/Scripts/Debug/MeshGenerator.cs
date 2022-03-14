using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour {

    public float branchRadius = 0.02f;
    public int meshFaces = 8;

    // List<Vector3> nodes;
    List<IvyNode> nodes;

    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    Material material;
    Mesh mesh;

    Vector3 findTangentFromArbitraryNormal(Vector3 normal) {
        Vector3 t1 = Vector3.Cross(normal, Vector3.forward);
        Vector3 t2 = Vector3.Cross(normal, Vector3.up);
        if (t1.magnitude > t2.magnitude) {
            return t1;
        }
        return t2;
    }

    float signedAngleBetween(Vector3 a, Vector3 b, Vector3 n) {
        // angle in [0,180]
        float angle = Vector3.Angle(a, b);
        float sign = Mathf.Sign(Vector3.Dot(n, Vector3.Cross(a, b)));

        // angle in [-179,180]
        float signed_angle = angle * sign;

        // angle in [0,360] (not used but included here for completeness)
        float angle360 = (signed_angle + 180) % 360;

        return angle360;
    }

    float remap(float input, float oldLow, float oldHigh, float newLow, float newHigh) {
        float t = Mathf.InverseLerp(oldLow, oldHigh, input);
        return Mathf.Lerp(newLow, newHigh, t);
    }

    List<IvyNode> takePoints() {
        var n = new List<IvyNode>();
        for (int i = 0; i < transform.childCount; i++) {

            var t = transform.GetChild(i);
            var normal = Vector3.zero;
            if (i > 0 && i < transform.childCount - 1) {
                var d0 = t.localPosition - transform.GetChild(i - 1).localPosition;
                d0.y = 0;
                var d1 = t.localPosition - transform.GetChild(i + 1).localPosition;
                d1.y = 0;
                d0.Normalize();
                d1.Normalize();
                // float sign = Vector3.Dot((d1 + d0) / 2, Vector3.forward);
                // Vector3 tmpCross = Vector3.Cross(d1, d0);
                float sign = Vector3.Dot(d1, d0);
                float angle = signedAngleBetween(d1, d0, Vector3.up);
                // if (i == 3)
                print(i + " " + sign + " " + angle);

                normal = angle > 180 ? Vector3.Cross(d1, d0) : Vector3.Cross(d0, d1);

                // if (sign > 0) {
                //     normal = angle > 180 ? Vector3.Cross(normal, Vector3.Cross(d1, normal)) : Vector3.Cross(Vector3.Cross(normal, d1), normal);

                // }

                normal.Normalize();
                // normal = Vector3.Cross(d1, d0).normalized;
                // normal = findTangentFromArbitraryNormal(d);
            } else if (i > 0) {
                var d = transform.GetChild(i - 1).localPosition - t.localPosition;
                d.Normalize();
                normal = findTangentFromArbitraryNormal(d);

                float angle = signedAngleBetween(d, normal, Vector3.up);
                float sign = Vector3.Dot(d, normal);
                // print(sign + " " + angle);

            } else {
                var d = t.localPosition - transform.GetChild(i + 1).localPosition;
                d.Normalize();
                normal = findTangentFromArbitraryNormal(d);
            }

            n.Add(new IvyNode(t.localPosition, normal));
        }
        return n;
    }

    void Start() {
        // nodes = new List<Vector3>();
        // foreach (Transform t in transform) {
        //     nodes.Add(t.localPosition);
        // }

        nodes = takePoints();

        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        material = new Material(Shader.Find("Specular"));

        meshRenderer.material = material;
        mesh = createMesh(nodes);
        meshFilter.mesh = mesh;
    }

    void Update() {
        // nodes = new List<Vector3>();
        // foreach (Transform t in transform) {
        //     nodes.Add(t.localPosition);
        // }

        nodes = takePoints();

        mesh = createMesh(nodes);
        meshFilter.mesh = mesh;
    }

    Mesh createMesh(List<IvyNode> poses) {
        Mesh branchMesh = new Mesh();

        Vector3[] vertices = new Vector3[(nodes.Count) * meshFaces * 4];
        Vector3[] normals = new Vector3[nodes.Count * meshFaces * 4];
        Vector2[] uv = new Vector2[nodes.Count * meshFaces * 4];
        int[] triangles = new int[(nodes.Count - 1) * meshFaces * 6];

        for (int i = 0; i < nodes.Count; i++) {
            float vStep = (2f * Mathf.PI) / meshFaces;

            var fw = Vector3.zero;
            if (i > 0) {
                fw = poses[i - 1].getPosition() - poses[i].getPosition();
            }

            if (i < transform.childCount - 1) {
                fw += poses[i].getPosition() - poses[i + 1].getPosition();
            }

            fw.z = 0;
            fw.Normalize();

            var up = poses[i].getNormal();
            up.Normalize();

            // Vector3.OrthoNormalize(ref up, ref fw);

            for (int v = 0; v < meshFaces; v++) {

                var orientation = Quaternion.LookRotation(fw, up);

                Vector3 xAxis = Vector3.up;
                Vector3 yAxis = Vector3.right;

                Vector3 pos = poses[i].getPosition();
                pos += orientation * xAxis * (branchRadius * Mathf.Sin(v * vStep));
                pos += orientation * yAxis * (branchRadius * Mathf.Cos(v * vStep));

                var diff = pos - poses[i].getPosition();

                // if (i == 0 || i + 1 >= nodes.Count) {
                //     pos = poses[i];
                // }

                vertices[i * meshFaces + v] = pos;

                normals[i * meshFaces + v] = diff / diff.magnitude;

                float uvID = remap(i, 0, nodes.Count - 1, 0, 1);
                uv[i * meshFaces + v] = new Vector2(uvID, v / meshFaces);
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


    void OnDrawGizmos() {

        // Gizmos.color = Color.yellow;
        // Gizmos.DrawSphere(centerOfSpline.position, .004f);

        if (nodes != null) {
            for (int i = 0; i < nodes.Count; i++) {
                if (i > 0) {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(transform.position + nodes[i - 1].getPosition(), transform.position + nodes[i].getPosition());

                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(transform.position + nodes[i].getPosition(), transform.position + nodes[i].getPosition() + nodes[i].getNormal() * .1f);
                }
            }
        } else {
            for (int i = 0; i < transform.childCount; i++) {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(transform.GetChild(i).position, .002f);

                if (i > 0) {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(transform.GetChild(i - 1).position, transform.GetChild(i).position);

                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(transform.GetChild(i).position, transform.GetChild(i).position + transform.GetChild(i).up * .1f);
                }


                // var fw = Vector3.zero;
                // if (i > 0) {
                //     fw = transform.GetChild(i - 1).position - transform.GetChild(i).position;
                // }

                // if (i < transform.childCount - 1) {
                //     fw += transform.GetChild(i).position - transform.GetChild(i + 1).position;
                // }

                // fw.Normalize();

                // var up = transform.GetChild(i).position - centerOfSpline.position;
                // up.Normalize();

                // // Vector3.OrthoNormalize(ref up, ref fw);

                // Gizmos.DrawLine(transform.GetChild(i).position, transform.GetChild(i).position + up * .1f);

                // float vStep = (2f * Mathf.PI) / meshFaces;
                // for (int v = 0; v < meshFaces; v++) {

                //     var orientation = Quaternion.LookRotation(fw, up);

                //     Vector3 xAxis = Vector3.up;
                //     Vector3 yAxis = Vector3.right;

                //     Vector3 pos = transform.GetChild(i).position;
                //     pos += orientation * xAxis * (branchRadius * Mathf.Sin(v * vStep));
                //     pos += orientation * yAxis * (branchRadius * Mathf.Cos(v * vStep));


                //     Gizmos.color = new Color(
                //         (float)v / meshFaces,
                //         (float)v / meshFaces,
                //         1f
                //     );

                //     Gizmos.DrawSphere(pos, .002f);

                // }

            }
        }

        if (mesh != null) {
            for (int i = 0; i < mesh.vertices.Length; i++) {
                Gizmos.color = new Color(
                       (float)i % meshFaces / meshFaces,
                       (float)i % meshFaces / meshFaces,
                       1f
                   );
                Gizmos.DrawSphere(transform.position + mesh.vertices[i], .002f);
            }
        }


    }
}