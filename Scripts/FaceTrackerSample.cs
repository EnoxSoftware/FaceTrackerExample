using UnityEngine;
using System.Collections;

#if UNITY_5_3
using UnityEngine.SceneManagement;
#endif

namespace FaceTrackerSample
{
		/// <summary>
		/// Face tracker sample.
		/// </summary>
		public class FaceTrackerSample : MonoBehaviour
		{
		
				// Use this for initialization
				void Start ()
				{
			
				}
		
				// Update is called once per frame
				void Update ()
				{
			
				}

				public void OnShowLicenseButton ()
				{
						#if UNITY_5_3
			SceneManager.LoadScene ("ShowLicense");
						#else
						Application.LoadLevel ("ShowLicense");
#endif
				}
		
				public void OnTexture2DFaceTrackerSample ()
				{
						#if UNITY_5_3
			SceneManager.LoadScene ("Texture2DFaceTrackerSample");
						#else
						Application.LoadLevel ("Texture2DFaceTrackerSample");
						#endif
				}
		
				public void OnWebCamTextureFaceTrackerSample ()
				{
						#if UNITY_5_3
			SceneManager.LoadScene ("WebCamTextureFaceTrackerSample");
						#else
						Application.LoadLevel ("WebCamTextureFaceTrackerSample");
						#endif
				}
		
				public void OnFaceTrackerARSample ()
				{
						#if UNITY_5_3
			SceneManager.LoadScene ("FaceTrackerARSample");
						#else
						Application.LoadLevel ("FaceTrackerARSample");
						#endif
				}
		}
}