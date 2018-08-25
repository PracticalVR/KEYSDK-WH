#if UNITY_WSA && ENABLE_AR
#define HOLOLENS_XR_ENABLED
using PracticalManaged.Practical.SpatialSpawning;
#endif
using System;
using UnityEngine;

namespace Practical.Internal
{
    [Serializable]
#if HOLOLENS_XR_ENABLED
    public class PracticalLocationBuilder : PracticalLocator
#else
    public class PracticalLocationBuilder : MonoBehaviour
#endif
    {
        private void Start()
        {
#if HOLOLENS_XR_ENABLED
            Debug.Log("tamamdir");
            PracticalLocator.Instance.DefinedGroups = PracticalLocationBuilder.Instance.DefinedGroups;
#endif
        }
    }
}