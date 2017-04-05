using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Game.ClientHandlerNet
{
    public class StdioHandler
    {
        private string _filePath;
        private Process _process;

        public StdioHandler(string path)
        {
            _filePath = path;
            Start();
        }

        private void Start()
        {
            var fullpath = Path.GetFullPath(_filePath);

            var startInfo = new ProcessStartInfo(_filePath)
            {
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
            };

            _process = Process.Start(startInfo);
        }

        private PlayerResp GetValue(PlayerReq req)
        {
            _process.StandardInput.WriteLine(JsonConvert.SerializeObject(req));

            var response = _process.StandardOutput.ReadLine();

            return JsonConvert.DeserializeObject<PlayerResp>(response);
        }

        public PlayerResp GetOutput(PlayerReq req)
        {
            if (_process.HasExited)
                Start();

            var result = GetValue(req);

            return result;
        }
    }
}
