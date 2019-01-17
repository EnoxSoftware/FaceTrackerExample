using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ObjdetectModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.UnityUtils.Helper;
using OpenCVFaceTracker;

namespace FaceTrackerExample
{
    /// <summary>
    /// WebCamTexture Face Tracker Example
    /// </summary>
    [RequireComponent (typeof(WebCamTextureToMatHelper))]
    public class WebCamTextureFaceTrackerExample : MonoBehaviour
    {
        /// <summary>
        /// The auto reset mode. if ture, Only if face is detected in each frame, face is tracked.
        /// </summary>
        public bool isAutoResetMode;

        /// <summary>
        /// The auto reset mode toggle.
        /// </summary>
        public Toggle isAutoResetModeToggle;
        
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

        /// <summary>
        /// The tracker_model_json_filepath.
        /// </summary>
        private string tracker_model_json_filepath;
        
        /// <summary>
        /// The haarcascade_frontalface_alt_xml_filepath.
        /// </summary>
        private string haarcascade_frontalface_alt_xml_filepath;

        #if UNITY_WEBGL && !UNITY_EDITOR
        IEnumerator getFilePath_Coroutine;
        #endif

        // Use this for initialization
        void Start ()
        {
            webCamTextureToMatHelper = gameObject.GetComponent<WebCamTextureToMatHelper> ();

            isAutoResetModeToggle.isOn = isAutoResetMode;

            #if UNITY_WEBGL && !UNITY_EDITOR
            getFilePath_Coroutine = GetFilePath ();
            StartCoroutine (getFilePath_Coroutine);
            #else
            tracker_model_json_filepath = Utils.getFilePath ("tracker_model.json");
            haarcascade_frontalface_alt_xml_filepath = Utils.getFilePath ("haarcascade_frontalface_alt.xml");
            Run ();
            #endif            
        }

        #if UNITY_WEBGL && !UNITY_EDITOR
        private IEnumerator GetFilePath ()
        {
            var getFilePathAsync_0_Coroutine = Utils.getFilePathAsync ("tracker_model.json", (result) => {
                tracker_model_json_filepath = result;
            });
            yield return getFilePathAsync_0_Coroutine;

            var getFilePathAsync_1_Coroutine = Utils.getFilePathAsync ("haarcascade_frontalface_alt.xml", (result) => {
                haarcascade_frontalface_alt_xml_filepath = result;
            });
            yield return getFilePathAsync_1_Coroutine;

            getFilePath_Coroutine = null;
            
            Run();
        }
        #endif

        private void Run ()
        {
            //initialize FaceTracker
            faceTracker = new FaceTracker (tracker_model_json_filepath);
            //initialize FaceTrackerParams
            faceTrackerParams = new FaceTrackerParams ();

            cascade = new CascadeClassifier ();
            cascade.load (haarcascade_frontalface_alt_xml_filepath);
//            if (cascade.empty())
//            {
//                Debug.LogError("cascade file is not loaded.Please copy from “FaceTrackerExample/StreamingAssets/” to “Assets/StreamingAssets/” folder. ");
//            }

            #if UNITY_ANDROID && !UNITY_EDITOR
            // Avoids the front camera low light issue that occurs in only some Android devices (e.g. Google Pixel, Pixel2).
            webCamTextureToMatHelper.avoidAndroidFrontCameraLowLightIssue = true;
            #endif
            webCamTextureToMatHelper.Initialize ();
        }

        /// <summary>
        /// Raises the webcam texture to mat helper initialized event.
        /// </summary>
        public void OnWebCamTextureToMatHelperInitialized ()
        {
            Debug.Log ("OnWebCamTextureToMatHelperInitialized");
            
            Mat webCamTextureMat = webCamTextureToMatHelper.GetMat ();
            
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
        }

        /// <summary>
        /// Raises the webcam texture to mat helper disposed event.
        /// </summary>
        public void OnWebCamTextureToMatHelperDisposed ()
        {
            Debug.Log ("OnWebCamTextureToMatHelperDisposed");

            faceTracker.reset ();
            grayMat.Dispose ();
        }

        /// <summary>
        /// Raises the webcam texture to mat helper error occurred event.
        /// </summary>
        /// <param name="errorCode">Error code.</param>
        public void OnWebCamTextureToMatHelperErrorOccurred (WebCamTextureToMatHelper.ErrorCode errorCode)
        {
            Debug.Log ("OnWebCamTextureToMatHelperErrorOccurred " + errorCode);
        }
            
