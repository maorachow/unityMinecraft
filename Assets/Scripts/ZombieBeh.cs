using System.Collections.Generic;
using UnityEngine;

public class ZombieBeh : MonoBehaviour,ILivingEntity, IAttackableEntityTarget
{   
    public int curBlockOnFootID;
    public int prevBlockOnFootID;
    public AudioSource AS;
  
  
    public Transform currentTrans;
    public static GameObject diedZombiePrefab;
    public bool isDied { get; set; } = false;
    private CharacterController cc;
    public Animator am;
    public Transform headTransform;
    public float entityHealth{get;set;}
    public Vector3 entityVec;
    public Vector3 entityMotionVec;
    public float moveSpeed{get{return 5f;}set{moveSpeed=5f;}}
    public float gravity=-9.8f;
    public float entityY=0f;
    public float jumpHeight=2f;
    public Vector3 entityFacingPos;
    public bool isPosInited=false;
    public float attackCD=1.2f;
    public bool isJumping=false;
    public float entitySpeed;
    public EntityBeh entity;
    public IAttackableEntityTarget primaryTargetEntity
    {
        get;
        set;
    }
    public List<IAttackableEntityTarget> primaryAttackerEntities
    {
        get;
        set;
    }

    public void ClearPrimaryTarget()
    {
        primaryTargetEntity = null;
    }

    public Transform entityTransformRef
    {
        get;
        set;
    }
    public bool isIdling{get;set;}
     public float timeUsedToReachTarget;
     public bool hasReachedTarget;
    public static bool isZombiePrefabLoaded=false;

     public void ApplyDamageAndKnockback(float damageAmount,Vector3 knockback){

         if (entityHealth - damageAmount > 0f)
         {
             AS.PlayOneShot(GlobalAudioResourcesManager.TryGetEntityAudioClip("zombieHurtClip"));
        }
      
        transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.red;
         transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.red;
          transform.GetChild(0).GetChild(2).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.red;
           transform.GetChild(0).GetChild(3).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.red;
           transform.GetChild(0).GetChild(4).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.red;
           transform.GetChild(0).GetChild(5).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.red;
        entityHealth-=damageAmount;
        entityMotionVec=knockback;
        Invoke("InvokeRevertColor",0.2f);
    }
    void InvokeRevertColor(){
         transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.white;
         transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.white;
          transform.GetChild(0).GetChild(2).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.white;
           transform.GetChild(0).GetChild(3).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.white;
           transform.GetChild(0).GetChild(4).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.white;
           transform.GetChild(0).GetChild(5).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.white;
    }

    public void Start () {
        entityHealth=20f;
       entity=GetComponent<EntityBeh>();
       
        if(isZombiePrefabLoaded==false){
     
         diedZombiePrefab=Resources.Load<GameObject>("Prefabs/diedzombie");
         isZombiePrefabLoaded=true;
     
        }
            currentTrans=transform;
        
        headTransform=transform.GetChild(0).GetChild(1);
        AS = headTransform.GetComponent<AudioSource>();
        entityFacingPos =transform.rotation.eulerAngles;
    
        cc = GetComponent<CharacterController>();
        am=GetComponent<Animator>();
        entityTransformRef = headTransform;
     
    }
    
    public void InitPos(){
        Invoke("InvokeInitPos",0.1f);
    }
    public void InvokeInitPos(){
        isPosInited=true;
    }
    public void OnDisable(){
            entityMotionVec=Vector3.zero;
        isDied = false;
            entityHealth=20f;
            isPosInited=false;

            foreach (var item in primaryAttackerEntities)
            {
                if (item.primaryAttackerEntities != null)
                {
                    item.primaryAttackerEntities.Clear();

                    item.ClearPrimaryTarget();
                }

            }
            primaryAttackerEntities.Clear();
            primaryTargetEntity = null;
    }

