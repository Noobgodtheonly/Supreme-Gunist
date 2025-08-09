using Godot;
using System;
using System.Text.RegularExpressions;
[GlobalClass]
public partial class IntaractWithMods : IntaractablesManual
{

    public override void IntaractAftermath(DistributionSystem thing)
    {
        foreach(HBoxContainer container in GetTree().GetNodesInGroup("Container"))
        {
            ModStorageScript c = container as ModStorageScript;
            if (c != null) 
            c.AddToStorage((Node2D)GetParent());
        }
        ((GunMod)GetParent()).Visible = false;
        ((GunMod)GetParent()).GetNode<Intaractable>("Intaractable").SetPhysicsProcess(false);
    }
}
