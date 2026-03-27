namespace Debug
{
    public static class DebugSystem
    {
        private const bool EnableLogging = true;

        public static void Log(string message)
        {
            if (EnableLogging)
            {
                UnityEngine.Debug.Log(message);
            }
        }

        public static void Warn(string message)
        {
            if (EnableLogging)
            {
                UnityEngine.Debug.LogWarning(message);
            }
        }

        public static void Error(string message)
        {
            if (EnableLogging)
            {
                UnityEngine.Debug.LogError(message);
            }
        }
    }
}