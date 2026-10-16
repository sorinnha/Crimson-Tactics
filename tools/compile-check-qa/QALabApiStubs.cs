// The members of QA Lab (com.miawworks.qalab) that Assets/QA uses, with the package's real signatures
// (Runtime/Bot/*.cs, Runtime/Core/QALab.cs, Runtime/Detectors/DetectorNames.cs). Only for compiling.
#pragma warning disable CS8618
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace MiawWorks.QALab
{
    public interface IBotAdapter
    {
        string Name { get; }
        void Begin(BotContext ctx);
        BotStepResult Step(BotContext ctx);
        void End(BotContext ctx);
    }
    public enum BotStepResult { Continue, Done }
    public sealed class SeededRandom { public T Pick<T>(IReadOnlyList<T> items) => items[0]; }
    public sealed class BotContext
    {
        public SeededRandom Random { get; }
        public void LogAction(string action, Vector3? target = null, string uiPath = null, JObject args = null) { }
        public void LogAction(string action, IDictionary<string, object> args) { }
    }
    public static class BotAdapterRegistry { public static void Register(string name, Func<IBotAdapter> factory) { } }
    public sealed class QAEvent { }
    public static class QALab
    {
        public static bool IsRunning => false;
        public static QAEvent ReportDetector(string detector, string severity, JObject details = null, Vector3? position = null) => null;
    }
    public static class DetectorSeverity
    {
        public const string Blocker = "blocker", Critical = "critical", Major = "major", Minor = "minor", Trivial = "trivial";
    }
}

namespace UnityEngine
{
    public enum RuntimeInitializeLoadType { AfterAssembliesLoaded }
    public class RuntimeInitializeOnLoadMethodAttribute : System.Attribute
    {
        public RuntimeInitializeOnLoadMethodAttribute(RuntimeInitializeLoadType t) { }
    }
}
