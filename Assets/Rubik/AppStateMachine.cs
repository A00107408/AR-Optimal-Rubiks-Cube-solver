/* <summary
 * 
 * Augmented Reality Rubik Cube Application
 * A.I.T 2018
 * A00107408
 * Masters by Research
 * 
 * File Description: 
 * This class interprets the recognised Rubik Faces and determines how primary 
 * application state should change.There are two state machines contained in this 
 * class: the Stable Face Recognizer State Machine, and the Application State Machine. 
 * 
 * Acknowledgments:
 * Original Author: Steven P. Punte (aka Android Steve : android.steve@cl-sw.com)
 * Date: April 25th 2015
 *   
 * <summary> */

namespace Rubik
{
    using System;
    using System.IO;
    using UnityEngine;
    using CSML;

    using Search = KociembaTwoPhase.Search;
    using Tools = KociembaTwoPhase.Tools;

    using AppStateEnum = Constants.AppStateEnum;
    using FaceNameEnum = Constants.FaceNameEnum;
    using GestureRecogniztionStateEnum = Constants.GestureRecogniztionStateEnum;
    using FaceRecognitionStatusEnum = RubikFace.FaceRecognitionStatusEnum;
    using ColorTileEnum = Constants.ColorTileEnum;

    //  using Search = KociembaTwoPhase.Search;
    //  using Tools = KociembaTwoPhase.Tools;


    /// <summary>
    /// @author android.steve@cl-sw.com
    /// 
    /// </summary>
    public class AppStateMachine
    {
        public StateModel stateModel; //Made public to allow user control of instruction delivery

        public CubeLocationBeep CLB;
        public CubeFoundDing CFD;
        private ChangeFrameColor CFC;

        private GameObject GObj1, GObj2, GObj3;


        // 12 tables need to be generated.  When count is 12, tables are valid.  Used by prune table loader.  
        public int pruneTableLoaderCount = 0;

        // Allows for more pleasing user interface
        private int gotItCount = 0;

        // Candidate Rubik Face to be possible adopted by Stable Face Recognizer state machine.
        private RubikFace candidateRubikFace = null;

        // Consecutive counts use by Stable Face Recognizer state machine.
        private int consecutiveCandiateRubikFaceCount = 0;

        // Use to determine a New Stable Face.
        private RubikFace lastNewStableRubikFace = null;

        // After all six faces have been seen, allow one more rotation to return cube to original orientation.
        private bool allowOneMoreRotation = false;

        // Set when we want to reset state, but do it synchronously in the frame thread.
        private bool scheduleReset = false;

        // Set when we want to recall a state from file, but do it synchronously in the frame thread.
        private bool scheduleRecall = false;

        private string CubeString;

        private DateTime TestStartTime, Times, PrevTime;
        private TimeSpan Durations, TestDuration;

        private string CollatedTimesPath = @"c:\RubikTestData\CollatedTimes.csv";   //Actual Day Clock Times.
        private string CollatedTTIPath = @"c:\RubikTestData\CollatedTTI.csv";       //mins:sec between instruction. (Time To Instruction)
        private string CollatedTTCPath = @"c:\RubikTestData\CollatedTTC.csv";       //Test duration. (Time To Completion)

        private string TestSubjectsPath = @"c:\RubikTestData\Individual\";
        private string FileName = "";

        private string FrameRatePath = @"c:\RubikTestData\FrameRates\";
        private string FrameRateFileName = "";

        private string HeadTrackingPath = @"c:\RubikTestData\MetaHeadTracking\";
        private string HeadTrakingFileName = "";

        /// <summary>
        /// Application State Machine Constructor
        /// </summary>
        /// <param name="stateModel"> </param>
        public AppStateMachine(StateModel stateModel)
        {
            this.stateModel = stateModel;

            GObj1 = GameObject.Find("MetaCameraRig");           
            CLB = GObj1.GetComponent<CubeLocationBeep>();

            GObj2 = GameObject.Find("Quad");
            CFD = GObj2.GetComponent<CubeFoundDing>();

            GObj3 = GameObject.Find("Cube");
            CFC = GObj3.GetComponent<ChangeFrameColor>();
        }

