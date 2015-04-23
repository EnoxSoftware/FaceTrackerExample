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
		
		void OnGUI ()
		{
			float screenScale = Screen.height / 240.0f;
			Matrix4x4 scaledMatrix = Matrix4x4.Scale (new Vector3 (screenScale, screenScale, screenScale));
			GUI.matrix = scaledMatrix;
			
			
			GUILayout.BeginVertical ();
			
			if (GUILayout.Button ("Show License")) {
				Application.LoadLevel ("ShowLicense");
			}
			
			if (GUILayout.Button ("Texture2DFaceTrackerSample")) {
				Application.LoadLevel ("Texture2DFaceTrackerSample");
			}
			
			if (GUILayout.Button ("WebCamTextureFaceTrackerSample")) {
				Application.LoadLevel ("WebCamTextureFaceTrackerSample");
			}

			if (GUILayout.Button ("FaceTrackerARSample")) {
				Application.LoadLevel ("FaceTrackerARSample");
			}
			
			
			GUILayout.EndVertical ();
		}
	}
}