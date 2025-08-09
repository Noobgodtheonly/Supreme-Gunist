using Godot;
using System.Collections.Generic;
using System.Collections;
using System;
[GlobalClass]
public partial class ModEnhancementScriptContainer : HBoxContainer,IModHBoxContainerScript
{
    public Dictionary<Button, GunMod> buttons { get; set; } = new();
    public Dictionary<string, StyleBoxTexture> textures { get; set; } = new();
    public Dictionary<Button, ButtonMovementGuid> buttonMovementGuidesDict { get; set; } = new();
    public bool buttonWasChanged;
    public string[] states { get; set; } = {
    "normal", "hover", "pressed", "disabled", "focus",
    "hover_pressed", "hover_focus", "pressed_focus", "disabled_focus"
    };

    public override void _Ready()
    {
        foreach (var button in GetChildren())
        {
            var b = button as Button;
            if (b != null)
            {
                buttons.Add(b, null);
                buttonMovementGuidesDict.Add(b, new ButtonMovementGuid());
            }
        }

        GetVisualsInFolder("res://Scenes//Gun//GunModScenes/");
        textures.Add("Reset", GetNode<Button>("Button").GetThemeStylebox("normal") as StyleBoxTexture);

        AddToGroup("Container");
    }
    public override void _PhysicsProcess(double delta)
    {
        if (buttonWasChanged)
            ((ModEnhancementScript)GetParent()).buttonWasChanged = true;
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
}
