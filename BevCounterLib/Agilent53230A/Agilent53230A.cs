using System;
using Ivi.Visa.Interop;


namespace Bev.Counter
{
    public class Agilent53230A : Counter
    {
        #region private variables
        private ResourceManager rm;
        private FormattedIO488 ioobj;
        #endregion

        #region Ctor
        public Agilent53230A()
        {
            rm = new ResourceManager();
            ioobj = new FormattedIO488();
            InstrumentManufacturer = "Agilent";
            InstrumentType = "53230A";
        }

        public Agilent53230A(string port) : this()
        {
            portname = port;
            Connect();
        }
        #endregion

        /// <summary>
        /// Starts the IO connection.
        /// </summary>
        public override void Connect()
        {
            if (connected)
                return;
            try
            {
                ioobj.IO = (IMessage)rm.Open(portname, AccessMode.NO_LOCK, 2000, "Timeout = 20000 ; TerminationCharacter = 10 ; TerminationCharacterEnabled=true");
                connected = true;
                //Reset();
            }
            catch (Exception)
            {                
            }            
        }

        /// <summary>
        /// Kills the IO connection.
        /// </summary>
        public override void Disconnect()
        {
            try
            {
                ioobj.IO.Close();
                connected = false;
            }
            catch (Exception)
            {                            
            }
        }

        /// <summary>
        /// Sends the given string to the instrument, if connected. Does nothing otherwise.
        /// </summary>
        /// <param name="cmd">string to be sent.</param>
        public override void SendCommand(string cmd)
        {
            if(connected)
            {
                try
                {
                    ioobj.WriteString(cmd, true);
                }
                catch (Exception)
                {
                }
            }            
        }

        /// <summary>
        /// Returns the next value in the buffer of the counter. Returns null if something went wrong.
        /// </summary>
        /// <returns>Next value from the counter. Null if something went wrong.</returns>
        protected override double? _GetCounterValue()
        {
            double result;

            if (!connected)
                return null;
            try
            {
                string str = SendQuerry("READ?");
                result = double.Parse(str.Replace('.', ','));
            }
            catch (Exception)
            {
                return null;
            }            

            return result;
        }

        /// <summary>
        /// Resets the counter, calls the base method and sends the proper commands to setup the mode and gatetime given.
        /// </summary>
        /// <param name="mm">Measurement mode to be set (frequency/totalize/unknown)</param>
        /// <param name="gt">Gatetime to be set (0.1s/1s/10s)</param>
        public override void SetupMeasurementMode(MeasurementMode mm, GateTime gt)
        {
            Reset();
            base.SetupMeasurementMode(mm, gt);
            if(mm == MeasurementMode.Frequency)
            {
                SendCommand("CONF:FREQ, (@1)");
                SendCommand("TRIG:COUN 1");
                SendCommand("SENS:FREQ:GATE:SOUR TIME");
                SendCommand("SENS:FREQ:GATE:TIME " + dGateTime);
            }
            else
            {
                //SendCommand("CONF:TOT:TIM "+ dGateTime +", (@1)");
            }       
        }

        /// <summary>
        /// Resets the counter, flushes all queues and register and prepares for measurement.
        /// Only takes action if counter is connected.
        /// </summary>
        public void Reset()
        {
            if(IsConnected)
            {
                // resets the counter
                SendCommand("*RST");
                // clear event register and error queue
                SendCommand("*CLS");
                // clear service request enable register
                SendCommand("*SRE 0");
                // clear event status enable register
                SendCommand("*ESE 0");
                // preset enable register and tran filter for op
                SendCommand("STATUS:PRESET");
            }
        }

        /// <summary>
        /// Reads if something is in counter buffer and returns it, does nothing if buffer is empty or timeout occurs.
        /// Only takes action if counter is connected.
        /// </summary>
        /// /// <returns>string from counter buffer.</returns>
        public override string ReadLine()
        {
            string output = "";
            if(connected)
            {
                try
                {
                    output = ioobj.ReadString();
                }
                catch (Exception e)
                {
                    output = e.Message;
                }
            }            
            return output;
        }

        /// <summary>
        /// Calls <c>SendCommand</c> with the given string and returns the value from <c>ReadLine</c>
        /// </summary>
        /// <param name="cmd">string to be sent.</param>
        /// <returns>result from <c>ReadLine.</c></returns>
        public string SendQuerry(string cmd)
        {
            SendCommand(cmd);
            return ReadLine();
        }

    }
}
