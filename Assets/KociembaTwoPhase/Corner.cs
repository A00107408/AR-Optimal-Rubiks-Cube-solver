/* <summary
 * 
 * Augmented Reality Rubik Cube Application
 * A.I.T 2018
 * A00107408
 * Masters by Research
 * 
 * File Description:
 * Corner value enumeration.
 * 
 * Acknowledgments:
 * Original @author: Elias Frantar.
 * Translated Kociemba algorithm to java.
 *   
 * <summary> */
 namespace KociembaTwoPhase
{
    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    //The names of the corner positions of the cube. Corner URF e.g., has an U(p), a R(ight) and a F(ront) facelet
    internal enum Corner
    {
        URF,
        UFL,
        ULB,
        UBR,
        DFR,
        DLF,
        DBL,
        DRB
    }
}
