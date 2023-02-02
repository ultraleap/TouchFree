using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Numerics;
using System.Xml.Linq;
using System.IO;
using System.Runtime.InteropServices;

namespace Ultraleap.TouchFree.Library.Configuration
{
    public enum LogggingMode
    {
        ROTATING,
        DAILY
    }

    public class TrackingLoggingConfigFile : ConfigFile<TrackingLoggingConfig, TrackingLoggingConfigFile>
    {
        // Note the config.json filename is the name used by the Tracking Service
        protected override string _ConfigFileDirectory
        {
            get 
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Ultraleap\\HandTracker\\");
            }
        }
        protected override string _ConfigFileName => "config.json";
    }

    [Serializable]
    public class TrackingLoggingConfig
    {
        public int max_log_files;
        public int max_log_size;
        public bool rotate_on_open;

        public int log_days;
        public int log_days_rotation_hour;

        public string log_level;
        public int log_stats_timeout;

        public TrackingLoggingConfig()
        {
            // Default values reflect those in
            // https://ultrahaptics.atlassian.net/wiki/spaces/LMV5/pages/3544776774/Global+Config
            this.log_days = 0;
            this.log_days_rotation_hour = 2;

            this.max_log_files = 3;
            this.max_log_size = 1024 * 1024;
            this.rotate_on_open = false;

            this.log_level = String.Empty;
            this.log_stats_timeout = 1;
        }
    }
}
