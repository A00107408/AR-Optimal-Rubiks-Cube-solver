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
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    using OpenCvSharp;

    /// <summary>
    /// Least Means Square
    /// Original @author android.steve@cl-sw.com 
    /// </summary>
    public class LeastMeansSquare
    {

        // Actually, this will/should be center of corner tile will lowest (i.e. smallest) value of Y.
        public Point origin;

        // =+= Migrate this to Lattice ?
        public double alphaLattice;

        //
        public Point[][] errorVectorArray;

        // Sum of all errors (RMS)
        public double sigma;

        // True if results are mathematically valid.
        public bool valid;

        public LeastMeansSquare(double x, double y, double alphaLatice, Point[][] errorVectorArray, double sigma, bool valid)
        {
            this.origin = new Point(x, y);
            this.alphaLattice = alphaLatice;
            this.errorVectorArray = errorVectorArray;
            this.sigma = sigma;
            this.valid = valid;
        }
    }
}
