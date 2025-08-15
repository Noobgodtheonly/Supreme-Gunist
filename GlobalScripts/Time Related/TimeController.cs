using Godot;
using System;

public  class TimeController
{
    private bool actionDone;
    private bool cooldownDone;
    private bool cooldownRunning;
    private bool run;
    public float currTime;
    public float actionDuration;
    public float cooldownDuration;

    public TimeController(float actionDuration, float cooldownDuration)
    {
        actionDone = true;
        cooldownDone = true;
        cooldownRunning = false;
        run = false;
        currTime = 0;
        this.actionDuration = Mathf.Abs(actionDuration);
        this.cooldownDuration = Mathf.Abs(cooldownDuration);

        GlobalTimeManager.AddTimer(this);
    }
    public void Start()
    {
        actionDone = false;
        cooldownDone = false;
        run = true;
    }
    public void Reset()
    {
        actionDone = true;
        cooldownDone = true;
        run = false;
        cooldownRunning = false;
        currTime = 0;
    }
    public void Update(float time) {
        if (!run) return;
        currTime += time;
        if(currTime >= actionDuration && !cooldownRunning)
        {
            actionDone = true;
            cooldownDone = false;
            run = true;
            cooldownRunning = true;
            currTime = (currTime - actionDuration > 0)? currTime - actionDuration: 0;
        }
        if(cooldownRunning && currTime >= cooldownDuration)
        {
            Reset();
        }
    }
    public bool ActionDone() { return actionDone; }
    public bool CooldownDone() { return cooldownDone; }
    public void ChangeActionDuration(float newTime)
    {
        actionDuration = Mathf.Abs(newTime);
    }
    public void ChangeCooldownDuration(float newTime)
    {
        cooldownDuration = Mathf.Abs(newTime);
    }
}
