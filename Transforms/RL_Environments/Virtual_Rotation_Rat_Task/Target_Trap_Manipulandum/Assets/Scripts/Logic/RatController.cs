

using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
using System.Linq;
using System.Text;
using System;
using System.Collections;

public class RatController : MonoBehaviour
{
    [Tooltip("The amount of movement per Move action. Between 0.1 and 2.0.")]
    public float moveSnap;
    [Tooltip("The amount of rotation per Rotate action. This will snap to a number that is a divisor for 350 (so the rat always returns to 0 angle). Between 1 and 90.")]
    public int rotateSnap;

    [Tooltip("The position of the rat when the environment resets")]
    public Vector3 initialPosition;
    [Tooltip("The orientation of the rat when the environment resets")]
    public Quaternion initialRotation;

    private int numberOfRotations = 0;

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

    private OrderedDictionary actionAndParametersComProtocol;
    private OrderedDictionary featuresComProtocol;
    private bool LeftPawExtended = false;
    private bool RightPawExtended = false;

    private Vector3 LeftButtonPosition;
    private Vector3 RightButtonPosition;
    private GameObject Manipulandum;
    private GameObject Target;

    private float SizeOfBody;
    private float SizeOfHead;


    void Start()
    {
        EventManager.Instance.onUpdatedAction.AddListener(TakeAction);
        EventManager.Instance.onParametersChange.AddListener(UpdateParameters);
        EventManager.Instance.onResetDone.AddListener(TakeActionAfterReset);

        actionAndParametersComProtocol = gameObject.GetComponent<CommunicationProtocol>().actionAndParametersComProtocol;
        featuresComProtocol = gameObject.GetComponent<CommunicationProtocol>().featuresComProtocol;

        LeftButtonPosition = GameObject.Find("LeftButton").transform.position;
        RightButtonPosition = GameObject.Find("RightButton").transform.position;

        Manipulandum = GameObject.Find("Manipulandum");
        Target = GameObject.Find("Target");

        EventManager.Instance.onFeaturesObservationReady.Invoke(GenerateFeaturesObservation());

        SizeOfBody = transform.Find("Body").localScale.z;
        SizeOfHead = transform.Find("Head").localScale.z;
    }

