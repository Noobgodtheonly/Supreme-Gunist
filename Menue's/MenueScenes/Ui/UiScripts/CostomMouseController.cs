using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;
using static Godot.WebSocketPeer;

[GlobalClass]

public partial class CostomMouseController : CanvasLayer
{
    [Export] DistributionSystem odm;
    public Vector2[] edges;
    public Area2D collider;
    private Node2D cage;
    RectangleShape2D shape;

    private List<ButtonMovementGuid> modsInAction;
    private ButtonMovementGuid buttonBeingHeld;

 
    private Vector2 threeTimesOfModSize = Vector2.One * 3;


    private int modZIndex;
    

    public override void _Ready()
    {

        collider = GetNode<Node2D>("Cage").GetNode<Area2D>("HBox");
        shape = collider.GetNode<CollisionShape2D>("Collider").Shape as RectangleShape2D;
        cage = GetNode<Node2D>("Cage");

        modsInAction = new();

        Input.MouseMode = Input.MouseModeEnum.Hidden;

        
    }
    public override void _PhysicsProcess(double delta)
    {
        cage.Position =  GetViewport().GetMousePosition();
        edges = GetGlobalCorners(shape, collider.GetNode<CollisionShape2D>("Collider"));

        IsButtonPressed();
        MouseFollow();
        LerpToButtonPos((float)delta);
        
    }
    
    public void MouseFollow()
    {
        if (buttonBeingHeld == null || buttonBeingHeld.modThatFollows == null) return;

        if (odm.shoot && buttonBeingHeld.buttonAtBeginingPosition.IsVisibleInTree())
        {
            buttonBeingHeld.modThatFollows.GlobalPosition = cage.GlobalPosition;

            TryDropMod();

        }
        else
        {
            foreach(var guid in modsInAction)
            {
                guid.lerpingToButtonPos = true;
            }
            buttonBeingHeld.modThatFollows.ZIndex = 998;
            if(buttonBeingHeld.buttonAtEndPos != null)
            {
                buttonBeingHeld.beginingContainer.buttons[buttonBeingHeld.buttonAtBeginingPosition] = null;
                buttonBeingHeld.targetContainer.buttons[buttonBeingHeld.buttonAtEndPos] = buttonBeingHeld.modThatFollows;
            }
            if(buttonBeingHeld.guidToMisplace != null)
            {
                buttonBeingHeld.guidToMisplace.targetContainer.buttons[buttonBeingHeld.guidToMisplace.buttonAtEndPos] = buttonBeingHeld.guidToMisplace.modThatFollows;
            }
            buttonBeingHeld = null;
        }
        
        
    }
    private void TryDropMod()
    {
        foreach (Node node in GetTree().GetNodesInGroup("Container"))
        {

            IModHBoxContainerScript container = node as IModHBoxContainerScript;

            if (container != null)
            {
                foreach (Button button in container.buttons.Keys)
                {
                    if (button != null && button != buttonBeingHeld.buttonAtBeginingPosition)
                    { 
                        Vector2 buttonGlobalPos = button.GlobalPosition + button.Size / 2;

                        if (PointInQuad(edges, buttonGlobalPos))
                        {
                            buttonBeingHeld.buttonAtEndPos = button;
                            buttonBeingHeld.targetContainer = container;

                            if (container.buttons[button] != null && (buttonBeingHeld.guidToMisplace == null || buttonBeingHeld.guidToMisplace != container.buttonMovementGuidesDict[button]))
                            {
                                if((buttonBeingHeld.guidToMisplace != null && buttonBeingHeld.guidToMisplace != container.buttonMovementGuidesDict[button]))
                                {
                                    container.buttonMovementGuidesDict[button].buttonAtEndPos = null;
                                    container.buttonMovementGuidesDict[button].targetContainer = container.buttonMovementGuidesDict[button].beginingContainer;
                                    EndModMovement(buttonBeingHeld.guidToMisplace);
                                    buttonBeingHeld.guidToMisplace = null;
                                }


                                buttonBeingHeld.guidToMisplace = container.buttonMovementGuidesDict[button];
                                SetModToFollow(container.buttons[button], container.buttonMovementGuidesDict[button], container, button);
                                container.buttons[button].ZIndex = 998;

                                container.buttonMovementGuidesDict[button].buttonAtEndPos = buttonBeingHeld.buttonAtBeginingPosition;
                                container.buttonMovementGuidesDict[button].targetContainer = buttonBeingHeld.beginingContainer;
                            }
                            return;
                        }
                        else
                        {
                            buttonBeingHeld.buttonAtEndPos = null;
                            buttonBeingHeld.targetContainer = buttonBeingHeld.beginingContainer;

                            if(buttonBeingHeld.guidToMisplace != null && buttonBeingHeld.guidToMisplace == container.buttonMovementGuidesDict[button])
                            {
                                container.buttonMovementGuidesDict[button].buttonAtEndPos = null;
                                container.buttonMovementGuidesDict[button].targetContainer = container.buttonMovementGuidesDict[button].beginingContainer;
                                EndModMovement(buttonBeingHeld.guidToMisplace);
                                buttonBeingHeld.guidToMisplace = null;
                            }
                        }
                    }
                }
            }
        }
    }
    private void LerpToButtonPos(float time)
    {
        if (modsInAction.Count < 1) return;

        foreach(var guid in modsInAction.ToList())
        {
            if (guid.lerpingToButtonPos)
            {
                if (guid.buttonAtEndPos == null)
                {
                    
                    Vector2 buttonCentre = guid.buttonAtBeginingPosition.GlobalPosition + guid.buttonAtBeginingPosition.Size / 2;
                    guid.modThatFollows.GlobalPosition = guid.modThatFollows.GlobalPosition.Lerp(buttonCentre, time * 20);

                    if ((buttonCentre - guid.modThatFollows.GlobalPosition).Length() <= 15)
                    {
                        EndModMovement(guid);
                    }
                    
                }
                else
                {
                    Vector2 buttonCentre = guid.buttonAtEndPos.GlobalPosition + guid.buttonAtEndPos.Size / 2;
                    guid.modThatFollows.GlobalPosition = guid.modThatFollows.GlobalPosition.Lerp(buttonCentre, time * 20);

                    if ((buttonCentre - guid.modThatFollows.GlobalPosition).Length() <= 15)
                    {
                        EndModMovement(guid);
                    }
                }
            }
        }
    }

