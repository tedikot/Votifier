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
using System.Collections;
using System.Diagnostics;
using Rocket.RocketAPI.Interfaces;
using Rocket.RocketAPI.Managers;

namespace unturned.ROCKS.Votifier
{
    public class Votifier : RocketPlugin
    {
        public static Configuration Configuration;

        public void Load() 
        {
            try
            {
                Configuration = ConfigurationManager.LoadConfiguration<Configuration>();
                if (!String.IsNullOrEmpty(Configuration.UnturnedServers) || !String.IsNullOrEmpty(Configuration.UnturnedSL))
                {
                    RocketAPI.Commands.RegisterCommand(new CommandReward("reward"));
                    RocketAPI.Commands.RegisterCommand(new CommandReward("vote"));
                    RocketAPI.Events.PlayerConnected += checkVote;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
            }
        }

        public void checkVote(CSteamID id)
        {
            try
            {
                string unturnedServer = String.IsNullOrEmpty(Votifier.Configuration.UnturnedServers) ? "" : new MyWebClient().DownloadString(String.Format("http://unturned-servers.net/api/?object=votes&element=claim&key={0}&steamid={1}", Votifier.Configuration.UnturnedServers, id.ToString()));
                string unturnedSL = String.IsNullOrEmpty(Votifier.Configuration.UnturnedSL) ? "" : new MyWebClient().DownloadString(String.Format("http://unturnedsl.com/api/dedicated/{0}/{1}", Votifier.Configuration.UnturnedSL, id.ToString()));


                string servernames = "";
                if (!String.IsNullOrEmpty(unturnedServer)) servernames += "unturned-servers.net";
                if (!String.IsNullOrEmpty(unturnedServer) && !String.IsNullOrEmpty(unturnedSL)) servernames += " or ";
                if (!String.IsNullOrEmpty(unturnedSL)) servernames += "unturnedsl.com";

                if (unturnedServer == "1" || unturnedSL == "1")
                {
                    ChatManager.say(id, "You voted for the server, type /reward to receive your reward.");
                }
                else
                {
                    ChatManager.say(id, "Vote for this server on " + servernames + " and get a reward!");
                    ChatManager.say(id, "Type /reward to receive the reward after you voted.");
                }
            }
            catch (Exception)
            {
                Logger.Log("Votifier connection timed out!");
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
