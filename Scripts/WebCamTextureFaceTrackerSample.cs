using UnityEngine;
using System.Collections;

using System.Collections.Generic;
using UnityEngine.UI;

#if UNITY_5_3
using UnityEngine.SceneManagement;
#endif
using OpenCVForUnity;
using OpenCVFaceTracker;

namespace FaceTrackerSample
{
		/// <summary>
		/// WebCamTexture face tracker sample.
		/// </summary>
		[RequireComponent(typeof(WebCamTextureToMatHelper))]
		public class WebCamTextureFaceTrackerSample : MonoBehaviour
		{

				/// <summary>
				/// The auto reset mode. if ture, Only if face is detected in each frame, face is tracked.
				/// </summary>
				public bool autoResetMode;

				/// <summary>
				/// The auto reset mode toggle.
				/// </summary>
				public Toggle autoResetModeToggle;
		
				/// <summary>
				/// The colors.
				/// </summary>
				Color32[] colors;
		
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
				/// The face tracker.
				/// </summary>
				FaceTracker faceTracker;
		
				/// <summary>
				/// The face tracker parameters.
				/// </summary>
				FaceTrackerParams faceTrackerParams;

				/// <summary>
				/// The web cam texture to mat helper.
				/// </summary>
				WebCamTextureToMatHelper webCamTextureToMatHelper;
		
				// Use this for initialization
				void Start ()
				{
						//initialize FaceTracker
						faceTracker = new FaceTracker (Utils.getFilePath ("tracker_model.json"));
						//initialize FaceTrackerParams
						faceTrackerParams = new FaceTrackerParams ();

						webCamTextureToMatHelper = gameObject.GetComponent<WebCamTextureToMatHelper> ();
						webCamTextureToMatHelper.Init ();

						autoResetModeToggle.isOn = autoResetMode;
				}

				/// <summary>
				/// Raises the web cam texture to mat helper inited event.
				/// </summary>
				public void OnWebCamTextureToMatHelperInited ()
				{
						Debug.Log ("OnWebCamTextureToMatHelperInited");
			
						Mat webCamTextureMat = webCamTextureToMatHelper.GetMat ();
			
						colors = new Color32[webCamTextureMat.cols () * webCamTextureMat.rows ()];
						texture = new Texture2D (webCamTextureMat.cols (), webCamTextureMat.rows (), TextureFormat.RGBA32, false);


						gameObject.transform.localScale = new Vector3 (webCamTextureMat.cols (), webCamTextureMat.rows (), 1);
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
			
						gameObject.GetComponent<Renderer> ().material.mainTexture = texture;



						grayMat = new Mat (webCamTextureMat.rows (), webCamTextureMat.cols (), CvType.CV_8UC1);
						cascade = new CascadeClassifier (Utils.getFilePath ("haarcascade_frontalface_alt.xml"));
						if (cascade.empty ()) {
								Debug.LogError ("cascade file is not loaded.Please copy from “FaceTrackerSample/StreamingAssets/” to “Assets/StreamingAssets/” folder. ");
						}
			
				}
		
				/// <summary>
				/// Raises the web cam texture to mat helper disposed event.
				/// </summary>
				public void OnWebCamTextureToMatHelperDisposed ()
				{
						Debug.Log ("OnWebCamTextureToMatHelperDisposed");

						faceTracker.reset ();
						grayMat.Dispose ();
				}
			
