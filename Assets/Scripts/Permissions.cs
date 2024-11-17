#if UNITY_EDITOR
using UnityEditor.Analytics;
#endif
using UnityEngine;

internal static class Permissions
{
    [RuntimeInitializeOnLoadMethod]
    private static void RequestPermissions()
    {
        
    }
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void SetEngineVariables()
    {
        #if UNITY_EDITOR
        AnalyticsSettings.enabled = false;
        #endif
        
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }
}
