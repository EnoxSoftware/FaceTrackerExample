using UnityEngine;
using System.Collections;

using System.Collections.Generic;
using UnityEngine.UI;

#if UNITY_5_3 || UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif
using OpenCVForUnity;
using OpenCVFaceTracker;

namespace FaceTrackerExample
{
    /// <summary>
    /// Face tracker AR example.
    /// This Example was referring to http://www.morethantechnical.com/2012/10/17/head-pose-estimation-with-opencv-opengl-revisited-w-code/
    /// and use effect asset from http://ktk-kumamoto.hatenablog.com/entry/2014/09/14/092400
    /// </summary>
    [RequireComponent(typeof(WebCamTextureToMatHelper))]
    public class FaceTrackerARExample : MonoBehaviour
    {

        /// <summary>
        /// The should draw face points.
        /// </summary>
        public bool isShowingFacePoints;

        /// <summary>
        /// The is showing face points toggle.
        /// </summary>
        public Toggle isShowingFacePointsToggle;
        
        /// <summary>
        /// The should draw axes.
        /// </summary>
        public bool isShowingAxes;

        /// <summary>
        /// The is showing axes toggle.
        /// </summary>
        public Toggle isShowingAxesToggle;
        
        /// <summary>
        /// The should draw head.
        /// </summary>
        public bool isShowingHead;

        /// <summary>
        /// The is showing head toggle.
        /// </summary>
        public Toggle isShowingHeadToggle;
        
        /// <summary>
        /// The should draw effects.
        /// </summary>
        public bool isShowingEffects;

        /// <summary>
        /// The is showing effects toggle.
        /// </summary>
        public Toggle isShowingEffectsToggle;

        /// <summary>
        /// The auto reset mode. if ture, Only if face is detected in each frame, face is tracked.
        /// </summary>
        public bool isAutoResetMode;

        /// <summary>
        /// The auto reset mode toggle.
        /// </summary>
        public Toggle isAutoResetModeToggle;
        
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
        /// The invert Y.
        /// </summary>
        Matrix4x4 invertYM;
        
        /// <summary>
        /// The transformation m.
        /// </summary>
        Matrix4x4 transformationM = new Matrix4x4();
        
        /// <summary>
        /// The invert Z.
        /// </summary>
        Matrix4x4 invertZM;
        
        /// <summary>
        /// The ar m.
        /// </summary>
        Matrix4x4 ARM;

        /// <summary>
        /// The ar game object.
        /// </summary>
        public GameObject ARGameObject;

        /// <summary>
        /// The should move AR camera.
        /// </summary>
        public bool shouldMoveARCamera;
        
        /// <summary>
        /// The 3d face object points.
        /// </summary>
        MatOfPoint3f objectPoints;
        
        /// <summary>
        /// The image points.
        /// </summary>
        MatOfPoint2f imagePoints;
        
        /// <summary>
        /// The rvec.
        /// </summary>
        Mat rvec;
        
        /// <summary>
        /// The tvec.
        /// </summary>
        Mat tvec;
        
        /// <summary>
        /// The rot m.
        /// </summary>
        Mat rotM;
        
        /// <summary>
        /// The old rvec.
        /// </summary>
        Mat oldRvec;
        
        /// <summary>
        /// The old tvec.
        /// </summary>
        Mat oldTvec;

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


        // Use this for initialization
        void Start()
        {
            webCamTextureToMatHelper = gameObject.GetComponent<WebCamTextureToMatHelper>();


            isShowingFacePointsToggle.isOn = isShowingFacePoints;
            isShowingAxesToggle.isOn = isShowingAxes;
            isShowingHeadToggle.isOn = isShowingHead;
            isShowingEffectsToggle.isOn = isShowingEffects;
            isAutoResetModeToggle.isOn = isAutoResetMode;

            #if UNITY_WEBGL && !UNITY_EDITOR
            StartCoroutine(getFilePathCoroutine());
            #else
            tracker_model_json_filepath = Utils.getFilePath("tracker_model.json");
            haarcascade_frontalface_alt_xml_filepath = Utils.getFilePath("haarcascade_frontalface_alt.xml");
            Run();
            #endif
            
        }

