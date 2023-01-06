using System;
using Unity.VisualScripting;
using UnityEngine;

[UnitCategory("TTM Nodes")]

[UnitSubtitle("Activate Objects and Generate Random Initial Rotations")]

//[TypeIcon(typeof(Codebase))]
public class RunningTrialOnEnter_ButtonsNoPoke : Unit
{
    [DoNotSerialize]
    public ControlInput triggerOnEnter;

    [DoNotSerialize]
    public ControlOutput resultOutput;

    [DoNotSerialize]
    public ValueInput Target;

    [DoNotSerialize]
    public ValueInput Trap;

    [DoNotSerialize]
    public ValueInput Manipulandum;

    [DoNotSerialize]
    public ValueOutput ManipulandumAngle;

    float manipulandum_angle_result;

    protected override void Definition()
    {
        Target = ValueInput<GameObject>("Target Object");
        Trap = ValueInput<GameObject>("Trap Object");
        Manipulandum = ValueInput<GameObject>("Manipulandum Object");        

        triggerOnEnter = ControlInput("triggerOnEnter", (flow) =>
        {

            GameObject target = flow.GetValue<GameObject>(Target);
            GameObject trap = flow.GetValue<GameObject>(Trap);
            GameObject manipulandum = flow.GetValue<GameObject>(Manipulandum);

            target.SetActive(true);
            trap.SetActive(true);
            manipulandum.SetActive(true);

            bool cointoss = UnityEngine.Random.value >= 0.5;

            if (cointoss)
            {
                target.transform.rotation = Quaternion.Euler(0, 0, 0);
                trap.transform.rotation = Quaternion.Euler(0, 0, 90);
            }
            else
            {
                target.transform.rotation = Quaternion.Euler(0, 0, 90);
                trap.transform.rotation = Quaternion.Euler(0, 0, 0);
            }

            float random_angle;
            if (UnityEngine.Random.value >= 0.5) random_angle = 0; else random_angle = 90;
            float base_angle = UnityEngine.Random.Range(10, 80);

            manipulandum.transform.rotation = Quaternion.Euler(0, 0, base_angle + random_angle);

            manipulandum_angle_result = manipulandum.transform.eulerAngles.z;
            return resultOutput;
        });

        ManipulandumAngle = ValueOutput<float>("Manipulandum Angle", (flow) => { return manipulandum_angle_result; });

        Requirement(Target, triggerOnEnter);
        Requirement(Trap, triggerOnEnter);
        Requirement(Manipulandum, triggerOnEnter);
        Assignment(triggerOnEnter, ManipulandumAngle);
    }
}