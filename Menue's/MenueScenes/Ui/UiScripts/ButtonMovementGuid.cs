using Godot;
using System;

public partial class ButtonMovementGuid : Node
{
    public GunMod modThatFollows;
    public Button buttonAtBeginingPosition;
    public Button buttonAtEndPos;
    public IModHBoxContainerScript targetContainer;
    public IModHBoxContainerScript beginingContainer;
    public ButtonMovementGuid guidToMisplace;
    public bool lerpingToButtonPos;

    public void Reset()
    {
        modThatFollows = null;
        buttonAtBeginingPosition = null;
        guidToMisplace = null;
        buttonAtEndPos = null;
        targetContainer = null;
        beginingContainer = null;
        lerpingToButtonPos = false;
    }
}
