using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pipe : MonoBehaviour {

    public float branchRadius = 0.02f;
    public int meshFaces = 8;

    List<Vector3> nodes;

    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    Material material;
    Mesh mesh;

    Quaternion fromToRotation(Vector3 aFrom, Vector3 aTo, Vector3 normal) {
        Vector3 axis = Vector3.Cross(aFrom, aTo);
        float angle = Vector3.SignedAngle(aFrom, aTo, normal);
        return Quaternion.AngleAxis(angle, axis.normalized);
    }

    Quaternion angleAxis(float aAngle, Vector3 aAxis) {
        aAxis.Normalize();
        float rad = aAngle * Mathf.Deg2Rad * 0.5f;
        aAxis *= Mathf.Sin(rad);
        return new Quaternion(aAxis.x, aAxis.y, aAxis.z, Mathf.Cos(rad));
    }

    Vector3 getPointOnCircle(Vector3 center, float radius, float angle) {
        Vector3 p;
        p.x = radius * Mathf.Sin(angle);
        p.y = radius * Mathf.Cos(angle);
        p.z = 0;
        return p + center;
    }

    Vector3 getPointOnCircle(Vector3 center, float radius, float angle, float sign) {
        Vector3 p;
        p.x = sign * radius * Mathf.Sin(angle);
        p.y = radius * Mathf.Cos(angle);
        p.z = 0;
        return p + center;
    }

    Vector3 rotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles) {
        var dir = point - pivot; // get point direction relative to pivot
        dir = Quaternion.Euler(angles) * dir; // rotate it
        point = dir + pivot; // calculate rotated point
        return point; // return it
    }
    Vector3 rotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion rot) {
        var dir = point - pivot; // get point direction relative to pivot
        dir = rot * dir; // rotate it
        point = dir + pivot; // calculate rotated point
        return point; // return it
    }

    float remap(float input, float oldLow, float oldHigh, float newLow, float newHigh) {
        float t = Mathf.InverseLerp(oldLow, oldHigh, input);
        return Mathf.Lerp(newLow, newHigh, t);
    }


    void Start() {
        nodes = new List<Vector3>();
        foreach (Transform t in transform) {
            nodes.Add(t.localPosition);
        }

        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        material = new Material(Shader.Find("Specular"));

        meshRenderer.material = material;
        mesh = createMesh(nodes);
        meshFilter.mesh = mesh;

    }

    private void Update() {
        nodes = new List<Vector3>();
        foreach (Transform t in transform) {
            nodes.Add(t.localPosition);
        }

        mesh = createMesh(nodes);
        meshFilter.mesh = mesh;
    }



    Mesh createMesh(List<Vector3> poses) {
        Mesh branchMesh = new Mesh();

        Vector3[] vertices = new Vector3[(nodes.Count) * meshFaces * 4];
        Vector3[] normals = new Vector3[nodes.Count * meshFaces * 4];
        Vector2[] uv = new Vector2[nodes.Count * meshFaces * 4];
        int[] triangles = new int[(nodes.Count - 1) * meshFaces * 6];

        for (int i = 0; i < nodes.Count; i++) {
            float vStep = (2f * Mathf.PI) / meshFaces;

            var fw = Vector3.zero;
            if (i + 1 < poses.Count) {
                var h = poses[i + 1] - poses[i];
                var d = h.magnitude;
                fw = h / d;
            } else {
                var h = poses[i] - poses[i - 1];
                var d = h.magnitude;
                fw = h / d;
            }

            // Vector3 fw = Vector3.forward;
            // if (i > 0) {
            //     fw = poses[i] - poses[i - 1];
            // }

            // if (i < poses.Count - 2) {
            //     fw += poses[i + 1] - poses[i];
            // }
            // fw.Normalize();

            for (int v = 0; v < meshFaces; v++) {

                var orientation = Quaternion.LookRotation(fw, transform.GetChild(i).up);
                float sign = Mathf.Sign(Vector3.Dot(Vector3.forward, fw));
                // var orientation = Quaternion.LookRotation(fw, fw);
                // Vector3 pos = poses[i] + orientation * new Vector3(branchRadius * Mathf.Sin(v * vStep), branchRadius * Mathf.Cos(v * vStep), 0);

                Vector3 xAxis = Vector3.up;
                Vector3 yAxis = Vector3.right;
                if (sign < 0) {
                    yAxis = Vector3.left;
                }

                Vector3 pos = poses[i];
                pos += orientation * xAxis * (branchRadius * Mathf.Sin(v * vStep));
                pos += orientation * yAxis * (branchRadius * Mathf.Cos(v * vStep));

                var diff = pos - poses[i];

                if (i == 0 || i + 1 >= nodes.Count) {
                    pos = poses[i];
                }
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

        for (int i = 0; i < transform.childCount; i++) {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(transform.GetChild(i).position, .002f);

            if (i > 0) {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.GetChild(i - 1).position, transform.GetChild(i).position);
            }

            var fw = Vector3.zero;
            if (i + 1 < transform.childCount) {
                var h = transform.GetChild(i + 1).position - transform.GetChild(i).position;
                var d = h.magnitude;
                fw = h / d;
            } else {
                var h = transform.GetChild(i).position - transform.GetChild(i - 1).position;
                var d = h.magnitude;
                fw = h / d;
            }

            // var fw = Vector3.forward;
            // if (i > 0) {
            //     fw = transform.GetChild(i).position - transform.GetChild(i - 1).position;
            // }

            // if (i < transform.childCount - 2) {
            //     fw += transform.GetChild(i + 1).position - transform.GetChild(i).position;
            // }

            fw.Normalize();

            float vStep = (2f * Mathf.PI) / meshFaces;
            for (int v = 0; v < meshFaces; v++) {
                if (i + 1 < transform.childCount) {
                    // float a = Vector3.SignedAngle(fw, Vector3.forward, transform.GetChild(i).up);
                    // float sign = Vector3.Dot(Vector3.forward, fw);
                    // Vector3 axis = Quaternion.FromToRotation(sign * Vector3.forward, fw).eulerAngles;
                    // // Vector3 axis = fromToRotation(Vector3.forward, fw, transform.GetChild(i).up).eulerAngles;
                    // Vector3 pos = getPointOnCircle(transform.GetChild(i).position, branchRadius, v * vStep);
                    // pos = rotatePointAroundPivot(pos, transform.GetChild(i).position, axis);

                    float sign = Mathf.Sign(Vector3.Dot(Vector3.forward, fw));
                    var orientation = Quaternion.LookRotation(fw, transform.GetChild(i).up);
                    // var orientation = Quaternion.LookRotation(fw, fw);

                    Vector3 xAxis = Vector3.up;
                    Vector3 yAxis = Vector3.right;
                    if (sign < 0) {
                        yAxis = Vector3.left;
                    }

                    // Vector3 pos = transform.GetChild(i).position
                    //     + orientation * new Vector3(branchRadius * Mathf.Sin(v * vStep), branchRadius * Mathf.Cos(v * vStep), 0);
                    Vector3 pos = transform.GetChild(i).position;
                    pos += orientation * xAxis * (branchRadius * Mathf.Sin(v * vStep));
                    pos += orientation * yAxis * (branchRadius * Mathf.Cos(v * vStep));

                    // Vector3 pos = getPointOnCircle(transform.GetChild(i).position, branchRadius, v * vStep);
                    // // pos.x *= sign;
                    // pos = rotatePointAroundPivot(pos, transform.GetChild(i).position, axis);

                    Gizmos.color = new Color(
                        (float)v / meshFaces,
                        (float)v / meshFaces,
                        1f
                    );
                    Gizmos.DrawSphere(pos, .002f);
                }
            }


        }


    }
}
