using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldMeshManager : MonoBehaviour
{
    public static List<CombineInstance> combine=new  List<CombineInstance>();
    public static Mesh allChunkMeshes;
    public static MeshFilter mf;
    // Start is called before the first frame update
    void Start()
    {
        allChunkMeshes=new Mesh();
        mf=GetComponent<MeshFilter>();
    }

    // Update is called once per frame
    public static void OnAllChunkMeshesChanged()
    {
        allChunkMeshes.CombineMeshes(combine.ToArray(),true);
        mf.mesh=allChunkMeshes;
    }
}
