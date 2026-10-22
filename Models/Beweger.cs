using System;
using System.Collections.Generic;
using System.Drawing;

namespace MyAvaloniaApp.Models
{
    // Basisklasse für alles, was sich auf dem Plan bewegt (Roboter und Legosteine).
    // Ein Beweger ist ein Rechteck mit Pose (x, y, angle). x/y = linke obere Ecke des
    // ungedrehten Rechtecks, angle = Drehung in Grad im Uhrzeigersinn um die Rechteck-Mitte.
    public class Beweger
    {
        private double x;
        private double y;
        private double angle;
        private readonly double width;
        private readonly double height;

        private Rectangle borders = Rectangle.Empty;            // Spielfeld; leer = keine Begrenzung
        private List<ProductionLine> productionLines = new List<ProductionLine>();   // Hindernisse
        private List<LegoBrick> bricks = new List<LegoBrick>();                      // greifbare Steine

        private LegoBrick? dockobjekt;       // der Stein, den der Beweger gerade trägt
        private LegoBrick? freigegeben;      // zuletzt losgelassener Stein (wird nicht sofort wieder gegriffen)
        private double dockWinkelOffset;     // Drehwinkel des Steins relativ zum Roboter
        private double dockOffsetX;          // Steinmitte relativ zur Roboter-Mitte,
        private double dockOffsetY;          // gemessen im Koordinatensystem des Roboters

        // ---------- Konstruktoren ----------
        public Beweger(double x, double y, double angle, double width, double height)
        {
            this.x = x;
            this.y = y;
            this.angle = Normalisiere(angle);
            this.width = width;
            this.height = height;
        }

        public Beweger(Pose pose, double width, double height)
            : this(pose.X, pose.Y, pose.Angle, width, height)
        {
        }

        public Beweger(Pose pose, double width, double height, Rectangle borders)
            : this(pose, width, height)
        {
            this.borders = borders;
        }

        // ---------- Eigenschaften ----------
        public double X => x;
        public double Y => y;
        public double Angle => angle;
        public double Width => width;
        public double Height => height;
        public Pose Pose => new Pose(x, y, angle);
        public Point Location => new Point((int)Math.Round(x), (int)Math.Round(y));

        public Rectangle Borders
        {
            get { return borders; }
            set { borders = value; }
        }

        public List<ProductionLine> ProductionLines
        {
            get { return productionLines; }
            set { productionLines = value; }
        }

        public List<LegoBrick> Bricks
        {
            get { return bricks; }
            set { bricks = value; }
        }

        public bool HatDockobjekt => dockobjekt != null;

        // ---------- Geometrie ----------
        public static double Normalisiere(double winkel)
        {
            winkel %= 360.0;
            if (winkel < 0) winkel += 360.0;
            return winkel;
        }

        // Die vier Ecken eines gedrehten Rechtecks (im Uhrzeigersinn, beginnend links oben)
        public static PointF[] Ecken(double x, double y, double width, double height, double angle)
        {
            double cx = x + width / 2.0;
            double cy = y + height / 2.0;
            double rad = angle * Math.PI / 180.0;
            double cos = Math.Cos(rad);
            double sin = Math.Sin(rad);

            double[] dxs = { -width / 2.0, width / 2.0, width / 2.0, -width / 2.0 };
            double[] dys = { -height / 2.0, -height / 2.0, height / 2.0, height / 2.0 };

            PointF[] ecken = new PointF[4];
            for (int i = 0; i < 4; i++)
            {
                // Drehung im Uhrzeigersinn (y zeigt nach unten): wie RotateTransform in Avalonia
                ecken[i] = new PointF(
                    (float)(cx + dxs[i] * cos - dys[i] * sin),
                    (float)(cy + dxs[i] * sin + dys[i] * cos));
            }
            return ecken;
        }

        public PointF[] Ecken() => Ecken(x, y, width, height, angle);

        // Überlappen sich zwei gedrehte Rechtecke? Trennachsen-Satz (SAT):
        // Gibt es EINE Achse (Normale einer Kante), auf der die Schatten beider Rechtecke
        // nicht überlappen, dann überlappen die Rechtecke nicht.
        public static bool RechteckeUeberlappen(PointF[] a, PointF[] b)
        {
            foreach (PointF[] rechteck in new[] { a, b })
            {
                for (int i = 0; i < 4; i++)
                {
                    PointF p1 = rechteck[i];
                    PointF p2 = rechteck[(i + 1) % 4];
                    double nx = -(p2.Y - p1.Y);
                    double ny = p2.X - p1.X;

                    Projektion(a, nx, ny, out double minA, out double maxA);
                    Projektion(b, nx, ny, out double minB, out double maxB);

                    if (maxA < minB || maxB < minA)
                    {
                        return false;   // Lücke gefunden: kein Kontakt
                    }
                }
            }
            return true;
        }

