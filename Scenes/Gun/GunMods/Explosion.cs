using Godot;
using System;

[GlobalClass]
public partial class Explosion : GunMod
{
    public override void Modify(GunData data) {
        data.scale += 0.2f;
        data.damage += 13;
        data.gunCooldown += 0.3f;
        data.bulletTime += 0.2f;
        data.speed = ((data.speed - 0.7f)>0)? data.speed - 0.7f : 0;
        data.explode = true;
        data.applyGravity = true;
    }
}
