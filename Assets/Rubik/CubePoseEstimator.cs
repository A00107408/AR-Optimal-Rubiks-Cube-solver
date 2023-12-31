﻿/* <summary
 * 
 * Augmented Reality Rubik Cube Application
 * A.I.T 2018
 * A00107408
 * Masters by Research
 * 
 * File Description:
 * Cube location and orientation in GL space coordinates are reconstructed from Face information.
 * The Rubik Cube is defined as a cube centered at the origin with edge length of 2.0 units.
 * 
 * Accuracy
 *   Despite code seeming to be correct, we still observe some perceived error in the cube 
 *   overlay rending to actual rubik cube.   Six parameters (3 rotation and 3 translation) 
 *   fudge factors allows offsets to be applied to Pose Estimator solution.  Possible
 *   root causes of this error are:
 *   o  Intrinsic camera calibration parameters not accurate: these are being obtained from Android.
 *   o  Assumption that camera distortion can be ignored.
 *   o  Center of tile calculations: perhaps use first-order momentum on curve.
 *   o  Camera Perspective and/or Rendering Perspective not in agreement.
 * 
 * Acknowledgments:
 * Original Author: Steven P. Punte (aka Android Steve : android.steve@cl-sw.com)
 * Date: April 25th 2015
 *   
 * <summary> */

 namespace Rubik
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using OpenCvSharp;

    using FaceRecognitionStatusEnum = RubikFace.FaceRecognitionStatusEnum;

    /// <summary>
    /// Class Cube Reconstructor
    /// 
    ///   Cube location and orientation in GL space coordinates are reconstructed from Face information.
    ///   The Rubik Cube is defined as a cube centered a the origin with edge length of 2.0 units.
    /// 
    /// original @author android.steve@cl-sw.com
    /// 
    /// </summary>
    public class CubePoseEstimator
    {

        private static Mat cameraMatrix;
        private static MatOfDouble distCoeffs;

        /// <summary>
        /// Pose Estimation
        /// 
        /// Deduce real world cube coordinates and rotation
        /// </summary>
        /// <param name="rubikFace"> </param>
        /// <param name="image"> </param>
        /// <param name="stateModel"> </param>
        /// <returns>  </returns> 
        public static CubePose PoseEstimation(RubikFace rubikFace, Mat image, StateModel stateModel)
        {
            if (rubikFace == null)
            {
                return null;
            }

            if (rubikFace.faceRecognitionStatus != FaceRecognitionStatusEnum.SOLVED)
            {
                return null;
            }

            LeastMeansSquare lmsResult = rubikFace.lmsResult;

            if (lmsResult == null)
            {
                return null;
            }

            // OpenCV Pose Estimate requires at least four points.
            if (rubikFace.rhombusList.Count <= 4)
            {
                return null;
            }

            //            if (cameraMatrix == null)
            //            {
            //                cameraMatrix = stateModel.cameraCalibration.getOpenCVCameraMatrix((int)(image.Size().Width), (int)(image.Size().Height));
            //                distCoeffs = new MatOfDouble(stateModel.cameraCalibration.DistortionCoefficients);
            //            }

            /*
             * For the purposes of external camera calibration: i.e., where the cube is 
             * located in camera coordinates, we define the geometry of the face of a
             * cube composed of nine 3D locations each representing the center of each tile.
             * Correspondence between these points and nine 2D points from the actual
             * camera image, along with camera calibration data, are using to calculate
             * the Pose of the Cube (i.e. "Cube Pose").
             * 
             * The geometry of the cube here is defined as having center at {0,0,0},
             * and edge size of 2 units (i.e., +/- 1.0).
             */

            // List of real world point and screen points that correspond.
            IList<Point3d> objectPointsList = new List<Point3d>(9);
            IList<Point> imagePointsList = new List<Point>(9);


            // Create list of image (in 2D) and object (in 3D) points.
            // Loop over Rubik Face Tiles
            for (int n = 0; n < 3; n++)
            {
                for (int m = 0; m < 3; m++)
                {

                    Rhombus rhombus = rubikFace.faceRhombusArray[n][m];

                    // Only use if Rhombus was non null.
                    if (rhombus != null)
                    {

                        // Obtain center of Rhombus in screen image coordinates
                        // Convention:
                        //  o X is zero on the left, and increases to the right.
                        //  o Y is zero on the top and increases downward.
                        Point imagePoint = new Point(rhombus.center.X, rhombus.center.Y);
                        imagePointsList.Add(imagePoint);

                        // N and M are actual not conceptual (as in design doc).
                        int mm = 2 - n;
                        int nn = 2 - m;
                        // above now matches design doc.
                        // that is:
                        //  o the nn vector is to the right and upwards.
                        //  o the mm vector is to the left and upwards.

                        // Calculate center of Tile in OpenCV World Space Coordinates
                        // Convention:
                        //  o X is zero in the center, and increases to the left.
                        //  o Y is zero in the center and increases downward.
                        //  o Z is zero (at the world coordinate origin) and increase away for the camera.
                        float X = (1 - mm) * 0.66666f;
                        float Y = -1.0f;
                        float Z = -1.0f * (1 - nn) * 0.666666f;
                        Point3d objectPoint = new Point3d(X, Y, Z);
                        objectPointsList.Add(objectPoint);
                    }
                }
            }

            // Cast image point list into OpenCV Matrix.
            MatOfPoint2f imagePoints = new MatOfPoint2f();
            //imagePoints.fromList(imagePointsList);
//            imagePoints.Cast<MatOfPoint2f>(imagePointsList);   //fromList(imagePointsList);

            // Cast object point list into OpenCV Matrix.
            MatOfPoint3f objectPoints = new MatOfPoint3f();
//            objectPoints.fromList(objectPointsList);


            Mat rvec = new Mat();
            Mat tvec = new Mat();


            //		Log.e(Constants.TAG, "Image Points: " + imagePoints.dump());
            //		Log.e(Constants.TAG, "Object Points: " + objectPoints.dump());

            //		=+= sometimes a "count >= 4" exception 
//            Calib3d.solvePnP(objectPoints, imagePoints, cameraMatrix, distCoeffs, rvec, tvec);


            Debug.Log(string.Format("Open CV Rotation Vector x={0,4:F2} y={1,4:F2} z={2,4:F2}", rvec.At<double>(0, 0), rvec.At<double>(1, 0), rvec.At<double>(2, 0)));

            // Convert from OpenCV to OpenGL World Coordinates
            float x = +1.0f * (float)tvec.At<double>(0, 0);
            float y = -1.0f * (float)tvec.At<double>(1, 0);
            float z = -1.0f * (float)tvec.At<double>(2, 0);

            //        // =+= Add manual offset correction to translation  
            //        x += MenuAndParams.xTranslationOffsetParam.value;
            //        y += MenuAndParams.yTranslationOffsetParam.value;
            //        z += MenuAndParams.zTranslationOffsetParam.value;		


            // Convert Rotation Vector from OpenCL polarity axes definition to OpenGL definition
            // Note, polarity of x-axis is OK, no need to invert.
            rvec.Set(1, 0, -1.0f * rvec.At<double>(1, 0)); // y-axis
            rvec.Set(2, 0, -1.0f * rvec.At<double>(2, 0)); // z-axis

            //        // =+= Add manual offset correction to Rotation
            //        rvec.put(0, 0, rvec.get(0, 0)[0] + MenuAndParams.xRotationOffsetParam.value * Math.PI / 180.0);  // X rotation
            //        rvec.put(1, 0, rvec.get(1, 0)[0] + MenuAndParams.yRotationOffsetParam.value * Math.PI / 180.0);  // Y rotation
            //        rvec.put(2, 0, rvec.get(2, 0)[0] + MenuAndParams.zRotationOffsetParam.value * Math.PI / 180.0);  // Z rotation

            // Package up as CubePose object
            CubePose cubePose = new CubePose {
                x = x,
                y = y,
                z = z,
                xRotation = rvec.At<double>(0, 0),
                yRotation = rvec.At<double>(1, 0),
                zRotation = rvec.At<double>(2, 0)
            };

            //		Log.e(Constants.TAG, "Result: " + result);
            //		Log.e(Constants.TAG, "Camera: " + cameraMatrix.dump());
            //		Log.e(Constants.TAG, "Rotation: " + rvec.dump());
            //		Log.e(Constants.TAG, "Translation: " + tvec.dump());

            //		// Reporting in OpenGL World Coordinates
            //		Core.rectangle(image, new Point(0, 50), new Point(1270, 150), Constants.ColorBlack, -1);
            //		Core.putText(image, String.format("Translation  x=%4.2f y=%4.2f z=%4.2f", x, y, z), new Point(50, 100), Constants.FontFace, 3, Constants.ColorWhite, 3);
            //		Core.putText(image, String.format("Rotation     x=%4.0f y=%4.0f z=%4.0f", cubeXrotation, cubeYrotation, cubeZrotation), new Point(50, 150), Constants.FontFace, 3, Constants.ColorWhite, 3);

            Debug.Log("Cube Pose: " + cubePose);

            return cubePose;

        }
    }
}