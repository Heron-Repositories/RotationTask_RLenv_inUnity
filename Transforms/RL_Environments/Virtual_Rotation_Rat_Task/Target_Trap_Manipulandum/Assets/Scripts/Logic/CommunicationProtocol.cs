using System.Collections.Specialized;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The <c>CommunicationProtocol</c> class defines the protocol of commands send and received between the agent (separate process) and the environment (the process that
/// runs this Unity game). By keeping things in one place (and writting the C# Unity code to adhere to this protocol) it becomes easy to write the agent's code and to update it
/// correctly if anything needs to change.
/// </summary>
public class CommunicationProtocol : MonoBehaviour
{
    public readonly OrderedDictionary actionAndParametersComProtocol = new OrderedDictionary();
    public readonly OrderedDictionary featuresComProtocol = new OrderedDictionary();
    public readonly List<string> observationsComProtocol = new List<string>() { "Pixels", "Features", "Everything" };

    void Start()
    {
        actionAndParametersComProtocol.Add("Parameter", new OrderedDictionary() {
            {"move_snap", new List<string>(){"float"} } ,
            {"rotate_snap", new List<string>(){"int" } },
            {"screen_res", new List<string>(){"int, int" } },
            {"reset", new List<string>(){"bool"} }
        });
        actionAndParametersComProtocol.Add("Action", new OrderedDictionary() {
            {"Move", new List<string>(){"Forwards", "Back"} } ,
            {"Rotate",  new List<string>(){"CW", "CCW" } },
            {"LeftPaw",  new List<string>(){"Extend", "Retrieve" } },
            {"RightPaw",  new List<string>(){"Extend", "Retrieve" } },
            {"Nothing",  new List<string>(){"Nothing"} }
        });

        // The featuresComProtocol defines the way features should be sent to the agent.
        // For each feature the environment will send a string with the type (e.g. 'float') then a string with the number of values (e.g. '2') and then the values themselves
        featuresComProtocol.Add("Rat Position", new List<string>() { "float", "2" });
        featuresComProtocol.Add("Rat Rotation", new List<string>() { "float", "1" });
        featuresComProtocol.Add("Left Paw Extended", new List<string>() { "bool", "1" });
        featuresComProtocol.Add("Right Paw Extended", new List<string>() { "bool", "1" });
        featuresComProtocol.Add("Left Button Position", new List<string>() { "float", "2" });
        featuresComProtocol.Add("Right Button Position", new List<string>() { "float", "2" });
        featuresComProtocol.Add("Target Trap State", new List<string>() { "bool", "1" });
        featuresComProtocol.Add("Manipulandum Angle", new List<string>() { "float", "1" });

    }
}
