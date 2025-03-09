using UnityEngine;

public class TNTBeh : MonoBehaviour
{

    public Rigidbody rb;
    public Transform currentTrans;
    public MeshRenderer meshRenderer;
    public bool isPosInited;
    public EntityBeh entityBeh;
   
 
    public float fuseTime = 4f;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentTrans = transform;
        meshRenderer = GetComponent<MeshRenderer>();
        entityBeh = GetComponent<EntityBeh>();
    }

    // Update is called once per frame
    void Update()
    {
        fuseTime-= Time.deltaTime;
        if (fuseTime - Mathf.Floor(fuseTime)>=0.5f)
        {
            meshRenderer.material.color=Color.white;
        }
        else
        {
            meshRenderer.material.color = Color.grey;
        }
        if (fuseTime < 0)
        {
            Explode();
            VoxelWorld.currentWorld.tntEntityPool.Release(gameObject);
        }
    }
    private void OnDisable()
    {
        fuseTime = 4f;
        rb.velocity = Vector3.zero;  
        
    }
    public void FixedUpdate()
    {
        if(entityBeh.isInUnloadedChunks)
        {
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }
        else
        {
            rb.constraints = RigidbodyConstraints.None;
        }


    }
    public void Explode()
    {
        Collider[] collider = Physics.OverlapSphere(transform.position, 6f);
        Vector3 exploCenter=transform.position;
        for(int x=-2;x<3;x++)
        {
            for (int y = -2; y < 3; y++)
            {
                for (int z = -2; z < 3; z++)
                {
                    Vector3 blockPoint=exploCenter + new Vector3(x,y,z);

                    if ((blockPoint - transform.position).magnitude < 2.5f)
                    {
                        if (WorldHelper.instance.GetBlock(blockPoint) != 0)
                        {
                            WorldHelper.instance.SendBreakBlockOperation(ChunkCoordsHelper.Vec3ToBlockPos(blockPoint) );
                        }
                       
                    }
                }
            }

        }
        foreach (Collider c in collider)
        {
            if (c.gameObject.tag == "Player" || c.gameObject.tag == "Entity")
            {
                if (c.GetComponent<PlayerMove>() != null)
                {
                    c.GetComponent<PlayerMove>().ApplyDamageAndKnockback(10f + Random.Range(-5f, 5f), (transform.position - c.transform.position).normalized * Random.Range(-20f, -30f));
                }
                if (c.GetComponent(typeof(ILivingEntity)) != null)
                {
                    ILivingEntity livingEntity = (ILivingEntity)c.GetComponent(typeof(ILivingEntity));
                    livingEntity.ApplyDamageAndKnockback(10 + Random.Range(-5f, 5f), (transform.position - c.transform.position).normalized * Random.Range(-20f, -30f));
                }
                if (c.GetComponent<ItemEntityBeh>() != null)
                {
                    c.GetComponent<Rigidbody>().velocity = (transform.position - c.transform.position).normalized * Random.Range(-20f, -30f);
                }
                if (c.GetComponent<TNTBeh>() != null)
                {
                    c.GetComponent<TNTBeh>().AddForce((transform.position - c.transform.position).normalized * Random.Range(-20f, -30f)) ;
                }
            }
        }
        ParticleEffectManagerBeh.instance.EmitExplodeParticleAtPosition(new Vector3(transform.position.x, transform.position.y, transform.position.z));
        GlobalAudioResourcesManager.PlayClipAtPointCustomRollOff(GlobalAudioResourcesManager.TryGetEntityAudioClip("entityExplodeClip"), transform.position, 1f, 40f);
    }
    public void AddForce(Vector3 force)
    {
        rb.velocity=force;
    }
    public void InitPos()
    {
       
        Invoke("InvokeInitPos", 0.1f);
    }
    public void InvokeInitPos()
    {
        isPosInited = true;
    }
}
