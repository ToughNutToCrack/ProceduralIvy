using UnityEngine;

public static class Constants {
    public const string root = "root";
    public const string p1 = "p1";
    public const string p2 = "p2";
    public const string p3 = "p3";
    public const string p4 = "p4";
    public const string m0 = "m0";
    public const string m1 = "m1";

    public static Color getColor(string name) {
        Color c = name switch {
            root => Color.green,
            p1 => Color.red,
            p2 => Color.blue,
            p3 => Color.yellow,
            p4 => Color.white,
            m0 => Color.gray,
            m1 => Color.magenta,
            _ => Color.black
        };
        return c;
    }
}