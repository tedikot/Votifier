using Rocket.API;
using Rocket.API.Collections;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace fr34kyn01535.Votifier
{
    public class Votifier : RocketPlugin<VotifierConfiguration>
    {
        public static Votifier Instance;
        protected override void Load()
        {
            Instance = this;
            U.Events.OnPlayerConnected += voteTrigger;
            if (Configuration.Instance.EnableRewardBundles)
            {
                OnPlayerVoted += Votifier_OnPlayerVoted;
            }
        }
        
        private void voteTrigger(UnturnedPlayer player){
                Vote(player, false);
        }
        
        protected override void Unload()
        {
            U.Events.OnPlayerConnected -= voteTrigger;
            if (Configuration.Instance.EnableRewardBundles)
            {
                OnPlayerVoted -= Votifier_OnPlayerVoted;
            }
        }

        void Votifier_OnPlayerVoted(UnturnedPlayer player, ServiceDefinition definition)
        {
            int propabilysum = Instance.Configuration.Instance.RewardBundles.Sum(p => p.Probability);

            RewardBundle bundle = new RewardBundle();

            if (propabilysum != 0)
            {
                Random r = new Random();

                int i = 0, diceRoll = r.Next(0, propabilysum);

                foreach (RewardBundle b in Instance.Configuration.Instance.RewardBundles)
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
                Logger.Log(Instance.Translations.Instance.Translate("no_rewards_found"));
                return;
            }

            foreach (Reward reward in bundle.Rewards)
            {
                
                if (!player.GiveItem(reward.ItemId, reward.Amount))
                {
                    Logger.Log(Instance.Translations.Instance.Translate("vote_give_error_message", player.CharacterName, reward.ItemId, reward.Amount));
                }
            }
            UnturnedChat.Say(Instance.Translations.Instance.Translate("vote_success_message", player.CharacterName, definition.Name, bundle.Name));        
        }

        public delegate void PlayerVotedEvent(UnturnedPlayer player, ServiceDefinition definition);
        public event PlayerVotedEvent OnPlayerVoted;

        public override TranslationList DefaultTranslations
        {
            get
            {
                return new TranslationList() { 
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
        
        public static void Vote(UnturnedPlayer caller,bool giveItemDirectly = true)
        {
            try
            {
                if (Instance.Configuration.Instance.Services.Where(s => !String.IsNullOrEmpty(s.APIKey)).FirstOrDefault() == null)
                {
                    Logger.Log(Instance.Translations.Instance.Translate("no_apikeys_message")); return;
                }

                List<Service> services = Instance.Configuration.Instance.Services.Where(s => !String.IsNullOrEmpty(s.APIKey)).ToList();


                foreach (Service service in services)
                {
                    ServiceDefinition apidefinition = Instance.Configuration.Instance.ServiceDefinitions.Where(s => s.Name == service.Name).FirstOrDefault();
                    if (apidefinition == null) { Logger.Log(Instance.Translations.Instance.Translate("api_unknown_message", service.Name)); return; }
                    try
                    {
                        VotifierWebclient wc = new VotifierWebclient();
                        wc.DownloadStringCompleted += (sender, e) => wc_DownloadStringCompleted(e, caller, service,apidefinition, giveItemDirectly);
                        wc.DownloadStringAsync(new Uri(String.Format(apidefinition.CheckHasVoted, service.APIKey, caller.ToString())));
                    }
                    catch (TimeoutException)
                    {
                        Logger.Log(Instance.Translations.Instance.Translate("api_down_message", service.Name));
                        UnturnedChat.Say(caller, Instance.Translations.Instance.Translate("api_down_message", service.Name));
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
            public UnturnedPlayer caller;
            public Service service;
            public ServiceDefinition apidefinition;
            public bool giveItemDirectly;
            public string result;
        }

        static void wc_DownloadStringCompleted(System.Net.DownloadStringCompletedEventArgs e, UnturnedPlayer _caller, Service _service,ServiceDefinition _apidefinition, bool _giveItemDirectly)
        {
            VoteResult v = new VoteResult() { caller = _caller, result = e.Result, apidefinition = _apidefinition, service = _service, giveItemDirectly = _giveItemDirectly };
            lock (queue)
            {
                queue.Enqueue(v);
            }
        }

        private static Queue<VoteResult> queue = new Queue<VoteResult>();
        private void FixedUpdate()
        {
            if(queue.Count > 0)
            {
                VoteResult v = queue.Dequeue();
                handleVote(v);
            }
        }

        static void handleVote(VoteResult result) {
            UnturnedPlayer p = result.caller;

#if DEBUG
            Console.WriteLine("Webserver returns: " +result.result);
#endif

            switch (result.result)
            {
                case "0":
                    UnturnedChat.Say(result.caller, Instance.Translations.Instance.Translate("not_yet_voted", result.service.Name));
                    break;
                case "1":
                    if (result.giveItemDirectly)
                    {
                        if (Instance.OnPlayerVoted != null) Instance.OnPlayerVoted(result.caller, result.apidefinition);

                        new VotifierWebclient().DownloadStringAsync(new Uri(String.Format(result.apidefinition.ReportSuccess, result.service.APIKey, result.caller.ToString())));
                        return;
                    }
                    else
                    {
                        UnturnedChat.Say(result.caller, Instance.Translations.Instance.Translate("vote_pending_message", result.service.Name));
                        return;
                    }
                case "2":
                    UnturnedChat.Say(result.caller, Instance.Translations.Instance.Translate("vote_due_message", result.service.Name));
                    break;
            }
        }


    } 
   
}
