using Rocket;
using Rocket.Unturned;
using Rocket.Unturned.Events;
using Rocket.Unturned.Logging;
using Rocket.Unturned.Player;
using Rocket.Unturned.Plugins;
using SDG;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace unturned.ROCKS.Votifier
{
    public class Votifier : RocketPlugin<VotifierConfiguration>
    {
        public static Votifier Instance;
        protected override void Load()
        {
            Instance = this;
            RocketServerEvents.OnPlayerConnected += Events_OnPlayerConnected;
            if (Configuration.EnableRewardBundles)
            {
                OnPlayerVoted += Votifier_OnPlayerVoted;
            }
        }

        void Votifier_OnPlayerVoted(RocketPlayer player, ServiceDefinition definition)
        {
            int propabilysum = Instance.Configuration.RewardBundles.Sum(p => p.Probability);

            RewardBundle bundle = new RewardBundle();

            if (propabilysum != 0)
            {
                Random r = new Random();

                int i = 0, diceRoll = r.Next(0, propabilysum);

                foreach (RewardBundle b in Instance.Configuration.RewardBundles)
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
                Logger.Log(Instance.Translate("no_rewards_found"));
                return;
            }

            foreach (Reward reward in bundle.Rewards)
            {
                
                if (!player.GiveItem(reward.ItemId, reward.Amount))
                {
                    Logger.Log(Instance.Translate("vote_give_error_message", player.CharacterName, reward.ItemId, reward.Amount));
                }
            }
            RocketChat.Say(Instance.Translate("vote_success_message", player.CharacterName, definition.Name, bundle.Name));        
        }

        public delegate void PlayerVotedEvent(RocketPlayer player, ServiceDefinition definition);
        public event PlayerVotedEvent OnPlayerVoted;

        public override Dictionary<string, string> DefaultTranslations
        {
            get
            {
                return new Dictionary<string, string>() { 
                    {"no_apikeys_message","No apikeys supplied."},
                    {"api_unknown_message", "The API for {0} is unknown"},
                    {"api_down_message","Can't reach {0}, is it down?!"},
                    {"not_yet_voted","You have not yet voted for this server on: {0}"},
                    {"no_rewards_found","Failed finding any rewardbundles"},
                    {"vote_give_error_message","Failed giving a item to {0} ({1},{2})"},
                    {"vote_success_message","{0} voted on {1} and received the \"{2}\" bundle"},
                    {"vote_pending_message","You have an outstanding reward for your vote on {0}"},
                    {"vote_due_message","You have already voted for this server on {0}, Thanks!"}
                };
            }
        }

        void Events_OnPlayerConnected(RocketPlayer player)
        {
            Vote(player.CSteamID,false);
        }

        public static void Vote(CSteamID caller,bool giveItemDirectly = true)
        {
            try
            {
                if (Instance.Configuration.Services.Where(s => !String.IsNullOrEmpty(s.APIKey)).FirstOrDefault() == null)
                {
                    Logger.Log(Instance.Translate("no_apikeys_message")); return;
                }

                List<Service> services = Instance.Configuration.Services.Where(s => !String.IsNullOrEmpty(s.APIKey)).ToList();


                foreach (Service service in services)
                {
                    ServiceDefinition apidefinition = Instance.Configuration.ServiceDefinitions.Where(s => s.Name == service.Name).FirstOrDefault();
                    if (apidefinition == null) { Logger.Log(Instance.Translate("api_unknown_message", service.Name)); return; }
                    try
                    {
                        VotifierWebclient wc = new VotifierWebclient();
                        wc.DownloadStringCompleted += (sender, e) => wc_DownloadStringCompleted(e, caller, service,apidefinition, giveItemDirectly);
                        wc.DownloadStringAsync(new Uri(String.Format(apidefinition.CheckHasVoted, service.APIKey, caller.ToString())));
                    }
                    catch (TimeoutException)
                    {
                        Logger.Log(Instance.Translate("api_down_message", service.Name));
                        RocketChat.Say(caller, Instance.Translate("api_down_message", service.Name));
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
          
            EnqueueTask(() =>
            {
                handleVote(v);
            });
        }

        static void handleVote(VoteResult result) {

            SteamPlayer voter = PlayerTool.getSteamPlayer(result.caller);

#if DEBUG
            Console.WriteLine("Webserver returns: " +result.result);
#endif

            switch (result.result)
            {
                case "0":
                    RocketChat.Say(result.caller, Instance.Translate("not_yet_voted", result.service.Name));
                    break;
                case "1":
                    if (result.giveItemDirectly)
                    {
                        if (Instance.OnPlayerVoted != null) Instance.OnPlayerVoted(RocketPlayer.FromCSteamID(result.caller), result.apidefinition);

                        new VotifierWebclient().DownloadStringAsync(new Uri(String.Format(result.apidefinition.ReportSuccess, result.service.APIKey, result.caller.ToString())));
                        return;
                    }
                    else
                    {
                        RocketChat.Say(result.caller, Instance.Translate("vote_pending_message", result.service.Name));
                        return;
                    }
                case "2":
                    RocketChat.Say(result.caller, Instance.Translate("vote_due_message", result.service.Name));
                    break;
            }
        }


    } 
   
}
