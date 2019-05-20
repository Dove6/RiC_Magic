using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//inclusive range [CounterMin; CounterMax]
public class CounterScript
{
    private int Counter,
                CounterMin,
                CounterMax,
                Increment;
    private bool Looped;

    public CounterScript(int MinimalValue, int MaximalValue, int IncrementialValue, int InitialValue)
    {
        Counter = InitialValue;
        CounterMin = MinimalValue;
        CounterMax = MaximalValue;
        Increment = IncrementialValue;
    }

    public CounterScript(int InitialValue, int LiminalValue, int IncrementialValue)
    {
        Counter = InitialValue;
        if (Increment >= 0) {
            CounterMin = InitialValue;
            CounterMax = LiminalValue;
        } else {
            CounterMin = LiminalValue;
            CounterMax = InitialValue;
        }
        Increment = IncrementialValue;
    }

    public int Get()
    {
        int Temporary = Counter;
        Counter += Increment;
        if (Increment >= 0) {
            if (Counter > CounterMax) {
                Counter = CounterMin;
                Looped = true;
            }
        } else {
            if (Counter < CounterMin) {
                Counter = CounterMax;
                Looped = true;
            }
        }
        return Temporary;
    }

    public bool HasLooped()
    {
        return Looped;
    }
}
