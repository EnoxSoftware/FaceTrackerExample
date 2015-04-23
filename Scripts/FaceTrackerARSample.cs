using UnityEngine;
using System.Collections;

using OpenCVForUnity;

namespace FaceTrackerSample
{
		/// <summary>
		/// Face tracker AR sample.
		/// This sample was referring to http://www.morethantechnical.com/2012/10/17/head-pose-estimation-with-opencv-opengl-revisited-w-code/
		/// and use effect asset from http://ktk-kumamoto.hatenablog.com/entry/2014/09/14/092400
		/// </summary>
		public class FaceTrackerARSample : MonoBehaviour
		{
				/// <summary>
				/// The is draw points.
				/// </summary>
				public bool isDrawPoints;

				/// <summary>
				/// The axes.
				/// </summary>
				public GameObject axes;

				/// <summary>
				/// The head.
				/// </summary>
				public GameObject head;

				/// <summary>
				/// The right eye.
				/// </summary>
				public GameObject rightEye;

				/// <summary>
				/// The left eye.
				/// </summary>
				public GameObject leftEye;

				/// <summary>
				/// The mouth.
				/// </summary>
				public GameObject mouth;

				/// <summary>
				/// The rvec noise filter range.
				/// </summary>
				[Range(0, 50)]
				public float
						rvecNoiseFilterRange = 8;

				/// <summary>
				/// The tvec noise filter range.
				/// </summary>
				[Range(0, 360)]
				public float
						tvecNoiseFilterRange = 90;

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
				/// The is front facing.
				/// </summary>
				public bool isFrontFacing = true;

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
				/// The face tracker.
				/// </summary>
				FaceTracker faceTracker;

				/// <summary>
				/// The face tracker parameters.
				/// </summary>
				FaceTrackerParams faceTrackerParams;

				/// <summary>
				/// The AR camera.
				/// </summary>
				public Camera ARCamera;

				/// <summary>
				/// The cam matrix.
				/// </summary>
				Mat camMatrix;

				/// <summary>
				/// The dist coeffs.
				/// </summary>
				MatOfDouble distCoeffs;

				/// <summary>
				/// The look at m.
				/// </summary>
				Matrix4x4 lookAtM;

				/// <summary>
				/// The transformation m.
				/// </summary>
				Matrix4x4 transformationM = new Matrix4x4 ();

				/// <summary>
				/// The invert Z.
				/// </summary>
				Matrix4x4 invertZM;

				/// <summary>
				/// The model view mtrx.
				/// </summary>
				Matrix4x4 modelViewMtrx;

				/// <summary>
				/// The object points.
				/// </summary>
				MatOfPoint3f objectPoints = new MatOfPoint3f (new Point3 (-31, 72, 86),//l eye
		                                              new Point3 (31, 72, 86),//r eye
		                                              new Point3 (0, 40, 114),//nose
		                                              new Point3 (-23, 19, 76),//l mouse
		                                              new Point3 (23, 19, 76)//r mouse
//		                                              	                                              ,
//		                                              	                                              new Point3 (-70, 60, -9),//l ear
//		                                              	                                              new Point3 (70, 60, -9)//r ear
				);

				/// <summary>
				/// The image points.
				/// </summary>
				MatOfPoint2f imagePoints = new MatOfPoint2f ();

				/// <summary>
				/// The rvec.
				/// </summary>
				Mat rvec = new Mat ();

				/// <summary>
				/// The tvec.
				/// </summary>
				Mat tvec = new Mat ();

				/// <summary>
				/// The rot m.
				/// </summary>
				Mat rotM = new Mat (3, 3, CvType.CV_64FC1);

				/// <summary>
				/// The old rvec.
				/// </summary>
				Mat oldRvec;

				/// <summary>
				/// The old tvec.
				/// </summary>
				Mat oldTvec;
		
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
						axes.SetActive (false);
						head.SetActive (false);
						rightEye.SetActive (false);
						leftEye.SetActive (false);
						mouth.SetActive (false);
			

						if (webCamTexture != null) {
								faceTracker.reset ();
								
								webCamTexture.Stop ();
								initDone = false;
				
								rgbaMat.Dispose ();
								grayMat.Dispose ();
								cascade.Dispose ();
								camMatrix.Dispose ();
								distCoeffs.Dispose ();

						}
			
