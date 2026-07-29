using System.Collections.Generic;
using UnityEngine;
using VertigoDemo.WheelOfFortune.Data;

namespace VertigoDemo.WheelOfFortune.Core
{
    public sealed class WheelConfigProviderMono : MonoBehaviour, IWheelConfigProvider
    {
        [SerializeField] private List<WheelConfig> _wheelConfigs = new List<WheelConfig>();

        public WheelConfig GetConfig(ZoneType zoneType)
        {
            for (int i = 0; i < _wheelConfigs.Count; i++)
            {
                WheelConfig config = _wheelConfigs[i];

                if (config != null && config.zoneType == zoneType)
                    return config;
            }

            Debug.LogError($"Wheel config not found for zone type: {zoneType}", this);

            return null;
        }
    }
}
