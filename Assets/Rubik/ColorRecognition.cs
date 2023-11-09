/* <summary>
*
* Augmented Reality Rubik Cube Application
* A.I.T 2018
* A00107408
* Masters by Research
* 
* File Description:
* Assignment of measured tile colors from camera pixels to ColorTile enumeration is
* performed here in this file/class.  There are actually two separate algorithms: "Cube Color
* Recognition" and "Face Color Recognition."
*
* Cube Color Recognition
* It is assumed that all six sides of the cube have been observed, and this 
* algorithm analyzes all 54 tiles together. There MUST be exactly 
* nine tiles assigned to each color.  A recursive algorithm is used
* to achieve a minimum total color error-square costs (i.e., distance in pixels
* between measured color values (RGB) and expected color values) of all 
* 54 tiles.
*
* Face Color Recognition
* This algorithm attempts to analyze one Face independently of other faces.
* In this case, no restriction of tile assignments applies.  The algorithm
* used here:
*  - Assigns measured tile to closest expected color.
*  - Assume that some selection of Orange vs. Red was incorrect above.
*    Algorithm adjust for LUMONISITY using the Blue, Green, Yellow and
*    White tiles (assuming the face has some, and that they are correctly
*    identified) and re-assigns tiles based on the adjusted closest expected color.
* 
* Acknowledgments:
* Original Author: Steven P. Punte (aka Android Steve : android.steve@cl-sw.com)
* Date:   April 25th 2015
* 
* <summary> */

namespace Rubik
{
    using System;
    using System.Text;
    using System.Linq;
    using System.Collections.Generic;
    using UnityEngine;
    using OpenCvSharp;

    using ColorTileEnum = Constants.ColorTileEnum;
    using FaceNameEnum = Constants.FaceNameEnum;

    /// <summary>
    /// A.I.T 2018 A00107408
    /// Credit: Steve Punte
    /// </summary>
    public class ColorRecognition
    {
        /// <summary>
        /// Private Class Tile Location
        /// This is used to refer to a location of a tile on the cube
        /// Original @author android.steve@cl-sw.com
        /// </summary>
        internal class TileLocation
        {
            internal readonly FaceNameEnum faceNameEnum;
            internal readonly int n;
            internal readonly int m;
            public TileLocation(FaceNameEnum faceNameEnum, int n, int m)
            {
                this.faceNameEnum = faceNameEnum;
                this.n = n;
                this.m = m;
            }
        }

        /// <summary>
        /// Inner class Cube
        /// 
        ///     All six sides of the cube have now been observed.  There MUST be exactly 
        ///     nine tiles assigned to each color.  A recursive algorithm is used
        ///     to achieve a minimum total color error costs (i.e., distance in pixels
        ///     between measured color values (RGB) and expected color values) of all 
        ///     54 tiles.
        /// 
        /// @author android.steve@cl-sw.com
        /// 
        /// </summary>
        public class Cube
        {
            internal StateModel stateModel;

            // Map of ColorTimeEnum to a group of tiles.  Group of tiles is actually a Map of color error to tile location. 
            // This mapping must be synchronized with the assignments in StateModel.
            // It would be possible/natural to place this data structure in State Model since it is synchronized with RubikFace[name].observedTileArray,
            // which contains the tile to color mapping state.
            internal static IDictionary<ColorTileEnum, SortedDictionary<double, TileLocation>> observedColorGroupMap;
            internal static IDictionary<ColorTileEnum, SortedDictionary<double, TileLocation>> bestColorGroupMap;

            // Best Color Assignment State.
            // When assignment is completed, this data structure is copied to StateModel.face[name].colorTile[][] of the main state.
            // It is important to have two copies so that one can represent the starting point of a recursive search, and the other (below)
            // can represent the best assignment mapping achieved during the current (i.e., for a specific tile) recursive search.  
            internal static IDictionary<FaceNameEnum, ColorTileEnum[][]> bestAssignmentState;

            // Best Costs.
            internal static double bestAssignmentCost;

            /// <summary>
            /// Subclass Cube Constructor
            /// </summary>
            /// <param name="stateModel"> </param>
            public Cube(StateModel stateModel)
            {
                this.stateModel = stateModel;
            }


