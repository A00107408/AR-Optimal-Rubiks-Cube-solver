/* <summary
 * 
 * Augmented Reality Rubik Cube Application
 * A.I.T 2018
 * A00107408
 * Masters by Research
 * 
 * File Description:
 * Enumeration of Edges
 * 
 * Acknowledgments:
 * Original @author: Elias Frantar.
 * Translated Kociemba algorithm to java.
 *   
 * <summary> */

namespace KociembaTwoPhase
{
    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    //Then names of the edge positions of the cube. Edge UR e.g., has an U(p) and R(ight) facelet.
    internal enum Edge
    {
        UR,
        UF,
        UL,
        UB,
        DR,
        DF,
        DL,
        DB,
        FR,
        FL,
        BL,
        BR
    }
}