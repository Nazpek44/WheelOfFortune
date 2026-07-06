using System;
using UnityEngine;

public enum ZoneType
{
    Normal,
    Safe,
    Super
}

[Serializable]
public class WheelSlice
{
    public string rewardName;
    public bool isBomb;
    public Sprite icon;
    public int amount;
    public string labelOverride;

    public string GetDisplayText()
    {
        if (isBomb)
            return "";

        if (!string.IsNullOrWhiteSpace(labelOverride))
            return labelOverride;

        return amount > 0 ? "x" + amount : "";
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

public static class ZoneRules
{
    public static ZoneType GetZoneType(int zone)
    {
        if (zone % 30 == 0)
            return ZoneType.Super;

        if (zone % 5 == 0)
            return ZoneType.Safe;

        return ZoneType.Normal;
    }

    public static bool CanLeave(int zone, bool isSpinning)
    {
        if (isSpinning)
            return false;

        ZoneType type = GetZoneType(zone);
        return type == ZoneType.Safe || type == ZoneType.Super;
    }
}