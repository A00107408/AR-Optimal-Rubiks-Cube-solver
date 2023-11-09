/* <summary
 * 
 * Augmented Reality Rubik Cube Application
 * A.I.T 2018
 * A00107408
 * Masters by Research
 * 
 * File Description:
 * Draws a wide variety of diagnostic information on right side of the screen 
 * using OpenCV procedure calls.Activation of these diagnostics is through 
 * the Menu -> Annotation option.Also, draws user instructions(same information 
 * as UserInstructionsGLRenderer but in text form) across the top when 
 * enabled. 
 * 
 * Acknowledgments:
 * Original Author: Steven P. Punte (aka Android Steve : android.steve@cl-sw.com)
 * Date: April 25th 2015
 *   
 * <summary> */

namespace Rubik
{
    using System;
    using System.Text;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using OpenCvSharp;

    using AnnotationModeEnum = Constants.AnnotationModeEnum;
    using ColorTileEnum = Constants.ColorTileEnum;
    using FaceNameEnum = Constants.FaceNameEnum;
    using GestureRecogniztionStateEnum = Constants.GestureRecogniztionStateEnum;
    using FaceRecognitionStatusEnum = RubikFace.FaceRecognitionStatusEnum;
    using StatusEnum = Rhombus.StatusEnum;
    using AppStateEnum = Constants.AppStateEnum;

    public class Annotation
    {
        // Local reference to State Model
        private StateModel stateModel;

        // Local reference to Application State Machine
        private AppStateMachine appStateMachine;

        private bool SaidCantSeeCube, SaidSearching, SaidRotate, SaidWaiting, SaidSolved = false;
        public static bool SaidInst = false;
       
        private NextInstruction NI = GameObject.Find("Text").GetComponent<NextInstruction>();

        private Instructions instr = null;
                   

        /// <summary>
        /// Annotation Constructor
        /// </summary>
        /// <param name="stateModel"> </param>
        /// <param name="appStateMachine">  </param>
        public Annotation(StateModel stateModel, AppStateMachine appStateMachine)
        {
            this.stateModel = stateModel;
            this.appStateMachine = appStateMachine;

            instr = GameObject.Find("MetaCameraRig").GetComponent<Instructions>();
        }

        /// <summary>
        /// Draw Annotation
        /// 
        /// This typically will consume the right third of the landscape orientation image.
        /// </summary>
        /// <param name="image">
        /// @return </param>
        public Mat DrawAnnotation(ref Mat image)
        {
            // DrawFaceOverlayAnnotation(ref image);

            DrawFaceTileSymbolsAnnotation(ref image);

            //  DrawFaceColorMetrics(image, stateModel.activeRubikFace);

            // Render Text User Instructions on top part of screen.
            DrawUserInstructions(ref image);

            return image;
        }

