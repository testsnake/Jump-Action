using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public class PrefabsManager : MonoBehaviour
{
    //Add as many prefabs here as needed. Make sure to also add them to PrefabsData though
    //Also make sure you assign it in the Bake function
    public GameObject player = null;
    
    public struct PrefabsData : IComponentData
    {
        public Entity player;
    }

    public class PrefabsBaker : Baker<PrefabsManager>
    {
        public override void Bake(PrefabsManager authoring)
        {
            //For every new prefab, we add a new Entity here like the playerPrefab
            Entity playerPrefab = default;

            //Then do another check just like this one to assign it's value
            if (authoring.player != null)
            {
                playerPrefab = GetEntity(authoring.player, TransformUsageFlags.Dynamic);
            }
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            //Then finally make sure to add the prefab to this list, like you see here
            AddComponent(entity, new PrefabsData
            {
                player = playerPrefab
            });
        }
    }

}
