using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinematicMenuCam : MonoBehaviour
{
    public CameraTrackScriptableObject[] tracks;
    public int currentTrack;
    // Start is called before the first frame update
    void Start()
    {
        currentTrack = 0;
        StartCoroutine(CameraMove());
    }


    IEnumerator CameraMove()
    {
        CameraTrackScriptableObject track = tracks[currentTrack];
        transform.position = track.startPoint;
        transform.rotation = Quaternion.Euler(track.startRotation);
        while (transform.position != track.endPoint)
        {
            transform.position = Vector3.MoveTowards(transform.position, track.endPoint, track.speed);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(track.endRotation), track.rotationSpeed);
            yield return null;
        }
        while (transform.rotation != Quaternion.Euler(track.endRotation))
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(track.endRotation), track.rotationSpeed);
            yield return null;
        }
            yield return new WaitForSeconds(1f);
        if (currentTrack == tracks.Length - 1) {
            currentTrack = 0;
        } else
        {
            currentTrack++;
        }
        yield return StartCoroutine(CameraMove());
    }

}
