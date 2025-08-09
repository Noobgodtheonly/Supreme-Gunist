using Godot;
using System;
using System.Collections.Generic;
[GlobalClass]
public partial class ModEnhancementScript : VBoxContainer
{
    public bool buttonWasChanged;
    public DistributionSystem odm;
    public override void _Ready()
    {
        foreach (var node in GetTree().GetNodesInGroup("IEntity"))
        {
            DistributionSystem odm = node as DistributionSystem;
            if(odm != null)
            {
                this.odm = odm;
                return;
            }
        }
    }
    public override void _PhysicsProcess(double delta)
    {
        if (buttonWasChanged)
        {
            odm.modifyGun = true;
            odm.gunMods.Clear();
            foreach(ModEnhancementScriptContainer slot in GetChildren())
            {
                slot.buttonWasChanged = false;
                foreach(Button button in slot.buttons.Keys)
                {
                    if(slot.buttons[button] != null)
                    {
                        odm.gunMods.Add(slot.buttons[button]);
                    }
                }
            }
        }
        buttonWasChanged = false;
    }

}
