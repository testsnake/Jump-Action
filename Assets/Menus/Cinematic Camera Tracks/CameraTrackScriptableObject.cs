using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Cinematic Camera Track", order = 1)]
public class CameraTrackScriptableObject : ScriptableObject
{
    public float speed;
    public float rotationSpeed;
    public Vector3 startRotation;
    public Vector3 startPoint;
    public Vector3 endRotation;
    public Vector3 endPoint;
}