						// Checks how many and which cameras are available on the device
						for (int cameraIndex = 0; cameraIndex < WebCamTexture.devices.Length; cameraIndex++) {
				
								if (WebCamTexture.devices [cameraIndex].isFrontFacing == isFrontFacing) {
					
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
								#if UNITY_IPHONE && !UNITY_EDITOR
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
					
					
										cascade = new CascadeClassifier (Utils.getFilePath ("haarcascade_frontalface_alt.xml"));
										if (cascade.empty ()) {
												Debug.LogError ("cascade file is not loaded.Please copy from “FaceTrackerSample/StreamingAssets/” to “Assets/StreamingAssets/” folder. ");
										}
						
										gameObject.transform.localScale = new Vector3 (webCamTexture.width, webCamTexture.height, 1);


										gameObject.transform.localEulerAngles = new Vector3 (0, 0, 0);
//										gameObject.transform.rotation = gameObject.transform.rotation * Quaternion.AngleAxis (webCamTexture.videoRotationAngle, Vector3.back);

					
//										bool _videoVerticallyMirrored = webCamTexture.videoVerticallyMirrored;
//										float scaleX = 1;
//										float scaleY = _videoVerticallyMirrored ? -1.0f : 1.0f;
//										gameObject.transform.localScale = new Vector3 (scaleX * gameObject.transform.localScale.x, scaleY * gameObject.transform.localScale.y, 1);
					
					
										gameObject.GetComponent<Renderer> ().material.mainTexture = texture;
					
										Camera.main.orthographicSize = webCamTexture.height / 2;



										int max_d = Mathf.Max (rgbaMat.rows (), rgbaMat.cols ());
										camMatrix = new Mat (3, 3, CvType.CV_64FC1);
										camMatrix.put (0, 0, max_d);
										camMatrix.put (0, 1, 0);
										camMatrix.put (0, 2, rgbaMat.cols () / 2.0f);
										camMatrix.put (1, 0, 0);
										camMatrix.put (1, 1, max_d);
										camMatrix.put (1, 2, rgbaMat.rows () / 2.0f);
										camMatrix.put (2, 0, 0);
										camMatrix.put (2, 1, 0);
										camMatrix.put (2, 2, 1.0f);
					
										Size imageSize = new Size (rgbaMat.cols (), rgbaMat.rows ());
										double apertureWidth = 0;
										double apertureHeight = 0;
										double[] fovx = new double[1];
										double[] fovy = new double[1];
										double[] focalLength = new double[1];
										Point principalPoint = new Point ();
										double[] aspectratio = new double[1];
					
					
					
					
										Calib3d.calibrationMatrixValues (camMatrix, imageSize, apertureWidth, apertureHeight, fovx, fovy, focalLength, principalPoint, aspectratio);
					
										Debug.Log ("imageSize " + imageSize.ToString ());
										Debug.Log ("apertureWidth " + apertureWidth);
										Debug.Log ("apertureHeight " + apertureHeight);
										Debug.Log ("fovx " + fovx [0]);
										Debug.Log ("fovy " + fovy [0]);
										Debug.Log ("focalLength " + focalLength [0]);
										Debug.Log ("principalPoint " + principalPoint.ToString ());
										Debug.Log ("aspectratio " + aspectratio [0]);
					
					
										ARCamera.fieldOfView = (float)fovy [0];
					
										Debug.Log ("camMatrix " + camMatrix.dump ());
					
					
										distCoeffs = new MatOfDouble (0, 0, 0, 0);
										Debug.Log ("distCoeffs " + distCoeffs.dump ());
					
					
					
										lookAtM = getLookAtMatrix (new Vector3 (0, 0, 0), new Vector3 (0, 0, 1), new Vector3 (0, -1, 0));
										Debug.Log ("lookAt " + lookAtM.ToString ());
					
										invertZM = Matrix4x4.TRS (Vector3.zero, Quaternion.identity, new Vector3 (1, 1, -1));


					
										initDone = true;
					
										break;
								} else {
										yield return 0;
								}
						}
				}
		
