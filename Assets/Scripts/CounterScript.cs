using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//inclusive range [CounterMin; CounterMax]
public class CounterScript
{
    private int counter,
                counterMin,
                counterMax,
                increment,
                startingValue;
    private bool looped = false;

    public CounterScript(int minimalValue, int maximalValue, int incrementialValue, int initialValue)
    {
        counter = initialValue;
        counterMin = minimalValue;
        counterMax = maximalValue;
        increment = incrementialValue;
        startingValue = initialValue;
    }

    public CounterScript(int initialValue, int liminalValue, int incrementialValue)
    {
        counter = initialValue;
        increment = incrementialValue;
        if (increment >= 0) {
            counterMin = initialValue;
            counterMax = liminalValue;
        } else {
            counterMin = liminalValue;
            counterMax = initialValue;
        }
        startingValue = initialValue;
    }

    public CounterScript(int initialValue, int incrementialValue)
    {
        counter = initialValue;
        increment = incrementialValue;
        if (increment >= 0) {
            counterMin = int.MinValue;
            counterMax = int.MaxValue;
        } else {
            counterMin = int.MaxValue;
            counterMax = int.MinValue;
        }
        startingValue = initialValue;
    }

    public int Get()
    {
        int temporary = counter;
        counter += increment;
        if (increment >= 0) {
            if (counter > counterMax) {
                counter = counterMin;
                looped = true;
            }
        } else {
            if (counter < counterMin) {
                counter = counterMax;
                looped = true;
            }
        }
        return temporary;
    }

    public void Reset()
    {
        counter = startingValue;
        looped = false;
    }

    public bool HasLooped()
    {
        return looped;
    }
}
