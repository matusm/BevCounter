using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bev.Counter;
using System.Threading;
using System.Globalization;

namespace HpTest
{
    class HpTest
    {
        static string outputFormat; // the format for the y-value

        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            outputFormat = "  {1}";

            SerialHpCounter ctr = new SerialHpCounter("COM1");

            // register the event handlers
            ctr.UpdatedEventHandler += UpdateView;
            ctr.ReadyEventHandler += LoopReady;
            ctr.TimeOutEventHandler += Foo;

            // start the actual measurement
            ctr.StartMeasurementLoopThread();

            // continue until user presses 'q' or 'Q'
            Console.WriteLine("Press 'q' to exit thread. (May take some time)");
            ConsoleKeyInfo key;
            do
            {
                key = Console.ReadKey(true);
            } while ((key.KeyChar != 'q') && (key.KeyChar != 'Q'));
            ctr.RequestStopMeasurementLoop();

            ctr.Disconnect();


            // Wait until user restarts
            Console.WriteLine("Press 's' to start new thread.");
            do
            {
                key = Console.ReadKey(true);
            } while ((key.KeyChar != 's') && (key.KeyChar != 'S'));

            // start the actual measurement
            ctr.Connect();

            
            ctr.StartMeasurementLoopThread();

            // continue until user presses 'q' or 'Q'
            Console.WriteLine("Press 'q' to exit thread. (May take some time)");
            do
            {
                key = Console.ReadKey(true);
            } while ((key.KeyChar != 'q') && (key.KeyChar != 'Q'));
            ctr.RequestStopMeasurementLoop();
        }

        #region Data output
        static void UpdateView(object sender, EventArgs e)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            var ob = sender as SerialHpCounter;
            // format the output string
            double timeSinceStart = (ob.SampleTime - ob.InitTime).TotalSeconds;
            string line = string.Format("{0,10:F1} " + outputFormat, timeSinceStart, ob.LastValue);
            Console.WriteLine(line);
        }
        #endregion

        #region Exit
        static void LoopReady(object sender, EventArgs e)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            var ob = sender as SerialHpCounter;
            Console.WriteLine("LoopReady Event");
            //Environment.Exit(0);
        }
        #endregion

        static void Foo(object sender, EventArgs e)
        {
            Console.WriteLine("Timeout");
        }


    }
}
