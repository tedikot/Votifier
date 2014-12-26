using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Steamworks;
using SDG;
using System.Timers;
using System.IO;
using System.Reflection;
using Rocket.RocketAPI;

namespace unturned.ROCKS.Votifier
{
    public class Votifier : RocketPlugin
    {
        public static Configuration Configuration;

        public void Load() 
        {
            try
            {
                Configuration = (Configuration)(new Configuration()).LoadConfiguration();
                Commands.RegisterCommand(new CommandReward("reward"));
                Commands.RegisterCommand(new CommandReward("vote"));
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
            }
        }

        public string Author
        {
            get { return "fr34kyn01535"; }
        }

        public string Name
        {
            get { return Assembly.GetExecutingAssembly().GetName().Name.ToString(); }
        }

        public string Version
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version.ToString(); }
        }
    }
}
