namespace Rubik
{
    using System.Collections.Generic;
    using OpenCvSharp;

    public class Constants
    {
        public enum AppStateEnum
        {
            START, // Ready
            GOT_IT, // A Cube Face has been recognized and captured.
            ROTATE_CUBE, // Request user to rotate Cube.
            SEARCHING, // Attempting to lock onto new Cube Face.
            COMPLETE, // All six faces have been captured, and we seem to have valid color.
            BAD_COLORS, // All six faces have been captured, but we do not have properly nine tiles of each color.
            VERIFIED, // Two Phase solution has verified that the cube tile/colors/positions are a valid cube.
            WAIT_TABLES, // Waiting for TwoPhase Prune Tree generation to complete.
            INCORRECT, // Two Phase solution could not produce a solution; see error code.
            ERROR, // Two Phase solution has analyzed the cube and found it to be invalid.
            SOLVED, // Two Phase solution has analyzed the cube and found a solution.
            ROTATE_FACE, // Inform user to perform a face rotation
            WAITING_MOVE, // Wait for face rotation to complete
            DONE // Cube should be completely physically solved.
        }

        public enum GestureRecogniztionStateEnum
        {
            UNKNOWN, // No face recognition
            PENDING, // A particular face seems to becoming stable.
            STABLE, // A particular face is stable.
            NEW_STABLE, // A new face is stable.
            PARTIAL // A particular face seems to becoming unstable.
        }

        // Conventional Rubik Face nomenclature
        public enum FaceNameEnum
        {
            UP,
            DOWN,
            LEFT,
            RIGHT,
            FRONT,
            BACK
        }

        // Specifies what annotation to add
        public enum AnnotationModeEnum
        {
            LAYOUT,
            RHOMBUS,
            FACE_METRICS,
            TIME,
            COLOR_FACE,
            COLOR_CUBE,
            CUBE_METRICS,
            NORMAL
        }

        /// <summary>
        /// Color Tile Enum
        /// 
        /// This one class serves as both a collection of colors and the values used by various activities
        /// throughout the application, and as a Tile type (more specifically enumeration) that 
        /// the RubikFace class can reference to for each of the nine tiles on each face.
        /// 
        /// Each enumerated color value possess three values: 
        /// - openCV of type Scalar
        /// - openGL of type float[4]
        /// - symbol of type char 
        /// 
        /// @author android.steve@cl-sw.com
        /// 
        /// </summary>
        public sealed class ColorTileEnum
        {
            //                     Target Measurement Colors                   Graphics (both CV and GL)  
            public static readonly ColorTileEnum RED = new ColorTileEnum("RED", InnerEnum.RED, true, 'R', new Scalar(220.0, 20.0, 30.0), new float[] { 1.0f, 0.0f, 0.0f, 1.0f });
            public static readonly ColorTileEnum ORANGE = new ColorTileEnum("ORANGE", InnerEnum.ORANGE, true, 'O', new Scalar(240.0, 80.0, 0.0), new float[] { 0.9f, 0.4f, 0.0f, 1.0f });
            public static readonly ColorTileEnum YELLOW = new ColorTileEnum("YELLOW", InnerEnum.YELLOW, true, 'Y', new Scalar(230.0, 230.0, 20.0), new float[] { 0.9f, 0.9f, 0.2f, 1.0f });
            public static readonly ColorTileEnum GREEN = new ColorTileEnum("GREEN", InnerEnum.GREEN, true, 'G', new Scalar(0.0, 140.0, 60.0), new float[] { 0.0f, 1.0f, 0.0f, 1.0f });
            public static readonly ColorTileEnum BLUE = new ColorTileEnum("BLUE", InnerEnum.BLUE, true, 'B', new Scalar(0.0, 60.0, 220.0), new float[] { 0.2f, 0.2f, 1.0f, 1.0f });
            public static readonly ColorTileEnum WHITE = new ColorTileEnum("WHITE", InnerEnum.WHITE, true, 'W', new Scalar(225.0, 225.0, 225.0), new float[] { 1.0f, 1.0f, 1.0f, 1.0f });

