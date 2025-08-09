using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics.Arm;

public enum PlayerStates
{
    Jump,
    Walk,
    Dash,
    Falling,
    OnWall
}

public partial class DistributionSystem : CharacterBody2D, IEntity
{
    [Export] public Curve jumpGravityScaleCurve;
    [Export] public Curve dashFrictionScaleCurve;
    public float cyotiTime;

    public delegate void timeControllerDelegat(float time);
    public event timeControllerDelegat nowUpdateTime;
    public TimeController jumpTimeController;
    public TimeController dashTimeController;
    public TimeController wallJumpTime;
    public TimeController cyotiTimeController;
    public TimeController dashAfterImageSpawnRate;
    public TimeController dashCooldownBlinkingTimeController;

    // A variable to hold the bullet scene. Make sure to load your bullet scene here.
    [Export] public PackedScene BulletScene;

    public PackedScene afterImageScene = (PackedScene)ResourceLoader.Load("res://Scenes/Player/dash_after_image.tscn");
    public AnimationPlayer animationPlayer;
    public MovementStruct getMovementStruct;
    public InputeStruct getMovementInputeStruct;
    public List<AfterImage> afterImages = new();
    public List<PlayerStates> statesList;
    public StateTransition<PlayerStates> stateTransition;
    public StateTransition<PlayerStates> animationTransition;
    public List<GunMod> gunMods = new List<GunMod>();
    private float gravitationalScale = 500f * 4;
    public float averageJumpGravitationalScale;
    public float averageDashFrictionScale;
    private float speed = 75 * 4;
    public int maxJumpAmount = 2;
    public int jumpAmountRemaning = 2;
    public bool facingRight = true;
    public bool canChangeDir;
    public bool shoot;
    public bool parry;
    public bool modifyGun = false;
    public bool disableMovement;
    private bool tochingWallOnly;
    public bool TochingWallOnly
    {
        get
        {
            return tochingWallOnly;
        }
        set
        {
            tochingWallOnly = value;
            if (tochingWallOnly)
            {
                if (canChangeDir)
                {
                    bool lookDir = (GetWallNormal().X == -1) ? true : false;
                    GetNode<Sprite2D>("Sprite2D").FlipH = lookDir;
                    facingRight = lookDir;
                    canChangeDir = false;
                }
                jumpAmountRemaning = maxJumpAmount - 1;
                cyotiTimeController.Reset();
            }
            if (!tochingWallOnly)
                canChangeDir = true;
        }
    }
    private bool onGround;
    public bool OnGround
    {
        get
        {
            return onGround;
        }
        set
        {
            if (value == true)
            {
                onGround = value;
                FloorSnapLength = 4 * 4;
                ApplyFloorSnap();
                jumpAmountRemaning = maxJumpAmount;
            }
            else
            {
                if (onGround != value && jumpAmountRemaning == maxJumpAmount)
                {
                    cyotiTimeController.Start();
                    jumpAmountRemaning--;
                }
                onGround = value;
            }
        }
    }

    public override void _Ready()
    {
        float jumpGravityScaleAvg = 0;
        float dashFrictionScaleAvg = 0;

        for (float i = 0; i < 100; i++)
        {
            float t = i / 99f;
            jumpGravityScaleAvg += jumpGravityScaleCurve.Sample(t);
            dashFrictionScaleAvg += dashFrictionScaleCurve.Sample(t);

        }
        averageJumpGravitationalScale = jumpGravityScaleAvg / 100;
        averageDashFrictionScale = dashFrictionScaleAvg / 100;

        animationPlayer = GetNode<Sprite2D>("Sprite2D").GetNode<AnimationPlayer>("AnimationPlayer");
        animationPlayer.Play("Idle");

        statesList = new List<PlayerStates>();
        stateTransition = new StateTransition<PlayerStates>();
        animationTransition = new();
        stateTransition.Begin();
        animationTransition.Begin();


        getMovementStruct = new MovementStruct(Vector2.Zero, gravitationalScale, speed, this, this);
        getMovementInputeStruct = new InputeStruct(this);

        jumpTimeController = new TimeController(0, 0);
        dashTimeController = new TimeController(0, 0.75f);
        wallJumpTime = new TimeController(0, 0);
        cyotiTimeController = new TimeController(cyotiTime, 0);
        dashAfterImageSpawnRate = new TimeController(0.05f, 0);
        dashCooldownBlinkingTimeController = new TimeController(0.005f, 0);

        nowUpdateTime += dashAfterImageSpawnRate.Update;
        nowUpdateTime += jumpTimeController.Update;
        nowUpdateTime += dashTimeController.Update;
        nowUpdateTime += wallJumpTime.Update;
        nowUpdateTime += cyotiTimeController.Update;
        nowUpdateTime += getMovementInputeStruct.cannotIntaractForNow.Update;
        nowUpdateTime += dashCooldownBlinkingTimeController.Update;

        stateTransition.AddConditionTo(PlayerStates.Jump, jumpTimeController.ActionDone);
        stateTransition.AddConditionToAllExept(dashTimeController.ActionDone, null);
        stateTransition.AddConditionTo(PlayerStates.Dash, dashTimeController.CooldownDone);
        stateTransition.AddConditionToAllExept(wallJumpTime.ActionDone, new List<PlayerStates> { PlayerStates.Dash, PlayerStates.Falling });
        stateTransition.AddConditionTo(PlayerStates.OnWall, GetTochingWallOnly);

        animationTransition.AddConditionTo(PlayerStates.Walk, getMovementStruct.IsWalking);
        animationTransition.AddConditionTo(PlayerStates.Walk, jumpTimeController.ActionDone);
        animationTransition.AddConditionTo(PlayerStates.Walk, wallJumpTime.ActionDone);
        animationTransition.AddConditionTo(PlayerStates.Falling, jumpTimeController.ActionDone);
        animationTransition.AddConditionTo(PlayerStates.Falling, wallJumpTime.ActionDone);
        animationTransition.AddConditionToAllExept(dashTimeController.ActionDone, new List<PlayerStates> { PlayerStates.Dash });

        getMovementStruct.ChangeDir();
        AddToGroup("IEntity");
    }

