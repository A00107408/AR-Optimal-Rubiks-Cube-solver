/*
 * File Description:
 * 
 *   Actually, can accommodate any polygon, but logic to filter out all non parallelograms
 *   is put here.  We use name "Rhombus" because it is succinct and unique.
 * 
 * 
 *   After processing, convex quadrilater vertices should be as show:
 *  
 *   *    -----> X
 *   |
 *   |
 *  \ /
 *   Y                          * 0
 *                             / \
 *                       beta /   \ alpha
 *                           /     \
 *                        1 *       * 3
 *                           \     /
 *                            \   /
 *                             \ /
 *                              * 2
 * 
 *  As show above alpha angle ~= +60 deg, beta angle ~= +120 deg
 * 
 * License:
 * 
 *  GPL
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

namespace Rubik
{

    using System.Collections.Generic;
    using System;
    using UnityEngine;
    using OpenCvSharp;

    public class Rhombus
    {
        // Possible states that this Rhombus can be identified.
        public enum StatusEnum
        {
            NOT_PROCESSED,
            NOT_4_POINTS,
            NOT_CONVEX,
            AREA,
            CLOCKWISE,
            OUTLIER,
            VALID
        }

        // Current Status
        public StatusEnum status = StatusEnum.NOT_PROCESSED;

        // Various forms of storing the corner points.
        private MatOfPoint polygonMatrix;
        private List<Point> polygonPointList; // =+= possibly eliminate
        public Point[] polygonPointArray; // =+= note order is adjusted

        // Center of Polygon
        internal Point center = new Point();

        // Area of Quadrilateral.
        internal double area;

        // Smaller angle (in degrees: between 0 and 180) that two set parallelogram edges make to x-axis. 
        internal double alphaAngle;

        // Larger angle (in degrees: between 0 and 180) that two set parallelogram edges make to x-axis. 
        internal double betaAngle;

        // Best estimate (average) of parallelogram alpha side length.
        internal double alphaLength;

        // Best estimate (average) of parallelogram beta side length.
        internal double betaLength;

        // Ratio of beta to alpha length.
        internal double gammaRatio;

        /// <summary>
        /// Rhombus Constructor
        /// 
        /// </summary>
        /// <param name="polygon"> </param>
        //JAVA TO C# CONVERTER WARNING: The following constructor is declared outside of its associated class:
        //ORIGINAL LINE: public Rhombus(MatOfPoint polygon)
        public Rhombus(MatOfPoint polygon)
        {

            polygonMatrix = polygon;
            polygonPointArray = polygon.ToArray();

            //polygonPointList = polygonPointArray.ToList();
           // int index = 0;
           // while (index < polygonPointArray.Length)                                   // Test This !!!!!
           // {
           //     polygonPointList.Add(polygonPointArray[index]);
           //     index++;
           // }
        }

        /// <summary>
        /// Determine is polygon is a value Rubik Face Parallelogram
        /// </summary>
        public virtual void Qualify()
        {

            // Calculate center
            double x = 0;
            double y = 0;
            foreach (Point point in polygonPointArray)
            {
                x += point.X; 
                y += point.Y;
            }
            center.X = (int)x / polygonPointArray.Length;   // Eoghan cast manually from double to int. May loose precission ??
            center.Y = (int)y / polygonPointArray.Length;

            // Check if has four sizes and endpoints.
            if (polygonPointArray.Length != 4)
            {
                status = StatusEnum.NOT_4_POINTS;
                return;
            }

            // Check if convex
            // =+= I don't believe this is working.  result should be either true or 
            // =+= false indicating clockwise or counter-clockwise depending if image 
            // =+= is a "hole" or a "blob".
            InputArray ia = polygonMatrix;
            if (Cv2.IsContourConvex(ia) == false)
            {
                status = StatusEnum.NOT_CONVEX;
                return;
            }

            // Compute area; check if it is reasonable.
            area = AreaOfConvexQuadrilateral(polygonPointArray);
            if ((area < 1000 /*MenuAndParams.minimumRhombusAreaParam.value*/) || (area > 10000 /*MenuAndParams.maximumRhombusAreaParam.value*/))
            {
                status = StatusEnum.AREA;
                return;
            }

            // Adjust vertices such that element 0 is at bottom and order is counter clockwise.
            // =+= return true here if points are counter-clockwise.
            // =+= sometimes both rotations are provided.
            if (AdjustQuadrilaterVertices() == true)
            {
                status = StatusEnum.CLOCKWISE;
                return;
            }


            // =+= beta calculation is failing when close to horizontal.
            // =+= Can vertices be chooses so that we do not encounter the roll over problem at +180?
            // =+= Or can math be performed differently?

            /*
             * Calculate angles to X axis of Parallelogram sides.  Take average of both sides.
             * =+= To Do:
             *   1) Move to radians.
             *   2) Move to +/- PIE representation.
             */
            alphaAngle = 180.0 / Math.PI * Math.Atan2((polygonPointArray[1].Y - polygonPointArray[0].Y) + (polygonPointArray[2].Y - polygonPointArray[3].Y), (polygonPointArray[1].X - polygonPointArray[0].X) + (polygonPointArray[2].X - polygonPointArray[3].X));

            betaAngle = 180.0 / Math.PI * Math.Atan2((polygonPointArray[2].Y - polygonPointArray[1].Y) + (polygonPointArray[3].Y - polygonPointArray[0].Y), (polygonPointArray[2].X - polygonPointArray[1].X) + (polygonPointArray[3].X - polygonPointArray[0].X));

            alphaLength = (LineLength(polygonPointArray[0], polygonPointArray[1]) + LineLength(polygonPointArray[3], polygonPointArray[2])) / 2;
            betaLength = (LineLength(polygonPointArray[0], polygonPointArray[3]) + LineLength(polygonPointArray[1], polygonPointArray[2])) / 2;

            gammaRatio = betaLength / alphaLength;


            status = StatusEnum.VALID;

            //Debug.Log("alphaAngle: " + alphaAngle + '\n' + "betaAngle: " + betaAngle + '\n' + "gamaRatio: " + gammaRatio);
            //Debug.Log(string.Format("Rhombus: {0,4:F0} {1,4:F0} {2,6:F0} {3,4:F0} {4,4:F0} {5,3:F0} {6,3:F0} {7,5:F2} {{{8,4:F0},{9,4:F0}}} {{{10,4:F0},{11,4:F0}}} {{{12,4:F0},{13,4:F0}}} {{{14,4:F0},{15,4:F0}}}", center.X, center.Y, area, alphaAngle, betaAngle, alphaLength, betaLength, gammaRatio, polygonPointArray[0].X, polygonPointArray[0].Y, polygonPointArray[1].X, polygonPointArray[1].Y, polygonPointArray[2].X, polygonPointArray[2].Y, polygonPointArray[3].X, polygonPointArray[3].Y) + " " + status);
        }

        /// <summary>
        /// Area of Convex Quadrilateral
        /// </summary>
        /// <param name="quadrilateralPointArray">
        /// @return </param>
        private static double AreaOfConvexQuadrilateral(Point[] quadrilateralPointArray)
        {

            //		Log.i(Constants.TAG, String.format( "Test: {%4.0f,%4.0f} {%4.0f,%4.0f} {%4.0f,%4.0f} {%4.0f,%4.0f}",
            //
            //				quadrilateralPointArray[0].x,
            //				quadrilateralPointArray[0].y,
            //				quadrilateralPointArray[1].x,
            //				quadrilateralPointArray[1].y,
            //				quadrilateralPointArray[2].x,
            //				quadrilateralPointArray[2].y,
            //				quadrilateralPointArray[3].x,
            //				quadrilateralPointArray[3].y));

            double area = AreaOfaTriangle(LineLength(quadrilateralPointArray[0], quadrilateralPointArray[1]), LineLength(quadrilateralPointArray[1], quadrilateralPointArray[2]), LineLength(quadrilateralPointArray[2], quadrilateralPointArray[0])) + AreaOfaTriangle(LineLength(quadrilateralPointArray[0], quadrilateralPointArray[3]), LineLength(quadrilateralPointArray[3], quadrilateralPointArray[2]), LineLength(quadrilateralPointArray[2], quadrilateralPointArray[0]));

            //Debug.Log(String.format( "Quadrilater Area: %6.0f", area));

            return area;
        }

        /// <summary>
        /// Adjust Quadrilater Vertices such that:
        ///   1) Element 0 has the minimum y coordinate.
        ///   2) Order draws a counter clockwise quadrilater.
        /// </summary>
        private bool AdjustQuadrilaterVertices()
        {

            // Find minimum.
            double y_min = double.MaxValue;
            int index = 0;
            for (int i = 0; i < polygonPointArray.Length; i++)
            {
                if (polygonPointArray[i].Y < y_min)
                {
                    y_min = polygonPointArray[i].Y;
                    index = i;
                }
            }

            // Rotate to get the minimum Y element ("index") as element 0.
            for (int i = 0; i < index; i++)
            {
                Point tmp = polygonPointArray[0];
                polygonPointArray[0] = polygonPointArray[1];
                polygonPointArray[1] = polygonPointArray[2];
                polygonPointArray[2] = polygonPointArray[3];
                polygonPointArray[3] = tmp;
            }

            // Return true if points are as depicted above and in a clockwise manner.
            if (polygonPointArray[1].X < polygonPointArray[3].X)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Area of a triangle specified by the three side lengths.
        /// </summary>
        /// <param name="a"> </param>
        /// <param name="b"> </param>
        /// <param name="c">
        /// @return </param>
        private static double AreaOfaTriangle(double a, double b, double c)
        {
            double area = Math.Sqrt((a + b - c) * (a - b + c) * (-a + b + c) * (a + b + c)) / 4.0;

            //Debug.Log(String.format( "Triangle Area: %4.0f %4.0f %4.0f %6.0f", a, b, c, area));

            return area;
        }


        /// <summary>
        /// Line length between two points.
        /// </summary>
        /// <param name="a"> </param>
        /// <param name="b">
        /// @return </param>
        private static double LineLength(Point a, Point b)
        {
            double length = Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));

            //		Log.i(Constants.TAG, String.format( "Line Length: %6.0f", length));

            return length;
        }

        /// <summary>
        /// Draw: Render actual polygon.
        /// </summary>
        /// <param name="rgba_gray_image"> </param>
        /// <param name="color"> </param>
        public virtual void Draw(Mat rgba_gray_image, Scalar color)
        {
            // Draw Polygon Edges
            Cv2.Polylines(rgba_gray_image, polygonMatrix, true, color, 3);
        }

        /// <summary>
        /// Remove Outlier Rhombi
        /// 
        /// For Alpha and Beta Angles:
        ///   1) Find Median Value: i.e. value in which half are greater and half are less.
        ///   2) Remove any that are > 10 degrees different
        /// 
        /// </summary>
        public static void RemovedOutlierRhombi(List<Rhombus> rhombusList)
        {

            const double angleOutlierTolerance = 10; // MenuAndParams.angleOutlierThresholdPaaram.value;

            if (rhombusList.Count < 3)
            {
                return;
            }

            int midIndex = rhombusList.Count / 2;

            rhombusList.Sort(new ComparatorAnonymousInnerClass());
            double medianAlphaAngle = rhombusList[midIndex].alphaAngle;

            rhombusList.Sort(new ComparatorAnonymousInnerClass2());
            double medianBetaAngle = rhombusList[midIndex].betaAngle;

            //Debug.Log(string.Format("Outlier Filter medianAlphaAngle={0,6:F0} medianBetaAngle={1,6:F0}", medianAlphaAngle, medianBetaAngle));

            // IEnumerator<Rhombus> rhombusItr = rhombusList.GetEnumerator();
            //while(rhombusItr.MoveNext())
            for(int i=0; i < rhombusList.Count; i++)
            {

                //Rhombus rhombus = rhombusItr.Current;
                Rhombus rhombus = rhombusList[i];

                if ((Math.Abs(rhombus.alphaAngle - medianAlphaAngle) > angleOutlierTolerance) || (Math.Abs(rhombus.betaAngle - medianBetaAngle) > angleOutlierTolerance))
                {
                    rhombus.status = StatusEnum.OUTLIER;
                    //rhombusItr.remove();
                    rhombusList.Remove(rhombus);
                    //Debug.Log(string.Format("Removed Outlier Rhombus with alphaAngle={0,6:F0} betaAngle={1,6:F0}", rhombus.alphaAngle, rhombus.betaAngle));
                }
            }
        }

        private class ComparatorAnonymousInnerClass : IComparer<Rhombus>
        {
            public ComparatorAnonymousInnerClass()
            {
            }

            public virtual int Compare(Rhombus lhs, Rhombus rhs)
            {
                return (int)(lhs.alphaAngle - rhs.alphaAngle);
            }
        }

        private class ComparatorAnonymousInnerClass2 : IComparer<Rhombus>
        {
            public ComparatorAnonymousInnerClass2()
            {
            }

            public virtual int Compare(Rhombus lhs, Rhombus rhs)
            {
                return (int)(lhs.betaAngle - rhs.betaAngle);
            }
        }

    }
}