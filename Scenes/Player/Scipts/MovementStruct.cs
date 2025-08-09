using Godot;
using System;
using System.Diagnostics;

public struct MovementStruct
{
    private float gravitationalScale;
    private float gravitationalScaler;

    public bool jump;
    public bool jumping;
    public bool wallJump;
    public bool wallJumping;
    public bool dash;
    public bool dashing;
    public bool changeAfterImageCol;

    float targetXVelocity;
    float maxSpeed;
    float acceleration = 2800f;
    float friction = 3600;
    float frictionScaler;


    DistributionSystem odm;
    CharacterBody2D target;


    public Vector2 velocity;
    Vector2 dirBetweenMouseAndPlayer = Vector2.Zero;
    float wallJumpDir = -0.50f;

    public MovementStruct(Vector2 velocity, float gravitationalScale, float maxSpeed, DistributionSystem odm,CharacterBody2D target)
    {
        this.velocity = velocity;
        this.gravitationalScale = gravitationalScale;
        this.target = target;
        this.maxSpeed = maxSpeed;
        this.odm = odm;
        this.target = target;

        jump = false;
        jumping = false;
        dash = false;
        dashing = false;

        gravitationalScaler = 1;
    }
    public void ApplyGravity(float time)
    {
        if (odm.OnGround || !odm.stateTransition.CouldTransitionTo(PlayerStates.Falling)) {

            if (odm.animationPlayer.CurrentAnimation == "Falling")
                odm.animationPlayer.Stop();

            return;
        }
        if (odm.animationTransition.CouldTransitionTo(PlayerStates.Falling))
            odm.animationPlayer.Play("Falling");
        velocity.Y += gravitationalScale * gravitationalScaler * time;
        gravitationalScaler = 1;
    }
    public void ApplyFriction(float time)
    {
        if (!odm.stateTransition.CouldTransitionTo(PlayerStates.OnWall) || odm.getMovementInputeStruct.moveDir == 0) return;
        velocity.Y += -MathF.Sign(velocity.Y) * 1700 * time;
    }
    public void ChangePos(float time)
    {
        
        velocity.X = Mathf.MoveToward(velocity.X,  targetXVelocity , (odm.getMovementInputeStruct.moveDir != 0 && odm.stateTransition.CouldTransitionTo(PlayerStates.Walk)   ? acceleration : friction * frictionScaler) * time);
        target.Velocity = velocity;
        target.MoveAndSlide();

        target.Position = target.Position.Round();
        velocity = target.Velocity;


        if (target.IsOnWall()) ForceCancelDash();


        odm.OnGround = target.IsOnFloor();
        odm.TochingWallOnly = target.IsOnWallOnly();

        targetXVelocity = 0;
        frictionScaler = 1;
        ChangeDir();
     
    }
    public bool IsWalking()
    {
        return odm.getMovementInputeStruct.moveDir != 0;
    }
    public void ChangeDir()
    {
        if (odm.disableMovement) return;
        dirBetweenMouseAndPlayer = (odm.GetGunDir()).Normalized();

        if (dirBetweenMouseAndPlayer.X == 0 || !odm.canChangeDir) return;
        if ((odm.facingRight && dirBetweenMouseAndPlayer.X > 0) || (!odm.facingRight && dirBetweenMouseAndPlayer.X < 0))
        {
            odm.facingRight = !odm.facingRight;
            odm.GetNode<Sprite2D>("Sprite2D").FlipH = odm.facingRight;
        }
    }
    public void Jump()
    {
        if (!jump && !jumping) return;
        if ((!odm.cyotiTimeController.ActionDone()  || odm.jumpAmountRemaning > 0 ) && ((jump && odm.OnGround) || (odm.getMovementInputeStruct.jumpJustPressed)) && odm.stateTransition.CouldTransitionTo(PlayerStates.Jump) && !odm.TochingWallOnly) {
            var somthing = Mathf.Sqrt(2 * odm.jumpGravityScaleCurve.Sample(0)  * gravitationalScale* 120);

            velocity.Y = -somthing;
            odm.jumpTimeController.ChangeActionDuration(somthing / (odm.averageJumpGravitationalScale * gravitationalScale));
            odm.jumpTimeController.Start();
            jumping = true;
            odm.OnGround = false;
            if (!odm.cyotiTimeController.ActionDone())
            {
                odm.cyotiTimeController.Reset();
            }
            else
                odm.jumpAmountRemaning--;

            odm.animationPlayer.Play("Jump");
        }
        jump = false;
        if (odm.jumpTimeController.ActionDone())
        {
            jumping = false;
            odm.animationPlayer.Stop();
            return;
        }
        if (odm.getMovementInputeStruct.jumpJustRealiced)
        {
            if(velocity.Y < -5f)
            {
                velocity.Y = -5f;
            }
            odm.jumpTimeController.Reset();
            gravitationalScaler = 1;

            jumping = false;
            odm.animationPlayer.Stop();
            return;
        }
        if (jumping)
        {
            gravitationalScaler = odm.jumpGravityScaleCurve.Sample(odm.jumpTimeController.currTime / odm.jumpTimeController.actionDuration);
        }
    }
    public void ForceCancelJump()
    {
        if (odm.jumpTimeController.ActionDone()) return;
        odm.jumpTimeController.Reset();
        velocity.Y = 0;
    }

