using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// add this component to parent gameobject,
/// combine same material child to parent and don't show child
/// child must be static
/// after combine every parent.mesh.vertexCount must less than 65535
/// </summary>
public class CombineMeshGameObject : MonoBehaviour
{
    void Start()
    {
        Combine();
    }
    
    [ContextMenu("Combine Now")]
    public void Combine() {
        Material matCombined=null;
        CombineInstance[] combine = new CombineInstance[transform.childCount];
        int i = 0;
        
        while (i < transform.childCount) {
            combine[i].mesh = transform.GetChild(i).GetComponent<MeshFilter>().sharedMesh;
            combine[i].transform = transform.GetChild(i).localToWorldMatrix;
            if (matCombined == null) {
                matCombined = transform.GetChild(i).GetComponent<MeshRenderer>().material;
            }
            i++;
        }
        Mesh mesh = new Mesh();
        mesh.CombineMeshes(combine);
        transform.GetComponent<MeshFilter>().sharedMesh = mesh;
        transform.GetComponent<MeshRenderer>().material = matCombined;

        i = 0;
        while (i < transform.childCount) {
            Destroy(transform.GetChild(i).GetComponent<MeshRenderer>());
            Destroy(transform.GetChild(i).GetComponent<MeshFilter>());
            i++;
        }

        Destroy(this,0.1f);
    }
}