            /// <summary>
            /// Cube Tile Color Recognition
            /// 
            ///     All six sides of the cube have now been observed.  There MUST be exactly 
            ///     nine tiles assigned to each color.  A recursive algorithm is used
            ///     to achieve a minimum total color error costs (i.e., distance in pixels
            ///     between measured color values (RGB) and expected color values) of all 
            ///     54 tiles.  Also, the cost calculation algorithm rules out any two faces 
            ///     having the same center tile color.
            /// 
            /// </summary>
            public virtual void CubeTileColorRecognition()
            {

                Debug.Log("\n \n \n \n \n \n \n \n Entering cube tile color recognition.");
                Debug.Log("\n \n \n ");
                PrintDiagnosticsColorTileAssignments();

                // Clear out all tile color mapping in state model.
                //foreach (FaceNameEnum faceNameEnum in FaceNameEnum.values())
                foreach (FaceNameEnum faceNameEnum in Enum.GetValues(typeof(FaceNameEnum)))
                {
                    {
                        for (int n = 0; n < 3; n++)
                        {
                            for (int m = 0; m < 3; m++)
                            {
                                stateModel.nameRubikFaceMap[faceNameEnum].observedTileArray[n][m] = null;
                            }
                        }
                    }

                    // Populate Color Group Map with necessary objects: i.e., one tree object per color.
                    observedColorGroupMap = new SortedDictionary<ColorTileEnum, SortedDictionary<double, TileLocation>>();
                    foreach (ColorTileEnum colorTile in ColorTileEnum.values())
                    {
                        if (colorTile.isRubikColor == true)
                        {
                            observedColorGroupMap[colorTile] = new SortedDictionary<double, TileLocation>();
                            Debug.Log("observed colorTile: " + observedColorGroupMap[colorTile]);
                        }
                        else
                        {
                            Debug.Log("Not a rubik color.");
                        }
                    }

                    // Populate Best Color Group Map with necessary objects: i.e., tree object per color.
                    bestColorGroupMap = new SortedDictionary<ColorTileEnum, SortedDictionary<double, TileLocation>>();
                    foreach (ColorTileEnum colorTile in ColorTileEnum.values())
                    {
                        if (colorTile.isRubikColor == true)
                        {
                            //bestColorGroupMap.put(colorTile, new SortedDictionary<double, TileLocation>());
                            bestColorGroupMap[colorTile] = new SortedDictionary<double, TileLocation>();
                            Debug.Log("best colorTile: " + observedColorGroupMap[colorTile]);
                        }
                        else
                        {
                            Debug.Log("Best, Not a rubik color.");
                        }
                    }

                    // Loop over all 54 tile location and assign a ColorTileEnum to this location.
                    //foreach (FaceNameEnum faceNameEnum in FaceNameEnum.values())


                    for (int n = 0; n < 3; n++)
                    {
                        for (int m = 0; m < 3; m++)
                        {

                            /* Initialize Best Variables */

                            // Copy State Model to private "best assignment state" as starting point for recursive search.
                            bestAssignmentState = new SortedDictionary<FaceNameEnum, ColorTileEnum[][]>(); // Fresh new Map
                            foreach (FaceNameEnum faceNameEnum2 in Enum.GetValues(typeof(FaceNameEnum)))
                            {
                                ColorTileEnum[][] tileArrayClone = RectangularArrays.ReturnRectangularColorTileEnumArray(3, 3);
                                for (int n2 = 0; n2 < 3; n2++)
                                {
                                    for (int m2 = 0; m2 < 3; m2++)
                                    {
                                        tileArrayClone[n2][m2] = stateModel.nameRubikFaceMap[faceNameEnum2].observedTileArray[n2][m2];
                                    }
                                }
                                bestAssignmentState[faceNameEnum2] = tileArrayClone;
                                Debug.Log("bestAsignmentState: " + bestAssignmentState[faceNameEnum2]);
                            }

                            // Copy Local State Color Group to "best color group"
                            foreach (ColorTileEnum colorTile2 in ColorTileEnum.values())
                            {
                                if (colorTile2.isRubikColor == true)
                                {
                                    SortedDictionary<double, TileLocation> colorGroupClone = new SortedDictionary<double, TileLocation>(observedColorGroupMap[colorTile2]);
                                    bestColorGroupMap[colorTile2] = colorGroupClone;
                                    Debug.Log("bestColorGroupMap: " + bestColorGroupMap[colorTile2]);
                                }
                                else
                                {
                                    Debug.Log("colorTile2 Not a Rubik color");
                                }
                            }

                            bestAssignmentCost = double.MaxValue;



                            /* Insert tile into State Model */

                            // Evaluate (possibly recursive) tile for insertion
                            // The least-cost solution shall be copied to the "best" variables.
                            TileLocation tileLocation = new TileLocation(faceNameEnum, n, m);
/*(9)*/                     EvaluateTileAssignmentForLowestOverallCostPossiblyReursively(tileLocation, new HashSet<ColorTileEnum> { }); // (9));


                            // Copy "best assignment state" to State Model
                            foreach (FaceNameEnum faceNameEnum3 in Enum.GetValues(typeof(FaceNameEnum)))
                            {
                                stateModel.nameRubikFaceMap[faceNameEnum3].observedTileArray = bestAssignmentState[faceNameEnum3]; // no need for clone.
                            }

                            // Copy "best color group" to Local State Model
                            foreach (ColorTileEnum colorTile2 in ColorTileEnum.values())
                            {
                                if (colorTile2.isRubikColor == true)
                                {
                                    observedColorGroupMap[colorTile2] = new SortedDictionary<double, TileLocation>(bestColorGroupMap[colorTile2]);
                                }
                            }

                            PrintDiagnosticsColorTileAssignments();
                        }
                    }
                }
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

            /// <summary>
            /// Evaluate Tile Assignment for Lowest Overall Cost Possibly Recursive
            /// 
            /// Attempts to assign tile in State Model to every tile color, and copies
            /// lowest cost total assignment to member data "bestAssignmentState".  If
            /// a tile is assigned to a color group, and now that group has more than
            /// nine tiles assigned to it, then remove the tile with the highest cost,
            /// and assign it to some other group recursively.
            /// </summary>
            /// <param name="tileLocation"> </param>
            /// <param name="measuredColor"> </param>
            /// <param name="blackList"> </param>
            private void EvaluateTileAssignmentForLowestOverallCostPossiblyReursively(TileLocation tileLocation, HashSet<ColorTileEnum> blackList)
            {

                Debug.Log("Assign tile with blacklist = " + blackList.Count);

                // Loop and evaluate cost of assigning all colors to this location.  Keep lowest cost in "best" variables.
                foreach (ColorTileEnum colorTile in ColorTileEnum.values())
                {

                    if (colorTile.isRubikColor == false)
                    {
                        continue;
                    }

                    // No evaluation for this color tile: group is full.
                    if (blackList.Contains(colorTile))
                    {
                        continue;
                    }


                    // Assign a color to State Model at specified location
                    AssignTileToColor(tileLocation, colorTile);

                    // If mapping is still valid (i.e., not more than 9 tiles are assigned to any color),
                    // then do not attempt any further re-arrangement of tile mapping.
                    SortedDictionary<double, TileLocation> colorGroup = observedColorGroupMap[colorTile];
                    if (colorGroup.Count <= 9)
                    {

                        // This is sum of selected color errors of tiles that are assigned.  No-assigned are not considered.
                        double cost = CalculateTotalColorErrorCostOfAssignment();

                        // If lower costs of anything so far found, then adopt.
                        if (cost < bestAssignmentCost)
                        {

                            // Copy current cost to best assignment cost
                            bestAssignmentCost = cost;

                            // Copy State Model to Best Assignment State
                            bestAssignmentState = new SortedDictionary<Constants.FaceNameEnum, Constants.ColorTileEnum[][]>(); // Fresh new Map
                            foreach (FaceNameEnum faceNameEnum in Enum.GetValues(typeof(FaceNameEnum)))
                            {
                                //JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
                                //ORIGINAL LINE: ColorTileEnum[][] tileArrayClone = new ColorTileEnum[3][3];
                                ColorTileEnum[][] tileArrayClone = RectangularArrays.ReturnRectangularColorTileEnumArray(3, 3);
                                for (int n = 0; n < 3; n++)
                                {
                                    for (int m = 0; m < 3; m++)
                                    {
                                        tileArrayClone[n][m] = stateModel.nameRubikFaceMap[faceNameEnum].observedTileArray[n][m];
                                    }
                                }
                                bestAssignmentState[faceNameEnum] = tileArrayClone;
                            }

                            // Copy Local State Color Group to "best color group"
                            foreach (ColorTileEnum colorTile2 in ColorTileEnum.values())
                            {
                                if (colorTile2.isRubikColor == true)
                                {
                                    SortedDictionary<double, TileLocation> colorGroupClone = new SortedDictionary<double, TileLocation>(observedColorGroupMap[colorTile2]);
                                    bestColorGroupMap[colorTile2] = colorGroupClone;
                                }
                            }
                        }
                    }

                    // Else, current color group is invalid: too many tiles.  Take out highest cost
                    // tile, and try moving elsewhere.
                    else
                    {

                        // Log.d(Constants.TAG_COLOR, "Color Group " + colorTile + " has too many elements.");

                        // Highest cost assignment is at end of list for TreeMap
                        //TileLocation moveTileLoc = colorGroup.lastEntry().Value;
                        TileLocation moveTileLoc = colorGroup.Values.Last();

                        // Remove from State Model
                        UnassignTileToColor(moveTileLoc, colorTile);
                        // Debug.Log("Unassign tile at location [" + moveTileLoc.faceNameEnum + "][" + moveTileLoc.n + "][" + moveTileLoc.m + "] from color group " + colorTile + " with error cost " + moveTileColorError);

                        // Add to blacklist
                        blackList.Add(colorTile);

                        // Recursively assign tile 2 somewhere else.
                        EvaluateTileAssignmentForLowestOverallCostPossiblyReursively(moveTileLoc, blackList);

                        // Remove from blacklist
                        blackList.Remove(colorTile);

                        // Replace Back to State Model
                        AssignTileToColor(moveTileLoc, colorTile);
                    }

                    // Remove from State Model
                    UnassignTileToColor(tileLocation, colorTile);

                } // End of loop over colors
            }

            /// <summary>
            /// Unassign Color Tile Enum from State Model Tile Location: i.e., observed tile array
            /// </summary>
            /// <param name="tileLocation"> </param>
            /// <param name="colorlTile"> </param>
            private void UnassignTileToColor(TileLocation tileLocation, ColorTileEnum colorlTile)
            {

                RubikFace rubikFace = stateModel.nameRubikFaceMap[tileLocation.faceNameEnum];
                rubikFace.observedTileArray[tileLocation.n][tileLocation.m] = null;

                SortedDictionary<double, TileLocation> colorGroup = observedColorGroupMap[colorlTile];
                double keyOfItemToBeRemoved = 0.0; // null; double?
                foreach (KeyValuePair<double, TileLocation> entry in colorGroup.SetOfKeyValuePairs())
                {
                    if (entry.Value == tileLocation) // =+= Is tile location same object?  Seems like this is true.
                    {
                        keyOfItemToBeRemoved = entry.Key;
                    }
                }
                colorGroup.Remove(keyOfItemToBeRemoved);
            }



            /// <summary>
            /// Assign Color Tile Enum to State Model Tile Location: i.e., observed tile array
            /// </summary>
            /// <param name="tileLocation"> </param>
            /// <param name="colorTile"> </param>
            private void AssignTileToColor(TileLocation tileLocation, ColorTileEnum colorTile)
            {

                RubikFace rubikFace = stateModel.nameRubikFaceMap[tileLocation.faceNameEnum];
                rubikFace.observedTileArray[tileLocation.n][tileLocation.m] = colorTile;

                SortedDictionary<double, TileLocation> colorGroup = observedColorGroupMap[colorTile];
                double MeasuredColorArrayPosition = Convert.ToDouble(rubikFace.measuredColorArray[tileLocation.n][tileLocation.m]);
                colorGroup[CalculateColorErrorSqauresCost(new Scalar(MeasuredColorArrayPosition), colorTile.cvColor)] = tileLocation;
            }



            /// <summary>
            /// Calculate and return the Color Error Assignment costs of the provided assignment map.
            /// 
            /// Return is sum of color error vectors added in a simple scalar magnitude manner.  Note,
            /// a cost of Double.MAX_VALUE is returned if center tile of any two faces have duplicate color.
            /// =+= possibly should be sum square.
            /// 
            /// @return
            /// </summary>
            private double CalculateTotalColorErrorCostOfAssignment()
            {

                double cost = 0.0;

                // Loop over all 54 tile location and assign an ColorTileEnum to this location.
                foreach (FaceNameEnum faceNameEnum in Enum.GetValues(typeof(FaceNameEnum)))
                {
                    for (int n = 0; n < 3; n++)
                    {
                        for (int m = 0; m < 3; m++)
                        {
                            RubikFace rubikFace = this.stateModel.nameRubikFaceMap[faceNameEnum];
                            if (rubikFace.observedTileArray[n][m] != null)
                            {
                                double MeasuredColorArrayPosition = Convert.ToDouble(rubikFace.measuredColorArray[n][m]);
                                cost += CalculateColorErrorSqauresCost(new Scalar(MeasuredColorArrayPosition), rubikFace.observedTileArray[n][m].cvColor);
                            }
                        }
                    }
                }


                // Test if all six center tiles have different colors.  Return infinite cost if not true.
                // Test if any two sides have the same center tile color.  Ignore sides that are not yet assigned.
                HashSet<ColorTileEnum> centerTileColorSet = new HashSet<ColorTileEnum> { }; // (16);
                foreach (FaceNameEnum faceNameEnum in Enum.GetValues(typeof(FaceNameEnum)))
                {
                    RubikFace rubikFace = this.stateModel.nameRubikFaceMap[faceNameEnum];
                    ColorTileEnum centerColorTile = rubikFace.observedTileArray[1][1];

                    if (centerColorTile == null)
                    {
                        continue;
                    }

                    if (centerTileColorSet.Contains(centerColorTile))
                    {
                        return double.MaxValue;
                    }
                    else
                    {
                        centerTileColorSet.Add(centerColorTile);
                    }
                }

                return cost;
            }

            // This is not used.           
            private void PrintDiagnosticsColorTileAssignments()
            {

                Debug.Log("%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%% EOGHAN %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%");
//                if (true)
//                {
//                    return;
//                }

                // Print State Model Observed Tile Array
                foreach (FaceNameEnum faceNameEnum2 in Enum.GetValues(typeof(FaceNameEnum)))
                {

                    StringBuilder str = new StringBuilder();
                    for (int n2 = 0; n2 < 3; n2++)
                    {
                        for (int m2 = 0; m2 < 3; m2++)
                        {
                            str.Append("|" + stateModel.nameRubikFaceMap[faceNameEnum2].observedTileArray[n2][m2]);
                        }
                    }

                    Debug.Log("State Tile at [" + faceNameEnum2 + "] " + str + "|");
                }


                // Print State Model ObservedColorGroupMap
                if (observedColorGroupMap != null)
                {
                    foreach (ColorTileEnum colorTile in observedColorGroupMap.Keys)
                    {

                        SortedDictionary<double, TileLocation> tileColorMap = observedColorGroupMap[colorTile];
                        StringBuilder str = new StringBuilder();
                        foreach (KeyValuePair<double, TileLocation> entry in tileColorMap.SetOfKeyValuePairs())
                        {
                            str.Append("|" + string.Format("{0,5:F1}", entry.Key));
                        }
                        Debug.Log("Color Group " + colorTile + " " + tileColorMap.Count + " " + str + "|");
                    }
                }

                Debug.Log("Total Currnet Cost = " + CalculateTotalColorErrorCostOfAssignment());
            }

            /// <summary>
            /// Calculate the magnitude-squared of the color error vector between 
            /// the two provided color values.
            /// 
            /// =+= probably should make RubicFace.measuredColor a Scalar
            /// </summary>
            /// <param name="color1"> </param>
            /// <param name="color2">
            /// @return </param>
            private static double CalculateColorErrorSqauresCost(Scalar color1, Scalar color2)
            {

                // Calculate distance
                double distance = (color1.Val0 - color2.Val0) * (color1.Val0 - color2.Val0) + (color1.Val1 - color2.Val1) * (color1.Val1 - color2.Val1) + (color1.Val2 - color2.Val2) * (color1.Val2 - color2.Val2);

                return distance;
            }
        }

