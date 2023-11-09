/* <summary>
 * 
 * A.I.T 2018
 * A00107408
 * Masters By Research
 * 
 * File Description:
 * The purpose of this class is to record timestamps and also provide for 
 * diagnostic annotation.
 *   
 * Acknowledgments:  
 * Original Author: Steven P. Punte (aka Android Steve : android.steve@cl-sw.com)
 * Date:   April 25th 2015
 * 
 * Project Description:
 * Unity based Augmented Reality Application for guiding a user through solving a
 * Rubiks Cube in an optimised fashion. Adopted from a Android application intended
 * for the Moverio AR headset.
</summary> */

namespace Rubik
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using OpenCvSharp;

    using ColorTileEnum = Constants.ColorTileEnum;

    /// <summary>
    /// A.I.T 2018 A00107408
    /// original @author android.steve@cl-sw.com
    /// </summary>
    public class Profiler
    {

        public enum Event
        {
            START,
            GREYSCALE,
            GAUSSIAN,
            EDGE,
            DILATION,
            CONTOUR,
            POLYGON,
            RHOMBUS,
            FACE,
            POSE,
            CONTROLLER,
            TOTAL
        }

        // Store time stamps of various events.
        private IDictionary<Event, long> eventSet = new Dictionary<Event, long>(32);

        // Store minimum event times so far observed.
        private static IDictionary<Event, long> minEventSet = new Dictionary<Event, long>(32);

        // Store time stamp for Frames Per Second
        private static long framesPerSecondTimeStamp = 0;

        private bool scheduleReset = false;

        //---------------------------------------------------------------------------------------------------------
        //	Copyright © 2007 - 2018 Tangible Software Solutions Inc.
        //	This class can be used by anyone provided that the copyright notice remains intact.
        //
        //	This class is used to replace calls to Java's System.currentTimeMillis with the C# equivalent.
        //	Unix time is defined as the number of seconds that have elapsed since midnight UTC, 1 January 1970.
        //---------------------------------------------------------------------------------------------------------
        internal static class DateTimeHelper
        {
            private static readonly System.DateTime Jan1st1970 = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
            internal static long CurrentUnixTimeMillis()
            {
                return (long)(System.DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
            }
        }

        public virtual void markTime(Event @event)
        {
            long time = DateTimeHelper.CurrentUnixTimeMillis();
            eventSet[@event] = time;
            if (minEventSet.ContainsKey(@event) == false)
            {
                minEventSet[@event] = long.MaxValue;
            }
        }

        public virtual void Reset()
        {
            scheduleReset = true;
        }
    }
        
}