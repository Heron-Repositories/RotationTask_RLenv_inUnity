using System;
using Unity.VisualScripting;
using UnityEngine;

[UnitCategory("TTM Nodes")]

[UnitSubtitle("Rotate the Manipulandum if Buttons are touched")]

//[TypeIcon(typeof(Codebase))]
public class RunningTrialOnUpdate_ButtonsNoPoke : Unit
{
    [DoNotSerialize]
    public ControlInput triggerOnUpdate;

    [DoNotSerialize]
    public ControlOutput resultOutput;

    [DoNotSerialize]
    public ValueInput Manipulandum;

    [DoNotSerialize]
    public ValueInput RotationSpeed;

    [DoNotSerialize]
    public ValueInput SuccesThresholdAngle;

    [DoNotSerialize]
    public ValueInput TargetAngle;

    [DoNotSerialize]
    public ValueInput InitialManipulandumAngle;

    [DoNotSerialize]
    public ValueInput LeftButtonState;

    [DoNotSerialize]
    public ValueInput RightButtonState;

    [DoNotSerialize]
    public ValueInput HoleState;

    [DoNotSerialize]
    public ValueInput NewTrial;

    protected override void Definition()
    {
        //RatController ratController = GameObject.Find("Rat").GetComponent<RatController>();
        int number_of_rotations = 0;

        Manipulandum = ValueInput<GameObject>("Manipulandum Object");
        RotationSpeed = ValueInput<float>("Rotation Speed");

        SuccesThresholdAngle = ValueInput<int>("Success Threshold Angle");
        InitialManipulandumAngle = ValueInput<float>("Init Manipulandum Angle");
        TargetAngle = ValueInput<float>("Target Angle");

        LeftButtonState = ValueInput<bool>("LeftButtonState");
        RightButtonState = ValueInput<bool>("RightButtonState");
        HoleState = ValueInput<bool>("HoleState");

        NewTrial = ValueInput<bool>("New Trial Started");


        triggerOnUpdate = ControlInput("triggerOnUpdate", (flow) =>
        {
            GameObject manipulandum = flow.GetValue<GameObject>(Manipulandum);
            float rotation_speed = flow.GetValue<float>(RotationSpeed);
            int success_threshold_angle = flow.GetValue<int>(SuccesThresholdAngle);

            bool left_button_state = flow.GetValue<bool>(LeftButtonState);
            bool right_button_state = flow.GetValue<bool>(RightButtonState);
            bool hole_state = flow.GetValue<bool>(HoleState);

            int initial_man_angle = (int)flow.GetValue<float>(InitialManipulandumAngle);
            int target_angle = (int)flow.GetValue<float>(TargetAngle);
            int trap_angle = 0;
            if (target_angle == 0)
                trap_angle = 90;

            bool new_trial = flow.GetValue<bool>(NewTrial);
            if (new_trial)
                number_of_rotations = 0;

            if (left_button_state)
            {
                number_of_rotations += 1;
                manipulandum.transform.rotation = Quaternion.Euler(0, 0, initial_man_angle + number_of_rotations * rotation_speed);
            }
            if (right_button_state)
            {
                number_of_rotations -= 1;
                manipulandum.transform.rotation = Quaternion.Euler(0, 0, initial_man_angle + number_of_rotations * rotation_speed);
            }

            int man_angle = (int)manipulandum.transform.eulerAngles.z;
            if (CheckIfManReachedSomething(man_angle,  target_angle, success_threshold_angle))
            {
                CustomEvent.Trigger(manipulandum, "TargetReached");
            }
            if (CheckIfManReachedSomething(man_angle, trap_angle, success_threshold_angle))
            {
                CustomEvent.Trigger(manipulandum, "TrapReached");
            }

            return resultOutput;
        });


        Requirement(Manipulandum, triggerOnUpdate);
        Requirement(RotationSpeed, triggerOnUpdate);
        Requirement(LeftButtonState, triggerOnUpdate);
        Requirement(RightButtonState, triggerOnUpdate);
        Requirement(HoleState, triggerOnUpdate);
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