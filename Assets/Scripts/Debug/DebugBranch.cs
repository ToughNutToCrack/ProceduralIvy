using System.Collections.Generic;
using UnityEngine;

public class DebugBranch : MonoBehaviour {
    public List<IvyNode> nodes;

    private void OnDrawGizmos() {
        Gizmos.color = Color.blue;

        if (nodes != null) {
            for (int i = 0; i < nodes.Count; i++) {
                if (i > 0) {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(nodes[i - 1].getPosition(), nodes[i].getPosition());
                }
                // if (nodes[i].getMiddle()) {
                //     Gizmos.color = Color.red;
                // }
                Gizmos.DrawSphere(nodes[i].getPosition(), .01f);
            }
        }
    }


    // Vector3 getPointOnCircle1(Vector3 center, float radius, float angle) {
    //     Vector3 p;
    //     p.x = radius * Mathf.Sin(angle);
    //     p.y = radius * Mathf.Cos(angle);
    //     p.z = 0;
    //     return p + center;
    // }

    // Vector3 rotatePointAroundPivot1(Vector3 point, Vector3 pivot, Vector3 angles) {
    //     var dir = point - pivot; // get point direction relative to pivot
    //     dir = Quaternion.Euler(angles) * dir; // rotate it
    //     point = dir + pivot; // calculate rotated point
    //     return point; // return it
    // }

    // Vector3 rotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 fromDir, Vector3 toDir) {
    //     Vector3 dir = point - pivot;
    //     dir = Quaternion.FromToRotation(fromDir, toDir) * dir;
    //     point = dir + pivot;
    //     return point;
    // }

    // Vector3 getPointOnCircle(Vector3 center, float radius, float angle, Vector3 fw) {
    //     Vector3 p;
    //     p.x = radius * Mathf.Sin(angle);
    //     p.y = radius * Mathf.Cos(angle);
    //     p.z = 0;
    //     return rotatePointAroundPivot(center + p, center, Vector3.forward, fw);
    // }

    // // float calculateAngle(Vector3 fromDir, Vector3 toDir) {
    // //     float angle = Quaternion.FromToRotation(fromDir, toDir).eulerAngles.y;
    // //     if (angle > 180) {
    // //         return angle - 360f;
    // //     }
    // //     return angle;
    // // }

    // Vector3 rotatePointAroundAxis(Vector3 point, Vector3 pivot, float angle, Vector3 axis) {
    //     Vector3 dir = point - pivot;
    //     dir = Quaternion.AngleAxis(angle, axis) * dir;
    //     return dir + pivot;
    // }

    // Vector3 getPointOnCircle(Vector3 center, float radius, float angle, Vector3 fw, Vector3 normal) {
    //     Vector3 p;
    //     p.x = radius * Mathf.Sin(angle);
    //     p.y = radius * Mathf.Cos(angle);
    //     p.z = 0;
    //     // return rotatePointAroundPivot(rotatePointAroundPivot(center + p, center, Vector3.up, normal), center, Vector3.forward, fw);
    //     float a = Vector3.SignedAngle(fw, Vector3.forward, normal);
    //     return rotatePointAroundAxis(center + p, center, a, normal);
    //     // return rotatePointAroundPivot(rotatePointAroundPivot(center + p, center, Vector3.up, normal), center, Vector3.forward, fw);
    //     // Quaternion rotation = Quaternion.LookRotation(fw, normal);
    //     // return rotation * (center + p);
    // }
}