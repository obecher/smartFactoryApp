using System;
using System.Drawing;
using System.Reflection.Metadata.Ecma335;
using static MyAvaloniaApp.Models.Parameters;

namespace MyAvaloniaApp.Models
{
    public class LegoBricks : Beweger
    {
        private LegoColor color;
        private LegoSize size;
        public ParametersEncoder.LegoColor Color {get { return color;} set {color = value;} }
        public LegoSize Size{get {return size;} set { size = value;} }
        public Pose LegoPose { get; }

        public LegoBricks(Pose pose, LegoColor color, LegoSize size)
        {
            Color = color;
            Size = size;
            LegoPose = pose;
        }
    }
}