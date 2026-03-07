namespace Debug
{
    public static class DebugSystem
    {
        public static void Log(string message)
        {
            UnityEngine.Debug.Log(message);
        } 
        public static void Warn(string message)
        {
            UnityEngine.Debug.LogWarning(message);
        } 
        public static void Error(string message)
        {
            UnityEngine.Debug.LogError(message);
        } 
    }
}