using Rocket;
using Rocket.RocketAPI;
using Rocket.RocketAPI.Managers;
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
            RocketEvents.OnPlayerConnected += Events_OnPlayerConnected;
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


                foreach (Service service in services)
                {
                    ServiceDefinition apidefinition = Votifier.Configuration.ServiceDefinitions.Where(s => s.Name == service.Name).FirstOrDefault();
                    if (apidefinition == null) { Logger.Log("The API for " + service.Name + " is unknown"); return; }
                    try
                    {
                        VotifierWebclient wc = new VotifierWebclient();
                        wc.DownloadStringCompleted += (sender, e) => wc_DownloadStringCompleted(e, caller, service,apidefinition, giveItemDirectly);
                        wc.DownloadStringAsync(new Uri(String.Format(apidefinition.CheckHasVoted, service.APIKey, caller.ToString())));
                    }
                    catch (TimeoutException)
                    {
                        Logger.Log("Can't reach " + service.Name + ", is it down?!");
                        RocketChatManager.Say(caller,"Can't reach " + service.Name + ", please try again later...");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        private static List<VoteResult> voteResult = new List<VoteResult>();

        class VoteResult{
            public CSteamID caller;
            public Service service;
            public ServiceDefinition apidefinition;
            public bool giveItemDirectly;
            public string result;
        }

        static void wc_DownloadStringCompleted(System.Net.DownloadStringCompletedEventArgs e, CSteamID _caller, Service _service,ServiceDefinition _apidefinition, bool _giveItemDirectly)
        {
            VoteResult v = new VoteResult() { caller = _caller, result = e.Result, apidefinition = _apidefinition, service = _service, giveItemDirectly = _giveItemDirectly };
          
            RocketTaskManager.Enqueue(() =>
            {
                handleVote(v);
            });
        }

        static void handleVote(VoteResult result) {

            SteamPlayer voter = PlayerTool.getSteamPlayer(result.caller);

            switch (result.result)
            {
                case "0":
                    RocketChatManager.Say(result.caller, "You have not yet voted for this server on: " + result.service.Name);
                    break;
                case "1":
                    if (result.giveItemDirectly)
                    {
                        int propabilysum = Votifier.Configuration.RewardBundles.Sum(p => p.Probability);

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
                        else
                        {
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
                        RocketChatManager.Say(voter.SteamPlayerID.CharacterName + " voted on " + result.service.Name + " and received the \"" + bundle.Name + "\" bundle");
                        new VotifierWebclient().DownloadStringAsync(new Uri(String.Format(result.apidefinition.ReportSuccess, result.service.APIKey, result.caller.ToString())));
                        return;
                    }
                    else
                    {
                        RocketChatManager.Say(result.caller, String.Format("You have an outstanding reward for your vote on " + result.service.Name));
                        return;
                    }
                case "2":
                    RocketChatManager.Say(result.caller, String.Format("You have already voted for this server on " + result.service.Name + ", Thanks!"));
                    break;
            }
        }


    } 
   
}
