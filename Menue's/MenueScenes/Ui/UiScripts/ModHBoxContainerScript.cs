using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

public interface IModHBoxContainerScript
{
     Dictionary<Button, GunMod> buttons { get; set; }
     Dictionary<string, StyleBoxTexture> textures { get; set; }
     Dictionary<Button, ButtonMovementGuid> buttonMovementGuidesDict { get; set; }
     public string[] states { get; set; }
}
