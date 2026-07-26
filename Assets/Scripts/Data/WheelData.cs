using System;
using UnityEngine;
using VertigoDemo.WheelOfFortune.Utilities;

namespace VertigoDemo.WheelOfFortune.Data
{
    public enum ZoneType
    {
        Normal,
        Safe,
        Super
    }

    [Serializable]
    public class WheelSlice
    {
        public string rewardId;
        public string rewardName;
        public bool isBomb;
        public Sprite icon;
        public int amount;
        public string labelOverride;

        public string InventoryKey
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(rewardId))
                    return NormalizeKey(rewardId);

                if (!string.IsNullOrWhiteSpace(rewardName))
                    return NormalizeKey(rewardName);

                if (icon != null)
                    return NormalizeKey(icon.name);

                return "reward_unknown";
            }
        }

        private string NormalizeKey(string value)
        {
            return value
                .Trim()
                .ToLowerInvariant()
                .Replace(" ", "_")
                .Replace("-", "_");
        }

        public string GetDisplayText()
        {
            if (isBomb)
                return string.Empty;

            if (!string.IsNullOrWhiteSpace(labelOverride))
                return labelOverride;

            return RewardTextFormatter.FormatAmount(amount);
        }
    }

    [Serializable]
    public class WheelConfig
    {
        public string configName;
        public ZoneType zoneType;
        public Sprite wheelBaseSprite;
        public Sprite indicatorSprite;
        public WheelSlice[] slices = new WheelSlice[8];
    }
}