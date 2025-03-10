﻿using System.CodeDom.Compiler;

using System.Diagnostics;
using System.Reflection;

using AspectInjector.Broker;

namespace StreamMaster.Domain.Logging;


[Aspect(Scope.Global)]
[Injection(typeof(LogExecutionTimeAspect))]
[GeneratedCode("", "")]
public class LogExecutionTimeAspect : Attribute
{
    [Advice(Kind.Around, Targets = Target.Method)]
    public object HandleMethod(
    [Argument(Source.Name)] string name,
    [Argument(Source.Arguments)] object[] args,
    [Argument(Source.Target)] Func<object[], object> target,
    [Argument(Source.Metadata)] MethodBase method,
    [Argument(Source.ReturnType)] Type retType,
    [Argument(Source.Triggers)] Attribute[] triggers)
    {
        string fullName = method.DeclaringType?.FullName ?? "";

        string abbreviatedNamespace = AbbreviateNamespace(fullName);
        string nameToLog = $"{abbreviatedNamespace}.{name}";


        List<string> whiteList = ["xmltvbuilder", "processm3ufile", "deletevideostreams", "processstreams"];

        if (!whiteList.Any(nameToLog.ContainsIgnoreCase)
            && !ShouldLog(fullName, ["*.Queries"]))
        {
            return target(args); // If the name doesn't match any string in the list, execute the method without any logging logic.
        }

        ILogger logger = GlobalLoggerProvider.CreateLogger(nameToLog);

        Stopwatch stopwatch = new();
        stopwatch.Start();

        object result = target(args);

        if (result is Task task)
        {
            // Wait for the task to complete and log the execution time.
            // Note: We're NOT using await, so we're blocking until the task completes.
            task.ContinueWith(t =>
            {
                stopwatch.Stop();
                if (t.IsFaulted)
                {
                    logger.LogError(t.Exception, $"Error executing {nameToLog}.");
                }
                else
                {
                    logger.LogInformation($"{nameToLog} executed in {stopwatch.ElapsedMilliseconds} ms.");
                }
            }).Wait();  // This ensures the continuation completes before we proceed.

            return result;  // This will return the original Task or Task<T>
        }

        stopwatch.Stop();
        logger.LogInformation($"{nameToLog} executed in {stopwatch.ElapsedMilliseconds} ms.");

        return result;
    }

    private static bool ShouldLog(string nameToLog, List<string> LogPerformance)
    {
        if (nameToLog.Contains("GetStreamGroupLineupStatusHandler") || nameToLog.Contains("GetStreamGroupDiscoverHandler"))
        {
            return false;
        }

        if (LogPerformance.Contains("*"))
        {
            return true;
        }

        foreach (string pattern in LogPerformance)
        {
            if (pattern.EndsWith(".*")) // If the pattern ends with ".*", we match the start of nameToLog
            {
                string trimmedPattern = pattern.TrimEnd('.', '*');
                if (nameToLog.StartsWith(trimmedPattern))
                {
                    return true;
                }
            }
            else if (pattern.StartsWith("*."))
            {
                string trimmedPattern = pattern.TrimStart('*');
                if (nameToLog.Contains(trimmedPattern))
                {
                    return true;
                }
            }
            else if (nameToLog.Contains(pattern))
            {
                return true;
            }
        }

        return false;
    }


    private static string AbbreviateNamespace(string fullNamespace)
    {
        string[] parts = fullNamespace.Split('.');
        if (parts.Length <= 1)
        {
            return fullNamespace; // If there are 3 or fewer parts, just return the original
        }

        // Only take the last three parts
        return string.Join('.', parts.Skip(parts.Length - 1));
    }


}
