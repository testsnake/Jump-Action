//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using Unity.Entities;
//using Unity.NetCode;
//using Unity.Collections;

//public struct ClientMessageRpcCommand : IRpcCommand
//{
//    //Using this because the value can't be null, apparently
//    public FixedString64Bytes message;
//}

//public struct SpawnPlayerRpcCommand : IRpcCommand
//{

//}

//[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
//public partial class ClientSystem : SystemBase
//{
//    protected override void OnCreate()
//    {
//        //Assure entity won't update unless the connection has been established with the server
//        RequireForUpdate<NetworkId>();
//    }
//    protected override void OnUpdate()
//    {
//        var commandBuffer = new EntityCommandBuffer(Allocator.Temp);
//        //This runs when a new RPC is recieved from the server.
//        foreach (var (request, command, entity) in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>, RefRO<ServerMessageRpcCommand>>().WithEntityAccess())
//        {
//            Debug.Log(command.ValueRO.message);
//            //Make sure to destroy the entity so the message isn't read repeatedly!
//            commandBuffer.DestroyEntity(entity);
//        }

//        //if (Input.GetKeyDown(KeyCode.Space))
//        //{
//        //    SendMessageRpc("Hello", ConnectionManager.clientWorld);
//        //}
//        if (Input.GetKeyDown(KeyCode.Space))
//        {
//            SpawnPlayerRpc(ConnectionManager.clientWorld);
//        }

//        commandBuffer.Playback(EntityManager);
//        commandBuffer.Dispose();
//    }

//    public void SendMessageRpc(string text, World world)
//    {
//        //Automatically fail if world doesn't exist yet
//        if(world == null || world.IsCreated == false)
//        {
//            return;
//        }
//        var entity = world.EntityManager.CreateEntity(typeof(SendRpcCommandRequest), typeof(ClientMessageRpcCommand));
//        world.EntityManager.SetComponentData(entity, new ClientMessageRpcCommand() 
//        {
//            message = text
//        });
//    }

//    //This code sends the RPC request to the server to instantiate a player. 
//    public void SpawnPlayerRpc(World world)
//    {
//        if (world == null || world.IsCreated == false) { return; }
//        world.EntityManager.CreateEntity(typeof(SendRpcCommandRequest), typeof(SpawnPlayerRpcCommand));
//    }
//}
