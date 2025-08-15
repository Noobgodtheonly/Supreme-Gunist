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
    private DistributionSystem odm;
    public Node2D gameWorld;

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

        gameWorld = GetTree().CurrentScene.GetNode<SubViewport>("SubViewport").GetNode<Node2D>("Node2D");
        odm = (DistributionSystem)gameWorld.GetNode<CharacterBody2D>("Player");

       

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

        buttons[button].GetParent().RemoveChild(buttons[button]);
        buttons[button].Owner = null;
        gameWorld.AddChild(buttons[button]);

        buttonMovementGuidesDict[button].Reset();


        DistributionSystem odm = ((DistributionSystem)gameWorld.GetNode<CharacterBody2D>("Player"));

        buttons[button].GlobalPosition = odm.GlobalPosition + modSpawnPositionOffset;

        buttons[button].Scale = Vector2.One;



        ModVelocity velocity = buttons[button].GetNode<ModVelocity>("ModVelocity");

        velocity.SetPhysicsProcess(true);

        foreach (var state in states)
        {
            button.AddThemeStyleboxOverride(state, textures["Reset"]);
        }

        if (odm.facingRight) velocity.velocity.X = -80;
        else
            velocity.velocity.X = 80;

        velocity.GetNode<CollisionShape2D>("Collider").Disabled = false;
        buttons[button] = null;
    }
    public void AddToStorage(Node2D gunMod)
    {
        int currentButtonIndex = 0;
        foreach(var button in buttons.Keys)
        {
            if (buttons[button] == null)
            {
               

                if (!odm.getMovementInputeStruct.cannotIntaractForNow.ActionDone()) return;
                buttons[button] = gunMod as GunMod;

                ModVelocity modVelocity = buttons[button].GetNode<ModVelocity>("ModVelocity");

                foreach (var state in states)
                {
                    button.AddThemeStyleboxOverride(state, textures[gunMod.GetType().Name]);
                }

                modVelocity.SetPhysicsProcess(false);
                modVelocity.GetNode<CollisionShape2D>("Collider").Disabled = true;

                odm.getMovementInputeStruct.cannotIntaractForNow.Start();
                return;
            }
            else if(currentButtonIndex == buttons.Keys.Count - 1)
            {
                

                if (!odm.getMovementInputeStruct.cannotIntaractForNow.ActionDone()) return;

                ThrowModAway(button);

                buttons[button] = gunMod as GunMod;


                ModVelocity modVelocity = buttons[button].GetNode<ModVelocity>("ModVelocity");

                foreach (var state in states)
                {
                    button.AddThemeStyleboxOverride(state, textures[gunMod.GetType().Name]);
                }

                modVelocity.SetPhysicsProcess(false);
                modVelocity.GetNode<CollisionShape2D>("Collider").Disabled = true;

                odm.getMovementInputeStruct.cannotIntaractForNow.Start();

                return;
            }
            currentButtonIndex++;
        }
    }
}
