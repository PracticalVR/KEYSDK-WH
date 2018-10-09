#if UNITY_WSA && ENABLE_AR
#define HOLOLENS_XR_ENABLED
using PracticalManaged.Practical.SpatialSpawning;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Practical.Internal
{
    [Serializable]
#if HOLOLENS_XR_ENABLED
    public class SpatialPlacementTool : PracticalLocator
#else
    public class SpatialPlacementTool : MonoBehaviour
#endif
    {
        private void Start()
        {
#if HOLOLENS_XR_ENABLED
            PracticalLocator.Instance.DefinedGroups = SpatialPlacementTool.Instance.DefinedGroups;
#endif
        }
    }
}