    void OnEnable()
    {
        if (cc != null)
        {
            cc.enabled = true;
        }
        if (primaryAttackerEntities != null)
        {
            foreach (var item in primaryAttackerEntities)
            {
                if (item.primaryAttackerEntities != null)
                {
                    item.primaryAttackerEntities.Clear();

                    item.ClearPrimaryTarget();
                }

            }
            primaryAttackerEntities.Clear();
        }
        this.primaryTargetEntity = null;
        primaryAttackerEntities = new List<IAttackableEntityTarget>();
    }
    public void DieWithKnockback(Vector3 knockback){



        foreach (var item in primaryAttackerEntities)
        {
            if (item.primaryAttackerEntities != null)
            {
                item.primaryAttackerEntities.Clear();

                item.ClearPrimaryTarget();
            }

        }
       
    isDied = true;
            Transform curTrans=transform;
            Transform diedZombieTrans=Instantiate(diedZombiePrefab,curTrans.position,curTrans.rotation).GetComponent<Transform>();
       

           // cc.enabled=false;
            diedZombieTrans.GetChild(0).GetChild(0).GetComponent<Rigidbody>().position=curTrans.GetChild(0).GetChild(0).position;
            diedZombieTrans.GetChild(0).GetChild(1).GetChild(0).GetComponent<Rigidbody>().position=curTrans.GetChild(0).GetChild(1).GetChild(0).position;
            diedZombieTrans.GetChild(0).GetChild(2).GetChild(0).GetComponent<Rigidbody>().position=curTrans.GetChild(0).GetChild(2).GetChild(0).position;
            diedZombieTrans.GetChild(0).GetChild(3).GetChild(0).GetComponent<Rigidbody>().position=curTrans.GetChild(0).GetChild(3).GetChild(0).position;
            diedZombieTrans.GetChild(0).GetChild(4).GetChild(0).GetComponent<Rigidbody>().position=curTrans.GetChild(0).GetChild(4).GetChild(0).position;
            diedZombieTrans.GetChild(0).GetChild(5).GetChild(0).GetComponent<Rigidbody>().position=curTrans.GetChild(0).GetChild(5).GetChild(0).position;

            diedZombieTrans.GetChild(0).GetChild(0).rotation=curTrans.GetChild(0).GetChild(0).rotation;
            diedZombieTrans.GetChild(0).GetChild(1).GetChild(0).GetComponent<Rigidbody>().rotation=curTrans.GetChild(0).GetChild(1).GetChild(0).rotation;
            diedZombieTrans.GetChild(0).GetChild(2).GetChild(0).GetComponent<Rigidbody>().rotation=curTrans.GetChild(0).GetChild(2).GetChild(0).rotation;
            diedZombieTrans.GetChild(0).GetChild(3).GetChild(0).GetComponent<Rigidbody>().rotation=curTrans.GetChild(0).GetChild(3).GetChild(0).rotation;
            diedZombieTrans.GetChild(0).GetChild(4).GetChild(0).GetComponent<Rigidbody>().rotation=curTrans.GetChild(0).GetChild(4).GetChild(0).rotation;
            diedZombieTrans.GetChild(0).GetChild(5).GetChild(0).GetComponent<Rigidbody>().rotation=curTrans.GetChild(0).GetChild(5).GetChild(0).rotation;

         

            diedZombieTrans.GetChild(0).GetChild(0).GetComponent<Rigidbody>().velocity=knockback;
            diedZombieTrans.GetChild(0).GetChild(1).GetChild(0).GetComponent<Rigidbody>().velocity=knockback;
            diedZombieTrans.GetChild(0).GetChild(2).GetChild(0).GetComponent<Rigidbody>().velocity=knockback;
            diedZombieTrans.GetChild(0).GetChild(3).GetChild(0).GetComponent<Rigidbody>().velocity=knockback;
            diedZombieTrans.GetChild(0).GetChild(4).GetChild(0).GetComponent<Rigidbody>().velocity=knockback;
            diedZombieTrans.GetChild(0).GetChild(5).GetChild(0).GetComponent<Rigidbody>().velocity=knockback;
        //    cc.enabled=true;
            Destroy(diedZombieTrans.gameObject,30f);
        ItemEntityBeh.SpawnNewItem(transform.position.x,transform.position.y + 0.9f, transform.position.z,154,new Vector3(Random.Range(-3f,3f),Random.Range(-3f,3f),Random.Range(-3f,3f)));
             VoxelWorld.currentWorld.zombieEntityPool.Release(gameObject);
            
    }




