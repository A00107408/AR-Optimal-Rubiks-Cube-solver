/* <summary
 * 
 * Augmented Reality Rubik Cube Application
 * A.I.T 2018
 * A00107408
 * Masters by Research
 * 
 * File Description:
 * Declares the "Prunning Table" arrays in memory asynchronously.
 * These arrays are used in the heuristic cube solution search. 
 * 
 * Acknowledgments:
 * Original Author: Steven P. Punte (aka Android Steve : android.steve@cl-sw.com)
 * Date: April 25th 2015
 *   
 * <summary> */

namespace KociembaTwoPhase
{
    /// <summary>
    /// <para>This class provides (very basic) control over the loading of the pruning tables in <seealso cref="CoordCube"/> for Herbert Kociemba's 
    /// <i>Twophase Solver</i>.
    /// 
    /// </para>
    /// <para>This class will mostly be used like this:
    /// <pre><code>
    /// PruneTableLoader tableLoader = new PruneTableLoader();
    /// while (!tableLoader.loadingFinished())
    ///       loadNext();
    /// </pre></code>
    /// 
    /// </para>
    /// <para><i>Note</i>: For this class to have any effect, you must have replaced <seealso cref="CoordCube"/> with the custom 
    /// implementation beforehand.
    /// 
    /// @author Herbert Kociemba <i>(generation of the pruning tables)</i>
    /// @author Elias Frantar <i>(implementation of this class)</i>
    /// @version 2014-8-16
    /// </para>
    /// </summary>
    public class PruneTableLoader
    {
        private const int TABLES = 12; // there are 12 different pruning tables to load

        private int tablesLoaded; // the number of already loaded tables

        /// <summary>
        /// Constructor<br>
        /// Number of tables loaded is set to 0.
        /// </summary>
        public PruneTableLoader()
        {
            tablesLoaded = 0;
        }

        /// <summary>
        /// Loads the next pruning table if and only if it is <i>null</i>.<br>
        /// Equivalent to <code>loadNext(false);</code>
        /// </summary>
        public virtual void LoadNext()
        {
            LoadNext(false);
        }

