using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tfmAlert;

namespace tfmAlert.Handlers
{
    internal class MapChangeHandler : Handler
    {
        public List<int> Codes = new List<int>()
        {
            2019, //Farm Event Map
        };

        public override async Task Run(byte[] packet)
        {
            int index = packet.FindPattern("?? ?? 0x05 0x02 0x00 ?? ?? ?? 0x00");
            int code = packet[index + 5] << 16 | packet[index + 6] << 8 | packet[index + 7];
            if (Codes.Contains(code))
            {
                Audio.Play("cheese");
            }
        }
    }
}
