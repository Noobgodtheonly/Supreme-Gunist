using Godot;
using System;
using System.Collections.Generic;

public  class StateTransition<T> where T : Enum
{
    public delegate bool Condition();
    private Dictionary<T, List<Condition>> getStateTransition = new Dictionary<T, List<Condition>>();
    private List<T> statesToBeAddedBack = new List<T>();
    private List<List<Condition>> conditionListsToBeAddedBack = new List<List<Condition>>();

    public void Begin()
    {
        foreach (T state in Enum.GetValues(typeof(T)))
        {
            getStateTransition.Add(state, new List<Condition>());
        }
    }
    public bool CouldTransitionTo(T state)
    {
        if (getStateTransition[state].Count <= 0) return true;
        foreach (var condition in getStateTransition[state]) {
            if (!condition()) return false;
        }
        return true; 
    }
    public void AddConditionTo(T state, Condition condition) {
        getStateTransition[state].Add(condition);
    }
    public void AddConditionToAllExept(Condition condition, List<T> states) {
        if(states != null)
        {
            foreach(var state in states)
            {
                statesToBeAddedBack.Add(state);
                conditionListsToBeAddedBack.Add(getStateTransition[state]);
                getStateTransition.Remove(state);
            }
        }
        if (getStateTransition.Keys.Count >= 0)
        {
            foreach (var state in getStateTransition.Keys)
            {
                getStateTransition[state].Add(condition);
            }
        }
        for(int i = 0; i < statesToBeAddedBack.Count; i++)
        {
            getStateTransition.Add(statesToBeAddedBack[i], conditionListsToBeAddedBack[i]);
        }
        statesToBeAddedBack.Clear();
        conditionListsToBeAddedBack.Clear();
    }
}
