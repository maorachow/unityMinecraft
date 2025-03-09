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
        GlobalAudioResourcesManager.PlayClipAtPointCustomRollOff(GlobalAudioResourcesManager.TryGetEntityAudioClip("playerSweepAttackClip"),attackEffectPoint,1,40f);
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
            EntityBeh arrow = EntityBeh.SpawnNewEntity(arrowPos.x, arrowPos.y, arrowPos.z, 4, playerHeadCenterRef.forward);
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
           AS.PlayOneShot(GlobalAudioResourcesManager.TryGetEntityAudioClip("playerShootClip"));

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
         
            if (inventoryDic[currentSelectedHotbar - 1] == 152)
            {
                go.GetComponent<IAttackableEntityTarget>()
                    .ApplyDamageAndKnockback(7f, (transform.position - go.transform.position).normalized * -20f);
            }
            else if (inventoryDic[currentSelectedHotbar - 1] == 151)
            {
                go.GetComponent<IAttackableEntityTarget>()
                    .ApplyDamageAndKnockback(5f, (transform.position - go.transform.position).normalized * -20f);
            }
            else
            {
                go.GetComponent<IAttackableEntityTarget>()
                    .ApplyDamageAndKnockback(1f, (transform.position - go.transform.position).normalized * -10f);
            }

            if (go.GetComponent<IAttackableEntityTarget>().primaryAttackerEntities != null)
            {
                go.GetComponent<IAttackableEntityTarget>()
                    .primaryAttackerEntities.Add(this);
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
            inventoryItemNumberDic[currentSelectedHotbar - 1]--;
            GameUIBeh.instance.UpdateBlockOnHandText();
            PlayerEatAnimate();
            for (int i = 0; i < 3; i++)
            {
                await UniTask.Delay(300);
                AS.PlayOneShot(GlobalAudioResourcesManager.TryGetEntityAudioClip("playerEatClip"));
            }

            Invoke("PlayerCancelEatAnimateInvoke", 0f);
            playerHealth += 4f;
            playerHealth = Mathf.Clamp(playerHealth, 0f, 20f);

            GameUIBeh.instance.PlayerHealthSliderOnValueChanged(playerHealth);

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
        EntityBeh tntEntity = EntityBeh.SpawnNewEntity(tntPos.x, tntPos.y, tntPos.z, 2);
        await UniTask.WaitUntil(() => tntEntity.GetComponent<TNTBeh>().GetComponent<Rigidbody>() != null);
        tntEntity.GetComponent<TNTBeh>().AddForce((headPos.forward * 16));
        inventoryItemNumberDic[currentSelectedHotbar - 1]--;
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
        AS.PlayOneShot(GlobalAudioResourcesManager.TryGetEntityAudioClip("equipDiamondClip"));
        GameUIBeh.instance.PlayerArmorPointsSliderOnValueChanged(playerArmorPoints);
        inventoryItemNumberDic[currentSelectedHotbar - 1]--;
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
        if (inventoryDic[currentSelectedHotbar - 1] == 154)
        {
            PlayerEat();
            return;
        }

        if (inventoryDic[currentSelectedHotbar - 1] == 156)
        {
            ThrowTNT();
            return;
        }

        if (inventoryDic[currentSelectedHotbar - 1] == 158)
        {
            UpgradeArmor();
            return;
        }

        if (critAttackCD <= 0f)
        {
            if (inventoryDic[currentSelectedHotbar - 1] == 157)
            {
                PlayerCritAttackBow();
                critAttackCD = 2.3f;
                return;
            }
        }
        
        if (Physics.Raycast(ray, out info, 5f, ~LayerMask.GetMask("Ignore Raycast")) && info.collider.gameObject.tag == "Entity" && critAttackCD <= 0f)
        {
            if (inventoryDic[currentSelectedHotbar - 1] == 152)
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
            bool isBlockPlaced = TryPlaceBlock((short)inventoryDic[currentSelectedHotbar - 1], blockRay, out Vector3 blockPoint);

            if (isBlockPlaced)
            {
                GlobalAudioResourcesManager.PlayClipAtPointCustomRollOff(GlobalAudioResourcesManager.TryGetBlockAudioClip(inventoryDic[currentSelectedHotbar - 1]),transform.position,1f,40f);
                

                inventoryItemNumberDic[currentSelectedHotbar - 1]--;

              //  WorldHelper.instance.StartUpdateAtPoint(blockPoint);
                AttackAnimate();
                Invoke("cancelAttackInvoke", 0.16f);
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

        BlockShape? resultShape = WorldHelper.instance.GetBlockShape(result1 + new Vector3(0.5f, 0.5f, 0.5f));
        if (resultShape is BlockShape.Door)
        {
            BlockData blockData = WorldHelper.instance.GetBlockData(result1 + new Vector3(0.5f, 0.5f, 0.5f));

            
                GlobalAudioResourcesManager.PlayClipAtPointCustomRollOff(GlobalAudioResourcesManager.TryGetBlockAudioClip(blockData.blockID), result1 + new Vector3(0.5f, 0.5f, 0.5f),
                    1f,40f);
            

            VoxelWorld.currentWorld.worldUpdater.queuedChunkUpdatePoints.Enqueue(new DoorInteractingOperation(result1));

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
        if (inventoryDic[currentSelectedHotbar - 1] == 151)
        {
            //   return;

            WorldHelper.instance.BreakBlockInArea(blockPoint, new Vector3(-1f, -1f, -1f), new Vector3(1f, 1f, 1f));



        }
        else
        {
            BlockData data = WorldHelper.instance.GetBlockData(blockPoint);
            WorldHelper.instance.SendBreakBlockOperation(result1);


        //    WorldHelper.instance.StartUpdateAtPoint(blockPoint,ChunkUpdateTypes.BlockBreakUpdate, data);

        }

        return true;
    }
    bool TryPlaceBlock(short blockID, VoxelWorldRay ray, out Vector3 blockPoint)
    {
        Vector3Int result1;
        BlockFaces resultFaces;
        VoxelCast.Cast(ray, 5, out result1, out resultFaces);


        if (!ItemIDToBlockID.ItemIDToBlockIDDic.ContainsKey(inventoryDic[currentSelectedHotbar - 1]) ||
            ItemIDToBlockID.ItemIDToBlockIDDic[inventoryDic[currentSelectedHotbar - 1]] == -1)
        {
            blockPoint = new Vector3(-1, -1, -1);
            return false;
        }

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

        BlockShape placingShape = Chunk
            .blockInfosNew[ItemIDToBlockID.ItemIDToBlockIDDic[inventoryDic[currentSelectedHotbar - 1]]].shape;
        if (result1.y != -1)
        {
            switch (placingShape)
            {

               
                case BlockShape.Solid:
                    switch (resultFaces)
                    {
                        case BlockFaces.PositiveX:


                            WorldHelper.instance.SendPlaceBlockOperation(new Vector3(result1.x + 1.5f, result1.y + 0.5f, result1.z + 0.5f),
                                (short)ItemIDToBlockID.ItemIDToBlockIDDic[inventoryDic[currentSelectedHotbar - 1]]);
                            blockPoint = new Vector3(result1.x + 1.5f, result1.y + 0.5f, result1.z + 0.5f);
                            return true;
                            break;
                        case BlockFaces.PositiveY:


                            WorldHelper.instance.SendPlaceBlockOperation(new Vector3(result1.x + 0.5f, result1.y + 1.5f, result1.z + 0.5f),
                                (short)ItemIDToBlockID.ItemIDToBlockIDDic[inventoryDic[currentSelectedHotbar - 1]]);
                            blockPoint = new Vector3(result1.x + 0.5f, result1.y + 1.5f, result1.z + 0.5f);
                            return true;
                            break;
                        case BlockFaces.PositiveZ:

                            WorldHelper.instance.SendPlaceBlockOperation(new Vector3(result1.x + 0.5f, result1.y + 0.5f, result1.z + 1.5f),
                                (short)ItemIDToBlockID.ItemIDToBlockIDDic[inventoryDic[currentSelectedHotbar - 1]]);
                            blockPoint = new Vector3(result1.x + 0.5f, result1.y + 0.5f, result1.z + 1.5f);
                            return true;
                            break;
                        case BlockFaces.NegativeX:

                            WorldHelper.instance.SendPlaceBlockOperation(new Vector3(result1.x - 0.5f, result1.y + 0.5f, result1.z + 0.5f),
                                (short)ItemIDToBlockID.ItemIDToBlockIDDic[inventoryDic[currentSelectedHotbar - 1]]);
                            blockPoint = new Vector3(result1.x - 0.5f, result1.y + 0.5f, result1.z + 0.5f);
                            return true;
                            break;
                        case BlockFaces.NegativeY:

                            WorldHelper.instance.SendPlaceBlockOperation(new Vector3(result1.x + 0.5f, result1.y - 0.5f, result1.z + 0.5f),
                                (short)ItemIDToBlockID.ItemIDToBlockIDDic[inventoryDic[currentSelectedHotbar - 1]]);
                            blockPoint = new Vector3(result1.x + 0.5f, result1.y - 0.5f, result1.z + 0.5f);
                            return true;
                            break;
                        case BlockFaces.NegativeZ:

                            WorldHelper.instance.SendPlaceBlockOperation(new Vector3(result1.x + 0.5f, result1.y + 0.5f, result1.z - 0.5f),
                                (short)ItemIDToBlockID.ItemIDToBlockIDDic[inventoryDic[currentSelectedHotbar - 1]]);
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
                                WorldHelper.instance.GetBlockShape(new Vector3(result1.x + 0.5f, result1.y + 1.5f,
                                    result1.z + 0.5f));
                            BlockShape? placingDownPosShape =
                                WorldHelper.instance.GetBlockShape(new Vector3(result1.x + 0.5f, result1.y + 0.5f,
                                    result1.z + 0.5f));
                            if (placingDownPosShape is BlockShape.SolidTransparent ||
                                placingDownPosShape is BlockShape.Solid)
                            {
                                if (placingPosShape1 == null)
                                {
                                    WorldHelper.instance.SendPlaceBlockOperation(new Vector3(result1.x + 0.5f, result1.y + 1.5f, result1.z + 0.5f),
                                        (short)ItemIDToBlockID.ItemIDToBlockIDDic[inventoryDic[currentSelectedHotbar - 1]]);
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
                   
                    if (WorldHelper.instance.GetBlock(result1 + new Vector3(0.5f, 2.5f, 0.5f)) == 0)
                    {
                        WorldHelper.instance.SendPlaceBlockOperation(result1 + new Vector3(0.5f,1.5f, 0.5f), new BlockData(ItemIDToBlockID.ItemIDToBlockIDDic[inventoryDic[currentSelectedHotbar - 1]], optionalDataVal1));
                     //   WorldHelper.instance.StartUpdateAtPoint(result1 + new Vector3(0.5f, 1.5f, 0.5f),ChunkUpdateTypes.BlockPlacedUpdate,null);
                        blockPoint = new Vector3(result1.x +0.5f, result1.y + 1.5f, result1.z + 0.5f);

                        return true;
                    }
                  
                    break;

                case BlockShape.SolidTransparent:
                    switch (resultFaces)
                    {
                        case BlockFaces.PositiveX:

                            if (WorldHelper.instance.GetBlockShape(new Vector3(result1.x + 1.5f, result1.y + 0.5f,
                                    result1.z + 0.5f)) is null
                               )
                            {
                                WorldHelper.instance.SendPlaceBlockOperation(
                                    new Vector3(result1.x + 1.5f, result1.y + 0.5f, result1.z + 0.5f),
                                    (short)ItemIDToBlockID.ItemIDToBlockIDDic[inventoryDic[currentSelectedHotbar - 1]]);
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

                            if (WorldHelper.instance.GetBlockShape(new Vector3(result1.x + 0.5f, result1.y + 1.5f,
                                    result1.z + 0.5f)) is null
                               )
                            {
                                WorldHelper.instance.SendPlaceBlockOperation(
                                    new Vector3(result1.x + 0.5f, result1.y + 1.5f, result1.z + 0.5f),
                                    (short)ItemIDToBlockID.ItemIDToBlockIDDic[inventoryDic[currentSelectedHotbar - 1]]);
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

                            if (WorldHelper.instance.GetBlockShape(new Vector3(result1.x + 0.5f, result1.y + 0.5f,
                                    result1.z + 1.5f)) is null
                               )
                            {
                                WorldHelper.instance.SendPlaceBlockOperation(
                                    new Vector3(result1.x + 0.5f, result1.y + 0.5f, result1.z + 1.5f),
                                    (short)ItemIDToBlockID.ItemIDToBlockIDDic[inventoryDic[currentSelectedHotbar - 1]]);
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
                            if (WorldHelper.instance.GetBlockShape(new Vector3(result1.x - 0.5f, result1.y + 0.5f,
                                    result1.z + 0.5f)) is null
                               )
                            {
                                WorldHelper.instance.SendPlaceBlockOperation(
                                    new Vector3(result1.x - 0.5f, result1.y + 0.5f, result1.z + 0.5f),
                                    (short)ItemIDToBlockID.ItemIDToBlockIDDic[inventoryDic[currentSelectedHotbar - 1]]);
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
                            if (WorldHelper.instance.GetBlockShape(new Vector3(result1.x + 0.5f, result1.y - 0.5f,
                                    result1.z + 0.5f)) is null
                               )
                            {
                                WorldHelper.instance.SendPlaceBlockOperation(
                                    new Vector3(result1.x + 0.5f, result1.y - 0.5f, result1.z + 0.5f),
                                    (short)ItemIDToBlockID.ItemIDToBlockIDDic[inventoryDic[currentSelectedHotbar - 1]]);
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
                            if (WorldHelper.instance.GetBlockShape(new Vector3(result1.x + 0.5f, result1.y + 0.5f,
                                    result1.z - 0.5f)) is null
                               )
                            {
                                WorldHelper.instance.SendPlaceBlockOperation(
                                    new Vector3(result1.x + 0.5f, result1.y + 0.5f, result1.z - 0.5f),
                                    (short)ItemIDToBlockID.ItemIDToBlockIDDic[inventoryDic[currentSelectedHotbar - 1]]);
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

                            if (WorldHelper.instance.GetBlockShape(new Vector3(result1.x + 1.5f, result1.y + 0.5f,
                                    result1.z + 0.5f)) is null
                               )
                            {
                                WorldHelper.instance.SendPlaceBlockOperation(
                                    new Vector3(result1.x + 1.5f, result1.y + 0.5f, result1.z + 0.5f),
                                    (short)ItemIDToBlockID.ItemIDToBlockIDDic[inventoryDic[currentSelectedHotbar - 1]]);
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

                            if (WorldHelper.instance.GetBlockShape(new Vector3(result1.x + 0.5f, result1.y + 1.5f,
                                    result1.z + 0.5f)) is null
                               )
                            {
                                WorldHelper.instance.SendPlaceBlockOperation(
                                    new Vector3(result1.x + 0.5f, result1.y + 1.5f, result1.z + 0.5f),
                                    (short)ItemIDToBlockID.ItemIDToBlockIDDic[inventoryDic[currentSelectedHotbar - 1]]);
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

                            if (WorldHelper.instance.GetBlockShape(new Vector3(result1.x + 0.5f, result1.y + 0.5f,
                                    result1.z + 1.5f)) is null
                               )
                            {
                                WorldHelper.instance.SendPlaceBlockOperation(
                                    new Vector3(result1.x + 0.5f, result1.y + 0.5f, result1.z + 1.5f),
                                    (short)ItemIDToBlockID.ItemIDToBlockIDDic[inventoryDic[currentSelectedHotbar - 1]]);
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
                            if (WorldHelper.instance.GetBlockShape(new Vector3(result1.x - 0.5f, result1.y + 0.5f,
                                    result1.z + 0.5f)) is null
                               )
                            {
                                WorldHelper.instance.SendPlaceBlockOperation(
                                    new Vector3(result1.x - 0.5f, result1.y + 0.5f, result1.z + 0.5f),
                                    (short)ItemIDToBlockID.ItemIDToBlockIDDic[inventoryDic[currentSelectedHotbar - 1]]);
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
                            if (WorldHelper.instance.GetBlockShape(new Vector3(result1.x + 0.5f, result1.y - 0.5f,
                                    result1.z + 0.5f)) is null
                               )
                            {
                                WorldHelper.instance.SendPlaceBlockOperation(
                                    new Vector3(result1.x + 0.5f, result1.y - 0.5f, result1.z + 0.5f),
                                    (short)ItemIDToBlockID.ItemIDToBlockIDDic[inventoryDic[currentSelectedHotbar - 1]]);
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
                            if (WorldHelper.instance.GetBlockShape(new Vector3(result1.x + 0.5f, result1.y + 0.5f,
                                    result1.z - 0.5f)) is null
                               )
                            {
                                WorldHelper.instance.SendPlaceBlockOperation(
                                    new Vector3(result1.x + 0.5f, result1.y + 0.5f, result1.z - 0.5f),
                                    (short)ItemIDToBlockID.ItemIDToBlockIDDic[inventoryDic[currentSelectedHotbar - 1]]);
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
                        WorldHelper.instance.GetBlockShape(new Vector3(result1.x + 0.5f, result1.y + 0.5f,
                            result1.z + 0.5f));
                    if (placingPosShape is BlockShape.SolidTransparent || placingPosShape is BlockShape.Solid)
                    {
                        switch (resultFaces)
                        {
                            case BlockFaces.PositiveX:
                                if (WorldHelper.instance.GetBlockShape(new Vector3(result1.x + 1.5f, result1.y + 0.5f,
                                        result1.z + 0.5f)) == null)
                                {
                                    WorldHelper.instance.SendPlaceBlockOperation(
                                        new Vector3(result1.x + 1.5f, result1.y + 0.5f, result1.z + 0.5f),
                                        new BlockData(
                                            (short)ItemIDToBlockID.ItemIDToBlockIDDic[
                                                inventoryDic[currentSelectedHotbar - 1]], 2));
                                    blockPoint = new Vector3(result1.x + 1.5f, result1.y + 0.5f, result1.z + 0.5f);
                                    return true;
                                }

                                break;


                            case BlockFaces.PositiveY:
                                if (WorldHelper.instance.GetBlockShape(new Vector3(result1.x + 0.5f, result1.y + 1.5f,
                                        result1.z + 0.5f)) == null)
                                {
                                    WorldHelper.instance.SendPlaceBlockOperation(
                                        new Vector3(result1.x + 0.5f, result1.y + 1.5f, result1.z + 0.5f),
                                        new BlockData(
                                            (short)ItemIDToBlockID.ItemIDToBlockIDDic[
                                                inventoryDic[currentSelectedHotbar - 1]], 0));
                                    blockPoint = new Vector3(result1.x + 0.5f, result1.y + 1.5f, result1.z + 0.5f);
                                    return true;
                                }

                                break;

                            case BlockFaces.PositiveZ:
                                if (WorldHelper.instance.GetBlockShape(new Vector3(result1.x + 0.5f, result1.y + 0.5f,
                                        result1.z + 1.5f)) == null)
                                {
                                    WorldHelper.instance.SendPlaceBlockOperation(
                                        new Vector3(result1.x + 0.5f, result1.y + 0.5f, result1.z + 1.5f),
                                        new BlockData(
                                            (short)ItemIDToBlockID.ItemIDToBlockIDDic[
                                                inventoryDic[currentSelectedHotbar - 1]], 4));
                                    blockPoint = new Vector3(result1.x + 0.5f, result1.y + 0.5f, result1.z + 1.5f);
                                    return true;
                                }

                                break;

                            case BlockFaces.NegativeX:
                                if (WorldHelper.instance.GetBlockShape(new Vector3(result1.x - 0.5f, result1.y + 0.5f,
                                        result1.z + 0.5f)) == null)
                                {
                                    WorldHelper.instance.SendPlaceBlockOperation(
                                        new Vector3(result1.x - 0.5f, result1.y + 0.5f, result1.z + 0.5f),
                                        new BlockData(
                                            (short)ItemIDToBlockID.ItemIDToBlockIDDic[
                                                inventoryDic[currentSelectedHotbar - 1]], 1));
                                    blockPoint = new Vector3(result1.x - 0.5f, result1.y + 0.5f, result1.z + 0.5f);
                                    return true;
                                }

                                break;

                            case BlockFaces.NegativeY:
                                blockPoint = new Vector3(-1, -1, -1);
                                return false;

                            case BlockFaces.NegativeZ:
                                if (WorldHelper.instance.GetBlockShape(new Vector3(result1.x + 0.5f, result1.y + 0.5f,
                                        result1.z - 0.5f)) == null)
                                {
                                    WorldHelper.instance.SendPlaceBlockOperation(
                                        new Vector3(result1.x + 0.5f, result1.y + 0.5f, result1.z - 0.5f),
                                        new BlockData(
                                            (short)ItemIDToBlockID.ItemIDToBlockIDDic[
                                                inventoryDic[currentSelectedHotbar - 1]], 3));
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
