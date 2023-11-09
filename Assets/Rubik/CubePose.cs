/* <summary>
 * 
 * Augmented Reality Rubik Cube Application
 * A.I.T 2018
 * A00107408
 * Masters by Research
 * 
 * File Description:
 * Simple class that represents position and orientation of cube.
 *
 * Acknowledgments:
 * Original Author: Steven P. Punte (aka Android Steve : android.steve@cl-sw.com)
 * Date: April 25th 2015
 * 
 * <summary> */

namespace Rubik
{
    public class CubePose
    {
        // Position of cube center in OpenGL 3D space in "Real World" units
        public float x;
        public float y;
        public float z;

        // Rotation of cube in OpenGL 3D space in units of radians
        public double xRotation;
        public double yRotation;
        public double zRotation;
    }
}