    /// <summary>
    /// <c>UpdateParameters</c> is called when the agent asks the environment to change some parameter. The message format here is "ParameterType:ParameterValue" and
    /// it should abide with the CommunicationProtocol
    /// </summary>
    void UpdateParameters(string message)
    {
        string parameterType = message.Split(":")[0];


        OrderedDictionary parameter_values = (OrderedDictionary)actionAndParametersComProtocol["Parameter"];
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
            case var resol when resol == all_parameters[2]: // "screen_res"
                var resolution = message.Split(":")[1].Split(",");
                int width = int.Parse(resolution[0]);
                int height = int.Parse(resolution[1]);
                EventManager.Instance.onNewScreenResolution.Invoke(width, height);
                break;
            case var reset when reset == all_parameters[3]: // "reset". The value is ignored
                transform.position = initialPosition;
                transform.rotation = initialRotation;
                EventManager.Instance.onReseting.Invoke();
                break;
        }
        
    }

    void TakeActionAfterReset()
    {
        TakeAction("Nothing:Nothing");
    }

    /// <summary>
    /// <c>TakeAction</c> is called when the agent asks the environment to update itself after the agents has taken an action. 
    /// The message format here is "ActionType:ActionValue" and it should abide with the CommunicationProtocol
    /// </summary>
    void TakeAction(string message)
    {

        CollisionCheck headCollisionCheck = transform.Find("Head").GetComponent<CollisionCheck>();
        CollisionCheck bodyCollisionCheck = transform.Find("Body").GetComponent<CollisionCheck>();

        OrderedDictionary actions_values = (OrderedDictionary)actionAndParametersComProtocol["Action"];
        List<string> all_actions = (List<string>)actions_values.Keys.Cast<string>().ToList();

        KeyValuePair<string, string> latest_action = new KeyValuePair<string, string>(message.Split(":")[0], message.Split(":")[1]);


        switch (latest_action.Key)  
        {
            case var move when move == all_actions[0]: // "Move"

                var all_values_in_action = (List<string>)actions_values[move];

                switch (latest_action.Value)
                {
                    case var value when value == all_values_in_action[0]: // "Forwards"
                        if (!RightPawExtended && !LeftPawExtended && !headCollisionCheck.ShouldIMove(transform.forward, 1.1f * SizeOfHead / 2))
                        {
                            transform.Translate(new Vector3(0, 0, moveSnap));
                            RepositionInGrid();
                            EventManager.Instance.onRewardStructureChange.Invoke(RewardStructure.Instance.MovedForwards);
                        }
                        else
                            EventManager.Instance.onRewardStructureChange.Invoke(RewardStructure.Instance.NotMoved);
                        break;
                    case var value when value == all_values_in_action[1]: // "Back"
                        if (!RightPawExtended && !LeftPawExtended && !bodyCollisionCheck.ShouldIMove(-transform.forward, 1.1f * SizeOfBody / 2))
                        {
                            transform.Translate(new Vector3(0, 0, -moveSnap));
                            RepositionInGrid();
                            EventManager.Instance.onRewardStructureChange.Invoke(RewardStructure.Instance.MovedBack);
                        }
                        else
                            EventManager.Instance.onRewardStructureChange.Invoke(RewardStructure.Instance.NotMoved);
                        break;
                }
                break;
            case var rotate when rotate == all_actions[1]: // "Rotate"\

                all_values_in_action = (List<string>)actions_values[rotate];

                switch (latest_action.Value)
                {
                    case var value when value == all_values_in_action[0]: // "CW"
                        if (!RightPawExtended && !LeftPawExtended && !headCollisionCheck.ShouldIMove(transform.right, 1.1f * SizeOfBody / 2))
                        {
                            numberOfRotations += 1;
                            transform.rotation = Quaternion.Euler(0.0f, numberOfRotations * rotateSnap, 0.0f);
                            EventManager.Instance.onRewardStructureChange.Invoke(RewardStructure.Instance.TurnedCW);
                        }
                        else
                            EventManager.Instance.onRewardStructureChange.Invoke(RewardStructure.Instance.NotMoved);
                        break;
                    case var value when value == all_values_in_action[1]: // "CCW"
                        if (!RightPawExtended && !LeftPawExtended && !headCollisionCheck.ShouldIMove(-transform.right, 1.1f * SizeOfBody / 2))
                        {
                            numberOfRotations -= 1;
                            transform.rotation = Quaternion.Euler(0.0f, numberOfRotations * rotateSnap, 0.0f);
                            EventManager.Instance.onRewardStructureChange.Invoke(RewardStructure.Instance.TurnedCCW);
                        }
                        else
                            EventManager.Instance.onRewardStructureChange.Invoke(RewardStructure.Instance.NotMoved);
                        break;
                }
                break;
            case var leftPaw when leftPaw == all_actions[2]: // "LeftPaw"

                all_values_in_action = (List<string>)actions_values[leftPaw];

                switch (latest_action.Value)
                {
                    case var value when value == all_values_in_action[0]: // "Extend"
                        if (!LeftPawExtended)
                        {
                            transform.Find("LeftPaw").Translate(new Vector3(0, 0, 0.6f));
                            LeftPawExtended = true;
                        }
                        break;
                    case var value when value == all_values_in_action[1]: // "Retrieve"
                        if (LeftPawExtended)
                        {
                            transform.Find("LeftPaw").Translate(new Vector3(0, 0, -0.6f));
                            LeftPawExtended = false;
                        }
                        break;
                }
                EventManager.Instance.onRewardStructureChange.Invoke(RewardStructure.Instance.NotMoved);
                break;
            case var rightPaw when rightPaw == all_actions[3]: // "RightPaw"

                all_values_in_action = (List<string>)actions_values[rightPaw];

                switch (latest_action.Value)
                {
                    case var value when value == all_values_in_action[0]: // "Extend"
                        if (!RightPawExtended)
                        {
                            RightPawExtended = true;
                            transform.Find("RightPaw").Translate(new Vector3(0, 0, 0.6f));
                        }
                        break;
                    case var value when value == all_values_in_action[1]: // "Retrieve"
                        if (RightPawExtended)
                        {
                            RightPawExtended = false;
                            transform.Find("RightPaw").Translate(new Vector3(0, 0, -0.6f));
                        }
                        break;
                }
                EventManager.Instance.onRewardStructureChange.Invoke(RewardStructure.Instance.NotMoved);
                break;
            case var nothing when nothing == all_actions[4]: // "Nothing"
                EventManager.Instance.onRewardStructureChange.Invoke(RewardStructure.Instance.NotMoved);
                break;
        }

        // At the end of an action the new observation must be prepared ready to be send if asked
        EventManager.Instance.onNeedingNewObservation.Invoke();
        EventManager.Instance.onFeaturesObservationReady.Invoke(GenerateFeaturesObservation());
    }

    /// <summary>
    /// <c>GenerateFeaturesObservation</c> is called when the environment has finished updating itself and can now generate the new features to pass to the agent if required.
    /// </summary>
    List<byte[]> GenerateFeaturesObservation()
    {

        float manipulandumAngle = Manipulandum.transform.rotation.eulerAngles.z;
        bool targetTrapState = (Target.transform.rotation.eulerAngles.z == 0);

        List<byte[]> features_to_send = new();

        int number_of_features = featuresComProtocol.Count;
        features_to_send.Add(BitConverter.GetBytes(number_of_features)); // First number is how many features there are

        foreach (DictionaryEntry feature_spec in featuresComProtocol) // Then for each feature:
        {
            string feature_name = (string)feature_spec.Key;
            features_to_send.Add(Encoding.UTF8.GetBytes(feature_name)); // 1) Name of feature

            List<string> feature_info = (List<string>)feature_spec.Value;

            string feature_dimension_type = feature_info[0];
            features_to_send.Add(Encoding.UTF8.GetBytes(feature_dimension_type)); // 2) Type of the feature's values

            string feature_dimension_size = feature_info[1];
            features_to_send.Add(Encoding.UTF8.GetBytes(feature_dimension_size)); // 3) Number of values for this feature

            List<string> all_feature_names = (List<string>)featuresComProtocol.Keys.Cast<string>().ToList();

            switch (feature_name)
            {
                case var value when value == all_feature_names[0]: // "Rat Position"
                    features_to_send.Add(BitConverter.GetBytes(transform.position.x));
                    features_to_send.Add(BitConverter.GetBytes(transform.position.z));
                    break;

                case var value when value == all_feature_names[1]: // "Rat Rotation"
                    features_to_send.Add(BitConverter.GetBytes(transform.eulerAngles.y));
                    break;

                case var value when value == all_feature_names[2]: // "Left Paw Extended"
                    features_to_send.Add(BitConverter.GetBytes(LeftPawExtended));
                    break;

                case var value when value == all_feature_names[3]: // "Right Paw Extended"
                    features_to_send.Add(BitConverter.GetBytes(RightPawExtended));
                    break;

                case var value when value == all_feature_names[4]: // "Left Button Position"
                    features_to_send.Add(BitConverter.GetBytes(LeftButtonPosition.x));
                    features_to_send.Add(BitConverter.GetBytes(LeftButtonPosition.z));
                    break;

                case var value when value == all_feature_names[5]: // "Right Button Position"
                    features_to_send.Add(BitConverter.GetBytes(RightButtonPosition.x));
                    features_to_send.Add(BitConverter.GetBytes(RightButtonPosition.z));
                    break;

                case var value when value == all_feature_names[6]: // "Target Trap State"
                    features_to_send.Add(BitConverter.GetBytes(targetTrapState));
                    break;

                case var value when value == all_feature_names[7]: // "Manipulandum Angle"
                    features_to_send.Add(BitConverter.GetBytes(manipulandumAngle));
                    break;
            }

        }
        return features_to_send;
    }

    void RepositionInGrid()
    {
        float x = transform.position.x;
        float z = transform.position.z;

        string str_snap = moveSnap.ToString();
        if (moveSnap < 0)
        {
            str_snap = str_snap.Replace("-", "");
        }
        str_snap = str_snap.Replace(".", "");
        int removeDigits = str_snap.Length - 1;

        float rounded_x = (float)Math.Round(x, removeDigits);
        float rounded_z = (float)Math.Round(z, removeDigits);

        transform.position = new Vector3(rounded_x, transform.position.y, rounded_z);
    }

}
