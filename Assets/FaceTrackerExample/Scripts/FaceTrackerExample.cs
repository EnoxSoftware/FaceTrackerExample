using UnityEngine;
using System.Collections;

#if UNITY_5_3 || UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif

namespace FaceTrackerExample
{
    /// <summary>
    /// Face tracker example.
    /// </summary>
    public class FaceTrackerExample : MonoBehaviour
    {
        
        // Use this for initialization
        void Start()
        {
            
        }
        
        // Update is called once per frame
        void Update()
        {
            
        }

        public void OnShowLicenseButton()
        {
            #if UNITY_5_3 || UNITY_5_3_OR_NEWER
            SceneManager.LoadScene ("ShowLicense");
            #else
            Application.LoadLevel("ShowLicense");
#endif
        }
        
        public void OnTexture2DFaceTrackerExample()
        {
            #if UNITY_5_3 || UNITY_5_3_OR_NEWER
            SceneManager.LoadScene ("Texture2DFaceTrackerExample");
            #else
            Application.LoadLevel("Texture2DFaceTrackerExample");
            #endif
        }
        
        public void OnWebCamTextureFaceTrackerExample()
        {
            #if UNITY_5_3 || UNITY_5_3_OR_NEWER
            SceneManager.LoadScene ("WebCamTextureFaceTrackerExample");
            #else
            Application.LoadLevel("WebCamTextureFaceTrackerExample");
            #endif
        }
        
        public void OnFaceTrackerARExample()
        {
            #if UNITY_5_3 || UNITY_5_3_OR_NEWER
            SceneManager.LoadScene ("FaceTrackerARExample");
            #else
            Application.LoadLevel("FaceTrackerARExample");
            #endif
        }
    }
}