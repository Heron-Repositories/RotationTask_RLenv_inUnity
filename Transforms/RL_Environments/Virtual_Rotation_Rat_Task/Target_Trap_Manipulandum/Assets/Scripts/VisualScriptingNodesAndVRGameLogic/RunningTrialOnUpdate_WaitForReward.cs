using System;
using Unity.VisualScripting;
using UnityEngine;

[UnitCategory("TTM Nodes")]

[UnitSubtitle("Rotate the Manipulandum if Buttons are touched")]

//[TypeIcon(typeof(Codebase))]
public class RunningTrialOnUpdate_WaitForReward : Unit
{
    [DoNotSerialize]
    public ControlInput triggerOnUpdate;

    [DoNotSerialize]
    public ControlOutput outputTrigger;

    [DoNotSerialize]
    public ValueInput Target;

    [DoNotSerialize]
    public ValueInput Trap;

    [DoNotSerialize]
    public ValueInput Manipulandum;

    [DoNotSerialize]
    public ValueInput RotationSpeed;

    [DoNotSerialize]
    public ValueInput SuccesThresholdAngle;

    [DoNotSerialize]
    public ValueInput InitialManipulandumAngle;

    [DoNotSerialize]
    public ValueInput HoleState;

    [DoNotSerialize]
    public ValueInput NewTrial;


    [DoNotSerialize]
    public ValueInput WaitFramesBeforeTrial;

    [DoNotSerialize]
    public ValueInput WaitIncreamentPercentage;

    [DoNotSerialize]
    public ValueOutput WaitFramesAfterTrial;

    float frames_to_wait_after_trial;

    protected override void Definition()
    {
        int number_of_rotations = 0;

        Target = ValueInput<GameObject>("Target Object");
        Trap = ValueInput<GameObject>("Trap Object");
        Manipulandum = ValueInput<GameObject>("Manipulandum Object");

        RotationSpeed = ValueInput<float>("Rotation Speed");

        SuccesThresholdAngle = ValueInput<int>("Success Threshold Angle");
        InitialManipulandumAngle = ValueInput<float>("Init Manipulandum Angle");

        HoleState = ValueInput<bool>("HoleState");

        NewTrial = ValueInput<bool>("New Trial Started");

        WaitFramesBeforeTrial = ValueInput<float>("Frames to Wait");
        WaitIncreamentPercentage = ValueInput<float>("Percentage to Increase Wait");

        outputTrigger = ControlOutput("outputTrigger");


        triggerOnUpdate = ControlInput("triggerOnUpdate", (flow) =>
        {
            GameObject target = flow.GetValue<GameObject>(Target);
            GameObject trap = flow.GetValue<GameObject>(Trap);
            GameObject manipulandum = flow.GetValue<GameObject>(Manipulandum);

            float rotation_speed = flow.GetValue<float>(RotationSpeed);
            int success_threshold_angle = flow.GetValue<int>(SuccesThresholdAngle);

            bool hole_state = flow.GetValue<bool>(HoleState);

            int initial_man_angle = (int)flow.GetValue<float>(InitialManipulandumAngle);
            int target_angle = (int)target.transform.rotation.eulerAngles.z;
            int trap_angle = 0;
            if (target_angle == 0)
                trap_angle = 90;
            bool cw_rotation = true;
            if((target_angle == 0 &  initial_man_angle < 90) || (target_angle == 90 && initial_man_angle > 90))
            {
                cw_rotation = false;
            }


            bool new_trial = flow.GetValue<bool>(NewTrial);
            if (new_trial)
                number_of_rotations = 0;

            float wait_frames_before_trial = flow.GetValue<float>(WaitFramesBeforeTrial);
            float wait_increament = 1.0f + flow.GetValue<float>(WaitIncreamentPercentage);
            frames_to_wait_after_trial = wait_frames_before_trial;
            if (hole_state)
            {
                target.GetComponent<MeshRenderer>().enabled = true;
                trap.GetComponent<MeshRenderer>().enabled = true;
                foreach (MeshRenderer child_MR in manipulandum.GetComponentsInChildren<MeshRenderer>())
                {
                    child_MR.enabled = true;
                }
            }
            else
            {
                target.GetComponent<MeshRenderer>().enabled = false;
                trap.GetComponent<MeshRenderer>().enabled = false;
                foreach (MeshRenderer child_MR in manipulandum.GetComponentsInChildren<MeshRenderer>())
                {
                    child_MR.enabled = false;
                }
            }

            if (cw_rotation & hole_state)
            {
                number_of_rotations += 1;
                manipulandum.transform.rotation = Quaternion.Euler(0, 0, initial_man_angle + number_of_rotations * rotation_speed);
            }
            if (!cw_rotation & hole_state)
            {
                number_of_rotations -= 1;
                manipulandum.transform.rotation = Quaternion.Euler(0, 0, initial_man_angle + number_of_rotations * rotation_speed);
            }

            int man_angle = (int)manipulandum.transform.eulerAngles.z;
            if (CheckIfManReachedSomething(man_angle,  target_angle, success_threshold_angle))
            {
                frames_to_wait_after_trial = wait_frames_before_trial * wait_increament;
                CustomEvent.Trigger(manipulandum, "TargetReached");
            }
            if (CheckIfManReachedSomething(man_angle, trap_angle, success_threshold_angle))
            {
                CustomEvent.Trigger(manipulandum, "TrapReached");
            }
            return outputTrigger;
        });

        WaitFramesAfterTrial = ValueOutput<float>("Frames To Wait After Trial", (flow) => { return frames_to_wait_after_trial; });

        Requirement(Target, triggerOnUpdate);
        Requirement(Trap, triggerOnUpdate);
        Requirement(Manipulandum, triggerOnUpdate);
        Requirement(RotationSpeed, triggerOnUpdate);
        Requirement(HoleState, triggerOnUpdate);
        Requirement(WaitIncreamentPercentage, triggerOnUpdate);
        Succession(triggerOnUpdate, outputTrigger);
        Assignment(triggerOnUpdate, WaitFramesAfterTrial);
    }

    private bool CheckIfManReachedSomething(int man_angle, int something_angle, int threshold)
    {
        bool result = false;

        float angle_diff = Math.Abs(Mathf.DeltaAngle(man_angle, something_angle));

        if (angle_diff > 90)
        {
            angle_diff = Math.Abs(angle_diff - 180);

        }

        if (angle_diff < threshold)
            result = true;

        return result;
    }

}