using UnityEngine;
using System.Collections;

using System.Collections.Generic;

using OpenCVForUnity;

namespace OpenCVFaceTracker
{

/// <summary>
/// Face tracker parameters.
/// Code is the rewrite of https://github.com/MasteringOpenCV/code/tree/master/Chapter6_NonRigidFaceTracking using the “OpenCV for Unity”.
/// </summary>
    public class FaceTrackerParams
    {

        public List<OpenCVForUnity.Size> ssize;                      //search region size/level
        public bool robust;                             //use robust fitting?
        public int itol;                                //maximum number of iterations to try
        public double ftol;                              //convergence tolerance
//      public float scaleFactor;                       //OpenCV Cascade detector parameters
//      public int minNeighbours;                       //...
//      public OpenCVForUnity.Size minSize;

        public FaceTrackerParams(bool robust = false, int itol = 20, double ftol = 1e-3, List<OpenCVForUnity.Size> ssize = null)
        {
            if (ssize == null)
            {
                this.ssize = new List<Size>();
                this.ssize.Add(new OpenCVForUnity.Size(21, 21));
                this.ssize.Add(new OpenCVForUnity.Size(11, 11));
                this.ssize.Add(new OpenCVForUnity.Size(5, 5));
            } else
            {
                this.ssize = ssize;
            }

            this.robust = robust;
            this.itol = itol;
            this.ftol = ftol;
//              scaleFactor = 1.1f;
//              minNeighbours = 2;
//              minSize = new OpenCVForUnity.Size (30, 30);
        }
    }
}