        /// <summary>
        /// On Face Event
        /// 
        /// This function is called any time a Rubik Face is recognized, even if it may be 
        /// inaccurate. Further filtering is performed in this function. The purpose
        /// of this state machine is to detect a reliable stable face, and to make 
        /// the event calls of onFace and offFace into the app state machine.
        /// </summary>
        /// <param name="rubikFace"> 
        ///  </param>
        public virtual void OnFaceEvent(RubikFace rubikFace)
        {

            // Threshold for the number of times a face must be seen in order to declare it stable.
            const int consecutiveCandidateCountThreashold = 12;
            VideoCapture.ConsecutiveStableCountThreshold = (float)consecutiveCandiateRubikFaceCount;

           // Debug.Log("\n rubikFace.FaceRecognitionStatusEnum: " + rubikFace.faceRecognitionStatus + " stateModel.gestureState: " + stateModel.gestureRecogniztionState +" Candidate= " + (candidateRubikFace == null ? 0 : candidateRubikFace.myHashCode) + " NewFace= " + (rubikFace == null ? 0 : rubikFace.myHashCode));
           // Debug.Log("\n");

            // Reset Application State.  All past is forgotten.
            /*=         if (scheduleReset == true)
                        {
                            gotItCount = 0;
                            scheduleReset = false;
                            candidateRubikFace = null;
                            consecutiveCandiateRubikFaceCount = 0;
                            lastNewStableRubikFace = null;
                            allowOneMoreRotation = false;
                            stateModel.reset();
                        }*/

            // Reset Application State, and then recall app state from file.
            /*=         if (scheduleRecall == true)
                        {
                            gotItCount = 0;
                            scheduleRecall = false;
                            candidateRubikFace = null;
                            consecutiveCandiateRubikFaceCount = 0;
                            lastNewStableRubikFace = null;
                            allowOneMoreRotation = false;
                            stateModel.reset();
                            stateModel.recallState();
                            stateModel.appState = AppStateEnum.COMPLETE; // Assumes state stored in file is complete.
                        }*/

            // Sometimes, we want state to change simply on frame events.  
            // This has the exact same event model as onFaceEvent().
            OnFrameEvent();

            switch (stateModel.gestureRecogniztionState)
            {
 
                case GestureRecogniztionStateEnum.UNKNOWN:
                    if (rubikFace.faceRecognitionStatus == FaceRecognitionStatusEnum.SOLVED)
                    {
                        CFC.CubeVisible = false;

                        stateModel.gestureRecogniztionState = GestureRecogniztionStateEnum.PENDING;
                        candidateRubikFace = rubikFace;
                        consecutiveCandiateRubikFaceCount = 0;

                        // Create a new Kalman Filter
//=                        stateModel.kalmanFilter = new KalmanFilter();
                        // Create a new Kalman Filter ALSM machine
//=                        stateModel.kalmanFilterALSM = new KalmanFilterALSM();
                    }
                    else
                    {
                        CFC.CubeVisible = false;  // stay in unknown state.
                    }
                    break;
                        
                case GestureRecogniztionStateEnum.PENDING:
                    if (rubikFace.faceRecognitionStatus == FaceRecognitionStatusEnum.SOLVED)
                    {

                        if (rubikFace.myHashCode == candidateRubikFace.myHashCode)
                        {

                            if (consecutiveCandiateRubikFaceCount > consecutiveCandidateCountThreashold)
                            {

                                if (lastNewStableRubikFace == null || rubikFace.myHashCode != lastNewStableRubikFace.myHashCode)
                                {
                                    lastNewStableRubikFace = rubikFace;
                                    stateModel.gestureRecogniztionState = GestureRecogniztionStateEnum.NEW_STABLE;
                                    OnNewStableFaceEvent(rubikFace);
                                    OnStableFaceEvent(candidateRubikFace);

                                    CFC.CubeVisible = true;
                                    CFD.PlayDing();
                                }

                                else
                                {
                                    CFC.CubeVisible = true;
                                    CFD.PlayDing();

                                    stateModel.gestureRecogniztionState = GestureRecogniztionStateEnum.STABLE;
                                    OnStableFaceEvent(candidateRubikFace);                                                                      
                                }
                            }
                            else
                                consecutiveCandiateRubikFaceCount++;

                                CFC.CubeVisible = true;
                                CLB.PlayBeep(consecutiveCandiateRubikFaceCount);
                        }
                        //else if(false)
                        //	;// =+= add partial match here
                        else
                        {
                            stateModel.gestureRecogniztionState = GestureRecogniztionStateEnum.UNKNOWN;

                            CFC.CubeVisible = false;

                            // =+= triplicated
                            //=                         stateModel.kalmanFilter = null;
                            //=                         if (stateModel.kalmanFilterALSM != null)
                            //=                             stateModel.kalmanFilterALSM.calculateResults();
                            //=                         stateModel.kalmanFilterALSM = null;
                        }
                    }
                    else
                    {
                        stateModel.gestureRecogniztionState = GestureRecogniztionStateEnum.UNKNOWN;

                        CFC.CubeVisible = false;
                        // =+= triplicated
                        //=                     stateModel.kalmanFilter = null;
                        //=                     if (stateModel.kalmanFilterALSM != null)
                        //=                        stateModel.kalmanFilterALSM.calculateResults();
                        //=                     stateModel.kalmanFilterALSM = null;
                    }
                    break;

                    
                case GestureRecogniztionStateEnum.STABLE:
                    if (rubikFace.faceRecognitionStatus == FaceRecognitionStatusEnum.SOLVED)
                    {

                        if (rubikFace.myHashCode == candidateRubikFace.myHashCode)
                            CFC.CubeVisible = true; // Just stay in this state
                              // else if(false)
                              // ; // =+= add partial match here
                        else
                        {
                            stateModel.gestureRecogniztionState = GestureRecogniztionStateEnum.PARTIAL;
                            consecutiveCandiateRubikFaceCount = 0;
                            CFC.CubeVisible = false;
                        }
                    }
                    else
                    {
                        stateModel.gestureRecogniztionState = GestureRecogniztionStateEnum.PARTIAL;
                        consecutiveCandiateRubikFaceCount = 0;
                        CFC.CubeVisible = false;
                    }
                    break;


                case GestureRecogniztionStateEnum.PARTIAL:


                    if (rubikFace.faceRecognitionStatus == FaceRecognitionStatusEnum.SOLVED)
                    {

                        if (rubikFace.myHashCode == candidateRubikFace.myHashCode)
                        {
                            CFC.CubeVisible = true;
                            if (lastNewStableRubikFace != null && rubikFace.myHashCode == lastNewStableRubikFace.myHashCode)
                            {
                                stateModel.gestureRecogniztionState = GestureRecogniztionStateEnum.NEW_STABLE;
                                CFC.CubeVisible = true;
                            }
                            else
                                stateModel.gestureRecogniztionState = GestureRecogniztionStateEnum.STABLE;
                        }
                        //else if(false)
                        //	; // =+= add partial match here
                        else
                        {
                            if (consecutiveCandiateRubikFaceCount > consecutiveCandidateCountThreashold)
                            {
                                CFC.CubeVisible = false;
                                stateModel.gestureRecogniztionState = GestureRecogniztionStateEnum.UNKNOWN;
                                OffNewStableFaceEvent();
                                OffStableFaceEvent();
                            }
                            else
                                consecutiveCandiateRubikFaceCount++; // stay in partial state
                                CFC.CubeVisible = true;
                        }
                    }
                    else
                    {
                        if (consecutiveCandiateRubikFaceCount > consecutiveCandidateCountThreashold)
                        {
                            CFC.CubeVisible = false;
                            stateModel.gestureRecogniztionState = GestureRecogniztionStateEnum.UNKNOWN;
                            OffNewStableFaceEvent();
                            OffStableFaceEvent();
                        }
                        else
                            consecutiveCandiateRubikFaceCount++; // stay in partial state
                            CFC.CubeVisible = true;
                    }
                    break;

                case GestureRecogniztionStateEnum.NEW_STABLE:
                    if (rubikFace.faceRecognitionStatus == FaceRecognitionStatusEnum.SOLVED)
                    {

                        if (rubikFace.myHashCode == candidateRubikFace.myHashCode)
                        {
                            CFC.CubeVisible = true;  // Just stay in this state
                        }
                        //else if(false)
                        //    ; // =+= add partial match here
                        else
                        {
                            CFC.CubeVisible = false;
                            stateModel.gestureRecogniztionState = GestureRecogniztionStateEnum.PARTIAL;
                            consecutiveCandiateRubikFaceCount = 0;
                        }
                    }
                    else
                    {
                        CFC.CubeVisible = false;
                        stateModel.gestureRecogniztionState = GestureRecogniztionStateEnum.PARTIAL;
                        consecutiveCandiateRubikFaceCount = 0;
                    }
                    break;
            }
           
        }

