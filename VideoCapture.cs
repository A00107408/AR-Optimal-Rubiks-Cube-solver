/*<summary>
 * 
 * Augmented Reality Rubik Cube Application
 * A.I.T 2018
 * A00107408
 * Masters by Research
 * 
 * Acknowledgments:
 * Original Author: Steven P. Punte (aka Android Steve : android.steve@cl-sw.com)
 * Date:   April 25th 2015
 * 
 *<summary*/

namespace Rubik
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using OpenCvSharp;
    using Uk.Org.Adcock.Parallel;  // Stewart Adcock's implementation of parallel processing speeds up Unity 2D to Mat conversion by half.
    
    using ColorTileEnum = Constants.ColorTileEnum;
   
    public class VideoCapture : MonoBehaviour
    {
        //======================================== New Unity Members ================================================
        // Video parameters
        public int deviceNumber;
        private WebCamTexture _webcamTexture;                           // Video input frame as unity Texture.

        private const int imWidth = 1280;
        private const int imHeight = 720;

        private Mat videoSourceImage;                                   // Convert Video input frame to OpenCV Mat for processing.
        private Vec3b[] videoSourceImageData;

        private Mat processedImage;                                     // Processed Video frame as OpenCV Mat.
        private byte[] processedImageData;
        private Texture2D processedTexture;                             // Convert processed Video frame back to Unity Texture for display.
       
        private Mat rgba_image;                                         // For debug only.
        //  private byte[] rgba_imageData;                              // For augmentation debug in pop-up window.

        public Mat OutputImage;
        private byte[] OutputImageData;
        public Texture2D OutputTexture;

        // Flip the video source axes (webcams are usually mirrored)
        // Unity and OpenCV images are flipped
        public bool FlipUpDownAxis = false, FlipLeftRightAxis = false;

        public static string Instruction = "Look at Cube for Instructions...";

        private bool NextInstruction = false;

        public static bool StartInstructions = false;
        public static int InstInd = 1;

        public static GameObject HeadTracking = null;

        public static bool TestStarted = false;
        public static float FPS, InstDelRate, OneSecond, InstDelTime;
        public static float ConsecutiveStableCountThreshold = 0;

        public static bool AudioPlaying = false;

        //================================================================================================================



        //========================================== Original App Members =================================================

        private StateModel stateModel;
        private AppStateMachine appStateMachine;
        private Annotation annotation;
        private Util.LoadPruningTablesTask lptt;

        // Time stamp for internal frame-per-second measuring and reporting.
        private long framesPerSecondTimeStamp;

        //=================================================================================================================


        //---------------------------------------------------------------------------------------------------------
        //	Copyright © 2007 - 2018 Tangible Software Solutions Inc.
        //	This class can be used by anyone provided that the copyright notice remains intact.
        //
        //	This class is used to replace calls to Java's System.currentTimeMillis with the C# equivalent.
        //	Unix time is defined as the number of seconds that have elapsed since midnight UTC, 1 January 1970.
        //---------------------------------------------------------------------------------------------------------
        internal static class DateTimeHelper
        {
            private static readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            internal static long CurrentUnixTimeMillis()
            {
                return (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
            }
        }

        // Use this for initialization of the class
        void Start()
        {       
            stateModel = new StateModel();
            appStateMachine = new AppStateMachine(stateModel);
            annotation = new Annotation(this.stateModel, this.appStateMachine);

            //(new Util.LoadPruningTablesTask()).execute(appStateMachine);
            lptt = new Util.LoadPruningTablesTask();
            lptt.DoInBackground(appStateMachine);                                    // Not currently asynchronous

            //Webcam initialisation
            WebCamDevice[] devices = WebCamTexture.devices;
            //Debug.Log("Number of video devices = " + devices.Length);

            if (devices.Length > 0)
            {   // If there is at least one camera

                /*foreach (var cam in devices)
                {                   
                    if (!cam.name.Contains("Holographic"))
                    {
                        Debug.Log("Choosing Cam: " + cam.name);
                        _webcamTexture = new WebCamTexture(cam.name, imWidth, imHeight);
                    }
                }*/

                _webcamTexture = new WebCamTexture(devices[deviceNumber].name, imWidth, imHeight);


                //HeadTracking using MetaCamerRig.transform
                HeadTracking = GameObject.Find("MetaCameraRig");

                // assign webcam texture to the meshrenderer for display
                //  var MeshRenderer = GetComponent<MeshRenderer>();
                //  MeshRenderer.material.mainTexture = _webcamTexture;

                // Attach camera to texture of the gameObject
                var renderer = GetComponent<Renderer>();
                renderer.material.mainTexture = _webcamTexture;

                // Un-mirror the webcam image
                /*if (FlipLeftRightAxis)
                {
                    transform.localScale = new Vector3(-transform.localScale.x,
                            transform.localScale.y, transform.localScale.z);
                }

                if (FlipUpDownAxis)
                {
                    transform.localScale = new Vector3(transform.localScale.x,
                            -transform.localScale.y, transform.localScale.z);
                }*/

                _webcamTexture.Play();  // Play the video source

                // Get the video source image width and height
                //imWidth = _webcamTexture.width;
                //imHeight = _webcamTexture.height;          

                // initialize video / image with given size
                videoSourceImage = new Mat(imHeight, imWidth, MatType.CV_8UC3); //3 channels RGB
                videoSourceImageData = new Vec3b[imHeight * imWidth];

                processedImage = new Mat(imHeight, imWidth, MatType.CV_8UC1);   //Only one channel?
                processedImageData = new byte[imHeight * imWidth];

                rgba_image = new Mat(imHeight, imWidth, MatType.CV_8UC3);       // For debug in pop-up window.
                                                                                //               rgba_imageData = new byte[imHeight * imWidth];



                OutputImage = new Mat(imHeight, imWidth, MatType.CV_8UC1);   //Only one channel?
                OutputImageData = new byte[imHeight * imWidth];
                Cv2.Rectangle(OutputImage, new Point(0, 0), new Point(1270, 1270), new Scalar(0, 0, 0, 0), -1);
                OutputTexture = new Texture2D(imWidth, imHeight, TextureFormat.RGBA32, true, true);




                // create processed video texture as Texture2D object
                processedTexture = new Texture2D(imWidth, imHeight, TextureFormat.RGBA32, true, true);

                // assign the processedTexture to the meshrenderer for display
                //ProcessedTextureRenderer.material.mainTexture = processedTexture;


                //renderer.material.mainTexture = processedTexture;       
                renderer.material.mainTexture = OutputTexture;
            }

            Cv2.NamedWindow("Copy video");

           // WindowsVoice.speak("Lets solve A Rubiks Cube");
        }

        /// <summary>
        /// On Camera Frame:
        /// Process frame image through Rubik Face recognition possibly resulting in a state change.
        /// </summary>
        void Update()
        {
            if (TestStarted == true)
            {
                FPS = 1.0f / Time.smoothDeltaTime;                      // FrameRate.         
                Debug.Log("FrameRate: " + FPS);

                InstDelRate = FPS / ConsecutiveStableCountThreshold;    //This many oportunities per second to deliver instruction.
                Debug.Log("Del Rate: " + InstDelRate);

                InstDelTime = ConsecutiveStableCountThreshold / FPS;    //Length of time it'll take to detect a stable face & deliver instruction.
                Debug.Log("Instruction Delivery Time: " + InstDelTime + " of a second");
            }



            if (Input.GetKeyDown("space"))
            {
                Debug.Log("Space Pressed");
               // appStateMachine.stateModel.solutionResultIndex++;
               // print("Instruction: " +appStateMachine.stateModel.solutionResultsArray[appStateMachine.stateModel.solutionResultIndex]);

               // float timeStamp = Time.time;
            }

            // Clear Outputexture
            /*   OutputTexture = new Texture2D(imWidth, imHeight, TextureFormat.RGBA32, true, true);

               // Reset all pixels color to transparent
               Color32 resetColor = new Color32(0, 0, 0, 0);           //BLACK
               Color32[] resetColorArray =OutputTexture.GetPixels32();

               for (int i = 0; i < resetColorArray.Length; i++)
               {
                   resetColorArray[i] = resetColor;
               }

               OutputTexture.SetPixels32(resetColorArray);
               OutputTexture.Apply();*/


            if (_webcamTexture.isPlaying)
            {
                if (_webcamTexture.didUpdateThisFrame)
                {
                    // convert texture of original video to OpenCVSharp Mat object
                    TextureToMat();

                    //OutputImage.Release();
                   // OutputImage = new Mat(imHeight, imWidth, MatType.CV_8UC1);

                    // create the canny edge image out of source image
                    ProcessImage(videoSourceImage);

                    // convert the OpenCVSharp Mat of canny image to Texture2D
                    // the texture will be displayed automatically
                    MatToTexture();
                    //RGBAtoTexture();                                                      
                    
                    OutputMatToOutputTexture();                   

                    UpdateWindow(rgba_image);                   
                }
            }
            else
            {
                Debug.Log("Can't find camera!");
            }
        }


        void TextureToMat()
        {
            // Color32 array : r, g, b, a
            Color32[] c = _webcamTexture.GetPixels32();

            // Parallel for loop
            // convert Color32 object to Vec3b object
            // Vec3b is the representation of pixel for Mat
            Parallel.For(0, imHeight, i => {
                for (var j = 0; j < imWidth; j++)
                {
                    var col = c[j + i * imWidth];
                    var vec3 = new Vec3b
                    {
                        Item0 = col.b,
                        Item1 = col.g,
                        Item2 = col.r
                    };
                    // set pixel to an array
                    videoSourceImageData[j + i * imWidth] = vec3;
                }
            });
            // assign the Vec3b array to Mat
            videoSourceImage.SetArray(0, 0, videoSourceImageData);
        }

        void MatToTexture()
        {
            // cannyImageData is byte array, because canny image is grayscale
            processedImage.GetArray(0, 0, processedImageData);
            // create Color32 array that can be assigned to Texture2D directly
            Color32[] c = new Color32[imHeight * imWidth];

            // parallel for loop
            Parallel.For(0, imHeight, i => {
                for (var j = 0; j < imWidth; j++)
                {
                    byte vec = processedImageData[j + i * imWidth];
                    var color32 = new Color32
                    {
                        r = vec,
                        g = vec,
                        b = vec,
                        a = 0
                    };
                    c[j + i * imWidth] = color32;
                }
            });

            processedTexture.SetPixels32(c);
            // to update the texture, OpenGL manner
            processedTexture.Apply();
        }
        
      /*  void OutputTextureToBlack()
        {
           // OutputImage.GetArray(0, 0, OutputImageData);
            // create Color32 array that can be assigned to Texture2D directly
            Color32[] c = new Color32[imHeight * imWidth];

            // parallel for loop
            Parallel.For(0, imHeight, i => {
                for (var j = 0; j < imWidth; j++)
                {
                  //  byte vec = OutputImageData[j + i * imWidth];
                    var color32 = new Color32
                    {
                        r = 0,
                        g = 0,
                        b = 0,
                        a = 0
                    };
                    c[j + i * imWidth] = color32;
                }
            });

            OutputTexture.SetPixels32(c);
            // to update the texture, OpenGL manner
            OutputTexture.Apply();
        }*/

        void OutputMatToOutputTexture()
        {
            //  Destroy(OutputTexture);
            //  OutputTexture = new Texture2D(imWidth, imHeight, TextureFormat.RGBA32, true, true);
            
            // cannyImageData is byte array, because canny image is grayscale
            OutputImage.GetArray(0, 0, OutputImageData);
            // create Color32 array that can be assigned to Texture2D directly


            Color32[] c = new Color32[imHeight * imWidth];

            // parallel for loop
            Parallel.For(0, imHeight, i => {
                for (var j = 0; j < imWidth; j++)
                {
                    byte vec = OutputImageData[j + i * imWidth];
                    var color32 = new Color32
                    {
                        r = vec,
                        g = vec,
                        b = vec,
                        a = 0
                    };
                    c[j + i * imWidth] = color32;
                }
            });

            
            OutputTexture.SetPixels32(c);
            // to update the texture, OpenGL manner
            OutputTexture.Apply();
        }

        /*  void RGBAtoTexture()
          {
              // cannyImageData is byte array, because canny image is grayscale
              rgba_image.GetArray(0, 0, rgba_imageData);
              // create Color32 array that can be assigned to Texture2D directly
              Color32[] c = new Color32[imHeight * imWidth];

              // parallel for loop
              Parallel.For(0, imHeight, i => {
                  for (var j = 0; j < imWidth; j++)
                  {
                      byte vec = rgba_imageData[j + i * imWidth];
                      var color32 = new Color32
                      {
                          r = vec,
                          g = vec,
                          b = vec,
                          a = 0
                      };
                      c[j + i * imWidth] = color32;
                  }
              });

              processedTexture.SetPixels32(c);
              // to update the texture, OpenGL manner
              processedTexture.Apply();
          }*/

        void ProcessImage(Mat _image)
        {
            try {

                // Initialize
                RubikFace rubikFace = new RubikFace();
                rubikFace.profiler.markTime(Profiler.Event.START);
               // Debug.Log("============================================================================");

                /* **********************************************************************
                * **********************************************************************
                * Return Original Image
                */
                //stateModel.activeRubikFace = rubikFace;

                /* **********************************************************************
                 * **********************************************************************
                 * Process to Grey Scale
                 * 
                 * This algorithm finds highlights areas that are all of nearly
                 * the same hue.  In particular, cube faces should be highlighted.
                 */
                Cv2.CvtColor(_image, processedImage, ColorConversionCodes.BGR2GRAY);


                /* **********************************************************************
			     * **********************************************************************
			     * Gaussian Filter Blur prevents getting a lot of false hits 
			     */
                int kernelSize = 7; //(int)MenuAndParams.gaussianBlurKernelSizeParam.value;
                kernelSize = kernelSize % 2 == 0 ? kernelSize + 1 : kernelSize; // make odd
                Cv2.GaussianBlur(processedImage, processedImage, new Size(kernelSize, kernelSize), -1, -1);


                /* **********************************************************************
                * **********************************************************************
                * Canny Edge Detection
                */
                Cv2.Canny(processedImage, processedImage, 100, 100);


                /* **********************************************************************
                 * **********************************************************************
                 * Dilation Image Process
                 */
                Cv2.Dilate(processedImage, processedImage, Cv2.GetStructuringElement(MorphShapes.Rect, new Size(10, 10))); //(MenuAndParams.dilationKernelSizeParam.value, MenuAndParams.dilationKernelSizeParam.value)));

                /* **********************************************************************
                 * **********************************************************************
                 * Contour Generation 
                 */
                Point[][] contours;
                HierarchyIndex[] hierarchy;

                Cv2.FindContours(processedImage, out contours, out hierarchy, RetrievalModes.CComp, ContourApproximationModes.ApproxSimple);

                /* **********************************************************************
                 * **********************************************************************
                 * Polygon Detection
                 */
                List<Rhombus> polygonList = new List<Rhombus>();
                foreach (Point[] contour in contours)
                {

                    // Keep only counter clockwise contours.  A clockwise contour is reported as a negative number.
                    double contourArea = Cv2.ContourArea(contour, true);
                    if (contourArea < 0.0)
                    {
                        continue;
                    }
              

                    // Keep only reasonable area contours
                    if (contourArea < 100) //MenuAndParams.minimumContourAreaParam.value)
                    {
                        continue;
                    }
                    
                    // Floating, instead of Double, for some reason required for approximate polygon detection algorithm.
                    MatOfPoint2f contour2f = new MatOfPoint2f();
                    MatOfPoint2f polygon2f = new MatOfPoint2f();
                    MatOfPoint polygon = new MatOfPoint();

                    // Make a Polygon out of a contour with provide Epsilon accuracy parameter.
                    // It uses the Douglas-Peucker algorithm http://en.wikipedia.org/wiki/Ramer-Douglas-Peucker_algorithm
                    //contour.ConvertTo(contour2f, MatType.CV_32FC2);
                    foreach (Point point in contour)
                    {
                        contour2f.Add(point);
                    }

                    Cv2.ApproxPolyDP(contour2f, polygon2f, 30 /*MenuAndParams.polygonEpsilonParam.value*/, true); // Resulting polygon representation is "closed:" its first and last vertices are connected. -  The maximum distance between the original curve and its approximation.
                    polygon2f.ConvertTo(polygon, MatType.CV_32S);

                    polygonList.Add(new Rhombus(polygon));
                }

                /* **********************************************************************
                 * **********************************************************************
                 * Rhombus Tile Recognition
                 * 
                 * From polygon list, produces a list of suitable Parallelograms (Rhombi).
                 */
                List<Rhombus> rhombusList = new List<Rhombus>();
                // Get only valid Rhombus(es) : actually parallelograms.
                int i = 0;
                foreach (Rhombus rhombus in polygonList)
                {
                    rhombus.Qualify();
                    if (rhombus.status == Rhombus.StatusEnum.VALID)
                    {
                        rhombusList.Add(rhombus);
                    }
                    i++;
                }

                // Filtering w.r.t. Rhmobus set characteristics
                Rhombus.RemovedOutlierRhombi(rhombusList);
     
                Cv2.CvtColor(videoSourceImage, rgba_image, ColorConversionCodes.RGB2RGBA, 4);

                //processedImage = new Mat(imHeight, imWidth, MatType.CV_8UC3, new Scalar(0, 0, 0));  // make img transparent for Meta headset.
                //Cv2.CvtColor(_image, processedImage, ColorConversionCodes.);

                foreach (Rhombus rhombus in rhombusList)
                {
                    //rhombus.Draw(final_image, ColorTileEnum.YELLOW.cvColor);
                    rhombus.Draw(rgba_image,new Scalar(255,0,255));
                }

                //Cv2.PutText(final_image, "Num Rhombi: " + rhombusList.Count, new Point(1000, 50), Constants.FontFace, 1, ColorTileEnum.RED.cvColor, 2);
               // Cv2.Flip(rgba_image, rgba_image, FlipMode.X);
               // Cv2.Flip(rgba_image, rgba_image, FlipMode.Y);
               // Cv2.PutText(rgba_image, "Num Rhombi: " + rhombusList.Count, new Point(100, 150), Constants.FontFace, 5, new Scalar(255,0,255), 5);
               // Cv2.Flip(rgba_image, rgba_image, FlipMode.X);

              //  Cv2.Flip(processedImage, processedImage, FlipMode.X);
              //  Cv2.Flip(processedImage, processedImage, FlipMode.Y);



              //  Cv2.Circle(processedImage, new Point(500,500), 1000, new Scalar(0,0,0)); //make img black for transparent background.



                //Cv2.PutText(processedImage, "Num Rhombi: " + rhombusList.Count, new Point(100, 150), Constants.FontFace, 5, new Scalar(255, 255, 255), 5);
               // Cv2.Flip(processedImage, processedImage, FlipMode.X);
               // Cv2.Flip(processedImage, processedImage, FlipMode.Y);


                /* **********************************************************************
                 * **********************************************************************
                 * Face Recognition
                 * 
                 * Takes a collection of Rhombus objects and determines if a valid
                 * Rubik Face can be determined from them, and then also determines 
                 * initial color for all nine tiles. 
                 */
                rubikFace.ProcessRhombuses(rhombusList, videoSourceImage);
                // rubikFace.profiler.markTime(Profiler.Event.FACE);

                //if menuAndParams
                stateModel.activeRubikFace = rubikFace;
                rubikFace.profiler.markTime(Profiler.Event.TOTAL);


                /* **********************************************************************
                * **********************************************************************
                * Cube Pose Estimation
                * 
                * Reconstruct the Rubik Cube 3D location and orientation in GL space coordinates.
                */

                // Only for Pilot cube rendering I think ?? - Eoghan


               /* **********************************************************************
               * **********************************************************************
               * Application State Machine
               * 
               * Will provide user instructions.
               * Will determine when we are on-face and off-face
               * Will determine when we are on-new-face
               * Will change state 
               */
                appStateMachine.OnFaceEvent(rubikFace);
                rubikFace.profiler.markTime(Profiler.Event.CONTROLLER);
                rubikFace.profiler.markTime(Profiler.Event.TOTAL);

                // Normal return point.
                stateModel.activeRubikFace = rubikFace;
                //annotation.DrawAnnotation(ref videoSourceImage);
                annotation.DrawAnnotation(ref rgba_image);
                //Cv2.CvtColor(videoSourceImage, final_image, ColorConversionCodes.RGB2RGBA);
                //annotation.DrawAnnotation(ref final_image);


                //annotation.DrawAnnotation(ref OutputImage);     
                Cv2.Rectangle(OutputImage, new Point(0, 0), new Point(1270, 1270), new Scalar(0, 0, 0, 0), -1);
                Cv2.Flip(OutputImage, OutputImage, FlipMode.X);
                Cv2.PutText(OutputImage, Instruction, new Point(500, 500), Constants.FontFace, 2, ColorTileEnum.RED.cvColor, 2);
                Cv2.Flip(OutputImage, OutputImage, FlipMode.X);
            }
            catch (Exception e)
            {
                Debug.Log("Exception: " + e.Message);
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
                Mat errorImage = new Mat();                              // (imageSize, CvType.CV_8UC4);
                Cv2.PutText(errorImage, "Exception: " + e.Message, new Point(50, 50), Constants.FontFace, 2, ColorTileEnum.WHITE.cvColor, 2);

                int i = 1;
                foreach (var element in e.StackTrace)
                {
                     Cv2.PutText(errorImage, element.ToString(), new Point(50, 50 + 50 * i++), Constants.FontFace, 2, ColorTileEnum.WHITE.cvColor, 2);
                }
                Cv2.ImShow("Copy video", errorImage);
            }
        }

        // Display the original video in a opencv window
        void UpdateWindow(Mat _image)
        {
            Cv2.Flip(_image, _image, FlipMode.X);
        //    Cv2.Flip(_image, _image, FlipMode.Y);
            Cv2.ImShow("Copy video", _image);
            Cv2.Flip(_image, _image, FlipMode.X);
        }

        // close the opencv window
        public void OnDestroy()
        {
            Cv2.DestroyAllWindows();
        }
    }
}