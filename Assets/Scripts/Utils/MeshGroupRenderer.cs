using System.Collections.Generic;
using UnityEngine;

public class MeshGroupRenderer : MonoBehaviour {
    const string COLOR = "_Color";
    const string COLOREND = "_ColorEnd";

    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    MeshGroup meshGroup;
    Material lastAddedMaterial;

    // public bool validate(string materialName, Color color, Color colorEnd){
    //     bool result = true;
    //     if(meshGroup != null){
    //         result = 
    //         (materialName == meshGroup.materialName) && 
    //         (color == meshGroup.color) && 
    //         (colorEnd == meshGroup.colorEnd);
    //     }
    //     return result;
    // }

    public bool add(Transform t, Mesh mesh, Material material) {
        Color color = material.GetColor(COLOR);
        Color colorEnd = material.GetColor(COLOREND);
        // bool isValid = validate(material.name,color, colorEnd);
        // if(isValid){
        if (meshGroup == null) {
            meshGroup = new MeshGroup(material.name, color, colorEnd);
        }
        meshGroup.meshes.Add(mesh);
        meshGroup.transforms.Add(t);
        lastAddedMaterial = material;
        // }
        return true;
    }

    public void combineAndRender() {
        if (meshGroup != null) {
            Mesh mesh = combineMeshes(meshGroup);
            meshFilter.mesh = mesh;
            meshRenderer.material = lastAddedMaterial;
        }
    }

    Mesh combineMeshes(MeshGroup group) {
        var combine = new CombineInstance[group.meshes.Count];
        for (int i = 0; i < group.meshes.Count; i++) {
            combine[i].mesh = group.meshes[i];
            combine[i].transform = group.transforms[i].localToWorldMatrix;
        }

        var mesh = new Mesh { indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
        mesh.CombineMeshes(combine, true);
        mesh.Optimize();

        for (int i = 0; i < group.meshes.Count; i++) {
            if (group.transforms[i] != null) {
                Destroy(group.transforms[i].gameObject);
            }
        }

        return mesh;
    }
}