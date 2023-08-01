using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Ports;
using System.Linq;

namespace Bev.Counter
{
    /// <summary>
    /// Derived class for the use of HP/Agilent counters which are connected via RS232.
    /// In this mode data can be received only. No way to programaticaly change instrument's settings.
    /// </summary>
    public class SerialHpCounter : Counter
    {

        #region Sub-class specific fields
        private SerialPort portCOM;
        private DateTime dtOld;
        private DateTime dtNew;
        private double outputInterval;
        protected UnitSymbol unit;    // as sent by counter!
        private MeasureMode mode;
        #endregion

        #region Ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="T:HpSerial.SerialHpCounter"/> class.
        /// </summary>
        /// <param name="portName">RS232 port name.</param>
        /// <remarks>
        /// There is a virtual member call in constructor!
        /// </remarks>
        public SerialHpCounter(string portName)
        {
            base.portName = portName;
            InstrumentManufacturer = "HP / Agilent";
            IdentifyInstrument();
            Connect();
            EstimateGateTime();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:HpSerial.SerialHpCounter"/> class.
        /// </summary>
        /// <param name="portNumber">RS232 port number.</param>
        public SerialHpCounter(int portNumber) : this($"COM{portNumber}") { }

        #endregion

        #region Sub-class specific properties
        public UnitSymbol Unit => unit;
        public MeasureMode Mode { get => mode; private set { mode = value; InterpretMode(); } }
        public double OutputInterval => outputInterval;  // not needed
        #endregion

        #region Public methods
        /// <summary>
        /// Estimates the current gate time of counter and updates <c>dGateTime</c>.
        /// Only 0.1 s or integer seconds can be returned!
        /// </summary>
        /// <param name="sampleSize">Number of samples to be averaged.</param>
        /// <remarks>Sets <c>dGateTime</c> to 0 if not successful.</remarks>
        public void EstimateGateTime(int sampleSize)
        {
            double? t = GetTimeBetweenSamples(sampleSize);
            gateTime = InterpretGateTime(t);
            GateTimeToDouble();
        }

        /// <summary>
        /// Estimates the current gate time of counter and updates <c>dGateTime</c>.
        /// Only 0.1 s or integer seconds can be returned!
        /// </summary>
        /// <remarks>Sets <c>dGateTime</c> to 0 if not successful.</remarks>
        public void EstimateGateTime() => EstimateGateTime(3);

        /// <summary>
        /// Sets <c>gateTime</c> to a user supplied value.
        /// </summary>
        /// <remarks>More secure than a setter.</remarks>
        /// <param name="t">The gate time in s.</param>
        public void ForceGateTime(double t)
        {
            gateTime = InterpretGateTime(t);
            GateTimeToDouble();
        }

        /// <summary>
        /// Iff <c>mode</c> equals <c>.Unknow</c> it is set to <c>.Totalize</c>.
        /// </summary>
        public void ForceTotalizeMode()
        {
            if (mode == MeasureMode.Unknown) 
                Mode = MeasureMode.Totalize;
        }
        #endregion

        #region Override
        /// <summary>
        /// The com buffer should be cleared befor entering the loop. Otherwise garbage might be returned.
        /// </summary>
        protected override void StartMeasurementLoop()
        {
            if (!connected) 
                return;
            portCOM.DiscardInBuffer();
            base.StartMeasurementLoop();
        }
        #endregion

        #region Implementations of abstract methods
        public override void Connect()
        {
            // Disconnect() necessary only for this class
            Disconnect();
            try
            {
                portCOM = new SerialPort(portName, 9600, Parity.None, 8, StopBits.One);
                portCOM.ReadTimeout = 20000;
                portCOM.Open();
                portCOM.Handshake = Handshake.None;
                portCOM.DiscardInBuffer();
                connected = true;
            }
            catch
            {
                portCOM = null;
                connected = false;
            }
        }

        public override void Disconnect()
        {
            if (!connected) return;
            portCOM.Close();
            connected = false;
        }

        protected override double? _GetCounterValue()
        {
            double? measurementValue;
            if (!connected) return null;
            try
            {
                // receive text line from RS232 port (can stall!)
                string str = portCOM.ReadLine();
                // find time since last call
                dtNew = DateTime.UtcNow;
                sampleTime = dtNew;
                outputInterval = (dtNew - dtOld).TotalSeconds;
                dtOld = dtNew;
                // parse the measurement value from the string 
                measurementValue = ParseString(str);
                return measurementValue;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region Private methods
        /// <summary>
        /// Determines the average sample interval by performing <c>n</c> measurements.
        /// </summary>
        /// <param name="sampleSize">Number of samples to be averaged.</param>
        /// <returns>The average sample interval in s.</returns>
        /// <remarks>Method can take a very long time to return!</remarks>
        private double? GetTimeBetweenSamples(int sampleSize)
        {
            if (sampleSize < 2) sampleSize = 2;
            List<double> times = new List<double>();
            _GetCounterValue();  // first value to be discarded
            for (int i = 0; i < sampleSize; i++)
                if (_GetCounterValue() != null) times.Add(outputInterval);
            if (times.Count == 0) return null;
            return times.Average();
        }

        /// <summary>
        /// Tries to find out the instrument identification out of the port number. Very site specific!
        /// </summary>
        /// <remarks>Can be completly wrong!</remarks>
        private void IdentifyInstrument()
        {
            switch (portName.ToUpper().Trim())
            {
                case "COM6":
                    InstrumentManufacturer = "HEWLETT PACKARD";
                    InstrumentType = "53131 A";
                    InstrumentSerialNumber = "3736A23165";
                    break;
                case "COM3":
                    InstrumentManufacturer = "HEWLETT PACKARD";
                    InstrumentType = "53181 A";
                    InstrumentSerialNumber = "3548A02330";
                    break;
                case "COM1":
                    InstrumentManufacturer = "HEWLETT PACKARD";
                    InstrumentType = "53131 A";
                    InstrumentSerialNumber = "3736A21306";
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Translates the subclass specific mode to the baseclass mode enum.
        /// </summary>
        /// <remarks>Used by the respective setter only.</remarks>
        private void InterpretMode()
        {
            measurementMode = MeasurementMode.Unknown;
            if (mode == MeasureMode.Frequency) measurementMode = MeasurementMode.Frequency;
            if (mode == MeasureMode.Totalize) measurementMode = MeasurementMode.Totalize;
        }

        /// <summary>
        /// Translates a time value to the best fitting <c>base.GateTime</c> enumeration.
        /// </summary>
        /// <param name="time">The time in s or <c>null</c>.</param>
        /// <returns>The respective enumeration value.</returns>
        private GateTime InterpretGateTime(double? time)
        {
            if (time == null)
                return GateTime.GateUnknown;
            if (time < 1.0)
                return gateTime = GateTime.Gate01s;
            double gt1 = Math.Truncate((double)time);
            if (gt1 == 1) return GateTime.Gate1s;
            if (gt1 == 10) return GateTime.Gate10s;
            return GateTime.GateUnknown;
        }

        #endregion

        #region Private methods for string parsing

        /// <summary>
        /// Parses the string to get the numerical value, unit and mode.
        /// </summary>
        /// <returns>The numerical value.</returns>
        /// <param name="str">The string to be converted.</param>
        double? ParseString(string str)
        {
            MeasureMode modeOld = mode; // store the actual mode
            unit = UnitSymbol.None;

            string[] separator = { " ", "\t" };
            string[] token = str.Replace("\r", "").Split(separator, StringSplitOptions.RemoveEmptyEntries);

            // return null if no or more than two words
            if (token.Length == 0) return null;
            if (token.Length >= 3) return null;

            // catch voltage measurements, which are not parsed yet.
            if (str.Contains("V"))
            {
                unit = UnitSymbol.V;
                Mode = MeasureMode.Voltage;
                return null;
            }

            // convert the first word to a number
            double? tempValue;
            tempValue = StringToNumber(RemoveGroupingDelimiters(token[0]));
            Mode = MeasureMode.Unknown; // for the time beeing

            // if a second word is present, parse it too
            if (token.Length == 2)
            {
                unit = ParseStringTwo(token[1]);
                double k = DecimalMultiple(unit);
                if (tempValue != null)
                    tempValue *= k;
            }

            // now check if MeasureMode.Totalize was forced by user
            if (modeOld == MeasureMode.Totalize && mode == MeasureMode.Unknown)
            {
                Mode = MeasureMode.Totalize;
                // here we could divide by gateTime to obtain a frequency
                if (tempValue != null && gateTimeValue != 0)
                    return tempValue / gateTimeValue;
            }

            return tempValue;
        }

        /// <summary>
        /// Finds the multiplication factor for the unit(-prefix).
        /// </summary>
        /// <returns>The factor.</returns>
        /// <param name="symb">The unit symbol enum.</param>
        double DecimalMultiple(UnitSymbol symb)
        {
            switch (symb)
            {
                case UnitSymbol.MHz:
                    return 1.0e6;
                case UnitSymbol.Hz:
                    return 1.0;
                case UnitSymbol.M:
                    return 1.0e6;
                case UnitSymbol.us:
                    return 1.0e-6;
                default:
                    return 1.0;
            }
        }

        /// <summary>
        /// Parses the second token of the reply string.
        /// </summary>
        /// <returns>Enumeration representing the unit.</returns>
        /// <param name="str">The string to be parsed.</param>
        UnitSymbol ParseStringTwo(string str)
        {
            switch (str)
            {
                case "Hz":
                    mode = MeasureMode.Frequency;
                    return UnitSymbol.Hz;
                case "MHz":
                    mode = MeasureMode.Frequency;
                    return UnitSymbol.MHz;
                case "M":
                    mode = MeasureMode.Totalize;
                    return UnitSymbol.M;
                case "DEG":
                    mode = MeasureMode.Phase;
                    return UnitSymbol.Deg;
                case "s":
                    mode = MeasureMode.Unknown;
                    return UnitSymbol.s;
                case "us":
                    mode = MeasureMode.Unknown;
                    return UnitSymbol.us;
                default:
                    return UnitSymbol.Unknown;
            }
        }

        /// <summary>
        /// Removes digit grouping delimiters.
        /// </summary>
        /// <returns>The number string without grouping delimiters.</returns>
        /// <param name="str">Number string with grouping delimiters.</param>
        /// <remarks>Deletes all "," characters.</remarks>
        string RemoveGroupingDelimiters(string str)
        {
            return str.Replace(",", "");
        }

        /// <summary>
        /// Converts a string to a nullable double, if possible.
        /// </summary>
        /// <returns>Numerical value.</returns>
        /// <param name="str">The string to be converted.</param>
        /// <remarks>The decimal separator must be the point.</remarks>
        double? StringToNumber(string str)
        {
            if (double.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out double x)) return x;
            return null;
        }

        #endregion

        #region Unused implementations of abstract methods!

        public override void SendCommand(string cmd) => throw new NotImplementedException();

        public override string ReadLine() => throw new NotImplementedException();
        
        #endregion
    }
}
