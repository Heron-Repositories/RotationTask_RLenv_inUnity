using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
public enum RewardStructure : int
{
    NotMoved = 0,
    Moved = -1,
    PokedAfterTarget = 30
}
*/

public class RewardStructure : MonoBehaviour
{
    public static RewardStructure Instance;

    public int NotMoved;
    public int Moved;
    public int PokedAfterTarget;

    private void Reset()
    {
        NotMoved = 0;
        Moved = -1;
        PokedAfterTarget = 30;
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
