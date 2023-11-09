/* <summary
 * 
 * Augmented Reality Rubik Cube Application
 * A.I.T 2018
 * A00107408
 * Masters by Research
 * 
 * File Description:
 *  The 3D Model is maintained with a Kalman Filter using OpenCV Pose estimates
 *  and measurement inputs (an also half of state variables).
 * 
 * Acknowledgments:
 * Original Author: Steven P. Punte (aka Android Steve : android.steve@cl-sw.com)
 * Date: April 25th 2015
 *   
 * <summary> */

/* <summary>
/// Class Kalman Filter
/// 
/// This Kalman Filter is constructed with twelve state variables: three are 
/// position, three are orientation, and six more are the first derivative
/// of these values: i.e., velocity and angular rotation rate.
/// 
/// A new Kalman Filter object is created when Gesture Recognition State
/// transitions to NEW_STABLE.  The current position and orientation are 
/// accepted into state and all first derivative state variables are set to
/// zero.
/// 
/// Upon the second Pose measurement, position and orientation are adopted,
/// and the first derivative state variables are set accordingly.
/// 
/// Thereafter, the Kalman Filter is operated in the normal manner.  
/// 
/// Some key aspects:
/// o  Measurement time intervals are not necessarily uniform, but close.
/// o  Rendering requests will be asynchronous and typically at a higher 
///    frame rate: ~60 Hz.
/// o  Start up sequence as described above.
/// o  Note: matrices are row major.
/// 
/// Initial design:
/// o  Is simply a fixed, but time-interval-variant Kalman Filter Gain matrix.
/// o  Also records timestamp of last measurement update.
/// o  Forward, Kalman Gain, and Output are time-variant matrices, but linear to time.
/// 
/// 
/// @author android.steve@cl-sw.com
/// 
/// </summary>*/

namespace Rubik
{
    /*using Equation = org.ejml.equation.Equation;
      using Sequence = org.ejml.equation.Sequence;
      using Equation = org.ejml.equation.Equation;*/

    using Matrix = CSML.Matrix;
    using Complex = CSML.Complex;

    // using MathNet.Numerics.LinearAlgebra;
    // using MathNet.Numerics.LinearAlgebra.Double;

    

    public class KalmanFilter
    {

        /*
	    *  Defines elements of primary state vector.
	    *  However, not used that much.
	    */
        private enum STATE
        {
            X_POS,
            X_POS_VELOCITY,
            Y_POS,
            Y_POS_VELOCITY,
            Z_POS,
            Z_POS_VELOCITY,
            X_AXIS_ROTATION,
            X_AXIS_ROTATION_RATE,
            Y_AXIS_ROTATION,
            Y_AXIS_ROTATION_RATE,
            Z_AXIS_ROTATION,
            Z_AXIS_ROTATION_RATE
        }

        // State Vector. State is 12 elements long: x_pos, x_vel, 
        private Matrix xSimpleMatrix;

        // The next calculated state; above variable shall be set to this object to complete update.
        private Matrix rSimpleMatrix = new Matrix(12, 1);

        // Feed Forward Matrix
        private Matrix aSimpleMatrix = Matrix.Identity(12);

        // Project State Forward Matrix (But do not change state)
        private Matrix bSimpleMatrix = Matrix.Identity(12);

        // State to Output Matrix
        //	private final double[][] hMatrix = { { 0.0f } };

        // Input to State Matrix
        private Matrix cSimpleMatrix = new Matrix(12, 6);


        // Kalman Gain Matrix
        //	private final double[][] kMatrix = { { 0.0f } };

        // Projected state
        private Matrix zSimpleMatrix = new Matrix(12, 1);

        /*
         * Other Member Data
         */

        // Timestamp reference of state
        private long measUpateTime;

        // Feeback (or gain) Coefficient
        private double alpha = 1.0;

        // Last reported (i.e., measured) cube pose.  Last cube position is always reported when Kalman Filter is turned off.
        private CubePose lastCubePoseState;

        // EJML Equations object
        //  private Equation equations = new Equation();

        // Compiled Equation z = B * x : calculate projected state.
        //   private Sequence projectStateEquation;