        /// <summary>
        /// Draw Face Overlay Annotation
        /// </summary>
        /// <param name="image"> </param>
        private void DrawFaceOverlayAnnotation(ref Mat img)
        {
            RubikFace face = stateModel.activeRubikFace;
            
           // if (MenuAndParams.faceOverlayDisplay == false)
           // {
           //     return;
           // }

            if (face == null)
            {
                Debug.Log("3333333333333333333333333333333333 RETURN 3333333333333333333333333");
                return;
            }

            Scalar color = ColorTileEnum.BLACK.cvColor;
            switch (face.faceRecognitionStatus)
            {
                case FaceRecognitionStatusEnum.UNKNOWN:
                case FaceRecognitionStatusEnum.INSUFFICIENT:
                case FaceRecognitionStatusEnum.INVALID_MATH:
                    color = ColorTileEnum.RED.cvColor;
                    break;
                case FaceRecognitionStatusEnum.BAD_METRICS:
                case FaceRecognitionStatusEnum.INCOMPLETE:
                case FaceRecognitionStatusEnum.INADEQUATE:
                case FaceRecognitionStatusEnum.BLOCKED:
                case FaceRecognitionStatusEnum.UNSTABLE:
                    color = ColorTileEnum.ORANGE.cvColor;
                    break;
                case FaceRecognitionStatusEnum.SOLVED:
                    if (stateModel.gestureRecogniztionState == GestureRecogniztionStateEnum.STABLE || stateModel.gestureRecogniztionState == GestureRecogniztionStateEnum.NEW_STABLE)
                    {
                        color = ColorTileEnum.GREEN.cvColor;
                    }
                    else
                    {
                        color = ColorTileEnum.YELLOW.cvColor;
                    }
                    break;
            }

            // Adjust drawing grid to start at edge of cube and not center of a tile.
            // Eoghan changed x & y to X & Y
            double x = face.lmsResult.origin.X - (face.alphaLatticLength * Math.Cos(face.alphaAngle) + face.betaLatticLength * Math.Cos(face.betaAngle)) / 2;
            double y = face.lmsResult.origin.Y - (face.alphaLatticLength * Math.Sin(face.alphaAngle) + face.betaLatticLength * Math.Sin(face.betaAngle)) / 2;

            for (int n = 0; n < 4; n++)
            {
                //Core.line(img, new Point(x + n * face.alphaLatticLength * Math.Cos(face.alphaAngle), y + n * face.alphaLatticLength * Math.Sin(face.alphaAngle)), new Point(x + (face.betaLatticLength * 3 * Math.Cos(face.betaAngle)) + (n * face.alphaLatticLength * Math.Cos(face.alphaAngle)), y + (face.betaLatticLength * 3 * Math.Sin(face.betaAngle)) + (n * face.alphaLatticLength * Math.Sin(face.alphaAngle))), color, 3);
                Cv2.Line(img, new Point(x + n * face.alphaLatticLength * Math.Cos(face.alphaAngle), y + n * face.alphaLatticLength * Math.Sin(face.alphaAngle)), new Point(x + (face.betaLatticLength * 3 * Math.Cos(face.betaAngle)) + (n * face.alphaLatticLength * Math.Cos(face.alphaAngle)), y + (face.betaLatticLength * 3 * Math.Sin(face.betaAngle)) + (n * face.alphaLatticLength * Math.Sin(face.alphaAngle))), color, 3);
            }

            for (int m = 0; m < 4; m++)
            {
                //Core.line(img, new Point(x + m * face.betaLatticLength * Math.Cos(face.betaAngle), y + m * face.betaLatticLength * Math.Sin(face.betaAngle)), new Point(x + (face.alphaLatticLength * 3 * Math.Cos(face.alphaAngle)) + (m * face.betaLatticLength * Math.Cos(face.betaAngle)), y + (face.alphaLatticLength * 3 * Math.Sin(face.alphaAngle)) + (m * face.betaLatticLength * Math.Sin(face.betaAngle))), color, 3);
                Cv2.Line(img, new Point(x + m * face.betaLatticLength * Math.Cos(face.betaAngle), y + m * face.betaLatticLength * Math.Sin(face.betaAngle)), new Point(x + (face.alphaLatticLength * 3 * Math.Cos(face.alphaAngle)) + (m * face.betaLatticLength * Math.Cos(face.betaAngle)), y + (face.alphaLatticLength * 3 * Math.Sin(face.alphaAngle)) + (m * face.betaLatticLength * Math.Sin(face.betaAngle))), color, 3);
            }

            //		// Draw a circle at the Rhombus reported center of each tile.
            //		for(int n=0; n<3; n++) {
            //			for(int m=0; m<3; m++) {
            //				Rhombus rhombus = faceRhombusArray[n][m];
            //				if(rhombus != null)
            //					Core.circle(img, rhombus.center, 5, Constants.ColorBlue, 3);
            //			}
            //		}
            //		
            //		// Draw the error vector from center of tile to actual location of Rhombus.
            //		for(int n=0; n<3; n++) {
            //			for(int m=0; m<3; m++) {
            //				Rhombus rhombus = faceRhombusArray[n][m];
            //				if(rhombus != null) {
            //					
            //					Point tileCenter = getTileCenterInPixels(n, m);				
            //					Core.line(img, tileCenter, rhombus.center, Constants.ColorRed, 3);
            //					Core.circle(img, tileCenter, 5, Constants.ColorBlue, 1);
            //				}
            //			}
            //		}

            //		// Draw reported Logical Tile Color Characters in center of each tile.
            //		if(face.faceRecognitionStatus == FaceRecognitionStatusEnum.SOLVED)
            //			for(int n=0; n<3; n++) {
            //				for(int m=0; m<3; m++) {
            //
            //					// Draw tile character in UV plane
            //					Point tileCenterInPixels = face.getTileCenterInPixels(n, m);
            //					tileCenterInPixels.x -= 10.0;
            //					tileCenterInPixels.y += 10.0;
            //					String text = Character.toString(face.observedTileArray[n][m].symbol);
            //					Core.putText(img, text, tileCenterInPixels, Constants.FontFace, 3, ColorTileEnum.BLACK.cvColor, 3);
            //				}
            //			}

            // Also draw recognized Rhombi for clarity.
            if (face.faceRecognitionStatus != FaceRecognitionStatusEnum.SOLVED)
            {
                foreach (Rhombus rhombus in face.rhombusList)
                {
                    rhombus.Draw(img, ColorTileEnum.GREEN.cvColor);
                }
            }
        }

