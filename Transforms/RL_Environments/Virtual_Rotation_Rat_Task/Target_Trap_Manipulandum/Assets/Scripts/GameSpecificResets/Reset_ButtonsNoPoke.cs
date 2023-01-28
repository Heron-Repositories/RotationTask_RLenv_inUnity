using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Reset_ButtonsNoPoke : MonoBehaviour
{
    void Start()
    {
        EventManager.Instance.onReseting.AddListener(ResetTrialState);
    }

    void ResetTrialState()
    {
        CustomEvent.Trigger(transform.gameObject, "Reset");
        EventManager.Instance.onResetDone.Invoke();
    }

}