        // Compiled Equation: x(k+1) = A(alpha,tau) * x(k) + C(alpha) * u(k)		
        //   private Sequence updateEquation;


        /// <summary>
        /// Kalman Filter Constructor
        /// 
        /// Begin a new Kalman FIlter.
        /// Initialize all state variables are initialized.
        /// </summary>
        public KalmanFilter()
        {

            // Maybe (or should be) done by constructor, but docs don't say.
            //cSimpleMatrix.zero();          
            cSimpleMatrix = Matrix.Zeros(12, 6);
        }

        /// 
        /// 
        /// <summary>
        /// Supply Kalman Filter with a new measurement update and a timestamp of when
        /// the measurements were valid.
        /// </summary>
        /// <param name="cubeReconstructor"> </param>
        /// <param name="time"> </param>
        public virtual void MeasurementUpdate(CubePose cubePoseMeasure, long time)
        {

            lastCubePoseState = cubePoseMeasure;

            // If Kalman Filter not active, then just return.  Pose above will simply be used.
//            if (MenuAndParams.kalmanFilter == false)
//            {
//                return;
//            }

            // Sometimes happens when face is solved, but pose algorithm has problems.
            if (cubePoseMeasure == null)
            {
                return;
            }

            // Don't do any more if Kalman Filter is not active
//            if (MenuAndParams.kalmanFilter == false)
//            {
//                return;
//            }

            // First time through, simply set state to measurement.
            if (xSimpleMatrix == null)
            {

       //          xSimpleMatrix = new Matrix(12, 1, true, new double[] { cubePoseMeasure.x, 0, cubePoseMeasure.y, 0, cubePoseMeasure.z, 0, cubePoseMeasure.xRotation, 0, cubePoseMeasure.yRotation, 0, cubePoseMeasure.zRotation, 0 });
                //xSimpleMatrix.InsertRow(1, 1, new double[] { cubePoseMeasure.x, 0, cubePoseMeasure.y, 0, cubePoseMeasure.z, 0, cubePoseMeasure.xRotation, 0, cubePoseMeasure.yRotation, 0, cubePoseMeasure.zRotation, 0 });
                
                return;
            }

            // Calculate duration between last update.
            long tau = time - measUpateTime;

            // Input is 6 element column vector
        //    Matrix uSimpleMatrix = new Matrix(6, 1, true, new double[] { cubePoseMeasure.x, cubePoseMeasure.y, cubePoseMeasure.z, cubePoseMeasure.xRotation, cubePoseMeasure.yRotation, cubePoseMeasure.zRotation });

    //        equations.alias(uSimpleMatrix, "u"); // =+= hack because we are creating a new uSimpleMatrix each call.

            // Set coefficients of A matrix
     /*       aSimpleMatrix.Insert(0, 0, 1.0 - alpha);
            aSimpleMatrix.Set(0, 0, 1.0 - alpha);
            aSimpleMatrix.set(0, 1, (1.0 - alpha) * tau);
            aSimpleMatrix.set(1, 0, -1.0 * alpha / tau);
            aSimpleMatrix.set(1, 1, 1.0 - alpha);
            aSimpleMatrix.set(2, 2, 1.0 - alpha);
            aSimpleMatrix.set(2, 3, (1.0 - alpha) * tau);
            aSimpleMatrix.set(3, 2, -1.0 * alpha / tau);
            aSimpleMatrix.set(3, 3, 1.0 - alpha);
            aSimpleMatrix.set(4, 4, 1.0 - alpha);
            aSimpleMatrix.set(4, 5, (1.0 - alpha) * tau);
            aSimpleMatrix.set(5, 4, -1.0 * alpha / tau);
            aSimpleMatrix.set(5, 5, 1.0 - alpha);
            aSimpleMatrix.set(6, 6, 1.0 - alpha);
            aSimpleMatrix.set(6, 7, (1.0 - alpha) * tau);
            aSimpleMatrix.set(7, 6, -1.0 * alpha / tau);
            aSimpleMatrix.set(7, 7, 1.0 - alpha);
            aSimpleMatrix.set(8, 8, 1.0 - alpha);
            aSimpleMatrix.set(8, 9, (1.0 - alpha) * tau);
            aSimpleMatrix.set(9, 8, -1.0 * alpha / tau);
            aSimpleMatrix.set(9, 9, 1.0 - alpha);
            aSimpleMatrix.set(10, 10, 1.0 - alpha);
            aSimpleMatrix.set(10, 11, (1.0 - alpha) * tau);
            aSimpleMatrix.set(11, 10, -1.0 * alpha / tau);
            aSimpleMatrix.set(11, 11, 1.0 - alpha);

            // Set coefficients of C matrix
            cSimpleMatrix.set(0, 0, alpha);
            cSimpleMatrix.set(1, 0, alpha / tau);
            cSimpleMatrix.set(2, 1, alpha);
            cSimpleMatrix.set(3, 1, alpha / tau);
            cSimpleMatrix.set(4, 2, alpha);
            cSimpleMatrix.set(5, 2, alpha / tau);
            cSimpleMatrix.set(6, 3, alpha);
            cSimpleMatrix.set(7, 3, alpha / tau);
            cSimpleMatrix.set(8, 4, alpha);
            cSimpleMatrix.set(9, 4, alpha / tau);
            cSimpleMatrix.set(10, 5, alpha);
            cSimpleMatrix.set(11, 5, alpha / tau);*/

     /*       if (updateEquation == null)
            {

                // Aliases for EJML
                equations.alias(rSimpleMatrix, "r", aSimpleMatrix, "A", xSimpleMatrix, "x", cSimpleMatrix, "C", uSimpleMatrix, "u");

                // Compile project state linear algebra equation
                updateEquation = equations.compile("r = A*x + C*u");
            }

            // Calculate new state
            // x(k+1) = A(alpha,tau) * x(k) + C(alpha) * u(k)
            updateEquation.perform();

            // =+= next three lines of code should be semaphore locked with projectState() and GL thread.
            // Update State
            xSimpleMatrix = rSimpleMatrix;

            // Re=alias for x
            equations.alias(xSimpleMatrix, "x");*/
            // Record this time for future reference.
            measUpateTime = time;

            // =+= Crude control of feedback: goes from 100% to 25% and then stays there.
            // =+= Kalman Gain is supposed to be calculated algorithmically.
            if (alpha > 0.5)
            {
                alpha = 0.5;
            }
            else if (alpha > 0.25)
            {
                alpha = 0.25;
            }
        }

