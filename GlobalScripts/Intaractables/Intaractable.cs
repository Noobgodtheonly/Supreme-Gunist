using Godot;
using System;

[GlobalClass]
public partial class Intaractable : Node
{
    Area2D area;
    DistributionSystem player;
    IntaractablesManual intaractOutcome;

    public override void _Ready()
    {
        area = GetParent().GetNode<Area2D>("PickUpArea");
        intaractOutcome = GetParent().GetNode<IntaractablesManual>("IntaractWithMods");
    }
    public override void _PhysicsProcess(double delta)
    {
        foreach (Node body in area.GetOverlappingBodies())
        {
            Collided(body);
        }
    }
    public void Collided(Node body)
    {
        player = body as DistributionSystem;
        if (player == null)return;
        if (player.getMovementInputeStruct.intaract)
        {
            intaractOutcome.IntaractAftermath(player);
        }
    }
}
