using UnityEngine;
using System.Collections;

using OpenCVForUnity;

namespace FaceTrackerSample
{
		/// <summary>
		/// WebCamTexture face tracker sample.
		/// </summary>
		public class WebCamTextureFaceTrackerSample : MonoBehaviour
		{
				/// <summary>
				/// The web cam texture.
				/// </summary>
				WebCamTexture webCamTexture;

				/// <summary>
				/// The web cam device.
				/// </summary>
				WebCamDevice webCamDevice;

				/// <summary>
				/// The colors.
				/// </summary>
				Color32[] colors;

				/// <summary>
				/// Should use front facing.
				/// </summary>
				public bool shouldUseFrontFacing = true;

				/// <summary>
				/// The width.
				/// </summary>
				int width = 640;

				/// <summary>
				/// The height.
				/// </summary>
				int height = 480;

				/// <summary>
				/// The rgba mat.
				/// </summary>
				Mat rgbaMat;

				/// <summary>
				/// The gray mat.
				/// </summary>
				Mat grayMat;

				/// <summary>
				/// The texture.
				/// </summary>
				Texture2D texture;

				/// <summary>
				/// The cascade.
				/// </summary>
				CascadeClassifier cascade;

				/// <summary>
				/// The init done.
				/// </summary>
				bool initDone = false;

				/// <summary>
				/// The screenOrientation.
				/// </summary>
				ScreenOrientation screenOrientation = ScreenOrientation.Unknown;

				/// <summary>
				/// The face tracker.
				/// </summary>
				FaceTracker faceTracker;

				/// <summary>
				/// The face tracker parameters.
				/// </summary>
				FaceTrackerParams faceTrackerParams;
	
				// Use this for initialization
				void Start ()
				{
						//initialize FaceTracker
						faceTracker = new FaceTracker (Utils.getFilePath ("tracker_model.json"));
						//initialize FaceTrackerParams
						faceTrackerParams = new FaceTrackerParams ();
		
						StartCoroutine (init ());

				}
	
