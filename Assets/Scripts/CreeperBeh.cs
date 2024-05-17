using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;

public class CreeperBeh : MonoBehaviour,ILivingEntity
{
    
    public int curFootBlockID;
    public int prevFootBlockID;
    public static Transform playerPosition;
    public AudioSource AS;
    public static AudioClip creeperHurtClip;
    public static AudioClip explosionClip;
    public static GameObject diedCreeperPrefab;
 
    public static bool isCreeperPrefabLoaded=false;
    public bool isCreeperDied=false;
    private CharacterController cc;
    public Animator am;
    public Transform headTransform;
    public float entityHealth{get;set;}
    public float creeperExplodeFuse=0f;
    public Vector3 entityVec;
    public float moveSpeed{get{return 5f;}set{moveSpeed=5f;}}
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
     public float entityMoveDrag=0f;
     public EntityBeh entity;
   //  [SerializeField]
      public bool isIdling{get;set;}
     public bool hasReachedTarget=false;
     public float timeUsedToReachTarget=0f;
    public void Start () {
        entity=GetComponent<EntityBeh>();
        AS=GetComponent<AudioSource>();
        entityHealth=20f;
        isCreeperDied=false;
        if(isCreeperPrefabLoaded==false){
            isCreeperPrefabLoaded = true;
            creeperHurtClip =Resources.Load<AudioClip>("Audios/Creeper_say2");
                diedCreeperPrefab=Resources.Load<GameObject>("Prefabs/diedcreeper");
               // explosionPrefab=Resources.Load<GameObject>("Prefabs/creeperexploeffect");
            playerPosition=GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
            explosionClip = Resources.Load<AudioClip>("Audios/Explosion4");
        }
       
        headTransform=transform.GetChild(1);
        entityFacingPos=transform.rotation.eulerAngles;
    
        cc = GetComponent<CharacterController>();
        am=GetComponent<Animator>();
    
    }

