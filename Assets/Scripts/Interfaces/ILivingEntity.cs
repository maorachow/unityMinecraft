using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public interface ILivingEntity
{   
  //  public bool isIdling{get;set;}
   
    public float entityHealth{get;set;}
    public float moveSpeed{get;set;}

    public Transform entityTransformRef { get; set; }
    public bool isDied { get; set; }
    public void DieWithKnockback(Vector3 knockback);
    public void MoveToTarget(CharacterController cc,Vector3 pos,float dt);
    public void ApplyGravity(CharacterController cc,float gravity,float dt);
    public void EntityGroundSinkPrevent(CharacterController cc,int blockID,float dt);
    public void ApplyDamageAndKnockback(float damageAmount, Vector3 knockback);
}

public interface IAttackableEntityTarget
{
    public Transform entityTransformRef { get; set; }
    public bool isDied { get; set; }
    public void ApplyDamageAndKnockback(float damageAmount, Vector3 knockback);

    public IAttackableEntityTarget primaryTargetEntity { get; set; }
    public List<IAttackableEntityTarget> primaryAttackerEntities { get; set; }
    public void ClearPrimaryTarget();
}