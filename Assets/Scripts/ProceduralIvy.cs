using System.Collections.Generic;
using UnityEngine;

public class ProceduralIvy : MonoBehaviour {

    public Camera cam;
    [Space]
    public int branches = 3;
    public int maxPointsForBranch = 20;
    public float segmentLength = .002f;
    public float branchRadius = 0.02f;
    [Space]
    public Material branchMaterial;

    int ivyCount = 0;

    // List<DebugNode> debugPos;


    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100)) {
                createIvy(hit);
            }
        }
    }

    Vector3 findTangentFromArbitraryNormal(Vector3 normal) {
        Vector3 t1 = Vector3.Cross(normal, Vector3.forward);
        Vector3 t2 = Vector3.Cross(normal, Vector3.up);
        if (t1.magnitude > t2.magnitude) {
            return t1;
        }
        return t2;
    }

    void createIvy(RaycastHit hit) {
        // branches = 1;
        // debugPos = new List<DebugNode>();
        // Debug.DrawRay(hit.point, hit.normal, Color.blue, 10);
        Vector3 tangent = findTangentFromArbitraryNormal(hit.normal);
        // Debug.DrawRay(hit.point, tangent, Color.blue, 10);
        GameObject ivy = new GameObject("Ivy " + ivyCount);
        for (int i = 0; i < branches; i++) {
            Vector3 dir = Quaternion.AngleAxis(360 / branches * i + Random.Range(0, 360 / branches), hit.normal) * tangent;
            // Random.Range(360 / branches, 360) 
            // Vector3 dir = tangent;
            // Debug.DrawRay(hit.point, dir, Color.yellow, 10);

            List<IvyNode> nodes = createBranch(maxPointsForBranch, hit.point, hit.point, hit.normal, dir);
            GameObject branch = new GameObject("Branch " + i);

            // DebugBranch debug = branch.AddComponent<DebugBranch>();
            // debug.nodes = nodes;

            Branch b = branch.AddComponent<Branch>();
            b.init(nodes, branchRadius, branchMaterial);

            branch.transform.SetParent(ivy.transform);
        }

        ivyCount++;
    }

    Vector3 calculateTangent(Vector3 p0, Vector3 p1, Vector3 normal) {
        var heading = p1 - p0;
        var distance = heading.magnitude;
        var direction = heading / distance;
        return Vector3.Cross(normal, direction).normalized;
    }

    Vector3 applyCorrection(Vector3 p, Vector3 normal) {
        return p + normal * 0.01f;
    }

    bool isOccluded(Vector3 from, Vector3 to) {
        Ray ray = new Ray(from, (to - from) / (to - from).magnitude);
        if (Physics.Raycast(ray, (to - from).magnitude)) {
            return true;
        }
        return false;
    }

    bool isOccluded(Vector3 from, Vector3 to, Vector3 normal) {
        return isOccluded(applyCorrection(from, normal), applyCorrection(to, normal));
    }

    Vector3 calculateMiddlePoint(Vector3 p0, Vector3 p1, Vector3 normal) {
        Vector3 middle = (p0 + p1) / 2;
        var h = p0 - p1;
        var distance = h.magnitude;
        var dir = h / distance;
        return middle + normal * distance;
    }

    List<IvyNode> createBranch(int count, Vector3 previousPos, Vector3 pos, Vector3 normal, Vector3 dir) {
        if (count == maxPointsForBranch) {
            IvyNode rootNode = new IvyNode(pos, normal, calculateTangent(pos, previousPos, normal), Constants.root);
            return new List<IvyNode> { rootNode }.join(createBranch(count - 1, previousPos, pos, normal, dir));
        } else if (count < maxPointsForBranch && count > 0) {

            if (count % 2 == 0) {
                // dir += UnityEngine.Random.insideUnitSphere.normalized;
                // dir = dir.normalized;
                // Debug.DrawRay(pos, dir, Color.yellow, 10);
                // Debug.DrawRay(pos, normal, Color.blue, 10);
                dir = Quaternion.AngleAxis(Random.Range(-60.0f, 60.0f), normal) * dir;
                // Debug.DrawRay(pos, dir, Color.red, 10);
            }

            RaycastHit hit;
            Ray ray = new Ray(pos, normal);
            // debugPos.Add(new DebugNode(pos, Color.blue, "pos"));

            // Debug.DrawRay(pos, dir, Color.yellow, 10);
            // Debug.DrawRay(pos, normal, Color.blue, 10);
            // Debug.DrawRay(pos, Vector3.up, Color.green, 10);
            Vector3 p1 = pos + normal * segmentLength;

            if (Physics.Raycast(ray, out hit, segmentLength)) {
                p1 = hit.point;
                // debugPos.Add(new DebugNode(p1, Color.green, "p1"));
                //special case
                // IvyNode p1Node = new IvyNode(p1, -normal, calculateTangent(p1, pos, -normal), Constants.p1);
                // return new List<IvyNode> { p1Node }.join(createBranch(count - 1, pos, p1, -normal, dir));

            }
            ray = new Ray(p1, dir);
            // debugPos.Add(new DebugNode(p1, Color.red, "p1"));

            if (Physics.Raycast(ray, out hit, segmentLength)) {
                Vector3 p2 = hit.point;

                // debugPos.Add(new DebugNode(p2, Color.green, "p2"));

                IvyNode p2Node = new IvyNode(p2, -dir, calculateTangent(p2, pos, -dir), Constants.p2);
                // if (isOccluded(p2, pos)) {
                //     Vector3 middle = calculateMiddlePoint(p2, pos, (normal - dir) / 2);
                //     Vector3 m0 = (pos + middle) / 2;
                //     Vector3 m1 = (p2 + middle) / 2;
                //     // debugPos.Add(new DebugNode(middle, Color.red, "mid"));
                //     // IvyNode midNode = new IvyNode(middle, normal, calculateTangent(middle, pos, -dir), true);
                //     IvyNode m0Node = new IvyNode(m0, normal, calculateTangent(m0, pos, -dir), true);
                //     IvyNode m1Node = new IvyNode(m1, normal, calculateTangent(m1, m0, -dir), true);
                //     return new List<IvyNode> { m0Node, m1Node, p2Node }.join(createBranch(count - 1, m1, p2, -dir, normal));
                // }

                return new List<IvyNode> { p2Node }.join(createBranch(count - 1, pos, p2, -dir, normal));
            } else {
                Vector3 p2 = p1 + dir * segmentLength;
                // debugPos.Add(new DebugNode(p2, Color.red, "p2"));

                ray = new Ray(applyCorrection(p2, normal), -normal);
                if (Physics.Raycast(ray, out hit, segmentLength)) {
                    Vector3 p3 = hit.point;
                    // debugPos.Add(new DebugNode(p3, Color.green, "p3"));
                    IvyNode p3Node = new IvyNode(p3, normal, calculateTangent(p3, pos, normal), Constants.p3);
                    if (isOccluded(p3, pos, normal)) {
                        Vector3 middle = calculateMiddlePoint(p3, pos, (normal + dir) / 2);
                        // debugPos.Add(new DebugNode(middle, Color.red, "mid"));
                        Vector3 m0 = (pos + middle) / 2;
                        Vector3 m1 = (p3 + middle) / 2;

                        // IvyNode midNode = new IvyNode(middle, normal, calculateTangent(middle, pos, normal), true);
                        IvyNode m0Node = new IvyNode(m0, normal, calculateTangent(m0, pos, normal), Constants.m0);
                        IvyNode m1Node = new IvyNode(m1, normal, calculateTangent(m1, m0, normal), Constants.m1);
                        return new List<IvyNode> { m0Node, m1Node, p3Node }.join(createBranch(count - 3, m1, p3, normal, dir));
                    }

                    return new List<IvyNode> { p3Node }.join(createBranch(count - 1, pos, p3, normal, dir));
                } else {
                    Vector3 p3 = p2 - normal * segmentLength;
                    // debugPos.Add(new DebugNode(p3, Color.red, "p3"));

                    ray = new Ray(applyCorrection(p3, normal), -normal);
                    if (Physics.Raycast(ray, out hit, segmentLength)) {
                        Vector3 p4 = hit.point;
                        // debugPos.Add(new DebugNode(p4, Color.green, "p4"));
                        IvyNode p4Node = new IvyNode(p4, normal, calculateTangent(p4, pos, normal), Constants.p4);

                        if (isOccluded(p4, pos, normal)) {
                            Vector3 middle = calculateMiddlePoint(p4, pos, (normal + dir) / 2);
                            // debugPos.Add(new DebugNode(middle, Color.red, "mid"));
                            Vector3 m0 = (pos + middle) / 2;
                            Vector3 m1 = (p4 + middle) / 2;

                            IvyNode m0Node = new IvyNode(m0, normal, calculateTangent(m0, pos, normal), Constants.m0);
                            IvyNode m1Node = new IvyNode(m1, normal, calculateTangent(m1, m0, normal), Constants.m1);

                            // IvyNode midNode = new IvyNode(middle, normal, calculateTangent(middle, pos, normal), true);
                            return new List<IvyNode> { m0Node, m1Node, p4Node }.join(createBranch(count - 3, m1, p4, normal, dir));
                        }

                        return new List<IvyNode> { p4Node }.join(createBranch(count - 1, pos, p4, normal, dir));
                    } else {
                        Vector3 p4 = p3 - normal * segmentLength;
                        // debugPos.Add(new DebugNode(p4, Color.red, "p4"));

                        IvyNode p4Node = new IvyNode(p4, dir, calculateTangent(p4, pos, dir), Constants.p4);

                        if (isOccluded(p4, pos, normal)) {
                            Vector3 middle = calculateMiddlePoint(p4, pos, (normal + dir) / 2);
                            // debugPos.Add(new DebugNode(middle, Color.red, "mid"));
                            Vector3 m0 = (pos + middle) / 2;
                            Vector3 m1 = (p4 + middle) / 2;

                            IvyNode m0Node = new IvyNode(m0, dir, calculateTangent(m0, pos, dir), Constants.m0);
                            IvyNode m1Node = new IvyNode(m1, dir, calculateTangent(m1, m0, dir), Constants.m1);

                            // IvyNode midNode = new IvyNode(middle, normal, calculateTangent(middle, pos, dir), true);
                            return new List<IvyNode> { m0Node, m1Node, p4Node }.join(createBranch(count - 3, m1, p4, dir, -normal));
                        }
                        return new List<IvyNode> { p4Node }.join(createBranch(count - 1, pos, p4, dir, -normal));
                    }
                }
            }


        }
        return null;
    }


    // void OnDrawGizmos() {
    //     // Gizmos.color = Color.blue;

    //     if (debugPos != null) {
    //         for (int i = 0; i < debugPos.Count; i++) {
    //             if (i > 0) {
    //                 // Gizmos.color = Color.white;
    //                 // Gizmos.DrawLine(debugPos[i - 1].position, debugPos[i].position);
    //             }
    //             Gizmos.color = debugPos[i].color;
    //             if (debugPos[i].label == "pos") {
    //                 Gizmos.DrawWireSphere(debugPos[i].position, .01f);
    //             } else {
    //                 Gizmos.DrawSphere(debugPos[i].position, .01f);
    //             }
    //         }
    //     }
    // }
}