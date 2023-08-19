using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGrid.Models.Visualizations
{
    public enum ChartType
    {
        Area,

        Bar,

        Pie,

        Line,
    }
    public class ChartVisualization : VisualizationBase
    {
        public ChartType Type { get; set; }
    }
}