            public static readonly ColorTileEnum BLACK = new ColorTileEnum("BLACK", InnerEnum.BLACK, false, 'K', new Scalar(0.0, 0.0, 0.0));
            public static readonly ColorTileEnum GREY = new ColorTileEnum("GREY", InnerEnum.GREY, false, 'E', new Scalar(50.0, 50.0, 50.0));

            private static readonly IList<ColorTileEnum> valueList = new List<ColorTileEnum>();

            static ColorTileEnum()
            {
                valueList.Add(RED);
                valueList.Add(ORANGE);
                valueList.Add(YELLOW);
                valueList.Add(GREEN);
                valueList.Add(BLUE);
                valueList.Add(WHITE);
                valueList.Add(BLACK);
                valueList.Add(GREY);
            }

            public enum InnerEnum
            {
                RED,
                ORANGE,
                YELLOW,
                GREEN,
                BLUE,
                WHITE,
                BLACK,
                GREY
            }

            public readonly InnerEnum innerEnumValue;
            private readonly string nameValue;
            private readonly int ordinalValue;
            private static int nextOrdinal = 0;


            // A Rubik Color
            public readonly bool isRubikColor;

            // Measuring and Decision Testing in OpenCV
            public readonly Scalar rubikColor;

            // Rendering in OpenCV
            public readonly Scalar cvColor;

            // Rendering in OpenGL
            public readonly float[] glColor;

            // Single letter character
            public readonly char symbol;

            /// <summary>
            /// Color Tile Enum Constructor
            /// 
            /// Accept an Rubik Color and derive OpenCV and OpenGL colors from this.
            /// </summary>
            /// <param name="isRubik"> </param>
            /// <param name="symbol"> </param>
            /// <param name="rubikColor"> </param>
            private ColorTileEnum(string name, InnerEnum innerEnum, bool isRubik, char symbol, Scalar rubikColor)
            {
                this.isRubikColor = isRubik;
                this.cvColor = rubikColor;
                this.rubikColor = rubikColor;
                this.glColor = new float[] { (float)rubikColor[0] / 255f, (float)rubikColor[1] / 255f, (float)rubikColor[2] / 255f, 1.0f };
                this.symbol = symbol;

                nameValue = name;
                ordinalValue = nextOrdinal++;
                innerEnumValue = innerEnum;
            }

            /// <summary>
            /// Color Tile Enum Constructor
            /// 
            /// Accept an Rubik Color and an OpenGL color.  Derive OpenCV color from OpenGL color.
            /// </summary>
            /// <param name="isRubik"> </param>
            /// <param name="symbol"> </param>
            /// <param name="rubikColor"> </param>
            private ColorTileEnum(string name, InnerEnum innerEnum, bool isRubik, char symbol, Scalar rubikColor, float[] renderColor)
            {
                this.isRubikColor = isRubik;
                this.cvColor = new Scalar(renderColor[0] * 255, renderColor[1] * 255, renderColor[2] * 255);
                this.rubikColor = rubikColor;
                this.glColor = renderColor;
                this.symbol = symbol;

                nameValue = name;
                ordinalValue = nextOrdinal++;
                innerEnumValue = innerEnum;
            }


            public static IList<ColorTileEnum> values()
            {
                return valueList;
            }

            public int ordinal()
            {
                return ordinalValue;
            }

            public override string ToString()
            {
                return nameValue;
            }

            public static ColorTileEnum valueOf(string name)
            {
                foreach (ColorTileEnum enumInstance in ColorTileEnum.valueList)
                {
                    if (enumInstance.nameValue == name)
                    {
                        return enumInstance;
                    }
                }
                throw new System.ArgumentException(name);
            }
        }

        // Any OpenCV font
        //public static readonly int FontFace = Core.FONT_HERSHEY_PLAIN;
        public static readonly HersheyFonts FontFace = HersheyFonts.HersheyPlain;
    }
}