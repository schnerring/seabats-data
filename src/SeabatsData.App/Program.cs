using System;
using Microsoft.Extensions.Configuration;

namespace SeabatsData.App
{
    class Program
    {
        public static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();

            var dataDir = configuration["SeabatsData:DataDir"];
            Console.WriteLine(dataDir);
        }
    }
}
