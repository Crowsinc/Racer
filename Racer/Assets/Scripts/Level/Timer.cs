

using System;

public class Timer
{
    public float _totalTime = 0;

    public void Tick(float deltaTime)
    {
        _totalTime += deltaTime;
    }

    public void Reset()
    {
        _totalTime = 0;
    }

    public float GetTime()
    {
        return (float)((int)(_totalTime * 100) / 100.0);
    }

    public static string TimeToString(float time)
    {
        return String.Format("{0}:{1}", (int)(time / 60), (time%60).ToString("00.00"));
        // return (Mathf.Round(time * 100) / 100.0).ToString("#.00");
    }
}