        /// <summary>
        /// Draw Face Tile Symbols Annotation
        /// </summary>
        /// <param name="image"> </param>
        private void DrawFaceTileSymbolsAnnotation(ref Mat image)
        {

            RubikFace face = stateModel.activeRubikFace;

           // if (MenuAndParams.symbolOverlayDisplay == false)
           // {
           //     return;
           // }

            if (face == null)
            {
                return;
            }

            // Draw reported Logical Tile Color Characters in center of each tile.
            if (face.faceRecognitionStatus == FaceRecognitionStatusEnum.SOLVED)
            {
                for (int n = 0; n < 3; n++)
                {
                    for (int m = 0; m < 3; m++)
                    {

                        // Draw tile character in UV plane
                        Point tileCenterInPixels = face.GetTileCenterInPixels(n, m);                          // Eoghan changed from:
                        tileCenterInPixels.X -= 10;                                                           // tileCenterInPixels.x -= 10.0;
                        tileCenterInPixels.Y += 10;                                                           // tileCenterInPixels.y += 10.0;
                        string text = Convert.ToString(face.observedTileArray[n][m].symbol);
                        //Core.putText(image, text, tileCenterInPixels, Constants.FontFace, 3, ColorTileEnum.BLACK.cvColor, 3);
                     //   Cv2.Flip(image, image, FlipMode.Y);
                        //Cv2.Flip(image, image, FlipMode.X);
                        Cv2.PutText(image, text, tileCenterInPixels, Constants.FontFace, 3, ColorTileEnum.BLACK.cvColor, 3);
                       // Cv2.Flip(image, image, FlipMode.X);
                     //   Cv2.Flip(image, image, FlipMode.Y);
                    }
                }
            }
        }

