using Rocket.RocketAPI;
using SDG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace unturned.ROCKS.Votifier
{
    class CommandReward : RocketCommand
    {

        public CommandReward(String name)
        {
            this.name = name;
        }

        public void Execute(SteamPlayerID caller, string command)
        {
            try
            {
                String result = new WebClient().DownloadString(String.Format("http://unturned-servers.net/api/?object=votes&element=claim&key={0}&steamid={1}", Votifier.Configuration.ServerKey, caller.SteamId.ToString()));
                if (result == "1")
                {
                    SteamPlayer steamPlayer = null;

                    SteamPlayerlist.tryGetSteamPlayer(caller.SteamId.ToString(), out steamPlayer);
                    string playerName = steamPlayer.Player.name;
                    ChatManager.say(String.Format("{0} voted for this server on unturned-servers.net and got a reward.", playerName));

                    bool success = true;

                    foreach (Reward reward in Votifier.Configuration.Rewards)
                    {
                        if (!ItemTool.tryForceGiveItem(steamPlayer.Player, reward.ItemId, reward.Amount))
                        {
                            success = false;
                        }
                    }

                    if (success)
                    {
                        new WebClient().DownloadString(String.Format("http://unturned-servers.net/api/?action=post&object=votes&element=claim&key={0}&steamid={1}", Votifier.Configuration.ServerKey, caller.SteamId.ToString()));
                    }
                    else
                    {
                        Logger.Log(String.Format("Failed giving a item to {0}", playerName));
                    }

                }
                else
                {
                    ChatManager.say(caller.SteamId, "You have not voted for this server today, please visit unturned-servers.net to do so.");
                }
            }
            catch (Exception e)
            {
                Logger.Log(e.ToString());
            }
        }

        public string Help
        {
            get { return "Vote for the server"; }
        }
        private string name;
        public string Name
        {
            get { return name; }
        }
    }
}
