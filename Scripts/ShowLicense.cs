﻿using UnityEngine;
using System.Collections;

#if UNITY_5_3
using UnityEngine.SceneManagement;
#endif

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

				public void OnBackButton ()
				{
						#if UNITY_5_3
			SceneManager.LoadScene ("FaceTrackerSample");
						#else
						Application.LoadLevel ("FaceTrackerSample");
						#endif
				}
		}
}
