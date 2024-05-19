using UnityEngine;

namespace terrainslicer
{
    public static class LogUtils
    {

        public static void DebugLog(string msg, bool logEnabled = false)
        {
            if (logEnabled)
            {
                Debug.Log(msg);
            }
        }
    }
}