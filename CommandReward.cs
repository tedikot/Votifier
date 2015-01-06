using Rocket;
using Rocket.RocketAPI;
using SDG;
using System;
using System.Collections.Generic;
using System.Linq;

namespace unturned.ROCKS.Votifier
{
    class CommandReward : Command
    {
        public CommandReward()
        {
            base.commandName = "reward";
            base.commandInfo = base.commandHelp = "This is a testcommand";
        }

        protected override void execute(SteamPlayerID caller, string command)
        {
            try
            {
                if (Votifier.Configuration.Services.Where(s => !String.IsNullOrEmpty(s.APIKey)).Count() == 0)
                {
                    Logger.Log("No apikeys supplied."); return;
                }

                List<Service> services = Votifier.Configuration.Services.Where(s => !String.IsNullOrEmpty(s.APIKey)).ToList();


                SteamPlayer voter = PlayerTool.getSteamPlayer(caller.CSteamID);
                bool hasVoted = false;
                foreach (Service service in services)
                {
                    ServiceDefinition apidefinition = Votifier.Configuration.ServiceDefinitions.Where(s => s.Name == service.Name).FirstOrDefault();
                    if (apidefinition == null) { Logger.Log("The API for " + service.Name + " is unknown"); return; }
                    string result = new VotifierWebclient().DownloadString(String.Format(apidefinition.CheckHasVoted, service.APIKey, caller.CSteamID.ToString()));
                    if (result == "1") {
                        hasVoted = true;
                        
                        bool success = true;
                        foreach (Reward reward in Votifier.Configuration.Rewards)
                        {
                            if (!ItemTool.tryForceGiveItem(voter.Player, reward.ItemId,reward.Amount))
                            {
                                success = false;
                            }
                        }
                        if (success)
                        {
                            ChatManager.say(String.Format(voter.SteamPlayerID.CharacterName+" voted for this server on " + service.Name + " and got a reward."));
                            new VotifierWebclient().DownloadString(String.Format(apidefinition.ReportSuccess, service.APIKey, caller.CSteamID.ToString()));
                        }
                        else
                        {
                            Logger.Log("Failed giving a item to "+voter.SteamPlayerID.CharacterName);
                        }

                    }
                }
                if (!hasVoted)
                {
                    ChatManager.say(caller.CSteamID, "You have not voted for this server today, please visit " + String.Join(",", services.Select(s => s.Name).ToArray()) + " to do so.");
                }
            }
            catch (Exception e)
            {
                Logger.Log(e.ToString());
            }
        }
    }
}
