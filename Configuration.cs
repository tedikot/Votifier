using Rocket.RocketAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace unturned.ROCKS.Votifier
{
    public class Configuration : RocketConfiguration
    {
        public string ServerKey = "";
        public Dictionary<ushort, byte> Rewards = new Dictionary<ushort, byte>() { {18,1} };
    }
}
