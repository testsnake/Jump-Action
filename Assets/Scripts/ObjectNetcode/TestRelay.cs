using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

//The Relay handles the in-game connection, linking each client to each other and allowing for the easy bypassing of things like firewalls and such.
//Check out the CodeMonkey tutorial here for some of the info on where i got this: https://www.youtube.com/watch?v=msPNJ2cxWfw
public class TestRelay : MonoBehaviour
{
    //Singleton stuff
    public static TestRelay Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    //Creates the relay which links clients together
    public async Task<string> CreateRelay()
    {
        try
        {
            //This takes a parameter for max number of players. Notably, this doesn't include the host.
            //So, for a 10 player game, we set this to 9, since it'll be 9 + the host.
            Allocation alloc = await RelayService.Instance.CreateAllocationAsync(9);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(alloc.AllocationId);
            StartCoroutine(ChangeSceneAndHost(alloc));
            return joinCode;
        } catch (RelayServiceException e)
        {
                Debug.Log(e);
            return null;
        }
    }

    //Causes a player to join into a relay given its code
    public async void JoinRelay(string joinCode)
    {
        try
        {
            Debug.Log("Joining Relay with" + joinCode);
            JoinAllocation alloc = await RelayService.Instance.JoinAllocationAsync(joinCode);

            StartCoroutine(ChangeSceneAndJoin(alloc));
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }

    //Handles asynchronously loading a scene while maintaining important data, useful for performance. Then starts hosting on the networkmanager
    private IEnumerator ChangeSceneAndHost(Allocation alloc)
    {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync("Gameplay");

        while (!asyncOperation.isDone)
        {
            yield return null;
        }

        RelayServerData relayServerData = new RelayServerData(alloc, "dtls");
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
        NetworkManager.Singleton.StartHost();
    }

    //Handles asynchronously loading a scene while maintaining important data, useful for performance. Then joins as client on the networkmanager
    private IEnumerator ChangeSceneAndJoin(JoinAllocation alloc)
    {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync("Gameplay");

        while (!asyncOperation.isDone)
        {
            yield return null;
        }

        RelayServerData relayServerData = new RelayServerData(alloc, "dtls");
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
        NetworkManager.Singleton.StartClient();
    }
}