     public void OnDisable(){
        creeperExplodeFuse=0f;
        entityMotionVec=Vector3.zero;
        isCreeperDied=false;
        entityHealth=20f;
            isPosInited=false;
    }
    public void OnEnable(){
        if(cc!=null){
         cc.enabled=true;   
        }
        
    }
    public void ApplyDamageAndKnockback(float damageAmount,Vector3 knockback){
        AudioSource.PlayClipAtPoint(creeperHurtClip,transform.position,1f);
        transform.GetChild(0).GetComponent<MeshRenderer>().material.color=Color.red;
         transform.GetChild(1).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.red;
          transform.GetChild(2).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.red;
           transform.GetChild(3).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.red;
           transform.GetChild(4).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.red;
           transform.GetChild(5).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.red;
        entityHealth-=damageAmount;
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
                if (c.GetComponent(typeof(ILivingEntity)) != null)
                {
                    ILivingEntity livingEntity = (ILivingEntity)c.GetComponent(typeof(ILivingEntity));
                    livingEntity.ApplyDamageAndKnockback(10 + Random.Range(-5f, 5f), (transform.position - c.transform.position).normalized * Random.Range(-20f, -30f));
                }
                if (c.GetComponent(typeof(PlayerMove)) != null)
                {
                    PlayerMove livingEntity = (PlayerMove)c.GetComponent(typeof(PlayerMove));
                    livingEntity.ApplyDamageAndKnockback(10 + Random.Range(-5f, 5f), (transform.position - c.transform.position).normalized * Random.Range(-20f, -30f));
                }
                if (c.GetComponent<ItemEntityBeh>()!=null){
                    c.GetComponent<Rigidbody>().velocity=(transform.position-c.transform.position).normalized*Random.Range(-20f,-30f);
                }
            }
        }
        ParticleEffectManagerBeh.instance.EmitExplodeParticleAtPosition(new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z));
        AudioSource.PlayClipAtPoint(explosionClip,transform.position,5f);
        cc.enabled=false;
           VoxelWorld.currentWorld.creeperEntityPool.Release(gameObject);
    }
    public void DieWithKnockback(Vector3 knockback){
         AudioSource.PlayClipAtPoint(creeperHurtClip,transform.position,1f);
        isCreeperDied=true;
      //  cc.enabled=false;
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
           diedCreeperTrans.GetChild(5).GetChild(0).GetComponent<Rigidbody>().velocity= knockback;
        //     cc.enabled=true;
        Destroy(diedCreeperTrans.gameObject, 30f);
        ItemEntityBeh.SpawnNewItem(transform.position.x, transform.position.y, transform.position.z, 155, new Vector3(Random.Range(-3f, 3f), Random.Range(-3f, 3f), Random.Range(-3f, 3f)));
         VoxelWorld.currentWorld.creeperEntityPool.Release(gameObject);
       
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
      Vector3 direction = (pos-headTransform.position).normalized;
     headTransform.rotation=Quaternion.Slerp(headTransform.rotation,Quaternion.LookRotation(direction),10f*Time.deltaTime);
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
    void FixedUpdate(){
        if(entity.isInUnloadedChunks==true){
            return;
        }
        var results = new NativeArray<RaycastHit>(1, Allocator.TempJob);
        var commands = new NativeArray<RaycastCommand>(1, Allocator.TempJob);
        Vector3 rayDirection=Vector3.Normalize(playerPosition.position-(transform.position+new Vector3(0f,1f,0f)));
        commands[0]=new RaycastCommand(transform.position+new Vector3(0f,1f,0f),rayDirection,new QueryParameters((1 << 0) | (1 << 2)),16f);
        JobHandle handle = RaycastCommand.ScheduleBatch(commands, results, 1, 1, default(JobHandle));
        curFootBlockID=WorldHelper.instance.GetBlock(transform.position,entity.currentChunk);
 //  curHeadBlockID=WorldHelper.instance.GetBlock(cameraPos.position);
    if(curFootBlockID==0||(101<=curFootBlockID&&curFootBlockID<=200)){
        gravity=-9.8f;
    }
    EntityGroundSinkPrevent(cc,curFootBlockID,Time.deltaTime);
    if(curFootBlockID!=prevFootBlockID){
       
        if(curFootBlockID==100 && prevFootBlockID == 0)
            {
          
        gravity=-0.1f;
        entityMoveDrag=0.6f;
         AudioSource.PlayClipAtPoint(PlayerMove.playerSinkClip,transform.position,1f);
                ParticleEffectManagerBeh.instance.EmitWaterSplashParticleAtPosition(transform.position);
        } 

            if (curFootBlockID == 0 && prevFootBlockID == 100)
            {
                entityMoveDrag = 0f;
                gravity = -9.8f;
                      AudioSource.PlayClipAtPoint(PlayerMove.playerSinkClip,transform.position,1f);
                ParticleEffectManagerBeh.instance.EmitWaterSplashParticleAtPosition(transform.position);
            }
    }

        if (curFootBlockID == 100)
        {
            gravity = -0.1f;
            entityMoveDrag = 0.6f;
            

        }
        else
        {
            entityMoveDrag = 0f;
            gravity = -9.8f;
        }
            prevFootBlockID =curFootBlockID;
    handle.Complete();
  //  isIdling=true;
   // RaycastHit info;
     //isIdling=false;
     //Debug.DrawLine(transform.position+new Vector3(0f,1f,0f),playerPosition.position+new Vector3(0f,1f,0f),Color.green,0.05f);
        if(results[0].collider!=null){
          //  Debug.Log("hit");
           
            if(results[0].collider.gameObject.tag=="Player"){
               // Debug.Log("hitplayer");
                 isIdling=false;
            }else{
                isIdling=true; 
            }
            
        }else{
            isIdling=true;
        }

    if(isIdling==true){
        if(hasReachedTarget==true){
            timeUsedToReachTarget=0f;
          Vector2 randomTargetPos=new Vector2(Random.Range(transform.position.x-8f,transform.position.x+8f),Random.Range(transform.position.z-8f,transform.position.z+8f));
         Vector3 finalTargetPos=new Vector3(randomTargetPos.x,WorldHelper.instance.GetChunkLandingPoint(randomTargetPos.x,randomTargetPos.y)+1f,randomTargetPos.y);
        targetPos=finalTargetPos;  
        }else{
            timeUsedToReachTarget+=Time.deltaTime;
            if(timeUsedToReachTarget>=5f){
                hasReachedTarget=true;
                timeUsedToReachTarget=0f;
                   Vector2 randomTargetPos=new Vector2(Random.Range(transform.position.x-8f,transform.position.x+8f),Random.Range(transform.position.z-8f,transform.position.z+8f));
         Vector3 finalTargetPos=new Vector3(randomTargetPos.x,WorldHelper.instance.GetChunkLandingPoint(randomTargetPos.x,randomTargetPos.y)+1f,randomTargetPos.y);
        targetPos=finalTargetPos;  
            }
        }
           
    }else{
        targetPos=playerPosition.position;
    }
    results.Dispose();
        commands.Dispose();
    }
    public void MoveToTarget(CharacterController cc,Vector3 pos,float dt){
        if(cc.enabled==false){
            return;
        }
         transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.Euler(new Vector3(0f,headTransform.eulerAngles.y,0f)),5f*Time.deltaTime);
            ChangeHeadPos(pos);
           if((transform.position-playerPosition.position).magnitude<=3f){
            entityVec.x=0f;
            creeperExplodeFuse+=Time.deltaTime;    
            }else{
                 entityVec.x=1f;
               if(creeperExplodeFuse>=0f){
                creeperExplodeFuse-=Time.deltaTime;
                    }    
            if(entitySpeed<=0.01f){
                Jump();
            }
            }
        if(creeperExplodeFuse>2f){
            CreeperExplode();
        }       
           
  
         if((transform.position - pos).magnitude<1.4f){
            hasReachedTarget=true;
        }else{
            
            hasReachedTarget=false;
        }
            if(entityMotionVec.magnitude>0.7f){
                cc.Move(entityMotionVec*Time.deltaTime*(1f-entityMoveDrag)); 
            }else{
                cc.Move((transform.forward*entityVec.x+transform.right*entityVec.z)*(1f-entityMoveDrag)*moveSpeed*Time.deltaTime+entityMotionVec*Time.deltaTime);
            }
            entitySpeed=Speed();
            am.SetFloat("speed",entitySpeed);
    }



    public void ApplyGravity(CharacterController cc,float gravity,float dt){
            if(cc.enabled==true){
              cc.Move((new Vector3(0f,entityVec.y,0f))*moveSpeed*dt);
        if(cc.isGrounded!=true){
            if(entity.isInUnloadedChunks==false){
             entityY+=gravity*dt;   
            }
            
        }else{
            entityY=0f;
        }
        if(cc.isGrounded==true&&isJumping==true){
            entityY=jumpHeight;
            isJumping=false;
        }
        entityVec.y=entityY;    
        }else return;
        }
  /*  public void ApplyGravity(CharacterController cc,float gravity,float dt){
        
            if(cc.isGrounded!=true){
           
           
             entityY+=gravity*dt;   
          if(curFootBlockID==100){
                
                entityY=Mathf.Clamp(entityY,-3f,1f);
             }
            
            
        }else{
             
              entityY=0f;   

           
        }
          if((cc.isGrounded==true||curFootBlockID==100)&&isJumping==true){
            if(curFootBlockID==100){
            entityY=jumpHeight/3f;
            isJumping=false;
            }else{
                 entityY=jumpHeight;
                 isJumping=false;
            }
         
        }
        entityVec.y=entityY;
        cc.Move(new Vector3(0f,entityVec.y,0f)*5f*dt);        
      
    }*/


   public Vector3 targetPos;

    public void EntityGroundSinkPrevent(CharacterController cc,int blockID,float dt){
        if(cc.enabled==false){
            return;
        }
         if(blockID>0f&&blockID<100f){
            cc.Move(new Vector3(0f,dt*5f,0f));
            gravity=0f;
         }  else{
            gravity=-9.8f;
         }
    }
    public void Update () {
        if(cc.enabled==false){
            return;
        }
        float dt=Time.deltaTime;
        
        Vector3 position=transform.position;
        entityMotionVec=Vector3.Lerp(entityMotionVec,Vector3.zero,Time.deltaTime*3f);
        if(entityHealth<=0f&&isCreeperDied==false){
            DieWithKnockback(entityMotionVec);
        }
        if(!isPosInited){
            return;
        }
     
      
   //      seeker.StartPath(transform.position, targetPosition.position, OnPathComplete);
        if(position.y<-40f){
            VoxelWorld.currentWorld.creeperEntityPool.Release(gameObject);
        }
            
      
       
         
         if(entity.isInUnloadedChunks==true){
                    return;
        }
        if(cc.enabled==true){
            MoveToTarget(cc,targetPos,dt);
             ApplyGravity(cc,gravity,dt);
        }
       
            transform.GetChild(0).GetChild(0).localScale=new Vector3(0.99f,0.99f,0.99f)+new Vector3(creeperExplodeFuse*0.1f,creeperExplodeFuse*0.1f,creeperExplodeFuse*0.1f);
            transform.GetChild(1).GetChild(0).GetChild(0).localScale=new Vector3(0.99f,0.99f,0.99f)+new Vector3(creeperExplodeFuse*0.1f,creeperExplodeFuse*0.1f,creeperExplodeFuse*0.1f);
            transform.GetChild(2).GetChild(0).GetChild(0).localScale=new Vector3(0.99f,0.99f,0.99f)+new Vector3(creeperExplodeFuse*0.1f,creeperExplodeFuse*0.1f,creeperExplodeFuse*0.1f);
            transform.GetChild(3).GetChild(0).GetChild(0).localScale=new Vector3(0.99f,0.99f,0.99f)+new Vector3(creeperExplodeFuse*0.1f,creeperExplodeFuse*0.1f,creeperExplodeFuse*0.1f);
            transform.GetChild(4).GetChild(0).GetChild(0).localScale=new Vector3(0.99f,0.99f,0.99f)+new Vector3(creeperExplodeFuse*0.1f,creeperExplodeFuse*0.1f,creeperExplodeFuse*0.1f);
            transform.GetChild(5).GetChild(0).GetChild(0).localScale=new Vector3(0.99f,0.99f,0.99f)+new Vector3(creeperExplodeFuse*0.1f,creeperExplodeFuse*0.1f,creeperExplodeFuse*0.1f);
    
       // Quaternion targetRotation = Quaternion.LookRotation(targetDir);
       // entityFacingPos=targetRotation.eulerAngles;
        
       
            
        
       // Vector3 velocity = dir * speed * 6;
      //  controller.SimpleMove(velocity);

    }
}
