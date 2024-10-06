using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.NetCode;
using Unity.Entities;

public class ConnectionManager : MonoBehaviour
{
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

        if(role == Role.ServerClient || role == Role.Server)
        {
            serverWorld = ClientServerBootstrap.CreateServerWorld("ServerWorld");
        }

        if (role == Role.ServerClient || role == Role.Client)
        {
            clientWorld = ClientServerBootstrap.CreateClientWorld("ClientWorld");
        }
    }
}
