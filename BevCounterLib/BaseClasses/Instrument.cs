namespace Bev.Counter
{
    /// <summary>
    /// A abstract class to implement the functionality of instruments like lasers, counters, etc.
    /// Basically the identification of devices (both standards and DUTs).
    /// </summary>
    public abstract class Instrument
    {
        #region Private fields
        /// <summary>
        /// All fields are accessible by properties only!
        /// </summary>
        private string instrumentName;              // BEV2
        private string instrumentManufacturer;      // Winters Electro-Optics
        private string instrumentType;              // M100
        private string instrumentSerialNumber;      // 168
        private string instrumentFirmwareVersion;   // N.A.
        private string instrumentMMDB;              // MM000260
        #endregion

        #region Properties
        /// <summary>
        /// The getter checks if field is empty while the setter trimms the string.
        /// </summary>
        public string InstrumentName
        {
            get { return EditString(instrumentName); }
            set { instrumentName = value.Trim(); }
        }
        public string InstrumentManufacturer
        {
            get { return EditString(instrumentManufacturer); }
            set { instrumentManufacturer = value.Trim(); }
        }
        public string InstrumentType
        {
            get { return EditString(instrumentType); }
            set { instrumentType = value.Trim(); }
        }
        public string InstrumentSerialNumber
        {
            get { return EditString(instrumentSerialNumber); }
            set { instrumentSerialNumber = value.Trim(); }
        }
        public string InstrumentFirmwareVersion
        {
            get { return EditString(instrumentFirmwareVersion); }
            set { instrumentFirmwareVersion = value.Trim(); }
        }
        public string InstrumentMMDB
        {
            get { return EditString(instrumentMMDB); }
            set { instrumentMMDB = value.Trim(); } 
        }
        #endregion

        #region Ctor
        /// <summary>
        /// In the Ctor all fields are set.
        /// </summary>
        public Instrument():this("") { }
        public Instrument(string name)
        {
            instrumentName = name;
            instrumentManufacturer = "";
            instrumentType = "";
            instrumentSerialNumber = "";
            instrumentFirmwareVersion = "";
            instrumentMMDB = "MM______";
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Check if a given string is empty.
        /// </summary>
        /// <returns>The edited string.</returns>
        /// <param name="s">The string to be checked.</param>
        private string EditString(string s)
        {
            if (s.Trim() != "") return s;
            return "< not given >";
        }
        #endregion
    }
}
