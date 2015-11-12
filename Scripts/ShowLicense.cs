using UnityEngine;
using System.Collections;

namespace FaceTrackerSample
{

	/// <summary>
	/// Show license.
	/// </summary>
	public class ShowLicense : MonoBehaviour
	{
		
		// Use this for initialization
		void Start ()
		{
			
		}
		
		// Update is called once per frame
		void Update ()
		{
			
		}
		
		private Vector2 scrollViewVector = Vector2.zero;

		public void OnBackButton ()
		{
			Application.LoadLevel ("FaceTrackerSample");
		}
	}
}