        // Update is called once per frame
        void Update ()
        {

            if (webCamTextureToMatHelper.IsPlaying () && webCamTextureToMatHelper.DidUpdateThisFrame ()) {
                
                Mat rgbaMat = webCamTextureToMatHelper.GetMat ();

                //convert image to greyscale
                Imgproc.cvtColor (rgbaMat, grayMat, Imgproc.COLOR_RGBA2GRAY);
                                        
                                            
                if (isAutoResetMode || faceTracker.getPoints ().Count <= 0) {
//                    Debug.Log ("detectFace");

                    //convert image to greyscale
                    using (Mat equalizeHistMat = new Mat ()) using (MatOfRect faces = new MatOfRect ()) {
                                                
                        Imgproc.equalizeHist (grayMat, equalizeHistMat);
                                                
                        cascade.detectMultiScale (equalizeHistMat, faces, 1.1f, 2, 0
                        //                                                                                 | Objdetect.CASCADE_FIND_BIGGEST_OBJECT
                        | Objdetect.CASCADE_SCALE_IMAGE, new Size (equalizeHistMat.cols () * 0.15, equalizeHistMat.cols () * 0.15), new Size ());
                                                
                        if (faces.rows () > 0) {
//                            Debug.Log ("faces " + faces.dump ());

                            List<OpenCVForUnity.CoreModule.Rect> rectsList = faces.toList ();
                            List<Point[]> pointsList = faceTracker.getPoints ();

                            if (isAutoResetMode) {
                                //add initial face points from MatOfRect
                                if (pointsList.Count <= 0) {
                                    faceTracker.addPoints (faces);
//                                    Debug.Log ("reset faces ");
                                } else {
                                                        
                                    for (int i = 0; i < rectsList.Count; i++) {
                                                        
                                        OpenCVForUnity.CoreModule.Rect trackRect = new OpenCVForUnity.CoreModule.Rect (rectsList [i].x + rectsList [i].width / 3, rectsList [i].y + rectsList [i].height / 2, rectsList [i].width / 3, rectsList [i].height / 3);
                                        //It determines whether nose point has been included in trackRect.                                      
                                        if (i < pointsList.Count && !trackRect.contains (pointsList [i] [67])) {
                                            rectsList.RemoveAt (i);
                                            pointsList.RemoveAt (i);
//                                                                                      Debug.Log ("remove " + i);
                                        }
                                        Imgproc.rectangle (rgbaMat, new Point (trackRect.x, trackRect.y), new Point (trackRect.x + trackRect.width, trackRect.y + trackRect.height), new Scalar (0, 0, 255, 255), 2);
                                    }
                                }
                            } else {
                                faceTracker.addPoints (faces);
                            }
                            //draw face rect
                            for (int i = 0; i < rectsList.Count; i++) {
                                Imgproc.rectangle (rgbaMat, new Point (rectsList [i].x, rectsList [i].y), new Point (rectsList [i].x + rectsList [i].width, rectsList [i].y + rectsList [i].height), new Scalar (255, 0, 0, 255), 2);
                            }                                                    
                                                
                        } else {
                            if (isAutoResetMode) {
                                faceTracker.reset ();
                            }
                        }
                    }                                            
                }

                //track face points.if face points <= 0, always return false.
                if (faceTracker.track (grayMat, faceTrackerParams)) faceTracker.draw (rgbaMat, new Scalar (255, 0, 0, 255), new Scalar (0, 255, 0, 255));
                                        
                Imgproc.putText (rgbaMat, "'Tap' or 'Space Key' to Reset", new Point (5, rgbaMat.rows () - 5), Imgproc.FONT_HERSHEY_SIMPLEX, 0.8, new Scalar (255, 255, 255, 255), 2, Imgproc.LINE_AA, false);                                        
                                        
//                Imgproc.putText (rgbaMat, "W:" + rgbaMat.width () + " H:" + rgbaMat.height () + " SO:" + Screen.orientation, new Point (5, rgbaMat.rows () - 10), Imgproc.FONT_HERSHEY_SIMPLEX, 1.0, new Scalar (255, 255, 255, 255), 2, Imgproc.LINE_AA, false);

                Utils.fastMatToTexture2D (rgbaMat, texture);                                 
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

            if (cascade != null) cascade.Dispose ();

            #if UNITY_WEBGL && !UNITY_EDITOR
            if (getFilePath_Coroutine != null) {
                StopCoroutine (getFilePath_Coroutine);
                ((IDisposable)getFilePath_Coroutine).Dispose ();
            }
            #endif
        }

        /// <summary>
        /// Raises the back button event.
        /// </summary>
        public void OnBackButton ()
        {
            SceneManager.LoadScene ("FaceTrackerExample");
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
            webCamTextureToMatHelper.requestedIsFrontFacing = !webCamTextureToMatHelper.IsFrontFacing ();
        }

        /// <summary>
        /// Raises the change auto reset mode toggle event.
        /// </summary>
        public void OnIsAutoResetModeToggle ()
        {
            if (isAutoResetModeToggle.isOn) {
                isAutoResetMode = true;
            } else {
                isAutoResetMode = false;
            }
        }
    }
}