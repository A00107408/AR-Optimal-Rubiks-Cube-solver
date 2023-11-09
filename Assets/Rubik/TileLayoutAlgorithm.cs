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
    using System.Text;
    using System.Linq;
    using UnityEngine;

    public class TileLayoutAlgorithm
    {
        private static IList<Rhombus> rhombusList = null;

        private static double alphaAngle;
        private static double betaAngle;

        /// <summary>
        /// Initial Layout Algorithm
        /// 
        /// Attempt a two-dimensional sort into, of course, a 3x3 array.
        /// 
        /// Algorithm:
        ///   o  X and Y axis values are converted to "alpha" and "beta" position
        ///      in, roughly, units of pixels.
        /// 
        ///   o  Along alpha and beta axis, but not simultaneously, sort Rohmbi
        ///      w.r.t., alpha or beta, into three sets: Low, Mid, and High.
        /// 
        ///      Some conditions on sort:
        ///      o All sets must have at least 1 Rhombus.
        ///      o Minimize: Sum {   Sum  { (R_i - R_j)^2 } } where i and j are in the same set.
        /// 
        ///   o  Populate Rhombus Face Array according to sorted sets.   
        /// </summary>
        /// <param name="faceRhombusArray"> 
        /// 
        /// </param>
        /// <param name="rubikFace"> </param>
        /// <returns>  </returns>
        public static bool DoInitialLayout(IList<Rhombus> _rhombusList, Rhombus[][] _rhombusFaceArray, double _alphaAngle, double _betaAngle)
        {
            rhombusList = _rhombusList;
            alphaAngle = _alphaAngle;
            betaAngle = _betaAngle;

            // Sort Rhombi into three sets along alpha axis.
            List<List<Rhombus>> alphaListOfSets = CreateOptimizedListOfRhombiSets(new AlphaAngleComparatorAnonymousInnerClass());

            // Sort Rhombi into three sets along beta axis.
            List<List<Rhombus>> betaListOfSets = CreateOptimizedListOfRhombiSets(new BetaAngleComparatorAnonymousInnerClass());
     
            // Fill Rhombus Face Array
            // Loop over N and M indicies.
            for (int n = 0; n < 3; n++)
            {
                for (int m = 0; m < 3; m++)
                {

                    // Get candidate Rhombi that have the M and N indices.
                    List<Rhombus> alphaSet = alphaListOfSets[n];
                    List<Rhombus> betaSet = betaListOfSets[m];

                    // Find Rhmobi that have both the desired M and N indices.
                    IList<Rhombus> commonElements = FindCommonElements(alphaSet, betaSet);

                    if (commonElements.Count == 0)
                    {
                        _rhombusFaceArray[n][m] = null; // No Rhombus for this tile
                    }

                    else if (commonElements.Count == 1)
                    {
                        _rhombusFaceArray[n][m] = commonElements[0]; // Desired result
                    }

                    else
                    {
                        // Problem, more than one Rhombus seem candidate for this tile location.
                        // Just use first
                        _rhombusFaceArray[n][m] = commonElements[0];
                        //Debug.Log("Excess Rhombi Candidate(s) ");
                        // =+= Possibly put in extra set ??
                    }
                }
            }

            /*Debug.Log("Alpha Low  Set: " + RhombiSetToString(alphaListOfSets[0]));
            Debug.Log("Alpha Mid  Set: " + RhombiSetToString(alphaListOfSets[1]));
            Debug.Log("Alpha High Set: " + RhombiSetToString(alphaListOfSets[2]));
            Debug.Log("Beta  Low  Set: " + RhombiSetToString(betaListOfSets[0]));
            Debug.Log("Beta  Mid  Set: " + RhombiSetToString(betaListOfSets[1]));
            Debug.Log("Beta  High Set: " + RhombiSetToString(betaListOfSets[2]));*/


            // Diagnostic Print
            /*Debug.Log(string.Format(" m:n|--------------0--------------|---------------1-------------|-------------2---------------|"));
            Debug.Log(string.Format(" 0  |{0}|{1}|{2}|", RhombusToString(_rhombusFaceArray[0][0]), RhombusToString(_rhombusFaceArray[1][0]), RhombusToString(_rhombusFaceArray[2][0])));
            Debug.Log(string.Format(" 1  |{0}|{1}|{2}|", RhombusToString(_rhombusFaceArray[0][1]), RhombusToString(_rhombusFaceArray[1][1]), RhombusToString(_rhombusFaceArray[2][1])));
            Debug.Log(string.Format(" 2  |{0}|{1}|{2}|", RhombusToString(_rhombusFaceArray[0][2]), RhombusToString(_rhombusFaceArray[1][2]), RhombusToString(_rhombusFaceArray[2][2])));
            Debug.Log(string.Format("    |-----------------------------|-----------------------------|-----------------------------|"));*/

            
            // Check that there is at least on Rhombus in each row and column.
            if (_rhombusFaceArray[0][0] == null && _rhombusFaceArray[0][1] == null && _rhombusFaceArray[0][2] == null)
            {
                return false;
            }
            if (_rhombusFaceArray[1][0] == null && _rhombusFaceArray[1][1] == null && _rhombusFaceArray[1][2] == null)
            {
                return false;
            }
            if (_rhombusFaceArray[2][0] == null && _rhombusFaceArray[2][1] == null && _rhombusFaceArray[2][2] == null)
            {
                return false;
            }
            if (_rhombusFaceArray[0][0] == null && _rhombusFaceArray[1][0] == null && _rhombusFaceArray[2][0] == null)
            {
                return false;
            }
            if (_rhombusFaceArray[0][1] == null && _rhombusFaceArray[1][1] == null && _rhombusFaceArray[2][1] == null)
            {
                return false;
            }
            if (_rhombusFaceArray[0][2] == null && _rhombusFaceArray[1][2] == null && _rhombusFaceArray[2][2] == null)
            {
                return false;
            }

            return true;
        }

        private class AlphaAngleComparatorAnonymousInnerClass : IComparer<Rhombus>
        {
            public AlphaAngleComparatorAnonymousInnerClass()
            {
            }

            public virtual int Compare(Rhombus rhombus0, Rhombus rhombus1)
            {
                return (GetAlpha(rhombus0) - GetAlpha(rhombus1));
            }
        }

        private class BetaAngleComparatorAnonymousInnerClass : IComparer<Rhombus>
        {
            public BetaAngleComparatorAnonymousInnerClass()
            {
            }

            public virtual int Compare(Rhombus rhombus0, Rhombus rhombus1)
            {
                return (GetBeta(rhombus0) - GetBeta(rhombus1));
            }
        }

        /// <summary>
        /// Find Common Elements and return in a new List.
        ///    
        /// </summary>
        /// <param name="alphaSet"> </param>
        /// <param name="betaSet">
        /// @return </param>
        private static List<Rhombus> FindCommonElements(List<Rhombus> alphaSet, List<Rhombus> betaSet)
        {
            //List<Rhombus> result = new List<Rhombus>(alphaSet);
            // result.RetainAll(betaSet);

            List<Rhombus> result = new List<Rhombus>(); 
            result = alphaSet.Intersect(betaSet).ToList();
            return result;
        }

        private static int GetAlpha(Rhombus rhombus)
        {
            return (int)(rhombus.center.X * Math.Cos(alphaAngle) + rhombus.center.Y * Math.Sin(alphaAngle));
        }

        private static int GetBeta(Rhombus rhombus)
        {
            return (int)(rhombus.center.X * Math.Cos(betaAngle) + rhombus.center.Y * Math.Sin(betaAngle));
        }

        /// <summary>
        /// Create Optimized List Of Rhombi Sets with respect to provided comparator.
        /// 
        /// Creates three set of Rhombi: Low, Medium, and High.  
        /// </summary>
        /// <param name="comparator">
        /// @return </param>
        private static List<List<Rhombus>> CreateOptimizedListOfRhombiSets(IComparer<Rhombus> comparator)
        {
            double best_error = double.PositiveInfinity;
            int best_p = 0;
            int best_q = 0;

            int n = rhombusList.Count;

           // List<List<Rhombus>> result = new List<List<Rhombus>>();         // Eoghan - To suit Range() 0 index crashes
          ////  if (n > 0)
          //  {

                // First just perform a linear sort: smallest to largest.
                List<Rhombus> sortedRhombusList = new List<Rhombus>(rhombusList);
                sortedRhombusList.Sort(comparator);

            /*foreach (Rhombus rhombus in sortedRhombusList) {
                //Debug.Log(String.Format("Sorted Rhombi List: x=%4.0f y=%4.0f alpha=%d beta=%d", rhombus.center.X, rhombus.center.Y, GetAlpha(rhombus), GetBeta(rhombus)));
                Debug.Log("%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%");
                Debug.Log("X: " +rhombus.center.X);
                Debug.Log("Y: " + rhombus.center.Y);
                Debug.Log("Alpha: " + GetAlpha(rhombus));
                Debug.Log("Beta: " + GetBeta(rhombus));
                Debug.Log("%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%");
            }*/

            // Next search overall all partition possibilities, and find that with the least error w.r.t. provided comparator
         
                for (int p = 1; p < n - 1; p++)
                {
           
                    for (int q = p + 1; q < n; q++)
                    {
          
                    double error = CalculateErrorAccordingToPartition_P_Q(sortedRhombusList, comparator, p, q);
          
                        if (error < best_error)
                        {
                            best_error = error;
                            best_p = p;
                            best_q = q;
                        }
                    }
                }


            //Debug.Log(String.format("createOptimizedListOfRhombiSets: Selected p=%d and q=%d N=%d", best_p, best_q, n));

            List<List<Rhombus>> result = new List<List<Rhombus>>();

            result.Add(GetRangedList(sortedRhombusList, 0, best_p));
            result.Add(GetRangedList(sortedRhombusList, best_p, best_q));
            result.Add(GetRangedList(sortedRhombusList, best_q, n));
          
            return result;
        }

        /// <summary>
        /// Calculate Error According To Partition P and Q and provided comparator
        /// 
        /// Low Set  are elements 0 to P-1
        /// Mid Set  are elements P to Q-1
        /// High Set are elements Q to N-1
        /// 
        /// Sum of error of each set created by partition P and Q
        /// </summary>
        /// <param name="sortedRhombusList"> </param>
        /// <param name="comparator"> </param>
        /// <param name="p"> </param>
        /// <param name="q"> </param>
        /// <returns> sum square of error </returns>
        private static int CalculateErrorAccordingToPartition_P_Q(List<Rhombus> sortedRhombusList, IComparer<Rhombus> comparator, int p, int q)
        {
            int n = sortedRhombusList.Count;
           
            int sum = CalculateSumSquaredErrorOfSet(GetRangedList(sortedRhombusList, 0, p), comparator) + CalculateSumSquaredErrorOfSet(GetRangedList(sortedRhombusList, p, q), comparator) + CalculateSumSquaredErrorOfSet(GetRangedList(sortedRhombusList,q, n), comparator);
            //Debug.Log(String.Format("calculateErrorAccordingToPartition_P_Q: sum=%d for p=%d and q=%d", sum, p, q));

            return sum;
        }

        /// <summary>
        /// Calculate Sum Squared Error Of Set with respect to provided comparator.
        /// </summary>
        /// <param name="subList"> </param>
        /// <param name="comparator">
        /// @return </param>
        private static int CalculateSumSquaredErrorOfSet(IList<Rhombus> subList, IComparer<Rhombus> comparator)
        {
            int n = subList.Count;
            int sumSquared = 0;

            for (int i = 0; i < n - 1; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    int cmp = comparator.Compare(subList[i], subList[j]);
                    sumSquared += cmp * cmp;
                }
            }

            //Debug.Log(String.Format("calculateSumSquaredErrorOfSet: sum=%d for size=%d", sumSquared, n));
            //Debug.Log("calculateSumSquaredErrorOfSet: " +sumSquared +n);

            return sumSquared;
        }

        private static string RhombiSetToString(ICollection<Rhombus> collection)
        {
            StringBuilder buffer = new StringBuilder();

            foreach (Rhombus rhombus in collection)
            {
                buffer.Append(RhombusToString(rhombus));
            }

            return buffer.ToString();
        }


        private static string RhombusToString(Rhombus rhombus)
        {
            if (rhombus == null)
            {
                return "----------null---------------";
            }
            else
            {
                return string.Format("{{x={0,4:F0} y={1,4:F0} a={2,4:D} b={3,4:D}}}", rhombus.center.X, rhombus.center.Y, GetAlpha(rhombus), GetBeta(rhombus));
            }
        }

        public static List<Rhombus> GetRangedList(List<Rhombus> list, int startIndex, int endIndex)
        {
            List<Rhombus> returnList = new List<Rhombus>();
            if (startIndex >= 0 && endIndex <= list.Count)
            {
                for (int i = startIndex; i <= list.Count; i++)
                {
                    if (i < endIndex)
                    {
                        returnList.Add(list[i]);
                    }
                }
            }
            return returnList;
        }

    }
}