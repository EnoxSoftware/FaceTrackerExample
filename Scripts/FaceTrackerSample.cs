using UnityEngine;
using System.Collections;

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
			Application.LoadLevel ("ShowLicense");
		}
		
		public void OnTexture2DFaceTrackerSample ()
		{
			Application.LoadLevel ("Texture2DFaceTrackerSample");
		}
		
		public void OnWebCamTextureFaceTrackerSample ()
		{
			Application.LoadLevel ("WebCamTextureFaceTrackerSample");
		}
		
		public void OnFaceTrackerARSample ()
		{
			Application.LoadLevel ("FaceTrackerARSample");
		}
	}
}