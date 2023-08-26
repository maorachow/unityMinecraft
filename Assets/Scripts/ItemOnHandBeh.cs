using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemOnHandBeh : MonoBehaviour
{
    public MeshFilter mf;
    public int blockID;
    void Start()
    {
        mf=GetComponent<MeshFilter>();

    }

    // Update is called once per frame
 //   void Update()
  //  {
        
  //  }
    public void OnBlockIDChanged(int blockID){
        BuildItemModel(blockID);
    }
    public void BuildItemModel(int itemID){
    Mesh itemMesh=new Mesh();
    float x=-0.5f;
    float y=-0.5f;
    float z=-0.5f;
    List<Vector3> verts=new List<Vector3>();
    List<Vector2> uvs=new List<Vector2>();
    List<int> tris=new List<int>();
    if(itemID==0){

        mf.mesh=itemMesh;
        return;
    }
    if(itemID>0&&itemID<100){
        BuildFace(itemID, new Vector3(x, y, z), Vector3.up, Vector3.forward, false, verts, uvs, tris,0);
        //Right
   
         BuildFace(itemID, new Vector3(x + 1, y, z), Vector3.up, Vector3.forward, true, verts, uvs, tris,1);

        //Bottom
   
         BuildFace(itemID, new Vector3(x, y, z), Vector3.forward, Vector3.right, false, verts, uvs, tris,2);
        //Top
  
        BuildFace(itemID, new Vector3(x, y + 1, z), Vector3.forward, Vector3.right, true, verts, uvs, tris,3);

        //Back
     
        BuildFace(itemID, new Vector3(x, y, z), Vector3.up, Vector3.right, true, verts, uvs, tris,4);
        //Front
       
        BuildFace(itemID, new Vector3(x, y, z + 1), Vector3.up, Vector3.right, false, verts, uvs, tris,5); 
    }else if(itemID==100){
        BuildFace(itemID, new Vector3(x, y, z), Vector3.up, Vector3.forward, false, verts, uvs, tris,0);
        //Right
   
         BuildFace(itemID, new Vector3(x + 1, y, z), Vector3.up, Vector3.forward, true, verts, uvs, tris,1);

        //Bottom
   
         BuildFace(itemID, new Vector3(x, y, z), Vector3.forward, Vector3.right, false, verts, uvs, tris,2);
        //Top
  
        BuildFace(itemID, new Vector3(x, y + 1, z), Vector3.forward, Vector3.right, true, verts, uvs, tris,3);

        //Back
     
        BuildFace(itemID, new Vector3(x, y, z), Vector3.up, Vector3.right, true, verts, uvs, tris,4);
        //Front
       
        BuildFace(itemID, new Vector3(x, y, z + 1), Vector3.up, Vector3.right, false, verts, uvs, tris,5); 
    }else{
    
        if(itemID>=101&&itemID<150){
            Vector3 randomCrossModelOffset=new Vector3(0f,0f,0f);
            BuildFace(itemID, new Vector3(x, y, z)+randomCrossModelOffset, new Vector3(0f,1f,0f)+randomCrossModelOffset, new Vector3(1f,0f,1f)+randomCrossModelOffset, false, verts, uvs, tris,0);
            BuildFace(itemID, new Vector3(x, y, z)+randomCrossModelOffset, new Vector3(0f,1f,0f)+randomCrossModelOffset, new Vector3(1f,0f,1f)+randomCrossModelOffset, true, verts, uvs, tris,0);
            BuildFace(itemID, new Vector3(x, y, z+1f)+randomCrossModelOffset, new Vector3(0f,1f,0f)+randomCrossModelOffset, new Vector3(1f,0f,-1f)+randomCrossModelOffset, false, verts, uvs, tris,0);
            BuildFace(itemID, new Vector3(x, y, z+1f)+randomCrossModelOffset, new Vector3(0f,1f,0f)+randomCrossModelOffset, new Vector3(1f,0f,-1f)+randomCrossModelOffset, true, verts, uvs, tris,0);
        }
    }

        itemMesh.vertices = verts.ToArray();
        itemMesh.uv = uvs.ToArray();
        itemMesh.triangles = tris.ToArray();
        itemMesh.RecalculateBounds();
        itemMesh.RecalculateNormals();
        mf.mesh=itemMesh;

}
void BuildFace(int typeid, Vector3 corner, Vector3 up, Vector3 right, bool reversed, List<Vector3> verts, List<Vector2> uvs, List<int> tris, int side){

        int index = verts.Count;
    
        verts.Add (corner);
        verts.Add (corner + up);
        verts.Add (corner + up + right);
        verts.Add (corner + right);

        Vector2 uvWidth = new Vector2(0.0625f, 0.0625f);
        Vector2 uvCorner = new Vector2(0.00f, 0.00f);

        //uvCorner.x = (float)(typeid - 1) / 16;
        uvCorner=Chunk.itemBlockInfo[typeid][side];
        uvs.Add(uvCorner);
        uvs.Add(new Vector2(uvCorner.x, uvCorner.y + uvWidth.y));
        uvs.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y + uvWidth.y));
        uvs.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y));
    
        if (reversed)
            {
            tris.Add(index + 0);
            tris.Add(index + 1);
            tris.Add(index + 2);
            tris.Add(index + 2);
            tris.Add(index + 3);
            tris.Add(index + 0);
            }
            else
            {
            tris.Add(index + 1);
            tris.Add(index + 0);
            tris.Add(index + 2);
            tris.Add(index + 3);
            tris.Add(index + 2);
            tris.Add(index + 0);
        }
    
    }
}
