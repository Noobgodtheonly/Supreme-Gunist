using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]

public partial class WormThingnyScript : CharacterBody2D,IEntity
{
    public enum WormThingnyStates
    {
        Walking,
        GettingUp,
        ReadyingUp,
        Jumping
    }



    public DistributionSystem player;
    private TimeController hitTimer;
    private TimeController cooldownTime;
    private AnimationPlayer animationPlayer;
    private Vector2 dir = new Vector2(1, 0); 
    private Vector2 velocity = Vector2.Zero;
    private Node2D container;
    private RayCast2D front; 
    private RayCast2D middle;
    private Area2D damageBox;
    public Gravity gravity;
    private float speed = 80;
    public float hp = 28;
    public bool onGround;
    public bool canStartJump = true;
    WormThingnyStates state;

    public override void _Ready()
    {
        container = GetNode<Node2D>("Cage");
        front = container.GetNode<RayCast2D>("Front");
        middle = container.GetNode<RayCast2D>("Middle");
        gravity = container.GetNode<Gravity>("Gravity");
        animationPlayer = container.GetNode<Sprite2D>("Sprite").GetNode<AnimationPlayer>("AnimationPlayer");
        damageBox = container.GetNode<Area2D>("Area2D");
        gravity.gravitationalScale = 480;
        player = GetTree().CurrentScene.FindChild("Player") as DistributionSystem;
        if (player == null)
            GD.Print("There is no current player in scene");
        else
            AddCollisionExceptionWith(player);

        foreach (var node in GetTree().GetNodesInGroup("IEntity"))
        {

            front.AddException((CollisionObject2D)node);
            middle.AddException((CollisionObject2D)node);
            
        }
        hitTimer = new TimeController(0.1f, 0);
        cooldownTime = new TimeController(1.5f,0);
        FloorSnapLength = 16;
        SetRaycastDirection(dir.X);
        AddToGroup("IEntity");
        state = WormThingnyStates.Walking;
        damageBox.BodyEntered += Attack;
    }

    public override void _PhysicsProcess(double delta)
    {
        hitTimer.Update((float)delta);
        cooldownTime.Update((float)delta);
        onGround = IsOnFloor();

        if (!onGround)
        {
            gravity.ApplyGravity((float)delta, ref velocity);
        }
        else if((!cooldownTime.ActionDone() || (player.Position - Position).Length() >60) && state == WormThingnyStates.Walking)
        {
            ApplyMovement((float)delta, dir.X * speed);
        }
        else if(state == WormThingnyStates.Walking)
        {
            if(MathF.Sign((player.Position - Position).X) != Mathf.Sign(container.Scale.X)){
                Flip();
            }
            state = WormThingnyStates.GettingUp;
            velocity.X = 0;
        }
        StateCheck((float)delta);
            Velocity = velocity;
            MoveAndSlide();

            if (IsOnWall() && state == WormThingnyStates.Walking)
                Flip();

            velocity = Velocity;

            Effects();
        
        
    }
    private void Attack(Node attackable)
    {
        DistributionSystem player = attackable as DistributionSystem;
        if(player != null)
        {
            player.Attacked(1);
        }
    }
    public void Effects()
    {
        if(hitTimer.ActionDone())
            (container.GetNode<Sprite2D>("Sprite").Material as ShaderMaterial).SetShaderParameter("flashColThresh", 0);
    }
    public void StateCheck(float time)
    {
        switch (state)
        {
            case WormThingnyStates.Walking:
                animationPlayer.Stop();
                animationPlayer.Play("Moving");
                break;
            case WormThingnyStates.GettingUp:
                GetUp();
                break;
            case WormThingnyStates.ReadyingUp:
                ReadyingUp();
                break;
            case WormThingnyStates.Jumping:
                Jumping(time);
                break;
        }
    }
    public void Attacked(float damage)
    {
        hp -= damage;
        hitTimer.Start();
        (container.GetNode<Sprite2D>("Sprite").Material as ShaderMaterial).SetShaderParameter("flashColThresh", 1);
        if (hp < 1)
        this.QueueFree();
    }
    private void GetUp()
    {
        
            if(MathF.Sign((player.Position - Position).X) != Mathf.Sign(container.Scale.X)){
                Flip();
            }
        if (!animationPlayer.IsPlaying() && onGround)
        {
            state = WormThingnyStates.ReadyingUp;
            return;
        }
        if (!(animationPlayer.CurrentAnimation == "GettingUp") && onGround)
        {
            animationPlayer.Stop();
            animationPlayer.Play("GettingUp");
        }
       
    }
    private void Jumping(float time)
    {
        if (canStartJump)
        {
            animationPlayer.Stop();
            animationPlayer.Play("Moving");

            if (MathF.Sign((player.Position - Position).X) != Mathf.Sign(container.Scale.X))
            {
                Flip();
            }
            velocity.X = 700 * container.Scale.X;
            velocity.Y = -MathF.Sqrt(2 * gravity.gravitationalScale * 10 );
           

        }
        

        ApplyMovement(time, 0);
        if (onGround &&  !canStartJump)
        {
            state = WormThingnyStates.Walking;
            canStartJump = true;
            animationPlayer.Stop();
            animationPlayer.Play("Moving");
            cooldownTime.Start();
            velocity = Vector2.Zero;
            return;
        }
        canStartJump = false;
    }
    private void ReadyingUp()
    {

        if (MathF.Sign((player.Position - Position).X) != Mathf.Sign(container.Scale.X))
        {
            Flip();
        }
        if (!animationPlayer.IsPlaying() && onGround)
        {
            state = WormThingnyStates.Jumping;

        }
        if (!(animationPlayer.CurrentAnimation == "ReadyingUp") && onGround)
        {
            animationPlayer.Stop();
            animationPlayer.Play("ReadyingUp");

        }
       
    }
    private void ApplyMovement(float delta,float targetVelocity)
    {
        
        LedgeCheck();

        
        velocity.X = Mathf.MoveToward(velocity.X, targetVelocity, 550 * delta);
    }

    
    public void Flip()
    {
        dir.X = -dir.X;
        SetRaycastDirection(dir.X);

        Vector2 newScale = container.Scale;
        newScale.X = -newScale.X;
        container.Scale = newScale;
    }


    private void SetRaycastDirection(float directionX)
    {
        float frontTargetX = Mathf.Abs(front.TargetPosition.X);
        float middleTargetX = Mathf.Abs(middle.TargetPosition.X);

        front.TargetPosition = new Vector2(directionX * frontTargetX, front.TargetPosition.Y);
        middle.TargetPosition = new Vector2(directionX * middleTargetX, middle.TargetPosition.Y);
    }

   
    private void LedgeCheck()
    {
        // Update the raycast direction for the current frame
        SetRaycastDirection(dir.X);
        front.ForceRaycastUpdate();
        middle.ForceRaycastUpdate();

        // The ledge is detected if we are on the ground but the front raycast is not colliding
        if (middle.IsColliding() && !front.IsColliding() && state == WormThingnyStates.Walking)
        {
            
            velocity.X = 0;
            Flip();
        }
    }
}