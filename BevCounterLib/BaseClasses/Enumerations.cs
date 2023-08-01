namespace Bev.Counter
{
    /// <summary>
    /// Generic modes each frequency counter should provide.
    /// </summary>
    public enum MeasurementMode { Unknown, Frequency, Totalize }
    
    /// <summary>
    /// generic gate time each frequency counter should provide.
    /// </summary>
    public enum GateTime { GateUnknown, Gate01s, Gate1s, Gate10s }
}
