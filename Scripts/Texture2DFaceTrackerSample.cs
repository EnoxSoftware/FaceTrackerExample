using UnityEngine;
using System.Collections;

#if UNITY_5_3
using UnityEngine.SceneManagement;
#endif
using OpenCVForUnity;
using OpenCVFaceTracker;

namespace FaceTrackerSample
{
		/// <summary>
		/// Texture2D face tracker sample.
		/// </summary>
		public class Texture2DFaceTrackerSample : MonoBehaviour
		{

				/// <summary>
				/// The image texture.
				/// </summary>
				public Texture2D imgTexture;

				// Use this for initialization
				void Start ()
				{
						gameObject.transform.localScale = new Vector3 (imgTexture.width, imgTexture.height, 1);
						Debug.Log ("Screen.width " + Screen.width + " Screen.height " + Screen.height + " Screen.orientation " + Screen.orientation);
			
						float width = 0;
						float height = 0;
			
						width = gameObject.transform.localScale.x;
						height = gameObject.transform.localScale.y;

						float widthScale = (float)Screen.width / width;
						float heightScale = (float)Screen.height / height;
						if (widthScale < heightScale) {
								Camera.main.orthographicSize = (width * (float)Screen.height / (float)Screen.width) / 2;
						} else {
								Camera.main.orthographicSize = height / 2;
						}


						//initialize FaceTracker
						FaceTracker faceTracker = new FaceTracker (Utils.getFilePath ("tracker_model.json"));
						//initialize FaceTrackerParams
						FaceTrackerParams faceTrackerParams = new FaceTrackerParams ();
						
		
						Mat imgMat = new Mat (imgTexture.height, imgTexture.width, CvType.CV_8UC4);
		
						Utils.texture2DToMat (imgTexture, imgMat);
						Debug.Log ("imgMat dst ToString " + imgMat.ToString ());


						CascadeClassifier cascade = new CascadeClassifier (Utils.getFilePath ("haarcascade_frontalface_alt.xml"));
						if (cascade.empty ()) {
								Debug.LogError ("cascade file is not loaded.Please copy from “FaceTrackerSample/StreamingAssets/” to “Assets/StreamingAssets/” folder. ");
						}

						//convert image to greyscale
						Mat gray = new Mat ();
						Imgproc.cvtColor (imgMat, gray, Imgproc.COLOR_RGBA2GRAY);

		
						MatOfRect faces = new MatOfRect ();
		
						Imgproc.equalizeHist (gray, gray);
		
						cascade.detectMultiScale (gray, faces, 1.1f, 2, 0
//								                           | Objdetect.CASCADE_FIND_BIGGEST_OBJECT
								| Objdetect.CASCADE_SCALE_IMAGE, new OpenCVForUnity.Size (gray.cols () * 0.05, gray.cols () * 0.05), new Size ());
		
						Debug.Log ("faces " + faces.dump ());
		
						if (faces.rows () > 0) {
								//add initial face points from MatOfRect
								faceTracker.addPoints (faces);
						}


						//track face points.if face points <= 0, always return false.
						if (faceTracker.track (imgMat, faceTrackerParams))
								faceTracker.draw (imgMat, new Scalar (255, 0, 0, 255), new Scalar (0, 255, 0, 255));


		
						Texture2D texture = new Texture2D (imgMat.cols (), imgMat.rows (), TextureFormat.RGBA32, false);
		
						Utils.matToTexture2D (imgMat, texture);
		
						gameObject.GetComponent<Renderer> ().material.mainTexture = texture;
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
