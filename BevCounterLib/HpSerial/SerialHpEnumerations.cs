namespace Bev.Counter
{
    public enum UnitSymbol
    {
        None,       // no unit given, e.g. in totalize, Ratio. unit = 1
        Unknown,    // unidentified unit (yet)
        Hz,         // as the name implies
        MHz,        // as the name implies
        M,          // 1e6 for totalize mode
        s,          // second
        us,         // 1e-6 second
        Deg,        // degree (Phase)
        V           // voltage
    }

    public enum MeasureMode
    {
        Unknown,
        Frequency,
        Totalize,
        Ratio,
        DutyCycle,
        Phase,
        Voltage
    }
}
