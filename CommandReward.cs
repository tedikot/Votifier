using Rocket.RocketAPI;
using Rocket.RocketAPI.Interfaces;
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
                string unturnedServers = String.IsNullOrEmpty(Votifier.Configuration.UnturnedServers) ? "" : new MyWebClient().DownloadString(String.Format("http://unturned-servers.net/api/?object=votes&element=claim&key={0}&steamid={1}", Votifier.Configuration.UnturnedServers, caller.CSteamId.ToString()));
                string unturnedSL = String.IsNullOrEmpty(Votifier.Configuration.UnturnedSL) ? "" : new MyWebClient().DownloadString(String.Format("http://unturnedsl.com/api/dedicated/{0}/{1}", Votifier.Configuration.UnturnedSL, caller.CSteamId.ToString()));

                string servernames = "";
                if (!String.IsNullOrEmpty(unturnedServers)) servernames+= "unturned-servers.net";
                if (!String.IsNullOrEmpty(unturnedServers) && !String.IsNullOrEmpty(unturnedSL)) servernames += " or ";
                if (!String.IsNullOrEmpty(unturnedSL)) servernames += "unturnedsl.com";

                
                if (unturnedServers == "1" || unturnedSL == "1")
                {
                    SteamPlayer steamPlayer = null;

                    SteamPlayerlist.tryGetSteamPlayer(caller.CSteamId.ToString(), out steamPlayer);
                    string playerName = steamPlayer.Player.name;
                    ChatManager.say(String.Format("{0} voted for this server on " + servernames + " and got a reward.", playerName));

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
                        if (!String.IsNullOrEmpty(unturnedServers)) new MyWebClient().DownloadString(String.Format("http://unturned-servers.net/api/?action=post&object=votes&element=claim&key={0}&steamid={1}", Votifier.Configuration.UnturnedServers, caller.CSteamId.ToString()));
                        if (!String.IsNullOrEmpty(unturnedSL)) new MyWebClient().DownloadString(String.Format("http://unturnedsl.com/api/dedicated/post/{0}/{1}", Votifier.Configuration.UnturnedSL, caller.CSteamId.ToString()));
                    }
                    else
                    {
                        Logger.Log(String.Format("Failed giving a item to {0}", playerName));
                    }

                }
                else
                {
                    ChatManager.say(caller.CSteamId, "You have not voted for this server today, please visit " + servernames + " to do so.");
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
