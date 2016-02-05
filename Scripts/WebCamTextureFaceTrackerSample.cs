using UnityEngine;
using System.Collections;

using OpenCVForUnity;

namespace FaceTrackerSample
{
		/// <summary>
		/// WebCamTexture face tracker sample.
		/// </summary>
		[RequireComponent(typeof(WebCamTextureToMatHelper))]
		public class WebCamTextureFaceTrackerSample : MonoBehaviour
		{
		
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
						webCamTextureToMatHelper.Init (OnWebCamTextureToMatHelperInited, OnWebCamTextureToMatHelperDisposed);
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
																#if OPENCV_3
														Imgproc.rectangle(rgbaMat, new Point(rects[i].x, rects[i].y), new Point(rects[i].x + rects[i].width, rects[i].y + rects[i].height), new Scalar(255, 0, 0, 255), 2);
																#else
																Core.rectangle (rgbaMat, new Point (rects [i].x, rects [i].y), new Point (rects [i].x + rects [i].width, rects [i].y + rects [i].height), new Scalar (255, 0, 0, 255), 2);
																#endif
														}
													
												}
												
										}
											
								}
										
										
								//track face points.if face points <= 0, always return false.
								if (faceTracker.track (grayMat, faceTrackerParams))
										faceTracker.draw (rgbaMat, new Scalar (255, 0, 0, 255), new Scalar (0, 255, 0, 255));
										
								#if OPENCV_3
										Imgproc.putText(rgbaMat, "'Tap' or 'Space Key' to Reset", new Point(5, rgbaMat.rows() - 5), Core.FONT_HERSHEY_SIMPLEX, 0.8, new Scalar(255, 255, 255, 255), 2, Imgproc.LINE_AA, false);
								#else
								Core.putText (rgbaMat, "'Tap' or 'Space Key' to Reset", new Point (5, rgbaMat.rows () - 5), Core.FONT_HERSHEY_SIMPLEX, 0.8, new Scalar (255, 255, 255, 255), 2, Core.LINE_AA, false);
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
						Application.LoadLevel ("FaceTrackerSample");
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
						webCamTextureToMatHelper.Init (null, webCamTextureToMatHelper.requestWidth, webCamTextureToMatHelper.requestHeight, !webCamTextureToMatHelper.requestIsFrontFacing, OnWebCamTextureToMatHelperInited, OnWebCamTextureToMatHelperDisposed);
				}
				
				
				
		}
}