       public void TryAttack(){
           if (primaryTargetEntity == null)
           {
               return;
           }
           if (Vector3.Magnitude(entityTransformRef.position - primaryTargetEntity.entityTransformRef.position) < zombieTargetRadius * 1.2f &&
               isIdling == false)
           {
               if (attackCD <= 0f)
               {
                primaryTargetEntity
                     .ApplyDamageAndKnockback(1f, transform.forward * 10f + transform.up * 5f);

               primaryTargetEntity.TryAddPriamryAttackerEntity(this);
                
                   am.SetBool("attack", true);
                   attackCD = 1.2f;
                   Invoke("CancelAttack", 0.2f);
               }
           }

       }
    public void CancelAttack(){
        am.SetBool("attack",false);
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
       //      Debug.Log(cc.velocity);
       	return cc.velocity.magnitude;

	}


    public void FixedUpdate(){
        if (!isPosInited)
        {
            return;
        }
        if (entity.isInUnloadedChunks==true){
            return;
        }

       
        primaryTargetEntity = PlayerMove.instance;
        for (int i=0;i< primaryAttackerEntities.Count;i++)
        {
            var item = primaryAttackerEntities[i];
            if (item is not PlayerMove)
            {
                if (item.entityTransformRef != null)
                {
                    if (item.entityTransformRef.gameObject.activeInHierarchy == true)
                    {
                        //Debug.Log("attacker enemy count:" + primaryAttackerEntities.Count);
                        primaryTargetEntity = item;
                        break;
                    }
                    else
                    {
                        primaryAttackerEntities.RemoveAt(i);
                        break;

                    }
                  
                }
            
            }
        }

        if (primaryTargetEntity != null)
        {
            RaycastHit hitInfo;

            Physics.Linecast((primaryTargetEntity.entityTransformRef.position), (headTransform.position), out hitInfo, LayerMask.GetMask("Default"));
            Debug.DrawLine((primaryTargetEntity.entityTransformRef.position), (headTransform.position), Color.green, Time.fixedDeltaTime);
            if (hitInfo.collider != null)
            {


                isIdling = true;
            }
            else
            {
                if ((primaryTargetEntity.entityTransformRef.position - headTransform.position)
                    .magnitude < 16f)
                {
                    if (primaryTargetEntity.isDied == true)
                    {
                        isIdling = true;
                    }
                    else
                    {
                        isIdling = false;
                    }
                }
                else
                {
                    isIdling = true;
                }

            }

        }
        else
        {
            isIdling = true;
        }
     
        curBlockOnFootID = WorldHelper.instance.GetBlock(currentTrans.position, entity.currentChunk);
        if (curBlockOnFootID == 0 || (101 <= curBlockOnFootID && curBlockOnFootID <= 200))
        {
            gravity = -9.8f;
        }

        EntityGroundSinkPrevent(cc, curBlockOnFootID, Time.deltaTime);
        if (prevBlockOnFootID != curBlockOnFootID)
        {
            if (curBlockOnFootID == 100)
            {
                gravity = -1f;
                AS.PlayOneShot(GlobalAudioResourcesManager.TryGetEntityAudioClip("entitySinkClip"));
                ParticleEffectManagerBeh.instance.EmitWaterSplashParticleAtPosition(transform.position);
            }
            else
            {
                gravity = -9.8f;
            }
        }

        prevBlockOnFootID = curBlockOnFootID;
        //  targetPos = playerPosition.position;
        if (Random.Range(0f, 100f) > 99f)
        {
            AS.PlayOneShot(GlobalAudioResourcesManager.TryGetEntityAudioClip("zombieIdleClip"));
        }
 

        if (isIdling == true)
        {
            if (hasReachedTarget == true)
            {
                timeUsedToReachTarget = 0f;
                Vector2 randomTargetPos =
                    new Vector2(Random.Range(transform.position.x - 8f, transform.position.x + 8f),
                        Random.Range(transform.position.z - 8f, transform.position.z + 8f));
                Vector3 finalTargetPos = new Vector3(randomTargetPos.x,
                    WorldHelper.instance.GetChunkLandingPoint(randomTargetPos.x, randomTargetPos.y) + 1f,
                    randomTargetPos.y);
                targetPos = finalTargetPos;
            }
            else
            {
                timeUsedToReachTarget += Time.deltaTime;
                if (timeUsedToReachTarget >= 5f)
                {
                    hasReachedTarget = true;
                    timeUsedToReachTarget = 0f;
                    Vector2 randomTargetPos =
                        new Vector2(Random.Range(transform.position.x - 8f, transform.position.x + 8f),
                            Random.Range(transform.position.z - 8f, transform.position.z + 8f));
                    Vector3 finalTargetPos = new Vector3(randomTargetPos.x,
                        WorldHelper.instance.GetChunkLandingPoint(randomTargetPos.x, randomTargetPos.y) + 1f,
                        randomTargetPos.y);
                    targetPos = finalTargetPos;
                }
            }
        }
        else
        {
            timeUsedToReachTarget = 0f;
            if (primaryTargetEntity != null)
            {
                targetPos = primaryTargetEntity.entityTransformRef.position;
            }
            else
            {
                isIdling=true;
            }
            
        }
         
    }