    private void EndModMovement(ButtonMovementGuid guid)
    {
        GD.Print(guid.modThatFollows == null);
        guid.modThatFollows.ZIndex = modZIndex;
        guid.modThatFollows.Visible = false;



        if(guid.buttonAtEndPos == null)
        {
            foreach(var state in guid.targetContainer.states)
                guid.buttonAtBeginingPosition.AddThemeStyleboxOverride(state, guid.targetContainer.textures[guid.modThatFollows.GetType().Name]);
        }
        else
        {
            foreach (var state in guid.targetContainer.states)
                guid.buttonAtEndPos.AddThemeStyleboxOverride(state, guid.targetContainer.textures[guid.modThatFollows.GetType().Name]);
        }

       
        modsInAction.Remove(guid);
       

        ModEnhancementScriptContainer slot = guid.targetContainer as ModEnhancementScriptContainer;
        if (slot != null)
        {
            slot.buttonWasChanged = true;
        }

        guid.Reset();
    }
    public void IsButtonPressed()
    {
        if (buttonBeingHeld != null) return;

        foreach(Node node in GetTree().GetNodesInGroup("Container"))
        {
            
            IModHBoxContainerScript container = node as IModHBoxContainerScript;
            
            if(container != null)
            {
                foreach(Button button in container.buttons.Keys)
                {
                    if(button != null)
                    {
                        if (!button.IsVisibleInTree()) return;
                        Vector2 buttonGlobalPos = button.GlobalPosition + button.Size/ 2;

                        if (PointInQuad(edges, buttonGlobalPos) && container.buttonMovementGuidesDict[button].buttonAtBeginingPosition == null)
                        {
                            if (odm.parry)
                            {
                                ModStorageScript modStorageScript = container as ModStorageScript;

                                if (modStorageScript != null)
                                {
                                    modStorageScript.ThrowModAway(button);
                                    return;
                                }
                            }
                            //This if statement is to see if i could carry a mod that is there aka shoot and !null
                            if (odm.shoot && container.buttons[button] != null)
                            {
                                

                                SetModToFollow(container.buttons[button], container.buttonMovementGuidesDict[button],container, button);
                                buttonBeingHeld = container.buttonMovementGuidesDict[button];
                                
                                return;
                               
                            }

                        }
                    }
                }
            }
        }
    }
    public void SetModToFollow(GunMod mod,ButtonMovementGuid movementGuid, IModHBoxContainerScript container, Button button)
    {
        //Putting this in the cursors canvas layer
        mod.GetParent().RemoveChild(mod);
        mod.Owner = null;
        AddChild(mod);
        // end of it

        //So the scale can be halfed and making it visible
        mod.Scale = threeTimesOfModSize;
        mod.Visible = true;

        modsInAction.Add(movementGuid);

        modZIndex = mod.ZIndex;
        mod.ZIndex = 999;

        movementGuid.buttonAtBeginingPosition = button;
        movementGuid.targetContainer = container;
        movementGuid.beginingContainer = container;

        mod.GlobalPosition = cage.GlobalPosition;

        foreach (var state in container.states)
        {
            button.AddThemeStyleboxOverride(state, container.textures["Reset"]);
        }

        movementGuid.modThatFollows = mod;

        
    }
    public Vector2[] GetGlobalCorners(RectangleShape2D shape, CollisionShape2D shapeNode)
    {
        Vector2 extents = shape.Size / 2f;
        Transform2D globalXform = shapeNode.GlobalTransform;

        return new Vector2[]
        {
        globalXform * new Vector2(-extents.X, -extents.Y), // Top-left
        globalXform * new Vector2(extents.X, -extents.Y),  // Top-right
        globalXform * new Vector2(extents.X, extents.Y),   // Bottom-right
        globalXform * new Vector2(-extents.X, extents.Y),  // Bottom-left
        };
    }
    public bool PointInQuad(Vector2[] quad, Vector2 point)
    {
        bool SameSide(Vector2 a, Vector2 b, Vector2 c, Vector2 p)
        {
            float cp1 = (b - a).Cross(c - a);
            float cp2 = (b - a).Cross(p - a);
            return cp1 * cp2 >= 0;
        }

        return
            SameSide(quad[0], quad[1], quad[2], point) &&
            SameSide(quad[1], quad[2], quad[3], point) &&
            SameSide(quad[2], quad[3], quad[0], point) &&
            SameSide(quad[3], quad[0], quad[1], point);
    }
}
