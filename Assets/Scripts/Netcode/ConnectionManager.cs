using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.NetCode;
using Unity.Entities;
using System.Net;
using Unity.Collections;
using Unity.Networking.Transport;

public class ConnectionManager : MonoBehaviour
{
    //Change this IP address to the address of the dedicated server when we publish!
    [SerializeField] private string ip = "127.0.0.1";
    [SerializeField] private ushort port = 7979;
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
        Connect();
    }

    private void Connect()
    {
        World serverWorld = null;
        World clientWorld = null;

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

        if(serverWorld != null)
        {
            //Make the server listen for connections
            using var query = serverWorld.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
            query.GetSingletonRW<NetworkStreamDriver>().ValueRW.Listen(ClientServerBootstrap.DefaultListenAddress.WithPort(port));
        }
        if (clientWorld != null)
        {
            //All of this is just to convert from ip and port into an endpoint
            IPAddress serverAddress = IPAddress.Parse(ip);
            NativeArray<byte> nativeArrayAddress = new NativeArray<byte>(serverAddress.GetAddressBytes().Length, Allocator.Temp);
            nativeArrayAddress.CopyFrom(serverAddress.GetAddressBytes());
            NetworkEndpoint endpoint = NetworkEndpoint.AnyIpv4;
            endpoint.SetRawAddressBytes(nativeArrayAddress);
            endpoint.Port = port;

            //Now we can actually send the connection request to the endpoint
            using var query = clientWorld.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
            query.GetSingletonRW<NetworkStreamDriver>().ValueRW.Connect(clientWorld.EntityManager, endpoint);
        }

    }
}
