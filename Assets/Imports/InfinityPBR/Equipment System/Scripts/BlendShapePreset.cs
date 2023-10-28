using System.Collections.Generic;
using UnityEngine;

namespace InfinityPBR
{
    [System.Serializable]
    public class BlendShapePreset
    {
        public string name;
        public List<BlendShapePresetValue> presetValues = new List<BlendShapePresetValue>();
        [HideInInspector] public bool showValues = false;
    }
}