				// Update is called once per frame
				void Update ()
				{

						if (webCamTextureToMatHelper.isPlaying () && webCamTextureToMatHelper.didUpdateThisFrame ()) {
				
								Mat rgbaMat = webCamTextureToMatHelper.GetMat ();

								//convert image to greyscale
								Imgproc.cvtColor (rgbaMat, grayMat, Imgproc.COLOR_RGBA2GRAY);
										
											
								if (autoResetMode || faceTracker.getPoints ().Count <= 0) {
//										Debug.Log ("detectFace");

										//convert image to greyscale
										using (Mat equalizeHistMat = new Mat ()) 
										using (MatOfRect faces = new MatOfRect ()) {
												
												Imgproc.equalizeHist (grayMat, equalizeHistMat);
												
												cascade.detectMultiScale (equalizeHistMat, faces, 1.1f, 2, 0
												//														                           | Objdetect.CASCADE_FIND_BIGGEST_OBJECT
														| Objdetect.CASCADE_SCALE_IMAGE, new OpenCVForUnity.Size (equalizeHistMat.cols () * 0.15, equalizeHistMat.cols () * 0.15), new Size ());
												
												if (faces.rows () > 0) {
//												Debug.Log ("faces " + faces.dump ());

														List<OpenCVForUnity.Rect> rectsList = faces.toList ();
														List<Point[]> pointsList = faceTracker.getPoints ();

														if (autoResetMode) {
																//add initial face points from MatOfRect
																if (pointsList.Count <= 0) {
																		faceTracker.addPoints (faces);
//																		Debug.Log ("reset faces ");
																} else {
														
																		for (int i = 0; i < rectsList.Count; i++) {
														
																				OpenCVForUnity.Rect trackRect = new OpenCVForUnity.Rect (rectsList [i].x + rectsList [i].width / 3, rectsList [i].y + rectsList [i].height / 2, rectsList [i].width / 3, rectsList [i].height / 3);
																				//It determines whether nose point has been included in trackRect.										
																				if (i < pointsList.Count && !trackRect.contains (pointsList [i] [67])) {
																						rectsList.RemoveAt (i);
																						pointsList.RemoveAt (i);
//																						Debug.Log ("remove " + i);
																				}
																				Imgproc.rectangle (rgbaMat, new Point (trackRect.x, trackRect.y), new Point (trackRect.x + trackRect.width, trackRect.y + trackRect.height), new Scalar (0, 0, 255, 255), 2);
																		}
																}
														} else {
																faceTracker.addPoints (faces);
														}
														//draw face rect
														for (int i = 0; i < rectsList.Count; i++) {
																#if OPENCV_2
								Core.rectangle (rgbaMat, new Point (rectsList [i].x, rectsList [i].y), new Point (rectsList [i].x + rectsLIst [i].width, rectsList [i].y + rectsList [i].height), new Scalar (255, 0, 0, 255), 2);
																#else
																Imgproc.rectangle (rgbaMat, new Point (rectsList [i].x, rectsList [i].y), new Point (rectsList [i].x + rectsList [i].width, rectsList [i].y + rectsList [i].height), new Scalar (255, 0, 0, 255), 2);
																#endif
														}
													
												
												} else {
														if (autoResetMode) {
																faceTracker.reset ();
														}
												}
										}
											
								}

								//track face points.if face points <= 0, always return false.
								if (faceTracker.track (grayMat, faceTrackerParams))
										faceTracker.draw (rgbaMat, new Scalar (255, 0, 0, 255), new Scalar (0, 255, 0, 255));
										
										
								#if OPENCV_2
				Core.putText (rgbaMat, "'Tap' or 'Space Key' to Reset", new Point (5, rgbaMat.rows () - 5), Core.FONT_HERSHEY_SIMPLEX, 0.8, new Scalar (255, 255, 255, 255), 2, Core.LINE_AA, false);
								#else
								Imgproc.putText (rgbaMat, "'Tap' or 'Space Key' to Reset", new Point (5, rgbaMat.rows () - 5), Core.FONT_HERSHEY_SIMPLEX, 0.8, new Scalar (255, 255, 255, 255), 2, Imgproc.LINE_AA, false);
								#endif
										
										
//								Core.putText (rgbaMat, "W:" + rgbaMat.width () + " H:" + rgbaMat.height () + " SO:" + Screen.orientation, new Point (5, rgbaMat.rows () - 10), Core.FONT_HERSHEY_SIMPLEX, 1.0, new Scalar (255, 255, 255, 255), 2, Core.LINE_AA, false);

								Utils.matToTexture2D (rgbaMat, texture, colors);
										
						}
									
						if (Input.GetKeyUp (KeyCode.Space) || Input.touchCount > 0) {
								faceTracker.reset ();
						}
					
				}

				
				/// <summary>
				/// Raises the disable event.
				/// </summary>
				void OnDisable ()
				{
						webCamTextureToMatHelper.Dispose ();
				}
	
				/// <summary>
				/// Raises the back button event.
				/// </summary>
				public void OnBackButton ()
				{
						#if UNITY_5_3
			SceneManager.LoadScene ("FaceTrackerSample");
						#else
						Application.LoadLevel ("FaceTrackerSample");
						#endif
				}
	
				/// <summary>
				/// Raises the play button event.
				/// </summary>
				public void OnPlayButton ()
				{
						webCamTextureToMatHelper.Play ();
				}
	
				/// <summary>
				/// Raises the pause button event.
				/// </summary>
				public void OnPauseButton ()
				{
						webCamTextureToMatHelper.Pause ();
				}
	
				/// <summary>
				/// Raises the stop button event.
				/// </summary>
				public void OnStopButton ()
				{
						webCamTextureToMatHelper.Stop ();
				}
	
				/// <summary>
				/// Raises the change camera button event.
				/// </summary>
				public void OnChangeCameraButton ()
				{
						webCamTextureToMatHelper.Init (null, webCamTextureToMatHelper.requestWidth, webCamTextureToMatHelper.requestHeight, !webCamTextureToMatHelper.requestIsFrontFacing);
				}
				
				/// <summary>
				/// Raises the change auto reset mode toggle event.
				/// </summary>
				public void OnChangeAutoResetModeToggle ()
				{
						if (autoResetModeToggle.isOn) {
								autoResetMode = true;
						} else {
								autoResetMode = false;
						}
				}
				
		}
}