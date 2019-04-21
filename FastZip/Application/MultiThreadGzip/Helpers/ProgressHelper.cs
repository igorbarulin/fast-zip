using System;
using MultiThreadGzip.Components;

namespace MultiThreadGzip.Helpers
{
    public static class ProgressHelper
    {
        public static void SetProgress(this Progress progress, long numerator, long denominator)
        {
            var ration = (int) Math.Ceiling((double) numerator / denominator * 100);
            progress.SetProgress(ration);
        }
    }
}