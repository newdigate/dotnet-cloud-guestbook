using System;

namespace frontend
{
    public interface IEnvironmentConfiguration
    {
        string? BackendAddress { get; set; }
        Uri? OtlpTraceSyncUri { get; set; }
    }

    public class EnvironmentConfiguration : IEnvironmentConfiguration
    {
        public string? BackendAddress { get; set; }
        public Uri? OtlpTraceSyncUri { get; set; }
    }
}
