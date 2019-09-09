using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UDGV
{
    internal class DependencyCacheDataHandler
    {
        private Dictionary<string, DependencyData> _data = new Dictionary<string, DependencyData>();

        public void Clear()
        {
            _data.Clear();
        }

        public bool TryGetValue(string key, out DependencyData dependencyData)
        {
            return _data.TryGetValue(key, out dependencyData);
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
