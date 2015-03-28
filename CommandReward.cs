using Rocket;
using Rocket.RocketAPI;
using SDG;
using System;
using System.Collections.Generic;
using System.Linq;

namespace unturned.ROCKS.Votifier
{
    class CommandReward : IRocketCommand
    {
        public void Execute(Steamworks.CSteamID caller, string command)
        {
            Votifier.Vote(caller);
        }

        public string Help
        {
            get { return "Get rewards for voting"; }
        }

        public string Name
        {
            get { return "reward"; }
        }

        public bool RunFromConsole
        {
            get { return false; }
        }
    }
}
