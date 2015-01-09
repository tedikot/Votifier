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
                if (Votifier.Configuration.Services.Where(s => !String.IsNullOrEmpty(s.APIKey)).FirstOrDefault() == null)
                {
                    Logger.Log("No apikeys supplied."); return;
                }

                List<Service> services = Votifier.Configuration.Services.Where(s => !String.IsNullOrEmpty(s.APIKey)).ToList();


                SteamPlayer voter = PlayerTool.getSteamPlayer(caller);

                List<string> alreadyVoted = new List<string>();
                List<string> notVoted = new List<string>();

                foreach (Service service in services)
                {
                    ServiceDefinition apidefinition = Votifier.Configuration.ServiceDefinitions.Where(s => s.Name == service.Name).FirstOrDefault();
                    if (apidefinition == null) { Logger.Log("The API for " + service.Name + " is unknown"); return; }
                    string result = new VotifierWebclient().DownloadString(String.Format(apidefinition.CheckHasVoted, service.APIKey, caller.ToString()));
                    switch (result)
                    {
                        case "0":
                            notVoted.Add(service.Name);
                            break;
                        case "1":
                            if (giveItemDirectly)
                            {
                                int propabilysum =  Votifier.Configuration.RewardBundles.Sum(p => p.Probability);

                                RewardBundle bundle = new RewardBundle();

                                if (propabilysum != 0)
                                {
                                    Random r = new Random();

                                    int i = 0, diceRoll = r.Next(0, propabilysum);
                                    
                                    foreach (RewardBundle b in Votifier.Configuration.RewardBundles)
                                    {
                                        if (diceRoll > i && diceRoll <= i + b.Probability)
                                        {
                                            bundle = b;
                                            break;
                                        }
                                        i = i + b.Probability;
                                    }
                                }
                                else {
                                    Logger.Log("Failed finding any rewardbundles");
                                    return;
                                }

                                foreach (Reward reward in bundle.Rewards)
                                {
                                    if (!ItemTool.tryForceGiveItem(voter.Player, reward.ItemId, reward.Amount))
                                    {
                                        Logger.Log("Failed giving a item to " + voter.SteamPlayerID.CharacterName + " (" + reward.ItemId + "," + reward.Amount + ")");
                                    }
                                }
                                RocketChatManager.Say(voter.SteamPlayerID.CharacterName + " voted on " + service.Name + " and received the \"" + bundle.Name + "\" bundle");
                                new VotifierWebclient().DownloadString(String.Format(apidefinition.ReportSuccess, service.APIKey, caller.ToString()));
                                return;
                            }
                            else
                            {
                                RocketChatManager.Say(caller, String.Format("You have voted this server on " + service.Name));
                                RocketChatManager.Say(caller, String.Format("Type /reward to receive your reward."));
                                return;
                            }
                        case "2":
                            alreadyVoted.Add(service.Name);
                            break;
                    }
                }

                if (alreadyVoted.Count == 0 && notVoted.Count != 0)
                {
                    RocketChatManager.Say(caller, "To get a reward, vote for this server on: " + String.Join(", ", notVoted.ToArray()));
                    RocketChatManager.Say(caller, "Type /reward to receive the reward after you voted.");
                }
                else if (alreadyVoted.Count != 0 && notVoted.Count != 0)
                {
                    RocketChatManager.Say(caller, "You can still vote for this server on: " + String.Join(", ", notVoted.ToArray()));
                    RocketChatManager.Say(caller, "Type /reward to receive the reward after you voted.");
                }
                else {
                    RocketChatManager.Say(caller, "Thank you for voting, try again tomorrow.");
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

    } 
   
}
