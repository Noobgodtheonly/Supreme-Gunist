using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics.Arm;

[GlobalClass]
public partial class Bullet : Node2D
{
    Vector2 velocity;
    public Vector2 dir;
    public float speed;
    public float scale;
    public float gravitationalScale = 4;
    public float damage;
    TimeController actionTimer;
    public PlayersGun playerGun;
    Area2D collision;
    GpuParticles2D hitAnim;
    public Random random = new Random();
    List<GunMod> gunMods;
    private bool activate = true;
    private bool isInThePool = false;
    public bool applyGravity;
    public bool shouldDefaultHit;
    public bool shouldDefaultExplodeOnTime;
    public bool shouldDefaultMove;
    public bool applyInitialYVelocity = true;
    public bool changeSomthingForSingleFrame = true;
    public void Init(Vector2 dir, float speed,float time, PlayersGun playerGun,GpuParticles2D hitAnim,float scale, List<GunMod> gunMods,bool applyGravity,float gravitationalScale, float damage)
    {
        velocity = Vector2.Zero;
        this.dir = dir;
        this.gravitationalScale = gravitationalScale;
        this.speed = speed;
        actionTimer = new TimeController(time,0);
        this.playerGun = playerGun;
        if (!isInThePool)
        {
            collision = GetNode<Area2D>("Area2D");
            collision.BodyEntered += OnCollision;
        }
        this.hitAnim = hitAnim;
        this.scale = scale;
        actionTimer.Start();

        ((Node2D)this).Scale = Vector2.One * scale/ playerGun.bulletScaleScaler;

        hitAnim.Lifetime = 0.21 * scale;
        hitAnim.Amount = (50) + 50 * (int)scale + 1;

        this.gunMods = gunMods;

        shouldDefaultHit = true;
        shouldDefaultExplodeOnTime = true;
        shouldDefaultMove = true;
        this.applyGravity = applyGravity;
        applyInitialYVelocity = true;
        changeSomthingForSingleFrame = true;
        this.damage = damage;
    }
    public override void _PhysicsProcess(double delta)
    {
        if(hitAnim.GetParent() == null)
           GetTree().CurrentScene.GetNode<SubViewport>("SubViewport").GetNode<Node2D>("Node2D").AddChild(hitAnim);
        if (!activate) return;
        actionTimer.Update((float)delta);
        if (actionTimer.ActionDone())DeActivate();

        foreach (GunMod mod in gunMods)
        {
            mod.movementStyle(this);
        }
        if (changeSomthingForSingleFrame)
        {
            foreach (GunMod mod in gunMods)
            {
                mod.ChangeMovementForSingleFrame(this);
                changeSomthingForSingleFrame = false;
            }
        }
        if (applyGravity)
        {
            if (applyInitialYVelocity)
            {
                float capTime = actionTimer.actionDuration > 0.5f ? 0.5f : actionTimer.actionDuration;
                velocity.Y = ((dir.Y * speed * capTime) - 0.5f * gravitationalScale * capTime * capTime) / capTime;

                velocity.X = dir.X * speed * capTime;
            }
            ApplyGravity((float)delta);
            Position += velocity;
            ((Bullet)this).Rotation = velocity.Angle();
            applyInitialYVelocity = false;
        }
        else if (shouldDefaultMove)
        {
            velocity += dir * speed * (float)delta;
            Position += velocity;
        }
    }


    public void DeActivate()
    {
        foreach (GunMod mod in gunMods)
        {
            mod.OnActionTimerDone(this);
        }
        if (shouldDefaultExplodeOnTime)
        {
            activate = false;
            hitAnim.GlobalPosition = this.GlobalPosition;

            hitAnim.Emitting = false;
            hitAnim.Emitting = true;
            playerGun.bullets.Add(this);
            this.Visible = false;
            this.SetProcess(false);
            this.SetPhysicsProcess(false);
            collision.SetDeferred("monitoring", false);
            isInThePool = true;
        }  
    }
    public void Activate()
    {
        activate = true;
        this.Visible = true;
        this.SetProcess(true);
        this.SetPhysicsProcess(true);
        collision.SetDeferred("monitoring", true);
    }
    public void OnCollision(Node body)
    {
        foreach (GunMod mod in gunMods)
        {
            mod.OnHit(this);
        }
        if (shouldDefaultHit)
        {
            if ((body as DistributionSystem) != null) return;
            IEntity entity = body as IEntity;

            if (entity != null) entity.Attacked(damage);

            hitAnim.GlobalPosition = this.GlobalPosition;
            activate = false;

            hitAnim.Emitting = false;
            hitAnim.Emitting = true;
            playerGun.camera.AddShake(scale);
            playerGun.bullets.Add(this);
            this.Visible = false;
            this.SetProcess(false);
            this.SetPhysicsProcess(false);
            collision.SetDeferred("monitoring", false);
            isInThePool = true;
            
        }
    }
    public void ApplyGravity(float time)
    {
       
        velocity.Y += gravitationalScale * time;
    }
}
