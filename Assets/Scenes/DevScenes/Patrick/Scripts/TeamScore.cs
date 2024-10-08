using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class TeamScore : MonoBehaviour
{
    [SerializeField] private int score;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public int GetScore()
    {
        return score;
    }

    public void incrementScore()
    {
        score = score + 1;
    }


}
