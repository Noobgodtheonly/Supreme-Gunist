using Godot;
using System;

[GlobalClass]
public partial class GunMod : Node2D
{
    
    public virtual void Modify(GunData data) { }
    public virtual void movementStyle(Bullet bullet) { }
    public virtual void OnHit(Bullet bullet) { }
    public virtual void ChangeMovementForSingleFrame(Bullet bullet) { }
    public virtual void OnActionTimerDone(Bullet bullet) { }
}
