
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
using System.Linq;
using System.Reflection;

public class RatController : MonoBehaviour
{
    [Tooltip("The amount of movement per Move action. Between 0.1 and 2.0.")]
    public float moveSnap;
    [Tooltip("The amount of rotation per Rotate action. This will snap to a number that is a divisor for 350 (so the rat always returns to 0 angle). Between 1 and 90.")]
    public int rotateSnap;


    [ExecuteInEditMode]
    void OnValidate()
    {
        if(moveSnap < 0.1)
            moveSnap = 0.1F;
        if (moveSnap > 2.0)
            moveSnap = 2.0F;

        CorrectRotateSnap();
    }

    void CorrectRotateSnap()
    {
        if (360 % rotateSnap != 0)
        {
            int divisor = 360 / rotateSnap;
            int result = 360 / divisor;
            rotateSnap = result;
        }

        if (rotateSnap < 1)
            rotateSnap = 1;
        if (rotateSnap > 90)
            rotateSnap = 90;
    }

    // private CircularBuffer<KeyValuePair<string, string>> actions_memory;

    private OrderedDictionary comProtocol;


    // Start is called before the first frame update
    void Start()
    {
        EventManager.Instance.onUpdatedAction.AddListener(TakeAction);
        EventManager.Instance.onParametersChange.AddListener(UpdateParameters);

        comProtocol = gameObject.AddComponent<CommunicationProtocol>().comProtocol;

        // actions_memory = new CircularBuffer<KeyValuePair<string, string>>(100);
    }

    void UpdateParameters(string message)
    {
        string parameterType = message.Split(":")[0];


        OrderedDictionary parameter_values = (OrderedDictionary)comProtocol["Parameter"];
        List<string> all_parameters = (List<string>)parameter_values.Keys.Cast<string>().ToList();


        switch (parameterType)
        {
            case var move when move == all_parameters[0]: // "move_snap"
                moveSnap = float.Parse(message.Split(":")[1]);
                break;
            case var rotate when rotate == all_parameters[1]: // "rotate_snap"
                rotateSnap = int.Parse(message.Split(":")[1]);
                CorrectRotateSnap();
                break;
            case var res when res == all_parameters[2]: // "screen_res"
                var resolution = message.Split(":")[1].Split(",");
                int width = int.Parse(resolution[0]);
                int height = int.Parse(resolution[1]);
                EventManager.Instance.onNewScreenResolution.Invoke(width, height);
                break;
        }
        
    }

    void TakeAction(string message)
    {
        /*
         * Use the following code to allow multiple actions to be send at the same time (but that code ignores all but the latest of them)
        string[] key_values = message.Split(",");
        foreach (string str in key_values)
        {
            actions_memory.Add(new KeyValuePair<string, string>(str.Split(":")[0], str.Split(":")[1]));
        }

        KeyValuePair<string, string> latest_action = actions_memory.Latest();
        */


        KeyValuePair<string, string> latest_action = new KeyValuePair<string, string>(message.Split(":")[0], message.Split(":")[1]);


        OrderedDictionary actions_values = (OrderedDictionary)comProtocol["Action"];
        List<string> all_actions = (List<string>)actions_values.Keys.Cast<string>().ToList();

        switch (latest_action.Key)  
        {
            case var move when move == all_actions[0]: // "Move"

                var all_values_in_action = (List<string>)actions_values[move];

                switch (latest_action.Value)
                {
                    case var value when value == all_values_in_action[0]: // "Forwards"
                        transform.Translate(new Vector3(0, 0, moveSnap));
                        break;
                    case var value when value == all_values_in_action[1]: // "Back"
                        transform.Translate(new Vector3(0, 0, - moveSnap));
                        break;
                }
                break;
            case var rotate when rotate == all_actions[1]: // "Rotate"\

                all_values_in_action = (List<string>)actions_values[rotate];

                switch (latest_action.Value)
                {
                    case var value when value == all_values_in_action[0]: // "CW"
                        transform.Rotate(new Vector3(transform.rotation.x, transform.rotation.y + rotateSnap, transform.position.z));
                        break;
                    case var value when value == all_values_in_action[1]: // "CCW"
                        transform.Rotate(new Vector3(transform.rotation.x, transform.rotation.y - rotateSnap, transform.position.z));
                        break;
                }
                break;
            case var leftPaw when leftPaw == all_actions[2]: // "LeftPaw"

                all_values_in_action = (List<string>)actions_values[leftPaw];

                switch (latest_action.Value)
                {
                    case var value when value == all_values_in_action[0]: // "Extend"
                        throw new NotImplementedException();
                        break;
                    case var value when value == all_values_in_action[1]: // "Retrieve"
                        throw new NotImplementedException();
                        break;
                }
                break;
            case var rightPaw when rightPaw == all_actions[2]: // "RightPaw"

                all_values_in_action = (List<string>)actions_values[rightPaw];

                switch (latest_action.Value)
                {
                    case var value when value == all_values_in_action[0]: // "Extend"
                        throw new NotImplementedException();
                        break;
                    case var value when value == all_values_in_action[1]: // "Retrieve"
                        throw new NotImplementedException();
                        break;
                }
                break;
        }

        // At the end of an action the new observation must be prepared ready to be send if asked
        //Debug.Log("1. Sending Observation Required Message");
        EventManager.Instance.onNeedingNewObservation.Invoke();
    }

    

}
