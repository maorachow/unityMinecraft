using Cysharp.Threading.Tasks;
using monogameMinecraftShared.World;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

public partial class PlayerMove
{

    async void PlayerCritAttack()
    {
        isPlayerWieldingItem = true;
        CritAttackAnimate();
        Invoke("CancelCritAttackInvoke", 0.75f);
        await UniTask.Delay(TimeSpan.FromSeconds(0.75), ignoreTimeScale: false);

        Vector3 attackEffectPoint;
        RaycastHit infoForward;
        if (Physics.Linecast(headPos.position, headPos.position + headPos.forward * 4f, out infoForward))
        {
            attackEffectPoint = infoForward.point;
        }
        else
        {
            attackEffectPoint = headPos.position + headPos.forward * 4f;
        }

        /* GameObject a=Instantiate(playerSweepParticlePrefab,attackEffectPoint,Quaternion.identity);
         a.GetComponent<ParticleSystem>().Emit(1);
         Destroy(a,2f);*/
        ParticleEffectManagerBeh.instance.EmitPlayerSweepParticleAtPosition(attackEffectPoint);
        GlobalAudioResourcesManager.PlayClipAtPointCustomRollOff(GlobalGameResourcesManager.instance.audioResourcesManager.TryGetEntityAudioClip("playerSweepAttackClip"),attackEffectPoint,1,40f);
        Collider[] colliders = Physics.OverlapSphere(transform.position, 4f);
        foreach (var c in colliders)
        {
            if (c.gameObject.tag == "Entity")
            {
                if (c.GetComponent(typeof(ILivingEntity)) != null)
                {
                    ILivingEntity livingEntity = (ILivingEntity)c.GetComponent(typeof(ILivingEntity));
                    livingEntity.ApplyDamageAndKnockback(10f + Random.Range(-5f, 5f),
                        (transform.position - c.transform.position).normalized * Random.Range(-20f, -30f));
                }

                if (c.GetComponent<ItemEntityBeh>() != null)
                {
                    c.GetComponent<Rigidbody>().velocity =
                        (transform.position - c.transform.position).normalized * Random.Range(-20f, -30f);
                }
            }
        }

        isPlayerWieldingItem = false;
    }

    async void PlayerCritAttackBow()
    {
        isPlayerWieldingItem = true;
        CritAttackBowAnimate();
        Invoke("CancelCritAttackBowInvoke",1.25f);
        await UniTask.Delay(TimeSpan.FromSeconds(0.65), ignoreTimeScale: false);

        for (int i = 0; i < 5; i++)
        {
            Vector3 arrowPos = playerHeadCenterRef.position + playerHeadCenterRef.forward * 1.3f;
            EntityBeh arrow = VoxelWorld.currentWorld.entityManager .SpawnNewEntity(arrowPos.x, arrowPos.y, arrowPos.z, 4, playerHeadCenterRef.forward);
            ArrowBeh arrowComponent = arrow.GetComponent<ArrowBeh>();
            arrowComponent.sourceTrans = transform;
            arrow.GetComponent<Rigidbody>().rotation = Quaternion.LookRotation(playerHeadCenterRef.forward);
            arrowComponent.CheckIsOwnedByPlayer();
            arrowComponent.SetArrowDamage(4f,2f);
            //  System.Diagnostics.Stopwatch sw=new System.Diagnostics.Stopwatch();
            //   sw.Start();

            await UniTask.WaitUntil(() => arrowComponent.isPosInited == true);
            //   sw.Stop();
            // Debug.Log(sw.Elapsed.TotalMilliseconds);
            arrow.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
            arrow.GetComponent<Rigidbody>().WakeUp();
            arrow.GetComponent<Rigidbody>().AddForce(playerHeadCenterRef.forward * 40f, ForceMode.Impulse);
           AS.PlayOneShot(GlobalGameResourcesManager.instance.audioResourcesManager.TryGetEntityAudioClip("playerShootClip"));

            await UniTask.Delay(TimeSpan.FromSeconds(0.1), ignoreTimeScale: false);
        }
     
        isPlayerWieldingItem = false;

    }
    void cancelAttackInvoke()
    {
        am.SetBool("isattacking", false);
    }

    void AttackAnimate()
    {
        am.SetBool("isattacking", true);
    }

    void CancelCritAttackInvoke()
    {
        am.SetBool("isattackingcrit", false);
    }

    void CritAttackAnimate()
    {
        am.SetBool("isattackingcrit", true);
    }
    void CancelCritAttackBowInvoke()
    {
        am.SetBool("isattackingcritbow", false);
    }

