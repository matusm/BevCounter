using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace HpLogger
{
    public class Options
    {
        [Option('c', "comport", Default = "COM1", HelpText = "RS232 port the counter is connected to")]
        public string ComPortName { get; set; }

        [Option('n', Default = int.MaxValue, HelpText = "Number of data points to record")]
        public int NumberOfSamples { get; set; }

        [Option('q', "quiet", HelpText = "Quiet mode. No screen output (except for errors).")]
        public bool BeQuiet { get; set; }
        public bool Verbatim => !BeQuiet;

        [Option('g', "gatetime", Default = 0.0, HelpText = "Gate time value in s.")]
        public double GateTime { get; set; }

        [Option('t', "totalize", HelpText = "Force totalize mode for unknown mode.")]
        public bool ForceTotalize { get; set; }

        [Option("MJD", HelpText = "Use Modified Julian Date for timestamps.")]
        public bool UseMJD { get; set; }

        [Option("comment", Default = "", HelpText = "Comment for output file.")]
        public string UserComment { get; set; }

        [Value(0, MetaName = "filename", Required = false, HelpText = "Output file name.")]
        public string FileName { get; set; }

        [Usage(ApplicationAlias = "HpLogger")]
        public static IEnumerable<Example> Examples =>
            new List<Example>
            {
                new Example("Record to file", new Options { FileName = "output" })
            };
    }
}
