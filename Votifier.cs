using Rocket;
using Rocket.RocketAPI;
using SDG;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace unturned.ROCKS.Votifier
{
    public class Votifier : RocketPlugin<VotifierConfiguration>
    {
        protected override void Load() {
            Events.OnPlayerConnected += Events_OnPlayerConnected;
        }

        void Events_OnPlayerConnected(Player player)
        {
            if (Configuration.Services.Where(s => !String.IsNullOrEmpty(s.APIKey)).Count() != 0)
            {
                checkVote(player.SteamChannel.SteamPlayer.SteamPlayerID.CSteamID);
            }
        }


        public void checkVote(CSteamID id)
        {
            try
            {
                List<Service> services = Configuration.Services.Where(s => !String.IsNullOrEmpty(s.APIKey)).ToList();

                bool hasVoted = false;
                foreach (Service service in services)
                {
                    ServiceDefinition apidefinition = Votifier.Configuration.ServiceDefinitions.Where(s => s.Name == service.Name).FirstOrDefault();
                    if (apidefinition == null) { Logger.Log("The API for " + service.Name + " is unknown"); return; }
                    string result = new VotifierWebclient().DownloadString(String.Format(apidefinition.CheckHasVoted, service.APIKey, id.ToString()));
                    if (result == "1") hasVoted = true;
                }


                if (hasVoted)
                {
                    ChatManager.say(id, "You voted for the server, type /reward to receive your reward.");
                }
                else
                {
                    ChatManager.say(id, "To get a reward vote for this server on:" + String.Join(",", services.Select(s => s.Name).ToArray()));
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