				private IEnumerator init ()
				{

						if (webCamTexture != null) {
								faceTracker.reset ();
								webCamTexture.Stop ();
								initDone = false;
				
								rgbaMat.Dispose ();
								grayMat.Dispose ();
						}

						// Checks how many and which cameras are available on the device
						for (int cameraIndex = 0; cameraIndex < WebCamTexture.devices.Length; cameraIndex++) {
				
				
								if (WebCamTexture.devices [cameraIndex].isFrontFacing == shouldUseFrontFacing) {
					
					
										Debug.Log (cameraIndex + " name " + WebCamTexture.devices [cameraIndex].name + " isFrontFacing " + WebCamTexture.devices [cameraIndex].isFrontFacing);

										webCamDevice = WebCamTexture.devices [cameraIndex];
									
										webCamTexture = new WebCamTexture (webCamDevice.name, width, height);
		
										break;
								}
				
				
						}
			
						if (webCamTexture == null) {
								webCamDevice = WebCamTexture.devices [0];
								webCamTexture = new WebCamTexture (webCamDevice.name, width, height);

						}
			
						Debug.Log ("width " + webCamTexture.width + " height " + webCamTexture.height + " fps " + webCamTexture.requestedFPS);
			

			
						// Starts the camera
						webCamTexture.Play ();


						while (true) {
								//If you want to use webcamTexture.width and webcamTexture.height on iOS, you have to wait until webcamTexture.didUpdateThisFrame == 1, otherwise these two values will be equal to 16. (http://forum.unity3d.com/threads/webcamtexture-and-error-0x0502.123922/)
								#if UNITY_IOS && !UNITY_EDITOR && (UNITY_4_6_3 || UNITY_4_6_4 || UNITY_5_0_0 || UNITY_5_0_1)
				                if (webCamTexture.width > 16 && webCamTexture.height > 16) {
								#else
								if (webCamTexture.didUpdateThisFrame) {
										#endif
										Debug.Log ("width " + webCamTexture.width + " height " + webCamTexture.height + " fps " + webCamTexture.requestedFPS);
										Debug.Log ("videoRotationAngle " + webCamTexture.videoRotationAngle + " videoVerticallyMirrored " + webCamTexture.videoVerticallyMirrored + " isFrongFacing " + webCamDevice.isFrontFacing);
				
										colors = new Color32[webCamTexture.width * webCamTexture.height];
				
										rgbaMat = new Mat (webCamTexture.height, webCamTexture.width, CvType.CV_8UC4);
										grayMat = new Mat (webCamTexture.height, webCamTexture.width, CvType.CV_8UC1);
				
										texture = new Texture2D (webCamTexture.width, webCamTexture.height, TextureFormat.RGBA32, false);

										gameObject.GetComponent<Renderer> ().material.mainTexture = texture;

										updateLayout ();

										cascade = new CascadeClassifier (Utils.getFilePath ("haarcascade_frontalface_alt.xml"));
										if (cascade.empty ()) {
												Debug.LogError ("cascade file is not loaded.Please copy from “FaceTrackerSample/StreamingAssets/” to “Assets/StreamingAssets/” folder. ");
										}

										screenOrientation = Screen.orientation;
										initDone = true;
				
										break;
								} else {
										yield return 0;
								}
						}
				}

				private void updateLayout ()
				{
						gameObject.transform.localRotation = new Quaternion (0, 0, 0, 0);
						gameObject.transform.localScale = new Vector3 (webCamTexture.width, webCamTexture.height, 1);

						if (webCamTexture.videoRotationAngle == 90 || webCamTexture.videoRotationAngle == 270) {
								gameObject.transform.eulerAngles = new Vector3 (0, 0, -90);
						}


						float width = 0;
						float height = 0;
						if (webCamTexture.videoRotationAngle == 90 || webCamTexture.videoRotationAngle == 270) {
								width = gameObject.transform.localScale.y;
								height = gameObject.transform.localScale.x;
						} else if (webCamTexture.videoRotationAngle == 0 || webCamTexture.videoRotationAngle == 180) {
								width = gameObject.transform.localScale.x;
								height = gameObject.transform.localScale.y;
						}

						float widthScale = (float)Screen.width / width;
						float heightScale = (float)Screen.height / height;
						if (widthScale < heightScale) {
								Camera.main.orthographicSize = (width * (float)Screen.height / (float)Screen.width) / 2;
						} else {
								Camera.main.orthographicSize = height / 2;
						}
				}

				// Update is called once per frame
				void Update ()
				{
						if (!initDone)
								return;


						if (screenOrientation != Screen.orientation) {
								screenOrientation = Screen.orientation;
								updateLayout ();
						}

						#if UNITY_IOS && !UNITY_EDITOR && (UNITY_4_6_3 || UNITY_4_6_4 || UNITY_5_0_0 || UNITY_5_0_1)
				        if (webCamTexture.width > 16 && webCamTexture.height > 16) {
						#else
						if (webCamTexture.didUpdateThisFrame) {
								#if UNITY_IOS && !UNITY_EDITOR && UNITY_5_2                                    
								while (webCamTexture.width <= 16) {
										webCamTexture.GetPixels32 ();
										yield return new WaitForEndOfFrame ();
								} 
								#endif
						#endif
								Utils.webCamTextureToMat (webCamTexture, rgbaMat, colors);

								//flip to correct direction.
								if (webCamDevice.isFrontFacing) {
										if (webCamTexture.videoRotationAngle == 0) {
												Core.flip (rgbaMat, rgbaMat, 1);
										} else if (webCamTexture.videoRotationAngle == 90) {
												Core.flip (rgbaMat, rgbaMat, 0);
										}
										if (webCamTexture.videoRotationAngle == 180) {
												Core.flip (rgbaMat, rgbaMat, 0);
										} else if (webCamTexture.videoRotationAngle == 270) {
												Core.flip (rgbaMat, rgbaMat, 1);
										}
								} else {
										if (webCamTexture.videoRotationAngle == 180) {
												Core.flip (rgbaMat, rgbaMat, -1);
										} else if (webCamTexture.videoRotationAngle == 270) {
												Core.flip (rgbaMat, rgbaMat, -1);
										}
								}

								//convert image to greyscale
								Imgproc.cvtColor (rgbaMat, grayMat, Imgproc.COLOR_RGBA2GRAY);


								if (faceTracker.getPoints ().Count <= 0) {
										Debug.Log ("detectFace");

										//convert image to greyscale
										using (Mat equalizeHistMat = new Mat ()) 
										using (MatOfRect faces = new MatOfRect ()) {
			
												Imgproc.equalizeHist (grayMat, equalizeHistMat);
			
												cascade.detectMultiScale (equalizeHistMat, faces, 1.1f, 2, 0
//														                           | Objdetect.CASCADE_FIND_BIGGEST_OBJECT
														| Objdetect.CASCADE_SCALE_IMAGE, new OpenCVForUnity.Size (equalizeHistMat.cols () * 0.15, equalizeHistMat.cols () * 0.15), new Size ());
			

			
												if (faces.rows () > 0) {
														Debug.Log ("faces " + faces.dump ());
														//add initial face points from MatOfRect
														faceTracker.addPoints (faces);

														//draw face rect
														OpenCVForUnity.Rect[] rects = faces.toArray ();
														for (int i = 0; i < rects.Length; i++) {
																Core.rectangle (rgbaMat, new Point (rects [i].x, rects [i].y), new Point (rects [i].x + rects [i].width, rects [i].y + rects [i].height), new Scalar (255, 0, 0, 255), 2);
														}
														
												}

										}

								}


								//track face points.if face points <= 0, always return false.
								if (faceTracker.track (grayMat, faceTrackerParams))
										faceTracker.draw (rgbaMat, new Scalar (255, 0, 0, 255), new Scalar (0, 255, 0, 255));

								Core.putText (rgbaMat, "'Tap' or 'Space Key' to Reset", new Point (5, rgbaMat.rows () - 5), Core.FONT_HERSHEY_SIMPLEX, 0.8, new Scalar (255, 255, 255, 255), 2, Core.LINE_AA, false);

			
								Utils.matToTexture2D (rgbaMat, texture, colors);
			
						}

						if (Input.GetKeyUp (KeyCode.Space) || Input.touchCount > 0) {
								faceTracker.reset ();
						}
		
				}
	
				void OnDisable ()
				{
						webCamTexture.Stop ();
				}

				void OnGUI ()
				{
						float screenScale = Screen.height / 240.0f;
						Matrix4x4 scaledMatrix = Matrix4x4.Scale (new Vector3 (screenScale, screenScale, screenScale));
						GUI.matrix = scaledMatrix;
			
			
						GUILayout.BeginVertical ();
						if (GUILayout.Button ("back")) {
								Application.LoadLevel ("FaceTrackerSample");
						}
						if (GUILayout.Button ("change camera")) {
								shouldUseFrontFacing = !shouldUseFrontFacing;
								StartCoroutine (init ());
						}
			
						GUILayout.EndVertical ();
				}

		}
}