using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reset_WaitForReward : MonoBehaviour
{
    void Start()
    {
        EventManager.Instance.onReseting.AddListener(DoNothingOnReset);
    }

    void DoNothingOnReset()
    {
        EventManager.Instance.onResetDone.Invoke();
    }

}
