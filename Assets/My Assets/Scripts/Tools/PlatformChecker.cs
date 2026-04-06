using UnityEngine;

public class PlatformChecker : MonoBehaviour
{

    public enum Platform
    {
        Extra, Editor, Standalone, Mobile
    }

    public static PlatformChecker instance;
    public Platform platform;

    void Awake()
    {
        instance = this;

        // Check if the application is running in the Unity Editor
        if (Application.isEditor)
        {
            platform = Platform.Editor;
        }
        // Check if the application is running on a standalone platform
        else if (Application.platform == RuntimePlatform.WindowsPlayer ||
                 Application.platform == RuntimePlatform.OSXPlayer ||
                 Application.platform == RuntimePlatform.LinuxPlayer)
        {
            platform = Platform.Standalone;
        }
        // Check if the application is running on a mobile platform
        else if (Application.platform == RuntimePlatform.IPhonePlayer ||
                 Application.platform == RuntimePlatform.Android)
        {
            platform = Platform.Mobile;
        }
        else
        {
            platform = Platform.Extra;
        }
    }
}