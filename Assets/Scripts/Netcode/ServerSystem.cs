using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.NetCode;
using Unity.Collections;

public struct InitializedClient : IComponentData
{

}

public struct ServerMessageRpcCommand : IRpcCommand
{
    //Using this because the value can't be null, apparently
    public FixedString64Bytes message;
}

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial class ServerSystem : SystemBase
{
    //Needed for grabbing network ID of clients
    private ComponentLookup<NetworkId> clients;

    protected override void OnCreate()
    {
        clients = GetComponentLookup<NetworkId>(true);
    }

    protected override void OnUpdate()
    {
        clients.Update(this);
        var commandBuffer = new EntityCommandBuffer(Allocator.Temp);

        //This runs when an RPC request is recieved from the client.
        foreach (var (request, command, entity) in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>, RefRO<ClientMessageRpcCommand>>().WithEntityAccess())
        {
            Debug.Log(command.ValueRO.message + " from client index " + request.ValueRO.SourceConnection.Index + " version " + request.ValueRO.SourceConnection.Version);
            //Make sure to destroy the entity so the message isn't read repeatedly!
            commandBuffer.DestroyEntity(entity);
        }

        //Detects whenever a client is connected, and adds the InitializedClient component to them.
        foreach (var (id, entity) in SystemAPI.Query<RefRO<NetworkId>>().WithNone<InitializedClient>().WithEntityAccess())
        {
            commandBuffer.AddComponent<InitializedClient>(entity);
            //Broadcast new client connected message to all clients
            SendMessageRpc("Client connected with id = " + id.ValueRO.Value, ConnectionManager.serverWorld);
        }
        commandBuffer.Playback(EntityManager);
        commandBuffer.Dispose();
    }

    //If a target is not specified, broadcasts to all clients.
    public void SendMessageRpc(string text, World world, Entity target = default)
    {
        //Automatically fail if world doesn't exist yet
        if (world == null || world.IsCreated == false)
        {
            return;
        }
        var entity = world.EntityManager.CreateEntity(typeof(SendRpcCommandRequest), typeof(ServerMessageRpcCommand));
        world.EntityManager.SetComponentData(entity, new ServerMessageRpcCommand()
        {
            message = text
        });
        if (target != Entity.Null)
        {
            world.EntityManager.SetComponentData(entity, new SendRpcCommandRequest()
            {
                TargetConnection = target
            });
        }
    }
}
