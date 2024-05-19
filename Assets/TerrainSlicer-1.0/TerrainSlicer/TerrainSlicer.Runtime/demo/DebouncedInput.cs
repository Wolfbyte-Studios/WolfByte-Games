using System.Collections.Generic;
using UnityEngine;

namespace demo
{
    public class DebouncedInput
    {
        private static readonly Dictionary<KeyCode, float> lastKeyPressedTime = new Dictionary<KeyCode, float>();
        
        public static bool GetKey(KeyCode keyCode, float delay = 0.5f)
        {
            if (!Input.GetKey(keyCode))
            {
                return false;
            }
            
            var currentTime = Time.time;
            if (!lastKeyPressedTime.ContainsKey(keyCode))
            {
                lastKeyPressedTime.Add(keyCode, currentTime);
                return true;
            }

            var lastKeyTime = lastKeyPressedTime[keyCode];
            if (currentTime - lastKeyTime > delay)
            {
                lastKeyPressedTime[keyCode] = currentTime;
                return true;
            }

            return false;
        }
    }
}