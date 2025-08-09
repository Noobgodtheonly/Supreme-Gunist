using Godot;
using System;
[GlobalClass]
public partial class UiScript : Control
{
    [Export] SpringCamera2D camera;
    [Export] DistributionSystem odm;
    bool gunModMenueOn = false;
    MarginContainer gunEnhancementUi;

    public override void _Ready()
    {
        gunEnhancementUi = GetNode<MarginContainer>("GunEnhancementUi");
    }
    public override void _PhysicsProcess(double delta)
    {
        CheckInpute();
        CheckGunModMenue();
    }
    public void CheckInpute()
    {
        if (Input.IsActionJustPressed("GunModMenue")) gunModMenueOn = !gunModMenueOn;
    }
    public void CheckGunModMenue()
    {
        if (gunModMenueOn)
        {
            gunEnhancementUi.Visible = true;
            camera.disableLookAhead = true;
            odm.disableMovement = true;
        }
        else { gunEnhancementUi.Visible = false; camera.disableLookAhead = false; odm.disableMovement = false; }
    }
}
