using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// combine same material child to parent and don't show child
/// child must be static
/// every parent 
/// </summary>
public class CombineMeshGameObject : MonoBehaviour
{
    void Start()
    {
        Combine();
    }
    
    [ContextMenu("Combine Now")]
    public void Combine() {
        CombineInstance[] combine = new CombineInstance[transform.childCount];
        int i = 0;
        
        while (i < transform.childCount) {
            combine[i].mesh = transform.GetChild(i).GetComponent<MeshFilter>().sharedMesh;
            combine[i].transform = transform.GetChild(i).localToWorldMatrix;
            i++;
        }
        Mesh mesh = new Mesh();
        mesh.CombineMeshes(combine);
        //mesh.vertexCount must less than 65535
        transform.GetComponent<MeshFilter>().sharedMesh = mesh;
        //todo change name to your gameobject name
        if (gameObject.name.Contains("some name")) {
            //todo change some mat to your materials
            //transform.GetComponent<MeshRenderer>().material = some material;
        }

        i = 0;
        while (i < transform.childCount) {
            Destroy(transform.GetChild(i).GetComponent<MeshRenderer>());
            Destroy(transform.GetChild(i).GetComponent<MeshFilter>());
            i++;
        }

        Destroy(this,0.1f);
    }
}
