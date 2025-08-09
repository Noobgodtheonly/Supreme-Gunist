using Godot;
using System;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Runtime.Intrinsics.Arm;
[GlobalClass]
public partial class ModVelocity : Node
{
    public Vector2 velocity;
    public Vector2 finalPoint = Vector2.Zero;
    public Vector2 normal = Vector2.Zero;
    public RayCast2D raycast;
    public Node2D parent;
    public Gravity gravity;
    public bool onGround;
    public float targetXVelocity = 0;

    public override void _Ready()
    {
        parent = (Node2D)GetParent();
        raycast = parent.GetNode<RayCast2D>("RayCast2D");
        gravity = parent.GetNode<Gravity>("Gravity");

        foreach (var node in GetTree().GetNodesInGroup("IEntity"))
        {
            raycast.AddException((CharacterBody2D)node);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!onGround)
            gravity.ApplyGravity((float)delta, ref velocity);

        if (velocity == Vector2.Zero) return;

        raycast.TargetPosition = (velocity.Normalized() * 4f) + velocity;
        raycast.ForceRaycastUpdate();

        if (raycast.IsColliding())
        {
            finalPoint = raycast.GetCollisionPoint();
            normal = raycast.GetCollisionNormal();

            if (Mathf.Abs(normal.Y) > 0.7f)
            {
                finalPoint.Y += (4 * Mathf.Sign(normal.Y));
                velocity.Y = 0;
                onGround = true;
            }
            else
            {
                finalPoint.Y = parent.Position.Y + velocity.Y;
                onGround = false;
            }

            if (Mathf.Abs(normal.X) == 1)
            {
                finalPoint.X += (4 * Mathf.Sign(normal.X));
                velocity.X = 0;
            }
            else
            {
                finalPoint.X = parent.Position.X + Mathf.MoveToward(velocity.X, targetXVelocity, 100 * (float)delta);
            }

            parent.Position = finalPoint;
        }
        else
        {
            onGround = false;
            velocity.X = Mathf.MoveToward(velocity.X, targetXVelocity, 100 * (float)delta);
            parent.Position += velocity;
        }
    }
}
