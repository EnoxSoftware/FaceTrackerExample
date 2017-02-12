using UnityEngine;
using System.Collections;

using OpenCVForUnity;

namespace OpenCVFaceTracker
{

/// <summary>
/// Patch model.
/// Code is the rewrite of https://github.com/MasteringOpenCV/code/tree/master/Chapter6_NonRigidFaceTracking using the “OpenCV for Unity”.
/// </summary>
    public class PatchModel
    {

        Mat P;                                   //normalised patch

        public PatchModel()
        {

        }

        //size of patch model
        public Size patch_size()
        {
            return P.size();
        }

        Mat convert_image(Mat im)
        {
            Mat I = null; 
            if (im.channels() == 1)
            {
                if (im.type() != CvType.CV_32F)
                {
                    I = new Mat();
                    im.convertTo(I, CvType.CV_32F); 
                } else
                {
                    I = im;
                }
            } else
            {
                if (im.channels() == 3)
                {
                    Mat img = new Mat();
                    Imgproc.cvtColor(im, img, Imgproc.COLOR_RGBA2GRAY);
                    if (img.type() != CvType.CV_32F)
                    {
                        I = new Mat();
                        img.convertTo(I, CvType.CV_32F); 
                    } else
                    {
                        I = img;
                    }
                } else
                {
                    Debug.Log("Unsupported image type!");
                }
            }
            Core.add(I, new Scalar(1.0), I);
            Core.log(I, I);
            return I;
        }

        public Mat calc_response(Mat im, bool sum2one)
        {
            Mat I = convert_image(im);
            Mat res = new Mat();

            Imgproc.matchTemplate(I, P, res, Imgproc.TM_CCOEFF_NORMED);
            if (sum2one)
            {
                Core.normalize(res, res, 0, 1, Core.NORM_MINMAX);

                Core.divide(res, new Scalar(Core.sumElems(res).val [0]), res);
            }
            return res;
        }

        public void read(object root_json)
        {
            IDictionary pmodel_json = (IDictionary)root_json;
        
            IDictionary P_json = (IDictionary)pmodel_json ["P"];
            P = new Mat((int)(long)P_json ["rows"], (int)(long)P_json ["cols"], CvType.CV_32F);
//              Debug.Log ("P " + P.ToString ());
        
            IList P_data_json = (IList)P_json ["data"];
            float[] P_data = new float[P.rows() * P.cols()];
            for (int i = 0; i < P_data_json.Count; i++)
            {
                P_data [i] = (float)(double)P_data_json [i];
            }
            Utils.copyToMat(P_data, P);
//              Debug.Log ("P dump " + P.dump ());
        
        
        
        }
    }
}

