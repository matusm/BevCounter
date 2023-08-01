using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;

namespace Bev.Counter
{
    public class Srs620 : Counter
    {
        private const int DELAY = 100;
        private Random rnd = new Random();
        private SerialPort comPort;
        

        public Srs620()
        {
            comPort = new SerialPort();
            connected = true;
            Init();
        }

        public Srs620(string com) : this()
        {
            portName = com;
        }

        public override void Connect()
        {
            if (connected)
                return;
            try
            {
                comPort.Open();
                connected = true;
                for (int i = 0; i < 5; i++)
                {
                    SendCommand("\n");
             //       ReadBufferInstant();
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Couldn't connect");
            }
        }

        public override void Disconnect()
        {
            if (comPort.IsOpen)
                comPort.Close();

            connected = false;
        }

        public override void SendCommand(string sCmd)
        {
            if (!connected)
                return;

            comPort.DiscardInBuffer();
            sCmd += "\r";
            byte[] cmd = Encoding.ASCII.GetBytes(sCmd);
            try
            {
                comPort.Write(cmd, 0, cmd.Length);
            }
            catch (Exception)
            {
                //TODO
                Console.WriteLine("Can't write to port");
            }
            Thread.Sleep(DELAY);
        }

     /*   public override string ReadBufferWaiting()
        {
            string tmp = "";
            int i = 0;

            while (!tmp.Contains("\n") && i < 20)
            {
                tmp += ReadBufferInstant();
                i++;
                Thread.Sleep(DELAY);
            }

            return tmp.Replace("\n", "");
        }

        public override string ReadBufferInstant()
        {
            if (!connected)
                return "Not connected";

            byte[] buffer = new byte[comPort.BytesToRead];

            try
            {
                comPort.Read(buffer, 0, buffer.Length);
                return Encoding.UTF8.GetString(buffer);
            }
            catch (Exception ex)
            {
                return "Can not read from device" + "\n" + ex;
            }
        }*/

        protected override double? _GetCounterValue()
        {
            /*if (!connected)
                return 0.0;

            string cmd = string.Format("MEAS?{0}", 0);
            SendCommand(cmd);
            string result = ReadLine();
            double x;
            //Double.TryParse(result,x)
            if (!Double.TryParse(result, NumberStyles.Any, CultureInfo.InvariantCulture, out x))
                return 0.0;

            return x;*/
            Thread.Sleep(500);
            return rnd.Next(0,10);
        }

        private void Init()
        {
            comPort.BaudRate = 9600;
            comPort.Parity = Parity.None;
            comPort.DataBits = 8;
            comPort.StopBits = StopBits.Two;
            comPort.Handshake = Handshake.None;
            comPort.ReadTimeout = 1000;
            comPort.WriteTimeout = 1000;
            comPort.RtsEnable = true;
            comPort.DtrEnable = true;
        }

        public override string ReadLine()
        {
            throw new NotImplementedException();
        }
    }
}
