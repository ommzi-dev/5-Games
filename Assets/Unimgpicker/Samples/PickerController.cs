using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;
using UnityEngine.Networking;

namespace Kakera
{
    public class PickerController : MonoBehaviour
    {
        [SerializeField]
        private Unimgpicker imagePicker;

        [SerializeField]
        private RawImage image;

        void Awake()
        {
            Debug.Log("Data Path: "+ Application.persistentDataPath);
            imagePicker.Completed += (string path) =>
            {
                StartCoroutine(LoadImage(path, image));
            };
        }

        public void OnPressShowPicker()
        {
            imagePicker.Show("Select Image", "unimgpicker", 1024);
        }

        private IEnumerator LoadImage(string path, RawImage output)
        {
            var url = "file://" + path;
            var www = new WWW(url);
            yield return www;

            var texture = www.texture;
            if (texture == null)
            {
                Debug.LogError("Failed to load texture url:" + url);
            }
            output.texture = texture;
            UserDetailsManager.userImageTexture = texture;

            byte[] bytes = texture.EncodeToPNG();
            string base64 = System.Convert.ToBase64String(bytes);
            StartCoroutine(UpdateImageAtBackend(base64));
            string activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (activeScene == "LoginSplash")
                UIManager.Instance.UpdateUserPic();
            else if (activeScene == "WhotPlay")
                WhotUiManager.instance.UpdateUserImage();
            //File.WriteAllBytes(Application.dataPath + "/Resources/UserImage.png", bytes);
        }

        public IEnumerator UpdateImageAtBackend(string avatar)
        {
            Debug.Log("Img String: "+ avatar);
            WWWForm form = new WWWForm();
            form.AddField("user_dp", avatar);
            UnityWebRequest www = UnityWebRequest.Post(UserDetailsManager.serverUrl + "updateuser", form);
            www.SetRequestHeader("Accept", "application/json");
            www.SetRequestHeader("Authorization", "Bearer " + UserDetailsManager.accessToken);
            www.timeout = 120;
            
            yield return www.SendWebRequest();
            if (www.error != null || www.isNetworkError)
            {
                Debug.Log("Update Image Error: " + www.error);
            }
            else
            {
                Debug.Log("Update Image Response: " + www.downloadHandler.text);
            }
        }
    }
}