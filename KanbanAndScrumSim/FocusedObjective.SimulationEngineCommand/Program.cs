using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RJH.CommandLineHelper;

namespace FocusedObjective.SimulationEngineCommand
{
    class Program
    {
        [STAThread]
        static int Main(string[] args)
        {
            Application app = new Application();
            return app.Run();
        }

        public class Application
        {
            private string _output = "";

            [CommandLineSwitch("Input", "The input SimML file.")]
            [CommandLineAlias("i")]
            public string Input
            {
                get;
                set;
            }

            [CommandLineSwitch("Output", "The output file name and path to save results to.")]
            [CommandLineAlias("o")]
            public string Output
            {
                get { return _output; }
                set { _output = value; }
            }

            public int Run()
            {
                Parser parser = new Parser(System.Environment.CommandLine, this);
                if (parser.Parse())
                {
                    if (System.IO.File.Exists(this.Input))
                    {
                        // do the simulation
                        Console.WriteLine("Starting simulation.");

                        string s = System.IO.File.ReadAllText(this.Input); 
                        
                        FocusedObjective.Simulation.Simulator sim = new Simulation.Simulator(s);
                        if (sim.Execute())
                        {
                            Console.WriteLine("Simulation complete. Writing results.");

                            if (string.IsNullOrEmpty(this.Output))
                            {
                                Console.WriteLine(sim.Result.ToString());
                            }
                            else
                            {
                                try
                                {
                                    sim.Result.Save(this.Output);
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("Failed to save the results to '{0}'. Error message: {1}", this.Output, e.Message);
                                    return -1;
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("Simulation failed to complete within the interval limit defined.");
                            return -2;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Input file '{0}' could not be found. Check the file exists at this location and that you have access rights to read it.", this.Input);
                        return -1;
                    }
                }
                else
                {
                    Console.WriteLine("Failed to parse input arguments. Valid arguments are 'Input' and 'Output'.");
                    return -1;
                }

                return 0;
            }
        }
    }
}
