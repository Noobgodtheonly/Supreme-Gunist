using Godot;
using System;

public static class GlobalTimeManager
{
    public delegate void timeControllerDelegate(float time);
    public static event timeControllerDelegate nowUpdateTime;

    public static void Update(float time)
    {
        nowUpdateTime?.Invoke(time);
    }
     public static void AddTimer(TimeController timer)
     {
        nowUpdateTime += timer.Update;
     }
}
