using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;

public class CollisionCheck : MonoBehaviour
{
    private Rigidbody this_rigidbody;

    public List<GameObject> ObjectsCanCollideWith;

    private string gameobject_name;

    private void Start()
    {
        this_rigidbody = GetComponent<Rigidbody>();
        gameobject_name = transform.gameObject.name;
    }

    public bool ShouldIMove(Vector3 direction, float distance)
    {
        RaycastHit hit;
        if (this_rigidbody.SweepTest(direction, out hit, distance))
        {
            if (hit.transform.name != "RewardPort")
            {
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
                break;
            case "RightButton":
                CustomEvent.Trigger(transform.gameObject, "RighttButtonPressed");
                break;
            case "RewardPort":
                CustomEvent.Trigger(transform.gameObject, "RewardPortPoked");
                break;
        }
    }

    private void SendAppropriateUnPressSignal()
    {
        switch (gameobject_name)
        {
            case "LeftButton":
                CustomEvent.Trigger(transform.gameObject, "LeftButtonUnPressed");
                break;
            case "RightButton":
                CustomEvent.Trigger(transform.gameObject, "RighttButtonUnPressed");
                break;
            case "RewardPort":
                CustomEvent.Trigger(transform.gameObject, "RewardPortUnPoked");
                break;
        }
    }
}
