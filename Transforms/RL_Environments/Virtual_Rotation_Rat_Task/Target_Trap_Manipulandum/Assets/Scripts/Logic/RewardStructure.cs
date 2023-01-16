using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// This is a Singleton that defines the different rewards of the environment.
/// </summary>
public class RewardStructure : MonoBehaviour
{
    public static RewardStructure Instance;

    public int NotMoved;
    public int Moved;
    public int RewPortPokedCorrectly;

    private void Reset()
    {
        NotMoved = 0;
        Moved = -1;
        RewPortPokedCorrectly = 30;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
            Destroy(this);
    }
}