        private static void Projektion(PointF[] ecken, double nx, double ny, out double min, out double max)
        {
            min = double.MaxValue;
            max = double.MinValue;
            foreach (PointF p in ecken)
            {
                double wert = p.X * nx + p.Y * ny;
                if (wert < min) min = wert;
                if (wert > max) max = wert;
            }
        }

        // Greifpunkt (TCP): Mitte der oberen Kante des Rechtecks, auf dem Plan
        public PointF Greifpunkt()
        {
            double cx = x + width / 2.0;
            double cy = y + height / 2.0;
            double rad = angle * Math.PI / 180.0;
            // lokal liegt der TCP bei (0, -height/2), gedreht um die Mitte
            return new PointF(
                (float)(cx + (height / 2.0) * Math.Sin(rad)),
                (float)(cy - (height / 2.0) * Math.Cos(rad)));
        }

        // Abstand eines Punktes zum (gedrehten) Rechteck. 0 = Punkt liegt im Rechteck.
        public double AbstandZu(PointF p)
        {
            double cx = x + width / 2.0;
            double cy = y + height / 2.0;
            double rad = angle * Math.PI / 180.0;
            double vx = p.X - cx;
            double vy = p.Y - cy;

            // Punkt ins Koordinatensystem des Rechtecks zurückdrehen
            double lx = vx * Math.Cos(rad) + vy * Math.Sin(rad);
            double ly = -vx * Math.Sin(rad) + vy * Math.Cos(rad);

            double ex = Math.Max(Math.Abs(lx) - width / 2.0, 0);
            double ey = Math.Max(Math.Abs(ly) - height / 2.0, 0);
            return Math.Sqrt(ex * ex + ey * ey);
        }

        private bool InnerhalbGrenzen(PointF[] ecken)
        {
            if (borders.IsEmpty) return true;
            foreach (PointF p in ecken)
            {
                if (p.X < borders.Left || p.X > borders.Right || p.Y < borders.Top || p.Y > borders.Bottom)
                {
                    return false;
                }
            }
            return true;
        }

        public bool KollidiertMitProduktionslinie(PointF[] ecken)
        {
            foreach (ProductionLine line in productionLines)
            {
                if (RechteckeUeberlappen(ecken, line.Ecken()))
                {
                    return true;
                }
            }
            return false;
        }

        // ---------- Bewegen und Drehen ----------
        public virtual bool Bewegen(Parameters.Direction direction)
        {
            return Bewegen(direction, Parameters.step);
        }

        public virtual bool Bewegen(Parameters.Direction direction, double distance)
        {
            var (dx, dy) = Vektor(direction, distance);
            return Verschieben(dx, dy);
        }

        public bool Drehen(Parameters.Rotation rotation)
        {
            return Drehen(rotation, Parameters.rotationStep);
        }

        public bool Drehen(Parameters.Rotation rotation, double degrees)
        {
            double delta = rotation == Parameters.Rotation.left ? -degrees : degrees;
            return VersucheZuSetzen(x, y, Normalisiere(angle + delta));
        }

        public bool WinkelSetzen(double neuerWinkel)
        {
            return VersucheZuSetzen(x, y, Normalisiere(neuerWinkel));
        }

        // Richtung -> Verschiebung in Plan-Koordinaten (y zeigt nach unten),
        // diagonal wird auf dieselbe Länge normiert
        protected static (double dx, double dy) Vektor(Parameters.Direction direction, double distance)
        {
            int ix = 0;
            int iy = 0;
            switch (direction)
            {
                case Parameters.Direction.up: iy = -1; break;
                case Parameters.Direction.down: iy = 1; break;
                case Parameters.Direction.left: ix = -1; break;
                case Parameters.Direction.right: ix = 1; break;
                case Parameters.Direction.upLeft: ix = -1; iy = -1; break;
                case Parameters.Direction.upRight: ix = 1; iy = -1; break;
                case Parameters.Direction.downLeft: ix = -1; iy = 1; break;
                case Parameters.Direction.downRight: ix = 1; iy = 1; break;
                default: break;
            }

            double laenge = Math.Sqrt(ix * ix + iy * iy);
            if (laenge == 0) return (0, 0);
            return (ix * distance / laenge, iy * distance / laenge);
        }

        protected bool Verschieben(double dx, double dy)
        {
            return VersucheZuSetzen(x + dx, y + dy, angle);
        }

