using PacketDotNet;
using SharpPcap;
using tfmAlert.Handlers;

namespace tfmAlert
{
    class Program
    {
        public enum TFMPacketType
        {
            Unknown,
            MapChange, // 0x05 0x02
            PlayerData, // 0x04 0x08 0x0A
        }

        public static Dictionary<TFMPacketType, Handler> PacketHandlers = new Dictionary<TFMPacketType, Handler>();

        static void Main(string[] args)
        {
            Audio.Cache("sham", "./sfx/sham.mp3");
            Audio.Cache("cheese", "./sfx/cheese.mp3");

            PacketHandlers.Add(TFMPacketType.MapChange, new MapChangeHandler());

            var devices = CaptureDeviceList.Instance;

            if (devices.Count < 1)
            {
                Console.WriteLine("No capture devices found.");
                return;
            }

            ILiveDevice device = null;
            for(int i = 0; i < devices.Count; ++i)
            {
                if (!devices[i].Description.Contains("loopback"))
                {
                    device = devices[i];
                    break;
                }
            }

            if (device == null)
            {
                Console.WriteLine("Unable to find a non loopback network device!");
                return;
            }

            Console.WriteLine($"Using device {device.Description}");

            var config = new DeviceConfiguration()
            {
                Mode = DeviceModes.Promiscuous,
                ReadTimeout = 1000,
            };
            device.Open(config);

            device.Filter = "tcp port 11801 or tcp port 12801 or tcp port 13801 or tcp port 14801 or tcp port 15801";

            device.OnPacketArrival += new PacketArrivalEventHandler(OnPacketArrival);

            device.StartCapture();

            Console.WriteLine("Press Enter to stop...");
            Console.ReadLine();

            device.StopCapture();
            device.Close();
        }

        private static void OnPacketArrival(object sender, PacketCapture e)
        {
            var packet = e.GetPacket();
            var parsedPacket = Packet.ParsePacket(packet.LinkLayerType, packet.Data);

            var tcpPacket = parsedPacket.Extract<TcpPacket>();
            if (tcpPacket != null)
            {
                var payloadData = tcpPacket.PayloadData;
                if (payloadData != null && payloadData.Length > 0)
                {
                    TFMPacketType packetType = TFMPacketType.Unknown;

                    int index = payloadData.FindPattern("?? ?? 0x05 0x02 0x00 ?? ?? ?? 0x00");
                    if (index != -1 && index <= 8) packetType = TFMPacketType.MapChange;

                    Handler handler;
                    if (PacketHandlers.TryGetValue(packetType, out handler))
                    {
#if DEBUG
                        Console.WriteLine();
                        PrintHexDump(payloadData);
                        Console.WriteLine();
#endif
                        handler?.Run(payloadData);
                    }
                }
            }
        }

        public static void PrintHexDump(byte[] data, int bytesPerLine = 16)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            for (int i = 0; i < data.Length; i += bytesPerLine)
            {
                Console.Write($"{i:X8}  ");

                for (int j = 0; j < bytesPerLine; j++)
                {
                    if (i + j < data.Length) Console.Write($"{data[i + j]:X2} ");
                    else Console.Write("   ");
                }

                Console.Write(" |");

                for (int j = 0; j < bytesPerLine && i + j < data.Length; j++)
                {
                    byte b = data[i + j];
                    if (b >= 32 && b <= 126) Console.Write((char)b);
                    else Console.Write('.');
                }

                Console.WriteLine("|");
            }
        }
    }
}
