using System.Collections.Generic;
using UnityEngine;
using VertigoDemo.WheelOfFortune.Data;

namespace VertigoDemo.WheelOfFortune.Core
{
    public interface IWheelConfigProvider
    {
        WheelConfig GetConfig(ZoneType zoneType);
    }

    public sealed class WheelConfigProvider : MonoBehaviour, IWheelConfigProvider
    {
        [SerializeField] private List<WheelConfig> wheelConfigs = new List<WheelConfig>();

        public WheelConfig GetConfig(ZoneType zoneType)
        {
            foreach (WheelConfig config in wheelConfigs)
            {
                if (config != null && config.zoneType == zoneType)
                    return config;
            }

            Debug.LogError($"Wheel config not found for zone type: {zoneType}");
            return null;
        }
    }
}