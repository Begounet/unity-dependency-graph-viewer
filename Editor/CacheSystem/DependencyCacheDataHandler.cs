using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UDGV.Utility;
using UnityEditor;
using UnityEngine;

namespace UDGV.CacheSystem
{
    [System.Serializable]
    internal class DependencyCacheDataHandler
    {
        [JsonProperty]
        private Dictionary<string, DependencyData> _data = new Dictionary<string, DependencyData>();

        public void Clear()
        {
            _data.Clear();
        }

        public bool Clear(string key)
        {
            return _data.Remove(key);
        }

        public bool TryGetValue(string key, out DependencyData dependencyData)
        {
            return _data.TryGetValue(key, out dependencyData);
        }

        public bool Contains(string key)
        {
            return _data.ContainsKey(key);
        }

        public void Add(string guid, DependencyData dependencyData)
        {
            _data.Add(guid, dependencyData);
        }

        public IEnumerable<DependencyData> GetDependenciesData()
        {
            return _data.Values;
        }

        public DependencyData CreateOrGetDependencyDataFromGuid(string guid)
        {
            if (_data.TryGetValue(guid, out DependencyData data))
            {
                return data;
            }

            DependencyData newData = new DependencyData()
            {
                objectGuid = guid
            };
            _data.Add(guid, newData);
            return newData;
        }
    }
}
