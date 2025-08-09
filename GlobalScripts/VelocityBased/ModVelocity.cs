using Godot;
using System;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Runtime.Intrinsics.Arm;
[GlobalClass]
public partial class ModVelocity : CharacterBody2D
{
    // The velocity is now a built-in property of CharacterBody2D, but we still need
    // a public one to be able to set it from outside this script.
    public Vector2 velocity;

    // We no longer need these, as CharacterBody2D handles them.
    // public Vector2 finalPoint = Vector2.Zero;
    // public Vector2 normal = Vector2.Zero;
    // public RayCast2D raycast;

    // We still need a reference to the parent and the gravity script.
    public Node2D parent;
    public Gravity gravity;
    public float targetXVelocity = 0;

    public override void _Ready()
    {
        // The parent of this script is now the CharacterBody2D itself, so we can
        // remove this line:
        // parent = (Node2D)GetParent();

        // We still get the Gravity node, which is a sibling.
        gravity = GetParent().GetNode<Gravity>("Gravity");
        parent = (Node2D)GetParent();

        // The CharacterBody2D handles its own collisions, so we don't need to
        // add exceptions to a RayCast2D.
        // foreach (var node in GetTree().GetNodesInGroup("IEntity"))
        // {
        //     raycast.AddException((CharacterBody2D)node);
        // }
        foreach (var node in GetTree().GetNodesInGroup("IEntity"))
        {

            AddCollisionExceptionWith(node);

        }
    }

    public override void _PhysicsProcess(double delta)
    {
        // Apply gravity if the character is not on the floor.
        if (!IsOnFloor())
        {
            gravity.ApplyGravity((float)delta, ref velocity);
        }
        else
        {
            // Reset vertical velocity when on the floor to prevent it from building up.
            velocity.Y = 0;
        }

        // Apply friction or damping to horizontal velocity.
        velocity.X = Mathf.MoveToward(velocity.X, targetXVelocity, 100 * (float)delta);

        // --- THIS IS THE KEY CHANGE ---
        // We use the built-in Velocity property of CharacterBody2D,
        // and let MoveAndSlide() handle all the collision and movement.
        Velocity = velocity;
        MoveAndSlide();
        parent.GlobalPosition = GlobalPosition;
        Position = Vector2.Zero;

        // After MoveAndSlide, we can check if the character is on the floor.
        // This is much more reliable than using a raycast.
       
    }
}
