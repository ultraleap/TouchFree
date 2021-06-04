using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ultraleap.TouchFree.Service
{
    public static class SegmentDisplacement
    {
        // Given the line segments 'a' and 'b', return the smallest distance between any
        // two points on the segments.
        //
        // Line segment 'a' goes between the two end-points 'a1' and 'a2, and similarly for 'b'.
        public static float SegmentToSegmentDistance(Vector3 a1, Vector3 a2, Vector3 b1, Vector3 b2)
        {
            return SegmentToSegmentDisplacement(a1, a2, b1, b2).magnitude;
        }

        public static Vector3 SegmentToSegmentDisplacement(Vector3 a1, Vector3 a2, Vector3 b1, Vector3 b2)
        {
            // SegmentToSegmentDisplacement taken from the Paint demo

            Vector3 u = a2 - a1; //from a1 to a2
            Vector3 v = b2 - b1; //from b1 to b2
            Vector3 w = a1 - b1;
            float a = Vector3.Dot(u, u);         // always >= 0
            float b = Vector3.Dot(u, v);
            float c = Vector3.Dot(v, v);         // always >= 0
            float d = Vector3.Dot(u, w);
            float e = Vector3.Dot(v, w);
            float D = a * c - b * b;        // always >= 0
            float sc, sN, sD = D;       // sc = sN / sD, default sD = D >= 0
            float tc, tN, tD = D;       // tc = tN / tD, default tD = D >= 0

            // compute the line parameters of the two closest points
            if (D < Mathf.Epsilon)
            { // the lines are almost parallel
                sN = 0.0f;         // force using point P0 on segment S1
                sD = 1.0f;         // to prevent possible division by 0.0 later
                tN = e;
                tD = c;
            }
            else
            {                 // get the closest points on the infinite lines
                sN = (b * e - c * d);
                tN = (a * e - b * d);
                if (sN < 0.0f)
                {        // sc < 0 => the s=0 edge is visible
                    sN = 0.0f;
                    tN = e;
                    tD = c;
                }
                else if (sN > sD)
                {  // sc > 1  => the s=1 edge is visible
                    sN = sD;
                    tN = e + b;
                    tD = c;
                }
            }

            if (tN < 0.0)
            {            // tc < 0 => the t=0 edge is visible
                tN = 0.0f;
                // recompute sc for this edge
                if (-d < 0.0)
                    sN = 0.0f;
                else if (-d > a)
                    sN = sD;
                else
                {
                    sN = -d;
                    sD = a;
                }
            }
            else if (tN > tD)
            {      // tc > 1  => the t=1 edge is visible
                tN = tD;
                // recompute sc for this edge
                if ((-d + b) < 0.0)
                    sN = 0;
                else if ((-d + b) > a)
                    sN = sD;
                else
                {
                    sN = (-d + b);
                    sD = a;
                }
            }
            // finally do the division to get sc and tc
            sc = (Mathf.Abs(sN) < Mathf.Epsilon ? 0.0f : sN / sD);
            tc = (Mathf.Abs(tN) < Mathf.Epsilon ? 0.0f : tN / tD);

            // get the difference of the two closest points
            Vector3 dP = w + (sc * u) - (tc * v);  // =  S1(sc) - S2(tc)
            return dP;   // return the closest distance
        }
    }
}