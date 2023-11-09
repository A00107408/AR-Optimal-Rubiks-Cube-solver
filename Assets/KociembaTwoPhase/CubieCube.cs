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
   using System;
   using UnityEngine;

    public class CubieCube
    {
        // initialize to Id-Cube

        // corner permutation
        internal Corner[] cp = new Corner[] { Corner.URF, Corner.UFL, Corner.ULB, Corner.UBR, Corner.DFR, Corner.DLF, Corner.DBL, Corner.DRB };

        // corner orientation
        internal sbyte[] co = new sbyte[] { 0, 0, 0, 0, 0, 0, 0, 0 };

        // edge permutation
        internal Edge[] ep = new Edge[] { Edge.UR, Edge.UF, Edge.UL, Edge.UB, Edge.DR, Edge.DF, Edge.DL, Edge.DB, Edge.FR, Edge.FL, Edge.BL, Edge.BR };

        // edge orientation
        internal sbyte[] eo = new sbyte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        // ************************************** Moves on the cubie level ***************************************************

        private static Corner[] cpU = new Corner[] { Corner.UBR, Corner.URF, Corner.UFL, Corner.ULB, Corner.DFR, Corner.DLF, Corner.DBL, Corner.DRB };
        private static sbyte[] coU = new sbyte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
        private static Edge[] epU = new Edge[] { Edge.UB, Edge.UR, Edge.UF, Edge.UL, Edge.DR, Edge.DF, Edge.DL, Edge.DB, Edge.FR, Edge.FL, Edge.BL, Edge.BR };
        private static sbyte[] eoU = new sbyte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        private static Corner[] cpR = new Corner[] { Corner.DFR, Corner.UFL, Corner.ULB, Corner.URF, Corner.DRB, Corner.DLF, Corner.DBL, Corner.UBR };
        private static sbyte[] coR = new sbyte[] { 2, 0, 0, 1, 1, 0, 0, 2 };
        private static Edge[] epR = new Edge[] { Edge.FR, Edge.UF, Edge.UL, Edge.UB, Edge.BR, Edge.DF, Edge.DL, Edge.DB, Edge.DR, Edge.FL, Edge.BL, Edge.UR };
        private static sbyte[] eoR = new sbyte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        private static Corner[] cpF = new Corner[] { Corner.UFL, Corner.DLF, Corner.ULB, Corner.UBR, Corner.URF, Corner.DFR, Corner.DBL, Corner.DRB };
        private static sbyte[] coF = new sbyte[] { 1, 2, 0, 0, 2, 1, 0, 0 };
        private static Edge[] epF = new Edge[] { Edge.UR, Edge.FL, Edge.UL, Edge.UB, Edge.DR, Edge.FR, Edge.DL, Edge.DB, Edge.UF, Edge.DF, Edge.BL, Edge.BR };
        private static sbyte[] eoF = new sbyte[] { 0, 1, 0, 0, 0, 1, 0, 0, 1, 1, 0, 0 };

        private static Corner[] cpD = new Corner[] { Corner.URF, Corner.UFL, Corner.ULB, Corner.UBR, Corner.DLF, Corner.DBL, Corner.DRB, Corner.DFR };
        private static sbyte[] coD = new sbyte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
        private static Edge[] epD = new Edge[] { Edge.UR, Edge.UF, Edge.UL, Edge.UB, Edge.DF, Edge.DL, Edge.DB, Edge.DR, Edge.FR, Edge.FL, Edge.BL, Edge.BR };
        private static sbyte[] eoD = new sbyte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        private static Corner[] cpL = new Corner[] { Corner.URF, Corner.ULB, Corner.DBL, Corner.UBR, Corner.DFR, Corner.UFL, Corner.DLF, Corner.DRB };
        private static sbyte[] coL = new sbyte[] { 0, 1, 2, 0, 0, 2, 1, 0 };
        private static Edge[] epL = new Edge[] { Edge.UR, Edge.UF, Edge.BL, Edge.UB, Edge.DR, Edge.DF, Edge.FL, Edge.DB, Edge.FR, Edge.UL, Edge.DL, Edge.BR };
        private static sbyte[] eoL = new sbyte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        private static Corner[] cpB = new Corner[] { Corner.URF, Corner.UFL, Corner.UBR, Corner.DRB, Corner.DFR, Corner.DLF, Corner.ULB, Corner.DBL };
        private static sbyte[] coB = new sbyte[] { 0, 0, 1, 2, 0, 0, 2, 1 };
        private static Edge[] epB = new Edge[] { Edge.UR, Edge.UF, Edge.UL, Edge.BR, Edge.DR, Edge.DF, Edge.DL, Edge.BL, Edge.FR, Edge.FL, Edge.UB, Edge.DB };
        private static sbyte[] eoB = new sbyte[] { 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 1, 1 };

        // this CubieCube array represents the 6 basic cube moves
        internal static CubieCube[] moveCube = new CubieCube[6];

        static CubieCube()
        {
            moveCube[0] = new CubieCube();
            moveCube[0].cp = cpU;
            moveCube[0].co = coU;
            moveCube[0].ep = epU;
            moveCube[0].eo = eoU;

            moveCube[1] = new CubieCube();
            moveCube[1].cp = cpR;
            moveCube[1].co = coR;
            moveCube[1].ep = epR;
            moveCube[1].eo = eoR;

            moveCube[2] = new CubieCube();
            moveCube[2].cp = cpF;
            moveCube[2].co = coF;
            moveCube[2].ep = epF;
            moveCube[2].eo = eoF;

            moveCube[3] = new CubieCube();
            moveCube[3].cp = cpD;
            moveCube[3].co = coD;
            moveCube[3].ep = epD;
            moveCube[3].eo = eoD;

            moveCube[4] = new CubieCube();
            moveCube[4].cp = cpL;
            moveCube[4].co = coL;
            moveCube[4].ep = epL;
            moveCube[4].eo = eoL;

            moveCube[5] = new CubieCube();
            moveCube[5].cp = cpB;
            moveCube[5].co = coB;
            moveCube[5].ep = epB;
            moveCube[5].eo = eoB;

        }

        internal CubieCube()
        {

        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        internal CubieCube(Corner[] cp, sbyte[] co, Edge[] ep, sbyte[] eo) : this()
        {
            for (int i = 0; i < 8; i++)
            {
                this.cp[i] = cp[i];
                this.co[i] = co[i];
            }
            for (int i = 0; i < 12; i++)
            {
                this.ep[i] = ep[i];
                this.eo[i] = eo[i];
            }
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // n choose k
        internal static int Cnk(int n, int k)
        {
            int i, j, s;
            if (n < k)
            {
                return 0;
            }
            if (k > n / 2)
            {
                k = n - k;
            }
            for (s = 1, i = n, j = 1; i != n - k; i--, j++)
            {
                s *= i;
                s /= j;
            }
            return s;
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        internal static void RotateLeft(Corner[] arr, int l, int r)
        {
            // Left rotation of all array elements between l and r
            Corner temp = arr[l];
            for (int i = l; i < r; i++)
            {
                arr[i] = arr[i + 1];
            }
            arr[r] = temp;
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        internal static void RotateRight(Corner[] arr, int l, int r)
        {
            // Right rotation of all array elements between l and r
            Corner temp = arr[r];
            for (int i = r; i > l; i--)
            {
                arr[i] = arr[i - 1];
            }
            arr[l] = temp;
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        internal static void RotateLeft(Edge[] arr, int l, int r)
        {
            // Left rotation of all array elements between l and r
            Edge temp = arr[l];
                      
            for (int i = l; i < r; i++)
            {
                arr[i] = arr[i + 1];
            }
            arr[r] = temp;
        }

        internal static void RotateLeftb(Edge[] arr, int l, int r)
        {
            // Left rotation of all array elements between l and r
            Edge temp = arr[l];

            for (int i = l; i < r; i++)
            {
                arr[i] = arr[i + 1];
            }
            arr[r] = temp;
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        internal static void RotateRight(Edge[] arr, int l, int r)
        {
            // Right rotation of all array elements between l and r
            Edge temp = arr[r];
            for (int i = r; i > l; i--)
            {
                arr[i] = arr[i - 1];
            }
            arr[l] = temp;
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Check a cubiecube for solvability. Return the error code.
        // 0: Cube is solvable
        // -2: Not all 12 edges exist exactly once
        // -3: Flip error: One edge has to be flipped
        // -4: Not all corners exist exactly once
        // -5: Twist error: One corner has to be twisted
        // -6: Parity error: Two corners ore two edges have to be exchanged
        internal virtual int Verify()
        {
            int sum = 0;
            int[] edgeCount = new int[12];
            foreach (Edge e in Enum.GetValues(typeof(Edge)))
            {
                edgeCount[(int)ep[(int)e]]++;
            }
            for (int i = 0; i < 12; i++)
            {
                if (edgeCount[i] != 1)
                {
                    // return -2;
                }
            }

            for (int i = 0; i < 12; i++)
            {
                sum += eo[i];
            }
            if (sum % 2 != 0)
            {
                //return -3;
            }

            int[] cornerCount = new int[8];
            foreach (Corner c in Enum.GetValues(typeof(Corner)))
            {
                cornerCount[(int)cp[(int)c]]++;
            }
            for (int i = 0; i < 8; i++)
            {
                if (cornerCount[i] != 1)
                {
                    // return -4; // missing corners
                }
            }

            sum = 0;
            for (int i = 0; i < 8; i++)
            {
                sum += co[i];
            }
            if (sum % 3 != 0)
            {
               // return -5; // twisted corner
            }

            if ((EdgeParity() ^ CornerParity()) != 0)
            {
               // return -6; // parity error
            }

            return 0; // cube ok
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // return the twist of the 8 corners. 0 <= twist < 3^7
        internal virtual short Twist
        {          
            get
            {
                short ret = 0;
                for (int i = (int)Corner.URF; i < (int)Corner.DRB; i++)
                {
                    ret = (short)(3 * ret + co[i]);
                }
                return ret;
            }
            set
            {
                int twistParity = 0;
                for (int i = (int)Corner.DRB - 1; i >= (int)Corner.URF; i--)
                {
                    twistParity += co[i] = (sbyte)(value % 3);
                    value /= 3;
                }
                co[(int)Corner.DRB] = (sbyte)((3 - twistParity % 3) % 3);
            }
        }


        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // return the flip of the 12 edges. 0<= flip < 2^11
        internal virtual short Flip
        {
            get
            {
                short ret = 0;
                for (int i = (int)Edge.UR; i < (int)Edge.BR; i++)
                {
                    ret = (short)(2 * ret + eo[i]);
                }
              //  Debug.Log("ret: " + ret);
                return ret;
            }
            set
            {
                int flipParity = 0;
                for (int i = (int)Edge.BR - 1; i >= (int)Edge.UR; i--)
                {
                    flipParity += eo[i] = (sbyte)(value % 2);
                    value /= 2;
                }
                eo[(int)Edge.BR] = (sbyte)((2 - flipParity % 2) % 2);
            }
        }

        internal virtual short GetFlip()
        {           
            short ret = 0;

            for (int i = (int)Edge.UR; i < (int)Edge.BR; i++)
            {
                ret = (short)(2 * ret + eo[i]);             
            }           
            return ret;
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Parity of the corner permutation
        internal virtual short CornerParity()
        {
            int s = 0;
            for (int i = (int)Corner.DRB; i >= (int)Corner.URF + 1; i--)
            {
                for (int j = i - 1; j >= (int)Corner.URF; j--)
                {
                    if ((int)cp[j] > (int)cp[i])
                    {
                        s++;
                    }
                }
            }
            return (short)(s % 2);
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Parity of the edges permutation. Parity of corners and edges are the same if the cube is solvable.
        internal virtual short EdgeParity()
        {
            int s = 0;
            for (int i = (int)Edge.BR; i >= (int)Edge.UR + 1; i--)
            {
                for (int j = i - 1; j >= (int)Edge.UR; j--)
                {
                    if ((int)ep[j] > (int)ep[i])
                    {
                        s++;
                    }
                }
            }
            return (short)(s % 2);
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // permutation of the UD-slice edges FR,FL,BL and BR
        internal virtual short FRtoBR
        {            
            get
            {                
                int a = 0, x = 0;
                Edge[] edge4 = new Edge[4];
                // compute the index a < (12 choose 4) and the permutation array perm.
                for (int j = (int)Edge.BR; j >= (int)Edge.UR; j--)
                {
                    if ((int)Edge.FR <= (int)ep[j] && (int)ep[j] <= (int)Edge.BR)
                    {
                        a += Cnk(11 - j, x + 1);
                        edge4[3 - x++] = ep[j];
                    }
                }

                int b = 0;
                for (int j = 3; j > 0; j--) // compute the index b < 4! for the
                {
                    // permutation in perm
                    int k = 0;
                    while ((int)edge4[j] != j + 8)
                    {
                        RotateLeft(edge4, 0, j);
                        k++;
                    }
                    b = (j + 1) * b + k;
                }
                return (short)(24 * a + b);
            }
            set
            {
                int x;
                Edge[] sliceEdge = new Edge[] { Edge.FR, Edge.FL, Edge.BL, Edge.BR };
                Edge[] otherEdge = new Edge[] { Edge.UR, Edge.UF, Edge.UL, Edge.UB, Edge.DR, Edge.DF, Edge.DL, Edge.DB };
                int b = value % 24; // Permutation
                int a = value / 24; // Combination
                foreach (Edge e in Enum.GetValues(typeof(Edge)))
                {
                    ep[(int)e] = Edge.DB; // Use UR to invalidate all edges
                }

                for (int j = 1, k; j < 4; j++) // generate permutation from index b
                {
                    k = b % (j + 1);
                    b /= j + 1;
                    while (k-- > 0)
                    {
                        RotateRight(sliceEdge, 0, j);
                    }
                }

                x = 3; // generate combination and set slice edges
                for (int j = (int)Edge.UR; j <= (int)Edge.BR; j++)
                {
                    if (a - Cnk(11 - j, x + 1) >= 0)
                    {
                        ep[j] = sliceEdge[3 - x];
                        a -= Cnk(11 - j, x-- + 1);
                    }
                }
                x = 0; // set the remaining edges UR..DB
                for (int j = (int)Edge.UR; j <= (int)Edge.BR; j++)
                {
                    if (ep[j] == Edge.DB)
                    {
                        ep[j] = otherEdge[x++];
                    }
                }

            }
        }      

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Permutation of all corners except DBL and DRB
        internal virtual short URFtoDLF
        {
            get
            {
                int a = 0, x = 0;
                Corner[] corner6 = new Corner[6];
                // compute the index a < (8 choose 6) and the corner permutation.
                for (int j = (int)Corner.URF; j <= (int)Corner.DRB; j++)
                {
                    if ((int)cp[j] <= (int)Corner.DLF)
                    {
                        a += Cnk(j, x + 1);
                        corner6[x++] = cp[j];
                    }
                }

                int b = 0;
                for (int j = 5; j > 0; j--) // compute the index b < 6! for the
                {
                    // permutation in corner6
                    int k = 0;
                    while ((int)corner6[j] != j)
                    {
                        RotateLeft(corner6, 0, j);
                        k++;
                    }
                    b = (j + 1) * b + k;
                }
                return (short)(720 * a + b);
            }

            set
            {
                int x;
                Corner[] corner6 = new Corner[] { Corner.URF, Corner.UFL, Corner.ULB, Corner.UBR, Corner.DFR, Corner.DLF };
                Corner[] otherCorner = new Corner[] { Corner.DBL, Corner.DRB };
                int b = value % 720; // Permutation
                int a = value / 720; // Combination
                foreach (Corner c in Enum.GetValues(typeof(Corner)))
                {
                    cp[(int)c] = Corner.DRB; // Use DRB to invalidate all corners
                }

                for (int j = 1, k; j < 6; j++) // generate permutation from index b
                {
                    k = b % (j + 1);
                    b /= j + 1;
                    while (k-- > 0)
                    {
                        RotateRight(corner6, 0, j);
                    }
                }
                x = 5; // generate combination and set corners
                for (int j = (int)Corner.DRB; j >= 0; j--)
                {
                    if (a - Cnk(j, x + 1) >= 0)
                    {
                        cp[j] = corner6[x];
                        a -= Cnk(j, x-- + 1);
                    }
                }
                x = 0;
                for (int j = (int)Corner.URF; j <= (int)Corner.DRB; j++)
                {
                    if (cp[j] == Corner.DRB)
                    {
                        cp[j] = otherCorner[x++];
                    }
                }
            }
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Permutation of the three edges UR,UF,UL
        internal virtual short URtoUL
        {
            get
            {
                int a = 0, x = 0;
                Edge[] edge3 = new Edge[3];
                // compute the index a < (12 choose 3) and the edge permutation.
                for (int j = (int)Edge.UR; j <= (int)Edge.BR; j++)
                {
                    if ((int)ep[j] <= (int)Edge.UL)
                    {
                        a += Cnk(j, x + 1);
                        edge3[x++] = ep[j];
                    }
                }

                int b = 0;
                for (int j = 2; j > 0; j--) // compute the index b < 3! for the
                {
                    // permutation in edge3
                    int k = 0;
                    while ((int)edge3[j] != j)
                    {
                        RotateLeft(edge3, 0, j);
                        k++;
                    }
                    b = (j + 1) * b + k;
                }
                return (short)(6 * a + b);
            }
            set
            {
                int x;
                Edge[] edge3 = new Edge[] { Edge.UR, Edge.UF, Edge.UL };
                int b = value % 6; // Permutation
                int a = value / 6; // Combination
                foreach (Edge e in Enum.GetValues(typeof(Edge)))
                {
                    ep[(int)e] = Edge.BR; // Use BR to invalidate all edges
                }

                for (int j = 1, k; j < 3; j++) // generate permutation from index b
                {
                    k = b % (j + 1);
                    b /= j + 1;
                    while (k-- > 0)
                    {
                        RotateRight(edge3, 0, j);
                    }
                }
                x = 2; // generate combination and set edges
                for (int j = (int)Edge.BR; j >= 0; j--)
                {
                    if (a - Cnk(j, x + 1) >= 0)
                    {
                        ep[j] = edge3[x];
                        a -= Cnk(j, x-- + 1);
                    }
                }
            }
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Permutation of the three edges UB,DR,DF
        internal virtual short UBtoDF
        {
            get
            {
                int a = 0, x = 0;
                Edge[] edge3 = new Edge[3];
                // compute the index a < (12 choose 3) and the edge permutation.
                for (int j = (int)Edge.UR; j <= (int)Edge.BR; j++)
                {
                    if ((int)Edge.UB <= (int)ep[j] && (int)ep[j] <= (int)Edge.DF)
                    {
                        a += Cnk(j, x + 1);
                        edge3[x++] = ep[j];
                    }
                }

                int b = 0;
                for (int j = 2; j > 0; j--) // compute the index b < 3! for the
                {
                    // permutation in edge3
                    int k = 0;
                    while ((int)edge3[j] != (int)Edge.UB + j)
                    {
                        RotateLeft(edge3, 0, j);
                        k++;
                    }
                    b = (j + 1) * b + k;
                }
                return (short)(6 * a + b);
            }
            set
            {
                int x;
                Edge[] edge3 = new Edge[] { Edge.UB, Edge.DR, Edge.DF };
                int b = value % 6; // Permutation
                int a = value / 6; // Combination
                foreach (Edge e in Enum.GetValues(typeof(Edge)))
                {
                    ep[(int)e] = Edge.BR; // Use BR to invalidate all edges
                }

                for (int j = 1, k; j < 3; j++) // generate permutation from index b
                {
                    k = b % (j + 1);
                    b /= j + 1;
                    while (k-- > 0)
                    {
                        RotateRight(edge3, 0, j);
                    }
                }
                x = 2; // generate combination and set edges
                for (int j = (int)Edge.BR; j >= 0; j--)
                {
                    if (a - Cnk(j, x + 1) >= 0)
                    {
                        ep[j] = edge3[x];
                        a -= Cnk(j, x-- + 1);
                    }
                }
            }
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Permutation of the six edges UR,UF,UL,UB,DR,DF.
        internal virtual int URtoDF
        {
            get
            {
                int a = 0, x = 0;
                Edge[] edge6 = new Edge[6];
                // compute the index a < (12 choose 6) and the edge permutation.
                for (int j = (int)Edge.UR; j <= (int)Edge.BR; j++)
                {
                    if ((int)ep[j] <= (int)Edge.DF)
                    {
                        a += Cnk(j, x + 1);
                        edge6[x++] = ep[j];
                    }
                }

                int b = 0;
                for (int j = 5; j > 0; j--) // compute the index b < 6! for the
                {
                    // permutation in edge6
                    int k = 0;
                    while ((int)edge6[j] != j)
                    {
                        RotateLeft(edge6, 0, j);
                        k++;
                    }
                    b = (j + 1) * b + k;
                }
                return 720 * a + b;
            }
            set
            {
                int x;
                Edge[] edge6 = new Edge[] { Edge.UR, Edge.UF, Edge.UL, Edge.UB, Edge.DR, Edge.DF };
                Edge[] otherEdge = new Edge[] { Edge.DL, Edge.DB, Edge.FR, Edge.FL, Edge.BL, Edge.BR };
                int b = value % 720; // Permutation
                int a = value / 720; // Combination
                foreach (Edge e in Enum.GetValues(typeof(Edge)))
                {
                    ep[(int)e] = Edge.BR; // Use BR to invalidate all edges
                }

                for (int j = 1, k; j < 6; j++) // generate permutation from index b
                {
                    k = b % (j + 1);
                    b /= j + 1;
                    while (k-- > 0)
                    {
                        RotateRight(edge6, 0, j);
                    }
                }
                x = 5; // generate combination and set edges
                for (int j = (int)Edge.BR; j >= 0; j--)
                {
                    if (a - Cnk(j, x + 1) >= 0)
                    {
                        ep[j] = edge6[x];
                        a -= Cnk(j, x-- + 1);
                    }
                }
                x = 0; // set the remaining edges DL..BR
                for (int j = (int)Edge.UR; j <= (int)Edge.BR; j++)
                {
                    if (ep[j] == Edge.BR)
                    {
                        ep[j] = otherEdge[x++];
                    }
                }
            }
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Multiply this CubieCube with another cubiecube b, restricted to the corners.<br>
        // Because we also describe reflections of the whole cube by permutations, we get a complication with the corners. The
        // orientations of mirrored corners are described by the numbers 3, 4 and 5. The composition of the orientations
        // cannot
        // be computed by addition modulo three in the cyclic group C3 any more. Instead the rules below give an addition in
        // the dihedral group D3 with 6 elements.<br>
        //	 
        // NOTE: Because we do not use symmetry reductions and hence no mirrored cubes in this simple implementation of the
        // Two-Phase-Algorithm, some code is not necessary here.
        //	
        internal virtual void CornerMultiply(CubieCube b)
        {
            Corner[] cPerm = new Corner[8];
            sbyte[] cOri = new sbyte[8];
            foreach (Corner corn in Enum.GetValues(typeof(Corner)))
            {
                cPerm[(int)corn] = cp[(int)b.cp[(int)corn]];

                sbyte oriA = co[(int)b.cp[(int)corn]];
                sbyte oriB = b.co[(int)corn];
                sbyte ori = 0;
                ;
                if (oriA < 3 && oriB < 3) // if both cubes are regular cubes...
                {
                    ori = (sbyte)(oriA + oriB); // just do an addition modulo 3 here
                    if (ori >= 3)
                    {
                        ori -= 3; // the composition is a regular cube
                    }

                // +++++++++++++++++++++not used in this implementation +++++++++++++++++++++++++++++++++++
                }
                else if (oriA < 3 && oriB >= 3) // if cube b is in a mirrored
                {
                    // state...
                    ori = (sbyte)(oriA + oriB);
                    if (ori >= 6)
                    {
                        ori -= 3; // the composition is a mirrored cube
                    }
                }
                else if (oriA >= 3 && oriB < 3) // if cube a is an a mirrored
                {
                    // state...
                    ori = (sbyte)(oriA - oriB);
                    if (ori < 3)
                    {
                        ori += 3; // the composition is a mirrored cube
                    }
                }
                else if (oriA >= 3 && oriB >= 3) // if both cubes are in mirrored
                {
                    // states...
                    ori = (sbyte)(oriA - oriB);
                    if (ori < 0)
                    {
                        ori += 3; // the composition is a regular cube
                    }
                    // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                }
                cOri[(int)corn] = ori;
            }
            foreach (Corner c in Enum.GetValues(typeof(Corner)))
            {
                cp[(int)c] = cPerm[(int)c];
                co[(int)c] = cOri[(int)c];
            }
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Multiply this CubieCube with another cubiecube b, restricted to the edges.
        internal virtual void EdgeMultiply(CubieCube b)
        {
            Edge[] ePerm = new Edge[12];
            sbyte[] eOri = new sbyte[12];
            foreach (Edge edge in Enum.GetValues(typeof(Edge)))
            {
                ePerm[(int)edge] = ep[(int)b.ep[(int)edge]];
                eOri[(int)edge] = (sbyte)((b.eo[(int)edge] + eo[(int)b.ep[(int)edge]]) % 2);
            }
            foreach (Edge e in Enum.GetValues(typeof(Edge)))
            {
                ep[(int)e] = ePerm[(int)e];
                eo[(int)e] = eOri[(int)e];
            }
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Permutation of the six edges UR,UF,UL,UB,DR,DF
        public static int GetURtoDF(short idx1, short idx2)
        {
            CubieCube a = new CubieCube();
            CubieCube b = new CubieCube();
            a.URtoUL = idx1;
            b.UBtoDF = idx2;
            for (int i = 0; i < 8; i++)
            {
                if (a.ep[i] != Edge.BR)
                {
                    if (b.ep[i] != Edge.BR) // collision
                    {
                        return -1;
                    }
                    else
                    {
                        b.ep[i] = a.ep[i];
                    }
                }
            }
            return b.URtoDF;
        }
    }
}
