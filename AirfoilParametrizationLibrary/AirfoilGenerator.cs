using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace AirfoilParametrizationLibrary
{
    public static class AirfoilGenerator
    {
        public static void CreateNacaFourDigits(string nacaFourDigits, int panels)
        {
            double maxCamber = double.Parse(nacaFourDigits[0].ToString())/100;
            double posCamber = double.Parse(nacaFourDigits[1].ToString())/10;
            double thickness = double.Parse(nacaFourDigits.Substring(2))/100;

            double[] x = NumberProcessor.CreateRange(0, 1, panels);

            var (yt, yc, dx, theta, xu, yu, xl, yl) = (new double[x.Length], new double[x.Length], new double[x.Length], new double[x.Length], new double[x.Length], new double[x.Length], new double[x.Length], new double[x.Length]);

            for (int i = 0; i < x.Length; i++)
            {
                yt[i] = 5 * thickness * (0.2969 * Math.Sqrt(x[i]) - 0.1260 * (x[i]) - 0.3516 * Math.Pow(x[i], 2) + 0.2843 * Math.Pow(x[i], 3) - 0.1015 * Math.Pow(x[i], 4));

                if (x[i] <= posCamber)
                {
                    yc[i] = maxCamber * (x[i] / Math.Pow(posCamber, 2)) * (2 * posCamber - x[i]);
                    dx[i] = (2 * maxCamber) / Math.Pow(posCamber, 2) * (posCamber - x[i]);
                }
                else
                {
                    yc[i] = maxCamber * ((1 - x[i]) / Math.Pow(1 - posCamber, 2)) * (1 + x[i] - (2 * posCamber));
                    dx[i] = ((2 * maxCamber) / Math.Pow(1 - posCamber, 2)) * (posCamber - x[i]);
                }

                theta[i] = Math.Atan(dx[i]);
                xu[i] = x[i] - (yt[i] * Math.Sin(theta[i]));
                yu[i] = yc[i] + (yt[i] * Math.Cos(theta[i]));
                xl[i] = x[i] + (yt[i] * Math.Sin(theta[i]));
                yl[i] = yc[i] - (yt[i] * Math.Cos(theta[i]));
            }

            List<double> xOutput = new List<double>(xu);
            xOutput.AddRange(new List<double>(xl));
            List<double> yOutput = new List<double>(yu);
            yOutput.AddRange(new List<double>(yl));

            FileProcessor.WriteDataToFile(xOutput, yOutput, "NACA " + nacaFourDigits, Environment.CurrentDirectory + @"\airfoil.dat");
        }

        public static double FindError (string inputFile, string outputFile)
        {
            (double[] y1L, double[] y1U) = FileProcessor.GetYCoordFromFile(inputFile);
            double[] y1 = y1L.Concat(y1U).ToArray();

            (double[] y2L, double[] y2U) = FileProcessor.GetYCoordFromFile(outputFile);
            double[] y2 = y2L.Concat(y2U).ToArray();

            double sum = 0;
            for (int i =0; i<y1.Length; i++)
            {
                sum += Math.Abs(y1[i] - y2[i]);
            }

            return sum / y1.Length;
        }

        public static void CreateCST(double[] w, double[] xL, double[] xU, string filename = "default")
        {
            double[] wL = new double[w.Length / 2];
            double[] wU = new double[w.Length / 2];

            for (int i = 0; i<w.Length/2; i++)
            {
                wL[i] = w[i];
            }
            for (int i = w.Length/2; i < w.Length; i++)
            {
                wU[i- w.Length / 2] = w[i];
            }

            double[] yL = CreateClassShape(wL, xL);
            double[] yU = CreateClassShape(wU, xU);

            List<double> yOutput = new List<double>(yL);
            yOutput.AddRange(new List<double>(yU));
            List<double> xOutput = new List<double>(xL);
            xOutput.AddRange(new List<double>(xU));

            StringBuilder sb = new StringBuilder();
            foreach (double d in wL)
            {
                sb.Append(string.Format(CultureInfo.InvariantCulture, "{0:0.###}", d) + " ");
            }
            foreach (double d in wU)
            {
                sb.Append(string.Format(CultureInfo.InvariantCulture, "{0:0.###}", d) + " ");
            }

            if (filename == "default")
            {
                filename = Environment.CurrentDirectory + @"\airfoil_out.dat";
            }

            FileProcessor.WriteDataToFile(xOutput, yOutput, sb.ToString(), filename);
        }

        public static double[] CreateClassShape(double[] w, double[] x)
        {
            double[] classFunc = new double[x.Length];
            double[] shapeFunc = new double[x.Length];
            double[] y = new double[x.Length];

            for (int i = 0; i < x.Length; i++)
            {
                classFunc[i] = Math.Pow(x[i], 0.5) * (1 - x[i]);
            }

            int n = w.Length - 1;
            double[] K = new double[w.Length];

            for (int i = 1; i < K.Length + 1; i++)
            {
                K[i - 1] = NumberProcessor.Factorial(n) / (NumberProcessor.Factorial(i - 1) * NumberProcessor.Factorial((n) - (i - 1)));
            }

            for (int i = 0; i<x.Length; i++)
            {
                shapeFunc[i] = 0;
                for (int j = 0; j<K.Length; j++)
                {
                    shapeFunc[i] = shapeFunc[i] + w[j] * K[j] * Math.Pow(x[i], j) * Math.Pow(1 - x[i], n - j);
                }
            }

            for (int i = 0; i<x.Length; i++)
            {
                y[i] = classFunc[i] * shapeFunc[i];
            }

            return y;
        }
    }
}
