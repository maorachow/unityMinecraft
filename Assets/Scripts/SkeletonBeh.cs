using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class SkeletonBeh : MonoBehaviour,ILivingEntity,IAttackableEntityTarget
{

   
    
    public static GameObject diedSkeletonPrefab;
    public AudioSource AS;
    public Vector3 entityVec;
    public Vector3 entityMotionVec;
    public Animator am;
    public Transform headTransform;
    public EntityBeh entity;
    public static bool isSkeletonPrefabLoaded=false;
    public CharacterController cc;
    public bool isDied { get; set; }= false;
    public Transform currentTrans;
    public bool isPosInited = false;
    public float attackCD;
    public float gravity;
    public float entityY;
    public float jumpHeight = 2f;
    public bool isJumping = false;
    public float timeUsedToReachTarget = 0f;
    public bool isIdling;
    public bool hasReachedTarget = false;
    public float entityHealth { get ; set ; }
    public float moveSpeed { get { return 5f; } set { moveSpeed = 5f; } }
    [SerializeField]
    public IAttackableEntityTarget primaryTargetEntity
    {
        get;
        set;
    }
    [SerializeField]
    public List<IAttackableEntityTarget> primaryAttackerEntities
    {
        get;
        set;
    }

    public void ClearPrimaryTarget()
    {
         this.primaryTargetEntity=null;
    }

    public Transform entityTransformRef
    {
        get;
        set;
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

    public void DieWithKnockback(Vector3 knockback)
    {
        foreach (var item in primaryAttackerEntities)
        {
            if (item.primaryAttackerEntities!=null)
            {
                item.primaryAttackerEntities.Clear();
                
                item.ClearPrimaryTarget();
            }
           
        }
        primaryAttackerEntities.Clear();
        primaryTargetEntity = null;
        isDied = true;
        Transform curTrans = transform;
        Transform diedSkeletonTrans = Instantiate(diedSkeletonPrefab, curTrans.position, curTrans.rotation).GetComponent<Transform>();

     
        //  cc.enabled = false;
        diedSkeletonTrans.GetChild(0).GetChild(0).GetComponent<Rigidbody>().position = curTrans.GetChild(0).GetChild(0).position;
        diedSkeletonTrans.GetChild(0).GetChild(1).GetChild(0).GetComponent<Rigidbody>().position = curTrans.GetChild(0).GetChild(1).GetChild(0).position;
        diedSkeletonTrans.GetChild(0).GetChild(2).GetChild(0).GetComponent<Rigidbody>().position = curTrans.GetChild(0).GetChild(2).GetChild(0).position;
        diedSkeletonTrans.GetChild(0).GetChild(3).GetChild(0).GetComponent<Rigidbody>().position = curTrans.GetChild(0).GetChild(3).GetChild(0).position;
        diedSkeletonTrans.GetChild(0).GetChild(4).GetChild(0).GetComponent<Rigidbody>().position = curTrans.GetChild(0).GetChild(4).GetChild(0).position;
        diedSkeletonTrans.GetChild(0).GetChild(5).GetChild(0).GetComponent<Rigidbody>().position = curTrans.GetChild(0).GetChild(5).GetChild(0).position;

        diedSkeletonTrans.GetChild(0).GetChild(0).rotation = curTrans.GetChild(0).GetChild(0).rotation;
        diedSkeletonTrans.GetChild(0).GetChild(1).GetChild(0).GetComponent<Rigidbody>().rotation = curTrans.GetChild(0).GetChild(1).GetChild(0).rotation;
        diedSkeletonTrans.GetChild(0).GetChild(2).GetChild(0).GetComponent<Rigidbody>().rotation = curTrans.GetChild(0).GetChild(2).GetChild(0).rotation;
        diedSkeletonTrans.GetChild(0).GetChild(3).GetChild(0).GetComponent<Rigidbody>().rotation = curTrans.GetChild(0).GetChild(3).GetChild(0).rotation;
        diedSkeletonTrans.GetChild(0).GetChild(4).GetChild(0).GetComponent<Rigidbody>().rotation = curTrans.GetChild(0).GetChild(4).GetChild(0).rotation;
        diedSkeletonTrans.GetChild(0).GetChild(5).GetChild(0).GetComponent<Rigidbody>().rotation = curTrans.GetChild(0).GetChild(5).GetChild(0).rotation;



        diedSkeletonTrans.GetChild(0).GetChild(0).GetComponent<Rigidbody>().velocity = knockback;
        diedSkeletonTrans.GetChild(0).GetChild(1).GetChild(0).GetComponent<Rigidbody>().velocity = knockback;
        diedSkeletonTrans.GetChild(0).GetChild(2).GetChild(0).GetComponent<Rigidbody>().velocity = knockback;
        diedSkeletonTrans.GetChild(0).GetChild(3).GetChild(0).GetComponent<Rigidbody>().velocity = knockback;
        diedSkeletonTrans.GetChild(0).GetChild(4).GetChild(0).GetComponent<Rigidbody>().velocity = knockback;
        diedSkeletonTrans.GetChild(0).GetChild(5).GetChild(0).GetComponent<Rigidbody>().velocity = knockback;
      //  cc.enabled = true;
       
        Destroy(diedSkeletonTrans.gameObject, 30f);
        ItemEntityBeh.SpawnNewItem(transform.position.x, transform.position.y + 1.5f, transform.position.z, 157, new Vector3(Random.Range(-3f, 3f), Random.Range(-3f, 3f), Random.Range(-3f, 3f)));
        VoxelWorld.currentWorld.skeletonEntityPool.Release(gameObject);

    }

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


    public void ApplyDamageAndKnockback(float damageAmount, Vector3 knockback)
    {
        if (entityHealth - damageAmount > 0f)
        {
            AS.PlayOneShot(GlobalAudioResourcesManager.TryGetEntityAudioClip("skeletonHurtClip"));
        }
        
        transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material.color = Color.red;
        transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<MeshRenderer>().material.color = Color.red;
        transform.GetChild(0).GetChild(2).GetChild(0).GetComponent<MeshRenderer>().material.color = Color.red;
        transform.GetChild(0).GetChild(3).GetChild(0).GetComponent<MeshRenderer>().material.color = Color.red;
        transform.GetChild(0).GetChild(4).GetChild(0).GetComponent<MeshRenderer>().material.color = Color.red;
        transform.GetChild(0).GetChild(5).GetChild(0).GetComponent<MeshRenderer>().material.color = Color.red;
        entityHealth -= damageAmount;
        entityMotionVec = knockback;
        Invoke("InvokeRevertColor", 0.2f);
    }
    void InvokeRevertColor()
    {
        transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material.color = Color.white;
        transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<MeshRenderer>().material.color = Color.white;
        transform.GetChild(0).GetChild(2).GetChild(0).GetComponent<MeshRenderer>().material.color = Color.white;
        transform.GetChild(0).GetChild(3).GetChild(0).GetComponent<MeshRenderer>().material.color = Color.white;
        transform.GetChild(0).GetChild(4).GetChild(0).GetComponent<MeshRenderer>().material.color = Color.white;
        transform.GetChild(0).GetChild(5).GetChild(0).GetComponent<MeshRenderer>().material.color = Color.white;
    }

    public async void Attack()
    {
        if (attackCD <= 0f)
        {
            //      playerPosition.gameObject.GetComponent<PlayerMove>().ApplyDamageAndKnockback(1f, transform.forward * 10f + transform.up * 15f);
           
            am.SetBool("attack", true);
            attackCD =2f;
            await UniTask.Delay(200);
            if (headTransform == null)
            {
                return;
            }
            Vector3 arrowPos = headTransform.position + headTransform.forward*1.3f;
            EntityBeh arrow=EntityBeh.SpawnNewEntity(arrowPos.x, arrowPos.y, arrowPos.z, 4,headTransform.forward);
            arrow.GetComponent<ArrowBeh>().sourceTrans = transform;
            arrow.GetComponent<Rigidbody>().rotation=Quaternion.LookRotation(headTransform.forward);
            arrow.GetComponent<ArrowBeh>().SetArrowDamage(3f, 2f);
            //  System.Diagnostics.Stopwatch sw=new System.Diagnostics.Stopwatch();
            //   sw.Start();

            await UniTask.WaitUntil(()=>arrow.GetComponent<ArrowBeh>().isPosInited==true);
            //   sw.Stop();
            // Debug.Log(sw.Elapsed.TotalMilliseconds);
            arrow.GetComponent<Rigidbody>().constraints=RigidbodyConstraints.None;
            arrow.GetComponent<Rigidbody>().WakeUp();
            arrow.GetComponent<Rigidbody>().AddForce( headTransform.forward*20f+new Vector3(Random.Range(-0.5f,0.5f), Random.Range(-0.5f, 4.5f), Random.Range(-0.5f, 0.5f)),ForceMode.VelocityChange);
           AS.PlayOneShot(GlobalAudioResourcesManager.TryGetEntityAudioClip("skeletonShootClip"));
            Invoke("CancelAttack", 0.2f);
        }

    }
    public void CancelAttack()
    {
        am.SetBool("attack", false);
    }

    public void InitPos()
    {
        Invoke("InvokeInitPos", 0.1f);
    }
    public void InvokeInitPos()
    {
        isPosInited = true;
    }
    public void OnDisable()
    {
        entityMotionVec = Vector3.zero;
        isDied = false;
        entityHealth = 20f;
        isPosInited = false;

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
    public void OnEnable()
    {
        if (cc != null)
        {
            cc.enabled = true;
        }
        this.primaryTargetEntity = null;
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

        primaryAttackerEntities = new List<IAttackableEntityTarget>();
    }
    void Start()
    {
        entityHealth = 20f;
        if (isSkeletonPrefabLoaded == false)
        {


            isSkeletonPrefabLoaded = true;
           
            diedSkeletonPrefab = Resources.Load<GameObject>("Prefabs/diedskeleton");
           
        }
        currentTrans = transform;
        cc=GetComponent<CharacterController>(); 
        am=GetComponent<Animator>();
        entity = GetComponent<EntityBeh>();
       
        headTransform = transform.GetChild(0).GetChild(1);

        AS = headTransform. GetComponent<AudioSource>();
        entityTransformRef = headTransform;
     
    }
    public void ChangeHeadPos(Vector3 pos)
    {
        //   headTransform.rotation=q;
        Vector3 direction = (pos - headTransform.position).normalized;
        headTransform.rotation = Quaternion.Slerp(headTransform.rotation, Quaternion.LookRotation(direction), 10f * Time.deltaTime);
    }
    float Speed()
    {
           //   Debug.Log(new Vector3(cc.velocity.x, 0, cc.velocity.y).magnitude);
        return new Vector3(cc.velocity.x,0,cc.velocity.z).magnitude;

    }
    public void Jump()
    {
        isJumping = true;
    }
    public float entitySpeed;

   public Vector3 targetPos;

    public void MoveToTarget(CharacterController cc, Vector3 pos, float dt)
    {
        currentTrans.rotation = Quaternion.Slerp(currentTrans.rotation, Quaternion.Euler(new Vector3(0f, headTransform.eulerAngles.y, 0f)), 5f * dt);
        
            ChangeHeadPos(pos);
        if (isIdling)
        {
            if ((currentTrans.position - pos).magnitude < 1.4f)
            {
                hasReachedTarget = true;
            }
            else
            {
                hasReachedTarget = false;
            }
        }
        else
        {
            if ((currentTrans.position - pos).magnitude < 12f)
            {
                hasReachedTarget = true;
            }
            else
            {
                hasReachedTarget = false;
            }
        }
        if (hasReachedTarget)
        {

            entityVec.x = 0f;
        }
        else
        {
            entityVec.x = 0.6f;
        }



        if (entityMotionVec.magnitude > 0.7f)
        {
            cc.Move(entityMotionVec * dt);
        }
        else
        {
            cc.Move((transform.forward * entityVec.x + transform.right * entityVec.z)* moveSpeed * dt + entityMotionVec * dt);
        }

      
            


            entitySpeed = Speed();
            //     Debug.Log(Speed());
            am.SetFloat("speed", entitySpeed);
            if (entitySpeed <= 0.01f && !hasReachedTarget)
            {
                Jump();
            }
        
       
       
    }

    
    public void Update()
    {
        
        float dt = Time.deltaTime;
        if (GlobalGameOptions.isGamePaused == true||dt<=0f)
        {
            return;
        }
        if (!isPosInited)
        {
            return;
        }

        if (entity.isInUnloadedChunks == true)
        {
            
                if (cc.enabled == true)
                {
                    cc.enabled = false;
                }

            


                return;
        }
        else
        {
           
                if (cc.enabled == false)
                {
                    cc.enabled = true;
                }

            
        }

        
        entityMotionVec = Vector3.Lerp(entityMotionVec, Vector3.zero, 3f * dt);

        

        if (currentTrans.position.y < -40f)
        {
             VoxelWorld.currentWorld.skeletonEntityPool.Release(gameObject);
        }
        if (attackCD > 0f)
        {
            attackCD -= dt;
        }

       
        MoveToTarget(cc, targetPos, dt);

        if (primaryTargetEntity != null)
        {
            if (Vector3.Magnitude(currentTrans.position - primaryTargetEntity.entityTransformRef.position) < 12f && isIdling == false)
            {
                
                Attack();

            }
        }
       
        ApplyGravity(cc, gravity, dt);


        if (entityHealth <= 0f && isDied == false)
        {
            DieWithKnockback(entityMotionVec);
        }


        // Quaternion targetRotation = Quaternion.LookRotation(targetDir);
        // entityFacingPos=targetRotation.eulerAngles;


        // Vector3 velocity = dir * speed * 6;
        //  controller.SimpleMove(velocity);

    }

    public int curBlockOnFootID;
    public int prevBlockOnFootID;


    public void FixedUpdate()
    {
        if (!isPosInited)
        {
            return;
        }
        if (entity.isInUnloadedChunks == true)
        {
            return;
        }


        primaryTargetEntity = PlayerMove.instance;
        for (int i = 0; i < primaryAttackerEntities.Count; i++)
        {
            var item = primaryAttackerEntities[i];
            if (item is not PlayerMove)
            {
                if (item.entityTransformRef != null)
                {
                    if (item.entityTransformRef.gameObject.activeInHierarchy == true)
                    {
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

            Vector3 rayDirection =
                Vector3.Normalize((primaryTargetEntity.entityTransformRef.position) - (headTransform.position));
            Ray playerDirectionRay = new Ray(headTransform.position, rayDirection);
            Physics.Linecast((primaryTargetEntity.entityTransformRef.position), (headTransform.position), out hitInfo,
                LayerMask.GetMask("Default"));
            Debug.DrawLine((primaryTargetEntity.entityTransformRef.position),
                (transform.position + new Vector3(0f, 1.5f, 0f)), Color.green, Time.fixedDeltaTime);
            if (hitInfo.collider != null)
            {
                /*    if (hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("Ignore Raycast"))
                    {
                        // Debug.Log("hitplayer");
                        if (PlayerMove.instance.isPlayerKilled == false)
                        {
                            isIdling = false;
                        }
                        else
                        {
                            isIdling = true;
                        }
                    }
                    else
                    {
                        Debug.Log("hit terrain");
                        isIdling = true;
                    }*/
                isIdling = true;
            }
            else
            {
                if ((primaryTargetEntity.entityTransformRef.position - headTransform.position)
                    .magnitude < 32f)
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
                AS.PlayOneShot(GlobalAudioResourcesManager.TryGetEntityAudioClip("entitySinkClip1"));
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
            AS.PlayOneShot(GlobalAudioResourcesManager.TryGetEntityAudioClip("skeletonIdleClip"));
        }

        TryFindNextTarget(Time.fixedDeltaTime);
        //  isIdling=true;
        // RaycastHit info;
        //isIdling=false;
        //Debug.DrawLine(transform.position+new Vector3(0f,1f,0f),playerPosition.position+new Vector3(0f,1f,0f),Color.green,0.05f);



    }

    public void TryFindNextTarget(float deltaTime)
    {
        if (isIdling == true)
        {
            if (hasReachedTarget == true)
            {
                timeUsedToReachTarget = 0f;
                Vector2 randomTargetPos = new Vector2(Random.Range(transform.position.x - 8f, transform.position.x + 8f), Random.Range(transform.position.z - 8f, transform.position.z + 8f));
                Vector3 finalTargetPos = new Vector3(randomTargetPos.x, WorldHelper.instance.GetChunkLandingPoint(randomTargetPos.x, randomTargetPos.y) + 1f, randomTargetPos.y);
                targetPos = finalTargetPos;
            }
            else
            {
                timeUsedToReachTarget += deltaTime;
                if (timeUsedToReachTarget >= 5f)
                {
                    hasReachedTarget = true;
                    timeUsedToReachTarget = 0f;
                    Vector2 randomTargetPos = new Vector2(Random.Range(transform.position.x - 8f, transform.position.x + 8f), Random.Range(transform.position.z - 8f, transform.position.z + 8f));
                    Vector3 finalTargetPos = new Vector3(randomTargetPos.x, WorldHelper.instance.GetChunkLandingPoint(randomTargetPos.x, randomTargetPos.y) + 1f, randomTargetPos.y);
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
}
