using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{

    [SerializeField] private Transform spawnedObjectPrefab;

    private NetworkVariable<MyCustomData> randomNumber = new NetworkVariable<MyCustomData>(
        new MyCustomData
        {
            _int = 56,
            _bool = true
        },
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );  

    public struct MyCustomData : INetworkSerializable
    {
        public int _int;
        public bool _bool;
        public FixedString128Bytes message;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _int);
            serializer.SerializeValue(ref _bool);
            serializer.SerializeValue(ref message);
        }
    }
    public override void OnNetworkSpawn()
    {
        randomNumber.OnValueChanged += (MyCustomData previousValue, MyCustomData newValue) =>
        {
            Debug.Log(+ OwnerClientId + "; " + newValue._int + "; " + newValue._bool + "; " + newValue.message);
        };
    }


    private void FixedUpdate()
    {
        if (!IsOwner) return;

        HandleMovement();
        HandleRotation();
        HandleServerClientCalls();
    }

    private void HandleMovement() 
    {
        Vector3 moveDir = new Vector3(0, 0, 0);

        if (Input.GetKey(KeyCode.W)) moveDir.z = +1f;
        if (Input.GetKey(KeyCode.S)) moveDir.z = -1f;
        if (Input.GetKey(KeyCode.A)) moveDir.x = -1f;
        if (Input.GetKey(KeyCode.D)) moveDir.x = +1f;

        float moveSpeed = 3f;
        transform.position += moveDir * moveSpeed * Time.deltaTime;
    }

    private void HandleRotation()
    {
        Plane plane = new Plane(Vector3.up, transform.position);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (plane.Raycast(ray, out float distance))
        {
            Vector3 mouseWorldPosition = ray.GetPoint(distance);
            Vector3 lookDirection = (mouseWorldPosition - transform.position).normalized; 
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 0.1f);
        }
    }

    private void HandleServerClientCalls() 
    {
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 mousePosition;

        if (plane.Raycast(ray, out float distance))
            mousePosition = ray.GetPoint(distance);
        else
            mousePosition = ray.GetPoint(distance);

        if (Input.GetMouseButtonDown(0))
            CreateObjectUsingServerRpc(mousePosition, new ServerRpcParams());
        else if (Input.GetMouseButtonDown(1))
            CreateObjectUsingClientRpcThroughServerRpc(mousePosition, new ServerRpcParams());
    }

    [ServerRpc]
    private void CreateObjectUsingServerRpc(Vector3 spawnPosition, ServerRpcParams serverRpcParams) 
    {
        Debug.Log("CreateObjectThroughServerRpc() " + OwnerClientId + "; " + serverRpcParams.Receive.SenderClientId);

        Transform spawnedObject = Instantiate(spawnedObjectPrefab);
        spawnedObject.transform.position = spawnPosition;
        NetworkObject networkObject = spawnedObject.GetComponent<NetworkObject>();
        networkObject.Spawn(true);
    }

    [ServerRpc]
    private void CreateObjectUsingClientRpcThroughServerRpc(Vector3 spawnPosition, ServerRpcParams serverRpcParams) 
    {
        Debug.Log("UseClientToCreateObjectServerRpc() " + OwnerClientId + "; " + serverRpcParams.Receive.SenderClientId);

        ulong serverClientId = 0;
        CreateObjectClientRpc(spawnPosition, serverClientId);
    }

    [ClientRpc]
    private void CreateObjectClientRpc(Vector3 spawnPosition, ulong serverClientId) 
    {
        Debug.Log("CreateObjectClientRpc() " + OwnerClientId + "; " + serverClientId);

        Transform spawnedObject = Instantiate(spawnedObjectPrefab);
        spawnedObject.transform.position = spawnPosition;
    }

}
