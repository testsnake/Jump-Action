using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.NetCode;
using Unity.Collections;
using UnityEditor.VersionControl;
using UnityEngine.Rendering;

public struct ClientMessageRpcCommand : IRpcCommand
{
    //Using this because the value can't be null, apparently
    public FixedString64Bytes message;
}

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial class ClientSystem : SystemBase
{
    protected override void OnCreate()
    {
        //Assure entity won't update unless the connection has been established with the server
        RequireForUpdate<NetworkId>();
    }
    protected override void OnUpdate()
    {
        var commandBuffer = new EntityCommandBuffer(Allocator.Temp);
        //This runs when a new RPC is recieved from the server.
        foreach (var (request, command, entity) in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>, RefRO<ServerMessageRpcCommand>>().WithEntityAccess())
        {
            Debug.Log(command.ValueRO.message);
            //Make sure to destroy the entity so the message isn't read repeatedly!
            commandBuffer.DestroyEntity(entity);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            SendMessageRpc("Hello", ConnectionManager.clientWorld);
        }

        commandBuffer.Playback(EntityManager);
        commandBuffer.Dispose();
    }

    public void SendMessageRpc(string text, World world)
    {
        //Automatically fail if world doesn't exist yet
        if(world == null || world.IsCreated == false)
        {
            return;
        }
        var entity = world.EntityManager.CreateEntity(typeof(SendRpcCommandRequest), typeof(ClientMessageRpcCommand));
        world.EntityManager.SetComponentData(entity, new ClientMessageRpcCommand() 
        {
            message = text
        });
    }
}
