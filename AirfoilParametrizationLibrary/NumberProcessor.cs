using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirfoilParametrizationLibrary
{
    public static class NumberProcessor
    {
        public static double[] CreateRange(double a, double b, int n)
        {
            double[] output = new double[n + 1];
            double step = (b - a) / Convert.ToDouble(n);

            for (int i = 0; i <= n; i++)
            {
                output[i] = step * i + a;
            }

            return output;
        }

        public static double Factorial(double a)
        {
            if (a == 1 || a == 0)
            {
                return 1;
            }
            else
            {
                return a * Factorial(a - 1);
            }
        }
    }
}
