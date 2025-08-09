using Godot;

[GlobalClass]
public partial class SpringCamera2D : Camera2D
{
    [Export] public CharacterBody2D TargetPath;
    [Export] public float Frequency = 4f;
    public float shakeStrength = 0f;
    private float shakeDecay = 8;
    [Export] public float LookAheadDistance = 24f;
    [Export] public bool PixelSnap = true;
    public bool disableLookAhead = false;

    private DistributionSystem _player;
    private Node2D _target;
    private Vector2 _velocity = Vector2.Zero;
    private Vector2 shakeOffset = Vector2.Zero;

    public override void _Ready()
    {
        _target = TargetPath;
        _player = TargetPath as DistributionSystem;
        MakeCurrent();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_target == null || _player == null) return;

        float dt = (float)delta;

        Vector2 dir = _player.GetGunDir();      // raw cursor‑to‑player vector
        float len = dir.Length();

        if (len < 1f) len = 2f;                 // ❶ keep it safely > 0 (no spin)

        // unit direction                        ❷ manual normalize is cheaper
        Vector2 norm = dir / len;

        // final look‑ahead with clamped scale   ❸ same formula as before
        float scale = Mathf.Clamp(len / 16f, 0, 1);
        Vector2 lookAhead = norm * LookAheadDistance * scale;
        if (disableLookAhead)
            lookAhead = Vector2.Zero;

        // Spring constants
        float omega = 2f * Mathf.Pi * Frequency;
        float x = omega * dt;
        float exp = 1f / (1f + x + 0.48f * x * x + 0.235f * x * x * x);

        // Displacement from camera to target (NO offset included here!)
        Vector2 displacement = GlobalPosition - _target.GlobalPosition;

        // Acceleration & velocity update
        Vector2 accel = (_velocity + displacement * omega) * dt;
        _velocity = (_velocity - accel * omega) * exp;

        // Final target position is target + lookAhead + spring offset
        Vector2 final = _target.GlobalPosition + lookAhead + (displacement + accel) * exp;

        if (shakeStrength > 0)
        {
            shakeOffset.X = (float)(GD.Randf() * 2 - 1) * shakeStrength;
            shakeOffset.Y = (float)(GD.Randf() * 2 - 1) * shakeStrength;
            shakeStrength = Mathf.MoveToward(shakeStrength, 0f, shakeDecay * (float)delta);
        }
        else
            shakeOffset = Vector2.Zero;


        GlobalPosition = PixelSnap ? (final + shakeOffset).Round() : (final + shakeOffset);
    }
    public void AddShake(float strength)
    {
        shakeStrength =Mathf.Min(strength,4);
    }
}