    public override void _PhysicsProcess(double delta)
    {
        nowUpdateTime((float)delta);
        getMovementInputeStruct.CheckInput();
        CheckState((float)delta);
        getMovementStruct.ChangePos((float)delta);
        UpdateAfterEffects();
        ResetUpdateThings();
        if (!animationPlayer.IsPlaying()) animationPlayer.Play("Idle");
    }
    public void Attacked(float damage)
    {
        if (dashTimeController.ActionDone())
            GD.Print("Oh noooo, i'm damaged");
    }

    private void ResetUpdateThings()
    {
        statesList.Clear();
        getMovementInputeStruct.moveDir = 0;
    }

    public Vector2 GetGunDir()
    {
        // Get the mouse position from the *main* viewport in screen coordinates.
        Vector2 mouseScreenPosition = GetTree().Root.GetMousePosition();

        // Find the active Camera2D node in the player's current viewport.
        Camera2D camera = GetViewport().GetCamera2D();

        if (camera == null)
        {
            // Fallback if no camera is found.
            GD.PrintErr("No Camera2D found in the Viewport. Returning a default direction.");
            return Vector2.Right;
        }

        // Get the viewport the player is in.
        var playerViewport = GetViewport();

        // This is the crucial step: convert the main screen mouse position
        // to a local position within the player's sub-viewport.
        Vector2 mouseLocalViewportPosition = playerViewport.GetFinalTransform().Inverse() * mouseScreenPosition;

        // Now, get the mouse's world position within the player's sub-viewport's world.
        Vector2 mouseWorldPosition = camera.GetCanvasTransform().Inverse() * mouseLocalViewportPosition;

        // The direction is the vector from the player's global position to the
        // correctly converted mouse world position.
        Vector2 direction = mouseWorldPosition - GlobalPosition;

        // Return the unfiltered, non-normalized direction vector as requested.
        return direction;
    }

    // This is the new method for shooting your bullet.
    public void ShootBullet()
    {
        // Check if the bullet scene is assigned.
        if (BulletScene == null)
        {
            GD.PrintErr("BulletScene is not assigned in the inspector!");
            return;
        }

        // Get the direction vector. This is the vector we've been working on.
        Vector2 direction = GetGunDir();

        // Get the bullet speed. You can set this as an [Export] variable or a constant.
        float bulletSpeed = 1000f; // Adjust this value to your liking.

        // Instantiate the bullet scene.
        var bulletInstance = BulletScene.Instantiate<RigidBody2D>();

        // Set the bullet's global position to the player's position.
        bulletInstance.GlobalPosition = GlobalPosition;

        // Set the bullet's velocity.
        // We normalize the direction vector to ensure a consistent speed.
        bulletInstance.LinearVelocity = direction.Normalized() * bulletSpeed;

        // Add the bullet to the scene tree. This is crucial!
        GetParent().AddChild(bulletInstance);
    }

    private void CheckState(float delta)
    {
        if (animationTransition.CouldTransitionTo(PlayerStates.Walk) && animationPlayer.CurrentAnimation != "Walk")
            animationPlayer.Play("Walking");
        else if (animationTransition.CouldTransitionTo(PlayerStates.Walk) && animationPlayer.CurrentAnimation == "Walking")
            animationPlayer.Stop();

        foreach (PlayerStates state in statesList)
        {
            switch (state)
            {
                case PlayerStates.Jump:
                    getMovementStruct.jump = true;
                    getMovementStruct.wallJump = true;
                    break;
                case PlayerStates.Dash:
                    getMovementStruct.dash = true;
                    break;
                case PlayerStates.Walk:
                    getMovementStruct.Walk();
                    break;
            }
        }
        getMovementStruct.ApplyGravity(delta);
        getMovementStruct.ApplyFriction(delta);
        getMovementStruct.Jump();
        getMovementStruct.WallJump();
        getMovementStruct.Dash();
    }
    public void UpdateAfterEffects()
    {
        if (!dashTimeController.CooldownDone())
        {
            if (!dashCooldownBlinkingTimeController.ActionDone())
            {
                (GetNode<Sprite2D>("Sprite2D").Material as ShaderMaterial).SetShaderParameter("flashColThresh", 1);
            }
            if (dashCooldownBlinkingTimeController.CooldownDone())
            {
                dashCooldownBlinkingTimeController.ChangeCooldownDuration(dashCooldownBlinkingTimeController.cooldownDuration - 0.005f);
                dashCooldownBlinkingTimeController.Start();
                (GetNode<Sprite2D>("Sprite2D").Material as ShaderMaterial).SetShaderParameter("flashColThresh", 1);
            }
            if (dashCooldownBlinkingTimeController.ActionDone() && !dashCooldownBlinkingTimeController.CooldownDone())
            {
                (GetNode<Sprite2D>("Sprite2D").Material as ShaderMaterial).SetShaderParameter("flashColThresh", 0);
            }
        }
        else
            (GetNode<Sprite2D>("Sprite2D").Material as ShaderMaterial).SetShaderParameter("flashColThresh", 0);
    }

    public bool GetTochingWallOnly()
    {
        return TochingWallOnly;
    }
}
