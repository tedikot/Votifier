using Rocket.RocketAPI;
using SDG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace unturned.ROCKS.Votifier
{
    class CommandReward : Command
    {

        public CommandReward(String exec)
        {
            base.commandName = exec;
            base.commandInfo = "";
            base.commandHelp = "";
        }

        protected override void execute(SteamPlayerID m, string D)
        {
            try
            {
                String result = new WebClient().DownloadString(String.Format("http://unturned-servers.net/api/?object=votes&element=claim&key={0}&steamid={1}", Votifier.Configuration.ServerKey, m.SteamId.ToString()));
                if (result == "1")
                {
                    SteamPlayer steamPlayer = null;
                   
                    SteamPlayerlist.tryGetSteamPlayer(m.SteamId.ToString(), out steamPlayer);
                    string playerName = steamPlayer.Player.name;
                    ChatManager.say(String.Format("{0} voted for this server on unturned-servers.net and got a reward.", playerName));

                    bool success = true;

                    foreach (KeyValuePair<ushort,byte> item in Votifier.Configuration.Rewards)
                    {
                        if (!ItemTool.tryForceGiveItem(steamPlayer.Player, item.Key, item.Value))
                        {
                            success = false;
                        }
                    }

                    if (success)
                    {
                        new WebClient().DownloadString(String.Format("http://unturned-servers.net/api/?action=post&object=votes&element=claim&key={0}&steamid={1}", Votifier.Configuration.ServerKey, m.SteamId.ToString()));
                    }
                    else
                    {
                        Logger.Log(String.Format("Failed giving a item to {0}", playerName));
                    }

                }
                else { 
                    ChatManager.say(m.SteamId, "You have not voted for this server today, please visit unturned-servers.net to do so and come back to claim your reward!");
                }
                Logger.Log(result);
            }
            catch (Exception e)
            {
                Logger.Log(e.ToString());
            }
        }
        

    }
}
