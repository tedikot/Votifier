using Rocket.RocketAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace unturned.ROCKS.Votifier
{
    public class Configuration : RocketConfiguration
    {
        public string ServerKey = "ServerKeyHere";
        public List<Reward> Rewards = new List<Reward>() { new Reward(18,1) };
    }
    public class Reward{

        public Reward(){ }

        public Reward(ushort itemId,byte amount){
            ItemId = itemId;
            amount = Amount;
        }

        [XmlAttribute("ItemId")]
        public ushort ItemId;

        [XmlAttribute("Amount")]
        public byte Amount;
    }
}
