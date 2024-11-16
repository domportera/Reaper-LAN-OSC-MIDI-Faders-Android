using UnityEngine;

internal static class Permissions
{
    [RuntimeInitializeOnLoadMethod]
    private static void RequestPermissions()
    {
        
    }
    
    [RuntimeInitializeOnLoadMethod]
    private static void SetScreenTimeout()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }
}
