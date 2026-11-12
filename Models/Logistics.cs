using System;
using System.Collections.Generic;

namespace MyAvaloniaApp.Models;

// Ein Planschritt: Welche Farbe soll der Roboter als Nächstes holen, und von welcher Station?
public readonly struct PickPlan
{
    public Parameters.LegoColor Color { get; }
    public Parameters.PickStation Station { get; }

    public PickPlan(Parameters.LegoColor color, Parameters.PickStation station)
    {
        Color = color;
        Station = station;
    }
}

// Die Logistik kennt die Bestellung (Order) und plant, welcher Stein als Nächstes geholt wird.
public class Logistics
{
    // Die Reihenfolge entspricht den Zahlen des Enums LegoColor (yellow = 0 ... green = 3).
    // Deshalb kann die Farbe direkt als Index in die drei Zahlenfelder unten dienen.
    private static readonly Parameters.LegoColor[] farben =
    {
        Parameters.LegoColor.yellow,
        Parameters.LegoColor.blue,
        Parameters.LegoColor.red,
        Parameters.LegoColor.green
    };

    private int[] order = new int[4];       // bestellte Steine pro Farbe
    private int[] delivered = new int[4];   // beim Kunden angekommene Steine pro Farbe
    private int[] underway = new int[4];    // zum Kunden unterwegs, aber noch nicht angekommen
    private Random random = new Random();
    private PickPlan? naechsterSchritt;     // der aktuelle Plan (null = keiner)

    // ---------- Zustand der Bestellung ----------
    public int TotalOrdered => Sum(order);
    public int TotalDelivered => Sum(delivered);
    public int TotalUnderway => Sum(underway);

    public bool HasOrder => TotalOrdered > 0;
    public bool OrderActive => HasOrder && TotalDelivered < TotalOrdered;
    public bool OrderComplete => HasOrder && TotalDelivered >= TotalOrdered;

    // Alle bestellten Steine sind entweder angekommen oder schon unterwegs
    public bool AllBricksSent => HasOrder && TotalDelivered + TotalUnderway >= TotalOrdered;

    // Der wievielte Stein ist als Nächstes dran? (für "Step 4/8")
    public int CurrentStep => TotalDelivered + TotalUnderway + 1;

    public PickPlan? NextPick => naechsterSchritt;

    public int GetOrder(Parameters.LegoColor color) => order[(int)color];
    public int GetDelivered(Parameters.LegoColor color) => delivered[(int)color];

    private static int Sum(int[] werte)
    {
        int summe = 0;
        foreach (int wert in werte) summe += wert;
        return summe;
    }

    // ---------- Order ----------

    // Würfelt eine neue Bestellung: je Farbe 0 bis 3 Steine, insgesamt mindestens einer.
    // Solange die aktuelle Bestellung noch läuft, passiert nichts (Rückgabe false).
    public bool CreateRandomOrder()
    {
        if (OrderActive) return false;

        int summe;
        do
        {
            summe = 0;
            for (int i = 0; i < order.Length; i++)
            {
                order[i] = random.Next(0, 4);   // 0, 1, 2 oder 3
                summe += order[i];
            }
        }
        while (summe == 0);

        Array.Clear(delivered);
        Array.Clear(underway);
        naechsterSchritt = null;
        return true;
    }

    // Wird diese Farbe noch gebraucht? Steine, die schon unterwegs sind, zählen mit,
    // sonst könnte man zu viele Steine gleichzeitig losschicken.
    public bool IsNeeded(Parameters.LegoColor color)
    {
        int i = (int)color;
        return OrderActive && order[i] - delivered[i] - underway[i] > 0;
    }

    // Ein Stein der Farbe wurde zum Kunden losgeschickt
    public void BrickSent(Parameters.LegoColor color)
    {
        underway[(int)color]++;
    }

    // Ein Stein der Farbe ist beim Kunden angekommen
    public void BrickDelivered(Parameters.LegoColor color)
    {
        int i = (int)color;
        if (underway[i] > 0) underway[i]--;
        delivered[i]++;
    }

    // ---------- Plan ----------

    // Plant den nächsten Stein: zufällig eine Farbe, die gebraucht wird UND irgendwo vorhanden ist.
    // Rückgabe null, wenn es nichts zu holen gibt.
    public PickPlan? PlanNextStep(IEnumerable<LegoBrick> bricks)
    {
        naechsterSchritt = null;
        if (!OrderActive) return null;

        List<Parameters.LegoColor> kandidaten = new List<Parameters.LegoColor>();
        foreach (Parameters.LegoColor farbe in farben)
        {
            if (IsNeeded(farbe) && StationFor(bricks, farbe) != null)
            {
                kandidaten.Add(farbe);
            }
        }

        if (kandidaten.Count == 0) return null;

        Parameters.LegoColor gewaehlt = kandidaten[random.Next(kandidaten.Count)];
        naechsterSchritt = new PickPlan(gewaehlt, StationFor(bricks, gewaehlt)!.Value);
        return naechsterSchritt;
    }

    // Gibt den bestehenden Plan zurück, solange er noch stimmt; sonst wird neu geplant
    public PickPlan? EnsurePlan(IEnumerable<LegoBrick> bricks)
    {
        if (naechsterSchritt != null
            && IsNeeded(naechsterSchritt.Value.Color)
            && StationFor(bricks, naechsterSchritt.Value.Color) == naechsterSchritt.Value.Station)
        {
            return naechsterSchritt;
        }

        return PlanNextStep(bricks);
    }

    // Wo liegt ein Stein dieser Farbe? Zuerst die Produktionslinien in fester Reihenfolge,
    // zuletzt die Recyclinglinie. null = nirgends.
    private static Parameters.PickStation? StationFor(IEnumerable<LegoBrick> bricks, Parameters.LegoColor farbe)
    {
        Parameters.PickStation[] linien =
        {
            Parameters.PickStation.yellow,
            Parameters.PickStation.blue,
            Parameters.PickStation.red,
            Parameters.PickStation.green
        };

        foreach (Parameters.PickStation linie in linien)
        {
            foreach (LegoBrick brick in bricks)
            {
                if (brick.Status == Parameters.BrickStatus.OnProductionLine
                    && brick.Color == farbe
                    && LineOf(brick) == linie)
                {
                    return linie;
                }
            }
        }

        foreach (LegoBrick brick in bricks)
        {
            if (brick.Status == Parameters.BrickStatus.OnRecycleLine && brick.Color == farbe)
            {
                return Parameters.PickStation.recycle;
            }
        }

        return null;
    }

    // Auf welcher Produktionslinie liegt der Stein? (Seine Heimlinie, dort bleibt er ja)
    private static Parameters.PickStation? LineOf(LegoBrick brick)
    {
        return brick.Heimlinie?.Color switch
        {
            Parameters.LegoColor.yellow => Parameters.PickStation.yellow,
            Parameters.LegoColor.blue => Parameters.PickStation.blue,
            Parameters.LegoColor.red => Parameters.PickStation.red,
            Parameters.LegoColor.green => Parameters.PickStation.green,
            _ => null
        };
    }
}
