using Godot;
using System;
[GlobalClass]

public partial class AfterImage : Sprite2D
{
    DistributionSystem odm;
    public TimeController actionTimer;
    public void Init(DistributionSystem odm, float time, Vector2 pos, bool flipDir, bool changeCol)
    {
        if (actionTimer == null)
            actionTimer = new TimeController(0, 0);
        actionTimer.ChangeActionDuration(time);
        this.odm = odm;
        actionTimer.Start();
        Position = pos;
        this.FlipH = flipDir;
        this.Visible = true;
        this.SetPhysicsProcess(true);
        if(changeCol)
            (Material as ShaderMaterial).SetShaderParameter("flashColThresh", 1);
        else
            (Material as ShaderMaterial).SetShaderParameter("flashColThresh", 0);
    }
    public override void _PhysicsProcess(double delta)
    {
        actionTimer.Update((float)delta);
        if (actionTimer.ActionDone())
        {
            odm.afterImages.Add(this);
            this.Visible = false;
            this.SetPhysicsProcess(false);
            return;
        }
        float amoutOfTimeDone = actionTimer.currTime / actionTimer.actionDuration;
        float opacityReductionThreshold = Mathf.Lerp(1, 0, amoutOfTimeDone);
        (Material as ShaderMaterial).SetShaderParameter("opacityThresh", opacityReductionThreshold);
    }
}
