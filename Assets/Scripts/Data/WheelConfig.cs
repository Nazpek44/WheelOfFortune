using System;
using UnityEngine;

namespace VertigoDemo.WheelOfFortune.Data
{
    [Serializable]
    public class WheelConfig
    {
        public string configName;
        public ZoneType zoneType;
        public Sprite wheelBaseSprite;
        public Sprite indicatorSprite;
        public WheelSlice[] slices = new WheelSlice[8];

        public int SliceCount => slices == null ? 0 : slices.Length;

        public bool HasSlices => SliceCount > 0;
    }
}
