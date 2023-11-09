/* <summary
 * 
 * Augmented Reality Rubik Cube Application
 * A.I.T 2018
 * A00107408
 * Masters by Research
 * 
 * File Description:
 * Heuristic search for cube solution using pruning tables.
 * 
 * Acknowledgments:
 * Original @author: Elias Frantar.
 * Translated Kociemba algorithm to java.
 *   
 * <summary> */

namespace KociembaTwoPhase
{
    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    //Representation of the cube on the coordinate level
    public class CoordCube
    {
        internal const short N_TWIST = 2187; // 3^7 possible corner orientations
        internal const short N_FLIP = 2048; // 2^11 possible edge flips
        internal const short N_SLICE1 = 495; // 12 choose 4 possible positions of FR,FL,BL,BR edges
        internal const short N_SLICE2 = 24; // 4! permutations of FR,FL,BL,BR edges in phase2
        internal const short N_PARITY = 2; // 2 possible corner parities
        internal const short N_URFtoDLF = 20160; // 8!/(8-6)! permutation of URF,UFL,ULB,UBR,DFR,DLF corners
        internal const short N_FRtoBR = 11880; // 12!/(12-4)! permutation of FR,FL,BL,BR edges
        internal const short N_URtoUL = 1320; // 12!/(12-3)! permutation of UR,UF,UL edges
        internal const short N_UBtoDF = 1320; // 12!/(12-3)! permutation of UB,DR,DF edges
        internal const short N_URtoDF = 20160; // 8!/(8-6)! permutation of UR,UF,UL,UB,DR,DF edges in phase2

        internal const int N_URFtoDLB = 40320; // 8! permutations of the corners
        internal const int N_URtoBR = 479001600; // 8! permutations of the corners

        internal const short N_MOVE = 18;

        // All coordinates are 0 for a solved cube except for UBtoDF, which is 114
        internal short twist;
        internal short flip;
        internal short parity;
        internal short FRtoBR;
        internal short URFtoDLF;
        internal short URtoUL;
        internal short UBtoDF;
        internal int URtoDF;

        /* all empty pruning tables; must be loaded with {@link PruneTableLoader} first before using the solver */
        internal static short[][] twistMove;
        internal static short[][] flipMove;
        internal static short[][] FRtoBR_Move;
        internal static short[][] URFtoDLF_Move;
        internal static short[][] URtoDF_Move;
        internal static short[][] URtoUL_Move;
        internal static short[][] UBtoDF_Move;
        internal static short[][] MergeURtoULandUBtoDF;
        internal static sbyte[] Slice_URFtoDLF_Parity_Prun;
        internal static sbyte[] Slice_URtoDF_Parity_Prun;
        internal static sbyte[] Slice_Twist_Prun;
        internal static sbyte[] Slice_Flip_Prun;

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Parity of the corner permutation. This is the same as the parity for the edge permutation of a valid cube.
        // parity has values 0 and 1
        internal static short[][] parityMove = new short[][]
        {
            new short[] {1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1},
            new short[] {0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0}
        };


        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Generate a CoordCube from a CubieCube
        internal CoordCube(CubieCube c)
        {
            
            twist = c.Twist;
            flip = c.GetFlip();
            parity = c.CornerParity();
            FRtoBR = c.FRtoBR;
            URFtoDLF = c.URFtoDLF;
            URtoUL = c.URtoUL;
            UBtoDF = c.UBtoDF;
            URtoDF = c.URtoDF; // only needed in phase2           
        }//Representation of the cube on the coordinate level

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Extract pruning value
        internal static sbyte GetPruning(sbyte[] table, int index)
        {
            if ((index & 1) == 0)
            {
                return (sbyte)(table[index / 2] & 0x0f);
            }
            else
            {
                return (sbyte)((int)((uint)(table[index / 2] & 0xf0) >> 4));                // uint ???
            }
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Set pruning value in table. Two values are stored in one byte.
        internal static void SetPruning(sbyte[] table, int index, sbyte value)
        {
            if ((index & 1) == 0)
            {
                table[index / 2] &= unchecked((sbyte)(0xf0 | value));
            }
            else
            {
                table[index / 2] &= (sbyte)(0x0f | (value << 4));
            }
        }
    }
}