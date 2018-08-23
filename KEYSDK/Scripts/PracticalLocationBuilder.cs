using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if NET_4_6
using PracticalManaged.Practical.SpatialSpawning;
namespace Practical.Internal
{
    [Serializable]
    public class PracticalLocationBuilder : PracticalLocator
    {
        private void Start()
        {
            PracticalLocator.Instance.DefinedGroups = PracticalLocationBuilder.Instance.DefinedGroups;
        }
    }
}
#endif