using Godot;
using System;

public struct InputeStruct
{
    public float moveDir = 0;
    private DistributionSystem odm;
    public bool jumpJustRealiced;
    public bool jumpJustPressed;
    public bool intaract;
    public TimeController cannotIntaractForNow = new TimeController(0.7f, 0);

    public InputeStruct(DistributionSystem odm)
    {
        this.odm = odm;
        jumpJustRealiced = false;
    }
    public void CheckInput()
    {
        if (odm.disableMovement != true)
        {
            if (Input.IsActionPressed("MoveRight"))
            {
                moveDir += 1;
            }
            if (Input.IsActionPressed("MoveLeft"))
            {
                moveDir += -1;
            }
            if (moveDir != 0 && odm.stateTransition.CouldTransitionTo(PlayerStates.Walk))
            {
                odm.statesList.Add(PlayerStates.Walk);

            }
            if (Input.IsActionPressed("Jump")) odm.statesList.Add(PlayerStates.Jump);
            if (Input.IsActionJustPressed("Jump")) jumpJustPressed = true;
            else jumpJustPressed = false;
            if (Input.IsActionJustReleased("Jump"))
            {
                jumpJustRealiced = true;
            }
            else jumpJustRealiced = false;
            if (Input.IsActionJustPressed("Dash")) odm.statesList.Add(PlayerStates.Dash);

            if (Input.IsActionPressed("Intaract") && cannotIntaractForNow.ActionDone()) intaract = true;
            else intaract = false;
        }
        else
        {
            jumpJustRealiced = false;
            jumpJustPressed = false;
            intaract = false;
            moveDir = 0;
        }
        if (Input.IsActionPressed("Shoot")) odm.shoot = true;
        else odm.shoot = false;
        if (Input.IsActionJustPressed("Parry")) odm.parry = true;
        else odm.parry = false;
    }
}
