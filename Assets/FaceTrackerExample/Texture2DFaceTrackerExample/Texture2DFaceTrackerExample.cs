using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ObjdetectModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVFaceTracker;

namespace FaceTrackerExample
{
    /// <summary>
    /// Texture2D Face Tracker Example
    /// </summary>
    public class Texture2DFaceTrackerExample : MonoBehaviour
    {
        /// <summary>
        /// The image texture.
        /// </summary>
        public Texture2D imgTexture;

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
            FaceTracker faceTracker = new FaceTracker (tracker_model_json_filepath);
            //initialize FaceTrackerParams
            FaceTrackerParams faceTrackerParams = new FaceTrackerParams ();
                        
        
            Mat imgMat = new Mat (imgTexture.height, imgTexture.width, CvType.CV_8UC4);
        
            Utils.texture2DToMat (imgTexture, imgMat);
            Debug.Log ("imgMat dst ToString " + imgMat.ToString ());


            CascadeClassifier cascade = new CascadeClassifier ();
            cascade.load (haarcascade_frontalface_alt_xml_filepath);
//            if (cascade.empty())
//            {
//                Debug.LogError("cascade file is not loaded.Please copy from “FaceTrackerExample/StreamingAssets/” to “Assets/StreamingAssets/” folder. ");
//            }

            //convert image to greyscale
            Mat gray = new Mat ();
            Imgproc.cvtColor (imgMat, gray, Imgproc.COLOR_RGBA2GRAY);

        
            MatOfRect faces = new MatOfRect ();
        
            Imgproc.equalizeHist (gray, gray);
        
            cascade.detectMultiScale (gray, faces, 1.1f, 2, 0
//                                                         | Objdetect.CASCADE_FIND_BIGGEST_OBJECT
            | Objdetect.CASCADE_SCALE_IMAGE, new Size (gray.cols () * 0.05, gray.cols () * 0.05), new Size ());
        
            Debug.Log ("faces " + faces.dump ());
        
            if (faces.rows () > 0) {
                //add initial face points from MatOfRect
                faceTracker.addPoints (faces);
            }


            //track face points.if face points <= 0, always return false.
            if (faceTracker.track (imgMat, faceTrackerParams)) faceTracker.draw (imgMat, new Scalar (255, 0, 0, 255), new Scalar (0, 255, 0, 255));


        
            Texture2D texture = new Texture2D (imgMat.cols (), imgMat.rows (), TextureFormat.RGBA32, false);
        
            Utils.matToTexture2D (imgMat, texture);
        
            gameObject.GetComponent<Renderer> ().material.mainTexture = texture;

            cascade.Dispose ();
        }
    
        // Update is called once per frame
        void Update ()
        {
    
        }

        /// <summary>
        /// Raises the destroy event.
        /// </summary>
        void OnDestroy ()
        {
            #if UNITY_WEBGL && !UNITY_EDITOR
            if (getFilePath_Coroutine != null) {
                StopCoroutine (getFilePath_Coroutine);
                ((IDisposable)getFilePath_Coroutine).Dispose ();
            }
            #endif
        }

        public void OnBackButton ()
        {
            SceneManager.LoadScene ("FaceTrackerExample");
        }
    }
}
