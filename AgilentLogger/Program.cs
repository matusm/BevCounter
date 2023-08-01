using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bev.Counter;

namespace AgilentLogger
{
    class Program
    {
        static Counter counter;
        static bool stop = false;

        static void Main(string[] args)
        {
            counter = new Agilent53230A("TCPIP0::A-53230A-01246::inst0::INSTR");
            counter.UpdatedEventHandler += UpdateEventHandler;
            counter.ReadyEventHandler += ReadyEventHandler;
            counter.Connect();
            counter.SetupMeasurementMode(MeasurementMode.Frequency, GateTime.Gate1s);
            //counter
            //counter.StartMeasurementLoopThread();

            counter.StartMeasurementLoopThread(10);
        }

        private static void ReadyEventHandler(object obj, EventArgs e)
        {
            counter.Disconnect();
        }

        private static void UpdateEventHandler(object obj, EventArgs e)
        {
            Console.WriteLine(counter.LastValue);
        }
    }
}
