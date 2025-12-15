using BeagleLib.Engine;
using BeagleLib.Engine.FitFunc;
using BeagleLib.Util;
using Run.MLSetups;

namespace Run;

public class Program
{
    static void Main(string[] args)
    {
        #region Read Command-line parameters
        // ReSharper disable once RedundantAssignment
        var stopAfterMin = -1;
        var noEscMenu = false;
        foreach (var arg in args)
        {
            try
            {
                if (arg.ToLower() == "noescmenu")
                {
                    noEscMenu = true;
                }
                else if (arg.ToLower().StartsWith("stopaftermin"))
                {
                    var argParts = arg.Split('=');
                    if (argParts.Length != 2 && argParts[0] != "stopaftermin") throw new ArgumentException();
                    stopAfterMin = int.Parse(argParts[1]);
                    if (stopAfterMin <= 0) throw new ArgumentException();
                }
                else
                {
                    throw new ArgumentException();
                }
            }
            catch (Exception)
            {
                Output.WriteLine($"ERROR: Invalid command line parameter: {arg}");
                Output.WriteLine("Available Command Line Parameters (not case sensitive):");
                Output.WriteLine("NoEscMenu - directs Beagle to not watch keyboard. Useful for batch runs");
                Output.WriteLine("StopAfterMin={minutes} - directs Beagle to stop after number of minutes specified");
                return;
            }
        }
        #endregion

        #region Custom settings per machine (not used)
        //LG Gram
        if (Environment.MachineName == "LG-GRAM")
        {
            //This is supported only on Windows and Linux, so we are ok since we do not run on Mac
            #pragma warning disable CA1416
            //Process.GetCurrentProcess().ProcessorAffinity = new IntPtr(0b0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_1111_1111_1111); //MAX Performance
            #pragma warning restore CA1416
        }

        //Alienware
        if (Environment.MachineName == "NB20527-M29598") 
        {
            //This is supported only on Windows and Linux, so we are ok since we do not run on Mac
            #pragma warning disable CA1416
            //Process.GetCurrentProcess().ProcessorAffinity = new IntPtr(0b0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_1111_1111_1111); //MAX Performance
            #pragma warning restore CA1416
        }

        //Workstation
        if (Environment.MachineName == "NB12466-M29598")
        {
            //This is supported only on Windows and Linux, so we are ok since we do not run on Mac
            #pragma warning disable CA1416
            //Process.GetCurrentProcess().ProcessorAffinity = new IntPtr(0b0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_1111_1111_1111_1111); //MAX Performance
            #pragma warning restore CA1416
        }

        //Server
        if (Environment.MachineName == "KILIMANJARO")
        {
            //This is supported only on Windows and Linux, so we are ok since we do not run on Mac
            #pragma warning disable CA1416
            //Process.GetCurrentProcess().ProcessorAffinity = new IntPtr(0b0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_1111_1111_1111_1111); //MAX Performance
            #pragma warning restore CA1416
        }

        //H100 Server
        if (Environment.MachineName == "blackcomb")
        {
            //This is supported only on Windows and Linux, so we are ok since we do not run on Mac
            #pragma warning disable CA1416
            //Process.GetCurrentProcess().ProcessorAffinity = new IntPtr(0b0000_0000_1111_1111_1111_1111_1111_1111_1111_1111_1111_1111_1111_1111_1111_1111); //MAX Performance
            #pragma warning restore CA1416
        }
        #endregion

        #region Benchmarking
        //stopAfterMin = 20;
        //using var mlBenchmarkEngine = new MLEngine<SinApproximation, StdCubeFitFunc>(forceCPUAccelerator: false);
        //mlBenchmarkEngine.Train(stopAfterMin, noEscMenu);
        //return;

        //using var mlBenchmarkEngine = new MLEngine<Benchmark, StdFitFunc>(forceCPUAccelerator: false);
        //mlBenchmarkEngine.Train(stopAfterMin, noEscMenu, benchmarkRun: true);
        //return;
        #endregion

        //new CsvGen<RydbergFormula>().CreateAndSaveCsvFile(5000); return;

        //using var mlEngine = new MLEngine<QuadraticEq, CorrelationFitFunc>(forceCPUAccelerator: false);
        //using var mlEngine = new MLEngine<AreaOfCircle, CorrelationFitFunc>(forceCPUAccelerator: false);
        //using var mlEngine = new MLEngine<DemoForMSU, CorrelationFitFunc>(forceCPUAccelerator: false);
        //using var mlEngine = new MLEngine<DemoForMSU2, CorrelationFitFunc>(forceCPUAccelerator: false);
        using var mlEngine = new MLEngine<DemoForMSU3, StdFitFunc>(forceCPUAccelerator: false);
        //using var mlEngine = new MLEngine<DemoForMSU4, CorrelationFitFunc>(forceCPUAccelerator: false);
        //using var mlEngine = new MLEngine<AreaOfCircle, CorrelationFitFunc>(forceCPUAccelerator: false);
        //using var mlEngine = new MLEngine<XPowY, StdFitFunc>(forceCPUAccelerator: false);
        //using var mlEngine = new MLEngine<AvgOf2, StdFitFunc>(forceCPUAccelerator: false);
        //using var mlEngine = new MLEngine<QuadraticEqNormalized, StdFitFunc>(forceCPUAccelerator: false);
        //using var mlEngine = new MLEngine<QuadraticEq, StdFitFunc>(forceCPUAccelerator: false);
        //using var mlEngine = new MLEngine<DepressedCubicEq, StdFitFunc>(forceCPUAccelerator: false);
        //using var mlEngine = new MLEngine<SinApproximation, StdCubeFitFunc>(forceCPUAccelerator: false);
        //using var mlEngine = new MLEngine<DemoForJdAndTheBoys, StdFitFunc>(forceCPUAccelerator: false);
        //using var mlEngine = new MLEngine<RydbergFormula, CorrelationFitFunc>(forceCPUAccelerator: false);
        //using var mlEngine = new MLEngine<ThrustData, StdFitFunc>(forceCPUAccelerator: false);

        mlEngine.Train(stopAfterMin, noEscMenu);
    }
}