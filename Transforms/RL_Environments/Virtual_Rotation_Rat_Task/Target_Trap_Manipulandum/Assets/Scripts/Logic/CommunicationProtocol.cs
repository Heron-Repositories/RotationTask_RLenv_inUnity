using System.Collections.Specialized;
using System.Collections.Generic;
using UnityEngine;

public class CommunicationProtocol : MonoBehaviour
{
    public readonly OrderedDictionary comProtocol = new OrderedDictionary();
    
   
    void Start()
    {
        comProtocol.Add("Parameter", new OrderedDictionary() { 
            {"move_snap", new List<string>(){"float"} } ,
            {"rotate_snap", new List<string>(){"int" } },
            {"screen_res", new List<string>(){"int, int" } }
        });
        comProtocol.Add("Action", new OrderedDictionary() {
            {"Move", new List<string>(){"Forwards", "Back"} } ,
            {"Rotate",  new List<string>(){"CW", "CCW" } },
            {"LeftPaw",  new List<string>(){"Extend", "Retrieve" } },
            {"RightPaw",  new List<string>(){"Extend", "Retrieve" } }
        });
    }
}
