using System.Collections.Specialized;
using System.Collections.Generic;
using UnityEngine;

public class CommunicationProtocol : MonoBehaviour
{
    public readonly OrderedDictionary actionAndParametersComProtocol = new OrderedDictionary();
    public readonly List<string> observationsComProtocol = new List<string>() { "Pixels", "Features", "Everything" };
    public readonly OrderedDictionary featuresComProtocol = new OrderedDictionary();

    // The Features Observation is a List of floats. The first float one is the number of remaining numbers (features)

    void Start()
    {
        actionAndParametersComProtocol.Add("Parameter", new OrderedDictionary() { 
            {"move_snap", new List<string>(){"float"} } ,
            {"rotate_snap", new List<string>(){"int" } },
            {"screen_res", new List<string>(){"int, int" } }
        });
        actionAndParametersComProtocol.Add("Action", new OrderedDictionary() {
            {"Move", new List<string>(){"Forwards", "Back"} } ,
            {"Rotate",  new List<string>(){"CW", "CCW" } },
            {"LeftPaw",  new List<string>(){"Extend", "Retrieve" } },
            {"RightPaw",  new List<string>(){"Extend", "Retrieve" } },
            {"Nothing",  new List<string>(){"Nothing"} }
        });

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
