using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace PCAGenerator
{
    public class PCA
    {
        public string InputFile { get; set; }
        public string ColorVar { get; set; }
        public string ShapeVar { get; set; }
        public bool ShowNames { get; set; }
        public string PlotTitle { get; set; }
        public string FileName { get; set; }
        public int SampleNameFontSize { get; set; }
        public string SampleNameOffset { get; set; }
        public int[] Offset { get; set; }

        public int PointSize { get; set; }
        public float PointTransparency { get; set; }

        public int TitleFontSize { get; set; }
        public int AxisTitleFontSize { get; set; }
        public int AxisLabelFontSize { get; set; }
        public int LegendTitleFontSize { get; set; }
        public int LegendFontSize { get; set; }

        public bool ShowGrid { get; set; }
        public string GridStyle { get; set; }
        public float GridTransparency { get; set; }
        public int PlotWidth { get; set; }
        public int PlotHeight { get; set; }
        public int Dpi { get; set; }

        public List<string> ExcludingSamples { get; set; } = new List<string>();
    }
}