    void CritAttackBowAnimate()
    {
        am.SetBool("isattackingcritbow", true);
     
    }
    void AttackEnemy(GameObject go)
    {
        AttackAnimate();
        Invoke("cancelAttackInvoke", 0.16f);


        if (go.GetComponent<IAttackableEntityTarget>() != null)
        {
            inventory.GetItemInfoFromSlot(currentSelectedHotbar - 1, out int itemID, out int itemCount);

            if (itemID == 152)
            {
                go.GetComponent<IAttackableEntityTarget>()
                    .ApplyDamageAndKnockback(7f, (transform.position - go.transform.position).normalized * -15f);
            }
            else if (itemID == 151)
            {
                go.GetComponent<IAttackableEntityTarget>()
                    .ApplyDamageAndKnockback(5f, (transform.position - go.transform.position).normalized * -15f);
            }
            else
            {
                go.GetComponent<IAttackableEntityTarget>()
                    .ApplyDamageAndKnockback(1f, (transform.position - go.transform.position).normalized * -10f);
            }

            if (go.GetComponent<IAttackableEntityTarget>()!= null)
            {
                go.GetComponent<IAttackableEntityTarget>()
                    .TryAddPriamryAttackerEntity(this);
            }
          
        }

       /* if (go.GetComponent<CreeperBeh>() != null)
        {
            if (inventoryDic[currentSelectedHotbar - 1] == 152)
            {
                go.GetComponent<CreeperBeh>()
                    .ApplyDamageAndKnockback(7f, (transform.position - go.transform.position).normalized * -20f);
            }
            else if (inventoryDic[currentSelectedHotbar - 1] == 151)
            {
                go.GetComponent<CreeperBeh>()
                    .ApplyDamageAndKnockback(5f, (transform.position - go.transform.position).normalized * -20f);
            }
            else
            {
                go.GetComponent<CreeperBeh>()
                    .ApplyDamageAndKnockback(1f, (transform.position - go.transform.position).normalized * -10f);
            }
        }

        if (go.GetComponent<SkeletonBeh>() != null)
        {
            if (inventoryDic[currentSelectedHotbar - 1] == 152)
            {
                go.GetComponent<SkeletonBeh>()
                    .ApplyDamageAndKnockback(7f, (transform.position - go.transform.position).normalized * -20f);
            }
            else if (inventoryDic[currentSelectedHotbar - 1] == 151)
            {
                go.GetComponent<SkeletonBeh>()
                    .ApplyDamageAndKnockback(5f, (transform.position - go.transform.position).normalized * -20f);
            }
            else
            {
                go.GetComponent<SkeletonBeh>()
                    .ApplyDamageAndKnockback(1f, (transform.position - go.transform.position).normalized * -10f);
            }
        }

        if (go.GetComponent<EndermanBeh>() != null)
        {
            if (inventoryDic[currentSelectedHotbar - 1] == 152)
            {
                go.GetComponent<EndermanBeh>()
                    .ApplyDamageAndKnockback(7f, (transform.position - go.transform.position).normalized * -20f);
            }
            else if (inventoryDic[currentSelectedHotbar - 1] == 151)
            {
                go.GetComponent<EndermanBeh>()
                    .ApplyDamageAndKnockback(5f, (transform.position - go.transform.position).normalized * -20f);
            }
            else
            {
                go.GetComponent<EndermanBeh>()
                    .ApplyDamageAndKnockback(1f, (transform.position - go.transform.position).normalized * -10f);
            }

            go.GetComponent<EndermanBeh>().isIdling = false;
        }*/
    }

    async void LeftClick()
    {
        if (cameraPosMode == 1)
        {
            return;
        }

        if (isPlayerWieldingItem)
        {
            return;
        }

        Ray ray = new Ray(playerHeadCenterRef.position, playerHeadCenterRef.forward); 
        RaycastHit info;
        if (Physics.Raycast(ray, out info, 5f, ~LayerMask.GetMask("Ignore Raycast")))
        {
            if (info.collider.gameObject.tag == "Entity")
            {
                AttackEnemy(info.collider.gameObject);
                return;
            }
            VoxelWorldRay blockRay = new VoxelWorldRay(ray.origin, ray.direction);
            Vector3 blockPoint = info.point + headPos.forward * 0.01f;
            /*   int tmpID = WorldHelper.instance.GetBlock(blockPoint);
               if (tmpID == 0)
               {
                   return;
               }*/

           bool isBlockBreak= TryBreakBlock(blockRay);
           if (isBlockBreak)
           {
               AttackAnimate();
               Invoke("cancelAttackInvoke", 0.16f);
            }
          
            /*       WorldHelper.instance.SetBlockByHand(blockPoint,0);
                   GameObject b=ObjectPools.particleEffectPool.Get();
                   b.transform.position=new Vector3(WorldHelper.instance.Vec3ToBlockPos(blockPoint).x+0.5f,WorldHelper.instance.Vec3ToBlockPos(blockPoint).y+0.5f,WorldHelper.instance.Vec3ToBlockPos(blockPoint).z+0.5f);
                   b.GetComponent<particleAndEffectBeh>().blockID=tmpID;
                   b.GetComponent<particleAndEffectBeh>().SendMessage("EmitParticle");*/

        }
    }