        /// <summary>
        /// On Stable Rubik Face Recognized
        /// 
        /// This function is called ever frame when a valid and stable Rubik Face is recognized.
        /// </summary>
        /// <param name="myHashCode"> 
        ///  </param>
        private void OnStableFaceEvent(RubikFace rubikFace)
        {

           // Debug.Log("+onStableRubikFaceRecognized: last=" + (lastNewStableRubikFace == null ? 0 : lastNewStableRubikFace.myHashCode) + " new=" + rubikFace.myHashCode);

            switch (stateModel.appState)
            {

                case AppStateEnum.SOLVED:

                    // Fist post scan look at cube triggers test start                                                      // TEST START
                    stateModel.appState = AppStateEnum.ROTATE_FACE;

                    VideoCapture.TestStarted = true;
                                       
                    TestStartTime = DateTime.Now;
                    PrevTime = TestStartTime;
                    Debug.Log("Start Time: " + TestStartTime);

                    FileName = FrameRateFileName = HeadTrakingFileName = DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss");

                    TestSubjectsPath   = TestSubjectsPath + FileName + ".csv";
                    FrameRatePath = FrameRatePath + FrameRateFileName + ".csv";
                    HeadTrackingPath = HeadTrackingPath + HeadTrakingFileName + ".csv";

                 /*   using (StreamWriter TestSubjectData = File.AppendText(TestSubjectsPath))
                    {
                        TestSubjectData.Write(TestStartTime);
                        TestSubjectData.Write(",");
                        TestSubjectData.Write("0");
                        TestSubjectData.Write("\r\n");
                        TestSubjectData.Close();
                    }

                    using (StreamWriter CollatedTimesWriter = File.AppendText(CollatedTimesPath))
                    {
                        CollatedTimesWriter.Write(TestStartTime);
                        CollatedTimesWriter.Write(",");
                        CollatedTimesWriter.Close();
                    }

                    using (StreamWriter CollatedDurationsWriter = File.AppendText(CollatedTTIPath))
                    {
                        CollatedDurationsWriter.Write("0");
                        CollatedDurationsWriter.Write(",");
                        CollatedDurationsWriter.Close();
                    }

                    using (StreamWriter FRWriter = File.AppendText(FrameRatePath))
                    {
                        FRWriter.Write(VideoCapture.FPS);
                        FRWriter.Write(",");
                        FRWriter.Write(VideoCapture.InstDelRate);
                        FRWriter.Write(",");
                        FRWriter.Write(VideoCapture.InstDelTime);
                        FRWriter.Write("\r\n");
                        FRWriter.Close();
                    }

                    string poseRotation = VideoCapture.HeadTracking.transform.rotation.ToString();
                    poseRotation = poseRotation.Trim(new Char[] { '(', ')' });

                    string posePosition = VideoCapture.HeadTracking.transform.position.ToString();
                    posePosition = posePosition.Trim(new Char[] { '(', ')' });

                    using (StreamWriter HeadPoseWriter = File.AppendText(HeadTrackingPath))
                    {
                        HeadPoseWriter.Write(poseRotation);
                        HeadPoseWriter.Write(", ,");
                       
                        HeadPoseWriter.Write(posePosition);
                        HeadPoseWriter.Write("\r\n");
                    }*/

                    break;

                case AppStateEnum.WAITING_MOVE:
                    stateModel.appState = AppStateEnum.ROTATE_FACE;

                    
                    stateModel.solutionResultIndex++;
                    VideoCapture.InstInd++;


                    if (stateModel.solutionResultIndex == stateModel.solutionResultsArray.Length-1)
                    {
                        stateModel.appState = AppStateEnum.DONE;
                        
                        Times = DateTime.Now;

                        Durations = Times - PrevTime;
                        //Collate Last instruction duration to collective durations file.
                     /*   using (StreamWriter CollatedDurationsWriter = File.AppendText(CollatedTTIPath))
                        {
                            CollatedDurationsWriter.Write(Durations);
                            CollatedDurationsWriter.Write("\r\n");
                            CollatedDurationsWriter.Close();
                        }
                                                                   
                        //Collate Test End Time (Last Instruction) to collective times file.
                        using (StreamWriter CollatedTimesWriter = File.AppendText(CollatedTimesPath))
                        {
                            CollatedTimesWriter.Write(Times);
                            CollatedTimesWriter.Write("\r\n");
                            CollatedTimesWriter.Close();
                        }

                        using (StreamWriter TestSubjectData = File.AppendText(TestSubjectsPath))
                        {
                            TestSubjectData.Write(Times);
                            TestSubjectData.Write(",");
                            TestSubjectData.Write(Durations);                            
                            TestSubjectData.Write("\r\n");
                            TestSubjectData.Close();
                        }

                        Durations = Times - TestStartTime;
                        //Collate test duration to colletive .csv file for all test subjects to facilitate analysis.
                        using(StreamWriter CollatedTTCWriter = File.AppendText(CollatedTTCPath))
                        {
                            CollatedTTCWriter.Write(Durations);
                            CollatedTTCWriter.Write("\r\n");
                            CollatedTTCWriter.Close();
                        }

                        using (StreamWriter TestSubjectData = File.AppendText(TestSubjectsPath))
                        {                           
                            TestSubjectData.Write(Durations);
                            TestSubjectData.Write("\r\n");
                            TestSubjectData.Close();
                        }

                        using (StreamWriter FRWriter = File.AppendText(FrameRatePath))
                        {
                            FRWriter.Write(VideoCapture.FPS);
                            FRWriter.Write(",");
                            FRWriter.Write(VideoCapture.InstDelRate);
                            FRWriter.Write(",");
                            FRWriter.Write(VideoCapture.InstDelTime);
                            FRWriter.Write("\r\n");
                            FRWriter.Close();
                        }

                        //Record Head Tracking Data for Meta test subjects
                        poseRotation = VideoCapture.HeadTracking.transform.rotation.ToString();
                        poseRotation = poseRotation.Trim(new Char[] { '(', ')' });

                        posePosition = VideoCapture.HeadTracking.transform.position.ToString();
                        posePosition = posePosition.Trim(new Char[] { '(', ')' });

                        using (StreamWriter HeadPoseWriter = File.AppendText(HeadTrackingPath))
                        {
                            HeadPoseWriter.Write(poseRotation);
                            HeadPoseWriter.Write(", ,");

                            HeadPoseWriter.Write(posePosition);
                            HeadPoseWriter.Write("\r\n");
                        }*/

                    }
                    else
                    {
                        Times = DateTime.Now;
                        Durations = Times - PrevTime;
                        PrevTime = DateTime.Now;
                        Debug.Log("Time to Instruction: " + Durations);

                        //Collate instruction Time to file.
                     /*   using (StreamWriter CollatedTimesWriter = File.AppendText(CollatedTimesPath))
                        {
                            CollatedTimesWriter.Write(Times);
                            CollatedTimesWriter.Write(",");
                            CollatedTimesWriter.Close();
                        }

                        //Collate instruction Durations to file.
                        using (StreamWriter CollatedDurationsWriter = File.AppendText(CollatedTTIPath))
                        {
                            CollatedDurationsWriter.Write(Durations);
                            CollatedDurationsWriter.Write(",");
                            CollatedDurationsWriter.Close();
                        }

                        using (StreamWriter TestSubjectData = File.AppendText(TestSubjectsPath))
                        {
                            TestSubjectData.Write(Times);
                            TestSubjectData.Write(",");
                            TestSubjectData.Write(Durations);
                            TestSubjectData.Write("\r\n");
                            TestSubjectData.Close();
                        }

                        using (StreamWriter FRWriter = File.AppendText(FrameRatePath))
                        {
                            FRWriter.Write(VideoCapture.FPS);
                            FRWriter.Write(",");
                            FRWriter.Write(VideoCapture.InstDelRate);
                            FRWriter.Write(",");
                            FRWriter.Write(VideoCapture.InstDelTime);
                            FRWriter.Write("\r\n");
                            FRWriter.Close();
                        }

                        poseRotation = VideoCapture.HeadTracking.transform.rotation.ToString();
                        poseRotation = poseRotation.Trim(new Char[] { '(', ')' });

                        posePosition = VideoCapture.HeadTracking.transform.position.ToString();
                        posePosition = posePosition.Trim(new Char[] { '(', ')' });

                        using (StreamWriter HeadPoseWriter = File.AppendText(HeadTrackingPath))
                        {
                            HeadPoseWriter.Write(poseRotation);
                            HeadPoseWriter.Write(", ,");

                            HeadPoseWriter.Write(posePosition);
                            HeadPoseWriter.Write("\r\n");
                        }*/
                    }
                    break;

                default:
                    break;
            }
        }
         
