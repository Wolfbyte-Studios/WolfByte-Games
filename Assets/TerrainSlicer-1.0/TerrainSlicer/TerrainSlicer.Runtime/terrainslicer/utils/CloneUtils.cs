using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace terrainslicer
{
    public static class CloneUtils
    {
        public static void CopyFrom<T1, T2>(T1 dst, T2 src, bool debugLog = false)
            where T1: class
            where T2: class
        {
            PropertyInfo[] srcFields = src.GetType().GetProperties(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);
         
            PropertyInfo[] destFields = dst.GetType().GetProperties(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty);

            var fieldsToSkip = new HashSet<string>()
            {
                "treeInstances"
            };
         
            LogUtils.DebugLog($"Copy {srcFields.Length} terrain fields...", debugLog);
            foreach (var property in srcFields) {
                if (fieldsToSkip.Contains(property.Name))
                {
                    continue;
                }
            
                var dest = destFields.FirstOrDefault(x => x.Name == property.Name);
                LogUtils.DebugLog($"Found dst field [{dest?.Name}] for src field [{property.Name}]", debugLog);
                if (dest != null && dest.CanWrite)
                {
                    dest.SetValue(dst, property.GetValue(src, null), null);
                    LogUtils.DebugLog($" - writing to field [{dest.Name}]", debugLog);
                }
                else
                {
                    LogUtils.DebugLog($" - can't write [{property.Name}]", debugLog);
                }
            }
        }
    }
}