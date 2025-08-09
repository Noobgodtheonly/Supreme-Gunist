using Godot;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Runtime.Intrinsics.Arm;
[GlobalClass]

public partial class ModStorageScript : HBoxContainer, IModHBoxContainerScript
{
    public Dictionary<Button,GunMod>  buttons { get; set; } = new();
    public Dictionary<string, StyleBoxTexture> textures { get; set; } = new();
    public Dictionary<Button,ButtonMovementGuid> buttonMovementGuidesDict { get; set; } = new();

    public Vector2 modSpawnPositionOffset = Vector2.Zero;
    public string[] states { get; set; } = {
    "normal", "hover", "pressed", "disabled", "focus",
    "hover_pressed", "hover_focus", "pressed_focus", "disabled_focus"
    };
    public int buttonCount = 0;
    public int targetButton = 0;
    

    public override void _Ready()
    {
        modSpawnPositionOffset.Y -= 4.5f;
        foreach(var button in GetChildren())
        {
            var b = button as Button;
            if(b != null)
            {
                buttons.Add(b, null);
                buttonMovementGuidesDict.Add(b,new ButtonMovementGuid());
                buttonCount++;
            }
        }

        GetVisualsInFolder("res://Scenes//Gun//GunModScenes/");
        textures.Add("Reset",  GetNode<Button>("Button").GetThemeStylebox("normal") as StyleBoxTexture);
        
        AddToGroup("Container");
    }
    public void GetVisualsInFolder(string path)
    {
        DirAccess dir = DirAccess.Open(path);
        if (dir == null)
        {
            GD.PrintErr("Could not open directory: ", path);
            return;
        }

        dir.ListDirBegin();
        string fileName = dir.GetNext();

        while (fileName != "")
        {
            if (!dir.CurrentIsDir() && fileName.EndsWith(".tscn"))
            {
                string fullPath = path + "/" + fileName;
                PackedScene scene = ResourceLoader.Load<PackedScene>(fullPath);

                if (scene != null)
                {
                    GunMod mod = scene.Instantiate<GunMod>();
                    if (mod != null)
                    {
                        // Set up reference for interaction
                        

                        // Get the texture and region from the mod's sprite
                        Sprite2D sprite = mod.GetNode<Sprite2D>("Sprite2D");
                        Texture2D baseTexture = sprite.Texture;
                        Rect2 region = sprite.RegionRect;

                        // Convert region to integer Rect2I
                        Rect2I regionInt = new Rect2I(
                            (int)region.Position.X,
                            (int)region.Position.Y,
                            (int)region.Size.X,
                            (int)region.Size.Y
                        );

                        // Extract and crop the texture
                        Image fullImage = baseTexture.GetImage();
                        Image croppedImage = fullImage.GetRegion(regionInt);
                        ImageTexture croppedTexture = ImageTexture.CreateFromImage(croppedImage);

                        // Store the cropped texture in a style box
                        StyleBoxTexture style = new StyleBoxTexture();
                        style.Texture = croppedTexture;

                        // Store in dictionary using subclass name
                        string modType = mod.GetType().Name;
                        textures[modType] = style;
                    }
                }
            }

            fileName = dir.GetNext();
        }

        dir.ListDirEnd();
    }


    public void ThrowModAway(Button button)
    {
        if (buttons[button] == null) return;
        (buttons[button]).Visible = true;
        (buttons[button]).GetNode<Intaractable>("Intaractable").SetPhysicsProcess(true);

        DistributionSystem odm = ((DistributionSystem)GetTree().CurrentScene.FindChild("Player"));

        buttons[button].GlobalPosition = odm.GlobalPosition + modSpawnPositionOffset;

        ModVelocity velocity = (buttons[button]).GetNode<ModVelocity>("ModVelocity");

        foreach (var state in states)
        {
            button.AddThemeStyleboxOverride(state, textures["Reset"]);
        }

        if (odm.facingRight) velocity.velocity.X = -8;
        else
            velocity.velocity.X = 8;
        buttons[button] = null;
    }
    public void AddToStorage(Node2D gunMod)
    {
        int currentButtonIndex = 0;
        foreach(var button in buttons.Keys)
        {

            if ((buttons[button] != null || buttonMovementGuidesDict[button].buttonatBeginingPosition == button || buttonMovementGuidesDict[button].buttonAtEndPos == button) && currentButtonIndex == buttons.Keys.Count - 1)
            {
                if (!((DistributionSystem)GetTree().CurrentScene.GetNode<CharacterBody2D>("Player")).getMovementInputeStruct.cannotIntaractForNow.ActionDone()) return;
                if(buttonMovementGuidesDict[button].buttonatBeginingPosition == button)
                {
                    buttonMovementGuidesDict[button].modThatFollows.GetParent().RemoveChild(buttonMovementGuidesDict[button].modThatFollows);
                    buttonMovementGuidesDict[button].modThatFollows.Owner = null;
                    ((DistributionSystem)GetTree().CurrentScene.GetNode<CharacterBody2D>("Player")).GetParent().AddChild(buttonMovementGuidesDict[button].modThatFollows);
                    buttons[button] = buttonMovementGuidesDict[button].modThatFollows;
                    buttonMovementGuidesDict[button].modThatFollows = null;
                    buttonMovementGuidesDict[button].buttonatBeginingPosition = null;
                }
                if(button == buttonMovementGuidesDict[button].buttonAtEndPos)
                {
                    buttonMovementGuidesDict[button].modThatFollows.GetParent().RemoveChild(buttonMovementGuidesDict[button].modThatFollows);
                    buttonMovementGuidesDict[button].modThatFollows.Owner = null;
                    ((DistributionSystem)GetTree().CurrentScene.GetNode<CharacterBody2D>("Player")).GetParent().AddChild(buttonMovementGuidesDict[button].modThatFollows);
                    buttons[button] = buttonMovementGuidesDict[button].modThatFollows;
                    buttonMovementGuidesDict[button].modThatFollows = null;
                    buttonMovementGuidesDict[button].buttonAtEndPos = null;
                }
                    
                
                ThrowModAway(button);

                ((DistributionSystem)GetTree().CurrentScene.GetNode<CharacterBody2D>("Player")).getMovementInputeStruct.cannotIntaractForNow.Start();

                foreach (var state in states)
                {
                    button.AddThemeStyleboxOverride(state, textures[gunMod.GetType().Name]);
                }


                buttons[button] = gunMod as GunMod;
                return;
            }
            if (buttons[button] == null && button != buttonMovementGuidesDict[button].buttonatBeginingPosition && button != buttonMovementGuidesDict[button].buttonAtEndPos)
            {
                Sprite2D modSprite = gunMod.GetNode<Sprite2D>("Sprite2D");

                foreach(var state in states)
                {
                    button.AddThemeStyleboxOverride(state, textures[gunMod.GetType().Name]);
                }


                buttons[button] = gunMod as GunMod;
                return;
            }
            currentButtonIndex++;
        }
    }
}