        /// <summary>
        /// Off Stable Rubik Face Recognized
        /// 
        /// This function is called ever frame when there is no longer a stable face.
        /// </summary>
        /// <param name="myHashCode"> 
        ///  </param>
        public virtual void OffStableFaceEvent()
        {

           // Debug.Log("-offStableRubikFaceRecognized: previous=" + lastNewStableRubikFace.myHashCode);

            switch (stateModel.appState)
            {

                case AppStateEnum.ROTATE_FACE:
                    stateModel.appState = AppStateEnum.WAITING_MOVE;
                    break;

                default:
                    break;
            }

            // =+= triplicated
//=            stateModel.kalmanFilter = null;
//=            if (stateModel.kalmanFilterALSM != null)
//=            {
//=                stateModel.kalmanFilterALSM.calculateResults();
//=            }
//=            stateModel.kalmanFilterALSM = null;
        }

        /// <summary>
        /// On New Stable Rubik Face Recognized
        /// 
        /// This function is called when a new and different stable Rubik Face is recognized.
        /// In other words, this should be a different face than the last stable face, 
        /// however, it will be called on frame rate while new stable Rubik Face is 
        /// recognized in image.
        /// </summary>
        /// <param name="rubikFaceHashCode"> </param>
        private void OnNewStableFaceEvent(RubikFace candidateRubikFace)
        {
            Annotation.SaidInst = false;
            //Debug.Log("+onNewStableRubikFaceRecognized  Previous State =" + stateModel.appState);

            switch (stateModel.appState)
            {

                case AppStateEnum.START:
                    stateModel.Adopt(candidateRubikFace);

                    // In camera calibration diagnostic mode, don't progress states.
//=                    if (MenuAndParams.cameraCalDiagMode == false)
//=                    {
                        stateModel.appState = AppStateEnum.GOT_IT;
                        gotItCount = 0;
//=                    }
                    break;

                case AppStateEnum.SEARCHING:
                    stateModel.Adopt(candidateRubikFace);

                    // Have not yet seen all six sides.
                    if (stateModel.ThereAfullSetOfFaces == false)
                    {
                        stateModel.appState = AppStateEnum.GOT_IT;
                        allowOneMoreRotation = true;
                        gotItCount = 0;
                    }

                    // Do one more turn so cube returns to original orientation.
                    else if (allowOneMoreRotation == true)
                    {
                        stateModel.appState = AppStateEnum.GOT_IT;
                        allowOneMoreRotation = false;
                        gotItCount = 0;
                    }

                    // All faces have been observed, and cube is back in original position.  Begin processing of cube.
                    else
                    {
                        stateModel.appState = AppStateEnum.COMPLETE;
                    }
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Off New Stable Rubik Face Recognized
        /// 
        /// This is called when the new stable face is gone.
        /// </summary>
        /// <param name="rubikFaceHashCode"> </param>
        private void OffNewStableFaceEvent()                                                            // Eoghan- Not using pilot cube for now.
        {

          //  Debug.Log("-offNewStableRubikFaceRecognition  Previous State =" + stateModel.appState);


            switch (stateModel.appState)
            {

                case AppStateEnum.ROTATE_CUBE:
                    stateModel.appState = AppStateEnum.SEARCHING;
                    break;

                default:
                    break;
            }

            // Rotate Cube Model so that it matches physical model as user is requested to rotate cube.
            if (stateModel.adoptFaceCount <= 6)
            {

                // =+= x and z matrix can be made static, but performance here hardly matters.
                if (stateModel.adoptFaceCount % 2 == 0)
                {
                    // Rotate cube -90 degrees along X axis
                    float[] xRotationMatrix = new float[16];

 //                   Matrix.setIdentityM(xRotationMatrix, 0);
 //                   Matrix.rotateM(xRotationMatrix, 0, -90f, 1.0f, 0.0f, 0.0f);
                    float[] z = new float[16];
 //                   Matrix.multiplyMM(z, 0, xRotationMatrix, 0, stateModel.additionalGLCubeRotation, 0);
 //                   Array.Copy(z, 0, stateModel.additionalGLCubeRotation, 0, stateModel.additionalGLCubeRotation.length);
                }
                else
                {
                    // Rotate cube +90 degrees along Z axis.
                    float[] zRotationMatrix = new float[16];
 //                   Matrix.setIdentityM(zRotationMatrix, 0);
 //                   Matrix.rotateM(zRotationMatrix, 0, +90f, 0.0f, 0.0f, 1.0f);
                    float[] z = new float[16];
 //                   Matrix.multiplyMM(z, 0, zRotationMatrix, 0, stateModel.additionalGLCubeRotation, 0);
 //                   Array.Copy(z, 0, stateModel.additionalGLCubeRotation, 0, stateModel.additionalGLCubeRotation.length);
                }
            }

        }

        /// <summary>
        /// On Frame State Changes
        /// 
        /// This function is call every time function onFaceEvent() is called, and thus has
        /// the identical event model.
        /// 
        /// It appears handy to have some controller state changes advanced on the periodic frame rate.
        /// Unfortunately, the rate that is function is called is dependent upon the bulk of opencv
        /// processing which can vary with the background.
        /// </summary>
        private void OnFrameEvent()
        {
            CFC.ChangeColor();

            switch (stateModel.appState)
            {

                case AppStateEnum.WAIT_TABLES:                   
                    if (pruneTableLoaderCount == 12)
                    {
                        stateModel.appState = AppStateEnum.VERIFIED;                       
                    }
                    break;


                case AppStateEnum.GOT_IT:
                    if (gotItCount < 3)
                    {
                        gotItCount++;
                    }
                    else
                    {
                        stateModel.appState = AppStateEnum.ROTATE_CUBE;
                        gotItCount = 0;
                    }
                    break;


                case AppStateEnum.COMPLETE:

                    
                    // Re-asses color mapping.  Use algorithm that evaluates entire cube at once.
                    try
                    {
                        //new ColorRecognition.Cube(stateModel).CubeTileColorRecognition();
                    }catch(Exception e)
                    {
                        Debug.Log("\nCube Whole Has Bad Colors: " + e);
                        Debug.Log("\n ");
                    }

                    // Check if color mapping meets certain criteria.  Note, above algorithm should result in this always be being true.
                     /*    if (Util.IsTileColorsValid(stateModel) == false)
                         {
                            Debug.Log("BAD COLORS");
                              
                            stateModel.appState = AppStateEnum.BAD_COLORS;                    
                            break;
                         }


                    // Create/update transformed tile array
                    // =+= This logic duplicated in class StateModel
                    // stateModel.nameRubikFaceMap.get(FaceNameEnum.UP).transformedTileArray = stateModel.nameRubikFaceMap.get(FaceNameEnum.UP).observedTileArray.clone();
                     stateModel.nameRubikFaceMap[FaceNameEnum.UP].transformedTileArray = (ColorTileEnum[][])stateModel.nameRubikFaceMap[FaceNameEnum.UP].observedTileArray.Clone();

                     stateModel.nameRubikFaceMap[FaceNameEnum.RIGHT].transformedTileArray = Util.GetTileArrayRotatedClockwise(stateModel.nameRubikFaceMap[FaceNameEnum.RIGHT].observedTileArray);
                     stateModel.nameRubikFaceMap[FaceNameEnum.FRONT].transformedTileArray = Util.GetTileArrayRotatedClockwise(stateModel.nameRubikFaceMap[FaceNameEnum.FRONT].observedTileArray);
                     stateModel.nameRubikFaceMap[FaceNameEnum.DOWN].transformedTileArray = Util.GetTileArrayRotatedClockwise(stateModel.nameRubikFaceMap[FaceNameEnum.DOWN].observedTileArray);
                     stateModel.nameRubikFaceMap[FaceNameEnum.LEFT].transformedTileArray = Util.GetTileArrayRotated180(stateModel.nameRubikFaceMap[FaceNameEnum.LEFT].observedTileArray);
                     stateModel.nameRubikFaceMap[FaceNameEnum.BACK].transformedTileArray = Util.GetTileArrayRotated180(stateModel.nameRubikFaceMap[FaceNameEnum.BACK].observedTileArray);
                     */

                     // Build string representation of cube state
                     CubeString = stateModel.StringRepresentationOfCube;

                     CubeString = MetaHackCubeString(CubeString);


                    //Super Flip Cube State String: UBULURUFURURFRBRDRFUFLFRFDFDFDLDRDBDLULBLFLDLBUBRBLBDB
                    //string cubeString = "UBULURUFURURFRBRDRFUFLFRFDFDFDLDRDBDLULBLFLDLBUBRBLBDB";

                    // Perform twophase algorithm verification check.
                    // stateModel.verificationResults = Tools.Verify(CubeString);                    
                    
                    // If OK, then make sure prune tables have been built.
                    if (stateModel.verificationResults == 0)
                    {
                        stateModel.appState = AppStateEnum.WAIT_TABLES;                       
                    }
                    else
                    {
                        stateModel.appState = AppStateEnum.INCORRECT;                        
                    }           
                    
                 //   string stringErrorMessage = Util.GetTwoPhaseErrorString((char)(stateModel.verificationResults * -1 + '0'));
                    Debug.Log("Cube String Rep: " + CubeString);
                 //   Debug.Log("Verification Results: (" + stateModel.verificationResults + ") " + stringErrorMessage);
                  
                    break;


                case AppStateEnum.VERIFIED:
                    
                    //string cubeString2 = "UBULURUFURURFRBRDRFUFLFRFDFDFDLDRDBDLULBLFLDLBUBRBLBDB";
                    string cubeString2 = CubeString;
                   
                    // Returns 0 if solution computed
                   
                    stateModel.solutionResults = Search.Solution(cubeString2, 25, 8, false);                  
                    Debug.Log("Solution Results: " + stateModel.solutionResults);

                    if (stateModel.solutionResults.Contains("Error"))
                    {
                        //char solutionCode = stateModel.solutionResults.charAt(stateModel.solutionResults.length() - 1);
                        char solutionCode = stateModel.solutionResults[stateModel.solutionResults.Length - 1];
                        stateModel.verificationResults = solutionCode - '0';
                        Debug.Log("Solution Error: " + Util.GetTwoPhaseErrorString(solutionCode));
                        stateModel.appState = AppStateEnum.ERROR;
                    }
                    else
                    {
                        stateModel.appState = AppStateEnum.SOLVED;
                    }                    

                    break;


                case AppStateEnum.SOLVED:                    
                    stateModel.solutionResultsArray = stateModel.solutionResults.Split(' ');                        
                    stateModel.solutionResultIndex = 0;

                    //Dont start instructions until test subject looks at cube for the first time.
                    //Cube has been scanned by accessor.
                    if (VideoCapture.StartInstructions == true)
                    {
                        stateModel.appState = AppStateEnum.ROTATE_FACE;
                    }
                    
                    break;


                default:
                    break;
            }
        }

        //Video feed of Meta is flipped ??
        private string MetaHackCubeString(string cs)
        {
            string InputFace1 = cs.Substring(0, 9);
           // Debug.Log("InputFace1: " + InputFace1);
            string InputFace2 = cs.Substring(9, 9);
           // Debug.Log("InputFace2: " + InputFace2);
            string InputFace3 = cs.Substring(18, 9);
           // Debug.Log("InputFace3: " + InputFace3);
            string InputFace4 = cs.Substring(27, 9);
           // Debug.Log("InputFace4: " + InputFace4);
            string InputFace5 = cs.Substring(36, 9);
           // Debug.Log("InputFace5: " + InputFace5);
            string InputFace6 = cs.Substring(45, 9);
          //  Debug.Log("InputFace6: " + InputFace6);

            char[] OutputFace1 = new char[9]; // 0 index. 9 tiles on Rubiks Cube Face.
            char[] OutputFace2 = new char[9];
            char[] OutputFace3 = new char[9];
            char[] OutputFace4 = new char[9];
            char[] OutputFace5 = new char[9];
            char[] OutputFace6 = new char[9];

            OutputFace1[0] = InputFace1[8]; OutputFace1[1] = InputFace1[5]; OutputFace1[2] = InputFace1[2];
            OutputFace1[3] = InputFace1[7]; OutputFace1[4] = InputFace1[4]; OutputFace1[5] = InputFace1[1];       // Green = Up
            OutputFace1[6] = InputFace1[6]; OutputFace1[7] = InputFace1[3]; OutputFace1[8] = InputFace1[0];

            OutputFace2[0] = InputFace2[0]; OutputFace2[1] = InputFace2[3]; OutputFace2[2] = InputFace2[6];
            OutputFace2[3] = InputFace2[1]; OutputFace2[4] = InputFace2[4]; OutputFace2[5] = InputFace2[7];       // Red = Right
            OutputFace2[6] = InputFace2[2]; OutputFace2[7] = InputFace2[5]; OutputFace2[8] = InputFace2[8];

            OutputFace3[0] = InputFace3[0]; OutputFace3[1] = InputFace3[3]; OutputFace3[2] = InputFace3[6];
            OutputFace3[3] = InputFace3[1]; OutputFace3[4] = InputFace3[4]; OutputFace3[5] = InputFace3[7];       // Yellow = Front
            OutputFace3[6] = InputFace3[2]; OutputFace3[7] = InputFace3[5]; OutputFace3[8] = InputFace3[8];

            OutputFace4[0] = InputFace4[0]; OutputFace4[1] = InputFace4[3]; OutputFace4[2] = InputFace4[6];
            OutputFace4[3] = InputFace4[1]; OutputFace4[4] = InputFace4[4]; OutputFace4[5] = InputFace4[7];       // Blue = Down
            OutputFace4[6] = InputFace4[2]; OutputFace4[7] = InputFace4[5]; OutputFace4[8] = InputFace4[8];

            OutputFace5[0] = InputFace5[8]; OutputFace5[1] = InputFace5[5]; OutputFace5[2] = InputFace5[2];
            OutputFace5[3] = InputFace5[7]; OutputFace5[4] = InputFace5[4]; OutputFace5[5] = InputFace5[1];       // Orange = Left
            OutputFace5[6] = InputFace5[6]; OutputFace5[7] = InputFace5[3]; OutputFace5[8] = InputFace5[0];

            OutputFace6[0] = InputFace6[8]; OutputFace6[1] = InputFace6[5]; OutputFace6[2] = InputFace6[2];
            OutputFace6[3] = InputFace6[7]; OutputFace6[4] = InputFace6[4]; OutputFace6[5] = InputFace6[1];       // White = Back
            OutputFace6[6] = InputFace6[6]; OutputFace6[7] = InputFace6[3]; OutputFace6[8] = InputFace6[0];

            string s = new string(OutputFace1) + new string(OutputFace2) + new string(OutputFace3) + new string(OutputFace4) + new string(OutputFace5) + new string(OutputFace6);

            return s;
        }
    }
}