    void PlayerEatAnimate()
    {
        am.SetBool("iseating", true);
    }

    void PlayerCancelEatAnimateInvoke()
    {
        am.SetBool("iseating", false);
    }
    void PlayerUpdateBlockOutlineShapeInvoke()
    {
        
        blockOutline.GetComponent<BlockOutlineBeh>().ManualOnCurrentBlockChanged();
    }
    public bool isPlayerWieldingItem = false;

    async void PlayerEat()
    {
        if (playerHealth < 20f)
        {
            isPlayerWieldingItem = true;
          
         //   GameUIBeh.instance.UpdateBlockOnHandText();
            PlayerEatAnimate();
            for (int i = 0; i < 3; i++)
            {
                await UniTask.Delay(300);
                AS.PlayOneShot(GlobalGameResourcesManager.instance.audioResourcesManager.TryGetEntityAudioClip("playerEatClip"));
            }

            Invoke("PlayerCancelEatAnimateInvoke", 0f);
            playerHealth += 4f;
            playerHealth = Mathf.Clamp(playerHealth, 0f, 20f);

         //   GameUIBeh.instance.PlayerHealthSliderOnValueChanged(playerHealth);

            isPlayerWieldingItem = false;
        }
        else
        {
            return;
        }
    }

    async void ThrowTNT()
    {
        Vector3 tntPos = headPos.position + headPos.forward;
        EntityBeh tntEntity = VoxelWorld.currentWorld.entityManager.SpawnNewEntity(tntPos.x, tntPos.y, tntPos.z, 2);
        await UniTask.WaitUntil(() => tntEntity.GetComponent<TNTBeh>().GetComponent<Rigidbody>() != null);
        tntEntity.GetComponent<TNTBeh>().AddForce((headPos.forward * 16));
      
        AttackAnimate();
        Invoke("cancelAttackInvoke", 0.16f);
    }

    public void UpgradeArmor()
    {
        if (playerArmorPoints >= playerMaxArmorPoints)
        {
            return;
        }

        playerArmorPoints = Mathf.Min(playerArmorPoints + 2f, playerMaxArmorPoints);
        AS.PlayOneShot(GlobalGameResourcesManager.instance.audioResourcesManager.TryGetEntityAudioClip("equipDiamondClip"));
      //  GameUIBeh.instance.PlayerArmorPointsSliderOnValueChanged(playerArmorPoints);
     
        AttackAnimate();
        Invoke("cancelAttackInvoke", 0.16f);
    }

    async void RightClick()
    {
        if (cameraPosMode == 1)
        {
            return;
        }

        if (isPlayerWieldingItem)
        {
            return;
        }

        Ray ray = new Ray(playerHeadCenterRef.position, playerHeadCenterRef.forward);
        RaycastHit info;
        inventory.GetItemInfoFromSlot(currentSelectedHotbar - 1,out int itemID,out int itemCount);
        if (itemID == 154)
        {
            inventory.TryRemoveItemFromSlot(currentSelectedHotbar - 1, 1);
            PlayerEat();
           
            return;
        }

        if (itemID == 156)
        {
            inventory.TryRemoveItemFromSlot(currentSelectedHotbar - 1, 1);
            ThrowTNT();
            return;
        }

        if (itemID == 158)
        {
            inventory.TryRemoveItemFromSlot(currentSelectedHotbar - 1, 1);
            UpgradeArmor();
            return;
        }

        if (critAttackCD <= 0f)
        {
            if (itemID == 157)
            {
                PlayerCritAttackBow();
                critAttackCD = 2.3f;
                return;
            }
        }
        
        if (Physics.Raycast(ray, out info, 5f, ~LayerMask.GetMask("Ignore Raycast")) && info.collider.gameObject.tag == "Entity" && critAttackCD <= 0f)
        {
            if (itemID == 152)
            {
                PlayerCritAttack();
                critAttackCD = 1f;
                return;
            }
        }

        if (Physics.Raycast(ray, out info, 5f, ~LayerMask.GetMask("Ignore Raycast")) && info.collider.gameObject.tag != "Entity" &&
            info.collider.gameObject.tag != "Player")
        {

            VoxelWorldRay blockRay = new VoxelWorldRay(ray.origin, ray.direction);
            BlockData outBlockData;
            bool isInteractingWithBlocks = TryInteractWithBlock(blockRay,out outBlockData);
            if (isInteractingWithBlocks)
            {
                AttackAnimate();
                Invoke("cancelAttackInvoke", 0.16f);
               Invoke("PlayerUpdateBlockOutlineShapeInvoke",0.1f);
              
                return;
            }

            bool canItemConvertToBlock= GlobalGameResourcesManager.instance.itemIDToBlockIDMapper.CanMapToBlockID(itemID);
            BlockData blockID = GlobalGameResourcesManager.instance.itemIDToBlockIDMapper.ToBlockID(itemID);

            if (canItemConvertToBlock)
            {
                bool isBlockPlaced = TryPlaceBlock(blockID, blockRay, out Vector3 blockPoint);

                if (isBlockPlaced)
                {
                    GlobalAudioResourcesManager.PlayClipAtPointCustomRollOff(GlobalGameResourcesManager.instance.audioResourcesManager.TryGetBlockAudioClip(itemID), transform.position, 1f, 40f);


                    inventory.TryRemoveItemFromSlot(currentSelectedHotbar - 1, 1);

                    //  WorldHelper.instance.StartUpdateAtPoint(blockPoint);
                    AttackAnimate();
                    Invoke("cancelAttackInvoke", 0.16f);
                }
            }
         

        }
    }