        /// <summary>
        /// Loads the next pruning table. </summary>
        /// <param name="force"> if true override it even if it already exists; if false only load when it is <i>null</i> </param>
        public virtual void LoadNext(bool force)
        {
            switch (tablesLoaded++)
            {
                case 0:
                    LoadTwistMoves(force);
                    break;
                case 1:
                    LoadFlipMoves(force);
                    break;
                case 2:
                    LoadFRtoBRMoves(force);
                    break;
                case 3:
                    LoadURFtoDLFMoves(force);
                    break;
                case 4:
                    LoadURtoDFMoves(force);
                    break;
                case 5:
                    LoadURtoULMoves(force);
                    break;
                case 6:
                    LoadUBtoDFMoves(force);
                    break;
                case 7:
                    MergeURtoULandUBtoDF(force);
                    break;
                case 8:
                    LoadSliceURFtoDLFParityPrun(force);
                    break;
                case 9:
                    LoadSliceURtoDFParityPrun(force);
                    break;
                case 10:
                    LoadSliceTwistPrune(force);
                    break;
                case 11:
                    LoadSliceFlipPrune(force);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Determines if all pruning tables have already been loaded by using this class. </summary>
        /// <returns> true if all tables have already been loaded; false otherwise </returns>
        public virtual bool LoadingFinished()
        {
            return tablesLoaded >= TABLES;
        }

        /*
        * Methods for loading each individual pruning table.
        * @param force if true override the table even it already exists; if false only load it when it is <i>null</i>
        * 
        * Code and commments have been directly copied from {@author Herbert Kociemba}'s original CoordCube-class.
        */

        // ******************************************Phase 1 move tables*****************************************************

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Move table for the twists of the corners
        // twist < 2187 in phase 2.
        // twist = 0 in phase 2.
        private void LoadTwistMoves(bool force)
        {
            /* only load if not already loaded */
            if (!force && CoordCube.twistMove != null)
            {
                return;
            }
            CoordCube.twistMove = RectangularArrays.ReturnRectangularShortArray(CoordCube.N_TWIST, CoordCube.N_MOVE);

            CubieCube a = new CubieCube();
            for (short i = 0; i < CoordCube.N_TWIST; i++)
            {
                a.Twist = i;
                for (int j = 0; j < 6; j++)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        a.CornerMultiply(CubieCube.moveCube[j]);
                        CoordCube.twistMove[i][3 * j + k] = a.Twist;
                    }
                    a.CornerMultiply(CubieCube.moveCube[j]); // 4. faceturn restores a
                }
            }
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Move table for the flips of the edges
        // flip < 2048 in phase 1
        // flip = 0 in phase 2.
        private void LoadFlipMoves(bool force)
        {
            if (!force && CoordCube.flipMove != null)
            {
                return;
            }
            //JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
            //ORIGINAL LINE: flipMove = new short[N_FLIP][N_MOVE];
            CoordCube.flipMove = RectangularArrays.ReturnRectangularShortArray(CoordCube.N_FLIP, CoordCube.N_MOVE);

            CubieCube a = new CubieCube();
            for (short i = 0; i < CoordCube.N_FLIP; i++)
            {
                a.Flip = i;
                for (int j = 0; j < 6; j++)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        a.EdgeMultiply(CubieCube.moveCube[j]);
                        CoordCube.flipMove[i][3 * j + k] = a.Flip;
                    }
                    a.EdgeMultiply(CubieCube.moveCube[j]); // a
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
            internal static short[][] ReturnRectangularShortArray(int size1, int size2)
            {
                short[][] newArray = new short[size1][];
                for (int array1 = 0; array1 < size1; array1++)
                {
                    newArray[array1] = new short[size2];
                }

                return newArray;
            }
        }

        // ***********************************Phase 1 and 2 movetable********************************************************

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Move table for the four UD-slice edges FR, FL, Bl and BR
        // FRtoBRMove < 11880 in phase 1
        // FRtoBRMove < 24 in phase 2
        // FRtoBRMove = 0 for solved cube
        private void LoadFRtoBRMoves(bool force)
        {
            if (!force && CoordCube.FRtoBR_Move != null)
            {
                return;
            }
            
            CoordCube.FRtoBR_Move = RectangularArrays.ReturnRectangularShortArray(CoordCube.N_FRtoBR, CoordCube.N_MOVE);

            CubieCube a = new CubieCube();
            for (short i = 0; i < CoordCube.N_FRtoBR; i++)
            {
                a.FRtoBR = i;
                for (int j = 0; j < 6; j++)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        a.EdgeMultiply(CubieCube.moveCube[j]);
                        CoordCube.FRtoBR_Move[i][3 * j + k] = a.FRtoBR;
                    }
                    a.EdgeMultiply(CubieCube.moveCube[j]);
                }
            }
        }

