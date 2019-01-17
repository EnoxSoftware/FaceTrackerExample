using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using OpenCVForUnity.CoreModule;

namespace OpenCVFaceTracker
{

    /// <summary>
    /// Face tracker parameters.
    /// Code is the rewrite of https://github.com/MasteringOpenCV/code/tree/master/Chapter6_NonRigidFaceTracking using the “OpenCV for Unity”.
    /// </summary>
    public class FaceTrackerParams
    {

        public List<Size> ssize;
        //search region size/level
        public bool robust;
        //use robust fitting?
        public int itol;
        //maximum number of iterations to try
        public double ftol;
        //convergence tolerance
        //      public float scaleFactor;                       //OpenCV Cascade detector parameters
        //      public int minNeighbours;                       //...
        //      public Size minSize;

        public FaceTrackerParams (bool robust = false, int itol = 20, double ftol = 1e-3, List<Size> ssize = null)
        {
            if (ssize == null) {
                this.ssize = new List<Size> ();
                this.ssize.Add (new Size (21, 21));
                this.ssize.Add (new Size (11, 11));
                this.ssize.Add (new Size (5, 5));
            } else {
                this.ssize = ssize;
            }

            this.robust = robust;
            this.itol = itol;
            this.ftol = ftol;
//              scaleFactor = 1.1f;
//              minNeighbours = 2;
//              minSize = new Size (30, 30);
        }
    }
}

