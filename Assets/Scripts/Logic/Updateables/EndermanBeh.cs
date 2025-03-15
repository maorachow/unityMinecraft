using System.Collections.Generic;
using UnityEngine;

public class EndermanBeh : MonoBehaviour, ILivingEntity, IAttackableEntityTarget
{
    public int curBlockOnFootID;
    public int prevBlockOnFootID;
    public AudioSource AS;
 
   
    public Transform currentTrans;
    public static GameObject diedEndermanPrefab;
    public bool isDied { get; set; } = false;
    private CharacterController cc;
    public Animator am;
    public Transform headTransform;
    public Transform headHatTransform;
    public float entityHealth { get; set; }
    public Vector3 entityVec;
    public Vector3 entityMotionVec;
    public float moveSpeed { get { return 5f; } set { moveSpeed = 5f; } }
    public float gravity = -9.8f;
    public float entityY = 0f;
    public float jumpHeight = 2f;
    public Vector3 entityFacingPos;
    public bool isPosInited = false;
    public float attackCD = 1.2f;
    public bool isJumping = false;
    public float entitySpeed;
    public EntityBeh entity;
    public bool isIdling { get; set; }
    public float timeUsedToReachTarget;
    public bool hasReachedTarget;
    public static bool isEndermanPrefabLoaded = false;
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
        this.primaryTargetEntity = null;
    }

    public Transform entityTransformRef
    {
        get;
        set;
    }
    public void ApplyDamageAndKnockback(float damageAmount, Vector3 knockback)
    {
        if (entityHealth - damageAmount > 0f)
        {
            AS.PlayOneShot(GlobalGameResourcesManager.instance.audioResourcesManager.TryGetEntityAudioClip("endermanHurtClip"));
        }


        transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material.color = Color.red;
        transform.GetChild(0).GetChild(1).GetComponent<MeshRenderer>().material.color = Color.red;
        transform.GetChild(1).GetChild(0).GetComponent<MeshRenderer>().material.color = Color.red;
        transform.GetChild(1).transform.GetChild(1).GetChild(0).GetComponent<MeshRenderer>().material.color = Color.red;
        transform.GetChild(1).transform.GetChild(2).GetChild(0).GetComponent<MeshRenderer>().material.color = Color.red;
        transform.GetChild(1).transform.GetChild(3).GetChild(0).GetComponent<MeshRenderer>().material.color = Color.red;
        transform.GetChild(1).transform.GetChild(4).GetChild(0).GetComponent<MeshRenderer>().material.color = Color.red;

        entityHealth -= damageAmount;
        entityMotionVec = knockback;
        Invoke("InvokeRevertColor", 0.2f);
    }
    void InvokeRevertColor()
    {
        transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material.color = Color.white;
        transform.GetChild(0).GetChild(1).GetComponent<MeshRenderer>().material.color = Color.white;
        transform.GetChild(1).GetChild(0).GetComponent<MeshRenderer>().material.color = Color.white;
        transform.GetChild(1).transform.GetChild(1).GetChild(0).GetComponent<MeshRenderer>().material.color = Color.white;
        transform.GetChild(1).transform.GetChild(2).GetChild(0).GetComponent<MeshRenderer>().material.color = Color.white;
        transform.GetChild(1).transform.GetChild(3).GetChild(0).GetComponent<MeshRenderer>().material.color = Color.white;
        transform.GetChild(1).transform.GetChild(4).GetChild(0).GetComponent<MeshRenderer>().material.color = Color.white;
    }

    public void Start()
    {
        entityHealth = 30f;
        entity = GetComponent<EntityBeh>();
       
        if (isEndermanPrefabLoaded == false)
        {
        
            diedEndermanPrefab = Resources.Load<GameObject>("Prefabs/diedenderman");
            isEndermanPrefabLoaded = true;
          
        }
        currentTrans = transform;

        headTransform = transform.GetChild(0);
        AS = headTransform.GetComponent<AudioSource>();
        headHatTransform = headTransform.GetChild(1);
        entityFacingPos = transform.rotation.eulerAngles;

        cc = GetComponent<CharacterController>();
        am = GetComponent<Animator>();
        isIdling = true;
        entityTransformRef = headTransform;
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
        entityHealth = 30f;
        isIdling = true;
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

    public void DieWithKnockback(Vector3 knockback)
    {

        foreach (var item in primaryAttackerEntities)
        {
            if (item.primaryAttackerEntities != null)
            {
                item.primaryAttackerEntities.Clear();

                item.ClearPrimaryTarget();
            }

        }
        
        isDied = true;
        Transform curTrans = transform;
        Transform diedEndermanTrans = Instantiate(diedEndermanPrefab, curTrans.position, curTrans.rotation).GetComponent<Transform>();


       
        diedEndermanTrans.GetChild(0).GetChild(0).GetChild(0).GetComponent<Rigidbody>().position= transform.GetChild(0).GetChild(0).GetChild(0).position;
        diedEndermanTrans.GetChild(0).GetChild(1).GetComponent<Rigidbody>().position= transform.GetChild(0).GetChild(1).position;
        diedEndermanTrans.GetChild(1).GetComponent<Rigidbody>().position= transform.GetChild(1).GetChild(0).position;
        diedEndermanTrans.GetChild(2).GetChild(0).GetComponent<Rigidbody>().position = transform.GetChild(1).transform.GetChild(1).GetChild(0).position;
        diedEndermanTrans.GetChild(3).GetChild(0).GetComponent<Rigidbody>().position = transform.GetChild(1).transform.GetChild(2).GetChild(0).position;
        diedEndermanTrans.GetChild(4).GetChild(0).GetComponent<Rigidbody>().position = transform.GetChild(1).transform.GetChild(3).GetChild(0).position;
        diedEndermanTrans.GetChild(5).GetChild(0).GetComponent<Rigidbody>().position = transform.GetChild(1).transform.GetChild(4).GetChild(0).position;



        diedEndermanTrans.GetChild(0).GetChild(0).GetChild(0).GetComponent<Rigidbody>().rotation = transform.GetChild(0).GetChild(0).GetChild(0).rotation;
        diedEndermanTrans.GetChild(0).GetChild(1).GetComponent<Rigidbody>().rotation = transform.GetChild(0).GetChild(1).rotation;
        diedEndermanTrans.GetChild(1).GetComponent<Rigidbody>().rotation = transform.GetChild(1).GetChild(0).rotation;
        diedEndermanTrans.GetChild(2).GetChild(0).GetComponent<Rigidbody>().rotation = transform.GetChild(1).transform.GetChild(1).GetChild(0).rotation;
        diedEndermanTrans.GetChild(3).GetChild(0).GetComponent<Rigidbody>().rotation = transform.GetChild(1).transform.GetChild(2).GetChild(0).rotation;
        diedEndermanTrans.GetChild(4).GetChild(0).GetComponent<Rigidbody>().rotation = transform.GetChild(1).transform.GetChild(3).GetChild(0).rotation;
        diedEndermanTrans.GetChild(5).GetChild(0).GetComponent<Rigidbody>().rotation = transform.GetChild(1).transform.GetChild(4).GetChild(0).rotation;




        diedEndermanTrans.GetChild(0).GetChild(0).GetChild(0).GetComponent<Rigidbody>().velocity = knockback;
        diedEndermanTrans.GetChild(0).GetChild(1).GetComponent<Rigidbody>().velocity = knockback;
        diedEndermanTrans.GetChild(1).GetComponent<Rigidbody>().velocity = knockback;
        diedEndermanTrans.GetChild(2).GetChild(0).GetComponent<Rigidbody>().velocity = knockback;
        diedEndermanTrans.GetChild(3).GetChild(0).GetComponent<Rigidbody>().velocity = knockback;
        diedEndermanTrans.GetChild(4).GetChild(0).GetComponent<Rigidbody>().velocity = knockback;
        diedEndermanTrans.GetChild(5).GetChild(0).GetComponent<Rigidbody>().velocity = knockback;

     
     
        Destroy(diedEndermanTrans.gameObject, 30f);
        VoxelWorld.currentWorld.itemEntityManager.SpawnNewItem(transform.position.x, transform.position.y+1.5f, transform.position.z, 13, new Vector3(Random.Range(-3f, 3f), Random.Range(-3f, 3f), Random.Range(-3f, 3f)));
        entity.ReleaseEntity();

    }




    public void TryAttack()
    {
        if (primaryTargetEntity == null)
        {
            return;
        }
        if (Vector3.Magnitude(currentTrans.position - primaryTargetEntity.entityTransformRef.position) < endermanTargetRadius*1.2f && isIdling == false)
        {

            if (attackCD <= 0f)
            {
                
                am.SetBool("attack", true);
                attackCD = 1.5f;
                Invoke("InvokeDamage", 0.375f);
                Invoke("CancelAttack", 0.83f);
            }
        }
      

    }

    public void InvokeDamage()
    {
        if (primaryTargetEntity == null)
        {
            return;
        }
        if (gameObject.activeInHierarchy == false|| primaryTargetEntity.entityTransformRef==null)
        {
            return;
        }
        if (isIdling == false)
        {
                Collider[] collider = Physics.OverlapSphere(headTransform.position+headTransform.forward*1.3f, 1.7f,LayerMask.GetMask("Ignore Raycast", "Entity", "ItemEntity"));


                foreach (Collider c in collider)
                {
                    if (c.gameObject.tag == "Player" || c.gameObject.tag == "Entity")
                    {
                        if (c.GetComponent(typeof(IAttackableEntityTarget)) != null)
                        {
                           
                            IAttackableEntityTarget attackableEntityTarget = (IAttackableEntityTarget)c.GetComponent(typeof(IAttackableEntityTarget));
                            if (attackableEntityTarget != this)
                            {
                                attackableEntityTarget.ApplyDamageAndKnockback(2 + Random.Range(-1f, 1f), transform.forward * 20f + transform.up * 15f);

                                if (attackableEntityTarget.primaryAttackerEntities != null)
                                {
                                   attackableEntityTarget.TryAddPriamryAttackerEntity(this);
                                
                                }
                            }
                          
                        }
                       
                        if (c.GetComponent<ItemEntityBeh>() != null)
                        {
                            c.GetComponent<Rigidbody>().velocity = transform.forward * 20f + transform.up * 15f;
                        }
                    }
                }
              
            
        }
    }

    public void CancelAttack()
    {
        am.SetBool("attack", false);
    }

    public void Jump()
    {
        isJumping = true;
    }
    public void ChangeHeadPos(Vector3 pos)
    {
        //   headTransform.rotation=q;
        Vector3 direction = (pos - headTransform.position).normalized;
        headTransform.rotation = Quaternion.Slerp(headTransform.rotation, Quaternion.LookRotation(direction), 10f * Time.deltaTime);
    }
    float Speed()
    {
        //      Debug.Log(cc.velocity);
        return cc.velocity.magnitude;

    }


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
        for (int i = 0; i < primaryAttackerEntities.Count; i++)
        {
            var item = primaryAttackerEntities[i];
             
                if (item.entityTransformRef != null)
                {
                    if (item.entityTransformRef.gameObject.activeInHierarchy == true&&item.isDied==false)
                    {
                        primaryTargetEntity = item;
                      Debug.Log("attacker enemy count:"+primaryAttackerEntities.Count);
                    }
                    else
                    {
                        primaryAttackerEntities.RemoveAt(i);
                        break;

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
                        ClearPrimaryTarget();
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
                AS.PlayOneShot(GlobalGameResourcesManager.instance.audioResourcesManager.TryGetEntityAudioClip("entitySinkClip1"));
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
            AS.PlayOneShot(GlobalGameResourcesManager.instance.audioResourcesManager.TryGetEntityAudioClip("endermanIdleClip"));
        }

 
       //     
        

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
                timeUsedToReachTarget += Time.deltaTime;
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
                isIdling = true;
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
    public static readonly float endermanTargetRadius = 2.4f;
    public void MoveToTarget(CharacterController cc, Vector3 pos, float dt)
    {

        currentTrans.rotation = Quaternion.Slerp(currentTrans.rotation, Quaternion.Euler(new Vector3(0f, headTransform.eulerAngles.y, 0f)), 5f * dt);
        ChangeHeadPos(pos);
        if ((currentTrans.position - pos).magnitude < endermanTargetRadius)
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
    public void Update()
    {
       
        float dt = Time.deltaTime;
        
        if (GlobalGameOptions.isGamePaused == true || dt <= 0f)
        {
            return;
        }
        entityMotionVec = Vector3.Lerp(entityMotionVec, Vector3.zero, 3f * dt);

        if (!isPosInited)
        {
            return;
        }

        if (currentTrans.position.y < -40f)
        {
            entity.ReleaseEntity();
        }
        if (attackCD > 0f)
        {
            attackCD -= dt;
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

        MoveToTarget(cc, targetPos, dt);
        if (isIdling == true)
        {
            headHatTransform.localPosition = new Vector3(0, -2.375f, 0f);
        }
        else
        {
            headHatTransform.localPosition = new Vector3(0, -2.25f, 0f);
        }

        TryAttack();
       
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
}
