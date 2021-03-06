﻿using System;
using System.IO;
using System.Reflection;
using CommandDotNet.Extensions;

namespace CommandDotNet.Help
{
    internal static class HelpBuilderExtensions
    {
        internal static string GetAppName(this Command command, AppHelpSettings appHelpSettings)
        {
            if (!appHelpSettings.UsageAppName.IsNullOrEmpty())
            {
                return appHelpSettings.UsageAppName;
            }

            switch (appHelpSettings.UsageAppNameStyle)
            {
                case UsageAppNameStyle.Adaptive:
                    return GetAppNameAdaptive();
                case UsageAppNameStyle.DotNet:
                    return $"dotnet {GetAppFileName()}";
                case UsageAppNameStyle.GlobalTool:
                    var rootAppName = GetRootAppName(command);
                    if (rootAppName == null)
                    {
                        throw new AppRunnerException(
                            $"Invalid configuration: {nameof(CommandAttribute)}.{nameof(CommandAttribute.Name)} " +
                            $"is required for the root command when {nameof(UsageAppNameStyle)}.{nameof(UsageAppNameStyle.GlobalTool)} " +
                            "is specified.");
                    }
                    return rootAppName;
                case UsageAppNameStyle.Executable:
                    return GetAppFileName();
                default:
                    throw new ArgumentOutOfRangeException(nameof(UsageAppNameStyle), $"unknown style: {appHelpSettings.UsageAppNameStyle}");
            }
        }

        private static string GetAppNameAdaptive()
        {
            string fileName = GetAppFileName();
            if (fileName != null)
            {
                // If the file is an .exe, it's more likely a .Net Framework file
                // and will not be executed via the dotnet tool.
                return fileName.EndsWith(".exe") ? fileName : $"dotnet {fileName}";
            }

            // https://github.com/bilal-fazlani/commanddotnet/issues/65
            // in some cases, entry assembly won't exist.  generally only an issue for tests.
            return "...";
        }

        private static string GetRootAppName(Command command)
        {
            return command.GetRootCommand().CustomAttributes?.GetCustomAttribute<CommandAttribute>()?.Name;
        }

        private static string GetAppFileName()
        {
            var hostAssembly = Assembly.GetEntryAssembly();
            return hostAssembly == null 
                ? null 
                : Path.GetFileName(hostAssembly.Location);
        }
    }
}