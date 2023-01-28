using System;
using Unity.VisualScripting;
using UnityEngine;

[UnitCategory("TTM Nodes")]

[UnitSubtitle("Activate Objects and Generate Random Initial Rotations")]

//[TypeIcon(typeof(Codebase))]
public class RunningTrialOnEnter_WaitForReward : Unit
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
    public ValueInput WaitFrames;

    [DoNotSerialize]
    public ValueInput RotationSpeed;

    [DoNotSerialize]
    public ValueOutput ManipulandumAngle;

    float manipulandum_angle;
    float frames_to_wait_after_trial;

    protected override void Definition()
    {
        Target = ValueInput<GameObject>("Target Object");
        Trap = ValueInput<GameObject>("Trap Object");
        Manipulandum = ValueInput<GameObject>("Manipulandum Object");
        WaitFrames = ValueInput<float>("Frames to Wait");

        RotationSpeed = ValueInput<float>("Rotation Speed");


        triggerOnEnter = ControlInput("triggerOnEnter", (flow) =>
        {

            GameObject target = flow.GetValue<GameObject>(Target);
            GameObject trap = flow.GetValue<GameObject>(Trap);
            GameObject manipulandum = flow.GetValue<GameObject>(Manipulandum);

            int wait_frames = (int)flow.GetValue<float>(WaitFrames);
            float rotation_speed = flow.GetValue<float>(RotationSpeed);

            target.GetComponent<MeshRenderer>().enabled = false;
            trap.GetComponent<MeshRenderer>().enabled = false;
            foreach (MeshRenderer child_MR in manipulandum.GetComponentsInChildren<MeshRenderer>())
            {
                child_MR.enabled = false;
            }

            bool cointoss = UnityEngine.Random.value >= 0.5;

            if (cointoss)
            {
                target.transform.rotation = Quaternion.Euler(0, 0, 0);
                trap.transform.rotation = Quaternion.Euler(0, 0, 90);
                manipulandum_angle = wait_frames * rotation_speed;
            }
            else
            {
                target.transform.rotation = Quaternion.Euler(0, 0, 90);
                trap.transform.rotation = Quaternion.Euler(0, 0, 0);
                manipulandum_angle = 90f - wait_frames * rotation_speed;
            }

            manipulandum.transform.rotation = Quaternion.Euler(0, 0, manipulandum_angle);


            return resultOutput;
        });

        ManipulandumAngle = ValueOutput<float>("Manipulandum Angle", (flow) => { return manipulandum_angle; });
        

        Requirement(Target, triggerOnEnter);
        Requirement(Trap, triggerOnEnter);
        Requirement(Manipulandum, triggerOnEnter);
        Requirement(WaitFrames, triggerOnEnter);
        Assignment(triggerOnEnter, ManipulandumAngle);
    }
}