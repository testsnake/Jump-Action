using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.NetCode;

public class AutoConnectBootstrap : ClientServerBootstrap
{
    public override bool Initialize(string defaultWorldName)
    {
        return false;
    }
}
