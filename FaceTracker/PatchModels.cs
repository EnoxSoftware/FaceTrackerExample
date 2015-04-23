using UnityEngine;
using System.Collections;

using System.Collections.Generic;

using System;

using OpenCVForUnity;

/// <summary>
/// Patch models.
/// Code is the rewrite of https://github.com/MasteringOpenCV/code/tree/master/Chapter6_NonRigidFaceTracking using the “OpenCV for Unity”.
/// </summary>
public class PatchModels
{

		public Mat reference;                           //reference shape
		public IList<PatchModel> patches;             //patch models

		public PatchModels ()
		{

		}
	

		//number of patches
		public int n_patches ()
		{
				return patches.Count;
		}

		public Point[] calc_peaks (Mat im,
			           Point[] points,
			           OpenCVForUnity.Size ssize)
		{
				int n = points.Length;
//				Debug.Log ("n == int(patches.size()) " + patches.Count);
				using (Mat pt = (new MatOfPoint2f (points)).reshape (1, 2 * n))
				using (Mat S = calc_simil (pt))
				using (Mat Si = inv_simil (S)) {
						Point[] pts = apply_simil (Si, points);

						for (int i = 0; i < n; i++) {

								OpenCVForUnity.Size wsize = new OpenCVForUnity.Size (ssize.width + patches [i].patch_size ().width, ssize.height + patches [i].patch_size ().height);
								using (Mat A = new Mat (2, 3, CvType.CV_32F)) {
										A.put (0, 0, S.get (0, 0) [0]);
										A.put (0, 1, S.get (0, 1) [0]);
										A.put (1, 0, S.get (1, 0) [0]);
										A.put (1, 1, S.get (1, 1) [0]);
										A.put (0, 2, pt.get (2 * i, 0) [0] - 
												(A.get (0, 0) [0] * (wsize.width - 1) / 2 + A.get (0, 1) [0] * (wsize.height - 1) / 2));
										A.put (1, 2, pt.get (2 * i + 1, 0) [0] - 
												(A.get (1, 0) [0] * (wsize.width - 1) / 2 + A.get (1, 1) [0] * (wsize.height - 1) / 2));
										using (Mat I = new Mat ()) {
												Imgproc.warpAffine (im, I, A, wsize, Imgproc.INTER_LINEAR + Imgproc.WARP_INVERSE_MAP);
												using (Mat R = patches [i].calc_response (I, false)) {
			
														Core.MinMaxLocResult minMaxLocResult = Core.minMaxLoc (R);
														pts [i].x = pts [i].x + minMaxLocResult.maxLoc.x - 0.5 * ssize.width;
														pts [i].y = pts [i].y + minMaxLocResult.maxLoc.y - 0.5 * ssize.height;
												}
										}
								}

						}

						return apply_simil (S, pts);
				}
		}

		Point[] apply_simil (Mat S, Point[] points)
		{
				int n = points.Length;
				Point[] p = new Point[n];
				for (int i = 0; i < n; i++) {
						p [i] = new Point ();
						p [i].x = S.get (0, 0) [0] * points [i].x + S.get (0, 1) [0] * points [i].y + S.get (0, 2) [0];
						p [i].y = S.get (1, 0) [0] * points [i].x + S.get (1, 1) [0] * points [i].y + S.get (1, 2) [0];
				}
				return p;
		}

		Mat inv_simil (Mat S)
		{
				Mat Si = new Mat (2, 3, CvType.CV_32F);
				float d = (float)S.get (0, 0) [0] * (float)S.get (1, 1) [0] - (float)S.get (1, 0) [0] * (float)S.get (0, 1) [0];

				Si.put (0, 0, S.get (1, 1) [0] / d);
				Si.put (0, 1, -S.get (0, 1) [0] / d);
				Si.put (1, 1, S.get (0, 0) [0] / d);
				Si.put (1, 0, -S.get (1, 0) [0] / d);

				Mat Ri = new Mat (Si, new OpenCVForUnity.Rect (0, 0, 2, 2));


				Mat negaRi = new Mat ();
				Core.multiply (Ri, new Scalar (-1), negaRi);
				Mat t = new Mat ();
				Core.gemm (negaRi, S.col (2), 1, new Mat (negaRi.rows (), negaRi.cols (), negaRi.type ()), 0, t);

				Mat St = Si.col (2);
				t.copyTo (St);

				return Si;
		}

		Mat calc_simil (Mat pts)
		{
				//compute translation
				int n = pts.rows () / 2;
				float mx = 0, my = 0;
				for (int i = 0; i < n; i++) {
						mx += (float)pts.get (2 * i, 0) [0];
						my += (float)pts.get (2 * i + 1, 0) [0];
				}
				using (Mat p = new Mat (2 * n, 1, CvType.CV_32F)) {
						mx /= n;
						my /= n;
						for (int i = 0; i < n; i++) {
								p.put (2 * i, 0, pts.get (2 * i, 0) [0] - mx);
								p.put (2 * i + 1, 0, pts.get (2 * i + 1, 0) [0] - my);
						}
						//compute rotation and scale
						float a = 0, b = 0, c = 0;
						for (int i = 0; i < n; i++) {
								a += (float)reference.get (2 * i, 0) [0] * (float)reference.get (2 * i, 0) [0] + 
										(float)reference.get (2 * i + 1, 0) [0] * (float)reference.get (2 * i + 1, 0) [0];
								b += (float)reference.get (2 * i, 0) [0] * (float)p.get (2 * i, 0) [0] + (float)reference.get (2 * i + 1, 0) [0] * (float)p.get (2 * i + 1, 0) [0];
								c += (float)reference.get (2 * i, 0) [0] * (float)p.get (2 * i + 1, 0) [0] - (float)reference.get (2 * i + 1, 0) [0] * (float)p.get (2 * i, 0) [0];
						}
						b /= a;
						c /= a;
						float scale = (float)Math.Sqrt (b * b + c * c), theta = (float)Math.Atan2 (c, b); 
						float sc = scale * (float)Math.Cos (theta), ss = scale * (float)Math.Sin (theta);
				
						Mat returnMat = new Mat (2, 3, CvType.CV_32F);
						returnMat.put (0, 0, sc, -ss, mx, ss, sc, my);

						return returnMat;
				}
	
		}

		public void read (object root_json)
		{
				IDictionary pmodels_json = (IDictionary)root_json;
		
				IDictionary reference_json = (IDictionary)pmodels_json ["reference"];
		
				reference = new Mat ((int)(long)reference_json ["rows"], (int)(long)reference_json ["cols"], CvType.CV_32F);
//				Debug.Log ("reference " + reference.ToString ());
		
				IList data_json = (IList)reference_json ["data"];
				float[] data = new float[reference.rows () * reference.cols ()];
				for (int i = 0; i < data_json.Count; i++) {
						data [i] = (float)(double)data_json [i];
				}
				reference.put (0, 0, data);
//				Debug.Log ("reference dump " + reference.dump ());
		
		
				int n = (int)(long)pmodels_json ["n_patches"];
				patches = new List<PatchModel> (n);
		
				for (int i = 0; i < n; i++) {
						PatchModel patchModel = new PatchModel ();
						patchModel.read (pmodels_json ["patch " + i]);
			
						patches.Add (patchModel);
				}
		}
}

