using EasyHook;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static EasyHook.RemoteHooking;
using static SocketHook.NativeSocketMethods;

namespace SocketHook
{
    public class Main : IEntryPoint
    {
        private HookInterface _interface;
        private LocalHook _connectHook;
        private ushort _redirectionPort;

        public Main(IContext context, string channelName, int redirectionPort)
        {
            _interface = IpcConnectClient<HookInterface>(channelName);
            _redirectionPort = (ushort)redirectionPort;
        }

        public void Run(IContext context, string channelName, int redirectionPort)
        {
            var process = Process.GetCurrentProcess();
            _interface.DetourInstalled(process.ProcessName, process.Id);

            try
            {
                _connectHook = LocalHook.Create(
                    LocalHook.GetProcAddress("Ws2_32.dll", "connect"),
                    new WinsockConnectDelegate(_onConnect), this);

                _connectHook.ThreadACL.SetExclusiveACL(new[] { 0 });
            }
            catch(Exception e)
            {
                _interface.Error(e);
            }

            WakeUpProcess();
            while (true) Thread.Sleep(1000);
        }

        private int _onConnect(IntPtr socket, IntPtr address, int addrSize)
        {
            var structure = Marshal.PtrToStructure<sockaddr_in>(address);
            var ipAddress = new IPAddress(structure.sin_addr.S_addr);
            var port = structure.sin_port;

            if (_isLocalIpAddress(ipAddress.ToString())) return connect(socket, address, addrSize);

            _interface.OnDetour(new IPEndPoint(ipAddress, htons(port)), Process.GetCurrentProcess().Id);

            var strucPtr = Marshal.AllocHGlobal(addrSize);
            var struc = new sockaddr_in
            {
                sin_addr = { S_addr = inet_addr("127.0.0.1") },
                sin_port = htons(_redirectionPort),
                sin_family = (short)AddressFamily.InterNetworkv4,
            };

            Marshal.StructureToPtr(struc, strucPtr, true);
            return connect(socket, strucPtr, addrSize);
        }

        private bool _isLocalIpAddress(string host)
        {
            try
            {
                // get host IP addresses
                IPAddress[] hostIPs = Dns.GetHostAddresses(host);
                // get local IP addresses
                IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());

                // test if any host IP equals to any local IP or to localhost
                foreach (IPAddress hostIP in hostIPs)
                {
                    // is localhost
                    if (IPAddress.IsLoopback(hostIP)) return true;
                    // is local address
                    foreach (IPAddress localIP in localIPs)
                    {
                        if (hostIP.Equals(localIP)) return true;
                    }
                }
            }
            catch { }
            return false;
        }
    }
}
