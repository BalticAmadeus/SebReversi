using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.ClientHandlerNet
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1 || args.Length > 2)
            {
                Console.WriteLine("USAGE: Game.ClientHandlerNet profile.json [clientName]");
                return;
            }
            var profile = ReadProfile(args[0]);
            if (args.Length >= 2)
                profile.ClientName = args[1];
            var loop = new ClientLoop(profile);
            loop.Connect();
            loop.DoLoop();
        }

        static Profile ReadProfile(string fileName)
        {
            string profileData = File.ReadAllText(fileName);
            var profile = JsonConvert.DeserializeObject<Profile>(profileData);
            profile.Validate();
            return profile;
        }
    }
}
