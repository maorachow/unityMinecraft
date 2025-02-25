using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TNTBeh : MonoBehaviour
{

    public Rigidbody rigidbody;
    public Transform currentTrans;
    public MeshRenderer meshRenderer;
    public bool isPosInited;
    public EntityBeh entityBeh;
   public static AudioClip explosionClip { get { return CreeperBeh.explosionClip; } } 
 
    public float fuseTime = 4f;
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
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
        rigidbody.velocity = Vector3.zero;  
        
    }
    public void FixedUpdate()
    {
        if(entityBeh.isInUnloadedChunks)
        {
            rigidbody.constraints = RigidbodyConstraints.FreezeAll;
        }
        else
        {
            rigidbody.constraints = RigidbodyConstraints.None;
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
                    WorldHelper.instance.BreakBlockAtPoint(blockPoint);
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
        AudioSource.PlayClipAtPoint(explosionClip, transform.position, 5f);
    }
    public void AddForce(Vector3 force)
    {
        rigidbody.velocity=force;
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
