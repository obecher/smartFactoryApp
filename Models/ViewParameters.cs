using System.Drawing;

namespace MyAvaloniaApp.Models
{
    public struct Pose
    {
        public int X {get; }
        public int Y {get; }
        public double Angle {get; }
        public Pose(int x, int y, double angle)
        {
            X = x;
            Y = y;
            Angle = angle;
        }
        public Point Location => new Point(X,Y);
 
    }

    public static class ViewParameters
    {
        public static Pose RobotPose = new Pose(300, 300, 0);
        public static Rectangle Borders = new Rectangle(37, 37, 453, 480);
    }
}