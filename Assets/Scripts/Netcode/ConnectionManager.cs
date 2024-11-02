using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.NetCode;
using Unity.Entities;
using System.Net;
using Unity.Collections;
using Unity.Networking.Transport;
using Unity.Scenes;

public class ConnectionManager : MonoBehaviour
{
    //Change this IP address to the address of the dedicated server when we publish!
    [SerializeField] private string listenIP = "127.0.0.1";
    [SerializeField] private string connectIP = "127.0.0.1";
    [SerializeField] private ushort port = 7979;

    public static World serverWorld = null;
    public static World clientWorld = null;
    public enum Role
    {
        ServerClient = 0, Server = 1, Client = 2
    }
    private static Role role = Role.ServerClient;

    // Start is called before the first frame update
    private void Start()
    {
        //Check whether we're the server or the client (or both, if we're in the editor)
        if (Application.isEditor)
        {
            role = Role.ServerClient;
        } else if (Application.platform == RuntimePlatform.WindowsServer || Application.platform == RuntimePlatform.LinuxServer || Application.platform == RuntimePlatform.OSXServer)
        {
            role = Role.Server;
        } else
        {
            role = Role.Client;
        }
        StartCoroutine(Connect());
    }

    private IEnumerator Connect()
    {
        //Set up worlds for server and client
        if(role == Role.ServerClient || role == Role.Server)
        {
            serverWorld = ClientServerBootstrap.CreateServerWorld("ServerWorld");
        }

        if (role == Role.ServerClient || role == Role.Client)
        {
            clientWorld = ClientServerBootstrap.CreateClientWorld("ClientWorld");
        }

        //Delete the default generated world
        foreach (var world in World.All)
        {
            if(world.Flags == WorldFlags.Game)
            {
                world.Dispose();
                break;
            }
        }

        if(serverWorld != null)
        {
            World.DefaultGameObjectInjectionWorld = serverWorld;
        } else if (clientWorld != null) 
        {
            World.DefaultGameObjectInjectionWorld = clientWorld;
        }

        //Get all our SubScenes
        SubScene[] subScenes = FindObjectsByType<SubScene>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        
        
        if (serverWorld != null)
        { 
            while (!serverWorld.IsCreated)
            {
                yield return null;
            } 
            if (subScenes != null)
            {
                for (int i = 0; i < subScenes.Length; i++)
                {
                        //Load all the subscenes with parameters
                        SceneSystem.LoadParameters loadParameters = new SceneSystem.LoadParameters() { Flags = SceneLoadFlags.BlockOnStreamIn };
                        var sceneEntity = SceneSystem.LoadSceneAsync(serverWorld.Unmanaged, new Unity.Entities.Hash128(subScenes[i].SceneGUID.Value), loadParameters);
                        while(!SceneSystem.IsSceneLoaded(serverWorld.Unmanaged, sceneEntity))
                        {
                            serverWorld.Update();
                        }
                    }
            }
            //Make the server listen for connections
            using var query = serverWorld.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
            query.GetSingletonRW<NetworkStreamDriver>().ValueRW.Listen(NetworkEndpoint.Parse(listenIP, port));
        }

        if (clientWorld != null)
        {
            while (!clientWorld.IsCreated)
            {
                yield return null;
            }
            if (subScenes != null)
            {
                for (int i = 0; i < subScenes.Length; i++)
                {
                    //Load all the subscenes with parameters
                    SceneSystem.LoadParameters loadParameters = new SceneSystem.LoadParameters() { Flags = SceneLoadFlags.BlockOnStreamIn };
                    var sceneEntity = SceneSystem.LoadSceneAsync(clientWorld.Unmanaged, new Unity.Entities.Hash128(subScenes[i].SceneGUID.Value), loadParameters);
                    while (!SceneSystem.IsSceneLoaded(clientWorld.Unmanaged, sceneEntity))
                    {
                        clientWorld.Update();
                    }
                }
            }
            //Now we can actually send the connection request to the endpoint
            using var query = clientWorld.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
            query.GetSingletonRW<NetworkStreamDriver>().ValueRW.Connect(clientWorld.EntityManager, NetworkEndpoint.Parse(connectIP, port));
        }

    }
}
