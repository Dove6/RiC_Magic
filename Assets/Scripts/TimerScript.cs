using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerData
{
    public bool Passed;
    public bool InSeconds;
    public int FixedUpdates;
    public int RemainingFixedUpdates;
    public float Seconds;
    public float RemainingSeconds;

    public TimerData(int FixedUpdatesToPass)
    {
        Passed = false;
        InSeconds = false;
        FixedUpdates = FixedUpdatesToPass;
        RemainingFixedUpdates = FixedUpdatesToPass;
        Seconds = 0f;
        RemainingSeconds = 0f;
    }

    public TimerData(float SecondsToPass)
    {
        Passed = false;
        InSeconds = true;
        FixedUpdates = 0;
        RemainingFixedUpdates = 0;
        Seconds = SecondsToPass;
        RemainingSeconds = SecondsToPass;
    }
}

public class TimerScript : MonoBehaviour
{
    private static Dictionary<int, TimerData> Timers;

    public static int MakeTimer(int FixedUpdatesToPass)
    {
        int i;
        for (i = Random.Range(0, int.MaxValue); Timers.ContainsKey(i); i++);
        Timers.Add(i, new TimerData(FixedUpdatesToPass));
        return i;
    }

    public static int MakeTimer(float SecondsToPass)
    {
        int i;
        for (i = Random.Range(0, int.MaxValue); Timers.ContainsKey(i); i++);
        Timers.Add(i, new TimerData(SecondsToPass));
        return i;
    }

    public static float GetRemainingSeconds(int Identifier)
    {
        if (Timers.ContainsKey(Identifier)) {
            if (Timers[Identifier].InSeconds) {
                return Timers[Identifier].RemainingSeconds;
            } else {
                return Timers[Identifier].RemainingFixedUpdates * Time.fixedDeltaTime;
            }
        } else {
            return float.NaN;
        }
    }

    public static int GetRemainingFixedUpdates(int Identifier)
    {
        if (Timers.ContainsKey(Identifier)) {
            if (Timers[Identifier].InSeconds) {
                return (int)(Timers[Identifier].RemainingSeconds / Time.fixedDeltaTime);
            } else {
                return Timers[Identifier].RemainingFixedUpdates;
            }
        } else {
            return -1;
        }
    }
    public static bool HasPassed(int Identifier)
    {
        if (Timers.ContainsKey(Identifier)) {
            return Timers[Identifier].Passed;
        } else {
            return true;
        }
    }

    public static void Remove(int Identifier)
    {
        if (Timers.ContainsKey(Identifier)) {
            Timers.Remove(Identifier);
        }
    }
    
    void Awake()
    {
        Timers = new Dictionary<int, TimerData>();
    }
    
    void FixedUpdate()
    {
        foreach (KeyValuePair<int, TimerData> Timer in Timers)
        {
            if (!Timer.Value.Passed) {
                if (Timer.Value.InSeconds) {
                    Timer.Value.RemainingSeconds -= Time.fixedDeltaTime;
                    if (Timer.Value.RemainingSeconds <= 0) {
                        Timer.Value.Passed = true;
                    }
                } else {
                    Timer.Value.RemainingFixedUpdates--;
                    if (Timer.Value.RemainingFixedUpdates <= 0) {
                        Timer.Value.Passed = true;
                    }
                }
            }
        }
    }
}
