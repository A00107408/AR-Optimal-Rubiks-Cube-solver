/* <summary
 * 
 * Augmented Reality Rubik Cube Application
 * A.I.T 2018
 * A00107408
 * Masters by Research
 * 
 * File Description:
 * Miscellaneous utilities that can exist as simple static functions here in 
 * this file, and are relatively uninteresting.
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
    using UnityEngine;
    using OpenCvSharp;

    using ColorTileEnum = Rubik.Constants.ColorTileEnum;
    using PruneTableLoader = KociembaTwoPhase.PruneTableLoader;

    public class Util
    {
        public static string DumpRGB(ColorTileEnum colorTile)
        {
            double[] val = new double[4];     // or  = { };  ??
            val[0] = colorTile.cvColor.Val0;
            val[1] = colorTile.cvColor.Val1;
            val[2] = colorTile.cvColor.Val2;
            return string.Format("r={0,3:F0} g={1,3:F0} b={2,3:F0}        ", val[0], val[1], val[2]);
        }

        public static string DumpRGB(double[] color, double colorError)
        {
            return string.Format("r={0,3:F0} g={1,3:F0} b={2,3:F0} e={3,5:F0}", color[0], color[1], color[2], colorError);
        }

        //	public static String dumpRGB(ConstantTile logicalTile) {
        //		double color[] = logicalTile.colorOpenCV.val;
        //		return String.format("r=%3.0f g=%3.0f b=%3.0f     t=%c", color[0], color[1], color[2], logicalTile.symbol);
        //	}
        public static string DumpYUV(double[] color)
        {
            color = GetYUVfromRGB(color);
            return string.Format("y={0,3:F0} u={1,3:F0} v={2,3:F0}        ", color[0], color[1], color[2]);
        }
     

        public static string DumpLoc(Rhombus rhombus)
        {
            if (rhombus == null)
            {
                return "           ";
            }
            else
            {
                return string.Format(" {0,4:F0},{1,4:F0} ", rhombus.center.X, rhombus.center.Y);
            }
        }
        public static string DumpPoint(Point point)
        {
            if (point == null)
            {
                return "           ";
            }
            else
            {
                return string.Format(" {0,4:F0},{1,4:F0} ", point.X, point.Y);
            }
        }

        /// <summary>
        /// Get YUV from RGB
        /// </summary>
        /// <param name="rgb">
        /// @return </param>
        public static double[] GetYUVfromRGB(double[] rgb)
        {

            if (rgb == null)
            {
                Debug.Log("RGB is NULL!");
                return new double[] { 0, 0, 0, 0 };
            }
            double[] yuv = new double[4];
            yuv[0] = 0.229 * rgb[0] + 0.587 * rgb[1] + 0.114 * rgb[2];
            yuv[1] = -0.147 * rgb[0] + -0.289 * rgb[1] + 0.436 * rgb[2];
            yuv[2] = 0.615 * rgb[0] + -0.515 * rgb[1] + -0.100 * rgb[2];
            return yuv;
        }

        internal static double GetYUVfromRGB(double val0)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns true if there are exactly nine of each tile color over entire cube,
        /// and if no two center tiles have the same color.
        /// 
        /// @return
        /// </summary>
        public static bool IsTileColorsValid(StateModel stateModel)
        {
            try
            {
                // Count how many tile colors entire cube has as a first check.
                int[] numColorTilesArray = new int[] { 0, 0, 0, 0, 0, 0 };
                foreach (RubikFace rubikFace in stateModel.nameRubikFaceMap.Values)
                {
                    for (int n = 0; n < 3; n++)
                    {
                        for (int m = 0; m < 3; m++)
                        {
                            numColorTilesArray[rubikFace.observedTileArray[n][m].ordinal()]++; // =+= saw null except: 20150713
                        }
                    }
                }
            

                // Check that we have nine of each tile color over entire cube.
                foreach (ColorTileEnum colorTile in ColorTileEnum.values())
                {
                    if (colorTile.isRubikColor == true)
                    {
                        if (numColorTilesArray[colorTile.ordinal()] != 9)
                        {
                            Debug.Log("REJECT: There are " + numColorTilesArray[colorTile.ordinal()] + " tiles of color " + colorTile + ", and there should be exactly 9");
                            return false;
                        }
                    }
                }

                // Check that there are exactly six elements in the above set.
                HashSet<ColorTileEnum> centerTileSet = new HashSet<ColorTileEnum> {  }; //(16)  //'Eoghan' hopefully size is dynamic??
                foreach (RubikFace rubikFace in stateModel.nameRubikFaceMap.Values)
                {
                    ColorTileEnum colorTile = rubikFace.observedTileArray[1][1];
                    if (centerTileSet.Contains(colorTile))
                    {
                        Debug.Log("REJECT: There are two center tiles that have been assigned the same color of:" + colorTile);
                        return false;
                    }
                    else
                    {
                        centerTileSet.Add(colorTile);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log("Bad color Crash !!!!!  **********************  " +e);
            }

            return true;
        }

        /// <summary>
        /// Create a new array instance object, populate it with tiles rotated clockwise
        /// with respect to the pass in arg, and then return the new object.
        /// </summary>
        /// <param name="arg">
        /// @return </param>
        public static ColorTileEnum[][] GetTileArrayRotatedClockwise(ColorTileEnum[][] arg)
        {
            //         n -------------->
            //   m     0-0    1-0    2-0
            //   |     0-1    1-1    2-1
            //   v     0-2    1-2    2-2
            //JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
            //ORIGINAL LINE: ColorTileEnum[][] result = new ColorTileEnum[3][3];
            ColorTileEnum[][] result = RectangularArrays.ReturnRectangularColorTileEnumArray(3, 3);
            result[1][1] = arg[1][1];
            result[2][0] = arg[0][0];
            result[2][1] = arg[1][0];
            result[2][2] = arg[2][0];
            result[1][2] = arg[2][1];
            result[0][2] = arg[2][2];
            result[0][1] = arg[1][2];
            result[0][0] = arg[0][2];
            result[1][0] = arg[0][1];

            return result;
        }

        //----------------------------------------------------------------------------------------
        //	Copyright © 2007 - 2018 Tangible Software Solutions Inc.
        //	This class can be used by anyone provided that the copyright notice remains intact.
        //
        //	This class includes methods to convert Java rectangular arrays (jagged arrays
        //	with inner arrays of the same length).
        //----------------------------------------------------------------------------------------
        internal static class RectangularArrays
        {
            internal static ColorTileEnum[][] ReturnRectangularColorTileEnumArray(int size1, int size2)
            {
                ColorTileEnum[][] newArray = new ColorTileEnum[size1][];
                for (int array1 = 0; array1 < size1; array1++)
                {
                    newArray[array1] = new ColorTileEnum[size2];
                }

                return newArray;
            }
        }

        public static ColorTileEnum[][] GetTileArrayRotated180(ColorTileEnum[][] arg)
        {
            return GetTileArrayRotatedClockwise(GetTileArrayRotatedClockwise(arg));
        }

        /// <summary>
        /// Get Two Phase Error String
        /// 
        /// Arg should be a character between 0 and 8 inclusive.
        /// </summary>
        /// <param name="errorCode">
        /// @return </param>
        public static string GetTwoPhaseErrorString(char errorCode)
        {
            string stringErrorMessage;
            switch (errorCode)
            {
                case '0':
                    stringErrorMessage = "Cube is verified and correct!";
                    WindowsVoice.speak("Cube is verified and correct!");
                    break;
                case '1':
                    stringErrorMessage = "There are not exactly nine facelets of each color!";
                    WindowsVoice.speak("There are not exactly nine facelets of each color!");
                    break;
                case '2':
                    stringErrorMessage = "Not all 12 edges exist exactly once!";
                    WindowsVoice.speak("Not all 12 edges exist exactly once!");
                    break;
                case '3':
                    stringErrorMessage = "Flip error: One edge has to be flipped!";
                    WindowsVoice.speak("Flip error: One edge has to be flipped!");
                    break;
                case '4':
                    stringErrorMessage = "Not all 8 corners exist exactly once!";
                    WindowsVoice.speak("Not all 8 corners exist exactly once!");
                    break;
                case '5':
                    stringErrorMessage = "Twist error: One corner has to be twisted!";
                    WindowsVoice.speak("Twist error: One corner has to be twisted!");
                    break;
                case '6':
                    stringErrorMessage = "Parity error: Two corners or two edges have to be exchanged!";
                    WindowsVoice.speak("Parity error: Two corners or two edges have to be exchanged!");
                    break;
                case '7':
                    stringErrorMessage = "No solution exists for the given maximum move number!";
                    WindowsVoice.speak("No solution exists for the given maximum move number!");
                    break;
                case '8':
                    stringErrorMessage = "Timeout, no solution found within given maximum time!";
                    WindowsVoice.speak("Timeout, no solution found within given maximum time!");
                    break;
                default:
                    stringErrorMessage = "Unknown error code returned: ";
                    WindowsVoice.speak("Unknown error code returned:");
                    break;
            }
            return stringErrorMessage;
        }

        /// <summary>
        /// Load Rubik Logic Algorithm Pruning Tables is a separate thread.
        /// 
        /// @author android.steve@cl-sw.com
        /// 
        /// </summary>
        public class LoadPruningTablesTask //: AsyncTask<AppStateMachine, Void, Void>
        {

            private PruneTableLoader tableLoader = new PruneTableLoader();
            private AppStateMachine appStateMachine;

            protected internal void DoInBackground(AppStateMachine appStateMachine)//params AppStateMachine[] @params)
            {

                //   appStateMachine = @params[0];

                this.appStateMachine = appStateMachine;

                /* load all tables if they are not already in RAM */
                while (!tableLoader.LoadingFinished())
                { // while tables are left to load
                    tableLoader.LoadNext(); // load next pruning table
                    appStateMachine.pruneTableLoaderCount++;
                    //Debug.Log("Created a prune table.");
                }
               // Debug.Log("Loaded all prunning tables.");                
               // return null;
            }
        }
    }
}