using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;

public class CollisionCheck : MonoBehaviour
{
    private Rigidbody rigidbody;

    public List<GameObject> ObjectsCanCollideWith;

    private string gameobject_name;

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        gameobject_name = transform.gameObject.name;
    }

    public bool ShouldIMove(Vector3 direction, float distance)
    {
        RaycastHit hit;
        if (rigidbody.SweepTest(direction, out hit, distance))
        {
            if (hit.transform.name != "RewardPort")
            {
                //Debug.Log($"ShouldIMove returned True by touching {hit.transform.name}");
                return true;
            }
                
        }
        return false;
    }


    private void OnTriggerEnter(Collider other)
    {
        for (int i=0; i< ObjectsCanCollideWith.Count; i++)
        {
            if (ObjectsCanCollideWith[i] == other.gameObject)
                SendAppropriatePressSignal();
        }
    }


    private void OnTriggerStay(Collider other)
    {
        for (int i = 0; i < ObjectsCanCollideWith.Count; i++)
        {
            if (ObjectsCanCollideWith[i] == other.gameObject && gameobject_name != "RewardPort")
                SendAppropriatePressSignal();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        for (int i = 0; i < ObjectsCanCollideWith.Count; i++)
        {
            if (ObjectsCanCollideWith[i] == other.gameObject)
                SendAppropriateUnPressSignal();
        }
    }

    private void SendAppropriatePressSignal()
    {
        switch (gameobject_name)
        {
            case "LeftButton":
                CustomEvent.Trigger(transform.gameObject, "LeftButtonPressed");
                //Debug.Log("LeftButton Is Being Pressed");
                break;
            case "RightButton":
                CustomEvent.Trigger(transform.gameObject, "RighttButtonPressed");
                //Debug.Log("RightButton Is Being Pressed");
                break;
            case "RewardPort":
                CustomEvent.Trigger(transform.gameObject, "RewardPortPoked");
                //Debug.Log("RewardPort Is Being Poked");
                break;
        }
    }

    private void SendAppropriateUnPressSignal()
    {
        switch (gameobject_name)
        {
            case "LeftButton":
                CustomEvent.Trigger(transform.gameObject, "LeftButtonUnPressed");
                //Debug.Log("LeftButton Is Being UnPressed");
                break;
            case "RightButton":
                CustomEvent.Trigger(transform.gameObject, "RighttButtonUnPressed");
                //Debug.Log("RightButton Is Being UnPressed");
                break;
            case "RewardPort":
                CustomEvent.Trigger(transform.gameObject, "RewardPortUnPoked");
                //Debug.Log("RewardPort Is Being UnPoked");
                break;
        }
    }
}