        /// <summary>
        /// Return state as per the specified time stamp.
        /// </summary>
        /// <param name="time">
        /// @return </param>
        public CubePose projectState(Complex time)
        {

            // =+= this and x matrix object should really be semaphore locked with a portion of updateState() running camera thread.
            Complex tau = time - measUpateTime;

            //            if (MenuAndParams.kalmanFilter == false)
            //            {
            //                return lastCubePoseState;
            //            }

            if (xSimpleMatrix == null)
            {
                return null;
            }

            bSimpleMatrix[0, 1] = tau;
            bSimpleMatrix[2, 3] = tau;
            bSimpleMatrix[4, 5] = tau;
            bSimpleMatrix[6, 7] = tau;
            bSimpleMatrix[8, 9] = tau;

   /*         if (projectStateEquation == null)
            {

                // Aliases for EJML
                equations.alias(xSimpleMatrix, "x", bSimpleMatrix, "B", zSimpleMatrix, "z");

                // Compile project state linear algebra equation
                projectStateEquation = equations.compile("z = B*x");
            }

            // Calculate projected state for specified time, but do not update state matrix.
            // z(t + tau) = B(tau) * x(t) 
            projectStateEquation.perform();*/
     

            // Package up
            CubePose cubePose = new CubePose();
          /*  cubePose.x = (float)zSimpleMatrix[(int)STATE.X_POS];
            cubePose.y = (float)zSimpleMatrix.get((int)STATE.Y_POS);
            cubePose.z = (float)zSimpleMatrix.get((int)STATE.Z_POS);
            cubePose.xRotation = zSimpleMatrix.get((int)(STATE.X_AXIS_ROTATION));
            cubePose.yRotation = zSimpleMatrix.get((int)(STATE.Y_AXIS_ROTATION));
            cubePose.zRotation = zSimpleMatrix.get((int)(STATE.Z_AXIS_ROTATION));*/

            return cubePose;

        }

    }

}