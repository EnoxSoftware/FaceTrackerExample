using UnityEngine;
using System.Collections;
using OpenCVForUnity;
using System.Collections.Generic;
using MiniJSON;

#if UNITY_WSA
using UnityEngine.Windows;
using System.Text;
#else
using System.IO;
#endif

namespace OpenCVFaceTracker
{

/// <summary>
/// Face tracker.
/// Code is the rewrite of https://github.com/MasteringOpenCV/code/tree/master/Chapter6_NonRigidFaceTracking using the “OpenCV for Unity”. 
/// </summary>
    public class FaceTracker
    {

        List<Point[]> points;                  //current tracked points
        FaceDetector detector;           //detector for initialisation
        ShapeModel smodel;               //shape model
        PatchModels pmodel;              //feature detectors
    
        public FaceTracker(string filepath)
        {
            if (filepath == null)
            {
                Debug.LogError("tracker_model file is not loaded.Please copy from “FaceTrackerSample/StreamingAssets/” to “Assets/StreamingAssets/” folder. ");
            }

            points = new List<Point[]>();

            string jsonText = null;


#if UNITY_WSA
                var data = File.ReadAllBytes(filepath);
                jsonText = Encoding.UTF8.GetString(data, 0, data.Length);
#else
            jsonText = File.ReadAllText(filepath);
#endif



//              TextAsset textAsset = Resources.Load (filename) as TextAsset;
//              string jsonText = textAsset.text;
        
            IDictionary json = (IDictionary)Json.Deserialize(jsonText);

            IDictionary ft = (IDictionary)json ["ft object"];

            detector = new FaceDetector();
            detector.read(ft ["detector"]);

            smodel = new ShapeModel();
            smodel.read(ft ["smodel"]);

            pmodel = new PatchModels();
            pmodel.read(ft ["pmodel"]);
        }

        public List<Point[]> getPoints()
        {
            return points;
        }

        public void addPoints(List<Point[]> points)
        {
            points.AddRange(points);
        }

        public void addPoints(MatOfRect rects)
        {
            points.AddRange(detector.convertMatOfRectToPoints(rects));

        }

        public Point[] getConnections()
        {

            Point[] c = new Point[smodel.C.rows()];
            int[] data = new int[c.Length * 2];
            Utils.copyFromMat<int>(smodel.C, data);

            int len = c.Length;
            for (int i = 0; i < len; i++)
            {
                c [i] = new Point(data [i * 2], data [(i * 2) + 1]);
            }

            return c;
        }

        public void reset()
        {
            //reset tracker 
            points.Clear();
        }

        public bool track(Mat im, FaceTrackerParams p)
        {
            if (points.Count <= 0)
                return false;


            //convert image to greyscale
            Mat gray = null;
            if (im.channels() == 1)
            {
                gray = im;
            } else
            {
                gray = new Mat();
                Imgproc.cvtColor(im, gray, Imgproc.COLOR_RGBA2GRAY);
            }

            //initialise
//              if (!tracking)
//                      points = detector.detect (gray, p.scaleFactor, p.minNeighbours, p.minSize);
            int count = points.Count;
            for (int i = 0; i < count; i++)
            {
                if (points [i].Length != smodel.npts())
                    return false;
            
                //fit
                int size_count = p.ssize.Count;
                for (int level = 0; level < size_count; level++)
                {
                    points [i] = fit(gray, points [i], p.ssize [level], p.robust, p.itol, p.ftol);
                }
            }

            return true;
        }

        public void draw(Mat im, Scalar  pts_color, Scalar con_color)
        {

            int[] smodel_C_int = new int[smodel.C.total()];
            Utils.copyFromMat<int>(smodel.C, smodel_C_int);

            foreach (var point in points)
            {
                int n = point.Length;
                if (n == 0)
                    return;

                int rows = smodel.C.rows();
                int cols = smodel.C.cols();
                for (int i = 0; i < rows; i++)
                {
                    int j = smodel_C_int [i * cols], k = smodel_C_int [(i * cols) + 1];
#if OPENCV_2
                Core.line(im, point[j], point[k], con_color, 1);
#else
                    Imgproc.line(im, point [j], point [k], con_color, 1);
#endif
                                
                }
                for (int i = 0; i < n; i++)
                {
#if OPENCV_2
                Core.circle (im, point [i], 1, pts_color, 2, Core.LINE_AA, 0);
#else
                    Imgproc.circle(im, point [i], 1, pts_color, 2, Core.LINE_AA, 0);
#endif
                }
            }
        }

        private Point[] fit(Mat image,
                Point[] init,
                OpenCVForUnity.Size ssize,
                bool robust,
                int itol,
                double ftol)
        {
            int n = smodel.npts(); 
//      assert((int(init.size())==n) && (pmodel.n_patches()==n));
//              Debug.Log ("init.size())==n " + init.Length + " " + n);
//              Debug.Log ("pmodel.n_patches()==n " + pmodel.n_patches () + " " + n);
            smodel.calc_params(init, new Mat(), 3.0f);
            Point[] pts = smodel.calc_shape();

            //find facial features in image around current estimates
            Point[] peaks = pmodel.calc_peaks(image, pts, ssize);

            //optimise
            if (!robust)
            {
                smodel.calc_params(peaks, new Mat(), 3.0f); //compute shape model parameters        
                pts = smodel.calc_shape(); //update shape
            } else
            {
                using (Mat weight = new Mat (n, 1, CvType.CV_32F))
                using (Mat weight_sort = new Mat (n, 1, CvType.CV_32F))
                {

                    float[] weight_float = new float[weight.total()];
                    Utils.copyFromMat<float>(weight, weight_float);
                    float[] weight_sort_float = new float[weight_sort.total()];

                    Point[] pts_old = pts;
                    for (int iter = 0; iter < itol; iter++)
                    {
                        //compute robust weight
                        for (int i = 0; i < n; i++)
                        {
                            using (MatOfPoint tmpMat = new MatOfPoint (new Point (pts [i].x - peaks [i].x, pts [i].y - peaks [i].y)))
                            {
                                weight_float [i] = (float)Core.norm(tmpMat);
                            }
                        }
                        Utils.copyToMat(weight_float, weight);

                        Core.sort(weight, weight_sort, Core.SORT_EVERY_COLUMN | Core.SORT_ASCENDING);


                        Utils.copyFromMat<float>(weight_sort, weight_sort_float);
                        double var = 1.4826 * weight_sort_float [n / 2];


                        if (var < 0.1)
                            var = 0.1;

                        Core.pow(weight, 2, weight);


                        Core.multiply(weight, new Scalar(-0.5 / (var * var)), weight);

                        Core.exp(weight, weight);
                        
                        //compute shape model parameters    
                        smodel.calc_params(peaks, weight, 3.0f);

                        
                        //update shape
                        pts = smodel.calc_shape();
                        
                        //check for convergence
                        float v = 0;
                        for (int i = 0; i < n; i++)
                        {
                            using (MatOfPoint tmpMat = new MatOfPoint (new Point (pts [i].x - pts_old [i].x, pts [i].y - pts_old [i].y)))
                            {
                                v += (float)Core.norm(tmpMat);
                            }
                        }
                        if (v < ftol)
                        {
                            break;
                        } else
                        {
                            pts_old = pts;
                        }
                    }
                }
            }
            return pts;
        }
    }
}
