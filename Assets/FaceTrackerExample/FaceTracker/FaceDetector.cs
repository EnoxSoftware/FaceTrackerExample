using UnityEngine;
using System.Collections;

using System.Collections.Generic;

using OpenCVForUnity;

namespace OpenCVFaceTracker
{

/// <summary>
/// Face detector.
/// Code is the rewrite of https://github.com/MasteringOpenCV/code/tree/master/Chapter6_NonRigidFaceTracking using the “OpenCV for Unity”.
/// </summary>
    public class FaceDetector
    {


        string detector_fname;                   //file containing cascade classifier
        Vector3 detector_offset;                   //offset from center of detection
        Mat reference;                           //reference shape
        CascadeClassifier detector;

        public FaceDetector()
        {

        }

        public List<Point[]> detect(Mat im, float scaleFactor, int minNeighbours, OpenCVForUnity.Size minSize)
        {
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


            using (Mat equalizeHistMat = new Mat ()) 
            using (MatOfRect faces = new MatOfRect ())
            {
            
                Imgproc.equalizeHist(gray, equalizeHistMat);

                detector.detectMultiScale(equalizeHistMat, faces, scaleFactor, minNeighbours, 0
                    | Objdetect.CASCADE_FIND_BIGGEST_OBJECT
                    | Objdetect.CASCADE_SCALE_IMAGE, minSize, new Size());
            
            
                if (faces.rows() < 1)
                {
                    return new List<Point[]>();
                }
                return convertMatOfRectToPoints(faces);
            }
                
        }

        public List<Point[]> convertMatOfRectToPoints(MatOfRect rects)
        {
            List<OpenCVForUnity.Rect> R = rects.toList();
        
            List<Point[]> points = new List<Point[]>(R.Count);
        
            int n = reference.rows() / 2;
            float[] reference_float = new float[reference.total()];
            Utils.copyFromMat<float>(reference, reference_float);

            foreach (var r in R)
            {
            
                Vector3 scale = detector_offset * r.width;
                Point[] p = new Point[n];
                for (int i = 0; i < n; i++)
                {
                    p [i] = new Point();
                    p [i].x = scale.z * reference_float [2 * i] + r.x + 0.5 * r.width + scale.x;
                    p [i].y = scale.z * reference_float [(2 * i) + 1] + r.y + 0.5 * r.height + scale.y;
                }
            
                points.Add(p);
            }

            return points;
        }

        public void read(object root_json)
        {
            IDictionary detector_json = (IDictionary)root_json;

            detector_fname = (string)detector_json ["fname"];
//              Debug.Log ("detector_fname " + detector_fname);
                
        
            detector_offset = new Vector3((float)(double)detector_json ["x offset"], (float)(double)detector_json ["y offset"], (float)(double)detector_json ["z offset"]);
//              Debug.Log ("detector_offset " + detector_offset.ToString ());
        

            IDictionary reference_json = (IDictionary)detector_json ["reference"];

        
            reference = new Mat((int)(long)reference_json ["rows"], (int)(long)reference_json ["cols"], CvType.CV_32F);
//              Debug.Log ("reference " + reference.ToString ());
        
            IList data_json = (IList)reference_json ["data"];
            float[] data = new float[reference.rows() * reference.cols()];
            for (int i = 0; i < data_json.Count; i++)
            {
                data [i] = (float)(double)data_json [i];
            }
            Utils.copyToMat(data, reference);
//              Debug.Log ("reference dump " + reference.dump ());
        
            detector = new CascadeClassifier(Utils.getFilePath(detector_fname));
            //              detector = new CascadeClassifier (System.IO.Path.Combine (Application.streamingAssetsPath, detector_fname));
        }
    }
}

