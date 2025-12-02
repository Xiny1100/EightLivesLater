using System;
using System.Collections.Generic;
using UnityEngine;

namespace Seagull.Interior_I1.SceneProps
{
    [Serializable]
    public class String2Shiftable
    {
        public string key;
        public Shiftable value;
    }

    public class ShiftableObject : MonoBehaviour
    {
        public List<String2Shiftable> shiftables = new();
        private Dictionary<string, Shiftable> shiftableMap = new();

        private void Awake()
        {
            foreach (var pair in shiftables)
            {
                if (pair.value != null && !string.IsNullOrEmpty(pair.key))
                {
                    shiftableMap[pair.key] = pair.value;
                }
            }
        }

        public void shift(string id, float rotation01)
        {
            rotation01 = Mathf.Clamp01(rotation01);
            if (shiftableMap.ContainsKey(id))
            {
                shiftableMap[id].shift = rotation01;
            }
            else
            {
                Debug.LogWarning($"Shiftable with id '{id}' not found!");
            }
        }

        public void shift(float rotation01)
        {
            rotation01 = Mathf.Clamp01(rotation01);
            foreach (var shiftable in shiftableMap.Values)
            {
                if (shiftable != null)
                {
                    shiftable.shift = rotation01;
                }
            }
        }

        // Optional: Editor method to help with initialization
        [ContextMenu("Log Shiftable Count")]
        private void LogShiftableCount()
        {
            Debug.Log($"Registered shiftables: {shiftableMap.Count}");
        }
    }
}