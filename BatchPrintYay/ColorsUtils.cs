using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;


namespace BatchPrintYay
{
    public static class ColorsUtils
    {
        public static List<Color> StringToColors(string colorsString)
        {
            List<Color> colors = new List<Color>();
            if (!colorsString.Contains(",")) return colors;
            string[] scolors = colorsString.Split(',');
            
            foreach (string scolor in scolors)
            {
                string[] srgb = scolor.Split(' ');
                int r = int.Parse(srgb[0]);
                int g = int.Parse(srgb[1]);
                int b = int.Parse(srgb[2]);
                Color color = Color.FromArgb(r, g, b);
                colors.Add(color);
            }
            return colors;
        }

        public static string ColorsToString(List<Color> colors)
        {
            string colorsString = "";

            for (int i = 0; i < colors.Count; i++)
            {
                Color c = colors[i];
                int r = (int)c.R;
                int g = (int)c.G;
                int b = (int)c.B;

                colorsString += r.ToString() + " " + g.ToString() + " " + b.ToString();
                if (i != colors.Count - 1)
                    colorsString += ",";
            }

            return colorsString;
        }

    }
}
