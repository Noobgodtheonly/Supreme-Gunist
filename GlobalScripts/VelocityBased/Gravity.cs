using Godot;
using System;

[GlobalClass]
public partial class Gravity : Node
{
    public float gravitationalScale = 300;
    public void ApplyGravity(float time,ref Vector2 velocity)
    {
        velocity.Y += gravitationalScale * time;
    }
}
