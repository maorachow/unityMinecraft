using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;
public class ItemOnHandBeh : MonoBehaviour
{
    public Mesh itemMesh;
    public Transform t;
    public bool isHandItemBuildCompleted=false;
    public static int textureXSize{get{return ItemEntityBeh.textureXSize;}}
    public static int textureYSize{get{return ItemEntityBeh.textureYSize;}}
    public static Dictionary<int,Vector2> itemMaterialInfo{get{return ItemEntityBeh.itemMaterialInfo;}}
    public static Dictionary<int,Vector2Int> itemTexturePosInfo{get{return ItemEntityBeh.itemTexturePosInfo;}}
    public static Texture2D itemTextureInfo{get{return ItemEntityBeh.itemTextureInfo;}}
    public MeshFilter mf;
      List<Vector3> verts=new List<Vector3>();
    List<Vector2> uvs=new List<Vector2>();
    List<int> tris=new List<int>();
    public int blockID;
    public int prevBlockID;
    void Start()
    {
        mf=GetComponent<MeshFilter>();
        InvokeRepeating("InvokeBuildItem",0f,0.5f);
        t=transform;
    }
    void InvokeBuildItem(){
        if(blockID!=prevBlockID){
            OnBlockIDChanged(blockID);
            prevBlockID=blockID;
        }
   //     OnBlockIDChanged(blockID);
    }
    // Update is called once per frame
    void Update()
    {
  //  if(isHandItemBuildCompleted==false){
   //     OnBlockIDChanged(blockID);
    //    isHandItemBuildCompleted=true;
  //  }
    }
    public void OnBlockIDChanged(int blockID){
        BuildItemModel(blockID);
    }
    public void BuildItemModel(int itemID){
       
        t.localPosition=new Vector3(0f,0f,0.1f);
        t.localEulerAngles=new Vector3(45f,45f,45f);
    itemMesh=new Mesh();
    float x=-0.5f;
    float y=-0.5f;
    float z=-0.5f;
    verts=new List<Vector3>();
    uvs=new List<Vector2>();
    tris=new List<int>();
    if(itemID>150&&itemID<=200){
        BuildFlatItemModel(itemID);
    }
    if(itemID==0){

        mf.mesh=itemMesh;
        return;
    }
    if(itemID>0&&itemID<100){
        BuildFace(itemID, new Vector3(x, y, z)*1.2f, Vector3.up*1.2f, Vector3.forward*1.2f, false, verts, uvs, tris,0);
        //Right
   
         BuildFace(itemID, new Vector3(x + 1, y, z)*1.2f, Vector3.up*1.2f, Vector3.forward*1.2f, true, verts, uvs, tris,1);

        //Bottom
   
         BuildFace(itemID, new Vector3(x, y, z)*1.2f, Vector3.forward*1.2f, Vector3.right*1.2f, false, verts, uvs, tris,2);
        //Top
  
        BuildFace(itemID, new Vector3(x, y + 1, z)*1.2f, Vector3.forward*1.2f, Vector3.right*1.2f, true, verts, uvs, tris,3);

        //Back
     
        BuildFace(itemID, new Vector3(x, y, z)*1.2f, Vector3.up*1.2f, Vector3.right*1.2f, true, verts, uvs, tris,4);
        //Front
       
        BuildFace(itemID, new Vector3(x, y, z + 1)*1.2f, Vector3.up*1.2f, Vector3.right*1.2f, false, verts, uvs, tris,5); 
    }else if(itemID==100){
        BuildFace(itemID, new Vector3(x, y, z)*1.2f, Vector3.up*1.2f, Vector3.forward*1.2f, false, verts, uvs, tris,0);
        //Right
   
         BuildFace(itemID, new Vector3(x + 1, y, z)*1.2f, Vector3.up*1.2f, Vector3.forward*1.2f, true, verts, uvs, tris,1);

        //Bottom
   
         BuildFace(itemID, new Vector3(x, y, z)*1.2f, Vector3.forward*1.2f, Vector3.right*1.2f, false, verts, uvs, tris,2);
        //Top
  
        BuildFace(itemID, new Vector3(x, y + 1, z)*1.2f, Vector3.forward*1.2f, Vector3.right*1.2f, true, verts, uvs, tris,3);

        //Back
     
        BuildFace(itemID, new Vector3(x, y, z)*1.2f, Vector3.up*1.2f, Vector3.right*1.2f, true, verts, uvs, tris,4);
        //Front
       
        BuildFace(itemID, new Vector3(x, y, z + 1)*1.2f, Vector3.up*1.2f, Vector3.right*1.2f, false, verts, uvs, tris,5); 
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


 public void BuildFlatItemModel(int itemID)
    {
        t.localPosition=new Vector3(-0.01f,-0.2f,-0.25f);
        t.localEulerAngles=new Vector3(-70f,0f,-90f);
    float x=0f;
    float y=0f;
    float z=0f;
     itemMesh=new Mesh();

        BuildFlatItemFace(itemMaterialInfo[itemID].x,itemMaterialInfo[itemID].y,0.0625f, new Vector3(x, y, z)/4, Vector3.forward*textureXSize/4/4, Vector3.right*textureYSize/4/4, false, verts, uvs, tris);
        BuildFlatItemFace(itemMaterialInfo[itemID].x,itemMaterialInfo[itemID].y,0.0625f, new Vector3(x, y+1f, z)/4, Vector3.forward*textureXSize/4/4, Vector3.right*textureYSize/4/4, true, verts, uvs, tris);
        for(int i=0;i<textureXSize;i++){
            for(int j=0;j<textureYSize;j++){
                if(i+1<textureXSize&&i-1>=0&&j+1<textureYSize&&j-1>=0){
                    if(itemTextureInfo.GetPixel(itemTexturePosInfo[itemID].x+i,itemTexturePosInfo[itemID].y+j).a!=0f&&itemTextureInfo.GetPixel(itemTexturePosInfo[itemID].x+i+1,itemTexturePosInfo[itemID].y+j).a==0f){
                        //right
                        BuildFlatItemFace(itemMaterialInfo[itemID].x+(float)i/textureXSize*0.0625f+Random.Range(-0.00001f,0.00001f),itemMaterialInfo[itemID].y+(float)j/textureYSize*0.0625f+Random.Range(-0.00001f,0.00001f), (float)0.0625f*0.0625f*0.25f,new Vector3(x+i + 1, y, z+j)/4/4, Vector3.up/4, Vector3.forward/4/4, true, verts, uvs, tris);

                    }
                    if(itemTextureInfo.GetPixel(itemTexturePosInfo[itemID].x+i,itemTexturePosInfo[itemID].y+j).a!=0f&&itemTextureInfo.GetPixel(itemTexturePosInfo[itemID].x+i-1,itemTexturePosInfo[itemID].y+j).a==0f){
                        //left
                        BuildFlatItemFace(itemMaterialInfo[itemID].x+(float)i/textureXSize*0.0625f+Random.Range(-0.00001f,0.00001f),itemMaterialInfo[itemID].y+(float)j/textureYSize*0.0625f+Random.Range(-0.00001f,0.00001f), (float)0.0625f*0.0625f*0.25f,new Vector3(x+i, y, z+j)/4/4, Vector3.up/4, Vector3.forward/4/4, false, verts, uvs, tris);

                    }
                    if(itemTextureInfo.GetPixel(itemTexturePosInfo[itemID].x+i,itemTexturePosInfo[itemID].y+j).a!=0f&&itemTextureInfo.GetPixel(itemTexturePosInfo[itemID].x+i,itemTexturePosInfo[itemID].y+j+1).a==0f){
                        //front
                        BuildFlatItemFace(itemMaterialInfo[itemID].x+(float)i/textureXSize*0.0625f+Random.Range(-0.00001f,0.00010f),itemMaterialInfo[itemID].y+(float)j/textureYSize*0.0625f+Random.Range(-0.00001f,0.00001f), (float)0.0625f*0.0625f*0.25f,new Vector3(x+i, y, z+j + 1)/4/4, Vector3.up/4, Vector3.right/4/4, false, verts, uvs, tris);

                    }
                    if(itemTextureInfo.GetPixel(itemTexturePosInfo[itemID].x+i,itemTexturePosInfo[itemID].y+j).a!=0f&&itemTextureInfo.GetPixel(itemTexturePosInfo[itemID].x+i,itemTexturePosInfo[itemID].y+j-1).a==0f){
                        //back
                        BuildFlatItemFace(itemMaterialInfo[itemID].x+(float)i/textureXSize*0.0625f+Random.Range(-0.00001f,0.00001f),itemMaterialInfo[itemID].y+(float)j/textureYSize*0.0625f+Random.Range(-0.00001f,0.00001f),  (float)0.0625f*0.0625f*0.25f,new Vector3(x+i,y, z+j)/4/4, Vector3.up/4, Vector3.right/4/4, true, verts, uvs, tris);

                    }
                }else{
                    if(i+1>=textureXSize){
                          BuildFlatItemFace(itemMaterialInfo[itemID].x+(float)i/textureXSize*0.0625f+Random.Range(-0.00001f,0.00001f),itemMaterialInfo[itemID].y+(float)j/textureYSize*0.0625f+Random.Range(-0.00001f,0.00001f), (float)1/16*0.0625f,new Vector3(x+i + 1, y,z+ j)/4/4, Vector3.up/4, Vector3.forward/4/4, true, verts, uvs, tris);
                    }
                    if(i-1<0){
                          BuildFlatItemFace(itemMaterialInfo[itemID].x+(float)i/textureXSize*0.0625f+Random.Range(-0.00001f,0.00001f),itemMaterialInfo[itemID].y+(float)j/textureYSize*0.0625f+Random.Range(-0.00001f,0.00001f), (float)1/16*0.0625f,new Vector3(x+i, y, z+j)/4/4, Vector3.up/4, Vector3.forward/4/4, false, verts, uvs, tris);
                    }
                    if(j+1>=textureYSize){
                          BuildFlatItemFace(itemMaterialInfo[itemID].x+(float)i/textureXSize*0.0625f+Random.Range(-0.00001f,0.00001f),itemMaterialInfo[itemID].y+(float)j/textureYSize*0.0625f+Random.Range(-0.00001f,0.00001f), (float)1/16*0.0625f,new Vector3(x+i, y, z+j + 1)/4/4, Vector3.up/4, Vector3.right/4/4, false, verts, uvs, tris);
                    }
                    if(j-1<0){
                         BuildFlatItemFace(itemMaterialInfo[itemID].x+(float)i/textureXSize*0.0625f+Random.Range(-0.00001f,0.00001f),itemMaterialInfo[itemID].y+(float)j/textureYSize*0.0625f+Random.Range(-0.00001f,0.00001f),  (float)1/16*0.0625f,new Vector3(x+i, y, z+j)/4/4, Vector3.up/4, Vector3.right/4/4, true, verts, uvs, tris);
                    }
                   
                }
                    
            }
        }
        itemMesh.vertices=verts.ToArray();
        itemMesh.uv=uvs.ToArray();
        itemMesh.triangles=tris.ToArray();
        itemMesh.RecalculateBounds();
        itemMesh.RecalculateNormals();
        mf.mesh=itemMesh;
      //  mc.sharedMesh=itemMesh;
    }
    void BuildFlatItemFace(float uvX,float uvY,float uvWidthXY,Vector3 corner, Vector3 up, Vector3 right, bool reversed, List<Vector3> verts, List<Vector2> uvs, List<int> tris){
        Vector2 uvCorner=new Vector2(uvX,uvY);
     
        int index = verts.Count;
    
        verts.Add (corner);
        verts.Add (corner + up);
        verts.Add (corner + up + right);
        verts.Add (corner + right);


        
        Vector2 uvWidth = new Vector2(uvWidthXY,uvWidthXY);
     

        //uvCorner.x = (float)(typeid - 1) / 16;

        
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