        // *******************************************Phase 1 and 2 movetable************************************************

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Move table for permutation of six corners. The positions of the DBL and DRB corners are determined by the parity.
        // URFtoDLF < 20160 in phase 1
        // URFtoDLF < 20160 in phase 2
        // URFtoDLF = 0 for solved cube.
        private void LoadURFtoDLFMoves(bool force)
        {
            if (!force && CoordCube.URFtoDLF_Move != null)
            {
                return;
            }
            //JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
            //ORIGINAL LINE: URFtoDLF_Move = new short[N_URFtoDLF][N_MOVE];
            CoordCube.URFtoDLF_Move = RectangularArrays.ReturnRectangularShortArray(CoordCube.N_URFtoDLF, CoordCube.N_MOVE);

            CubieCube a = new CubieCube();
            for (short i = 0; i < CoordCube.N_URFtoDLF; i++)
            {
                a.URFtoDLF = i;
                for (int j = 0; j < 6; j++)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        a.CornerMultiply(CubieCube.moveCube[j]);
                        CoordCube.URFtoDLF_Move[i][3 * j + k] = a.URFtoDLF;
                    }
                    a.CornerMultiply(CubieCube.moveCube[j]);
                }
            }
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Move table for the permutation of six U-face and D-face edges in phase2. The positions of the DL and DB edges are
        // determined by the parity.
        // URtoDF < 665280 in phase 1
        // URtoDF < 20160 in phase 2
        // URtoDF = 0 for solved cube.
        private void LoadURtoDFMoves(bool force)
        {
            if (!force && CoordCube.URtoDF_Move != null)
            {
                return;
            }
            //JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
            //ORIGINAL LINE: URtoDF_Move = new short[N_URtoDF][N_MOVE];
            CoordCube.URtoDF_Move = RectangularArrays.ReturnRectangularShortArray(CoordCube.N_URtoDF, CoordCube.N_MOVE);

            CubieCube a = new CubieCube();
            for (short i = 0; i < CoordCube.N_URtoDF; i++)
            {
                a.URtoDF = i;
                for (int j = 0; j < 6; j++)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        a.EdgeMultiply(CubieCube.moveCube[j]);
                        CoordCube.URtoDF_Move[i][3 * j + k] = (short)a.URtoDF; // Table values are only valid for phase 2 moves! For phase 1 moves, casting to short is not possible.
                    }
                    a.EdgeMultiply(CubieCube.moveCube[j]);
                }
            }
        }

        // **************************helper move tables to compute URtoDF for the beginning of phase2************************

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Move table for the three edges UR,UF and UL in phase1.	
        private void LoadURtoULMoves(bool force)
        {
            if (!force && CoordCube.URtoUL_Move != null)
            {
                return;
            }
            //JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
            //ORIGINAL LINE: URtoUL_Move = new short[N_URtoUL][N_MOVE];
            CoordCube.URtoUL_Move = RectangularArrays.ReturnRectangularShortArray(CoordCube.N_URtoUL, CoordCube.N_MOVE);

            CubieCube a = new CubieCube();
            for (short i = 0; i < CoordCube.N_URtoUL; i++)
            {
                a.URtoUL = i;
                for (int j = 0; j < 6; j++)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        a.EdgeMultiply(CubieCube.moveCube[j]);
                        CoordCube.URtoUL_Move[i][3 * j + k] = a.URtoUL;
                    }
                    a.EdgeMultiply(CubieCube.moveCube[j]);
                }
            }
        }


        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Move table for the three edges UB,DR and DF in phase1.
        private void LoadUBtoDFMoves(bool force)
        {
            if (!force && CoordCube.UBtoDF_Move != null)
            {
                return;
            }
            //JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
            //ORIGINAL LINE: UBtoDF_Move = new short[N_UBtoDF][N_MOVE];
            CoordCube.UBtoDF_Move = RectangularArrays.ReturnRectangularShortArray(CoordCube.N_UBtoDF, CoordCube.N_MOVE);

            CubieCube a = new CubieCube();
            for (short i = 0; i < CoordCube.N_UBtoDF; i++)
            {
                a.UBtoDF = i;
                for (int j = 0; j < 6; j++)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        a.EdgeMultiply(CubieCube.moveCube[j]);
                        CoordCube.UBtoDF_Move[i][3 * j + k] = a.UBtoDF;
                    }
                    a.EdgeMultiply(CubieCube.moveCube[j]);
                }
            }
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Table to merge the coordinates of the UR,UF,UL and UB,DR,DF edges at the beginning of phase2
        private void MergeURtoULandUBtoDF(bool force)
        {
            if (!force && CoordCube.MergeURtoULandUBtoDF != null)
            {
                return;
            }
            //JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
            //ORIGINAL LINE: MergeURtoULandUBtoDF = new short[336][336];
            CoordCube.MergeURtoULandUBtoDF = RectangularArrays.ReturnRectangularShortArray(336, 336);

            /* for i, j < 336 the six edges UR,UF,UL,UB,DR,DF are not in the UD-slice and the index is < 20160 */
            for (short uRtoUL = 0; uRtoUL < 336; uRtoUL++)
            {
                for (short uBtoDF = 0; uBtoDF < 336; uBtoDF++)
                {
                    CoordCube.MergeURtoULandUBtoDF[uRtoUL][uBtoDF] = (short)CubieCube.GetURtoDF(uRtoUL, uBtoDF);
                }
            }
        }

        // ****************************************Pruning tables for the search*********************************************

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Pruning table for the permutation of the corners and the UD-slice edges in phase2.
        // The pruning table entries give a lower estimation for the number of moves to reach the solved cube.
        private void LoadSliceURFtoDLFParityPrun(bool force)
        {
            if (!force && CoordCube.Slice_URFtoDLF_Parity_Prun != null)
            {
                return;
            }
            CoordCube.Slice_URFtoDLF_Parity_Prun = new sbyte[CoordCube.N_SLICE2 * CoordCube.N_URFtoDLF * CoordCube.N_PARITY / 2];

            for (int i = 0; i < CoordCube.N_SLICE2 * CoordCube.N_URFtoDLF * CoordCube.N_PARITY / 2; i++)
            {
                CoordCube.Slice_URFtoDLF_Parity_Prun[i] = -1;
            }

            int depth = 0;
            CoordCube.SetPruning(CoordCube.Slice_URFtoDLF_Parity_Prun, 0, (sbyte)0);
            int done = 1;
            while (done != CoordCube.N_SLICE2 * CoordCube.N_URFtoDLF * CoordCube.N_PARITY)
            {
                for (int i = 0; i < CoordCube.N_SLICE2 * CoordCube.N_URFtoDLF * CoordCube.N_PARITY; i++)
                {
                    int parity = i % 2;
                    int URFtoDLF = (i / 2) / CoordCube.N_SLICE2;
                    int slice = (i / 2) % CoordCube.N_SLICE2;
                    if (CoordCube.GetPruning(CoordCube.Slice_URFtoDLF_Parity_Prun, i) == depth)
                    {
                        for (int j = 0; j < 18; j++)
                        {
                            switch (j)
                            {
                                case 3:
                                case 5:
                                case 6:
                                case 8:
                                case 12:
                                case 14:
                                case 15:
                                case 17:
                                    continue;
                                default:
                                    int newSlice = CoordCube.FRtoBR_Move[slice][j];
                                    int newURFtoDLF = CoordCube.URFtoDLF_Move[URFtoDLF][j];
                                    int newParity = CoordCube.parityMove[parity][j];
                                    if (CoordCube.GetPruning(CoordCube.Slice_URFtoDLF_Parity_Prun, (CoordCube.N_SLICE2 * newURFtoDLF + newSlice) * 2 + newParity) == 0x0f)
                                    {
                                        CoordCube.SetPruning(CoordCube.Slice_URFtoDLF_Parity_Prun, (CoordCube.N_SLICE2 * newURFtoDLF + newSlice) * 2 + newParity, (sbyte)(depth + 1));
                                        done++;
                                    }
                                    break;
                            }
                        }
                    }
                }
                depth++;
            }
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Pruning table for the permutation of the edges in phase2.
        // The pruning table entries give a lower estimation for the number of moves to reach the solved cube.
        private void LoadSliceURtoDFParityPrun(bool force)
        {
            if (!force && CoordCube.Slice_URtoDF_Parity_Prun != null)
            {
                return;
            }
            CoordCube.Slice_URtoDF_Parity_Prun = new sbyte[CoordCube.N_SLICE2 * CoordCube.N_URtoDF * CoordCube.N_PARITY / 2];

            for (int i = 0; i < CoordCube.N_SLICE2 * CoordCube.N_URtoDF * CoordCube.N_PARITY / 2; i++)
            {
                CoordCube.Slice_URtoDF_Parity_Prun[i] = -1;
            }

            int depth = 0;
            CoordCube.SetPruning(CoordCube.Slice_URtoDF_Parity_Prun, 0, (sbyte)0);
            int done = 1;
            while (done != CoordCube.N_SLICE2 * CoordCube.N_URtoDF * CoordCube.N_PARITY)
            {
                for (int i = 0; i < CoordCube.N_SLICE2 * CoordCube.N_URtoDF * CoordCube.N_PARITY; i++)
                {
                    int parity = i % 2;
                    int URtoDF = (i / 2) / CoordCube.N_SLICE2;
                    int slice = (i / 2) % CoordCube.N_SLICE2;
                    if (CoordCube.GetPruning(CoordCube.Slice_URtoDF_Parity_Prun, i) == depth)
                    {
                        for (int j = 0; j < 18; j++)
                        {
                            switch (j)
                            {
                                case 3:
                                case 5:
                                case 6:
                                case 8:
                                case 12:
                                case 14:
                                case 15:
                                case 17:
                                    continue;
                                default:
                                    int newSlice = CoordCube.FRtoBR_Move[slice][j];
                                    int newURtoDF = CoordCube.URtoDF_Move[URtoDF][j];
                                    int newParity = CoordCube.parityMove[parity][j];
                                    if (CoordCube.GetPruning(CoordCube.Slice_URtoDF_Parity_Prun, (CoordCube.N_SLICE2 * newURtoDF + newSlice) * 2 + newParity) == 0x0f)
                                    {
                                        CoordCube.SetPruning(CoordCube.Slice_URtoDF_Parity_Prun, (CoordCube.N_SLICE2 * newURtoDF + newSlice) * 2 + newParity, (sbyte)(depth + 1));
                                        done++;
                                    }
                                    break;
                            }
                        }
                    }
                }
                depth++;
            }
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Pruning table for the twist of the corners and the position (not permutation) of the UD-slice edges in phase1
        // The pruning table entries give a lower estimation for the number of moves to reach the H-subgroup.
        private void LoadSliceTwistPrune(bool force)
        {
            if (!force && CoordCube.Slice_Twist_Prun != null)
            {
                return;
            }
            CoordCube.Slice_Twist_Prun = new sbyte[CoordCube.N_SLICE1 * CoordCube.N_TWIST / 2 + 1];

            for (int i = 0; i < CoordCube.N_SLICE1 * CoordCube.N_TWIST / 2 + 1; i++)
            {
                CoordCube.Slice_Twist_Prun[i] = -1;
            }

            int depth = 0;
            CoordCube.SetPruning(CoordCube.Slice_Twist_Prun, 0, (sbyte)0);
            int done = 1;
            while (done != CoordCube.N_SLICE1 * CoordCube.N_TWIST)
            {
                for (int i = 0; i < CoordCube.N_SLICE1 * CoordCube.N_TWIST; i++)
                {
                    int twist = i / CoordCube.N_SLICE1, slice = i % CoordCube.N_SLICE1;
                    if (CoordCube.GetPruning(CoordCube.Slice_Twist_Prun, i) == depth)
                    {
                        for (int j = 0; j < 18; j++)
                        {
                            int newSlice = CoordCube.FRtoBR_Move[slice * 24][j] / 24;
                            int newTwist = CoordCube.twistMove[twist][j];
                            if (CoordCube.GetPruning(CoordCube.Slice_Twist_Prun, CoordCube.N_SLICE1 * newTwist + newSlice) == 0x0f)
                            {
                                CoordCube.SetPruning(CoordCube.Slice_Twist_Prun, CoordCube.N_SLICE1 * newTwist + newSlice, (sbyte)(depth + 1));
                                done++;
                            }
                        }
                    }
                }
                depth++;
            }
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Pruning table for the flip of the edges and the position (not permutation) of the UD-slice edges in phase1
        // The pruning table entries give a lower estimation for the number of moves to reach the H-subgroup.
        private void LoadSliceFlipPrune(bool force)
        {
            if (!force && CoordCube.Slice_Flip_Prun != null)
            {
                return;
            }
            CoordCube.Slice_Flip_Prun = new sbyte[CoordCube.N_SLICE1 * CoordCube.N_FLIP / 2];

            for (int i = 0; i < CoordCube.N_SLICE1 * CoordCube.N_FLIP / 2; i++)
            {
                CoordCube.Slice_Flip_Prun[i] = -1;
            }

            int depth = 0;
            CoordCube.SetPruning(CoordCube.Slice_Flip_Prun, 0, (sbyte)0);
            int done = 1;
            while (done != CoordCube.N_SLICE1 * CoordCube.N_FLIP)
            {
                for (int i = 0; i < CoordCube.N_SLICE1 * CoordCube.N_FLIP; i++)
                {
                    int flip = i / CoordCube.N_SLICE1, slice = i % CoordCube.N_SLICE1;
                    if (CoordCube.GetPruning(CoordCube.Slice_Flip_Prun, i) == depth)
                    {
                        for (int j = 0; j < 18; j++)
                        {
                            int newSlice = CoordCube.FRtoBR_Move[slice * 24][j] / 24;
                            int newFlip = CoordCube.flipMove[flip][j];
                            if (CoordCube.GetPruning(CoordCube.Slice_Flip_Prun, CoordCube.N_SLICE1 * newFlip + newSlice) == 0x0f)
                            {
                                CoordCube.SetPruning(CoordCube.Slice_Flip_Prun, CoordCube.N_SLICE1 * newFlip + newSlice, (sbyte)(depth + 1));
                                done++;
                            }
                        }
                    }
                }
                depth++;
            }
        }

    }
}