    Vector3 targetPos;

    public void EntityGroundSinkPrevent(CharacterController cc, int blockID, float dt)
    {
        if (cc.enabled == false)
        {
            return;
        }

        if (blockID > 0f && blockID < 100f)
        {
            cc.Move(new Vector3(0f, dt * 5f, 0f));
            gravity = 0f;
        }
        else
        {
            gravity = -9.8f;
        }
    }

    public static readonly float zombieTargetRadius=1.4f;
    public void MoveToTarget(CharacterController cc, Vector3 pos, float dt)
    {
        currentTrans.rotation = Quaternion.Slerp(currentTrans.rotation,
            Quaternion.Euler(new Vector3(0f, headTransform.eulerAngles.y, 0f)), 5f * dt);
        ChangeHeadPos(pos);
       

     
        if ((currentTrans.position - pos).magnitude < zombieTargetRadius)
        {
            hasReachedTarget = true;
            entityVec.x = 0f;
        }
        else
        {
            hasReachedTarget = false;
            entityVec.x = 0.6f;
        }

       
            if (entityMotionVec.magnitude > 0.7f)
            {
                cc.Move(entityMotionVec * dt);
            }
            else
            {
                cc.Move((currentTrans.forward * entityVec.x + currentTrans.right * entityVec.z) * moveSpeed * dt +
                        entityMotionVec * dt);
            }

            entitySpeed = Speed();
            //     Debug.Log(Speed());
            am.SetFloat("speed", entitySpeed);

            if (entitySpeed <= 0.01f && hasReachedTarget == false)
            {
                Jump();
            }
        

    }

    public void ApplyGravity(CharacterController cc, float gravity, float dt)
    {
        //   Debug.Log("gra");
        if (cc.enabled == true)
        {
            cc.Move((new Vector3(0f, entityVec.y, 0f)) * moveSpeed * dt);
            if (cc.isGrounded != true)
            {
                if (entity.isInUnloadedChunks == false)
                {
                    entityY += gravity * dt;
                }
            }
            else
            {
                entityY = 0f;
            }

            if (cc.isGrounded == true && isJumping == true)
            {
                entityY = jumpHeight;
                isJumping = false;
            }

            entityVec.y = entityY;
        }
    }
    public void Update () {

        float dt = Time.deltaTime;

        if (GlobalGameOptions.isGamePaused == true || dt <= 0f)
        {
            return;
        }
        if (entity.isInUnloadedChunks == true)
        {
            if (cc != null)
            {
                if (cc.enabled == true)
                {
                    cc.enabled = false;
                }

            }


            return;
        }
        else
        {
            if (cc != null)
            {
                if (cc.enabled == false)
                {
                    cc.enabled = true;
                }

            }
        }
     
        entityMotionVec =Vector3.Lerp(entityMotionVec,Vector3.zero, 3f * dt);

        if(!isPosInited){
            return;
        }
     
        if(currentTrans.position.y<-40f){
             VoxelWorld.currentWorld.zombieEntityPool.Release(gameObject);
        }

        if (attackCD > 0f)
        {
            attackCD -= dt;
        }

        if (entity.isInUnloadedChunks == true)
        {
            return;
        }

        MoveToTarget(cc, targetPos, dt);
       



            //     Debug.Log(Speed());
            //  am.SetFloat("speed",entitySpeed);

            TryAttack();
            
          
          
        
        ApplyGravity(cc,gravity,dt);


        if (entityHealth <= 0f && isDied == false)
        {
            DieWithKnockback(entityMotionVec);
        }


        // Quaternion targetRotation = Quaternion.LookRotation(targetDir);
        // entityFacingPos=targetRotation.eulerAngles;


        // Vector3 velocity = dir * speed * 6;
        //  controller.SimpleMove(velocity);

    }
}
