using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using static NetworkedPlayer;
public class NetworkedPlayer : MonoBehaviour
{
    public float speed = 20f;
}

public struct PlayerData : IComponentData
{
    public float speed;
}

public class PlayerBaker : Baker<NetworkedPlayer>
{
    public override void Bake(NetworkedPlayer authoring)
    {
        var entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(entity, new PlayerData
        {
            speed = authoring.speed
        });
        AddComponent<PlayerInputData>(entity);
    }
}
