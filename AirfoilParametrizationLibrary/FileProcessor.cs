using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace AirfoilParametrizationLibrary
{
    public static class FileProcessor
    {
        public static void WriteDataToFile(List<double> x, List<double> y, string name, string filename)
        {
            List<string> output = new List<string>();
            output.Add(name);

            for (int i = 0; i < x.Count; i++)
            {
                output.Add(string.Format(CultureInfo.InvariantCulture, "{0:0.#####}", x[i]) + "     " + string.Format(CultureInfo.InvariantCulture, "{0:0.#####}", y[i]));
            }

            File.WriteAllLines(filename, output);
        }

        public static (double[], double[]) GetXCoordFromFile(string filepath)
        {
            List<string> lines = File.ReadAllLines(filepath).ToList();
            lines.RemoveAt(0);

            var (xL, xU) = (new double[lines.Count / 2 + 1], new double[lines.Count / 2]);

            if (lines.Count % 2 == 0)
            {
                (xL, xU) = (new double[lines.Count / 2], new double[lines.Count / 2]);
            }

            for (int i=0; i<lines.Count/2; i++)
            {
                List<string> values = lines[i].Split("     ").ToList();
                xL[i] = double.Parse(values[0], CultureInfo.InvariantCulture);
            }

            for (int i = lines.Count/2 + 1; i < lines.Count; i++)
            {
                List<string> values = lines[i].Split("     ").ToList();
                xU[i-lines.Count/2 - 1] = double.Parse(values[0], CultureInfo.InvariantCulture);
            }

            return (xL, xU);
        }

        public static (double[], double[]) GetYCoordFromFile(string filepath)
        {
            List<string> lines = File.ReadAllLines(filepath).ToList();
            lines.RemoveAt(0);

            var (yL, yU) = (new double[lines.Count], new double[lines.Count]);

            for (int i = 0; i < lines.Count / 2; i++)
            {
                List<string> values = lines[i].Split("     ").ToList();
                yL[i] = double.Parse(values[1], CultureInfo.InvariantCulture);
            }

            for (int i = lines.Count / 2; i < lines.Count; i++)
            {
                List<string> values = lines[i].Split("     ").ToList();
                yU[i-lines.Count/2] = double.Parse(values[1], CultureInfo.InvariantCulture);
            }

            return (yL, yU);
        }
    }
}
