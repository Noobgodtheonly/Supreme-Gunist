using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class PlayersGun : Node2D
{
    [Export] public DistributionSystem odm;
    [Export] float muzzleDist;
    public float averageRotationAmount;
    public float bulletScaleScaler = 2.5f;
    TimeController timeController;
    private GunData gunData;
    public SpringCamera2D camera;
    public List<Bullet> bullets;

    PackedScene bulletScene = (PackedScene)ResourceLoader.Load("res://Scenes//bullet.tscn");
    PackedScene bulletHitAnimation = (PackedScene)ResourceLoader.Load("res://Scenes//ParticleScenes//GunHit.tscn");
    PackedScene bulletShotAnimScene = (PackedScene)ResourceLoader.Load("res://Scenes//ParticleScenes//ShotAnimation.tscn");
    GpuParticles2D bulletShotAnim;

    public override void _Ready()
    {
        gunData = new GunData();
        timeController = new TimeController(0, gunData.gunCooldown);
        bullets = new List<Bullet>();
        bulletShotAnim = bulletShotAnimScene.Instantiate<GpuParticles2D>();
        AddChild(bulletShotAnim);
        camera = odm.GetParent().GetNode<SpringCamera2D>("SpringCamera2D");
    }
    public override void _PhysicsProcess(double delta)
    {
        if (odm.modifyGun)
        {
            gunData.Reset();
            foreach (GunMod gunMod in odm.gunMods)
            {
                gunMod.Modify(gunData);
            }
            odm.modifyGun = false;

            bulletShotAnim.Lifetime = 0.07f * gunData.bulletCount;
            bulletShotAnim.Amount = 20 * gunData.bulletCount;
        }

        if (odm.shoot && timeController.CooldownDone() && !odm.disableMovement)
        {
            for (int i = 0; i < gunData.bulletCount; i++)
            {
                Node2D bullet;
                if (bullets.Count <= 0)
                {
                    bullet = bulletScene.Instantiate<Node2D>();
                    AddChild(bullet);
                }
                else
                {
                    bullet = bullets[0];
                    ((Bullet)bullet).Activate();
                    bullets.RemoveAt(0);
                }
                camera.AddShake(1);
                if(gunData.bulletCount == 1)
                {
                    bulletShotAnim.GlobalPosition = odm.GlobalPosition + odm.GetGunDir().Normalized() * (muzzleDist);
                    bulletShotAnim.Rotation = odm.GetGunDir().Angle();
                    bulletShotAnim.Emitting = true;
                }
                timeController.ChangeActionDuration(gunData.gunCooldown);
                bullet.GlobalPosition = odm.GlobalPosition + odm.GetGunDir().Normalized() * (muzzleDist +  gunData.scale * 3);
                ((Bullet)bullet).Init(odm.GetGunDir().Normalized(), gunData.speed, gunData.bulletTime, this, bulletHitAnimation.Instantiate<GpuParticles2D>(),gunData.scale * bulletScaleScaler,odm.gunMods,gunData.applyGravity,4,gunData.damage);
                bullet.Rotation = odm.GetGunDir().Angle();
            }
            if (gunData.bulletCount > 1)
            {
                bulletShotAnim.GlobalPosition = odm.GlobalPosition + odm.GetGunDir().Normalized() * (muzzleDist);
                bulletShotAnim.Rotation = (odm.GetGunDir().Rotated(averageRotationAmount / gunData.bulletCount)).Angle();
                bulletShotAnim.Emitting = true;
            }
            timeController.Start();
            averageRotationAmount = 0;
        }
       
    }
}
