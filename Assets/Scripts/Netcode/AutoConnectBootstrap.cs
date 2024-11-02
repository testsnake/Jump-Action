using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.NetCode;
using UnityEngine.Scripting;

[Preserve]
public class AutoConnectBootstrap : ClientServerBootstrap
{
    public override bool Initialize(string defaultWorldName)
    {
        AutoConnectPort = 0;
        return false;
    }
}
