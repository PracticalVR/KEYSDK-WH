using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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