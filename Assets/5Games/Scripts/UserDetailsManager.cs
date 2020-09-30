using UnityEngine;
using Photon.Chat;
using UnityEngine.UI;

public class UserDetailsManager: MonoBehaviour
{
    
    [Header("Player Details")]
    public static string accessToken;
    public static string userId;
    public static string userName;
    public static string userEmail;
    public static string userPhone;
    public static string userCountryCode;
    public static string userImageString;
    public static int userCoins;
    public static int userCoinsWon;
    public static float betServerPercent = 0.10f;

    public static bool isAdminPlayer = true;
    public static bool loggedIn; 
    public static string PhotonAppID = "212c7ec7-8189-4440-a021-3c71773c2a63";  // used previously "9d9f1e8b-64f2-4830-bcc1-78612a41c508";
    public static string PhotonChatID = "b3d9b0ac-6a85-41c9-9e24-e42247d47cba"; // used previously "df7319ec-b8e8-4281-a35f-32cefe982c08";
    public static string serverUrl = "http://18.191.157.16:4000/apis/";
    public static Texture userImageTexture;
    public static ChatClient chatClient;
    public static bool inGame;
    public static string androidUnique()
    {
        AndroidJavaClass androidUnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject unityPlayerActivity = androidUnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject unityPlayerResolver = unityPlayerActivity.Call<AndroidJavaObject>("getContentResolver");
        AndroidJavaClass androidSettingsSecure = new AndroidJavaClass("android.provider.Settings$Secure");
        return androidSettingsSecure.CallStatic<string>("getString", unityPlayerResolver, "android_id");
    }
}
