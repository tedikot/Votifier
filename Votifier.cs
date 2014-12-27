using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Steamworks;
using SDG;
using System.Timers;
using System.IO;
using System.Reflection;
using Rocket.RocketAPI;
using System.Net;

namespace unturned.ROCKS.Votifier
{
    public class Votifier : RocketPlugin
    {
        public static Configuration Configuration;

        public void Load() 
        {
            try
            {
                Configuration = RocketConfiguration.LoadConfiguration<Configuration>();
                if (!Configuration.Enabled) return;
                Commands.RegisterCommand(new CommandReward("reward"));
                Commands.RegisterCommand(new CommandReward("vote"));
                SDG.Steam.serverConnected += checkVote;
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
            }
        }

        public void checkVote(CSteamID id)
        {
            String result = new WebClient().DownloadString(String.Format("http://unturned-servers.net/api/?object=votes&element=claim&key={0}&steamid={1}", Votifier.Configuration.ServerKey, id.ToString()));
            if (result == "1")
            {
                ChatManager.say(id, "You voted for the server, type /reward to receive your reward.");
            }
            else
            {
                ChatManager.say(id, "Vote for this server on unturned-servers.net and get a reward!");
                ChatManager.say(id, "Type /reward to receive the reward after you voted.");
            }
        }


        public string Author
        {
            get { return "fr34kyn01535"; }
        }

        public string Name
        {
            get { return Assembly.GetExecutingAssembly().GetName().Name.ToString(); }
        }

        public string Version
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version.ToString(); }
        }
    }
}
