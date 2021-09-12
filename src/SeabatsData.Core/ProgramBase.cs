using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace SeabatsData.Core
{
    public abstract class ProgramBase
    {
        static ProgramBase()
        {
            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();

            WriteHeading("Configuration");

            InputDir = configuration["SeabatsData:InputDir"];
            OutputDir = configuration["SeabatsData:OutputDir"];
            Icaos = configuration["SeabatsData:ICAOs"];

            Console.WriteLine($"Input Directory:  {InputDir}");
            Console.WriteLine($"Output Directory: {OutputDir}");
            Console.WriteLine($"ICAOs:            {Icaos}");

            if (!Directory.Exists(OutputDir))
                Directory.CreateDirectory(OutputDir);
        }

        protected static string InputDir { get; }
        protected static string OutputDir { get; }
        protected static string Icaos { get; }

        protected static void WriteHeading(string heading)
        {
            Console.WriteLine();
            Console.WriteLine(heading.ToUpper());
            Console.WriteLine();
        }

        protected static string GetAircraftNameByIcao(string icao)
        {
            // TODO move to where it belongs
            icao = icao.ToUpper();
            return icao switch
            {
                "406658" => "G-DMPP",
                "407637" => "G-WKTH",
                "40785B" => "G-WKTI",
                "43ED4B" => "2-WKTJ",
                "4D232D" => "HRON",
                _ => throw new InvalidOperationException($"Unknown ICAO: #{icao}")
            };
        }
    }
}