        /// <summary>
        /// Inner class Face
        /// 
        /// In this case, no restriction of tile assignments applies. The algorithm used here:
        ///  - Assigns measured tile to closest expected color.
        ///  - Assume that some selection of Orange vs. Red was incorrect above.
        ///    Algorithm adjust for LUMONISITY using the Blue, Green, Yellow and
        ///    White tiles (assuming the face has some, and that they are correctly
        ///    identified) and re-assigns tiles based on the adjusted closest expected color.
        ///  
        /// </summary>
        public class Face
        {

            private RubikFace rubikFace;

            // Sum of Color Error before Luminous correction
            public double colorErrorBeforeCorrection;

            // Sum of Color Error after Luminous correction
            public double colorErrorAfterCorrection;

            // Luminous Offset: Added to luminous of tiles for better accuracy
            public double luminousOffset = 0.0;


            public Face(RubikFace rubikFace)
            {
                this.rubikFace = rubikFace;
            }

            private Scalar BetterYellowDetection(double B, double G, double R)
            {
                Scalar BGR = new Scalar(B,G,R);
                
                if (R > 200)
                {
                    if (B < 50)
                    {
                        BGR = new Scalar(230.0, 230.0, 20.0);
                    }
                }
                return BGR;
            }
            /// <summary>
            /// Find Closest Tile Color
            /// 
            /// Two Pass algorithm:
            /// 1) Find closest fit using just U and V axis.
            /// 2) Calculate luminous correction value assuming above choices are correct (exclude Red and Orange)
            /// 3) Find closed fit again using Y, U and V axis where Y is corrected. 
            /// 
            /// </summary>
            /// 
            /// <param name="image" type="OpenCvSharp compatible Mat"> 
            /// @return </param>
            public virtual void FaceTileColorRecognition(Mat image)
            {
                double[][] colorError = RectangularArrays.ReturnRectangularDoubleArray(3, 3);

                // Obtain actual measured tile color from image.
                for (int n = 0; n < 3; n++)
                {
                    for (int m = 0; m < 3; m++)
                    {

                        Point tileCenter = rubikFace.GetTileCenterInPixels(n, m);
                        Size size = image.Size();
                        double width = size.Width;
                        double height = size.Height;

                        // Check location of tile on screen: can be too close to screen edge.
                        if (tileCenter.X < 10 || tileCenter.X > width - 10 || tileCenter.Y < 10 || tileCenter.Y > height - 10)
                        {
                            //Debug.Log(string.Format("Tile at [{0,1:D},{1,1:D}] has coordinates x={2,5:F1} y={3,5:F1} too close to edge to assign color.", n, m, tileCenter.X, tileCenter.Y));
                            //Debug.Log("Tile at " +n +" " +m +" x: " +tileCenter.X +" y: " +tileCenter.Y +"too close to edge to assign color.");
                            rubikFace.measuredColorArray[n][m] = new double[4]; // This will default to back.
                        }

                        // Obtain measured color from average over 20 by 20 pixel square.
                        else
                        {
                            try
                            {
                                Mat mat = image.SubMat((tileCenter.Y - 10), (tileCenter.Y + 10), (tileCenter.X - 10), (tileCenter.X + 10));
                                // rubikFace.measuredColorArray[n][m] = Cv2.Mean(mat).val;

                                rubikFace.measuredColorArray[n][m][0] = Cv2.Mean(mat).Val2; 
                                rubikFace.measuredColorArray[n][m][1] = Cv2.Mean(mat).Val1;
                                rubikFace.measuredColorArray[n][m][2] = Cv2.Mean(mat).Val0;
                               
                                // rubikFace.measuredColorArray[n][m][3] = Cv2.Mean(mat).Val3;

                                /*double B = Cv2.Mean(mat).Val2;
                                double G = Cv2.Mean(mat).Val1;
                                double R = Cv2.Mean(mat).Val0;

                                Scalar BGR = new Scalar();

                                BGR = BetterYellowDetection(B, G, R);
                                rubikFace.measuredColorArray[n][m][0] = BGR[0];
                                rubikFace.measuredColorArray[n][m][1] = BGR[1];
                                rubikFace.measuredColorArray[n][m][2] = BGR[2];*/
                            }

                            // Probably LMS calculations produced bogus tile location.
                            catch (Exception e)
                            {
                                Debug.Log("ERROR findClosestLogicalTiles(): x=" + tileCenter.X + " y=" + tileCenter.Y + " img=" + image + " :" + e);
                                rubikFace.measuredColorArray[n][m] = new double[4];
                            }
                        }
                    }
                }

                // First Pass: Find closest logical color using only UV axis.
                for (int n = 0; n < 3; n++)
                {
                    for (int m = 0; m < 3; m++)
                    { 
                        double[] measuredColor = rubikFace.measuredColorArray[n][m];
                        
                        double[] measuredColorYUV = Util.GetYUVfromRGB(measuredColor);  
                       
                        double smallestError = double.MaxValue;
                        ColorTileEnum bestCandidate = null;

                        foreach (ColorTileEnum candidateColorTile in ColorTileEnum.values())
                        {
                            if (candidateColorTile.isRubikColor == true)
                            { 
                                // double[] candidateColorYUV = Util.GetYUVfromRGB(candidateColorTile.rubikColor.val);
                                double[] CandidateColourRGB = new double[4];
                                CandidateColourRGB[0] = candidateColorTile.rubikColor.Val0;
                                CandidateColourRGB[1] = candidateColorTile.rubikColor.Val1;
                                CandidateColourRGB[2] = candidateColorTile.rubikColor.Val2;
                                CandidateColourRGB[3] = candidateColorTile.rubikColor.Val3;

                                double[] candidateColorYUV = Util.GetYUVfromRGB(CandidateColourRGB);

                                // Only examine U and V axis, and not luminous.
                                double error =  (candidateColorYUV[1] - measuredColorYUV[1]) * (candidateColorYUV[1] - measuredColorYUV[1]) + 
                                                (candidateColorYUV[2] - measuredColorYUV[2]) * (candidateColorYUV[2] - measuredColorYUV[2]);

                                colorError[n][m] = Math.Sqrt(error);

                                if (error < smallestError)
                                {
                                    bestCandidate = candidateColorTile;
                                    smallestError = error;
                                }
                                //Debug.Log("Candidate: "+candidateColorTile + "'s error: " + error + "- SmallestError: " + smallestError  +" - bestCandidate: " + bestCandidate );
                            }
                        }

                        // Debug.Log(String.format( "Tile[%d][%d] has R=%3.0f, G=%3.0f B=%3.0f %c err=%4.0f", n, m, measuredColor[0], measuredColor[1], measuredColor[2], bestCandidate.character, smallestError));

                        // Assign best candidate to this tile location.
                        rubikFace.observedTileArray[n][m] = bestCandidate;
                    }
                }

                // Calculate and record LMS error (including luminous).
                for (int n = 0; n < 3; n++)
                {
                    for (int m = 0; m < 3; m++)
                    {
                        // double[] selectedColor = rubikFace.observedTileArray[n][m].rubikColor.val;
                        double[] selectedColor = new double[4];
                        selectedColor[0] = rubikFace.observedTileArray[n][m].rubikColor.Val0;
                        selectedColor[1] = rubikFace.observedTileArray[n][m].rubikColor.Val1;
                        selectedColor[2] = rubikFace.observedTileArray[n][m].rubikColor.Val2;
                        selectedColor[3] = rubikFace.observedTileArray[n][m].rubikColor.Val3;

                        double[] measuredColor = rubikFace.measuredColorArray[n][m];
                        colorErrorBeforeCorrection += CalculateColorError(selectedColor, measuredColor, true, 0.0);
                    }
                }
                
                // Diagnostics:  For each tile location print: measure RGB, measure YUV, logical RGB, logical YUV
                /*Debug.Log("Table: Measure RGB, Measure YUV, Logical RGB, Logical YUV");
                Debug.Log(string.Format(" m:n|----------0--------------|-----------1-------------|---------2---------------|"));
                Debug.Log(string.Format(" 0  |{0}|{1}|{2}|", Util.DumpRGB(rubikFace.measuredColorArray[0][0], colorError[0][0]), Util.DumpRGB(rubikFace.measuredColorArray[1][0], colorError[1][0]), Util.DumpRGB(rubikFace.measuredColorArray[2][0], colorError[2][0])));
                Debug.Log(string.Format(" 0  |{0}|{1}|{2}|", Util.DumpYUV(rubikFace.measuredColorArray[0][0]), Util.DumpYUV(rubikFace.measuredColorArray[1][0]), Util.DumpYUV(rubikFace.measuredColorArray[2][0])));
                Debug.Log(string.Format(" 0  |{0}|{1}|{2}|", Util.DumpRGB(rubikFace.observedTileArray[0][0]), Util.DumpRGB(rubikFace.observedTileArray[1][0]), Util.DumpRGB(rubikFace.observedTileArray[2][0])));
              //  Debug.Log(string.Format(" 0  |{0}|{1}|{2}|", Util.DumpYUV(rubikFace.observedTileArray[0][0].rubikColor.val), Util.DumpYUV(rubikFace.observedTileArray[1][0].rubikColor.val), Util.DumpYUV(rubikFace.observedTileArray[2][0].rubikColor.val)));
                Debug.Log(string.Format("    |-------------------------|-------------------------|-------------------------|"));
                Debug.Log(string.Format(" 1  |{0}|{1}|{2}|", Util.DumpRGB(rubikFace.measuredColorArray[0][1], colorError[0][1]), Util.DumpRGB(rubikFace.measuredColorArray[1][1], colorError[1][1]), Util.DumpRGB(rubikFace.measuredColorArray[2][1], colorError[2][1])));
                Debug.Log(string.Format(" 1  |{0}|{1}|{2}|", Util.DumpYUV(rubikFace.measuredColorArray[0][1]), Util.DumpYUV(rubikFace.measuredColorArray[1][1]), Util.DumpYUV(rubikFace.measuredColorArray[2][1])));
                Debug.Log(string.Format(" 1  |{0}|{1}|{2}|", Util.DumpRGB(rubikFace.observedTileArray[0][1]), Util.DumpRGB(rubikFace.observedTileArray[1][1]), Util.DumpRGB(rubikFace.observedTileArray[2][1])));
              //  Debug.Log(string.Format(" 1  |{0}|{1}|{2}|", Util.DumpYUV(rubikFace.observedTileArray[0][1].rubikColor.val), Util.DumpYUV(rubikFace.observedTileArray[1][1].rubikColor.val), Util.DumpYUV(rubikFace.observedTileArray[2][1].rubikColor.val)));
                Debug.Log(string.Format("    |-------------------------|-------------------------|-------------------------|"));
                Debug.Log(string.Format(" 2  |{0}|{1}|{2}|", Util.DumpRGB(rubikFace.measuredColorArray[0][2], colorError[0][2]), Util.DumpRGB(rubikFace.measuredColorArray[1][2], colorError[1][2]), Util.DumpRGB(rubikFace.measuredColorArray[2][2], colorError[2][2])));
                Debug.Log(string.Format(" 2  |{0}|{1}|{2}|", Util.DumpYUV(rubikFace.measuredColorArray[0][2]), Util.DumpYUV(rubikFace.measuredColorArray[1][2]), Util.DumpYUV(rubikFace.measuredColorArray[2][2])));
                Debug.Log(string.Format(" 2  |{0}|{1}|{2}|", Util.DumpRGB(rubikFace.observedTileArray[0][2]), Util.DumpRGB(rubikFace.observedTileArray[1][2]), Util.DumpRGB(rubikFace.observedTileArray[2][2])));
               // Debug.Log(string.Format(" 2  |{0}|{1}|{2}|", Util.DumpYUV(rubikFace.observedTileArray[0][2].rubikColor.val), Util.DumpYUV(rubikFace.observedTileArray[1][2].rubikColor.val), Util.DumpYUV(rubikFace.observedTileArray[2][2].rubikColor.val)));
                Debug.Log(string.Format("    |-------------------------|-------------------------|-------------------------|"));
                Debug.Log("Total Color Error Before Correction: " + colorErrorBeforeCorrection);*/

                
                // Now compare Actual Luminous against expected luminous, and calculate an offset.
                // However, do not use Orange and Red because they are most likely to be miss-identified.
                // =+= TODO: Also, diminish weight on colors that are repeated.
                luminousOffset = 0.0;
                int count = 0;
                for (int n = 0; n < 3; n++)
                {
                    for (int m = 0; m < 3; m++)
                    {
                        ColorTileEnum colorTile = rubikFace.observedTileArray[n][m];
                        if (colorTile == ColorTileEnum.RED || colorTile == ColorTileEnum.ORANGE)
                        {
                            continue;
                        }
                        double measuredLuminousity = Util.GetYUVfromRGB(rubikFace.measuredColorArray[n][m])[0];
                        // double expectedLuminousity = Util.GetYUVfromRGB(colorTile.rubikColor.val)[0];
                        double[] rgb = new double[4];
                        rgb[0] = colorTile.rubikColor.Val0;
                        rgb[1] = colorTile.rubikColor.Val1;
                        rgb[2] = colorTile.rubikColor.Val2;
                        rgb[3] = colorTile.rubikColor.Val3;
                        
                        double[] expLum = new double[4];
                        expLum = Util.GetYUVfromRGB(rgb);

                        double expectedLuminousity;
                        expectedLuminousity = expLum[0];
                        luminousOffset += (expectedLuminousity - measuredLuminousity);
                        count++;
                    }
                }
                luminousOffset = (count == 0) ? 0.0 : luminousOffset / count;
                //Debug.Log("Luminousity Offset: " + luminousOffset);

                
                // Second Pass: Find closest logical color using YUV but add luminousity offset to measured values.
                for (int n = 0; n < 3; n++)
                {
                    for (int m = 0; m < 3; m++)
                    {

                        double[] measuredColor = rubikFace.measuredColorArray[n][m];
                        double[] measuredColorYUV = Util.GetYUVfromRGB(measuredColor);

                        double smallestError = double.MaxValue;
                        ColorTileEnum bestCandidate = null;

                        foreach (ColorTileEnum candidateColorTile in ColorTileEnum.values())
                        {

                            if (candidateColorTile.isRubikColor == true)
                            {

                                //double[] candidateColorYUV = Util.GetYUVfromRGB(candidateColorTile.rubikColor.val);
                                double[] rgb = new double[4];
                                rgb[0] = candidateColorTile.rubikColor.Val0;
                                rgb[1] = candidateColorTile.rubikColor.Val1;
                                rgb[2] = candidateColorTile.rubikColor.Val2;
                                rgb[3] = candidateColorTile.rubikColor.Val3;

                                double[] candidateColorYUV = new double[4];
                                candidateColorYUV =  Util.GetYUVfromRGB(rgb);

                                // Calculate Error based on U, V, and Y, but adjust with luminous offset.
                                double error = (candidateColorYUV[0] - (measuredColorYUV[0] + luminousOffset)) * (candidateColorYUV[0] - (measuredColorYUV[0] + luminousOffset)) + (candidateColorYUV[1] - measuredColorYUV[1]) * (candidateColorYUV[1] - measuredColorYUV[1]) + (candidateColorYUV[2] - measuredColorYUV[2]) * (candidateColorYUV[2] - measuredColorYUV[2]);

                                colorError[n][m] = Math.Sqrt(error);

                                if (error < smallestError)
                                {
                                    bestCandidate = candidateColorTile;
                                    smallestError = error;
                                }
                            }
                            
                        }

                        // Debug.Log(String.Format( "Tile[%d][%d] has R=%3.0f, G=%3.0f B=%3.0f %c err=%4.0f", n, m, measuredColor[0], measuredColor[1], measuredColor[2], bestCandidate.character, smallestError));

                        // Check and possibly re-assign this tile location with a different color.
                        if (bestCandidate != rubikFace.observedTileArray[n][m])
                        {
                            //Debug.Log(string.Format("Reclassiffying tile [{0:D}][{1:D}] from {2} to {3}", n, m, rubikFace.observedTileArray[n][m].symbol, bestCandidate.symbol));
                            //Debug.Log("Reclassiffying tile from {2} to {3} " +n + " " +m + " " + rubikFace.observedTileArray[n][m].symbol + " "  + bestCandidate.symbol);
                            rubikFace.observedTileArray[n][m] = bestCandidate;
                        }
                    }
                }
                
                // Calculate and record LMS error (includeing LMS).
                for (int n = 0; n < 3; n++)
                {
                    for (int m = 0; m < 3; m++)
                    {
                        // double[] selectedColor = rubikFace.observedTileArray[n][m].rubikColor.val;
                        double[] selectedColor = new double[4];
                        selectedColor[0] = rubikFace.observedTileArray[n][m].rubikColor.Val0;
                        selectedColor[1] = rubikFace.observedTileArray[n][m].rubikColor.Val1;
                        selectedColor[2] = rubikFace.observedTileArray[n][m].rubikColor.Val2;
                        selectedColor[3] = rubikFace.observedTileArray[n][m].rubikColor.Val3;

                        double[] measuredColor = rubikFace.measuredColorArray[n][m];
                        colorErrorAfterCorrection += CalculateColorError(selectedColor, measuredColor, true, luminousOffset);
                    }
                }

                // Diagnostics: 
                /*Debug.Log("Table: Measure RGB, Measure YUV, Logical RGB, Logical YUV");
                Debug.Log(string.Format(" m:n|----------0--------------|-----------1-------------|---------2---------------|"));
                Debug.Log(string.Format(" 0  |{0}|{1}|{2}|", Util.DumpRGB(rubikFace.measuredColorArray[0][0], colorError[0][0]), Util.DumpRGB(rubikFace.measuredColorArray[1][0], colorError[1][0]), Util.DumpRGB(rubikFace.measuredColorArray[2][0], colorError[2][0])));
                Debug.Log(string.Format(" 0  |{0}|{1}|{2}|", Util.DumpYUV(rubikFace.measuredColorArray[0][0]), Util.DumpYUV(rubikFace.measuredColorArray[1][0]), Util.DumpYUV(rubikFace.measuredColorArray[2][0])));
                Debug.Log(string.Format(" 0  |{0}|{1}|{2}|", Util.DumpRGB(rubikFace.observedTileArray[0][0]), Util.DumpRGB(rubikFace.observedTileArray[1][0]), Util.DumpRGB(rubikFace.observedTileArray[2][0])));
              //  Debug.Log(string.Format(" 0  |{0}|{1}|{2}|", Util.DumpYUV(rubikFace.observedTileArray[0][0].rubikColor.val), Util.DumpYUV(rubikFace.observedTileArray[1][0].rubikColor.val), Util.DumpYUV(rubikFace.observedTileArray[2][0].rubikColor.val)));
                Debug.Log(string.Format("    |--------------------D----|-------------------------|-------------------------|"));
                Debug.Log(string.Format(" 1  |{0}|{1}|{2}|", Util.DumpRGB(rubikFace.measuredColorArray[0][1], colorError[0][1]), Util.DumpRGB(rubikFace.measuredColorArray[1][1], colorError[1][1]), Util.DumpRGB(rubikFace.measuredColorArray[2][1], colorError[2][1])));
                Debug.Log(string.Format(" 1  |{0}|{1}|{2}|", Util.DumpYUV(rubikFace.measuredColorArray[0][1]), Util.DumpYUV(rubikFace.measuredColorArray[1][1]), Util.DumpYUV(rubikFace.measuredColorArray[2][1])));
                Debug.Log(string.Format(" 1  |{0}|{1}|{2}|", Util.DumpRGB(rubikFace.observedTileArray[0][1]), Util.DumpRGB(rubikFace.observedTileArray[1][1]), Util.DumpRGB(rubikFace.observedTileArray[2][1])));
              //  Debug.Log(string.Format(" 1  |{0}|{1}|{2}|", Util.DumpYUV(rubikFace.observedTileArray[0][1].rubikColor.val), Util.DumpYUV(rubikFace.observedTileArray[1][1].rubikColor.val), Util.DumpYUV(rubikFace.observedTileArray[2][1].rubikColor.val)));
                Debug.Log(string.Format("    |--------------------D----|-------------------------|-------------------------|"));
                Debug.Log(string.Format(" 2  |{0}|{1}|{2}|", Util.DumpRGB(rubikFace.measuredColorArray[0][2], colorError[0][2]), Util.DumpRGB(rubikFace.measuredColorArray[1][2], colorError[1][2]), Util.DumpRGB(rubikFace.measuredColorArray[2][2], colorError[2][2])));
                Debug.Log(string.Format(" 2  |{0}|{1}|{2}|", Util.DumpYUV(rubikFace.measuredColorArray[0][2]), Util.DumpYUV(rubikFace.measuredColorArray[1][2]), Util.DumpYUV(rubikFace.measuredColorArray[2][2])));
                Debug.Log(string.Format(" 2  |{0}|{1}|{2}|", Util.DumpRGB(rubikFace.observedTileArray[0][2]), Util.DumpRGB(rubikFace.observedTileArray[1][2]), Util.DumpRGB(rubikFace.observedTileArray[2][2])));
              //  Debug.Log(string.Format(" 2  |{0}|{1}|{2}|", Util.DumpYUV(rubikFace.observedTileArray[0][2].rubikColor.val), Util.DumpYUV(rubikFace.observedTileArray[1][2].rubikColor.val), Util.DumpYUV(rubikFace.observedTileArray[2][2].rubikColor.val)));
                Debug.Log(string.Format("    |-------------------------|-------------------------|-------------------------|"));*/

                //Debug.Log("Color Error After Correction: " + colorErrorAfterCorrection);
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
                internal static double[][] ReturnRectangularDoubleArray(int size1, int size2)
                {
                    double[][] newArray = new double[size1][];
                    for (int array1 = 0; array1 < size1; array1++)
                    {
                        newArray[array1] = new double[size2];
                    }

                    return newArray;
                }
            }

