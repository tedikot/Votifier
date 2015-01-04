using Rocket;
using SDG;
using Steamworks;
using System;

namespace unturned.ROCKS.Votifier
{
    public class Votifier : RocketComponent
    {
        public static VotifierConfiguration configuration;

        protected override void Load() 
        {
            try
            {
                configuration = Configuration.LoadConfiguration<VotifierConfiguration>();
                
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
            }
        }

        protected override void onPlayerConnected(CSteamID cSteamID)
        {
            if (!String.IsNullOrEmpty(configuration.UnturnedServers) || !String.IsNullOrEmpty(configuration.UnturnedSL))
            {
                checkVote(cSteamID);
            }
        }

        public void checkVote(CSteamID id)
        {
            try
            {
                string unturnedServer = String.IsNullOrEmpty(Votifier.configuration.UnturnedServers) ? "" : new MyWebClient().DownloadString(String.Format("http://unturned-servers.net/api/?object=votes&element=claim&key={0}&steamid={1}", Votifier.configuration.UnturnedServers, id.ToString()));
                string unturnedSL = String.IsNullOrEmpty(Votifier.configuration.UnturnedSL) ? "" : new MyWebClient().DownloadString(String.Format("http://unturnedsl.com/api/dedicated/{0}/{1}", Votifier.configuration.UnturnedSL, id.ToString()));


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
    }
}
