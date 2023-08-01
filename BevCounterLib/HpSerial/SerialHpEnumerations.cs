namespace Bev.Counter
{
    /// <summary>
    /// Unit symbols provided by this special counter type,
    /// </summary>
    public enum UnitSymbol
    {
        None,       // no unit give, e.g. in totalize, Ratio
        Unknown,    // unidentified unit (yet)
        Hz,         // as the name implies
        MHz,        // as the name implies
        M,          // 1e6 for totalize mode
        s,          // second
        us,         // 1e-6 second
        Deg,        // degree (Phase)
        V           // voltage
    }

    /// <summary>
    /// Possible modes for this special counter typ. Equivalent to <c>base.MeasurementMode</c>.
    /// </summary>
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