            /// <summary>
            /// Calculate Color Error
            /// 
            /// Return distance between two colors.
            /// </summary>
            /// <param name="slected"> </param>
            /// <param name="measured"> </param>
            /// <param name="useLuminous"> </param>
            /// <param name="_luminousOffset"> 
            /// @return </param>
            private static double CalculateColorError(double[] slected, double[] measured, bool useLuminous, double _luminousOffset)
            {
                double error = (slected[0] - (measured[0] + _luminousOffset)) * (slected[0] - (measured[0] + _luminousOffset)) + (slected[1] - measured[1]) * (slected[1] - measured[1]) + (slected[2] - measured[2]) * (slected[2] - measured[2]);
                return Math.Sqrt(error);
            }

        }
    }

    //---------------------------------------------------------------------------------------------------------
    //	Copyright © 2007 - 2018 Tangible Software Solutions Inc.
    //	This class can be used by anyone provided that the copyright notice remains intact.
    //
    //	This class is used to replace calls to some Java HashMap or Hashtable methods.
    //---------------------------------------------------------------------------------------------------------
    internal static class HashMapHelper
    {
        internal static HashSet<KeyValuePair<TKey, TValue>> SetOfKeyValuePairs<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
        {
            HashSet<KeyValuePair<TKey, TValue>> entries = new HashSet<KeyValuePair<TKey, TValue>>();
            foreach (KeyValuePair<TKey, TValue> keyValuePair in dictionary)
            {
                entries.Add(keyValuePair);
            }
            return entries;
        }

        internal static TValue GetValueOrNull<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue ret;
            dictionary.TryGetValue(key, out ret);
            return ret;
        }
    }
}