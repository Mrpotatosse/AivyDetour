using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SocketHook
{
    public class HookInterface : MarshalByRefObject
    {
        public virtual void DetourInstalled(string processName) => Console.WriteLine($"Detour {processName}");
        public virtual void Message(string message) => Console.WriteLine(message);
        public virtual void Error(Exception error) => Console.WriteLine(error);
        public virtual void OnDetour(IPEndPoint baseRemoteIp, int processId) => Console.WriteLine($"{processId}|{baseRemoteIp}");

        public virtual void Ping() => Console.WriteLine($"Ping");
    }
}