        #if UNITY_WEBGL && !UNITY_EDITOR
        private IEnumerator getFilePathCoroutine()
        {
            var getFilePathAsync_0_Coroutine = StartCoroutine(Utils.getFilePathAsync("tracker_model.json", (result) => {
                tracker_model_json_filepath = result;
            }));
            var getFilePathAsync_1_Coroutine = StartCoroutine(Utils.getFilePathAsync("haarcascade_frontalface_alt.xml", (result) => {
                haarcascade_frontalface_alt_xml_filepath = result;
            }));
            
            
            yield return getFilePathAsync_0_Coroutine;
            yield return getFilePathAsync_1_Coroutine;
            
            Run();
        }
        #endif

        private void Run()
        {
            //set 3d face object points.
            objectPoints = new MatOfPoint3f(new Point3(-31, 72, 86),//l eye
                new Point3(31, 72, 86),//r eye
                new Point3(0, 40, 114),//nose
                new Point3(-20, 15, 90),//l mouse
                new Point3(20, 15, 90)//r mouse
//                                                                                                                                                            ,
//                                                                                                                                                            new Point3 (-70, 60, -9),//l ear
//                                                                                                                                                            new Point3 (70, 60, -9)//r ear
            );
            imagePoints = new MatOfPoint2f();
            rvec = new Mat();
            tvec = new Mat();
            rotM = new Mat(3, 3, CvType.CV_64FC1);

            //initialize FaceTracker
            faceTracker = new FaceTracker(tracker_model_json_filepath);
            //initialize FaceTrackerParams
            faceTrackerParams = new FaceTrackerParams();

            cascade = new CascadeClassifier();
            cascade.load(haarcascade_frontalface_alt_xml_filepath);
//            if (cascade.empty())
//            {
//                Debug.LogError("cascade file is not loaded.Please copy from “FaceTrackerExample/StreamingAssets/” to “Assets/StreamingAssets/” folder. ");
//            }



            webCamTextureToMatHelper.Initialize();


        }

        /// <summary>
        /// Raises the webcam texture to mat helper initialized event.
        /// </summary>
        public void OnWebCamTextureToMatHelperInitialized()
        {
            Debug.Log("OnWebCamTextureToMatHelperInitialized");
            
            Mat webCamTextureMat = webCamTextureToMatHelper.GetMat();

            texture = new Texture2D(webCamTextureMat.cols(), webCamTextureMat.rows(), TextureFormat.RGBA32, false);
            gameObject.GetComponent<Renderer>().material.mainTexture = texture;


            gameObject.transform.localScale = new Vector3(webCamTextureMat.cols(), webCamTextureMat.rows(), 1);
            
            Debug.Log("Screen.width " + Screen.width + " Screen.height " + Screen.height + " Screen.orientation " + Screen.orientation);
            
            float width = webCamTextureMat.width();
            float height = webCamTextureMat.height();
            
            float imageSizeScale = 1.0f;
            
            width = gameObject.transform.localScale.x;
            height = gameObject.transform.localScale.y;

            float widthScale = (float)Screen.width / width;
            float heightScale = (float)Screen.height / height;
            if (widthScale < heightScale)
            {
                Camera.main.orthographicSize = (width * (float)Screen.height / (float)Screen.width) / 2;
                imageSizeScale = (float)Screen.height / (float)Screen.width;
            } else
            {
                Camera.main.orthographicSize = height / 2;
            }
         
                                    
                                    
            int max_d = (int)Mathf.Max(width, height);
            double fx = max_d;
            double fy = max_d;
            double cx = width / 2.0f;
            double cy = height / 2.0f;
            camMatrix = new Mat(3, 3, CvType.CV_64FC1);
            camMatrix.put(0, 0, fx);
            camMatrix.put(0, 1, 0);
            camMatrix.put(0, 2, cx);
            camMatrix.put(1, 0, 0);
            camMatrix.put(1, 1, fy);
            camMatrix.put(1, 2, cy);
            camMatrix.put(2, 0, 0);
            camMatrix.put(2, 1, 0);
            camMatrix.put(2, 2, 1.0f);
            Debug.Log("camMatrix " + camMatrix.dump());

            distCoeffs = new MatOfDouble(0, 0, 0, 0);
            Debug.Log("distCoeffs " + distCoeffs.dump());
                                    
            Size imageSize = new Size(width * imageSizeScale, height * imageSizeScale);
            double apertureWidth = 0;
            double apertureHeight = 0;
            double[] fovx = new double[1];
            double[] fovy = new double[1];
            double[] focalLength = new double[1];
            Point principalPoint = new Point(0, 0);
            double[] aspectratio = new double[1];
                                                          
                                    
            Calib3d.calibrationMatrixValues(camMatrix, imageSize, apertureWidth, apertureHeight, fovx, fovy, focalLength, principalPoint, aspectratio);
                                    
            Debug.Log("imageSize " + imageSize.ToString());
            Debug.Log("apertureWidth " + apertureWidth);
            Debug.Log("apertureHeight " + apertureHeight);
            Debug.Log("fovx " + fovx [0]);
            Debug.Log("fovy " + fovy [0]);
            Debug.Log("focalLength " + focalLength [0]);
            Debug.Log("principalPoint " + principalPoint.ToString());
            Debug.Log("aspectratio " + aspectratio [0]);
                                    
                                    
            //To convert the difference of the FOV value of the OpenCV and Unity. 
            double fovXScale = (2.0 * Mathf.Atan((float)(imageSize.width / (2.0 * fx)))) / (Mathf.Atan2((float)cx, (float)fx) + Mathf.Atan2((float)(imageSize.width - cx), (float)fx));
            double fovYScale = (2.0 * Mathf.Atan((float)(imageSize.height / (2.0 * fy)))) / (Mathf.Atan2((float)cy, (float)fy) + Mathf.Atan2((float)(imageSize.height - cy), (float)fy));
            
            Debug.Log("fovXScale " + fovXScale);
            Debug.Log("fovYScale " + fovYScale);
            
            
            //Adjust Unity Camera FOV https://github.com/opencv/opencv/commit/8ed1945ccd52501f5ab22bdec6aa1f91f1e2cfd4
            if (widthScale < heightScale)
            {
                ARCamera.fieldOfView = (float)(fovx [0] * fovXScale);
            } else
            {
                ARCamera.fieldOfView = (float)(fovy [0] * fovYScale);
            }
                                    
                                    
                                    
            invertYM = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, -1, 1));
            Debug.Log("invertYM " + invertYM.ToString());
            
