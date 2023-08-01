using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;

namespace HpLogger
{
    public class Options
    {
        [Option('c', "comport", DefaultValue = 1, HelpText = "RS232 port # the counter is connected to")]
        public int comPort { get; set; }

        [Option('n', DefaultValue = int.MaxValue, HelpText = "Number of data points to record")]
        public int numSampl { get; set; }

        [Option('q', "quiet", HelpText = "Quiet mode. No screen output (except for errors).")]
        public bool bQuiet { get; set; }

        [Option('g', "gatetime", DefaultValue = 0, HelpText = "Gate time value in s.")]
        public double gTime { get; set; }

        [Option('t', "totalize", HelpText = "Force totalize mode for unknown mode.")]
        public bool bTotalize { get; set; }

        [Option("comment", DefaultValue = "", HelpText = "Comment for outpot file.")]
        public string sComment { get; set; }

        [ValueList(typeof(List<string>), MaximumElements = 1)]
        public IList<string> ListOfFileNames { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            string AppName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            string AppVer = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            HelpText help = new HelpText
            {
                Heading = new HeadingInfo(AppName, "version " + AppVer),
                Copyright = new CopyrightInfo("Michael Matus", 2016),
                AdditionalNewLineAfterOption = false,
                AddDashesToOption = true
            };
            string sPre = "Program to record data send by an HP/Agilent 53131A or 53181A frequency counter connected via RS232. " +
                          "The counter settings must be entered manually on the instrument. ";
            help.AddPreOptionsLine(sPre);
            help.AddPreOptionsLine("");
            help.AddPreOptionsLine("Usage: " + AppName + " filename1 [options]");
            help.AddPostOptionsLine("");
            //help.AddPostOptionsLine("Supported values for -r: 1=min 2=max 3=avarage 4=mid 5=bias 6=first 7=last 8=center 9=linear 10=LSQ");

            help.AddOptions(this);

            return help;
        }

    }
}
