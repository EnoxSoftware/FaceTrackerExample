using UnityEngine;
using System.Collections;

using System.Collections.Generic;

using System;

using OpenCVForUnity;

namespace OpenCVFaceTracker
{

/// <summary>
/// Patch models.
/// Code is the rewrite of https://github.com/MasteringOpenCV/code/tree/master/Chapter6_NonRigidFaceTracking using the “OpenCV for Unity”.
/// </summary>
    public class PatchModels
    {

        public Mat reference;                           //reference shape
        public IList<PatchModel> patches;             //patch models

        public PatchModels()
        {

        }
    

        //number of patches
        public int n_patches()
        {
            return patches.Count;
        }

        public Point[] calc_peaks(Mat im,
                       Point[] points,
                       OpenCVForUnity.Size ssize)
        {
            int n = points.Length;
//              Debug.Log ("n == int(patches.size()) " + patches.Count);
            using (Mat pt = (new MatOfPoint2f (points)).reshape (1, 2 * n))
            using (Mat S = calc_simil (pt))
            using (Mat Si = inv_simil (S))
            {

                float[] pt_float = new float[pt.total()];
                Utils.copyFromMat<float>(pt, pt_float);
                int pt_cols = pt.cols();
                
                float[] S_float = new float[S.total()];
                Utils.copyFromMat<float>(S, S_float);
                int S_cols = S.cols();
                
                float[] A_float = new float[2 * 3];

                Point[] pts = apply_simil(Si, points);

                for (int i = 0; i < n; i++)
                {

                    OpenCVForUnity.Size wsize = new OpenCVForUnity.Size(ssize.width + patches [i].patch_size().width, ssize.height + patches [i].patch_size().height);
                    using (Mat A = new Mat (2, 3, CvType.CV_32F))
                    {

                        Utils.copyFromMat<float>(A, A_float);
                        int A_cols = A.cols();

                        A_float [0] = S_float [0];
                        A_float [1] = S_float [1];
                        A_float [1 * A_cols] = S_float [1 * S_cols];
                        A_float [(1 * A_cols) + 1] = S_float [(1 * S_cols) + 1];
                        A_float [2] = (float)(pt_float [(2 * pt_cols) * i] -
                            (A_float [0] * (wsize.width - 1) / 2 + A_float [1] * (wsize.height - 1) / 2));
                        A_float [(1 * A_cols) + 2] = (float)(pt_float [((2 * pt_cols) * i) + 1] -
                            (A_float [1 * A_cols] * (wsize.width - 1) / 2 + A_float [(1 * A_cols) + 1] * (wsize.height - 1) / 2));
                        
                        Utils.copyToMat(A_float, A);

                        using (Mat I = new Mat ())
                        {
                            Imgproc.warpAffine(im, I, A, wsize, Imgproc.INTER_LINEAR + Imgproc.WARP_INVERSE_MAP);
                            using (Mat R = patches [i].calc_response (I, false))
                            {
            
                                Core.MinMaxLocResult minMaxLocResult = Core.minMaxLoc(R);
                                pts [i].x = pts [i].x + minMaxLocResult.maxLoc.x - 0.5 * ssize.width;
                                pts [i].y = pts [i].y + minMaxLocResult.maxLoc.y - 0.5 * ssize.height;
                            }
                        }
                    }

                }
                return apply_simil(S, pts);
            }
        }

        Point[] apply_simil(Mat S, Point[] points)
        {

            float[] S_float = new float[S.total()];
            Utils.copyFromMat<float>(S, S_float);
            int S_cols = S.cols();

            int n = points.Length;
            Point[] p = new Point[n];
            for (int i = 0; i < n; i++)
            {
                p [i] = new Point();
                p [i].x = S_float [0] * points [i].x + S_float [1] * points [i].y + S_float [2];
                p [i].y = S_float [1 * S_cols] * points [i].x + S_float [(1 * S_cols) + 1] * points [i].y + S_float [(1 * S_cols) + 2];
            }
            return p;
        }

