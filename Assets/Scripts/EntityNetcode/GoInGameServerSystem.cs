//using System.Collections;
//using System.Collections.Generic;
//using Unity.Collections;
//using Unity.Entities;
//using Unity.NetCode;
//using UnityEngine;

//[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
//public partial struct GoInGameServerSystem : ISystem
//{
//    public void OnCreate(ref SystemState state)
//    {
//        //Only run an update when there is an entity in the scene with ReceiveRpcCommandRequest and GoInGameCommand on it
//        //Basically, only update when the client is sending a new command, i think
//        var builder = new EntityQueryBuilder(Allocator.Temp);
//        builder.WithAll<ReceiveRpcCommandRequest, GoInGameCommand>();
//        state.RequireForUpdate(state.GetEntityQuery(builder));
//    }

//    public void OnUpdate(ref SystemState state)
//    {
//        var commandBuffer = new EntityCommandBuffer(Allocator.Temp);

//        foreach (var (request, command, entity) in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>, RefRO<GoInGameCommand>>().WithEntityAccess())
//        {
//            //Adds NetworkStreamInGame to the prefab representing the client that made the request
//            //This should sync them across the client and server
//            commandBuffer.AddComponent<NetworkStreamInGame>(request.ValueRO.SourceConnection);
//            commandBuffer.DestroyEntity(entity);
//        }

//        commandBuffer.Playback(state.EntityManager);
//        commandBuffer.Dispose();
//    }
//}
