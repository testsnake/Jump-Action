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

public class TestRelay : MonoBehaviour
{
    public static TestRelay Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }
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
