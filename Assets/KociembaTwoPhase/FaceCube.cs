/* <summary
 * 
 * Augmented Reality Rubik Cube Application
 * A.I.T 2018
 * A00107408
 * Masters by Research
 * 
 * File Description:
 * 
 * 
 * Acknowledgments:
 * Original @author: Elias Frantar.
 * Translated Kociemba algorithm to java.
 *   
 * <summary> */

namespace KociembaTwoPhase
{
    using System;
       
    public class FaceCube
    {
        public Colors[] f = new Colors[54];

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Map the corner positions to facelet positions. cornerFacelet[URF.ordinal()][0] e.g. gives the position of the
        // facelet in the URF corner position, which defines the orientation.<br>
        // cornerFacelet[URF.ordinal()][1] and cornerFacelet[URF.ordinal()][2] give the position of the other two facelets
        // of the URF corner (clockwise).
        internal static readonly Facelet[][] cornerFacelet = new Facelet[][]
        {
            new Facelet[] {Facelet.U9, Facelet.R1, Facelet.F3 },
            new Facelet[] { Facelet.U7, Facelet.F1, Facelet.L3 },
            new Facelet[] { Facelet.U1, Facelet.L1, Facelet.B3 },
            new Facelet[] { Facelet.U3, Facelet.B1, Facelet.R3 },
            new Facelet[] { Facelet.D3, Facelet.F9, Facelet.R7 },
            new Facelet[] { Facelet.D1, Facelet.L9, Facelet.F7 },
            new Facelet[] { Facelet.D7, Facelet.B9, Facelet.L7 },
            new Facelet[] { Facelet.D9, Facelet.R9, Facelet.B7 }
        };

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Map the edge positions to facelet positions. edgeFacelet[UR.ordinal()][0] e.g. gives the position of the facelet in
        // the UR edge position, which defines the orientation.<br>
        // edgeFacelet[UR.ordinal()][1] gives the position of the other facelet
        internal static readonly Facelet[][] edgeFacelet = new Facelet[][]
        {
            new Facelet[] { Facelet.U6, Facelet.R2 },
            new Facelet[] { Facelet.U8, Facelet.F2 },
            new Facelet[] { Facelet.U4, Facelet.L2 },
            new Facelet[] { Facelet.U2, Facelet.B2 },
            new Facelet[] { Facelet.D6, Facelet.R8 },
            new Facelet[] { Facelet.D2, Facelet.F8 },
            new Facelet[] { Facelet.D4, Facelet.L8 },
            new Facelet[] { Facelet.D8, Facelet.B8 },
            new Facelet[] { Facelet.F6, Facelet.R4 },
            new Facelet[] { Facelet.F4, Facelet.L6 },
            new Facelet[] { Facelet.B6, Facelet.L4 },
            new Facelet[] { Facelet.B4, Facelet.R6 }
        };

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Map the corner positions to facelet colors.
        internal static readonly Colors[][] cornerColor = new Colors[][]
        {
            new Colors[] { Colors.U, Colors.R, Colors.F },
            new Colors[] { Colors.U, Colors.F, Colors.L },
            new Colors[] { Colors.U, Colors.L, Colors.B },
            new Colors[] { Colors.U, Colors.B, Colors.R },
            new Colors[] { Colors.D, Colors.F, Colors.R },
            new Colors[] { Colors.D, Colors.L, Colors.F },
            new Colors[] { Colors.D, Colors.B, Colors.L },
            new Colors[] { Colors.D, Colors.R, Colors.B }
        };

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Map the edge positions to facelet colors.
        internal static readonly Colors[][] edgeColor = new Colors[][]
        {
            new Colors[] { Colors.U, Colors.R },
            new Colors[] { Colors.U, Colors.F },
            new Colors[] { Colors.U, Colors.L },
            new Colors[] { Colors.U, Colors.B },
            new Colors[] { Colors.D, Colors.R },
            new Colors[] { Colors.D, Colors.F },
            new Colors[] { Colors.D, Colors.L },
            new Colors[] { Colors.D, Colors.B },
            new Colors[] { Colors.F, Colors.R },
            new Colors[] { Colors.F, Colors.L },
            new Colors[] { Colors.B, Colors.L },
            new Colors[] { Colors.B, Colors.R }
        };


        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        internal FaceCube()
        {
            string s = "UUUUUUUUURRRRRRRRRFFFFFFFFFDDDDDDDDDLLLLLLLLLBBBBBBBBB";
            for (int i = 0; i < 54; i++)
            {
                // f[i] = Color.valueOf(s.Substring(i, 1));
                f[i] = (Colors)Enum.Parse(typeof(Colors), s.Substring(i, 1));
            }
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Construct a facelet cube from a string
        internal FaceCube(string cubeString)
        {
            for (int i = 0; i < cubeString.Length; i++)
            {
                // f[i] = Color.valueOf(cubeString.Substring(i, 1));
                f[i] = (Colors)Enum.Parse(typeof(Colors), cubeString.Substring(i, 1));

            }
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Gives CubieCube representation of a faceletcube
        internal virtual CubieCube ToCubieCube()
        {
            sbyte ori;
            CubieCube ccRet = new CubieCube();
      
            for (int i = 0; i < 8; i++)
            {
                ccRet.cp[i] = Corner.URF; // invalidate corners
            }
           
            for (int i = 0; i < 12; i++)
            {
                ccRet.ep[i] = Edge.UR; // and edges
            }
            
            Colors col1, col2;
            foreach (Corner i in Enum.GetValues(typeof(Corner)))
            {                
                // get the colors of the cubie at corner i, starting with U/D
                for (ori = 0; ori < 3; ori++)
                {
                    //if (f[cornerFacelet[i.ordinal()][ori].ordinal()] == U || f[cornerFacelet[i.ordinal()][ori].ordinal()] == D)
                    if (f[(int)cornerFacelet[(int)i][ori]] == Colors.U || f[(int)cornerFacelet[(int)i][ori]] == Colors.D)
                    {
                        break;
                    }
                }

                //col1 = f[cornerFacelet[i.ordinal()][(ori + 1) % 3].ordinal()];
                //col2 = f[cornerFacelet[i.ordinal()][(ori + 2) % 3].ordinal()];
                col1 = f[(int)cornerFacelet[(int)i][(ori + 1) % 3]];
                col2 = f[(int)cornerFacelet[(int)i][(ori + 2) % 3]];

                foreach (Corner j in Enum.GetValues(typeof(Corner)))
                {                   
                    if (col1 == cornerColor[(int)j][1] && col2 == cornerColor[(int)j][2])
                    {
                        // in cornerposition i we have cornercubie j
                        ccRet.cp[(int)i] = j;
                        ccRet.co[(int)i] = (sbyte)(ori % 3);
                        break;
                    }
                }
            }
            foreach (Edge i in Enum.GetValues(typeof(Edge)))            
            {                
                foreach (Edge j in Enum.GetValues(typeof(Edge)))
                {                   
                    if (f[(int)edgeFacelet[(int)i][0]] == edgeColor[(int)j][0] && f[(int)edgeFacelet[(int)i][1]] == edgeColor[(int)j][1])
                    {
                        ccRet.ep[(int)i] = j;
                        ccRet.eo[(int)i] = 0;
                        break;
                    }
                    if (f[(int)edgeFacelet[(int)i][0]] == edgeColor[(int)j][1] && f[(int)edgeFacelet[(int)i][1]] == edgeColor[(int)j][0])
                    {
                        ccRet.ep[(int)i] = j;
                        ccRet.eo[(int)i] = 1;
                        break;
                    }
                }
            }
            return ccRet;
        }
    }
}