        /// <summary>
        /// Draw Face Color Metrics
        /// 
        /// Draw a 2D representation of observed tile colors vs.  pre-defined constant rubik tile colors. 
        /// Also, right side 1D representation of measured and adjusted luminous.  See ...... for 
        /// existing luminous correction.
        /// </summary>
        /// <param name="image"> </param>
        /// <param name="face"> </param>
        private void DrawFaceColorMetrics(Mat image, RubikFace face)
        {

            //Core.rectangle(image, new Point(0, 0), new Point(570, 720), ColorTileEnum.BLACK.cvColor, -1);
            Cv2.Rectangle(image, new Point(0, 0), new Point(570, 720), ColorTileEnum.BLACK.cvColor, -1);

            if (face == null || face.faceRecognitionStatus != FaceRecognitionStatusEnum.SOLVED)
            {
                return;
            }

            // Draw simple grid
            Cv2.Rectangle(image, new Point(-256 + 256, -256 + 400), new Point(256 + 256, 256 + 400), ColorTileEnum.WHITE.cvColor);
            Cv2.Line(image, new Point(0 + 256, -256 + 400), new Point(0 + 256, 256 + 400), ColorTileEnum.WHITE.cvColor);
            Cv2.Line(image, new Point(-256 + 256, 0 + 400), new Point(256 + 256, 0 + 400), ColorTileEnum.WHITE.cvColor);
            //		Core.putText(image, String.format("Luminosity Offset = %4.0f", face.luminousOffset), new Point(0, -256 + 400 - 60), Constants.FontFace, 2, ColorTileEnum.WHITE.cvColor, 2);
            //		Core.putText(image, String.format("Color Error Before Corr = %4.0f", face.colorErrorBeforeCorrection), new Point(0, -256 + 400 - 30), Constants.FontFace, 2, ColorTileEnum.WHITE.cvColor, 2);
            //		Core.putText(image, String.format("Color Error After Corr = %4.0f", face.colorErrorAfterCorrection), new Point(0, -256 + 400), Constants.FontFace, 2, ColorTileEnum.WHITE.cvColor, 2);

            for (int n = 0; n < 3; n++)
            {
                for (int m = 0; m < 3; m++)
                {

                    double[] measuredTileColor = face.measuredColorArray[n][m];
                    //				Log.e(Constants.TAG, "RGB: " + logicalTileArray[n][m].character + "=" + actualTileColor[0] + "," + actualTileColor[1] + "," + actualTileColor[2] + " x=" + x + " y=" + y );
                    double[] measuredTileColorYUV = Util.GetYUVfromRGB(measuredTileColor);
                    //				Log.e(Constants.TAG, "Lum: " + logicalTileArray[n][m].character + "=" + acutalTileYUV[0]);


                    double luminousScaled = measuredTileColorYUV[0] * 2 - 256;
                    double uChromananceScaled = measuredTileColorYUV[1] * 2;
                    double vChromananceScaled = measuredTileColorYUV[2] * 2;

                    string text = Convert.ToString(face.observedTileArray[n][m].symbol);

                    // Draw tile character in UV plane
                    Cv2.PutText(image, text, new Point(uChromananceScaled + 256, vChromananceScaled + 400), Constants.FontFace, 3, face.observedTileArray[n][m].cvColor, 3);

                    // Draw tile characters on INSIDE right side for Y axis for adjusted luminosity.
                    //				Core.putText(image, text, new Point(512 - 40, luminousScaled + 400 + face.luminousOffset), Constants.FontFace, 3, face.observedTileArray[n][m].cvColor, 3);

                    // Draw tile characters on OUTSIDE right side for Y axis as directly measured.
                    Cv2.PutText(image, text, new Point(512 + 20, luminousScaled + 400), Constants.FontFace, 3, face.observedTileArray[n][m].cvColor, 3);
                    //				Log.e(Constants.TAG, "Lum: " + logicalTileArray[n][m].character + "=" + luminousScaled);
                }
            }

            Scalar rubikRed = ColorTileEnum.RED.rubikColor;
            Scalar rubikOrange = ColorTileEnum.ORANGE.rubikColor;
            Scalar rubikYellow = ColorTileEnum.YELLOW.rubikColor;
            Scalar rubikGreen = ColorTileEnum.GREEN.rubikColor;
            Scalar rubikBlue = ColorTileEnum.BLUE.rubikColor;
            Scalar rubikWhite = ColorTileEnum.WHITE.rubikColor;


            // Draw Color Calibration in UV plane as dots    //GET ENUM VALUE()
//            Cv2.Circle(image, new Point(2 * Util.GetYUVfromRGB(rubikRed.Val1) + 256, 2 * Util.GetYUVfromRGB(rubikRed.Val2) + 400), 10, rubikRed, -1);
//            Cv2.Circle(image, new Point(2 * Util.GetYUVfromRGB(rubikOrange.Val1) + 256, 2 * Util.GetYUVfromRGB(rubikOrange.Val2) + 400), 10, rubikOrange, -1);
//            Cv2.Circle(image, new Point(2 * Util.GetYUVfromRGB(rubikYellow.Val1) + 256, 2 * Util.GetYUVfromRGB(rubikYellow.Val2) + 400), 10, rubikYellow, -1);
//            Cv2.Circle(image, new Point(2 * Util.GetYUVfromRGB(rubikGreen.Val1) + 256, 2 * Util.GetYUVfromRGB(rubikGreen.Val2) + 400), 10, rubikGreen, -1);
//            Cv2.Circle(image, new Point(2 * Util.GetYUVfromRGB(rubikBlue.Val1) + 256, 2 * Util.GetYUVfromRGB(rubikBlue.Val2) + 400), 10, rubikBlue, -1);
//            Cv2.Circle(image, new Point(2 * Util.GetYUVfromRGB(rubikWhite.Val1) + 256, 2 * Util.GetYUVfromRGB(rubikWhite.Val2) + 400), 10, rubikWhite, -1);

            // Draw Color Calibration on right side Y axis as dots
//            Cv2.Line(image, new Point(502, -256 + 2 * Util.GetYUVfromRGB(rubikRed.Val0) + 400), new Point(522, -256 + 2 * Util.GetYUVfromRGB(rubikRed.Val0) + 400), rubikRed, 3);
//            Cv2.Line(image, new Point(502, -256 + 2 * Util.GetYUVfromRGB(rubikOrange.Val0) + 400), new Point(522, -256 + 2 * Util.GetYUVfromRGB(rubikOrange.Val0) + 400), rubikOrange, 3);
//            Cv2.Line(image, new Point(502, -256 + 2 * Util.GetYUVfromRGB(rubikGreen.Val0) + 400), new Point(522, -256 + 2 * Util.GetYUVfromRGB(rubikGreen.Val0) + 400), rubikGreen, 3);
//            Cv2.Line(image, new Point(502, -256 + 2 * Util.GetYUVfromRGB(rubikYellow.Val0) + 400), new Point(522, -256 + 2 * Util.GetYUVfromRGB(rubikYellow.Val0) + 400), rubikYellow, 3);
//            Cv2.Line(image, new Point(502, -256 + 2 * Util.GetYUVfromRGB(rubikBlue.Val0) + 400), new Point(522, -256 + 2 * Util.GetYUVfromRGB(rubikBlue.Val0) + 400), rubikBlue, 3);
//            Cv2.Line(image, new Point(502, -256 + 2 * Util.GetYUVfromRGB(rubikWhite.Val0) + 400), new Point(522, -256 + 2 * Util.GetYUVfromRGB(rubikWhite.Val0) + 400), rubikWhite, 3);
        }

