using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blossom : MonoBehaviour {
    const string AMOUNT = "_Amount";
    const float MAX = 0.5f;

    Material material;
    List<MeshRenderer> renderers;

    bool animate;
    float growthSpeed = 2;
    float currentAmount = -1;

    public void init(Material material) {
        this.material = new Material(material);
    }

    void Start() {
        renderers = new List<MeshRenderer>();
        foreach (Transform t in transform) {
            MeshRenderer r = t.GetComponent<MeshRenderer>();
            renderers.Add(r);
            r.material = material;
        }

        material.SetFloat(AMOUNT, currentAmount);
        animate = false;
    }

    public void grow(float growthSpeed) {
        this.growthSpeed = growthSpeed;
        animate = true;
    }

    public bool isGrowing() {
        return animate || currentAmount >= MAX;
    }

    void Update() {
        if (animate) {
            currentAmount += Time.deltaTime * growthSpeed;
            material.SetFloat(AMOUNT, currentAmount);
            if (currentAmount >= MAX) {
                animate = false;
                material.SetFloat(AMOUNT, MAX);
                foreach (var r in renderers) {
                    MeshManager.instance.addMesh(r.transform, r.GetComponent<MeshFilter>().mesh, r.GetComponent<MeshRenderer>().sharedMaterial);
                }
            }
        }
    }
}
