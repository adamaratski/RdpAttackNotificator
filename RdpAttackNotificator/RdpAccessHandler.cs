using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NLog;
using RdpAttackNotificator.Configuration;
using RdpAttackNotificator.Models;
using RdpAttackNotificator.Models.Targets;

namespace RdpAttackNotificator
{
    public class RdpAccessHandler
    {
        public void Process()
        {
            AppConfigurationSection section = (AppConfigurationSection)ConfigurationManager.GetSection("ConfigurationSection");
            var operationSystems = section.OperationSystems.Cast<OperationSystemElement>().ToList();
            List<ILockTarget> targets = new List<ILockTarget>();
            foreach (LockTargetElement element in section.LockTargets)
            {
                targets.Add(new Mikrotik(element.Target, element.Port, element.BlackList, element.User, element.Password));
            }
            var currentOperationSystemElement = operationSystems.FirstOrDefault(item => item.Version == Environment.OSVersion.VersionString) ?? operationSystems.FirstOrDefault(item => item.IsDefault);
            
            // get regex expressions
            List<String> expressions = new List<String>();
            foreach(var key in currentOperationSystemElement.SearchPatterns.AllKeys)
            {
                expressions.Add(currentOperationSystemElement.SearchPatterns[key].Value);
            }

            //get white list
            List<String> whiteList = new List<String>();
            foreach (var key in section.WhiteList.AllKeys)
            {
                whiteList.Add(section.WhiteList[key].Value);
            }

            var config = currentOperationSystemElement != null ? new OperationSystemConfig()
            {
                Version = currentOperationSystemElement.Version,
                InstanceId = currentOperationSystemElement.InstanceId,
                SearchEngines = expressions.Select(item => new Regex(item, RegexOptions.Singleline)).ToList(),
                Source = currentOperationSystemElement.Source
            } : null;

            TimeSpan scanPeriod = TimeSpan.Parse(ConfigurationManager.AppSettings["ScanPeriod"]);
            Int32 countLimit = Int32.Parse(ConfigurationManager.AppSettings["CountLimit"]);

            if (config != null)
            {
                var ipList = this.GetIpList(config, scanPeriod, countLimit, whiteList).Distinct().ToList();
                this.SendBlockList(targets, ipList);
            }
            else
            {
                LogManager.GetLogger("Configuration").Error($"Unknown OS version {Environment.OSVersion.VersionString}");
            }

            targets.ForEach(target => target.Close());
            targets.Clear();
        }

        public IEnumerable<String> GetIpList(OperationSystemConfig config, TimeSpan scanPeriod, Int32 countLimit, List<String> whitelist)
        {
            var logs = EventLog.GetEventLogs().FirstOrDefault(item => item.Log == config.Source);

            if (logs != null)
            {
                Dictionary<String, Int32> attemptsDictionary = new Dictionary<String, Int32>();

                foreach (EventLogEntry entry in logs.Entries.Cast<EventLogEntry>().Where(item => item.InstanceId == config.InstanceId && (DateTime.Now - item.TimeGenerated) < scanPeriod).OrderByDescending(item => item.TimeGenerated))
                {
                    DateTime dateTime = entry.TimeGenerated;

                    var match = config.SearchEngines.Select(item => item.Match(entry.Message)).FirstOrDefault(item => item.Success);

                    if (match != null)
                    {
                        String sourceIp = match.Groups["SourceIp"].Value;
                        String login = match.Groups["Login"].Value;

                        if (whitelist.Contains(sourceIp))
                        {
                            LogManager.GetLogger("Scheduler").Info($"DateTime is {dateTime:G}, Source IP : {sourceIp.PadLeft(15, ' ')}, Login : {login} in white list.");
                            continue;
                        }

                        LogManager.GetLogger("Scheduler").Info($"DateTime is {dateTime:G}, Source IP : {sourceIp.PadLeft(15, ' ')}, Login : {login} added to black list.");

                        if (attemptsDictionary.ContainsKey(sourceIp) == false)
                        {
                            attemptsDictionary.Add(sourceIp, 0);
                        }

                        attemptsDictionary[sourceIp]++;

                        if (attemptsDictionary[sourceIp] >= countLimit)
                        {
                            yield return sourceIp;
                        }
                    }
                }
            }
        }

        public void SendBlockList(List<ILockTarget> targets, List<String> ipList)
        {
            Parallel.ForEach(targets, target => ipList.ForEach(ip => target.AddToBlockList(ip)));
        }
    }
}
