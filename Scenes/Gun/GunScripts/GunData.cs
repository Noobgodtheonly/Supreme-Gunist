using Godot;
using System;
[GlobalClass]
public partial class GunData : Resource
{
    public bool pierce;
    public float scale = 0.3f;
    public bool explode;
    public float speed = 4f;
    public float damage = 7;
    public float bulletTime = 0.9f;
    public float gunCooldown = 0.4f;
    public int bulletCount = 1;
    public bool applyGravity;
    
    public void Reset()
    {
        pierce = false;
        scale = 0.3f;
        explode = false;
        speed = 4f;
        damage = 7;
        bulletTime = 0.9f;
        gunCooldown = 0.5f;
        bulletCount = 1;
        applyGravity = false;
    }

}
