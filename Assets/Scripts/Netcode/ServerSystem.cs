//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using Unity.Entities;
//using Unity.NetCode;
//using Unity.Collections;
//using static PrefabsManager;
//using Unity.Transforms;

//public struct InitializedClient : IComponentData
//{

//}

//public struct ServerMessageRpcCommand : IRpcCommand
//{
//    //Using this because the value can't be null, apparently
//    public FixedString64Bytes message;
//}

//[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
//public partial class ServerSystem : SystemBase
//{
//    //Needed for grabbing network ID of clients
//    private ComponentLookup<NetworkId> clients;

//    protected override void OnCreate()
//    {
//        clients = GetComponentLookup<NetworkId>(true);
//    }

//    protected override void OnUpdate()
//    {
//        clients.Update(this);
//        var commandBuffer = new EntityCommandBuffer(Allocator.Temp);

//        //This runs when an RPC request is recieved from the client.
//        foreach (var (request, command, entity) in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>, RefRO<ClientMessageRpcCommand>>().WithEntityAccess())
//        {
//            Debug.Log(command.ValueRO.message + " from client index " + request.ValueRO.SourceConnection.Index + " version " + request.ValueRO.SourceConnection.Version);
//            //Make sure to destroy the entity so the message isn't read repeatedly!
//            commandBuffer.DestroyEntity(entity);
//        }

//        //Runs when the server receives a request to spawn a player
//        //foreach (var (request, command, entity) in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>, RefRO<SpawnPlayerRpcCommand>>().WithEntityAccess())
//        //{
//        //    PrefabsData prefabs;
//        //    if (SystemAPI.TryGetSingleton<PrefabsData>(out prefabs) && prefabs.player != null)
//        //    {
//        //        Entity player = commandBuffer.Instantiate(prefabs.player);

//        //        //Set the position to instantiate the player at. Should tweak later to set spawn points.
//        //        commandBuffer.SetComponent(player, new LocalTransform()
//        //        {
//        //            Position = new Unity.Mathematics.float3(UnityEngine.Random.Range(-10f, 10f), 0, UnityEngine.Random.Range(-10f, 10f)),
//        //            Rotation = Unity.Mathematics.quaternion.identity,
//        //            Scale = 1f
//        //        });

//        //        //Sets owner of the newly instantiated player prefab to be the client who instantiated it.
//        //        var networkId = clients[request.ValueRO.SourceConnection];
//        //        commandBuffer.SetComponent(player, new GhostOwner()
//        //        {
//        //            NetworkId = networkId.Value
//        //        });

//        //        //Links spawned players to the client that spawned them. Meaning, if a client disconnects, their corresponding player will be destroyed as well.
//        //        //tl;dr makes players despawn on D/C
//        //        commandBuffer.AppendToBuffer(request.ValueRO.SourceConnection, new LinkedEntityGroup()
//        //        {
//        //            Value = player
//        //        });

//        //        commandBuffer.DestroyEntity(entity);
//        //    }
//        //}

//        //Detects whenever a client is connected, and adds the InitializedClient component to them.
//        foreach (var (id, entity) in SystemAPI.Query<RefRO<NetworkId>>().WithNone<InitializedClient>().WithEntityAccess())
//        {
//            commandBuffer.AddComponent<InitializedClient>(entity);
//            //Broadcast new client connected message to all clients
//            SendMessageRpc("Client connected with id = " + id.ValueRO.Value, ConnectionManager.serverWorld);
            
//            //Instantiate a new player
//            PrefabsData prefabManager = SystemAPI.GetSingleton<PrefabsData>();
//            if (prefabManager.player != null)
//            {
//                Entity player = commandBuffer.Instantiate(prefabManager.player);
//                //Set the position to instantiate the player at. Should tweak later to set spawn points.
//                commandBuffer.SetComponent(player, new LocalTransform()
//                {
//                    Position = new Unity.Mathematics.float3(UnityEngine.Random.Range(-10f, 10f), 1.4f, UnityEngine.Random.Range(-10f, 10f)),
//                    Rotation = Unity.Mathematics.quaternion.identity,
//                    Scale = 1f
//                });
//                //Sets owner of the newly instantiated player prefab to be the client who instantiated it.
//                commandBuffer.SetComponent(player, new GhostOwner()
//                {
//                    NetworkId = id.ValueRO.Value
//                });
//                //Links spawned players to the client that spawned them. Meaning, if a client disconnects, their corresponding player will be destroyed as well.
//                //tl;dr makes players despawn on D/C
//                commandBuffer.AppendToBuffer(entity, new LinkedEntityGroup()
//                {
//                    Value = player
//                });
//            }
//        }
//        commandBuffer.Playback(EntityManager);
//        commandBuffer.Dispose();
//    }

//    //If a target is not specified, broadcasts to all clients.
//    public void SendMessageRpc(string text, World world, Entity target = default)
//    {
//        //Automatically fail if world doesn't exist yet
//        if (world == null || world.IsCreated == false)
//        {
//            return;
//        }
//        var entity = world.EntityManager.CreateEntity(typeof(SendRpcCommandRequest), typeof(ServerMessageRpcCommand));
//        world.EntityManager.SetComponentData(entity, new ServerMessageRpcCommand()
//        {
//            message = text
//        });
//        if (target != Entity.Null)
//        {
//            world.EntityManager.SetComponentData(entity, new SendRpcCommandRequest()
//            {
//                TargetConnection = target
//            });
//        }
//    }
//}
