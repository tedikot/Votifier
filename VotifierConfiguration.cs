using Rocket.RocketAPI;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace unturned.ROCKS.Votifier
{
    public class VotifierConfiguration : RocketConfiguration
    {
        public List<Service> Services;
        public List<Reward> Rewards;
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

                configuration.Rewards = new List<Reward>() { new Reward(245, 1),new Reward(81, 2),new Reward(16, 2) };
                configuration.Services = new List<Service>() { new Service("unturned-servers.net"), new Service("unturnedsl.com") };
                return configuration;
            }
        }
    }
    public class Reward{

        public Reward(){ }

        public Reward(ushort itemId, byte amount)
        {
            ItemId = itemId;
            amount = Amount;
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
