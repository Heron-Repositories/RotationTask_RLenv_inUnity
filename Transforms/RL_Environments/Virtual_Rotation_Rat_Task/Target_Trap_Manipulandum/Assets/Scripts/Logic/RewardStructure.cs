using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// This is a Singleton that defines the different rewards of the environment.
/// </summary>
public class RewardStructure : MonoBehaviour
{
    public static RewardStructure Instance;

    public float NotMoved;
    public float MovedForwards;
    public float MovedBack;
    public float TurnedCW;
    public float TurnedCCW;
    public float RewPortPokedCorrectly;
    public float AreaHighInterest;
    public float AreaMedInterest;
    public float AreaLowInterest;

    private void Reset()
    {
        NotMoved = 0f;
        MovedForwards = -1f;
        MovedBack = -2f;
        TurnedCW = -1f;
        TurnedCCW = -1f;
        RewPortPokedCorrectly = 30f;
        AreaHighInterest = 0f;
        AreaMedInterest = 0f;
        AreaLowInterest = 0f;
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
