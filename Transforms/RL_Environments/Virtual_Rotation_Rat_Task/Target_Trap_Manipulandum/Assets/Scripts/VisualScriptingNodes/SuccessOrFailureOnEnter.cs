using System;
using Unity.VisualScripting;
using UnityEngine;

[UnitCategory("TTM Nodes")]

[UnitSubtitle("Deactivate Objects")]

//[TypeIcon(typeof(Codebase))]
public class SuccessOrFailureOnEnter : Unit
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

            target.SetActive(false);
            trap.SetActive(false);
            manipulandum.SetActive(false);
         
            return resultOutput;
        });

        resultOutput = ControlOutput("ResultOutput");

        Requirement(Target, triggerOnEnter);
        Requirement(Trap, triggerOnEnter);
        Requirement(Manipulandum, triggerOnEnter);
        Succession(triggerOnEnter, resultOutput);
    }
}