        /// <summary>
        /// Draw User Instructions
        /// </summary>
        /// <param name="image"> </param>
        public virtual void DrawUserInstructions(ref Mat image)
        {
            try
            {
                // Create black area for text
                // if (MenuAndParams.userTextDisplay == true)
                // {
                Cv2.Flip(image, image, FlipMode.X);
                Cv2.Rectangle(image, new Point(0, 0), new Point(1270, 60), ColorTileEnum.BLACK.cvColor, -1);
                Cv2.Flip(image, image, FlipMode.X);
                // }

                string inst = "Look at the Cube for Step!";               

                switch (stateModel.appState)
                {

                    case AppStateEnum.START:
                        //  if (MenuAndParams.userTextDisplay == true)
                        //  {
                            Cv2.Flip(image, image, FlipMode.X);
                        //Cv2.Flip(image, image, FlipMode.Y);
                        Cv2.PutText(image, "Show Me The Rubik Cube", new Point(50, 60), Constants.FontFace, 2, ColorTileEnum.WHITE.cvColor, 2);
                       // Cv2.Flip(image, image, FlipMode.Y);
                        Cv2.Flip(image, image, FlipMode.X);
                        //  }

                        if (SaidCantSeeCube == false)
                        {
                          //  WindowsVoice.speak("Show Me The Cube");
                            SaidCantSeeCube = true;
                        }

                        inst = "Show Me The Rubik Cube";
                        instr.Instruction.text = inst;
                        break;

                   case AppStateEnum.GOT_IT:
                        // if (MenuAndParams.userTextDisplay == true)
                        // {
                        Cv2.Flip(image, image, FlipMode.X);
                        Cv2.PutText(image, "OK, Got It", new Point(10, 60), Constants.FontFace, 2, ColorTileEnum.WHITE.cvColor, 2);
                        Cv2.Flip(image, image, FlipMode.X);

                       // WindowsVoice.clearSpeechQueue();
                      //  WindowsVoice.speak("ok got It");

                        SaidSearching = false;
                        SaidRotate = false;
                        //}

                        inst = "OK, Got It";
                        instr.Instruction.text = inst;
                        break;

                    case AppStateEnum.ROTATE_CUBE:
                        // if (MenuAndParams.userTextDisplay == true)
                        // {
                        Cv2.Flip(image, image, FlipMode.X);
                        Cv2.PutText(image, "Please Rotate: " + stateModel.NumObservedFaces, new Point(10, 60), Constants.FontFace, 2, ColorTileEnum.WHITE.cvColor, 2);
                        Cv2.Flip(image, image, FlipMode.X);
                        // }

                        if (SaidRotate == false)
                        {
                        //    WindowsVoice.clearSpeechQueue();
                        //    WindowsVoice.speak("Rotate the Cube.");
                            SaidRotate = true;
                        }

                        inst = "Please Rotate: " + stateModel.NumObservedFaces;
                        instr.Instruction.text = inst;
                        break;

                    case AppStateEnum.SEARCHING:
                        // if (MenuAndParams.userTextDisplay == true)
                        // {
                        Cv2.Flip(image, image, FlipMode.X);
                        Cv2.PutText(image, "Searching for Another Face", new Point(10, 60), Constants.FontFace, 2, ColorTileEnum.WHITE.cvColor, 2);
                        Cv2.Flip(image, image, FlipMode.X);
                        // }

                        if (SaidSearching == false)
                        {
                            //WindowsVoice.clearSpeechQueue();
                          //  WindowsVoice.speak("Searching for another face");
                            SaidSearching = true;
                        }

                        inst = "Searching for Another Face";
                        instr.Instruction.text = inst;
                        break;

                    case AppStateEnum.COMPLETE:
                        // if (MenuAndParams.userTextDisplay == true)
                        // {
                        Cv2.Flip(image, image, FlipMode.X);
                        Cv2.PutText(image, "Cube is Complete and has Good Colors", new Point(10, 60), Constants.FontFace, 2, ColorTileEnum.WHITE.cvColor, 2);
                        Cv2.Flip(image, image, FlipMode.X);
                        //  WindowsVoice.speak("Cube is complete with good colors");
                        // }

                        inst = "Cube is Complete and has Good Colors";
                        instr.Instruction.text = inst;
                        break;

                    case AppStateEnum.WAIT_TABLES:
                        // if (MenuAndParams.userTextDisplay == true)
                        // {
                        Cv2.Flip(image, image, FlipMode.X);
                        Cv2.PutText(image, "Waiting - Preload Next: " + appStateMachine.pruneTableLoaderCount, new Point(10, 60), Constants.FontFace, 2, ColorTileEnum.WHITE.cvColor, 2);
                        Cv2.Flip(image, image, FlipMode.X);
                        // }
                       // WindowsVoice.clearSpeechQueue();
                       // WindowsVoice.speak("Processing Cube Solution");

                        inst = "Waiting - Preload Next: " + appStateMachine.pruneTableLoaderCount;
                        instr.Instruction.text = inst;
                        break;

                    case AppStateEnum.BAD_COLORS:
                        Cv2.Flip(image, image, FlipMode.X);
                        Cv2.PutText(image, "Cube is Complete but has Bad Colors", new Point(10, 60), Constants.FontFace, 2, ColorTileEnum.WHITE.cvColor, 2);
                        Cv2.Flip(image, image, FlipMode.X);
                       // WindowsVoice.clearSpeechQueue();
                       // WindowsVoice.speak("Wrong colors scanned. Please be aware of light reflection on the cube.");

                        inst = "Cube is Complete but has Bad Colors";
                        instr.Instruction.text = inst;
                        break;

                    case AppStateEnum.VERIFIED:
                        // if (MenuAndParams.userTextDisplay == true)
                        // {
                        Cv2.Flip(image, image, FlipMode.X);
                        Cv2.PutText(image, "Cube is Complete and Verified", new Point(10, 60), Constants.FontFace, 2, ColorTileEnum.WHITE.cvColor, 2);
                        Cv2.Flip(image, image, FlipMode.X);
                        // }
                       // WindowsVoice.clearSpeechQueue();
                       // WindowsVoice.speak("Cube is complete and verified.");

                        inst = "Cube is Complete and Verified";
                        instr.Instruction.text = "Look at the cube for instructions.";
                        break;

                    case AppStateEnum.INCORRECT:
                        Cv2.Flip(image, image, FlipMode.X);
                        Cv2.PutText(image, "Cube is Complete but Incorrect: " + stateModel.verificationResults, new Point(10, 60), Constants.FontFace, 2, ColorTileEnum.WHITE.cvColor, 2);
                        Cv2.Flip(image, image, FlipMode.X);
                       // WindowsVoice.clearSpeechQueue();                       
                       // WindowsVoice.speak("Cube is complete but incorrect.");

                        inst = "Cube is Complete but Incorrect: " + stateModel.verificationResults;
                        instr.Instruction.text = inst;
                        break;

                    case AppStateEnum.ERROR:
                        Cv2.Flip(image, image, FlipMode.X);
                        Cv2.PutText(image, "Cube Solution Error: " + stateModel.verificationResults, new Point(10, 60), Constants.FontFace, 2, ColorTileEnum.WHITE.cvColor, 2);
                        Cv2.Flip(image, image, FlipMode.X);
                       // WindowsVoice.clearSpeechQueue();
                       // WindowsVoice.speak("There was an error in the solution.");

                        inst = "Cube Solution Error: " + stateModel.verificationResults;
                        instr.Instruction.text = inst;
                        break;

                    case AppStateEnum.SOLVED:
                        // if (MenuAndParams.userTextDisplay == true)
                        // {

// DONT BOTHER DISPLAYING SOLUTION STRING FOR MILLISECONDS

                     //   Cv2.Flip(image, image, FlipMode.X);
                     //   Cv2.PutText(image, "SOLUTION: ", new Point(10, 60), Constants.FontFace, 2, ColorTileEnum.WHITE.cvColor, 2);
                     //   Cv2.Rectangle(image, new Point(0, 60), new Point(1270, 120), ColorTileEnum.BLACK.cvColor, -1);
                     //   Cv2.PutText(image, "" + stateModel.solutionResults, new Point(10, 120), Constants.FontFace, 2, ColorTileEnum.WHITE.cvColor, 2);
                     //   Cv2.Flip(image, image, FlipMode.X);
                        // }
                        break;

                    case AppStateEnum.ROTATE_FACE:
                        string moveNumonic = stateModel.solutionResultsArray[stateModel.solutionResultIndex];
                     //   Debug.Log("Move:" + moveNumonic + ":");
                        StringBuilder moveDescription = new StringBuilder("Rotate the face with the ");
                        switch (moveNumonic[0])
                        {
                            case 'U':
                                //moveDescription.Append("Top Face");
                                moveDescription.Append("Green");
                                break;
                            case 'D':
                                //moveDescription.Append("Down Face");
                                moveDescription.Append("Blue");
                                break;
                            case 'L':
                                //moveDescription.Append("Left Face");
                                moveDescription.Append("Orange");
                                break;
                            case 'R':
                                //moveDescription.Append("Right Face");
                                moveDescription.Append("Red");
                                break;
                            case 'F':
                                //moveDescription.Append("Front Face");
                                moveDescription.Append("Yellow");
                                break;
                            case 'B':
                                //moveDescription.Append("Back Face");
                                moveDescription.Append("White");
                                break;
                        }                       
                        moveDescription.Append(" tile at it's centre");
                        if (moveNumonic.Length == 1)
                        {
                            moveDescription.Append(" 90 Degrees Clockwise");
                        }
                        else if (moveNumonic[1] == '2')
                        {
                            moveDescription.Append(" 180 Degrees Clockwise");
                        }
                        else if (moveNumonic[1] == '\'')
                        {
                            moveDescription.Append(" 90 Degrees Anti-Clockwise");                            
                        }
                        else
                        {
                            moveDescription.Append("?");
                        }

                        // if (MenuAndParams.userTextDisplay == true)
                        // {
                        Cv2.Flip(image, image, FlipMode.X);
                        Cv2.PutText(image, moveDescription.ToString(), new Point(10, 60), Constants.FontFace, 2, ColorTileEnum.WHITE.cvColor, 2);                     
                        Cv2.Flip(image, image, FlipMode.X);

                        inst = "Step " + VideoCapture.InstInd + ": ";
                        instr.InstructionNumber.text = inst;
                       
                        // }

                        inst += moveDescription.ToString();                        

                        if (SaidInst == false)
                        {
                         //   WindowsVoice.clearSpeechQueue();                               
                            //WindowsVoice.speak(moveDescription.ToString());
                           // WindowsVoice.speak(inst);                           
                            
                            SaidInst = true;
                        }

                        instr.Instruction.text = moveDescription.ToString();
                        instr.Instruction.text = instr.Instruction.text.Replace("tile", "tile" + Environment.NewLine);                       

                        SaidWaiting = false;

                        break;

                    case AppStateEnum.WAITING_MOVE:
                        // if (MenuAndParams.userTextDisplay == true)
                        // {
                        Cv2.Flip(image, image, FlipMode.X);
                       // Cv2.PutText(image, "Waiting for move to be completed", new Point(10, 60), Constants.FontFace, 2, ColorTileEnum.WHITE.cvColor, 2);
                        Cv2.Flip(image, image, FlipMode.X);
                        // }

                        if (SaidWaiting == false)
                        {
                           // WindowsVoice.speak("I can't see the cube.");
                            SaidWaiting = true;
                        }

                        inst = "Look at Cube for next step!";
                        instr.InstructionNumber.text = inst;
                        break;

                    case AppStateEnum.DONE:
                        // if (MenuAndParams.userTextDisplay == true)
                        // {
                        Cv2.Flip(image, image, FlipMode.X);
                        Cv2.PutText(image, "Cube should now be solved.", new Point(10, 60), Constants.FontFace, 2, ColorTileEnum.WHITE.cvColor, 2);
                        Cv2.Flip(image, image, FlipMode.X);

                        if (SaidSolved == false)
                        {
                           // WindowsVoice.clearSpeechQueue();
                           // WindowsVoice.speak("The Cube should now be solved.");
                            SaidSolved = true;
                        }
                        // }

                        inst = "The Cube should now be solved.";
                        instr.InstructionNumber.text = inst;
                        instr.Instruction.text = "Thank You For Your Time :)";
                        break;

                    default:
                        //if (MenuAndParams.userTextDisplay == true)
                        //{
                            Cv2.Flip(image, image, FlipMode.X);
                            Cv2.PutText(image, "Oops", new Point(0, 60), Constants.FontFace, 5, ColorTileEnum.WHITE.cvColor, 5);
                            Cv2.Flip(image, image, FlipMode.X);

                       // WindowsVoice.clearSpeechQueue();
                       // WindowsVoice.speak("A problem has occured.");
                        // }

                        inst = "A problem has occured.";
                        instr.InstructionNumber.text = inst;
                        break;
                }

                VideoCapture.Instruction = inst;
                NI.RenderInstruction(inst);                

                // User indicator that tables have been computed.
                //   Cv2.Flip(image, image, FlipMode.X);
                Cv2.Line(image, new Point(0, 0), new Point(1270, 0), appStateMachine.pruneTableLoaderCount < 12 ? ColorTileEnum.RED.cvColor : ColorTileEnum.GREEN.cvColor, 2);
             //   Cv2.Flip(image, image, FlipMode.X);
            }
            catch(Exception e)
            {
                Debug.Log("Instruction Exception: " +e);
            }
        }
    }
}

