/* <summary>
 * 
 * Augmented Reality Rubik Cube Application
 * A.I.T 2018
 * A00107408
 * Masters by Research
 * 
 * File Description:
 * Simple class that represents position and orientation of cube.
 *
 * Acknowledgments:
 * Original Author: Steven P. Punte (aka Android Steve : android.steve@cl-sw.com)
 * Date: April 25th 2015
 * 
 * <summary> */
namespace Rubik
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using OpenCvSharp;

    using ColorTileEnum = Constants.ColorTileEnum;
    using FaceNameEnum = Constants.FaceNameEnum;
    using AppStateEnum = Constants.AppStateEnum;
    using GestureRecogniztionStateEnum = Constants.GestureRecogniztionStateEnum;

    public class StateModel
    {
        // Rubik Face of latest processed frame: may or may not be any of the six state objects.
        public RubikFace activeRubikFace;

        /*
         * This is "Rubik Cube State" or "Rubik Cube Model" in model-view-controller vernacular.
         * Map of above rubik face objects index by FaceNameEnum
         */
        public Dictionary<FaceNameEnum, RubikFace> nameRubikFaceMap = new Dictionary<Constants.FaceNameEnum, RubikFace>(6);

        /*
         * This is a hash map of OpenCV colors that are initialized to those specified by field
         * rubikColor of ColorTileEnum.   Function reevauateSelectTileColors() adjusts these 
         * colors according to a Mean-Shift algorithm to correct for lumonosity.
         */
        public Dictionary<ColorTileEnum, Scalar> mutableTileColors = new Dictionary<ColorTileEnum, Scalar>(6);

        // Application State; see AppStateEnum.
        public AppStateEnum appState = AppStateEnum.START;

        // Stable Face Recognizer State
        public GestureRecogniztionStateEnum gestureRecogniztionState = GestureRecogniztionStateEnum.UNKNOWN;

        // Result when Two Phase algorithm is ask to evaluate if cube in valid.  If valid, code is zero.
        public int verificationResults;

        // String notation on how to solve cube.
        public string solutionResults;

        // Above, but broken into individual moves.
        public string[] solutionResultsArray;

        // Index to above array as to which move we are on.
        public int solutionResultIndex;

        // We assume that faces will be explored in a particular sequence.
        public int adoptFaceCount = 0;

        // Additional Cube Rotation: initially set to Identity Rotation Matrix
        //public float[] additionalGLCubeRotation = new float[16];

        // True if it is OK to render GL Pilot Cube
        public bool renderPilotCube = true;

        // Intrinsic Camera Calibration Parameters from hardware.
        //public CameraCalibration cameraCalibration;

        // Processes OpenCV Pose results and maintains 3D Model.
        //[NonSerialized]
        //public KalmanFilter kalmanFilter;

        // For the purpose of running the Autocovariance Least Squares Method to object Kalman Filter Covariance Matrices
        //public KalmanFilterALSM kalmanFilterALSM;

        // Cube Location and Orientation deduced from Face.
        [NonSerialized]
        public CubePose cubePose;

        // Display size of JavaCameraView and OpenCV InputFrame
        [NonSerialized]
        public Size openCVSize;

        // Display size of OpenGL Rendering Surface
        [NonSerialized]
        public Size openGLSize;

        /// <summary>
        /// Default State Model Constructor
        /// </summary>
        public StateModel()
        {
            Reset();
        }

        /// <summary>
        /// Adopt Face
        /// 
        /// Adopt faces in a particular sequence dictated by the user directed instruction on
        /// how to rotate the code (CUBE ?) during the exploration phase.  Also tile name is 
        /// specified at this time, and "transformedTileArray" is created which is a 
        /// rotated version of the observed tile array so that the face orientations
        /// match the convention of a cut-out rubik cube layout.
        /// 
        /// =+= This logic duplicated in AppStateMachine
        /// </summary>
        /// <param name="rubikFace"> </param>
        public virtual void Adopt(RubikFace rubikFace)
        {
            switch (adoptFaceCount)
            {

                case 0:
                    rubikFace.faceNameEnum = FaceNameEnum.UP;
                    rubikFace.transformedTileArray = (ColorTileEnum[][])rubikFace.observedTileArray.Clone();
                    break;
                case 1:
                    rubikFace.faceNameEnum = FaceNameEnum.RIGHT;
                    rubikFace.transformedTileArray = Util.GetTileArrayRotatedClockwise(rubikFace.observedTileArray);
                    break;
                case 2:
                    rubikFace.faceNameEnum = FaceNameEnum.FRONT;
                    rubikFace.transformedTileArray = Util.GetTileArrayRotatedClockwise(rubikFace.observedTileArray);
                    break;
                case 3:
                    rubikFace.faceNameEnum = FaceNameEnum.DOWN;
                    rubikFace.transformedTileArray = Util.GetTileArrayRotatedClockwise(rubikFace.observedTileArray);
                    break;
                case 4:
                    rubikFace.faceNameEnum = FaceNameEnum.LEFT;
                    rubikFace.transformedTileArray = Util.GetTileArrayRotated180(rubikFace.observedTileArray);
                    break;
                case 5:
                    rubikFace.faceNameEnum = FaceNameEnum.BACK;
                    rubikFace.transformedTileArray = Util.GetTileArrayRotated180(rubikFace.observedTileArray);
                    break;

                default:
                    // =+= log error ?
                    break;
            }

            if (adoptFaceCount < 6)
            {
                // Record Face by Name: i.e., UP, DOWN, LEFT, ...
                nameRubikFaceMap[rubikFace.faceNameEnum] = rubikFace;  //nameRubikFaceMap.put(rubikFace.faceNameEnum, rubikFace);
            }
            adoptFaceCount++;
        }

        /// <summary>
        /// Return the number of valid and adopted faces.  Maximum is of course six.
        /// 
        /// @return
        /// </summary>
        public virtual int NumObservedFaces
        {
            get
            {
                return nameRubikFaceMap.Count;
            }
        }

        /// <summary>
        /// Return true if all six faces have been observed and adopted.
        /// 
        /// @return
        /// </summary>
        public virtual bool ThereAfullSetOfFaces
        {
            get
            {
                if (NumObservedFaces >= 6)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Get String Representation of Cube
        /// 
        /// The "String Representation" is a per the two-phase rubik cube
        /// logic solving algorithm requires.
        /// 
        /// This should only be called if cube if colors are valid.
        /// 
        /// @return
        /// </summary>
        public virtual string StringRepresentationOfCube
        {
            get
            {

                // Create a map of tile color to face name. The center tile of each face is used for this 
                // definition.  This information is used by Rubik Cube Logic Solution.
                Dictionary<ColorTileEnum, FaceNameEnum> colorTileToNameMap = new Dictionary<ColorTileEnum, FaceNameEnum>(6);
                colorTileToNameMap[GetFaceByName(FaceNameEnum.UP).transformedTileArray[1][1]] = FaceNameEnum.UP;
                colorTileToNameMap[GetFaceByName(FaceNameEnum.DOWN).transformedTileArray[1][1]] = FaceNameEnum.DOWN;
                colorTileToNameMap[GetFaceByName(FaceNameEnum.LEFT).transformedTileArray[1][1]] = FaceNameEnum.LEFT;
                colorTileToNameMap[GetFaceByName(FaceNameEnum.RIGHT).transformedTileArray[1][1]] = FaceNameEnum.RIGHT;
                colorTileToNameMap[GetFaceByName(FaceNameEnum.FRONT).transformedTileArray[1][1]] = FaceNameEnum.FRONT;
                colorTileToNameMap[GetFaceByName(FaceNameEnum.BACK).transformedTileArray[1][1]] = FaceNameEnum.BACK;


                StringBuilder sb = new StringBuilder();
                sb.Append(GetStringRepresentationOfFace(colorTileToNameMap, GetFaceByName(FaceNameEnum.UP)));
                sb.Append(GetStringRepresentationOfFace(colorTileToNameMap, GetFaceByName(FaceNameEnum.RIGHT)));
                sb.Append(GetStringRepresentationOfFace(colorTileToNameMap, GetFaceByName(FaceNameEnum.FRONT)));
                sb.Append(GetStringRepresentationOfFace(colorTileToNameMap, GetFaceByName(FaceNameEnum.DOWN)));
                sb.Append(GetStringRepresentationOfFace(colorTileToNameMap, GetFaceByName(FaceNameEnum.LEFT)));
                sb.Append(GetStringRepresentationOfFace(colorTileToNameMap, GetFaceByName(FaceNameEnum.BACK)));
                return sb.ToString();
            }
        }

        /// <summary>
        /// Get Rubik Face by Name
        /// </summary>
        /// <param name="faceNameEnum">
        /// @return </param>
        public virtual RubikFace GetFaceByName(FaceNameEnum faceNameEnum)
        {
            return nameRubikFaceMap[faceNameEnum];
        }

        /// <summary>
        /// Get String Representing a particular Face. </summary>
        /// <param name="colorTileToNameMap"> 
        /// </param>
        /// <param name="rubikFace">
        /// @return </param>
        private StringBuilder GetStringRepresentationOfFace(Dictionary<ColorTileEnum, FaceNameEnum> colorTileToNameMap, RubikFace rubikFace)
        {

            StringBuilder sb = new StringBuilder();
            ColorTileEnum[][] virtualLogicalTileArray = rubikFace.transformedTileArray;
            for (int m = 0; m < 3; m++)
            {
                for (int n = 0; n < 3; n++)
                {
                    sb.Append(GetCharacterRepresentingColor(colorTileToNameMap, virtualLogicalTileArray[n][m]));
                }
            }
            return sb;
        }

        /// <summary>
        /// Get Character Representing Color
        /// 
        /// Return single character representing Face Name (i.e., Up, Down, etc...) of face 
        /// who's center tile is of the passed in arg. </summary>
        /// <param name="colorTileToNameMap"> 
        /// 
        /// </param>
        /// <param name="colorEnum">
        /// @return </param>
        private char GetCharacterRepresentingColor(Dictionary<ColorTileEnum, FaceNameEnum> colorTileToNameMap, ColorTileEnum colorEnum)
        {

            //		Log.e(Constants.TAG_COLOR, "colorEnum=" + colorEnum + " colorTileToNameMap=" + colorTileToNameMap);

            switch (colorTileToNameMap[colorEnum])
            {
                case FaceNameEnum.FRONT:
                    return 'F';
                case FaceNameEnum.BACK:
                    return 'B';
                case FaceNameEnum.DOWN:
                    return 'D';
                case FaceNameEnum.LEFT:
                    return 'L';
                case FaceNameEnum.RIGHT:
                    return 'R';
                case FaceNameEnum.UP:
                    return 'U';
                default:
                    return (char)0; // Odd error message without this, but cannot get here by definition.  Hmm.
            }
        }

        /// <summary>
        /// Reset
        /// 
        /// Reset state to the initial values.
        /// </summary>
        public virtual void Reset()
        {

            // Rubik Face of latest processed frame: may or may not be any of the six state objects.
            activeRubikFace = null;

            // Array of above rubik face objects index by FaceNameEnum
            nameRubikFaceMap = new Dictionary<Constants.FaceNameEnum, RubikFace>(6);

            // Array of tile colors index by ColorTileEnum.
            mutableTileColors.Clear();
            foreach (ColorTileEnum colorTile in ColorTileEnum.values())
            {
                if (colorTile.isRubikColor == true)
                {
                    mutableTileColors[colorTile] = colorTile.rubikColor;
                }
            }

            // Application State = null; see AppStateEnum.
            appState = AppStateEnum.START;

            // Stable Face Recognizer State
            gestureRecogniztionState = GestureRecogniztionStateEnum.UNKNOWN;

            // Result when Two Phase algorithm is ask to evaluate if cube in valid.  If valid, code is zero.
            verificationResults = 0;

            // String notation on how to solve cube.
            solutionResults = null;

            // Above, but broken into individual moves.
            solutionResultsArray = null;

            // Index to above array as to which move we are on.
            solutionResultIndex = 0;

            // We assume that faces will be explored in a particular sequence.
            adoptFaceCount = 0;

            // True if we are to render GL Pilot Cube
            renderPilotCube = true;

            // Cube Location and Orientation deduced from Face.
            cubePose = null;

            // Set additional GL cube rotation to Identity Rotation Matrix
            //Matrix.setIdentityM(additionalGLCubeRotation, 0);
        }
    }
}
