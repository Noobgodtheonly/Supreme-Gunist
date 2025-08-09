using Godot;
using System;
using static Godot.OpenXRInterface;
[GlobalClass]
public partial class Spray : GunMod
{
    
    public override void Modify(GunData data)
    {
        data.bulletCount += 2;
        data.scale = (data.scale - 0.2f) > 0 ? data.scale - 0.2f : 0;
        data.damage = (data.damage - ((data.damage > 15)? 11 : 4)) > 0? (data.damage - ((data.damage > 15) ? 11 : 4)) : 0;
        data.bulletTime = ((data.bulletTime - 0.3f) > 0)? data.bulletTime - 0.3f : 0 ;
        data.gunCooldown += 0.2f;
        data.applyGravity = true;
    }
    public override void ChangeMovementForSingleFrame(Bullet bullet)
    {
        bullet.speed -= (float)bullet.random.NextDouble() * (bullet.speed - (bullet.speed / 2));
        bullet.gravitationalScale -= (float)bullet.random.NextDouble() * (bullet.gravitationalScale - (bullet.gravitationalScale / 2));

        float spreadDegrees = bullet.random.Next(-14, 14);
        float spreadRadians = Mathf.DegToRad(spreadDegrees);
        bullet.playerGun.averageRotationAmount += Mathf.DegToRad(spreadDegrees);

        // rotated direction
        bullet.dir = bullet.dir.Rotated(spreadRadians);
        bullet.playerGun.odm.getMovementStruct.velocity.X -= bullet.dir.X;
        bullet.playerGun.camera.AddShake(2f);
    }
}
