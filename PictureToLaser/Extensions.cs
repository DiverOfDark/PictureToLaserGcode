using System;
using System.Collections.Generic;
using System.Globalization;

namespace PictureToLaser
{
    public static class Extensions
    {
        const int MINIMUM_ENGAVING_POWER = 10;

        public static string MyRound(this double d) => Math.Round(d, 4).ToString(CultureInfo.InvariantCulture);
        public static string MyInt(this double d) => Math.Round(d, 0).ToString(CultureInfo.InvariantCulture);

        public static void Requeue<T>(this Queue<T> source, Queue<T> target)
        {
            while (source.Count > 0)
            {
                target.Enqueue(source.Dequeue());
            }
        }
        
        public static double Map(double value, double fromLow, double fromHigh, double toLow, double toHigh)
        {
            var fromRange = fromHigh - fromLow;
            var toRange = toHigh - toLow;
            var scaleFactor = toRange / fromRange;

            // Re-zero the value within the from range
            var tmpValue = value - fromLow;
            // Rescale the value to the to range
            tmpValue *= scaleFactor;
            // Re-zero back to the to range
            var result = tmpValue + toLow;

            if (result < 10)
                result = 0;
            
            return result;
        }
        
        public static void AdjustMinMaxPixels(Options args, float[,] arr, int lineIndex, ref int minX, ref int maxX)
        {
            var laserMin = args.LaserMin;
            var laserMax = args.LaserMax;
            
            for (int i = minX; i < maxX; i++)
            {
                var value = arr[i, lineIndex];
                var lvalue = Math.Round(Map(value, 1, 0, laserMin, laserMax), 0);
                if (lvalue > MINIMUM_ENGAVING_POWER)
                {
                    break;
                }

                minX = i;
            }

            for (int i = maxX - 1; i > minX; i--)
            {
                var value = arr[i, lineIndex];
                var lvalue = Math.Round(Map(value, 1, 0, laserMin, laserMax));
                if (lvalue > MINIMUM_ENGAVING_POWER)
                {
                    break;
                }

                maxX = i;
            }
        }
    }
}