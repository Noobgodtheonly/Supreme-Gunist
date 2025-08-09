using Godot;
using System;

public partial class ButtonMovementGuid : Node
{
    public GunMod modThatFollows;
    public Button buttonatBeginingPosition;
    public Button buttonAtEndPos;
    public GunMod modThatGetsMisplaced;
    public bool lerpingToButtonPos;
}
