//*************************************************************************************************
//
// Library for the control of various frequency counters via serial port or ethernet.
// Attention: currently a single board is supported (and tested) only
// 
// Author: Stefan Haas, 2015
//
// Usage:
// 1.) create instance of the respective counter class with port as parameter
// 2.) call one of the methods (see source)
//
// Example:
// 
//  
// 
// 
// version history
//
// 0.99 TimeOutEventHandler added 
// 0.97 _GetCounterValue() in HpSerial provided with try/catch.
//      Otherwise will crash on calling Disconnect() while still in StartMeasurementLoopThread()
// 0.96 added private variable stillInLoop to prevent generating new loop while a different one still active
// 0.94 first working version
// 
//*************************************************************************************************

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace Bev.Counter
{
    public abstract class Counter : Instrument
    {
        #region Variables
        protected CultureInfo intrumentCulture = new CultureInfo("en-US");
        protected bool connected;
        protected bool stillInLoop; // MM
        protected string portName;
        protected volatile bool stopRequest;
        protected List<Tuple<DateTime, double>> data;
        protected int loopSamples;
        protected DateTime initTime;  // MM
        protected DateTime sampleTime; // MM
        protected double? lastValue;
        protected Thread thread;
        protected MeasurementMode measurementMode;
        protected GateTime gateTime;
        protected double gateTimeValue;
        #endregion

        #region Properties
        public bool IsConnected => connected;
        public string Portname => portName;
        public List<Tuple<DateTime, double>> Data => data;
        public double? LastValue => lastValue;
        public DateTime SampleTime => sampleTime;
        public DateTime InitTime => initTime;
        public MeasurementMode MeasurementMode { get => measurementMode; set { measurementMode = value; } }
        public GateTime GateTime { get => gateTime; set { gateTime = value; } }
        public double DGateTime { get => gateTimeValue; set { gateTimeValue = value; } }
        #endregion

        protected Counter() : base("Frequency counter")
        {
            connected = false;
            stillInLoop = false;
            stopRequest = false;
            data = new List<Tuple<DateTime, double>>();
            loopSamples = 0;
            lastValue = null;
            initTime = DateTime.UtcNow;  // MM   
        }

        public abstract void Connect();

        public virtual void Connect(string port)
        {
            portName = port;
            Connect();
        }

        public abstract void Disconnect();

        public virtual void SetupMeasurementMode(MeasurementMode mm, GateTime gt)
        {
            measurementMode = mm;
            gateTime = gt;
            GateTimeToDouble();
        }

        protected void GateTimeToDouble()
        {
            switch (gateTime)
            {
                case GateTime.Gate01s:
                    gateTimeValue = 0.1;
                    break;
                case GateTime.Gate1s:
                    gateTimeValue = 1;
                    break;
                case GateTime.Gate10s:
                    gateTimeValue = 10;
                    break;
                default:
                    gateTimeValue = 0;
                    break;
            }
        }

        public abstract void SendCommand(string cmd);

        public abstract string ReadLine();

        protected abstract double? _GetCounterValue();

        public virtual double? GetCounterValue()
        {
            lastValue = _GetCounterValue();
            sampleTime = DateTime.UtcNow;
            if (lastValue == null) TimeOutEventHandler(this, new EventArgs());
            UpdatedEventHandler(this, new EventArgs());
            return lastValue;
        }

        protected virtual void StartMeasurementLoop()
        {
            if (stillInLoop) return;
            stillInLoop = true;
            int i = 0;
            Tuple<DateTime, double> dataPair;
            while (!stopRequest && i < loopSamples)
            {
                double? x = GetCounterValue();
                if (x != null)
                {
                    dataPair = new Tuple<DateTime, double>(sampleTime, (double)x);
                    data.Add(dataPair);
                    i++;
                }
            }
            stillInLoop = false;
            ReadyEventHandler(this, new EventArgs());
        }

        public virtual void StartMeasurementLoopThread(int n)
        {
            if (stillInLoop) return;
            loopSamples = n;
            stopRequest = false;
            data.Clear();
            thread = new Thread(new ThreadStart(StartMeasurementLoop));
            thread.Start();
        }

        public virtual void StartMeasurementLoopThread() => StartMeasurementLoopThread(Int32.MaxValue);

        public void RequestStopMeasurementLoop() => stopRequest = true;

        #region event declarations

        public delegate void CounterEventHandler(object obj, EventArgs e);

        public event CounterEventHandler UpdatedEventHandler;

        public event CounterEventHandler ReadyEventHandler;

        public event CounterEventHandler TimeOutEventHandler;

        #endregion
    }
}