        // Setzt die neue Pose nur, wenn sie erlaubt ist (Rand, Linien, getragener Stein).
        // Rückgabe: true = Bewegung ausgeführt, false = blockiert.
        protected bool VersucheZuSetzen(double neuX, double neuY, double neuWinkel)
        {
            PointF[] ecken = Ecken(neuX, neuY, width, height, neuWinkel);

            if (!InnerhalbGrenzen(ecken)) return false;
            if (KollidiertMitProduktionslinie(ecken)) return false;
            if (!DockobjektInnerhalbGrenzen(neuX, neuY, neuWinkel)) return false;

            x = neuX;
            y = neuY;
            angle = neuWinkel;

            DockobjektNachziehen();
            NachBewegung();
            return true;
        }

        // Wird nach jeder erfolgreichen Bewegung aufgerufen (der Roboter prüft hier das Docking)
        protected virtual void NachBewegung()
        {
        }

        // Nur für das Nachziehen eines getragenen Steins: setzt die Pose ohne Prüfung
        public void SetzePose(double neuX, double neuY, double neuWinkel)
        {
            x = neuX;
            y = neuY;
            angle = Normalisiere(neuWinkel);
        }

        // ---------- Docking ----------
        // Ist der Stein nah genug am Greifpunkt?
        public bool Kontakt(LegoBrick brick)
        {
            return brick.AbstandZu(Greifpunkt()) <= Parameters.dockDistance;
        }

        // Greift den nächstgelegenen Stein im Greifbereich (nur wenn noch nichts getragen wird)
        protected void PruefeDock()
        {
            if (dockobjekt != null) return;

            // Ein losgelassener Stein wird erst wieder gegriffen, wenn der Greifpunkt einmal weg war
            if (freigegeben != null && !Kontakt(freigegeben))
            {
                freigegeben = null;
            }

            PointF tcp = Greifpunkt();
            LegoBrick? bester = null;
            double besterAbstand = double.MaxValue;

            foreach (LegoBrick brick in bricks)
            {
                if (brick.Gedockt || brick == freigegeben) continue;

                double abstand = brick.AbstandZu(tcp);
                if (abstand <= Parameters.dockDistance && abstand < besterAbstand)
                {
                    bester = brick;
                    besterAbstand = abstand;
                }
            }

            if (bester != null)
            {
                Dock(bester);
            }
        }

        // Merkt sich die Lage des Steins relativ zum Roboter, damit er mitfahren kann
        public bool Dock(LegoBrick brick)
        {
            if (dockobjekt != null || brick.Gedockt) return false;

            double rad = angle * Math.PI / 180.0;
            double vx = (brick.X + brick.Width / 2.0) - (x + width / 2.0);
            double vy = (brick.Y + brick.Height / 2.0) - (y + height / 2.0);

            // Vektor Roboter-Mitte -> Stein-Mitte ins Roboter-Koordinatensystem zurückdrehen
            dockOffsetX = vx * Math.Cos(rad) + vy * Math.Sin(rad);
            dockOffsetY = -vx * Math.Sin(rad) + vy * Math.Cos(rad);
            dockWinkelOffset = brick.Angle - angle;

            dockobjekt = brick;
            brick.Gedockt = true;
            brick.AufProduktionslinie = false;
            return true;
        }

        public void Undock()
        {
            if (dockobjekt == null) return;

            dockobjekt.Gedockt = false;
            freigegeben = dockobjekt;
            dockobjekt = null;
        }

        // Pose des getragenen Steins, wenn der Roboter die Pose (rx, ry, ra) hätte
        private void DockobjektPose(double rx, double ry, double ra, out double bx, out double by, out double ba)
        {
            LegoBrick brick = dockobjekt!;
            double rad = ra * Math.PI / 180.0;
            double cx = rx + width / 2.0;
            double cy = ry + height / 2.0;

            // Offset aus dem Roboter-Koordinatensystem wieder auf den Plan drehen
            double mx = cx + dockOffsetX * Math.Cos(rad) - dockOffsetY * Math.Sin(rad);
            double my = cy + dockOffsetX * Math.Sin(rad) + dockOffsetY * Math.Cos(rad);

            bx = mx - brick.Width / 2.0;
            by = my - brick.Height / 2.0;
            ba = Normalisiere(ra + dockWinkelOffset);
        }

        protected void DockobjektNachziehen()
        {
            if (dockobjekt == null) return;

            DockobjektPose(x, y, angle, out double bx, out double by, out double ba);
            dockobjekt.SetzePose(bx, by, ba);
        }

        // Bleibt der getragene Stein im Spielfeld, wenn der Roboter die Pose (rx, ry, ra) einnimmt?
        public bool DockobjektInnerhalbGrenzen(double rx, double ry, double ra)
        {
            if (dockobjekt == null) return true;

            DockobjektPose(rx, ry, ra, out double bx, out double by, out double ba);
            return InnerhalbGrenzen(Ecken(bx, by, dockobjekt.Width, dockobjekt.Height, ba));
        }
    }
}
