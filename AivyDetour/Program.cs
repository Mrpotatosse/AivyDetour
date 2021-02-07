using EasyHook;
using SocketHook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;

namespace AivyDetour
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                bool onlyInject = int.TryParse(args[0], out int processIdReq);
                string processPathReq = onlyInject ? string.Empty : args[0];
                int redirectionPortReq = int.Parse(args[1]);

                HookElement hook = new HookElement();
                {
                    hook.IpcServer = RemoteHooking.IpcCreateServer<HookInterface>(ref hook.ChannelName, WellKnownObjectMode.Singleton);
                }

                if (onlyInject)
                {
                    RemoteHooking.Inject(
                        processIdReq,
                        "./SocketHook.dll",
                        "./SocketHook.dll",
                        hook.ChannelName,
                        redirectionPortReq
                    );
                }
                else
                {
                    RemoteHooking.CreateAndInject(
                        processPathReq,
                        string.Empty,
                        0x00000004,
                        InjectionOptions.DoNotRequireStrongName,
                        "./SocketHook.dll",
                        "./SocketHook.dll",
                        out hook.ProcessId,
                        hook.ChannelName,
                        redirectionPortReq
                    );
                }

                Console.Read();
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