				// Update is called once per frame
				void Update ()
				{
						if (!initDone)
								return;
			
						#if UNITY_IPHONE && !UNITY_EDITOR
				        if (webCamTexture.width > 16 && webCamTexture.height > 16) {
						#else
						if (webCamTexture.didUpdateThisFrame) {
								#endif
				
								Utils.webCamTextureToMat (webCamTexture, rgbaMat, colors);

								//flip to correct direction.
								if (webCamTexture.videoVerticallyMirrored) {
										if (webCamDevice.isFrontFacing) {
												if (webCamTexture.videoRotationAngle == 0) {
														Core.flip (rgbaMat, rgbaMat, -1);
												} else if (webCamTexture.videoRotationAngle == 180) {
									
												}
										} else {
												if (webCamTexture.videoRotationAngle == 0) {
														Core.flip (rgbaMat, rgbaMat, 0);
												} else if (webCamTexture.videoRotationAngle == 180) {
														Core.flip (rgbaMat, rgbaMat, 1);
												}
										}
								} else {
										if (webCamDevice.isFrontFacing) {
												if (webCamTexture.videoRotationAngle == 0) {
														Core.flip (rgbaMat, rgbaMat, 1);
												} else if (webCamTexture.videoRotationAngle == 180) {
														Core.flip (rgbaMat, rgbaMat, 0);
												}
										} else {
												if (webCamTexture.videoRotationAngle == 0) {
									
												} else if (webCamTexture.videoRotationAngle == 180) {
														Core.flip (rgbaMat, rgbaMat, -1);
												}
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
														| Objdetect.CASCADE_FIND_BIGGEST_OBJECT
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
								if (faceTracker.track (grayMat, faceTrackerParams)) {
										if (isDrawPoints)
												faceTracker.draw (rgbaMat, new Scalar (255, 0, 0, 255), new Scalar (0, 255, 0, 255));

										Core.putText (rgbaMat, "'Tap' or 'Space Key' to Reset", new Point (5, rgbaMat.rows () - 5), Core.FONT_HERSHEY_SIMPLEX, 0.8, new Scalar (255, 255, 255, 255), 2, Core.LINE_AA, false);


										Point[] points = faceTracker.getPoints () [0];
				
				
										if (points.Length > 0) {

//												for (int i = 0; i < points.Length; i++) {
//														Core.putText (rgbaMat, "" + i, new Point (points [i].x, points [i].y), Core.FONT_HERSHEY_SIMPLEX, 0.3, new Scalar (0, 0, 255, 255), 2, Core.LINE_AA, false);
//												}


												imagePoints.fromArray (
						points [31],//l eye
						points [36],//r eye
						points [67],//nose
						points [48],//l mouth
						points [54] //r mouth
//							,
//											points [1],//l ear
//											points [13]//r ear
												);
					
					
												Calib3d.solvePnP (objectPoints, imagePoints, camMatrix, distCoeffs, rvec, tvec);

												bool isRefresh = false;

												if (tvec.get (2, 0) [0] > 0 && tvec.get (2, 0) [0] < 1200 * ((float)webCamTexture.width/(float)width)) {
									
														isRefresh = true;
					
														if (oldRvec == null) {
																oldRvec = new Mat ();
																rvec.copyTo (oldRvec);
														}
														if (oldTvec == null) {
																oldTvec = new Mat ();
																tvec.copyTo (oldTvec);
														}
												
					
														//filter Rvec Noise.
														using (Mat absDiffRvec = new Mat ()) {
																Core.absdiff (rvec, oldRvec, absDiffRvec);
						
																//				Debug.Log ("absDiffRvec " + absDiffRvec.dump());
						
																using (Mat cmpRvec = new Mat ()) {
																		Core.compare (absDiffRvec, new Scalar (rvecNoiseFilterRange), cmpRvec, Core.CMP_GT);
							
																		if (Core.countNonZero (cmpRvec) > 0)
																				isRefresh = false;
																}
														}
					
					
												
														//filter Tvec Noise.
														using (Mat absDiffTvec = new Mat ()) {
																Core.absdiff (tvec, oldTvec, absDiffTvec);
						
																//				Debug.Log ("absDiffRvec " + absDiffRvec.dump());
						
																using (Mat cmpTvec = new Mat ()) {
																		Core.compare (absDiffTvec, new Scalar (tvecNoiseFilterRange), cmpTvec, Core.CMP_GT);
							
																		if (Core.countNonZero (cmpTvec) > 0)
																				isRefresh = false;
																}
														}
					
												
					
												}
					
												if (isRefresh) {

														if (!rightEye.activeSelf)
																rightEye.SetActive (true);
														if (!leftEye.activeSelf)
																leftEye.SetActive (true);

																												
														if ((Mathf.Abs ((float)(points [48].x - points [56].x)) < Mathf.Abs ((float)(points [31].x - points [36].x)) / 2.2 
																&& Mathf.Abs ((float)(points [51].y - points [57].y)) > Mathf.Abs ((float)(points [31].x - points [36].x)) / 2.9)
																|| Mathf.Abs ((float)(points [51].y - points [57].y)) > Mathf.Abs ((float)(points [31].x - points [36].x)) / 2.7) {

																if (!mouth.activeSelf)
																		mouth.SetActive (true);

														} else {
																if (mouth.activeSelf)
																		mouth.SetActive (false);
														}


						
														rvec.copyTo (oldRvec);
														tvec.copyTo (oldTvec);
						
														Calib3d.Rodrigues (rvec, rotM);
						
														transformationM .SetRow (0, new Vector4 ((float)rotM.get (0, 0) [0], (float)rotM.get (0, 1) [0], (float)rotM.get (0, 2) [0], (float)tvec.get (0, 0) [0]));
														transformationM.SetRow (1, new Vector4 ((float)rotM.get (1, 0) [0], (float)rotM.get (1, 1) [0], (float)rotM.get (1, 2) [0], (float)tvec.get (1, 0) [0]));
														transformationM.SetRow (2, new Vector4 ((float)rotM.get (2, 0) [0], (float)rotM.get (2, 1) [0], (float)rotM.get (2, 2) [0], (float)tvec.get (2, 0) [0]));
														transformationM.SetRow (3, new Vector4 (0, 0, 0, 1));
						
														modelViewMtrx = lookAtM * transformationM * invertZM;
						
														ARCamera.worldToCameraMatrix = modelViewMtrx;
						
						
														//				Debug.Log ("modelViewMtrx " + modelViewMtrx.ToString());
												}
										}
								}


				
								Utils.matToTexture2D (rgbaMat, texture, colors);
				
								
				
				
								
								
				
						}

						if (Input.GetKeyUp (KeyCode.Space) || Input.touchCount > 0) {
								faceTracker.reset ();
								if (oldRvec != null) {
										oldRvec.Dispose ();
										oldRvec = null;
								}
								if (oldTvec != null) {
										oldTvec.Dispose ();
										oldTvec = null;
								}
					
								ARCamera.ResetWorldToCameraMatrix ();

								rightEye.SetActive (false);
								leftEye.SetActive (false);
								mouth.SetActive (false);
						}
			
				}
		
				void OnDisable ()
				{
						webCamTexture.Stop ();
				}

				private Matrix4x4 getLookAtMatrix (Vector3 pos, Vector3 target, Vector3 up)
				{
			
						Vector3 z = Vector3.Normalize (pos - target);
						Vector3 x = Vector3.Normalize (Vector3.Cross (up, z));
						Vector3 y = Vector3.Normalize (Vector3.Cross (z, x));
			
						Matrix4x4 result = new Matrix4x4 ();
						result.SetRow (0, new Vector4 (x.x, x.y, x.z, -(Vector3.Dot (pos, x))));
						result.SetRow (1, new Vector4 (y.x, y.y, y.z, -(Vector3.Dot (pos, y))));
						result.SetRow (2, new Vector4 (z.x, z.y, z.z, -(Vector3.Dot (pos, z))));
						result.SetRow (3, new Vector4 (0, 0, 0, 1));
			
						return result;
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
								isFrontFacing = !isFrontFacing;
								StartCoroutine (init ());
						}

						if (GUILayout.Button ("drawPoints")) {
								if (isDrawPoints) {
										isDrawPoints = false;
								} else {
										isDrawPoints = true;
								}
						}
						if (GUILayout.Button ("axes")) {
								if (axes.activeSelf) {
										axes.SetActive (false);
								} else {
										axes.SetActive (true);
								}
						}
						if (GUILayout.Button ("head")) {
								if (head.activeSelf) {
										head.SetActive (false);
								} else {
										head.SetActive (true);
								}
						}
						
			
						GUILayout.EndVertical ();
				}

		}
}