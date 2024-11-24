using UnityEngine;
using System.Reflection;
using System;

public class UniClipboard
{
    private static IBoard _board;

    private static IBoard board{
        get{
            if (_board == null) {
                #if UNITY_ANDROID && !UNITY_EDITOR
                _board = new AndroidBoard();
                #elif UNITY_IOS && !UNITY_TVOS && !UNITY_EDITOR
                _board = new IOSBoard ();
                #else
                _board = new StandardBoard(); 
                #endif
            }
            return _board;
        }
    }

    public static void SetText(string str){
        board.SetText (str);
    }

    public static string GetText(){
        return board.GetText ();
    }
}

internal interface IBoard{
    void SetText(string str);
    string GetText();
}

internal class StandardBoard : IBoard {
    private static PropertyInfo m_systemCopyBufferProperty;
    private static PropertyInfo GetSystemCopyBufferProperty() {
        if (m_systemCopyBufferProperty == null) {
            var T = typeof(GUIUtility);
            m_systemCopyBufferProperty = T.GetProperty("systemCopyBuffer", BindingFlags.Static | BindingFlags.Public);
            if (m_systemCopyBufferProperty == null)
            {
                m_systemCopyBufferProperty =
                    T.GetProperty("systemCopyBuffer", BindingFlags.Static | BindingFlags.NonPublic);
            }

            if (m_systemCopyBufferProperty == null)
            {
                throw new Exception(
                    "Can't access internal member 'GUIUtility.systemCopyBuffer' it may have been removed / renamed");
            }
        }
        return m_systemCopyBufferProperty;
    }
    public void SetText(string str) {
        var P = GetSystemCopyBufferProperty();
        P.SetValue(null, str, null);
    }
    public string GetText(){
        var P = GetSystemCopyBufferProperty();
        return (string)P.GetValue(null, null);
    }
}

#if UNITY_IOS && !UNITY_TVOS
class IOSBoard : IBoard {
    [DllImport("__Internal")]
    static extern void SetText_ (string str);
    [DllImport("__Internal")]
    static extern string GetText_();

    public void SetText(string str){
        if (Application.platform != RuntimePlatform.OSXEditor) {
            SetText_ (str);
        }
    }

    public string GetText(){
        return GetText_();
    }
}
#endif

#if UNITY_ANDROID
class AndroidBoard : IBoard
{
    public void SetText(string str)
    {
        GetClipboardManager().Call("setText", str);
    }

    public string GetText()
    {
        return GetClipboardManager().Call<string>("getText");
    }

    AndroidJavaObject GetClipboardManager()
    {
        var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        var staticContext = new AndroidJavaClass("android.content.Context");
        var service = staticContext.GetStatic<AndroidJavaObject>("CLIPBOARD_SERVICE");
        return activity.Call<AndroidJavaObject>("getSystemService", service);
    }
}
#endif
