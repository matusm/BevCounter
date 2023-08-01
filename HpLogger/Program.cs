using System;
using System.Threading;
using System.Globalization;
using System.Linq;
using System.IO;
using Bev.Counter;
using Bev.UI;

namespace HpLogger
{
    class Program
    {
        static string sFileName;
        static StreamWriter hFile;
        static string outputFormat; // the format for the y-value

        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            Options options = new Options();    // for command line stuff
            ConsoleUI.Verbatim = !options.bQuiet;      // no console output if not verbatim
            ConsoleUI.Welcome();

            #region The CLA stuff
            string[] aFileNames;
            if (!CommandLine.Parser.Default.ParseArgumentsStrict(args, options))
                Console.WriteLine("*** ParseArgumentsStrict returned false");
            aFileNames = options.ListOfFileNames.ToArray();
            if (aFileNames.Length == 0)
                sFileName = ConsoleUI.Title;
            if (aFileNames.Length == 1)
                sFileName = Path.ChangeExtension(aFileNames[0], null);
            if (aFileNames.Length > 1)
                ConsoleUI.ErrorExit("More than one file name given!", 2);
            sFileName = Path.ChangeExtension(sFileName, "dat");
            #endregion

            // instantiate the counter object
            ConsoleUI.StartOperation("Initializing stuff");
            SerialHpCounter ctr = new SerialHpCounter(options.comPort);
            if (!ctr.IsConnected) ConsoleUI.ErrorExit("Counter not ready (wrong port?)!", 1);

            // register the event handlers
            ctr.UpdatedEventHandler += UpdateView;
            ctr.ReadyEventHandler += LoopReady;

            // consume the missing command line options
            if (options.bTotalize) ctr.ForceTotalizeMode();
            if (options.gTime > 0) ctr.ForceGateTime(options.gTime);

            // determine the output quantity for formatting
            string columnDescription = "<not set>";
            outputFormat = "  {1}";
            switch (ctr.Mode)
            {
                case MeasureMode.Unknown:
                    columnDescription = "unknown quantity";
                    if ((ctr.Unit == UnitSymbol.s) || (ctr.Unit == UnitSymbol.us))
                        columnDescription = "period/risetime/width or some other time in s";
                    break;
                case MeasureMode.Frequency:
                    outputFormat = "{1,16:F3}";
                    columnDescription = "frequency in Hz";
                    break;
                case MeasureMode.Totalize:
                    outputFormat = "{1,12:F0}";
                    columnDescription = "counts (totalize mode)";
                    break;
                case MeasureMode.Ratio:
                    columnDescription = "frequency ratio";
                    break;
                case MeasureMode.DutyCycle:
                    columnDescription = "duty cycle";
                    break;
                case MeasureMode.Phase:
                    columnDescription = "phase in degree";
                    break;
                case MeasureMode.Voltage:
                    columnDescription = "peak voltage in V";
                    break;
                default:
                    break;
            }

            // output file stuff
            hFile = new StreamWriter(sFileName);
            hFile.WriteLine("Output of {0} ver. {1}", ConsoleUI.Title, ConsoleUI.FullVersion);
            if (options.sComment != "") hFile.WriteLine(options.sComment);
            hFile.WriteLine("Logging started at "+ctr.InitTime.ToString("dd.MM.yyyy hh:mm"));
            hFile.WriteLine("Manufacturer: " + ctr.InstrumentManufacturer);
            hFile.WriteLine("Type: " + ctr.InstrumentType);
            hFile.WriteLine("Serial number: " + ctr.InstrumentSerialNumber);
            hFile.WriteLine("(Instrument identification might be wrong!)");
            hFile.WriteLine("Gate time: {0} s", ctr.GateTime);
            hFile.WriteLine("Mode: {0}", ctr.Mode);
            hFile.WriteLine("Unit: {0}", ctr.Unit);
            hFile.WriteLine("Connected to " + ctr.Portname);
            hFile.WriteLine("Column 1: time since start in s");
            hFile.WriteLine("Column 2: " + columnDescription);
            hFile.WriteLine("@@@@");
            hFile.Close();
            ConsoleUI.Done();

            // start the actual measurement
            ctr.StartMeasurementLoopThread(options.numSampl);

            // continue until user presses 'q' or 'Q'
            ConsoleUI.WriteLine("Press 'q' to exit application. (May take some time)");
            ConsoleKeyInfo key;
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
            hFile = File.AppendText(sFileName);
            hFile.WriteLine(line);
            hFile.Close();
            ConsoleUI.WriteLine(line);
        }
        #endregion

        #region Exit
        static void LoopReady(object sender, EventArgs e)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            var ob = sender as SerialHpCounter;
            ConsoleUI.WriteLine("Application stopped, no errors.");
            Environment.Exit(0);
        }
        #endregion
    }
}