        Mat inv_simil(Mat S)
        {

            float[] S_float = new float[S.total()];
            Utils.copyFromMat<float>(S, S_float);
            int S_cols = S.cols();

            Mat Si = new Mat(2, 3, CvType.CV_32F);
            float d = S_float [0] * S_float [(1 * S_cols) + 1] - S_float [1 * S_cols] * S_float [1];

            float[] Si_float = new float[Si.total()];
            Utils.copyFromMat<float>(Si, Si_float);
            int Si_cols = Si.cols();
            
            Si_float [0] = S_float [(1 * S_cols) + 1] / d;
            Si_float [1] = -S_float [1] / d;
            Si_float [(1 * Si_cols) + 1] = S_float [0] / d;
            Si_float [1 * Si_cols] = -S_float [1 * S_cols] / d;
            
            Utils.copyToMat(Si_float, Si);

            Mat Ri = new Mat(Si, new OpenCVForUnity.Rect(0, 0, 2, 2));


            Mat negaRi = new Mat();
            Core.multiply(Ri, new Scalar(-1), negaRi);
            Mat t = new Mat();
            Core.gemm(negaRi, S.col(2), 1, new Mat(negaRi.rows(), negaRi.cols(), negaRi.type()), 0, t);

            Mat St = Si.col(2);
            t.copyTo(St);

            return Si;
        }

        Mat calc_simil(Mat pts)
        {
            float[] pts_float = new float[pts.total()];
            Utils.copyFromMat<float>(pts, pts_float);
            int pts_cols = pts.cols();

            //compute translation
            int n = pts.rows() / 2;
            float mx = 0, my = 0;
            for (int i = 0; i < n; i++)
            {
                mx += pts_float [(2 * pts_cols) * i];
                my += pts_float [((2 * pts_cols) * i) + 1];
            }
            using (Mat p = new Mat (2 * n, 1, CvType.CV_32F))
            {

                float[] p_float = new float[p.total()];
                Utils.copyFromMat<float>(p, p_float);
                int p_cols = p.cols();

                mx /= n;
                my /= n;
                for (int i = 0; i < n; i++)
                {
                    p_float [(2 * p_cols) * i] = pts_float [(2 * pts_cols) * i] - mx;
                    p_float [((2 * p_cols) * i) + 1] = pts_float [((2 * pts_cols) * i) + 1] - my;
                }
                Utils.copyToMat(p_float, p);

                //compute rotation and scale
                float[] reference_float = new float[reference.total()];
                Utils.copyFromMat<float>(reference, reference_float);
                int reference_cols = reference.cols();

                float a = 0, b = 0, c = 0;
                for (int i = 0; i < n; i++)
                {
                    a += reference_float [(2 * reference_cols) * i] * reference_float [(2 * reference_cols) * i] +
                        reference_float [((2 * reference_cols) * i) + 1] * reference_float [((2 * reference_cols) * i) + 1];
                    b += reference_float [(2 * reference_cols) * i] * p_float [(2 * p_cols) * i] +
                        reference_float [((2 * reference_cols) * i) + 1] * p_float [((2 * p_cols) * i) + 1];
                    c += reference_float [(2 * reference_cols) * i] * p_float [((2 * p_cols) * i) + 1] -
                        reference_float [((2 * reference_cols) * i) + 1] * p_float [(2 * p_cols) * i];
                }
                b /= a;
                c /= a;
                float scale = (float)Math.Sqrt(b * b + c * c), theta = (float)Math.Atan2(c, b); 
                float sc = scale * (float)Math.Cos(theta), ss = scale * (float)Math.Sin(theta);
                
                Mat returnMat = new Mat(2, 3, CvType.CV_32F);
                returnMat.put(0, 0, sc, -ss, mx, ss, sc, my);

                return returnMat;
            }
    
        }

        public void read(object root_json)
        {
            IDictionary pmodels_json = (IDictionary)root_json;
        
            IDictionary reference_json = (IDictionary)pmodels_json ["reference"];
        
            reference = new Mat((int)(long)reference_json ["rows"], (int)(long)reference_json ["cols"], CvType.CV_32F);
//              Debug.Log ("reference " + reference.ToString ());
        
            IList data_json = (IList)reference_json ["data"];
            float[] data = new float[reference.rows() * reference.cols()];
            int count = data_json.Count;
            for (int i = 0; i < count; i++)
            {
                data [i] = (float)(double)data_json [i];
            }
            Utils.copyToMat(data, reference);
//              Debug.Log ("reference dump " + reference.dump ());
        
        
            int n = (int)(long)pmodels_json ["n_patches"];
            patches = new List<PatchModel>(n);
        
            for (int i = 0; i < n; i++)
            {
                PatchModel patchModel = new PatchModel();
                patchModel.read(pmodels_json ["patch " + i]);
            
                patches.Add(patchModel);
            }
        }
    }
}

