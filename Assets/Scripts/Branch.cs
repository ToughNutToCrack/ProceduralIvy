using System.Collections.Generic;
using UnityEngine;

public class Branch : MonoBehaviour {
    const string AMOUNT = "_Amount";
    const string RADIUS = "_Radius";
    const float MAX = 0.5f;

    List<IvyNode> branchNodes;

    Mesh mesh;
    Material material;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;

    Material leafMaterial;
    Material flowerMaterial;
    Blossom leafPrefab;
    Blossom flowerPrefab;
    bool wantBlossoms;
    Dictionary<int, Blossom> blossoms;

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

    public void init(List<IvyNode> branchNodes, float branchRadius, Material material, Material leafMaterial, Blossom leafPrefab, Material flowerMaterial, Blossom flowerPrefab, bool isFirst) {
        this.branchNodes = branchNodes;
        this.branchRadius = branchRadius;
        this.material = new Material(material);
        mesh = createMesh(branchNodes);

        this.leafMaterial = leafMaterial;
        this.flowerMaterial = flowerMaterial;
        this.leafPrefab = leafPrefab;
        this.flowerPrefab = flowerPrefab;
        this.wantBlossoms = true;
        blossoms = createBlossoms(branchNodes, isFirst);
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

            if (wantBlossoms) {
                var estimateNodeID = (int)remap(currentAmount, -.5f, .5f, 0, branchNodes.Count - 1);

                if (blossoms.ContainsKey(estimateNodeID)) {
                    Blossom b = blossoms[estimateNodeID];
                    if (!b.isGrowing()) {
                        b.grow(growthSpeed);
                    }
                }
            }

            if (currentAmount >= MAX) {
                animate = false;
                material.SetFloat(AMOUNT, MAX);
                MeshManager.instance.addMesh(transform, meshFilter.mesh, meshRenderer.sharedMaterial);
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

            fw.Normalize();

            var up = branchNodes[i].getNormal();
            up.Normalize();

            for (int v = 0; v < meshFaces; v++) {
                var orientation = Quaternion.LookRotation(fw, up);
                Vector3 xAxis = Vector3.up;
                Vector3 yAxis = Vector3.right;
                Vector3 pos = branchNodes[i].getPosition();
                pos += orientation * xAxis * (branchRadius * Mathf.Sin(v * vStep));
                pos += orientation * yAxis * (branchRadius * Mathf.Cos(v * vStep));

                vertices[i * meshFaces + v] = pos;

                var diff = pos - branchNodes[i].getPosition();
                normals[i * meshFaces + v] = diff / diff.magnitude;

                float uvID = remap(i, 0, nodes.Count - 1, 0, 1);
                uv[i * meshFaces + v] = new Vector2((float)v / meshFaces, uvID);
            }

            if (i + 1 < nodes.Count) {
                for (int v = 0; v < meshFaces; v++) {
                    triangles[i * meshFaces * 6 + v * 6] = ((v + 1) % meshFaces) + i * meshFaces;
                    triangles[i * meshFaces * 6 + v * 6 + 1] = triangles[i * meshFaces * 6 + v * 6 + 4] = v + i * meshFaces;
                    triangles[i * meshFaces * 6 + v * 6 + 2] = triangles[i * meshFaces * 6 + v * 6 + 3] = ((v + 1) % meshFaces + meshFaces) + i * meshFaces;
                    triangles[i * meshFaces * 6 + v * 6 + 5] = (meshFaces + v % meshFaces) + i * meshFaces;
                }
            }
        }

        branchMesh.vertices = vertices;
        branchMesh.triangles = triangles;
        branchMesh.normals = normals;
        branchMesh.uv = uv;
        return branchMesh;
    }

    Dictionary<int, Blossom> createBlossoms(List<IvyNode> nodes, bool isFirst) {

        Dictionary<int, Blossom> bls = new Dictionary<int, Blossom>();
        for (int i = 0; i < nodes.Count; i++) {

            var r = Random.Range(0, 10);
            if (i > 0 || isFirst) {

                if (r > 2) {
                    Vector3 n = nodes[i].getNormal();
                    Vector3 otherNormal = Vector3.up;
                    Vector3 fw = Vector3.forward;
                    if (i > 0) {
                        fw = nodes[i - 1].getPosition() - nodes[i].getPosition();
                        otherNormal = nodes[i - 1].getNormal();
                    } else if (i < nodes.Count - 1) {
                        fw = nodes[i].getPosition() - nodes[i + 1].getPosition();
                        otherNormal = nodes[i + 1].getNormal();
                    }

                    var isFlower = (r == 3) && Vector3.Dot(n, otherNormal) >= .95f;

                    var prefab = leafPrefab;
                    if (isFlower) {
                        prefab = flowerPrefab;
                    }

                    Quaternion rotation = Quaternion.LookRotation((fw).normalized, n);
                    float flowerOffset = isFlower ? 0.02f : 0;
                    float uvID = remap(i, 0, nodes.Count - 1, 0, 1);
                    Blossom b = Instantiate(prefab, nodes[i].getPosition() + nodes[i].getNormal() * (branchRadius + flowerOffset), rotation);
                    b.init(isFlower ? flowerMaterial : leafMaterial);
                    b.transform.SetParent(transform);
                    bls.Add(i, b);
                }
            }


        }
        return bls;
    }


    void OnDrawGizmosSelected() {

        if (branchNodes != null) {
            for (int i = 0; i < branchNodes.Count; i++) {
                Gizmos.DrawSphere(branchNodes[i].getPosition(), .002f);
                Gizmos.color = Color.white;

                Gizmos.color = Color.blue;

                var fw = Vector3.zero;
                if (i > 0) {
                    fw = branchNodes[i - 1].getPosition() - branchNodes[i].getPosition();
                }

                if (i < branchNodes.Count - 1) {
                    fw += branchNodes[i].getPosition() - branchNodes[i + 1].getPosition();
                }

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