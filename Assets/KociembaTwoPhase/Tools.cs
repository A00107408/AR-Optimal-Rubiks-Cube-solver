/* <summary
 * 
 * Augmented Reality Rubik Cube Application
 * A.I.T 2018
 * A00107408
 * Masters by Research
 * 
 * File Description:
 * Tools used in solution search.
 * 
 * Acknowledgments:
 * Original @author: Elias Frantar.
 * Translated Kociemba algorithm to java.
 *   
 * <summary> */

namespace KociembaTwoPhase
{
    using System;
    using UnityEngine;

    public class Tools
    {
        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Check if the cube string s represents a solvable cube.
        // 0: Cube is solvable
        // -1: There is not exactly one facelet of each colour
        // -2: Not all 12 edges exist exactly once
        // -3: Flip error: One edge has to be flipped
        // -4: Not all corners exist exactly once
        // -5: Twist error: One corner has to be twisted
        // -6: Parity error: Two corners or two edges have to be exchanged
        // 
        /// <summary>
        /// Check if the cube definition string s represents a solvable cube.
        /// </summary>
        /// <param name="s"> is the cube definition string , see <seealso cref="Facelet"/> </param>
        /// <returns> 0: Cube is solvable<br>
        ///         -1: There is not exactly one facelet of each colour<br>
        ///         -2: Not all 12 edges exist exactly once<br>
        ///         -3: Flip error: One edge has to be flipped<br>
        ///         -4: Not all 8 corners exist exactly once<br>
        ///         -5: Twist error: One corner has to be twisted<br>
        ///         -6: Parity error: Two corners or two edges have to be exchanged </returns>
        public static int Verify(string s)
        {
            Debug.Log("%%%%%%%%%%%%%%%%%%%%%%% In Verify %%%%%%%%%%%%%%%%%%%%%");
            int[] count = new int[6];
        
            try
            {
                for (int i = 0; i < 54; i++)
                {
                    // count[Color.valueOf(s.Substring(i, 1)).ordinal()]++;

                    //count[(int)Enum.Parse(typeof(Color), s.Substring(i, 1))]++;
                    Debug.Log("PARSE: " + (int)Enum.Parse(typeof(Color), s.Substring(i, 1)) );
                }
            }
            catch (Exception)
            {
               Debug.Log("54 exception: ");
               return -1;
            }

            for (int i = 0; i < 6; i++)
            {
                if (count[i] != 9)
                {
                      Debug.Log("Wrong amount of tiles on a face ?? ");
                      return -1;
                }  
            }

            //Debug.Log("count: " + count);

            FaceCube fc = new FaceCube(s);
            CubieCube cc = fc.ToCubieCube();

            //Debug.Log("cc.Verify(): " + cc.Verify());

            return cc.Verify();
        }

    }
}