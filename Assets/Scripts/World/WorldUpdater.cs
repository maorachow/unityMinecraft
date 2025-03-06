using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
 
using UnityEngine;
using Debug = UnityEngine.Debug;


namespace monogameMinecraftShared.World
{
    public class WorldUpdater
    {
        public VoxelWorld world;
        public WorldUpdater(VoxelWorld world)
        {
            this.world = world;
        }

        public void Init()
        {
            
            queuedChunkUpdatePoints = new Queue<IChunkUpdateOperation>();
            chunksNeededRebuild = new List<Chunk>();
            tryUpdateWorldBlocksThread = new Thread(UpdateWorldBlocksThread);
            tryUpdateWorldBlocksThread.IsBackground = true;
            tryUpdateWorldBlocksThread.Start();
        }
        public object chunksNeededRebuildListLock = new object();
        public void UpdateWorldBlocksThread()
        {

            while (true)
            {
                if (VoxelWorld.currentWorld.isGoingToQuitWorld || VoxelWorld.currentWorld.worldID != world.worldID)
                {
                    Debug.Log("quit update world block thread");
                    return;
                }
                Thread.Sleep(25);

                //    Debug.WriteLine("sleep");

                lock (chunksNeededRebuildListLock)
                {
                    //       Debug.WriteLine("count: "+queuedChunkUpdatePoints.Count);
                    int processedUpdateCount = 0;
                    while (queuedChunkUpdatePoints.Count > 0&&processedUpdateCount< maxChunkUpdateCountOnce)
                    {
                        if (queuedChunkUpdatePoints.Count > 0)
                        {
                            IChunkUpdateOperation updateOper = queuedChunkUpdatePoints.Dequeue();
                            updateOper.Update();
                            chunksNeededRebuild.Add(Chunk.GetChunk(ChunkCoordsHelper.Vec3ToChunkPos((Vector3)updateOper.position)));
                        }

                        processedUpdateCount++;
                    }
                   
                }




            }





        }
        public Queue<IChunkUpdateOperation> queuedChunkUpdatePoints;
        public Thread tryUpdateWorldBlocksThread;

        public List<Chunk> chunksNeededRebuild;


        public readonly float maxDelayedTime = 0.1f;
        public readonly int maxChunkUpdateCountOnce = 100;
        public float delayedTime = 0f;

        public delegate void OnChunkUpdated();

        private OnChunkUpdated onUpdated;

        public OnChunkUpdated onUpdatedOneShot;

        public ConcurrentQueue<Action> chunkRebuildActions = new ConcurrentQueue<Action>();
        public void MainThreadUpdate(float deltaTime)
        {
            while (chunkRebuildActions.Count > 0)
            {

                Action result;
                var item = chunkRebuildActions.TryDequeue(out result);
                if (result != null && item == true)
                {
                    result();
                }
            }
            delayedTime += deltaTime;
            if (delayedTime > maxDelayedTime)
            {
                delayedTime = 0f;
                lock (chunksNeededRebuildListLock)
                {
                    if (chunksNeededRebuild.Count > 0)
                    {
                        //     Debug.WriteLine("rebuild");
                        foreach (var chunk in chunksNeededRebuild)
                        {
                            if (chunk != null && chunk.isMapGenCompleted == true)
                            {
                                chunk.isChunkMapUpdated=true;


                                if (Chunk.GetChunk(
                                        new Vector2Int(chunk.chunkPos.x - Chunk.chunkWidth, chunk.chunkPos.y)) != null)
                                {
                                    Chunk.GetChunk(
                                            new Vector2Int(chunk.chunkPos.x - Chunk.chunkWidth, chunk.chunkPos.y))
                                        .isChunkMapUpdated = true;
                                }


                                // if (chunkNeededUpdate.rightChunk != null && chunkNeededUpdate.rightChunk.isMapGenCompleted == true)

                                //  chunkNeededUpdate.rightChunk.BuildChunk();
                                if (Chunk.GetChunk(
                                        new Vector2Int(chunk.chunkPos.x + Chunk.chunkWidth, chunk.chunkPos.y)))
                                {
                                    Chunk.GetChunk(
                                            new Vector2Int(chunk.chunkPos.x + Chunk.chunkWidth, chunk.chunkPos.y))
                                        .isChunkMapUpdated = true;
                                }


                                //  if (chunkNeededUpdate.backChunk != null && chunkNeededUpdate.backChunk.isMapGenCompleted == true)
                                //       {
                                //         chunkNeededUpdate.backChunk.BuildChunk();
                                //     }
                                if (Chunk.GetChunk(
                                        new Vector2Int(chunk.chunkPos.x, chunk.chunkPos.y - Chunk.chunkWidth)) !=
                                    null)
                                {
                                    Chunk.GetChunk(
                                            new Vector2Int(chunk.chunkPos.x, chunk.chunkPos.y - Chunk.chunkWidth))
                                        .isChunkMapUpdated = true;
                                }


                                //   if (chunkNeededUpdate.frontChunk != null && chunkNeededUpdate.frontChunk.isMapGenCompleted == true)
                                //     {
                                //      chunkNeededUpdate.frontChunk.BuildChunk();
                                //     }
                                if (Chunk.GetChunk(
                                        new Vector2Int(chunk.chunkPos.x, chunk.chunkPos.y + Chunk.chunkWidth)) != null)
                                {
                                    Chunk.GetChunk(
                                            new Vector2Int(chunk.chunkPos.x, chunk.chunkPos.y + Chunk.chunkWidth))
                                        .isChunkMapUpdated = true;
                                }
                             

                            }

                        }


                      //  onUpdated();




                    }

                    chunksNeededRebuild.Clear();

                }


            }

            if (onUpdatedOneShot != null)
            {
                onUpdatedOneShot();
                Delegate[] dels = onUpdatedOneShot.GetInvocationList();
                foreach (var del in dels)
                {
                    onUpdatedOneShot -= del as OnChunkUpdated;
                }
            }
        }

        public void StopAllThreads()
        {
            tryUpdateWorldBlocksThread.Join();
        }
    }
}
