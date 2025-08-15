using Godot;
using System;

[GlobalClass]
public partial class TimeManagerUpdater : Node
{
    public override void _PhysicsProcess(double delta)
    {
        GlobalTimeManager.Update((float)delta);
    }
}