    bool  TryInteractWithBlock(VoxelWorldRay voxelRay,out BlockData data)
    {
        Vector3Int result1;
       
        VoxelCast.Cast(voxelRay, 5, out result1, out _);
        if (result1.y <= -1)
        {
            data=new BlockData();
            return false;
        }

        BlockShape resultShape = WorldUpdateablesMediator.instance.GetBlockShape(result1 + new Vector3(0.5f, 0.5f, 0.5f));
        if (resultShape is BlockShape.Door)
        {
            BlockData blockData = WorldUpdateablesMediator.instance.GetBlockData(result1 + new Vector3(0.5f, 0.5f, 0.5f));


            GlobalAudioResourcesManager.PlayClipAtPointCustomRollOff(GlobalGameResourcesManager.instance.audioResourcesManager.TryGetBlockAudioClip(blockData.blockID), result1 + new Vector3(0.5f, 0.5f, 0.5f),
                    1f,40f);
            WorldUpdateablesMediator.instance.SendDoorInteractingOperation(result1);

             

            data = blockData;
            return true;
        }
        data = new BlockData();
        return false;
    }
    bool TryBreakBlock(VoxelWorldRay ray)
    {
        Vector3Int result1;
        BlockFaces resultFaces;

        VoxelCast.Cast(ray, 5, out result1, out resultFaces);


        if (result1.y <= -1)
        {
            return false;
        }
        Vector3 blockPoint = new Vector3(result1.x + 0.5f, result1.y + 0.5f, result1.z + 0.5f);
        inventory.GetItemInfoFromSlot(currentSelectedHotbar - 1, out int itemID, out int itemCount);
        if (itemID == 151)
        {
            //   return;

            WorldUpdateablesMediator.instance.BreakBlockInArea(blockPoint, new Vector3(-1f, -1f, -1f), new Vector3(1f, 1f, 1f));



        }
        else
        {
            BlockData data = WorldUpdateablesMediator.instance.GetBlockData(blockPoint);
            WorldUpdateablesMediator.instance.SendBreakBlockOperation(result1);


        //    WorldHelper.instance.StartUpdateAtPoint(blockPoint,ChunkUpdateTypes.BlockBreakUpdate, data);

        }

        return true;
    }
    bool TryPlaceBlock(short blockID, VoxelWorldRay ray, out Vector3 blockPoint)
    {
        Vector3Int result1;
        BlockFaces resultFaces;
        VoxelCast.Cast(ray, 5, out result1, out resultFaces);

         

        Vector3 resultCenterPoint = new Vector3(result1.x + 0.5f, result1.y + 0.5f, result1.z + 0.5f);
        switch (resultFaces)
        {
            case BlockFaces.NegativeX:
                resultCenterPoint.x -= 1f;
                break;
            case BlockFaces.NegativeY:
                resultCenterPoint.y -= 1f;
                break;
            case BlockFaces.NegativeZ:
                resultCenterPoint.z -= 1f;
                break;
            case BlockFaces.PositiveX:
                resultCenterPoint.x += 1f;
                break;
            case BlockFaces.PositiveY:
                resultCenterPoint.y += 1f;
                break;
            case BlockFaces.PositiveZ:
                resultCenterPoint.z += 1f;
                break;
        }

        Collider[] overlappedColliders = Physics.OverlapBox(resultCenterPoint, new Vector3(0.45f,0.45f,0.45f));
        bool isCollidingWithPlayer=false;
        bool isCollidingWithEntity=false;
        foreach (var item in overlappedColliders)
        {
            if (item.gameObject.tag == "Player")
            {
                isCollidingWithPlayer=true;
            }
            if (item.gameObject.tag == "Entity")
            {
                isCollidingWithEntity = true;
            }
        }
        if (isCollidingWithPlayer ||
            isCollidingWithEntity)
        {
            blockPoint = new Vector3(-1, -1, -1);
            return false;
        }

        BlockShape placingShape = GlobalGameResourcesManager.instance.meshBuildingInfoDataProvider.GetBlockInfo(blockID).shape;
        if (result1.y != -1)
        {
            switch (placingShape)
            {

               
                case BlockShape.Solid:
                    switch (resultFaces)
                    {
                        case BlockFaces.PositiveX:


                            WorldUpdateablesMediator.instance.SendPlaceBlockOperation(new Vector3(result1.x + 1.5f, result1.y + 0.5f, result1.z + 0.5f),
                                (short)blockID);
                            blockPoint = new Vector3(result1.x + 1.5f, result1.y + 0.5f, result1.z + 0.5f);
                            return true;
                            break;
                        case BlockFaces.PositiveY:


                            WorldUpdateablesMediator.instance.SendPlaceBlockOperation(new Vector3(result1.x + 0.5f, result1.y + 1.5f, result1.z + 0.5f),
                                (short)blockID);
                            blockPoint = new Vector3(result1.x + 0.5f, result1.y + 1.5f, result1.z + 0.5f);
                            return true;
                            break;
                        case BlockFaces.PositiveZ:

                            WorldUpdateablesMediator.instance.SendPlaceBlockOperation(new Vector3(result1.x + 0.5f, result1.y + 0.5f, result1.z + 1.5f),
                                (short)blockID);
                            blockPoint = new Vector3(result1.x + 0.5f, result1.y + 0.5f, result1.z + 1.5f);
                            return true;
                            break;
                        case BlockFaces.NegativeX:

                            WorldUpdateablesMediator.instance.SendPlaceBlockOperation(new Vector3(result1.x - 0.5f, result1.y + 0.5f, result1.z + 0.5f),
                                (short)blockID);
                            blockPoint = new Vector3(result1.x - 0.5f, result1.y + 0.5f, result1.z + 0.5f);
                            return true;
                            break;
                        case BlockFaces.NegativeY:

                            WorldUpdateablesMediator.instance.SendPlaceBlockOperation(new Vector3(result1.x + 0.5f, result1.y - 0.5f, result1.z + 0.5f),
                                (short)blockID);
                            blockPoint = new Vector3(result1.x + 0.5f, result1.y - 0.5f, result1.z + 0.5f);
                            return true;
                            break;
                        case BlockFaces.NegativeZ:

                            WorldUpdateablesMediator.instance.SendPlaceBlockOperation(new Vector3(result1.x + 0.5f, result1.y + 0.5f, result1.z - 0.5f),
                                (short)blockID);
                            blockPoint = new Vector3(result1.x + 0.5f, result1.y + 0.5f, result1.z - 0.5f);
                            return true;
                            break;
                    }
                    break;


                case BlockShape.CrossModel:
                    switch (resultFaces)
                    {

                        case BlockFaces.PositiveY:

                            BlockShape? placingPosShape1 =
                                WorldUpdateablesMediator.instance.GetBlockShape(new Vector3(result1.x + 0.5f, result1.y + 1.5f,
                                    result1.z + 0.5f));
                            BlockShape? placingDownPosShape =
                                WorldUpdateablesMediator.instance.GetBlockShape(new Vector3(result1.x + 0.5f, result1.y + 0.5f,
                                    result1.z + 0.5f));
                            if (placingDownPosShape is BlockShape.SolidTransparent ||
                                placingDownPosShape is BlockShape.Solid)
                            {
                                if (placingPosShape1 == null)
                                {
                                    WorldUpdateablesMediator.instance.SendPlaceBlockOperation(new Vector3(result1.x + 0.5f, result1.y + 1.5f, result1.z + 0.5f),
                                        (short)blockID);
                                    blockPoint = new Vector3(result1.x + 0.5f, result1.y + 1.5f, result1.z + 0.5f);
                                    return true;
                                }

                            }
                            else
                            {
                                blockPoint = new Vector3(-1, -1, -1);

                                return false;
                            }

                            break;

                    }
                    break;


                case BlockShape.Door:

                    bool[] dataBools1 = new[] { false, false, false, false, false, false, false, false };

                    switch (resultFaces)
                    {
                        case BlockFaces.PositiveX:
                            blockPoint = new Vector3(-1, -1, -1);
                            return false;

                        case BlockFaces.PositiveY:
                            if (MathF.Abs(ray.direction.x) > MathF.Abs(ray.direction.z))
                            {
                                if (ray.direction.x < 0)
                                {
                                    dataBools1[6] = false;
                                    dataBools1[7] = true;
                                }
                                else
                                {
                                    dataBools1[6] = false;
                                    dataBools1[7] = false;
                                }
                            }
                            else
                            {
                                if (ray.direction.z < 0)
                                {
                                    dataBools1[6] = true;
                                    dataBools1[7] = true;
                                }
                                else
                                {
                                    dataBools1[6] = true;
                                    dataBools1[7] = false;
                                }
                            }
                            break;


                        case BlockFaces.PositiveZ:
                            blockPoint = new Vector3(-1, -1, -1);
                            return false;

                        case BlockFaces.NegativeX:
                            blockPoint = new Vector3(-1, -1, -1);
                            return false;

                        case BlockFaces.NegativeY:
                            blockPoint = new Vector3(-1, -1, -1);
                            return false;

                        case BlockFaces.NegativeZ:
                            blockPoint = new Vector3(-1, -1, -1);
                            return false;

                    }

                    byte optionalDataVal1 = MathUtility.GetByte(dataBools1);
                   
                    if (WorldUpdateablesMediator.instance.GetBlockShape(result1 + new Vector3(0.5f, 2.5f, 0.5f)) == BlockShape.Empty)
                    {
                        WorldUpdateablesMediator.instance.SendPlaceBlockOperation(result1 + new Vector3(0.5f,1.5f, 0.5f), new BlockData(blockID, optionalDataVal1));
                     //   WorldHelper.instance.StartUpdateAtPoint(result1 + new Vector3(0.5f, 1.5f, 0.5f),ChunkUpdateTypes.BlockPlacedUpdate,null);
                        blockPoint = new Vector3(result1.x +0.5f, result1.y + 1.5f, result1.z + 0.5f);

                        return true;
                    }
                  
                    break;

                case BlockShape.SolidTransparent:
                    switch (resultFaces)
                    {
                        case BlockFaces.PositiveX:

                            if (WorldUpdateablesMediator.instance.GetBlockShape(new Vector3(result1.x + 1.5f, result1.y + 0.5f,
                                    result1.z + 0.5f)) is BlockShape.Empty
                               )
                            {
                                WorldUpdateablesMediator.instance.SendPlaceBlockOperation(
                                    new Vector3(result1.x + 1.5f, result1.y + 0.5f, result1.z + 0.5f),
                                    (short)blockID);
                                blockPoint = new Vector3(result1.x + 1.5f, result1.y + 0.5f, result1.z + 0.5f);
                                return true;
                            }
                            else
                            {
                                blockPoint = new Vector3(-1, -1, -1);
                                return false;
                            }

                            break;
                        case BlockFaces.PositiveY:

                            if (WorldUpdateablesMediator.instance.GetBlockShape(new Vector3(result1.x + 0.5f, result1.y + 1.5f,
                                    result1.z + 0.5f)) is BlockShape.Empty
                               )
                            {
                                WorldUpdateablesMediator.instance.SendPlaceBlockOperation(
                                    new Vector3(result1.x + 0.5f, result1.y + 1.5f, result1.z + 0.5f),
                                    (short)blockID);
                                blockPoint = new Vector3(result1.x + 0.5f, result1.y + 1.5f, result1.z + 0.5f);
                                return true;
                            }
                            else
                            {
                                blockPoint = new Vector3(-1, -1, -1);
                                return false;
                            }

                            break;
                        case BlockFaces.PositiveZ:

                            if (WorldUpdateablesMediator.instance.GetBlockShape(new Vector3(result1.x + 0.5f, result1.y + 0.5f,
                                    result1.z + 1.5f)) is BlockShape.Empty
                               )
                            {
                                WorldUpdateablesMediator.instance.SendPlaceBlockOperation(
                                    new Vector3(result1.x + 0.5f, result1.y + 0.5f, result1.z + 1.5f),
                                    (short)blockID);
                                blockPoint = new Vector3(result1.x + 0.5f, result1.y + 0.5f, result1.z + 1.5f);
                                return true;
                            }
                            else
                            {
                                blockPoint = new Vector3(-1, -1, -1);
                                return false;
                            }

                            break;
                        case BlockFaces.NegativeX:
                            if (WorldUpdateablesMediator.instance.GetBlockShape(new Vector3(result1.x - 0.5f, result1.y + 0.5f,
                                    result1.z + 0.5f)) is BlockShape.Empty
                               )
                            {
                                WorldUpdateablesMediator.instance.SendPlaceBlockOperation(
                                    new Vector3(result1.x - 0.5f, result1.y + 0.5f, result1.z + 0.5f),
                                    (short)blockID);
                                blockPoint = new Vector3(result1.x - 0.5f, result1.y + 0.5f, result1.z + 0.5f);
                                return true;
                            }
                            else
                            {
                                blockPoint = new Vector3(-1, -1, -1);
                                return false;
                            }

                            break;
                        case BlockFaces.NegativeY:
                            if (WorldUpdateablesMediator.instance.GetBlockShape(new Vector3(result1.x + 0.5f, result1.y - 0.5f,
                                    result1.z + 0.5f)) is BlockShape.Empty
                               )
                            {
                                WorldUpdateablesMediator.instance.SendPlaceBlockOperation(
                                    new Vector3(result1.x + 0.5f, result1.y - 0.5f, result1.z + 0.5f),
                                    (short)blockID);
                                blockPoint = new Vector3(result1.x + 0.5f, result1.y - 0.5f, result1.z + 0.5f);
                                return true;
                            }
                            else
                            {
                                blockPoint = new Vector3(-1, -1, -1);
                                return false;
                            }

                            break;
                        case BlockFaces.NegativeZ:
                            if (WorldUpdateablesMediator.instance.GetBlockShape(new Vector3(result1.x + 0.5f, result1.y + 0.5f,
                                    result1.z - 0.5f)) is BlockShape.Empty
                               )
                            {
                                WorldUpdateablesMediator.instance.SendPlaceBlockOperation(
                                    new Vector3(result1.x + 0.5f, result1.y + 0.5f, result1.z - 0.5f),
                                    (short)blockID);
                                blockPoint = new Vector3(result1.x + 0.5f, result1.y + 0.5f, result1.z - 0.5f);
                                return true;
                            }
                            else
                            {
                                blockPoint = new Vector3(-1, -1, -1);
                                return false;
                            }

                            break;
                    }
                    break;
                case BlockShape.Fence:
                    switch (resultFaces)
                    {
                        case BlockFaces.PositiveX:

                            if (WorldUpdateablesMediator.instance.GetBlockShape(new Vector3(result1.x + 1.5f, result1.y + 0.5f,
                                    result1.z + 0.5f)) is BlockShape.Empty
                               )
                            {
                                WorldUpdateablesMediator.instance.SendPlaceBlockOperation(
                                    new Vector3(result1.x + 1.5f, result1.y + 0.5f, result1.z + 0.5f),
                                    (short)blockID);
                                blockPoint = new Vector3(result1.x + 1.5f, result1.y + 0.5f, result1.z + 0.5f);
                                return true;
                            }
                            else
                            {
                                blockPoint = new Vector3(-1, -1, -1);
                                return false;
                            }

                            break;
                        case BlockFaces.PositiveY:

                            if (WorldUpdateablesMediator.instance.GetBlockShape(new Vector3(result1.x + 0.5f, result1.y + 1.5f,
                                    result1.z + 0.5f)) is BlockShape.Empty
                               )
                            {
                                WorldUpdateablesMediator.instance.SendPlaceBlockOperation(
                                    new Vector3(result1.x + 0.5f, result1.y + 1.5f, result1.z + 0.5f),
                                    (short)blockID);
                                blockPoint = new Vector3(result1.x + 0.5f, result1.y + 1.5f, result1.z + 0.5f);
                                return true;
                            }
                            else
                            {
                                blockPoint = new Vector3(-1, -1, -1);
                                return false;
                            }

                            break;
                        case BlockFaces.PositiveZ:

                            if (WorldUpdateablesMediator.instance.GetBlockShape(new Vector3(result1.x + 0.5f, result1.y + 0.5f,
                                    result1.z + 1.5f)) is BlockShape.Empty
                               )
                            {
                                WorldUpdateablesMediator.instance.SendPlaceBlockOperation(
                                    new Vector3(result1.x + 0.5f, result1.y + 0.5f, result1.z + 1.5f),
                                    (short)blockID);
                                blockPoint = new Vector3(result1.x + 0.5f, result1.y + 0.5f, result1.z + 1.5f);
                                return true;
                            }
                            else
                            {
                                blockPoint = new Vector3(-1, -1, -1);
                                return false;
                            }

                            break;
                        case BlockFaces.NegativeX:
                            if (WorldUpdateablesMediator.instance.GetBlockShape(new Vector3(result1.x - 0.5f, result1.y + 0.5f,
                                    result1.z + 0.5f)) is BlockShape.Empty
                               )
                            {
                                WorldUpdateablesMediator.instance.SendPlaceBlockOperation(
                                    new Vector3(result1.x - 0.5f, result1.y + 0.5f, result1.z + 0.5f),
                                    (short)blockID);
                                blockPoint = new Vector3(result1.x - 0.5f, result1.y + 0.5f, result1.z + 0.5f);
                                return true;
                            }
                            else
                            {
                                blockPoint = new Vector3(-1, -1, -1);
                                return false;
                            }

                            break;
                        case BlockFaces.NegativeY:
                            if (WorldUpdateablesMediator.instance.GetBlockShape(new Vector3(result1.x + 0.5f, result1.y - 0.5f,
                                    result1.z + 0.5f)) is BlockShape.Empty
                               )
                            {
                                WorldUpdateablesMediator.instance.SendPlaceBlockOperation(
                                    new Vector3(result1.x + 0.5f, result1.y - 0.5f, result1.z + 0.5f),
                                    (short)blockID);
                                blockPoint = new Vector3(result1.x + 0.5f, result1.y - 0.5f, result1.z + 0.5f);
                                return true;
                            }
                            else
                            {
                                blockPoint = new Vector3(-1, -1, -1);
                                return false;
                            }

                            break;
                        case BlockFaces.NegativeZ:
                            if (WorldUpdateablesMediator.instance.GetBlockShape(new Vector3(result1.x + 0.5f, result1.y + 0.5f,
                                    result1.z - 0.5f)) is BlockShape.Empty
                               )
                            {
                                WorldUpdateablesMediator.instance.SendPlaceBlockOperation(
                                    new Vector3(result1.x + 0.5f, result1.y + 0.5f, result1.z - 0.5f),
                                    (short)blockID);
                                blockPoint = new Vector3(result1.x + 0.5f, result1.y + 0.5f, result1.z - 0.5f);
                                return true;
                            }
                            else
                            {
                                blockPoint = new Vector3(-1, -1, -1);
                                return false;
                            }

                            break;
                    }
                    break;
                case BlockShape.Torch:
                    BlockShape? placingPosShape =
                        WorldUpdateablesMediator.instance.GetBlockShape(new Vector3(result1.x + 0.5f, result1.y + 0.5f,
                            result1.z + 0.5f));
                    if (placingPosShape is BlockShape.SolidTransparent || placingPosShape is BlockShape.Solid)
                    {
                        switch (resultFaces)
                        {
                            case BlockFaces.PositiveX:
                                if (WorldUpdateablesMediator.instance.GetBlockShape(new Vector3(result1.x + 1.5f, result1.y + 0.5f,
                                        result1.z + 0.5f)) == BlockShape.Empty)
                                {
                                    WorldUpdateablesMediator.instance.SendPlaceBlockOperation(
                                        new Vector3(result1.x + 1.5f, result1.y + 0.5f, result1.z + 0.5f),
                                        new BlockData(
                                            (short)blockID, 2));
                                    blockPoint = new Vector3(result1.x + 1.5f, result1.y + 0.5f, result1.z + 0.5f);
                                    return true;
                                }

                                break;


                            case BlockFaces.PositiveY:
                                if (WorldUpdateablesMediator.instance.GetBlockShape(new Vector3(result1.x + 0.5f, result1.y + 1.5f,
                                        result1.z + 0.5f)) == BlockShape.Empty)
                                {
                                    WorldUpdateablesMediator.instance.SendPlaceBlockOperation(
                                        new Vector3(result1.x + 0.5f, result1.y + 1.5f, result1.z + 0.5f),
                                        new BlockData(
                                            (short)blockID, 0));
                                    blockPoint = new Vector3(result1.x + 0.5f, result1.y + 1.5f, result1.z + 0.5f);
                                    return true;
                                }

                                break;

                            case BlockFaces.PositiveZ:
                                if (WorldUpdateablesMediator.instance.GetBlockShape(new Vector3(result1.x + 0.5f, result1.y + 0.5f,
                                        result1.z + 1.5f)) == BlockShape.Empty)
                                {
                                    WorldUpdateablesMediator.instance.SendPlaceBlockOperation(
                                        new Vector3(result1.x + 0.5f, result1.y + 0.5f, result1.z + 1.5f),
                                        new BlockData(
                                            (short)blockID, 4));
                                    blockPoint = new Vector3(result1.x + 0.5f, result1.y + 0.5f, result1.z + 1.5f);
                                    return true;
                                }

                                break;

                            case BlockFaces.NegativeX:
                                if (WorldUpdateablesMediator.instance.GetBlockShape(new Vector3(result1.x - 0.5f, result1.y + 0.5f,
                                        result1.z + 0.5f)) == BlockShape.Empty)
                                {
                                    WorldUpdateablesMediator.instance.SendPlaceBlockOperation(
                                        new Vector3(result1.x - 0.5f, result1.y + 0.5f, result1.z + 0.5f),
                                        new BlockData(
                                            (short)blockID, 1));
                                    blockPoint = new Vector3(result1.x - 0.5f, result1.y + 0.5f, result1.z + 0.5f);
                                    return true;
                                }

                                break;

                            case BlockFaces.NegativeY:
                                blockPoint = new Vector3(-1, -1, -1);
                                return false;

                            case BlockFaces.NegativeZ:
                                if (WorldUpdateablesMediator.instance.GetBlockShape(new Vector3(result1.x + 0.5f, result1.y + 0.5f,
                                        result1.z - 0.5f)) == BlockShape.Empty)
                                {
                                    WorldUpdateablesMediator.instance.SendPlaceBlockOperation(
                                        new Vector3(result1.x + 0.5f, result1.y + 0.5f, result1.z - 0.5f),
                                        new BlockData(
                                            (short)blockID, 3));
                                    blockPoint = new Vector3(result1.x + 0.5f, result1.y + 0.5f, result1.z - 0.5f);
                                    return true;
                                }

                                break;
                        }
                    }
                    else
                    {
                        blockPoint = new Vector3(-1, -1, -1);

                        return false;
                    }


                    break;
            }



        }


        blockPoint = new Vector3(-1, -1, -1);
        return false;
    }


}
