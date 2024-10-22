//using System.Collections;
//using System.Collections.Generic;
//using Unity.Collections;
//using Unity.Entities;
//using Unity.NetCode;
//using UnityEngine;
//using UnityEngine.EventSystems;

//[UpdateInGroup(typeof(GhostInputSystemGroup))]
//[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
//public partial class InputSystem : SystemBase
//{
//    private InputActions controls;
//    protected override void OnCreate()
//    {
//        controls = new InputActions();
//        controls.Enable();
//        //Ensure the update only runs if there is a PlayerInputData in the world
//        var builder = new EntityQueryBuilder(Allocator.Temp);
//        builder.WithAny<PlayerInputData>();
//        RequireForUpdate(GetEntityQuery(builder));
//    }
//    protected override void OnUpdate()
//    {
//        Vector2 v2 = controls.Player.Movement.ReadValue<Vector2>();
//        Debug.Log(v2);
//        foreach (RefRW<PlayerInputData> input in SystemAPI.Query<RefRW<PlayerInputData>>().WithAll<GhostOwnerIsLocal>())
//        {
//            input.ValueRW.move = v2;
//        }
        
//    }
//    protected override void OnDestroy()
//    {
//        controls.Disable();
//    }
//}
