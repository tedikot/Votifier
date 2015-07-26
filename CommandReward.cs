using Rocket.API;
using Rocket.Unturned.Commands;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Linq;

namespace unturned.ROCKS.Votifier
{
    public class CommandReward : IRocketCommand
    {
        public void Execute(IRocketPlayer caller, params string[] command)
        {
            Votifier.Vote((UnturnedPlayer)caller);
        }

        public string Help
        {
            get { return "Get rewards for voting"; }
        }

        public string Name
        {
            get { return "reward"; }
        }

        public string Syntax
        {
            get { return ""; }
        }

        public bool AllowFromConsole
        {
            get { return false; }
        }

        public List<string> Aliases {
            get { return new List<string>() { "vote" }; }
        }
        
        public List<string> Permissions
        {
            get
            {
                return new List<string>() { "votifier.reward" };
            }
        }
    }
}
