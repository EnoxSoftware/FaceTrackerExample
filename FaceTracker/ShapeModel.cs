﻿using UnityEngine;
using System.Collections;

using System;

using OpenCVForUnity;

/// <summary>
/// Shape model.
/// Code is the rewrite of https://github.com/MasteringOpenCV/code/tree/master/Chapter6_NonRigidFaceTracking using the “OpenCV for Unity”.
/// </summary>
public class ShapeModel
{

		public Mat p;                                   //parameter vector (kx1) CV_32F
		public Mat V;                                   //shape basis (2nxk) CV_32F
		public Mat e;                                   //parameter variance (kx1) CV_32F
		public Mat C;                                   //connectivity (cx2) CV_32S


		public ShapeModel ()
		{

		}

		public int npts ()
		{
				//number of points in shape model
				return V.rows () / 2;
		}

		public void calc_params (Point[] pts, Mat weight, float c_factor)
		{
				int n = pts.Length;
//		assert(V.rows == 2*n);
//				Debug.Log ("V.rows == 2*n " + V.rows () + " " + 2 * n);

				using (Mat s = (new MatOfPoint2f (pts)).reshape (1, 2 * n)) { //point set to vector format

						if (weight.empty ()) {
								Core.gemm (V.t (), s, 1, new Mat (), 0, p);     //simple projection

						} else {         //scaled projection
								if (weight.rows () != n) {
										Debug.Log ("Invalid weighting matrix");
								}
								int K = V.cols ();
								using (Mat H = Mat.zeros (K, K, CvType.CV_32F))
								using (Mat g = Mat.zeros (K, 1, CvType.CV_32F)) {
										for (int i = 0; i < n; i++) {
												using (Mat v = new Mat (V, new OpenCVForUnity.Rect (0, 2 * i, K, 2)))
												using (Mat tmpMat1 = new Mat ())
												using (Mat tmpMat2 = new Mat ())
												using (Mat tmpMat3 = new Mat ()) {

														float w = (float)weight.get (i, 0) [0];
												
														Core.multiply (v.t (), new Scalar (w), tmpMat1);
												
														Core.gemm (tmpMat1, v, 1, new Mat (), 0, tmpMat2);
														Core.add (H, tmpMat2, H);
												
														Core.gemm (tmpMat1, new MatOfPoint2f (pts [i]).reshape (1, 2), 1, new Mat (), 0, tmpMat3);
														Core.add (g, tmpMat3, g);
												}

										}
								
										Core.solve (H, g, p, Core.DECOMP_SVD);
								}
						}
				}
				clamp (c_factor);          //clamp resulting parameters
		}

		public Point[] calc_shape ()
		{
				using (Mat s = new Mat ()) {
						Core.gemm (V, p, 1, new Mat (), 0, s);
						int n = s.rows () / 2;
						Point[] pts = new Point[n];
						for (int i = 0; i < n; i++) {
								pts [i] = new Point (s.get (2 * i, 0) [0], s.get (2 * i + 1, 0) [0]);
						}
				
						return pts;
				}
		}

		void clamp (float c)
		{
				double scale = p.get (0, 0) [0];
				for (int i = 0; i < e.rows(); i++) {
						if (e.get (i, 0) [0] < 0)
								continue;
						float v = c * (float)Math.Sqrt (e.get (i, 0) [0]);
						if (Math.Abs (p.get (i, 0) [0] / scale) > v) {
								if (p.get (i, 0) [0] > 0) {
										p.put (i, 0, v * scale);
								} else {
										p.put (i, 0, -v * scale);
								}
						}
				}
		}

		public void read (object root_json)
		{
				IDictionary smodel_json = (IDictionary)root_json;
		
				IDictionary V_json = (IDictionary)smodel_json ["V"];
				V = new Mat ((int)(long)V_json ["rows"], (int)(long)V_json ["cols"], CvType.CV_32F);
//				Debug.Log ("V " + V.ToString ());
		
				IList V_data_json = (IList)V_json ["data"];
				float[] V_data = new float[V.rows () * V.cols ()];
				for (int i = 0; i < V_data_json.Count; i++) {
						V_data [i] = (float)(double)V_data_json [i];
				}
				V.put (0, 0, V_data);
//				Debug.Log ("V dump " + V.dump ());
		
		
		
				IDictionary e_json = (IDictionary)smodel_json ["e"];
				e = new Mat ((int)(long)e_json ["rows"], (int)(long)e_json ["cols"], CvType.CV_32F);
//				Debug.Log ("e " + e.ToString ());
		
				IList e_data_json = (IList)e_json ["data"];
				float[] e_data = new float[e.rows () * e.cols ()];
				for (int i = 0; i < e_data_json.Count; i++) {
						e_data [i] = (float)(double)e_data_json [i];
				}
				e.put (0, 0, e_data);
//				Debug.Log ("e dump " + e.dump ());
		
		
		
				IDictionary C_json = (IDictionary)smodel_json ["C"];
				C = new Mat ((int)(long)C_json ["rows"], (int)(long)C_json ["cols"], CvType.CV_32S);
//				Debug.Log ("C " + C.ToString ());
		
				IList C_data_json = (IList)C_json ["data"];
				int[] C_data = new int[C.rows () * C.cols ()];
				for (int i = 0; i < C_data_json.Count; i++) {
						C_data [i] = (int)(long)C_data_json [i];
				}
				C.put (0, 0, C_data);
//				Debug.Log ("C dump " + C.dump ());
				
		
		
				p = Mat.zeros (e.rows (), 1, CvType.CV_32F);
		}
}


