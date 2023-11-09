/*<summary>
 * 
 * Augmented Reality Rubik Cube Application
 * A.I.T 2018
 * A00107408
 * Masters by Research
 * 
 * Acknowledgments:
 * Original Author: Steven P. Punte (aka Android Steve : android.steve@cl-sw.com)
 * Date: April 25th 2015
 * 
 *<summary>*/

namespace Rubik
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using OpenCvSharp;

    using ColorTileEnum = Constants.ColorTileEnum;
    using FaceNameEnum = Constants.FaceNameEnum;

    /// <summary>
    /// original @author android.steve@cl-sw.com
    /// </summary>
    [Serializable]                                     
    public class RubikFace
    {

        // For purposes of serialization
        private const long serialVersionUID = -8498294721543708545L;

        [NonSerialized]
        public List<Rhombus> rhombusList = new List<Rhombus>();

        // A 3x3 matrix of Rhombus elements.  This array will be sorted to achieve
        // final correct position arrangement of available Rhombus objects.  Some elements can be null.
        [NonSerialized]
        public Rhombus[][] faceRhombusArray = RectangularArrays.ReturnRectangularRhombusArray(3, 3);

        // A 3x3 matrix of Logical Tiles.  All elements must be non-null for an appropriate Face solution.
        // The rotation of this array is the output of the Face Recognizer as per the current spatial
        // rotation of the cube.
        public ColorTileEnum[][] observedTileArray = RectangularArrays.ReturnRectangularColorTileEnumArray(3, 3);

        // Record actual RGB colors measured at the center of each tile.
        public Double[][][] measuredColorArray = RectangularArrays.ReturnRectangularDoubleArray(3, 3, 4);

        // A 3x3 matrix of Logical Tiles.  All elements must be non-null for an appropriate Face solution.
        // The rotation of this array has been adjusted so that, in the final cube state, the faces are read
        // and rendered correctly with respect to the "unfolded cube layout convention."
        public ColorTileEnum[][] transformedTileArray = RectangularArrays.ReturnRectangularColorTileEnumArray(3, 3);

        // Angle of Alpha-Axis (N) stored in radians.
        public double alphaAngle = 0.0;

        // Angle of Beta-Axis (M) stored in radians.
        public double betaAngle = 0.0;

        // Length in pixels of Alpha Lattice (i.e. a tile size)
        public double alphaLatticLength = 0.0;

        // Length in pixels of Beta Lattice (i.e. a tile size)
        public double betaLatticLength = 0.0;

        // Ratio of Beta Lattice to Alpha Lattice
        public double gammaRatio = 0.0;

        // Least Means Square Result
        [NonSerialized]
        public LeastMeansSquare lmsResult =
            // Put some dummy data here.
            new LeastMeansSquare(
                            800,    // -  X origin of Rubik Face (i.e. center of tile {0,0})
                            200,    // -  Y origin of Rubik Face (i.e. center of tile {0,0})
                            50,     // -Length of Alpha Lattice
                            null,  
                            314,    // -Sigma Error(i.e.RMS of know Rhombus to Tile centers)
                            true);  // Allow these dummy results to be display even though they are false   

        // Number of rhombus that were moved in order to obtain better LMS fit.
        public int numRhombusMoves = 0;

        // This is a proprietary hash code and NOT that of function hashCode().  This hash code is 
        // intended to be unique and repeatable for any given set of colored tiles in a specified set 
        // of locations on a Rubik Face.  It is used to determine if an identical Rubik Face is being
        // observed multiple times. Note, if a tiles color designation is changed due to a change in 
        // lighting conditions, the calculated hash code will be different. A more robust strategy 
        // would be to require that only 8 or 9 tiles match in order to determine if an 
        // identical face is being presented.
        public int myHashCode = 0;

        // Profiles CPU Consumption
        [NonSerialized]
        public Profiler profiler = new Profiler();

        // Face Designation: i.e., Up, Down, ....
        public FaceNameEnum faceNameEnum;

        // A Rubik Face can exist in the following states:
        public enum FaceRecognitionStatusEnum
        {
            UNKNOWN,
            INSUFFICIENT, // Insufficient Provided Rhombi to attempt solution
            BAD_METRICS, // Metric Calculation did not produce reasonable results
            INCOMPLETE, // Rhombi did not converge to proper solution
            INADEQUATE, // We require at least one Rhombus in each row and column
            BLOCKED, // Attempt to improve Rhombi layout in face was blocked: incorrect move direction reported
            INVALID_MATH, // LMS algorithm result in invalid math.
            UNSTABLE, // Last Tile move resulted in a increase in the overall error (LMS).
            SOLVED
        } // Full and proper solution obtained.
        public FaceRecognitionStatusEnum faceRecognitionStatus = FaceRecognitionStatusEnum.UNKNOWN;

        //----------------------------------------------------------------------------------------
        //	Copyright © 2007 - 2018 Tangible Software Solutions Inc.
        //	This class can be used by anyone provided that the copyright notice remains intact.
        //
        //	This class includes methods to convert Java rectangular arrays (jagged arrays
        //	with inner arrays of the same length).
        //----------------------------------------------------------------------------------------
        internal static class RectangularArrays
        {
            internal static Rhombus[][] ReturnRectangularRhombusArray(int size1, int size2)
            {
                Rhombus[][] newArray = new Rhombus[size1][];
                for (int array1 = 0; array1 < size1; array1++)
                {
                    newArray[array1] = new Rhombus[size2];
                }

                return newArray;
            }

            internal static ColorTileEnum[][] ReturnRectangularColorTileEnumArray(int size1, int size2)
            {
                ColorTileEnum[][] newArray = new ColorTileEnum[size1][];
                for (int array1 = 0; array1 < size1; array1++)
                {
                    newArray[array1] = new ColorTileEnum[size2];
                }

                return newArray;
            }

            internal static double[][][] ReturnRectangularDoubleArray(int size1, int size2, int size3)
            {
                double[][][] newArray = new double[size1][][];
                for (int array1 = 0; array1 < size1; array1++)
                {
                    newArray[array1] = new double[size2][];
                    if (size3 > -1)
                    {
                        for (int array2 = 0; array2 < size2; array2++)
                        {
                            newArray[array1][array2] = new double[size3];
                        }
                    }
                }

                return newArray;
            }

            internal static double[][] ReturnRectangularDoubleArray(int size1, int size2)
            {
                double[][] newArray = new double[size1][];
                for (int array1 = 0; array1 < size1; array1++)
                {
                    newArray[array1] = new double[size2];
                }

                return newArray;
            }

            internal static Point[][] ReturnRectangularPointArray(int size1, int size2)
            {
                Point[][] newArray = new Point[size1][];
                for (int array1 = 0; array1 < size1; array1++)
                {
                    newArray[array1] = new Point[size2];
                }

                return newArray;
            }
        }

        /// <summary>
        /// Rubik Face Constructor
        /// </summary>
        public RubikFace()
        {
            // Dummy data
            alphaAngle = 45.0 * Math.PI / 180.0;
            betaAngle = 135.0 * Math.PI / 180.0;
            alphaLatticLength = 50.0;
            betaLatticLength = 50.0;
        }

        /// <summary>
        /// Process Rhombuses
        /// 
        /// Given the Rhombus list, attempt to recognize the grid dimensions and orientation,
        /// and full tile color set.
        /// </summary>
        /// <param name="rhombusList"> </param>
        /// <param name="image">  </param>
        public virtual void ProcessRhombuses(List<Rhombus> rhombusList, Mat image)
        {
            this.rhombusList = rhombusList;

            // Don't even attempt if less than three rhombus are identified.
            if (rhombusList.Count < 3)
            {
                faceRecognitionStatus = FaceRecognitionStatusEnum.INSUFFICIENT;
                return;
            }

            // Calculate average alpha and beta angles, and also gamma ratio.
            // Sometimes (but probably only when certain bugs exist) can contain NaN data.
            if (CalculateMetrics() == false)
            {
                faceRecognitionStatus = FaceRecognitionStatusEnum.BAD_METRICS;
                return;
            }

            // Layout Rhombi into Face Array
            if (TileLayoutAlgorithm.DoInitialLayout(rhombusList, faceRhombusArray, alphaAngle, betaAngle) == false)
            {
                faceRecognitionStatus = FaceRecognitionStatusEnum.INADEQUATE;
                return;
            }
         
            lmsResult = FindOptimumFaceFit();
            
            if (lmsResult.valid == false)
            {
                faceRecognitionStatus = FaceRecognitionStatusEnum.INVALID_MATH;
                return;
            }

            alphaLatticLength = lmsResult.alphaLattice;
            betaLatticLength = gammaRatio * lmsResult.alphaLattice;
            double lastSigma = lmsResult.sigma;

            // Loop until some resolution
            while (lmsResult.sigma > 35) //MenuAndParams.faceLmsThresholdParam.value)
            {

                if (numRhombusMoves > 5)
                {
                    faceRecognitionStatus = FaceRecognitionStatusEnum.INCOMPLETE;
                    return;
                }

                // Move a Rhombi
                if (FindAndMoveArhombusToAbetterLocation() == false)
                {
                    faceRecognitionStatus = FaceRecognitionStatusEnum.BLOCKED;
                    return;
                }
                numRhombusMoves++;

                // Evaluate
                lmsResult = FindOptimumFaceFit();
                if (lmsResult.valid == false)
                {
                    faceRecognitionStatus = FaceRecognitionStatusEnum.INVALID_MATH;
                    return;
                }
                alphaLatticLength = lmsResult.alphaLattice;
                betaLatticLength = gammaRatio * lmsResult.alphaLattice;

                // RMS has increased, we are NOT converging on a solution.
                if (lmsResult.sigma > lastSigma)
                {
                    faceRecognitionStatus = FaceRecognitionStatusEnum.UNSTABLE;
                }      
            }
            
            // A good solution has been reached!

            // Obtain Logical Tiles
            new ColorRecognition.Face(this).FaceTileColorRecognition(image);

            // Calculate a hash code that is unique for the given collection of Logical Tiles.
            // Added right rotation to obtain unique number with respect to locations.
            myHashCode = 0;
            for (int n = 0; n < 3; n++)
            {
                for (int m = 0; m < 3; m++)
                {
                    // myHashCode = observedTileArray[n][m].GetHashCode() ^ Integer.rotateRight(myHashCode, 1);   
                    myHashCode = observedTileArray[n][m].GetHashCode() ^ myHashCode >> 1;  //Write code to test a translation //(byte) cast ??
                    //Debug.Log("myHashCode: " + myHashCode);
                }
            }
            faceRecognitionStatus = FaceRecognitionStatusEnum.SOLVED;
        }

        /// <summary>
        /// Calculate Metrics
        /// Obtain alpha beta and gammaRatio from Rhombi set.
        /// =+= Initially, assume all provide Rhombus are in face, and simply take average.
        /// =+= Later provide more smart filtering.
        /// </summary>
        private bool CalculateMetrics()
        {
            int numElements = rhombusList.Count;

            foreach (Rhombus rhombus in rhombusList)
            {

                alphaAngle += rhombus.alphaAngle;
                betaAngle += rhombus.betaAngle;
                gammaRatio += rhombus.gammaRatio;
            }

            alphaAngle = alphaAngle / numElements * Math.PI / 180.0;
            betaAngle = betaAngle / numElements * Math.PI / 180.0;
            gammaRatio = gammaRatio / numElements;

            //Debug.Log(String.Format("RubikFace: alphaAngle={0,4:F0} betaAngle={1,4:F0} gamma={2,4:F2}", alphaAngle * 180.0 / Math.PI, betaAngle * 180.0 / Math.PI, gammaRatio));

            // =+= currently, always return OK
            return true;
        }

        /// <summary>
        /// Calculate the optimum fit for the given layout of Rhombus in the Face.
        /// 
        /// Set Up BIG Linear Equation: Y = AX
        /// Where:
        ///   Y is a 2k x 1 matrix of actual x and y location from rhombus centers (known values)
        ///   X is a 3 x 1  matrix of { x_origin, y_origin, and alpha_lattice } (values we wish to find)
        ///   A is a 2k x 3 matrix of coefficients derived from m, n, alpha, beta, and gamma. 
        /// 
        /// Notes:
        ///   k := Twice the number of available rhombus.
        ///   n := integer axis of the face.
        ///   m := integer axis of the face.
        /// 
        ///   gamma := ratio of beta to alpha lattice size.
        /// 
        /// Also, calculate sum of errors squared.
        ///   E = Y - AX
        /// @return
        /// </summary>
        private LeastMeansSquare FindOptimumFaceFit()
        {
            // Count how many non-empty cell actually have a rhombus in it.
            int k = 0;
            for (int n = 0; n < 3; n++)
            {
                for (int m = 0; m < 3; m++)
                {
                    if (faceRhombusArray[n][m] != null)
                    {
                        k++;
                    }
                }
            }

            Mat bigAmatrix = new Mat(2 * k, 3, MatType.CV_64FC1);
            Mat bigYmatrix = new Mat(2 * k, 1, MatType.CV_64FC1);
            Mat bigXmatrix = new Mat(    3, 1, MatType.CV_64FC1); //{ origin_x, origin_y, latticeAlpha }

            // Load up matrices Y and A 
            // X_k = X + n * L_alpha * cos(alpha) + m * L_beta * cos(beta)
            // Y_k = Y + n * L_alpha * sin(alpha) + m * L_beta * sin(beta)
            int index = 0;          
            for (int n = 0; n < 3; n++)
            {
                for (int m = 0; m < 3; m++)
                {
                    Rhombus rhombus = faceRhombusArray[n][m];
                    if (rhombus != null)
                    {
                        
                        {
                            // Actual X axis value of Rhombus in this location
                            double bigY = rhombus.center.X;
                               
                            // Express expected X axis value : i.e. x = func( x_origin, n, m, alpha, beta, alphaLattice, gamma)
                            double bigA = n * Math.Cos(alphaAngle) + gammaRatio * m * Math.Cos(betaAngle);

                            bigYmatrix.Set(index, 0, bigY);
                                
                            bigAmatrix.Set(index, 0, 1.0);
                            bigAmatrix.Set(index, 1, 0.0);
                            bigAmatrix.Set(index, 2, bigA);

                            index++;
                        }


                        {
                            // Actual Y axis value of Rhombus in this location
                            double bigY = rhombus.center.Y;
                              
                            // Express expected Y axis value : i.e. y = func( y_origin, n, m, alpha, beta, alphaLattice, gamma)
                            double bigA = n * Math.Sin(alphaAngle) + gammaRatio * m * Math.Sin(betaAngle);

                            bigYmatrix.Set(index, 0, bigY);
                                
                            bigAmatrix.Set(index, 0, 0.0);
                            bigAmatrix.Set(index, 1, 1.0);
                            bigAmatrix.Set(index, 2, bigA);

                            index++;
                        }
                    }
                }
            }

            //Debug.Log("Big A Matrix: " + bigAmatrix.Dump());
            //Debug.Log("Big Y Matrix: " + bigYmatrix.Dump());

            /*Debug.Log("%%%%%%%%%%%%%%%%%%% BigA Dump %%%%%%%%%%%%%%%%%%%%%%%% \n");
            Debug.Log("bigA Height: " + bigAmatrix.Height);
            Debug.Log("bigA Width: " + bigAmatrix.Width);
            for (int y = 0; y < bigAmatrix.Height; y++)
            {
                for (int x = 0; x < bigAmatrix.Width; x++)
                {
                    Debug.Log(bigAmatrix.At<double>(y, x) +",");
                }
                Debug.Log(";");
            }
            Debug.Log("%%%%%%%%%%%%%%%%% BigA Dump END %%%%%%%%%%%%%%%%%%%%%%% \n");

            Debug.Log("%%%%%%%%%%%%%%%%%%% BigY Dump %%%%%%%%%%%%%%%%%%%%%%%% \n");
            Debug.Log("bigY Height: " + bigYmatrix.Height);
            Debug.Log("bigY Width: " + bigYmatrix.Width);
            for (int y = 0; y < bigYmatrix.Height; y++)
            {
                for (int x = 0; x < bigYmatrix.Width; x++)
                {
                    Debug.Log(bigYmatrix.At<double>(y, x) + ",");
                }
                Debug.Log(";");
            }
            Debug.Log("%%%%%%%%%%%%%%%%% BigY Dump END %%%%%%%%%%%%%%%%%%%%%%% \n");*/


            // Least Means Square Regression to find best values of origin_x, origin_y, and alpha_lattice.
            // Problem:  Y=AX  Known Y and A, but find X.
            // Tactic:   Find minimum | AX - Y | (actually sum square of elements?)
            // OpenCV:   Core.solve(Mat src1, Mat src2, Mat dst, int)
            // OpenCV:   dst = arg min _X|src1 * X - src2|
            // Thus:     src1 = A  { 2k rows and  3 columns }
            //           src2 = Y  { 2k rows and  1 column  }
            //           dst =  X  {  3 rows and  1 column  }
            //
               
                
          


                bool solveFlag = Cv2.Solve(bigAmatrix, bigYmatrix, bigXmatrix, DecompTypes.Normal);
                
                //Debug.Log("Big X Matrix Result: " + bigXmatrix.Dump());
                /*   Debug.Log("%%%%%%%%%%%%%%%%%%% BigX Dump %%%%%%%%%%%%%%%%%%%%%%%% \n");
                Debug.Log("bigX Height: " + bigXmatrix.Height);
                Debug.Log("bigX Width: " + bigXmatrix.Width);
                for (int y = 0; y < bigXmatrix.Height; y++)
                {
                    for (int x = 0; x < bigXmatrix.Width; x++)
                    {
                        Debug.Log(bigXmatrix.At<double>(y, x) + ",");
                    }
                    Debug.Log(";");
                }
                Debug.Log("%%%%%%%%%%%%%%%%% BigX Dump END %%%%%%%%%%%%%%%%%%%%%%% \n");*/

            // Sum of error square
            // Given X from above, the Y_estimate = AX
            // E = Y - AX
            Mat bigEmatrix = new Mat(2 * k, 1, MatType.CV_64FC1);
            for (int r = 0; r < (2 * k); r++)
            {
                double Y = bigYmatrix.Get<double>(r, 0);
                double error = Y;
                for (int c = 0; c < 3; c++)
                {
                    double a = bigAmatrix.Get<double>(r, c);
                    double X = bigXmatrix.Get<double>(c, 0);

                    error -= a * X;
                }
                bigEmatrix.Set(r, 0, error);                    
            }

            // sigma^2 = diagonal_sum( Et * E)
            double sigma = 0;
            for (int r = 0; r < (2 * k); r++)
            {
                double error = bigEmatrix.Get<double>(r, 0);
                sigma += error * error;
            }
            sigma = Math.Sqrt(sigma);

            //Debug.Log("Big E Matrix Result: " + bigEmatrix.Dump());
            /*Debug.Log("%%%%%%%%%%%%%%%%%%% BigE Dump %%%%%%%%%%%%%%%%%%%%%%%% \n");
            Debug.Log("bigX Height: " + bigEmatrix.Height);
            Debug.Log("bigX Width: " + bigEmatrix.Width);
            for (int r = 0; r < bigEmatrix.Height; r++)
            {
                for (int t = 0; t < bigEmatrix.Width; t++)
                {
                    Debug.Log(bigEmatrix.At<double>(r, t) + ",");
                }
                Debug.Log(";");
            }
            Debug.Log("%%%%%%%%%%%%%%%%% BigE Dump END %%%%%%%%%%%%%%%%%%%%%%% \n");*/

            // =+= not currently in use, could be deleted.
            // Retrieve Error terms and compose an array of error vectors: one of each occupied
            // cell who's vector point from tile center to actual location of rhombus.
            Point[][] errorVectorArray = RectangularArrays.ReturnRectangularPointArray(3, 3);
            index = 0;
            for (int n = 0; n < 3; n++)
            {
                for (int m = 0; m < 3; m++)
                {
                    Rhombus rhombus = faceRhombusArray[n][m]; // We expect this array to not have changed from above.
                    if (rhombus != null)
                    {
                        errorVectorArray[n][m] = new Point(bigEmatrix.Get<double>(index++, 0), bigEmatrix.Get<double>(index++, 0));
                    }
                }
            }

            double x = bigXmatrix.Get<double>(0, 0);
            double y = bigXmatrix.Get<double>(1, 0);
            double alphaLatice = bigXmatrix.Get<double>(2, 0);

            bool valid = !double.IsNaN(x) && !double.IsNaN(y) && !double.IsNaN(alphaLatice) && !double.IsNaN(sigma);

            //Debug.Log(string.Format("Rubik Solution: x=%4.0f y=%4.0f alphaLattice=%4.0f  sigma=%4.0f flag=%b", x, y, alphaLatice, sigma, solveFlag));
            //Debug.Log(string.Format("Rubik Solution: " +x +y +alphaLatice +sigma +solveFlag));

            return new LeastMeansSquare(x, y, alphaLatice, errorVectorArray, sigma, valid);
        }

        /// <summary>
        /// Find And Move A Rhombus To A Better Location
        /// 
        /// Returns true if a tile was move or swapped, otherwise returns false.
        /// 
        /// Find Tile-Rhombus (i.e. {n,m}) with largest error assuming findOptimumFaceFit() has been called.
        /// Determine which direction the Rhombus would like to move and swap it with that location.
        /// </summary>
        private bool FindAndMoveArhombusToAbetterLocation()
        {
            double[][] errorArray = RectangularArrays.ReturnRectangularDoubleArray(3, 3);
   
            Point[][] errorVectorArray = RectangularArrays.ReturnRectangularPointArray(3, 3);

            // Identify Tile-Rhombus with largest error
            Rhombus largestErrorRhombus = null;
            double largetError = double.NegativeInfinity;
            int tile_n = 0; // Record current location of Rhombus we wish to move.
            int tile_m = 0;
            for (int n = 0; n < 3; n++)
            {
                for (int m = 0; m < 3; m++)
                {
                    Rhombus rhombus = faceRhombusArray[n][m];
                    if (rhombus != null)
                    {

                        // X and Y location of the center of a tile {n,m}
                        double tile_x = lmsResult.origin.X + n * alphaLatticLength * Math.Cos(alphaAngle) + m * betaLatticLength * Math.Cos(betaAngle);
                        double tile_y = lmsResult.origin.Y + n * alphaLatticLength * Math.Sin(alphaAngle) + m * betaLatticLength * Math.Sin(betaAngle);

                        // Error from center of tile to reported center of Rhombus
                        double error = Math.Sqrt((rhombus.center.X - tile_x) * (rhombus.center.X - tile_x) + (rhombus.center.Y - tile_y) * (rhombus.center.Y - tile_y));
                        errorArray[n][m] = error;
                        errorVectorArray[n][m] = new Point((double)rhombus.center.X - tile_x, rhombus.center.Y - tile_y); // Eoghan cast to int

                        // Record largest error found
                        if (error > largetError)
                        {
                            largestErrorRhombus = rhombus;
                            tile_n = n;
                            tile_m = m;
                            largetError = error;
                        }
                    }
                }
            }

            // For each tile location print: center of current Rhombus, center of tile, error vector, error magnitude.
            /*Debug.Log(string.Format(" m:n|-----0-----|------1----|----2------|"));
            Debug.Log(string.Format(" 0  |{0}|{1}|{2}|", Util.DumpLoc(faceRhombusArray[0][0]), Util.DumpLoc(faceRhombusArray[1][0]), Util.DumpLoc(faceRhombusArray[2][0])));
            Debug.Log(string.Format(" 0  |{0}|{1}|{2}|", Util.DumpPoint(GetTileCenterInPixels(0, 0)), Util.DumpPoint(GetTileCenterInPixels(1, 0)), Util.DumpPoint(GetTileCenterInPixels(2, 0))));
            Debug.Log(string.Format(" 0  |{0}|{1}|{2}|", Util.DumpPoint(errorVectorArray[0][0]), Util.DumpPoint(errorVectorArray[1][0]), Util.DumpPoint(errorVectorArray[2][0])));
            Debug.Log(string.Format(" 0  |{0,11:F0}|{1,11:F0}|{2,11:F0}|", errorArray[0][0], errorArray[1][0], errorArray[2][0]));
            Debug.Log(string.Format(" 1  |{0}|{1}|{2}|", Util.DumpLoc(faceRhombusArray[0][1]), Util.DumpLoc(faceRhombusArray[1][1]), Util.DumpLoc(faceRhombusArray[2][1])));
            Debug.Log(string.Format(" 1  |{0}|{1}|{2}|", Util.DumpPoint(GetTileCenterInPixels(0, 1)), Util.DumpPoint(GetTileCenterInPixels(1, 1)), Util.DumpPoint(GetTileCenterInPixels(2, 1))));
            Debug.Log(string.Format(" 1  |{0}|{1}|{2}|", Util.DumpPoint(errorVectorArray[0][1]), Util.DumpPoint(errorVectorArray[1][1]), Util.DumpPoint(errorVectorArray[2][1])));
            Debug.Log(string.Format(" 1  |{0,11:F0}|{1,11:F0}|{2,11:F0}|", errorArray[0][1], errorArray[1][1], errorArray[2][1]));
            Debug.Log(string.Format(" 2  |{0}|{1}|{2}|", Util.DumpLoc(faceRhombusArray[0][2]), Util.DumpLoc(faceRhombusArray[1][2]), Util.DumpLoc(faceRhombusArray[2][2])));
            Debug.Log(string.Format(" 2  |{0}|{1}|{2}|", Util.DumpPoint(GetTileCenterInPixels(0, 2)), Util.DumpPoint(GetTileCenterInPixels(1, 2)), Util.DumpPoint(GetTileCenterInPixels(2, 2))));
            Debug.Log(string.Format(" 2  |{0}|{1}|{2}|", Util.DumpPoint(errorVectorArray[0][2]), Util.DumpPoint(errorVectorArray[1][2]), Util.DumpPoint(errorVectorArray[2][2])));
            Debug.Log(string.Format(" 2  |{0,11:F0}|{1,11:F0}|{2,11:F0}|", errorArray[0][2], errorArray[1][2], errorArray[2][2]));
            Debug.Log(string.Format("    |-----------|-----------|-----------|"));*/


            // Calculate vector error (from Tile to Rhombus) components along X and Y axis
            double error_x = largestErrorRhombus.center.X - (lmsResult.origin.X + tile_n * alphaLatticLength * Math.Cos(alphaAngle) + tile_m * betaLatticLength * Math.Cos(betaAngle));
            double error_y = largestErrorRhombus.center.Y - (lmsResult.origin.Y + tile_n * alphaLatticLength * Math.Sin(alphaAngle) + tile_m * betaLatticLength * Math.Sin(betaAngle));
            //Debug.Log(string.Format("Tile at [{0:D}][{1:D}] has x error = {2,4:F0} y error = {3,4:F0}", tile_n, tile_m, error_x, error_y));

            // Project vector error (from Tile to Rhombus) components along alpha and beta directions.
            double alphaError = error_x * Math.Cos(alphaAngle) + error_y * Math.Sin(alphaAngle);
            double betaError = error_x * Math.Cos(betaAngle) + error_y * Math.Sin(betaAngle);
            //Debug.Log(string.Format("Tile at [{0:D}][{1:D}] has alpha error = {2,4:F0} beta error = {3,4:F0}", tile_n, tile_m, alphaError, betaError));

            // Calculate index vector correction: i.e., preferred direction to move this tile.
            int delta_n = (int)Math.Round(alphaError / alphaLatticLength);
            int delta_m = (int)Math.Round(betaError / betaLatticLength);
            //Debug.Log(string.Format("Correction Index Vector: [{0:D}][{1:D}]", delta_n, delta_m));

            // Calculate new location of tile
            int new_n = tile_n + delta_n;
            int new_m = tile_m + delta_m;

            // Limit according to dimensions of face
            if (new_n < 0)
            {
                new_n = 0;
            }
            if (new_n > 2)
            {
                new_n = 2;
            }
            if (new_m < 0)
            {
                new_m = 0;
            }
            if (new_m > 2)
            {
                new_m = 2;
            }

            // Cannot move, move is to original location
            if (new_n == tile_n && new_m == tile_m)
            {
                //Debug.Log(string.Format("Tile at [{0:D}][{1:D}] location NOT moved", tile_n, tile_m));
                return false;
            }

            // Move Tile or swap with tile in that location.
            else
            {
                //Debug.Log(string.Format("Swapping Rhombi [{0:D}][{1:D}] with  [{2:D}][{3:D}]", tile_n, tile_m, new_n, new_m));
                Rhombus tmp = faceRhombusArray[new_n][new_m];
                faceRhombusArray[new_n][new_m] = faceRhombusArray[tile_n][tile_m];
                faceRhombusArray[tile_n][tile_m] = tmp;
                return true;
            }

        }

        /// <param name="n" type="int"> </param>
        /// <param name="m">
        /// @return </param>
        public virtual Point GetTileCenterInPixels(int n, int m)
        {
            return new Point(lmsResult.origin.X + n * alphaLatticLength * Math.Cos(alphaAngle) + m * betaLatticLength * Math.Cos(betaAngle), lmsResult.origin.Y + n * alphaLatticLength * Math.Sin(alphaAngle) + m * betaLatticLength * Math.Sin(betaAngle));
        }
    }
}
