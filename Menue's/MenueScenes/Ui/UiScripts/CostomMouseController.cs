using Godot;
using System;
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
    RectangleShape2D shape;
    Button buttonBeingHeld;

    public Vector2 modOffset = Vector2.Zero;
    IModHBoxContainerScript storageCurrentlyBeingHeld;
    IModHBoxContainerScript storageCurrentlyBeingTargeted;


    public bool caryingMod;
    int previousZIndex = 0;
    public override void _Ready()
    {

        collider = GetNode<Node2D>("Cage").GetNode<Area2D>("HBox");
        shape = collider.GetNode<CollisionShape2D>("Collider").Shape as RectangleShape2D;

        Input.MouseMode = Input.MouseModeEnum.Hidden;

        
    }
    public override void _PhysicsProcess(double delta)
    {
        GetNode<Node2D>("Cage").Position = GetViewport().GetMousePosition().Snapped(Vector2.One * DisplayServer.WindowGetSize().X / 640);
        edges = GetGlobalCorners(shape, collider.GetNode<CollisionShape2D>("Collider"));
        CheckAndActivateButtonAction();
        foreach (HBoxContainer container in GetTree().GetNodesInGroup("Container"))
        {
            IModHBoxContainerScript storage = container as IModHBoxContainerScript;
            if (storage != null)
            {
                
                foreach (Button button in storage.buttonMovementGuidesDict.Keys.ToList())
                {
                    MouseFollow((float)delta, button,storage);
                }
            }
        }
    }
    public void LoopAndCheckIfButtonShouldBeDoingSomthingIDono()
    {
        foreach (HBoxContainer container in GetTree().GetNodesInGroup("Container"))
        {
            if (container.IsVisibleInTree())
            {
                foreach (Button button in container.GetChildren())
                {
                    Vector2 buttonCenter = button.GetGlobalRect().Position + button.GetGlobalRect().Size / 2f;
                    if (PointInQuad(edges, buttonCenter))
                    {
                        button.GrabFocus();
                        if (odm.parry && (container as ModStorageScript) != null)
                        {
                            (container as ModStorageScript).ThrowModAway(button);
                            return;
                        }
                        if (odm.shoot)
                        {
                            IModHBoxContainerScript storage = container as IModHBoxContainerScript;
                            if (storage != null && storage.buttons[button] != null )
                            {
                                caryingMod = true;
                                ButtonMovementGuid b = storage.buttonMovementGuidesDict[button];
                                b.modThatFollows = storage.buttons[button];
                                b.modThatFollows.GetParent().RemoveChild(b.modThatFollows);
                                b.modThatFollows.Owner = null;
                                 AddChild(b.modThatFollows);
                                b.modThatFollows.Position = button.GetGlobalRect().Position + button.GetGlobalRect().Size / 2f;
                                modOffset = b.modThatFollows.Position - GetNode<Node2D>("Cage").Position;
                                b.modThatFollows.Visible = true;
                                previousZIndex = b.modThatFollows.ZIndex;
                                storage.buttonMovementGuidesDict[button].modThatFollows.ZIndex = 999;
                                b.lerpingToButtonPos = false;

                                buttonBeingHeld = button;
                                storageCurrentlyBeingHeld = storage;

                                b.buttonatBeginingPosition = button;

                                storage.buttonMovementGuidesDict[button] = b;

                                storage.buttons[button] = null;
                                foreach (var state in storage.states)
                                {
                                    button.AddThemeStyleboxOverride(state, storage.textures["Reset"]);
                                }

                            }
                            return;
                        }
                    }
                }
            }
        }
    }
    public void CheckAndActivateButtonAction()
    {
        if (caryingMod) return;
        LoopAndCheckIfButtonShouldBeDoingSomthingIDono();
    }
    public void CheckIfCurrentHeldButtonEndPosIsToBeSet()
    {
        foreach (HBoxContainer container in GetTree().GetNodesInGroup("Container"))
        {
            IModHBoxContainerScript storage = container as IModHBoxContainerScript;
            if (container.IsVisibleInTree())
            {
                foreach (Button button in container.GetChildren())
                {
                    Vector2 buttonCenter = button.GetGlobalRect().Position + button.GetGlobalRect().Size / 2f;
                    if (PointInQuad(edges, buttonCenter))
                    {
                        button.GrabFocus();
                            
                            if (storage != null)
                            {
                                ButtonMovementGuid guid = storageCurrentlyBeingHeld.buttonMovementGuidesDict[buttonBeingHeld];
                                guid.buttonAtEndPos = button;
                                storageCurrentlyBeingHeld.buttonMovementGuidesDict[buttonBeingHeld] = guid;
                                storageCurrentlyBeingTargeted = storage;

                                if (storage.buttons[button] != null && buttonBeingHeld != button )
                                {
                                    storage.buttonMovementGuidesDict[button].buttonatBeginingPosition = button;
                                    storage.buttonMovementGuidesDict[button].buttonAtEndPos = buttonBeingHeld;
                                    storage.buttonMovementGuidesDict[button].modThatFollows = storage.buttons[button];

                                    storage.buttonMovementGuidesDict[button].modThatFollows.Visible = true;
                                    storage.buttonMovementGuidesDict[button].modThatFollows.GetParent().RemoveChild(storage.buttonMovementGuidesDict[button].modThatFollows);
                                    storage.buttonMovementGuidesDict[button].modThatFollows.Owner = null;
                                    AddChild(storage.buttonMovementGuidesDict[button].modThatFollows);
                                    storage.buttonMovementGuidesDict[button].modThatFollows.Position = button.GetGlobalRect().Size / 2f + button.GetGlobalRect().Position;
                                    storage.buttonMovementGuidesDict[button].modThatFollows.ZIndex = 998;

                                foreach (var state in storage.states)
                                    {
                                        button.AddThemeStyleboxOverride(state, storage.textures["Reset"]);
                                    }
                                    storage.buttons[button] = null;
                                }
                            }
                        return;
                    }
                    else if(storage != null)
                    {
                        ButtonMovementGuid guid = storageCurrentlyBeingHeld.buttonMovementGuidesDict[buttonBeingHeld];
                        guid.buttonAtEndPos = null;
                        storageCurrentlyBeingHeld.buttonMovementGuidesDict[buttonBeingHeld] = guid;
                        storageCurrentlyBeingTargeted = null;
                        if (storage.buttonMovementGuidesDict[button].buttonatBeginingPosition != null && buttonBeingHeld != button)
                        {

                            storage.buttonMovementGuidesDict[button].modThatFollows.GetParent().RemoveChild(storage.buttonMovementGuidesDict[button].modThatFollows);
                            storage.buttonMovementGuidesDict[button].modThatFollows.Owner = null;
                            odm.GetParent().AddChild(storage.buttonMovementGuidesDict[button].modThatFollows);
                            storage.buttonMovementGuidesDict[button].modThatFollows.ZIndex = previousZIndex;
                            storage.buttonMovementGuidesDict[button].modThatFollows.Visible = false;

                            storage.buttons[button] = storage.buttonMovementGuidesDict[button].modThatFollows;

                            foreach (var state in storage.states)
                            {
                                button.AddThemeStyleboxOverride(state, storage.textures[storage.buttonMovementGuidesDict[button].modThatFollows.GetType().Name]);
                            }
                            storage.buttonMovementGuidesDict[button].buttonatBeginingPosition = null;
                            storage.buttonMovementGuidesDict[button].buttonAtEndPos = null;
                            storage.buttonMovementGuidesDict[button].modThatFollows = null;
                        }
                    }
                }
            }
        }
    }
    public void MouseFollow(float time, Button button, IModHBoxContainerScript storage )
    {
        ButtonMovementGuid movementList = storage.buttonMovementGuidesDict[button];
        HBoxContainer storageBoxContainerVar = storage as HBoxContainer;
        if (movementList.buttonatBeginingPosition == null || movementList.modThatFollows == null)
        {
                return;
        }
        if (button == buttonBeingHeld)
        {
            if (odm.shoot && !movementList.lerpingToButtonPos)
            {
                CheckIfCurrentHeldButtonEndPosIsToBeSet();
                storage.buttonMovementGuidesDict[button] = movementList;
            }
            if (odm.shoot && !movementList.lerpingToButtonPos && storageBoxContainerVar != null && storageBoxContainerVar.IsVisibleInTree())
            {

                movementList.modThatFollows.Position = GetNode<Node2D>("Cage").Position + modOffset;
                collider.Position = modOffset;
                edges = GetGlobalCorners(shape, collider.GetNode<CollisionShape2D>("Collider"));
                storage.buttonMovementGuidesDict[button] = movementList;
                return;
            }

            else
            {
                movementList.lerpingToButtonPos = true;
                storage.buttonMovementGuidesDict[button] = movementList;
            }
        }
        else if (storageCurrentlyBeingHeld != null && buttonBeingHeld != null && !storageCurrentlyBeingHeld.buttonMovementGuidesDict[buttonBeingHeld].lerpingToButtonPos) return;

        if (movementList.buttonAtEndPos == null)
        {
            movementList.modThatFollows.Position = movementList.modThatFollows.Position.Lerp(movementList.buttonatBeginingPosition.GetGlobalRect().Position + movementList.buttonatBeginingPosition.GetGlobalRect().Size / 2, time * 10);
            if (movementList.modThatFollows.Position.DistanceTo(movementList.buttonatBeginingPosition.GetGlobalRect().Position + movementList.buttonatBeginingPosition.GetGlobalRect().Size / 2) < 1)
            {
                ModEnhancementScriptContainer modEnhancementScriptContainer = storage as ModEnhancementScriptContainer;


                movementList.modThatFollows.GetParent().RemoveChild(movementList.modThatFollows);
                movementList.modThatFollows.Owner = null;
                odm.GetParent().AddChild(movementList.modThatFollows);
                movementList.modThatFollows.ZIndex = previousZIndex;
                movementList.modThatFollows.Visible = false;
                movementList.lerpingToButtonPos = false;
                foreach (var state in storage.states)
                {
                    movementList.buttonatBeginingPosition.AddThemeStyleboxOverride(state, storage.textures[movementList.modThatFollows.GetType().Name]);
                }
               

                storage.buttonMovementGuidesDict[button] = movementList;
                if (buttonBeingHeld == button)
                {
                    if (modEnhancementScriptContainer != null)
                    {
                        modEnhancementScriptContainer.buttonWasChanged = true;
                    }
                    storage.buttons[movementList.buttonatBeginingPosition] = movementList.modThatFollows;
                    
                    storageCurrentlyBeingTargeted = null;
                    storageCurrentlyBeingHeld = null;
                    buttonBeingHeld = null;
                    caryingMod = false;
                }
                movementList.modThatFollows = null;
                movementList.buttonatBeginingPosition = null;
                movementList.buttonAtEndPos = null;
                collider.Position = Vector2.Zero;

            }
        }
        else
        {
                
            movementList.modThatFollows.Position = movementList.modThatFollows.Position.Lerp(movementList.buttonAtEndPos.GetGlobalRect().Position + movementList.buttonAtEndPos.GetGlobalRect().Size / 2, time * 10);
            if (movementList.modThatFollows.Position.DistanceTo(movementList.buttonAtEndPos.GetGlobalRect().Position + movementList.buttonAtEndPos.GetGlobalRect().Size / 2) < 1)
            {

                ModEnhancementScriptContainer modEnhancementScriptContainerA = storageCurrentlyBeingTargeted as ModEnhancementScriptContainer;
                ModEnhancementScriptContainer modEnhancementScriptContainerB = storageCurrentlyBeingHeld as ModEnhancementScriptContainer;
                    
                    
                

                movementList.modThatFollows.GetParent().RemoveChild(movementList.modThatFollows);
                movementList.modThatFollows.Owner = null;
                odm.GetParent().AddChild(movementList.modThatFollows);
                movementList.modThatFollows.ZIndex = previousZIndex;
                movementList.modThatFollows.Visible = false;
                   
                    
                movementList.lerpingToButtonPos = false;
                foreach (var state in storage.states)
                {
                    movementList.buttonAtEndPos.AddThemeStyleboxOverride(state, storage.textures[storage.buttonMovementGuidesDict[button].modThatFollows.GetType().Name]);
                }
  
                storage.buttonMovementGuidesDict[button] = movementList;

                if (button == buttonBeingHeld)
                {
                    if (modEnhancementScriptContainerA != null)
                    {
                        modEnhancementScriptContainerA.buttonWasChanged = true;
                    }
                    else if (modEnhancementScriptContainerB != null)
                    {
                        modEnhancementScriptContainerB.buttonWasChanged = true;
                    }
                    storageCurrentlyBeingTargeted.buttons[movementList.buttonAtEndPos] = movementList.modThatFollows;
                    storageCurrentlyBeingTargeted = null;
                    buttonBeingHeld = null;
                    collider.Position = Vector2.Zero;
                    caryingMod = false;
                }
                else
                {
     
                    storageCurrentlyBeingHeld.buttons[movementList.buttonAtEndPos] = movementList.modThatFollows;
                }
                    

                movementList.modThatFollows = null;
                movementList.buttonAtEndPos = null;
                movementList.buttonatBeginingPosition = null;
                

            }
        }
        
        storage.buttonMovementGuidesDict[button] = movementList;
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
