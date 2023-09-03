using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreeperBeh : MonoBehaviour
{
    public Transform targetPosition;
    public AudioSource AS;
    public static AudioClip creeperHurtClip;
    public static GameObject diedCreeperPrefab;
    public static GameObject explosionPrefab;
    public static bool isCreeperPrefabLoaded=false;
    public bool isCreeperDied=false;
    private CharacterController cc;
    public Animator am;
    public Transform headTransform;
    public float creeperHealth=20f;
    public float creeperExplodeFuse=0f;
    public Vector3 entityVec;
    public float moveSpeed=5f;
    public float gravity=-9.8f;
    public float entityY=0f;
    public float jumpHeight=2f;
    public Vector3 entityFacingPos;
    public float nextWaypointDistance =6f;
    public bool isPosInited=false;
    Vector2 curpos;
    Vector2 lastpos;
    public bool isJumping=false;
    public Vector3 entityMotionVec;
    public float entitySpeed;
    public void Start () {
        AS=GetComponent<AudioSource>();
        creeperHealth=20f;
        isCreeperDied=false;
        if(isCreeperPrefabLoaded==false){
            creeperHurtClip=Resources.Load<AudioClip>("Audios/Creeper_say2");
                diedCreeperPrefab=Resources.Load<GameObject>("Prefabs/diedcreeper");
                explosionPrefab=Resources.Load<GameObject>("Prefabs/creeperexploeffect");
                isCreeperPrefabLoaded=true;
        }
        targetPosition=GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        headTransform=transform.GetChild(1);
        entityFacingPos=transform.rotation.eulerAngles;
    
        cc = GetComponent<CharacterController>();
        am=GetComponent<Animator>();
    
    }

     public void OnDisable(){
        entityMotionVec=Vector3.zero;
        isCreeperDied=false;
        creeperHealth=20f;
            isPosInited=false;
    }

    public void ApplyDamageAndKnockback(float damageAmount,Vector3 knockback){
        AudioSource.PlayClipAtPoint(creeperHurtClip,transform.position,1f);
        transform.GetChild(0).GetComponent<MeshRenderer>().material.color=Color.red;
         transform.GetChild(1).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.red;
          transform.GetChild(2).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.red;
           transform.GetChild(3).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.red;
           transform.GetChild(4).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.red;
           transform.GetChild(5).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.red;
        creeperHealth-=damageAmount;
        entityMotionVec=knockback;
        Invoke("InvokeRevertColor",0.2f);
    }
    void InvokeRevertColor(){
              transform.GetChild(0).GetComponent<MeshRenderer>().material.color=Color.white;
         transform.GetChild(1).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.white;
          transform.GetChild(2).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.white;
           transform.GetChild(3).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.white;
           transform.GetChild(4).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.white;
           transform.GetChild(5).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.white;
    }
    void CreeperExplode(){
        Collider[] collider = Physics.OverlapSphere(transform.position, 6f);
        foreach(Collider c in collider){
            if(c.gameObject.tag=="Player"||c.gameObject.tag=="Entity"){
                if(c.GetComponent<PlayerMove>()!=null){
                    c.GetComponent<PlayerMove>().ApplyDamageAndKnockback(10f+Random.Range(-5f,5f),(transform.position-c.transform.position).normalized*Random.Range(-20f,-30f));
                }
                if(c.GetComponent<CreeperBeh>()!=null){
                    c.GetComponent<CreeperBeh>().ApplyDamageAndKnockback(10f+Random.Range(-5f,5f),(transform.position-c.transform.position).normalized*Random.Range(-20f,-30f));
                }
                if(c.GetComponent<ZombieBeh>()!=null){
                    c.GetComponent<ZombieBeh>().ApplyDamageAndKnockback(10f+Random.Range(-5f,5f),(transform.position-c.transform.position).normalized*Random.Range(-20f,-30f));
                }
                if(c.GetComponent<ItemEntityBeh>()!=null){
                    c.GetComponent<Rigidbody>().velocity=(transform.position-c.transform.position).normalized*Random.Range(-20f,-30f);
                }
            }
        }
        GameObject a=Instantiate(explosionPrefab,new Vector3(transform.position.x,transform.position.y+0.5f,transform.position.z),transform.rotation);
        Destroy(a,2f);
           ObjectPools.creeperEntityPool.Release(gameObject);
    }
    void CreeperDie(Vector3 knockback){
         AudioSource.PlayClipAtPoint(creeperHurtClip,transform.position,1f);
        isCreeperDied=true;
          Transform diedCreeperTrans=Instantiate(diedCreeperPrefab,transform.position,transform.rotation).GetComponent<Transform>();
              diedCreeperTrans.GetChild(0).position=transform.GetChild(0).position;
         diedCreeperTrans.GetChild(1).GetChild(0).position=transform.GetChild(1).GetChild(0).position;
          diedCreeperTrans.GetChild(2).GetChild(0).position=transform.GetChild(2).GetChild(0).position;
           diedCreeperTrans.GetChild(3).GetChild(0).position=transform.GetChild(3).GetChild(0).position;
           diedCreeperTrans.GetChild(4).GetChild(0).position=transform.GetChild(4).GetChild(0).position;
           diedCreeperTrans.GetChild(5).GetChild(0).position=transform.GetChild(5).GetChild(0).position;
               diedCreeperTrans.GetChild(0).rotation=transform.GetChild(0).rotation;
         diedCreeperTrans.GetChild(1).GetChild(0).rotation=transform.GetChild(1).GetChild(0).rotation;
          diedCreeperTrans.GetChild(2).GetChild(0).rotation=transform.GetChild(2).GetChild(0).rotation;
           diedCreeperTrans.GetChild(3).GetChild(0).rotation=transform.GetChild(3).GetChild(0).rotation;
           diedCreeperTrans.GetChild(4).GetChild(0).rotation=transform.GetChild(4).GetChild(0).rotation;
           diedCreeperTrans.GetChild(5).GetChild(0).rotation=transform.GetChild(5).GetChild(0).rotation;


           diedCreeperTrans.GetChild(0).GetComponent<Rigidbody>().velocity=knockback;
            diedCreeperTrans.GetChild(1).GetChild(0).GetComponent<Rigidbody>().velocity=knockback;
            diedCreeperTrans.GetChild(2).GetChild(0).GetComponent<Rigidbody>().velocity=knockback;
           diedCreeperTrans.GetChild(3).GetChild(0).GetComponent<Rigidbody>().velocity=knockback;
           diedCreeperTrans.GetChild(4).GetChild(0).GetComponent<Rigidbody>().velocity=knockback;
           diedCreeperTrans.GetChild(5).GetChild(0).GetComponent<Rigidbody>().velocity=knockback;
           Destroy(diedCreeperTrans.gameObject,10f);
           ObjectPools.creeperEntityPool.Release(gameObject);
    }

    public void InitPos(){
        Invoke("InvokeInitPos",0.1f);
    }
    public void InvokeInitPos(){
        isPosInited=true;
    }
    public void Jump(){
        isJumping=true;
    }
    public void ChangeHeadPos(Vector3 pos){
     //   headTransform.rotation=q;
     headTransform.LookAt(pos);
    }
 
    float Speed()
	{
    /*    if(PlayerMove.isPaused==true){
            return entitySpeed;
        }
		curpos =new Vector2(gameObject.transform.position.x,gameObject.transform.position.z);//当前点
		float _speed = (Vector3.Magnitude(curpos - lastpos) / Time.deltaTime/4f);//与上一个点做计算除去当前帧花的时间。
		lastpos = curpos;//把当前点保存下一次用*/
        if(GameUIBeh.isPaused==true){
            return entitySpeed;
        }else{
        return cc.velocity.magnitude;    
        }
		
	}

    public void Update () {
        
        entityMotionVec=Vector3.Lerp(entityMotionVec,Vector3.zero,Time.deltaTime*3f);
        if(creeperHealth<=0f&&isCreeperDied==false){
            CreeperDie(entityMotionVec);
        }
        if(!isPosInited){
            return;
        }
     
      
   //      seeker.StartPath(transform.position, targetPosition.position, OnPathComplete);
        if(transform.position.y<-40f){
           ObjectPools.creeperEntityPool.Release(gameObject);
        }
            
      
         Vector3 targetDir;
    
         targetDir = targetPosition.position;
        
        
       
        if(cc.isGrounded!=true){
            if(!GetComponent<EntityBeh>().isInUnloadedChunks){
             entityY+=gravity*Time.deltaTime;   
            }
            
        }else{
            entityY=0f;
        }
        if(cc.isGrounded==true&&isJumping==true){
            entityY=jumpHeight;
            isJumping=false;
        }
        entityVec.y=entityY;
       // Quaternion targetRotation = Quaternion.LookRotation(targetDir);
       // entityFacingPos=targetRotation.eulerAngles;
        
        transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.Euler(new Vector3(0f,headTransform.eulerAngles.y,0f)),5f*Time.deltaTime);
        ChangeHeadPos(targetDir);
         transform.GetChild(0).GetChild(0).localScale=new Vector3(0.99f,0.99f,0.99f)+new Vector3(creeperExplodeFuse*0.1f,creeperExplodeFuse*0.1f,creeperExplodeFuse*0.1f);
            transform.GetChild(1).GetChild(0).GetChild(0).localScale=new Vector3(0.99f,0.99f,0.99f)+new Vector3(creeperExplodeFuse*0.1f,creeperExplodeFuse*0.1f,creeperExplodeFuse*0.1f);
            transform.GetChild(2).GetChild(0).GetChild(0).localScale=new Vector3(0.99f,0.99f,0.99f)+new Vector3(creeperExplodeFuse*0.1f,creeperExplodeFuse*0.1f,creeperExplodeFuse*0.1f);
           transform.GetChild(3).GetChild(0).GetChild(0).localScale=new Vector3(0.99f,0.99f,0.99f)+new Vector3(creeperExplodeFuse*0.1f,creeperExplodeFuse*0.1f,creeperExplodeFuse*0.1f);
           transform.GetChild(4).GetChild(0).GetChild(0).localScale=new Vector3(0.99f,0.99f,0.99f)+new Vector3(creeperExplodeFuse*0.1f,creeperExplodeFuse*0.1f,creeperExplodeFuse*0.1f);
           transform.GetChild(5).GetChild(0).GetChild(0).localScale=new Vector3(0.99f,0.99f,0.99f)+new Vector3(creeperExplodeFuse*0.1f,creeperExplodeFuse*0.1f,creeperExplodeFuse*0.1f);
    
        if((transform.position-targetDir).magnitude>=3f){ 
              if(entitySpeed<0.1f){
            Jump();
            }
             entityVec.x=1f;
          if(creeperExplodeFuse>=0f){
                creeperExplodeFuse-=Time.deltaTime;
            }
        }else{
            entityVec.x=0f;
            creeperExplodeFuse+=Time.deltaTime;
            if(creeperExplodeFuse>2f){
            CreeperExplode();
        }
        }
       
          
             if(cc.enabled==true){
        
            if(entityMotionVec.magnitude>0.7f){
                cc.Move(entityMotionVec*Time.deltaTime); 
            }else{
                 cc.Move((transform.forward*entityVec.x+transform.right*entityVec.z)*moveSpeed*Time.deltaTime+entityMotionVec*Time.deltaTime);
            }
            entitySpeed=Speed();
            am.SetFloat("speed",entitySpeed);
            cc.Move(new Vector3(0f,entityVec.y,0f)*5f*Time.deltaTime);    
        }
       // Vector3 velocity = dir * speed * 6;
      //  controller.SimpleMove(velocity);

    }
}