    public void WallJump()
    {
        if (!wallJump || jumping) return;
        if(odm.stateTransition.CouldTransitionTo(PlayerStates.Jump) && odm.GetTochingWallOnly() && odm.getMovementInputeStruct.moveDir != 0)
        {
            velocity.X =  Mathf.Sqrt(2 * acceleration * 90) * target.GetWallNormal().X;
            velocity.Y = Mathf.Sqrt(2 * gravitationalScale * 30) * wallJumpDir * 4;
            odm.wallJumpTime.ChangeActionDuration(Mathf.Abs(velocity.X) / acceleration > Mathf.Abs(velocity.Y) / gravitationalScale ? Mathf.Abs(velocity.X) / acceleration : Mathf.Abs(velocity.Y) / gravitationalScale);
            odm.wallJumpTime.Start();
            wallJumping = true;
            odm.animationPlayer.Play("Jump");
        }
        wallJump = false;
        if (odm.wallJumpTime.ActionDone())
        {
            wallJumping = false;
            odm.animationPlayer.Stop();
        }
        if (wallJumping)
        {
            if (odm.getMovementInputeStruct.moveDir == -Mathf.Sign(velocity.X))
            {
                targetXVelocity = maxSpeed * odm.getMovementInputeStruct.moveDir * 3;
            }
            else if(odm.getMovementInputeStruct.moveDir == Mathf.Sign(velocity.X))
            {
                targetXVelocity = maxSpeed * odm.getMovementInputeStruct.moveDir;
            }
            else
                targetXVelocity = 0;
            
        }
    }
    public void ForceCancelWallJump()
    {
        if (odm.wallJumpTime.ActionDone()) return;
        odm.wallJumpTime.Reset();
        velocity.Y = 0;
        velocity.X = 0;
    }

    public void Walk()
    {
        if (!odm.stateTransition.CouldTransitionTo(PlayerStates.Walk))return;
        targetXVelocity = maxSpeed * odm.getMovementInputeStruct.moveDir;
    }
    public void Dash()
    {
        if (!dash && !dashing) return;
        if (dash && odm.stateTransition.CouldTransitionTo(PlayerStates.Dash))
        {
            ForceCancelJump();
            ForceCancelWallJump();
            var somthing = Mathf.Sqrt(2 * friction * odm.dashFrictionScaleCurve.Sample(0) * 170);
            velocity.Y = 0;
            velocity.X = somthing * (odm.getMovementInputeStruct.moveDir != 0 && !odm.GetTochingWallOnly()? odm.getMovementInputeStruct.moveDir:odm.facingRight? -1: 1);
            odm.dashTimeController.ChangeActionDuration(somthing / (friction * odm.averageDashFrictionScale));
            odm.dashTimeController.Start();
            odm.animationPlayer.Play("Dash");
            dashing = true;
        }
        dash = false;
        if (odm.dashTimeController.ActionDone()) {
            dashing = false;
            (odm.GetNode<Sprite2D>("Sprite2D").Material as ShaderMaterial).SetShaderParameter("flashColThresh", 0);
            odm.dashCooldownBlinkingTimeController.ChangeActionDuration(0.2f);
            odm.animationPlayer.Stop();
        }
        if (dashing)
        {
            if (odm.dashAfterImageSpawnRate.ActionDone())
            {
                AfterImage image;
                if (odm.afterImages.Count < 1)
                {
                     image = (AfterImage)odm.afterImageScene.Instantiate<Sprite2D>();
                    odm.GetTree().CurrentScene.GetNode<SubViewport>("SubViewport").GetNode<Node2D>("Node2D").AddChild(image);
                }
                else
                {
                    image = odm.afterImages[0];
                    odm.afterImages.RemoveAt(0);
                }
                image.Init(odm, 0.5f, odm.Position, odm.facingRight,changeAfterImageCol);
                odm.dashAfterImageSpawnRate.Start();
                changeAfterImageCol = !changeAfterImageCol;
            }
            (odm.GetNode<Sprite2D>("Sprite2D").Material as ShaderMaterial).SetShaderParameter("flashColThresh", 1);

            frictionScaler = odm.dashFrictionScaleCurve.Sample(odm.dashTimeController.currTime /odm.dashTimeController.actionDuration);
            if(odm.getMovementInputeStruct.moveDir != 0 && odm.getMovementInputeStruct.moveDir == MathF.Sign(velocity.X))
            {
                targetXVelocity = maxSpeed * odm.getMovementInputeStruct.moveDir;
            }
            else
                targetXVelocity = 0;
        }
    }
    public void ForceCancelDash()
    {
        if (odm.dashTimeController.ActionDone()) return;
        (odm.GetNode<Sprite2D>("Sprite2D").Material as ShaderMaterial).SetShaderParameter("flashColThresh", 0);
        odm.dashTimeController.currTime = odm.dashTimeController.actionDuration;
        odm.dashCooldownBlinkingTimeController.ChangeCooldownDuration(0.2f);
        velocity.X = 0;
    }
}