            invertZM = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, 1, -1));
            Debug.Log("invertZM " + invertZM.ToString());



            grayMat = new Mat(webCamTextureMat.rows(), webCamTextureMat.cols(), CvType.CV_8UC1);
            

            
            
            axes.SetActive(false);
            head.SetActive(false);
            rightEye.SetActive(false);
            leftEye.SetActive(false);
            mouth.SetActive(false);
            
        }

        /// <summary>
        /// Raises the webcam texture to mat helper disposed event.
        /// </summary>
        public void OnWebCamTextureToMatHelperDisposed()
        {
            Debug.Log("OnWebCamTextureToMatHelperDisposed");
                                    
            faceTracker.reset();

            grayMat.Dispose();
            camMatrix.Dispose();
            distCoeffs.Dispose();
        }

        /// <summary>
        /// Raises the webcam texture to mat helper error occurred event.
        /// </summary>
        /// <param name="errorCode">Error code.</param>
        public void OnWebCamTextureToMatHelperErrorOccurred(WebCamTextureToMatHelper.ErrorCode errorCode)
        {
            Debug.Log("OnWebCamTextureToMatHelperErrorOccurred " + errorCode);
        }

        // Update is called once per frame
        void Update()
        {

            if (webCamTextureToMatHelper.IsPlaying() && webCamTextureToMatHelper.DidUpdateThisFrame())
            {
                
                Mat rgbaMat = webCamTextureToMatHelper.GetMat();


                //convert image to greyscale
                Imgproc.cvtColor(rgbaMat, grayMat, Imgproc.COLOR_RGBA2GRAY);
                                        
                                        
                if (isAutoResetMode || faceTracker.getPoints().Count <= 0)
                {
//                                      Debug.Log ("detectFace");
                                            
                    //convert image to greyscale
                    using (Mat equalizeHistMat = new Mat())
                    using (MatOfRect faces = new MatOfRect())
                    {
                                                
                        Imgproc.equalizeHist(grayMat, equalizeHistMat);
                                                
                        cascade.detectMultiScale(equalizeHistMat, faces, 1.1f, 2, 0
                        | Objdetect.CASCADE_FIND_BIGGEST_OBJECT
                        | Objdetect.CASCADE_SCALE_IMAGE, new OpenCVForUnity.Size(equalizeHistMat.cols() * 0.15, equalizeHistMat.cols() * 0.15), new Size());
                                                
                                                
                                                
                        if (faces.rows() > 0)
                        {
//                                              Debug.Log ("faces " + faces.dump ());

                            List<OpenCVForUnity.Rect> rectsList = faces.toList();
                            List<Point[]> pointsList = faceTracker.getPoints();
                        
                            if (isAutoResetMode)
                            {
                                //add initial face points from MatOfRect
                                if (pointsList.Count <= 0)
                                {
                                    faceTracker.addPoints(faces);                           
//                                                                      Debug.Log ("reset faces ");
                                } else
                                {
                            
                                    for (int i = 0; i < rectsList.Count; i++)
                                    {
                                
                                        OpenCVForUnity.Rect trackRect = new OpenCVForUnity.Rect(rectsList [i].x + rectsList [i].width / 3, rectsList [i].y + rectsList [i].height / 2, rectsList [i].width / 3, rectsList [i].height / 3);
                                        //It determines whether nose point has been included in trackRect.                                      
                                        if (i < pointsList.Count && !trackRect.contains(pointsList [i] [67]))
                                        {
                                            rectsList.RemoveAt(i);
                                            pointsList.RemoveAt(i);
//                                                                                      Debug.Log ("remove " + i);
                                        }
                                        Imgproc.rectangle(rgbaMat, new Point(trackRect.x, trackRect.y), new Point(trackRect.x + trackRect.width, trackRect.y + trackRect.height), new Scalar(0, 0, 255, 255), 2);
                                    }
                                }
                            } else
                            {
                                faceTracker.addPoints(faces);
                            }

                            //draw face rect
                            for (int i = 0; i < rectsList.Count; i++)
                            {
                                #if OPENCV_2
                                Core.rectangle (rgbaMat, new Point (rectsLIst [i].x, rectsList [i].y), new Point (rectsList [i].x + rectsList [i].width, rectsList [i].y + rectsList [i].height), new Scalar (255, 0, 0, 255), 2);
                                #else
                                Imgproc.rectangle(rgbaMat, new Point(rectsList [i].x, rectsList [i].y), new Point(rectsList [i].x + rectsList [i].width, rectsList [i].y + rectsList [i].height), new Scalar(255, 0, 0, 255), 2);
                                #endif
                            }

                        } else
                        {
                            if (isAutoResetMode)
                            {
                                faceTracker.reset();
                        
                                rightEye.SetActive(false);
                                leftEye.SetActive(false);
                                head.SetActive(false);
                                mouth.SetActive(false);
                                axes.SetActive(false);
                            }
                        }
                                                
                    }
                                            
                }
                                        
                                        
                //track face points.if face points <= 0, always return false.
                if (faceTracker.track(grayMat, faceTrackerParams))
                {
                    if (isShowingFacePoints)
                        faceTracker.draw(rgbaMat, new Scalar(255, 0, 0, 255), new Scalar(0, 255, 0, 255));
                                            
                    #if OPENCV_2
                    Core.putText (rgbaMat, "'Tap' or 'Space Key' to Reset", new Point (5, rgbaMat.rows () - 5), Core.FONT_HERSHEY_SIMPLEX, 0.8, new Scalar (255, 255, 255, 255), 2, Core.LINE_AA, false);
                    #else
                    Imgproc.putText(rgbaMat, "'Tap' or 'Space Key' to Reset", new Point(5, rgbaMat.rows() - 5), Core.FONT_HERSHEY_SIMPLEX, 0.8, new Scalar(255, 255, 255, 255), 2, Imgproc.LINE_AA, false);
                    #endif
                                            
                                            
                    Point[] points = faceTracker.getPoints() [0];
                                            
                                            
                    if (points.Length > 0)
                    {
                                                
//                                              for (int i = 0; i < points.Length; i++) {
//                                                      #if OPENCV_2
//                          Core.putText (rgbaMat, "" + i, new Point (points [i].x, points [i].y), Core.FONT_HERSHEY_SIMPLEX, 0.3, new Scalar (0, 0, 255, 255), 2, Core.LINE_AA, false);
//                                                      #else
//                                                      Imgproc.putText (rgbaMat, "" + i, new Point (points [i].x, points [i].y), Core.FONT_HERSHEY_SIMPLEX, 0.3, new Scalar (0, 0, 255, 255), 2, Core.LINE_AA, false);
//                                                      #endif
//                                              }
                                                
                                                
                        imagePoints.fromArray(
                            points [31],//l eye
                            points [36],//r eye
                            points [67],//nose
                            points [48],//l mouth
                            points [54] //r mouth
//                                                                              ,
//                                                                                              points [0],//l ear
//                                                                                              points [14]//r ear
                        );
                                                
                                                
                        Calib3d.solvePnP(objectPoints, imagePoints, camMatrix, distCoeffs, rvec, tvec);
                                                
                        bool isRefresh = false;
                                                
                        if (tvec.get(2, 0) [0] > 0 && tvec.get(2, 0) [0] < 1200 * ((float)rgbaMat.cols() / (float)webCamTextureToMatHelper.requestedWidth))
                        {
                                                    
                            isRefresh = true;
                                                    
                            if (oldRvec == null)
                            {
                                oldRvec = new Mat();
                                rvec.copyTo(oldRvec);
                            }
                            if (oldTvec == null)
                            {
                                oldTvec = new Mat();
                                tvec.copyTo(oldTvec);
                            }
                                                    
                                                    
                            //filter Rvec Noise.
                            using (Mat absDiffRvec = new Mat())
                            {
                                Core.absdiff(rvec, oldRvec, absDiffRvec);
                                                        
                                //              Debug.Log ("absDiffRvec " + absDiffRvec.dump());
                                                        
                                using (Mat cmpRvec = new Mat())
                                {
                                    Core.compare(absDiffRvec, new Scalar(rvecNoiseFilterRange), cmpRvec, Core.CMP_GT);
                                                            
                                    if (Core.countNonZero(cmpRvec) > 0)
                                        isRefresh = false;
                                }
                            }
                                                    
                                                    
                                                    
                            //filter Tvec Noise.
                            using (Mat absDiffTvec = new Mat())
                            {
                                Core.absdiff(tvec, oldTvec, absDiffTvec);
                                                        
                                //              Debug.Log ("absDiffRvec " + absDiffRvec.dump());
                                                        
                                using (Mat cmpTvec = new Mat())
                                {
                                    Core.compare(absDiffTvec, new Scalar(tvecNoiseFilterRange), cmpTvec, Core.CMP_GT);
                                                            
                                    if (Core.countNonZero(cmpTvec) > 0)
                                        isRefresh = false;
                                }
                            }
                                                    
                                                    
                                                    
                        }
                                                
                        if (isRefresh)
                        {
                                                    
                            if (isShowingEffects)
                                rightEye.SetActive(true);
                            if (isShowingEffects)
                                leftEye.SetActive(true);
                            if (isShowingHead)
                                head.SetActive(true);
                            if (isShowingAxes)
                                axes.SetActive(true);
                                                    
                                                    
                            if ((Mathf.Abs((float)(points [48].x - points [56].x)) < Mathf.Abs((float)(points [31].x - points [36].x)) / 2.2
                                && Mathf.Abs((float)(points [51].y - points [57].y)) > Mathf.Abs((float)(points [31].x - points [36].x)) / 2.9)
                                || Mathf.Abs((float)(points [51].y - points [57].y)) > Mathf.Abs((float)(points [31].x - points [36].x)) / 2.7)
                            {
                                                        
                                if (isShowingEffects)
                                    mouth.SetActive(true);
                                                        
                            } else
                            {
                                if (isShowingEffects)
                                    mouth.SetActive(false);
                            }
                                                    
                                                    
                                                    
                            rvec.copyTo(oldRvec);
                            tvec.copyTo(oldTvec);
                                                    
                            Calib3d.Rodrigues(rvec, rotM);
                                                    
                            transformationM.SetRow(0, new Vector4((float)rotM.get(0, 0) [0], (float)rotM.get(0, 1) [0], (float)rotM.get(0, 2) [0], (float)tvec.get(0, 0) [0]));
                            transformationM.SetRow(1, new Vector4((float)rotM.get(1, 0) [0], (float)rotM.get(1, 1) [0], (float)rotM.get(1, 2) [0], (float)tvec.get(1, 0) [0]));
                            transformationM.SetRow(2, new Vector4((float)rotM.get(2, 0) [0], (float)rotM.get(2, 1) [0], (float)rotM.get(2, 2) [0], (float)tvec.get(2, 0) [0]));
                            transformationM.SetRow(3, new Vector4(0, 0, 0, 1));
                                                    
                            if (shouldMoveARCamera)
                            {

                                if (ARGameObject != null)
                                {
                                    ARM = ARGameObject.transform.localToWorldMatrix * invertZM * transformationM.inverse * invertYM;
                                    ARUtils.SetTransformFromMatrix(ARCamera.transform, ref ARM);
                                    ARGameObject.SetActive(true);
                                }
                            } else
                            {
                                ARM = ARCamera.transform.localToWorldMatrix * invertYM * transformationM * invertZM;

                                if (ARGameObject != null)
                                {
                                    ARUtils.SetTransformFromMatrix(ARGameObject.transform, ref ARM);
                                    ARGameObject.SetActive(true);
                                }
                            }

                        }
                    }
                }
                                        
//                              Core.putText (rgbaMat, "W:" + rgbaMat.width () + " H:" + rgbaMat.height () + " SO:" + Screen.orientation, new Point (5, rgbaMat.rows () - 10), Core.FONT_HERSHEY_SIMPLEX, 1.0, new Scalar (255, 255, 255, 255), 2, Core.LINE_AA, false);
                                        
                Utils.matToTexture2D(rgbaMat, texture, webCamTextureToMatHelper.GetBufferColors());
                                        
            }
                                    
            if (Input.GetKeyUp(KeyCode.Space) || Input.touchCount > 0)
            {
                faceTracker.reset();
                if (oldRvec != null)
                {
                    oldRvec.Dispose();
                    oldRvec = null;
                }
                if (oldTvec != null)
                {
                    oldTvec.Dispose();
                    oldTvec = null;
                }
                                        
                rightEye.SetActive(false);
                leftEye.SetActive(false);
                head.SetActive(false);
                mouth.SetActive(false);
                axes.SetActive(false);
            }
                    
        }

        /// <summary>
        /// Raises the disable event.
        /// </summary>
        void OnDisable()
        {
            webCamTextureToMatHelper.Dispose();

            if (cascade != null)
                cascade.Dispose();
        }

        /// <summary>
        /// Raises the back button event.
        /// </summary>
        public void OnBackButton()
        {
            #if UNITY_5_3 || UNITY_5_3_OR_NEWER
            SceneManager.LoadScene("FaceTrackerExample");
            #else
            Application.LoadLevel("FaceTrackerExample");
            #endif
        }

        /// <summary>
        /// Raises the play button event.
        /// </summary>
        public void OnPlayButton()
        {
            webCamTextureToMatHelper.Play();
        }

        /// <summary>
        /// Raises the pause button event.
        /// </summary>
        public void OnPauseButton()
        {
            webCamTextureToMatHelper.Pause();
        }

        /// <summary>
        /// Raises the stop button event.
        /// </summary>
        public void OnStopButton()
        {
            webCamTextureToMatHelper.Stop();
        }

        /// <summary>
        /// Raises the change camera button event.
        /// </summary>
        public void OnChangeCameraButton()
        {
            webCamTextureToMatHelper.Initialize(null, webCamTextureToMatHelper.requestedWidth, webCamTextureToMatHelper.requestedHeight, !webCamTextureToMatHelper.requestedIsFrontFacing);
        }

        /// <summary>
        /// Raises the is showing face points toggle event.
        /// </summary>
        public void OnIsShowingFacePointsToggle()
        {
            if (isShowingFacePointsToggle.isOn)
            {
                isShowingFacePoints = true;
            } else
            {
                isShowingFacePoints = false;
            }
        }

        /// <summary>
        /// Raises the is showing axes toggle event.
        /// </summary>
        public void OnIsShowingAxesToggle()
        {
            if (isShowingAxesToggle.isOn)
            {
                isShowingAxes = true;
            } else
            {
                isShowingAxes = false;
                axes.SetActive(false);
            }
        }

        /// <summary>
        /// Raises the is showing head toggle event.
        /// </summary>
        public void OnIsShowingHeadToggle()
        {
            if (isShowingHeadToggle.isOn)
            {
                isShowingHead = true;
            } else
            {
                isShowingHead = false;
                head.SetActive(false);
            }
        }

        /// <summary>
        /// Raises the is showin effects toggle event.
        /// </summary>
        public void OnIsShowingEffectsToggle()
        {
            if (isShowingEffectsToggle.isOn)
            {
                isShowingEffects = true;
            } else
            {
                isShowingEffects = false;
                rightEye.SetActive(false);
                leftEye.SetActive(false);
                mouth.SetActive(false);
            }
        }

        /// <summary>
        /// Raises the change auto reset mode toggle event.
        /// </summary>
        public void OnIsAutoResetModeToggle()
        {
            if (isAutoResetModeToggle.isOn)
            {
                isAutoResetMode = true;
            } else
            {
                isAutoResetMode = false;
            }
        }

    }
}