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
        protected override void Load()
        {
            Events.OnPlayerConnected += Events_OnPlayerConnected;
        }

        void Events_OnPlayerConnected(Player player)
        {
            Vote(player.SteamChannel.SteamPlayer.SteamPlayerID.CSteamID,false);
        }


        public static void Vote(CSteamID caller,bool giveItemDirectly = true)
        {
            try
            {
                if (Votifier.Configuration.Services.Where(s => !String.IsNullOrEmpty(s.APIKey)).Count() == 0)
                {
                    Logger.Log("No apikeys supplied."); return;
                }

                List<Service> services = Votifier.Configuration.Services.Where(s => !String.IsNullOrEmpty(s.APIKey)).ToList();


                SteamPlayer voter = PlayerTool.getSteamPlayer(caller);

                bool hasVoted = false;
                foreach (Service service in services)
                {
                    ServiceDefinition apidefinition = Votifier.Configuration.ServiceDefinitions.Where(s => s.Name == service.Name).FirstOrDefault();
                    if (apidefinition == null) { Logger.Log("The API for " + service.Name + " is unknown"); return; }
                    string result = new VotifierWebclient().DownloadString(String.Format(apidefinition.CheckHasVoted, service.APIKey, caller.ToString()));
                    if (result == "1")
                    {
                        hasVoted = true;
                        if (giveItemDirectly)
                        {
                            foreach (Reward reward in Votifier.Configuration.Rewards)
                            {
                                if (!ItemTool.tryForceGiveItem(voter.Player, reward.ItemId, reward.Amount))
                                {
                                    Logger.Log("Failed giving a item to " + voter.SteamPlayerID.CharacterName + " (" + reward.ItemId + "," + reward.Amount + ")");
                                }
                            }
                            ChatManager.say(String.Format(voter.SteamPlayerID.CharacterName + " voted for this server on " + service.Name + " and got a reward."));
                            new VotifierWebclient().DownloadString(String.Format(apidefinition.ReportSuccess, service.APIKey, caller.ToString()));
                        }
                        else
                        {
                            ChatManager.say(caller, String.Format("You have voted this server on " + service.Name + "!"));
                            ChatManager.say(caller, String.Format("Type /reward to receive your reward."));
                        }
                    }
                }
                if (!hasVoted)
                {
                    ChatManager.say(caller, "To get a reward vote for this server on: " + String.Join(",", services.Select(s => s.Name).ToArray()));
                    ChatManager.say(caller, "Type /reward to receive the reward after you voted.");
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

    } 
   
}
