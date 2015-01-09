using Rocket.RocketAPI;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace unturned.ROCKS.Votifier
{
    public class VotifierConfiguration : RocketConfiguration
    {
        [XmlArrayItem(ElementName = "Service")]
        public List<Service> Services;
        [XmlArrayItem(ElementName = "RewardBundle")]
        public List<RewardBundle> RewardBundles;
        [XmlArrayItem(ElementName = "ServiceDefinition")]
        public List<ServiceDefinition> ServiceDefinitions;

        public RocketConfiguration DefaultConfiguration
        {
            get {
                VotifierConfiguration configuration = new VotifierConfiguration();
                configuration.ServiceDefinitions = new List<ServiceDefinition>() {
                    new ServiceDefinition() {
                        Name = "unturned-servers.net",
                        CheckHasVoted = "http://unturned-servers.net/api/?object=votes&element=claim&key={0}&steamid={1}",
                        ReportSuccess ="http://unturned-servers.net/api/?action=post&object=votes&element=claim&key={0}&steamid={1}"
                    }
                    , new ServiceDefinition() {
                        Name = "unturnedsl.com",
                        CheckHasVoted = "http://unturnedsl.com/api/dedicated/{0}/{1}",
                        ReportSuccess = "http://unturnedsl.com/api/dedicated/post/{0}/{1}"
                    }
                };

                configuration.RewardBundles = new List<RewardBundle>() { 
                    new RewardBundle() { Name="Survival", Rewards = new List<Reward>() { new Reward(245, 1), new Reward(81, 2), new Reward(16, 2) }, Probability = 33 },
                    new RewardBundle() { Name="Brute Force", Rewards = new List<Reward>() { new Reward(112, 1), new Reward(113, 3), new Reward(254, 3) }, Probability = 33 },
                    new RewardBundle() { Name="Watcher", Rewards = new List<Reward>() { new Reward(109, 1), new Reward(111, 3), new Reward(236, 3) }, Probability = 33 }
                };
                configuration.Services = new List<Service>() { new Service("unturned-servers.net"), new Service("unturnedsl.com") };
                return configuration;
            }
        }
    }

    public class RewardBundle
    {
        public RewardBundle() { }

        public int Probability;

        public string Name;

        [XmlArrayItem(ElementName = "Reward")]
        public List<Reward> Rewards;
    }

    public class Reward{

        public Reward(){ }

        public Reward(ushort itemId, byte amount)
        {
            ItemId = itemId;
            Amount = amount;
        }

        [XmlAttribute("itemid")]
        public ushort ItemId;

        [XmlAttribute("amount")]
        public byte Amount;
    }

    public class Service
    {

        public Service() { }

        public Service(string name)
        {
            Name = name;
        }

        [XmlAttribute("name")]
        public string Name = "";

        [XmlAttribute("apikey")]
        public string APIKey ="";
    }


    public class ServiceDefinition
    {
        [XmlAttribute("name")]
        public string Name;
        [XmlElement]
        public string CheckHasVoted;
        [XmlElement]
        public string ReportSuccess;
    }
}
