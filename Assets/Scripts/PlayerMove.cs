using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
public class PlayerMove : MonoBehaviour
{
    public static Dictionary<int,string> blockNameDic=new Dictionary<int,string>();
    public static bool isBlockNameDicAdded=false;
    public int blockOnHandID=0;
    public static bool isPaused=false;
    public GameObject prefabBlockOutline;
    public GameObject blockOutline;
    public Transform cameraPos;
    public Text blockOnHandText;
    public Camera mainCam;
    public CharacterController cc;
    public float cameraX;
    public float breakBlockCD=0.2f;
    public float moveSpeed=5f;
    public float gravity=-9.8f;
    public float playerY=0f;
    public float jumpHeight=2f;
    public float mouseSens=10f;
    public Vector3 playerVec;
    public Chunk chunkPrefab;
    public float viewRange=32;
    public GameObject pauseMenu;
    void Awake(){
        if(isBlockNameDicAdded==false){
        blockNameDic.Add(0,"None");
        blockNameDic.Add(1,"Stone");
        blockNameDic.Add(2,"Grass");
        blockNameDic.Add(3,"Dirt");
        blockNameDic.Add(4,"Side Grass Block");
        blockNameDic.Add(5,"Bedrock");
        blockNameDic.Add(6,"WoodX");
        blockNameDic.Add(7,"WoodY");
        blockNameDic.Add(8,"WoodZ");
        blockNameDic.Add(9,"Leaves");
        isBlockNameDicAdded=true;
        }

    }
    void Start()
    {   

        prefabBlockOutline=Resources.Load<GameObject>("Prefabs/blockoutline");
        blockOutline=Instantiate(prefabBlockOutline,transform.position,transform.rotation);
        pauseMenu=GameObject.Find("pausemenuUI");
        blockOnHandText=GameObject.Find("blockonhandIDtext").GetComponent<Text>();
        pauseMenu.SetActive(false);
        viewRange=64;
        cc=GetComponent<CharacterController>();
        cameraPos=transform.GetChild(0);
        mainCam=cameraPos.gameObject.GetComponent<Camera>();
        chunkPrefab=Resources.Load<Chunk>("Prefabs/chunk");
    }
    void OnApplicationFocus(bool focus)
    {
    Cursor.lockState = CursorLockMode.Locked;
    }
    void Update()
    {      
        if(Input.GetKeyDown(KeyCode.U)){
                Chunk.SaveWorldData();
        }
        Ray ray=mainCam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit info;
        if(Physics.Raycast(ray,out info,10f)){
        blockOutline.GetComponent<MeshRenderer>().enabled=true;
        Vector3 blockPoint=info.point-cameraPos.forward*0.01f;
        blockOutline.transform.position=new Vector3(Chunk.Vec3ToBlockPos(blockPoint).x+0.5f,Chunk.Vec3ToBlockPos(blockPoint).y+0.5f,Chunk.Vec3ToBlockPos(blockPoint).z+0.5f);
        }else{
            blockOutline.GetComponent<MeshRenderer>().enabled=false;
        }
           // Debug.Log(Input.GetAxis("Mouse ScrollWheel"));
            blockOnHandID+=(int)(Input.GetAxis("Mouse ScrollWheel")*15f);
            blockOnHandID=Mathf.Clamp(blockOnHandID,0,9);
            blockOnHandText.text="Block On Hand ID: "+blockOnHandID+" "+blockNameDic[blockOnHandID];
       
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            if(isPaused==false){
                PauseGame();
            }else{
                Resume();
            }
        }
        if(isPaused==true){
            return;
        }
        if(cc.isGrounded!=true){
            playerY+=gravity*Time.deltaTime;
        }else{
            playerY=0f;
        }
        if(cc.isGrounded==true&&Input.GetButton("Jump")){
            playerY=jumpHeight;
        }
        float mouseX=Input.GetAxis("Mouse X")*mouseSens;
        float mouseY=Input.GetAxis("Mouse Y")*mouseSens;

        mouseY=Mathf.Clamp(mouseY,-90f,90f);
        cameraX-=mouseY;
        cameraX=Mathf.Clamp(cameraX,-90f,90f);
        playerVec=new Vector3(Input.GetAxis("Vertical"),0f,Input.GetAxis("Horizontal"));
        playerVec.y=playerY;
        transform.eulerAngles+=new Vector3(0f,mouseX,0f);
        cameraPos.localEulerAngles=new Vector3(cameraX,0f,0f);
        cc.Move((transform.forward*playerVec.x+transform.right*playerVec.z+new Vector3(0f,playerVec.y,0f))*moveSpeed*Time.deltaTime);
        if(breakBlockCD>0f){
            breakBlockCD-=Time.deltaTime;
        }
        if(Input.GetMouseButton(0)&&breakBlockCD<=0f){
            BreakBlock();
            breakBlockCD=0.2f;
        }
        if(Input.GetMouseButton(1)&&breakBlockCD<=0f){
            PlaceBlock();
            breakBlockCD=0.2f;
        }
       
    }
    void FixedUpdate(){
         UpdateWorld();
    }

    void UpdateWorld()
    {
        for (float x = transform.position.x - viewRange; x < transform.position.x + viewRange; x += Chunk.chunkWidth)
        {
            for (float z = transform.position.z - viewRange; z < transform.position.z + viewRange; z += Chunk.chunkWidth)
            {
                Vector3 pos = new Vector3(x, 0, z);
                pos.x = Mathf.Floor(pos.x / (float)Chunk.chunkWidth) * Chunk.chunkWidth;
                pos.z = Mathf.Floor(pos.z / (float)Chunk.chunkWidth) * Chunk.chunkWidth;
                Vector2Int chunkPos=Chunk.Vec3ToChunkPos(pos);
                Chunk chunk = Chunk.GetChunk(chunkPos);
                if (chunk != null) {continue;}else{
                    chunk=ObjectPools.chunkPool.Get().GetComponent<Chunk>();
                    chunk.transform.position=new Vector3(chunkPos.x,0,chunkPos.y);
               //     chunk.isChunkPosInited=true;
                    chunk.SendMessage("ReInitData");
                }
            }
        }
     
    }


    void BreakBlock(){
        Ray ray=mainCam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit info;
        if(Physics.Raycast(ray,out info,10f)){
            Vector3 blockPoint=info.point+cameraPos.forward*0.01f;
            Chunk.SetBlock(blockPoint,0);
        }
    }

    void PlaceBlock(){
        Ray ray=mainCam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit info;
        if(Physics.Raycast(ray,out info,10f)){
            Vector3 blockPoint=info.point-cameraPos.forward*0.01f;
            if(blockOutline.GetComponent<BlockOutlineBeh>().isCollidingWithPlayer==true){
                return;
            }
            Chunk.SetBlock(blockPoint,blockOnHandID);
        }
    }
    
    void PauseGame(){
        isPaused=true;
        Time.timeScale=0;
        pauseMenu.SetActive(true);
    }
    void Resume(){
        isPaused=false;
        Time.timeScale=1;
        pauseMenu.SetActive(false);
    }
 
}
