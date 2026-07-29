using VertigoDemo.WheelOfFortune.Data;
using VertigoDemo.WheelOfFortune.Utilities;

namespace VertigoDemo.WheelOfFortune.Core
{
    public sealed class ZoneService : IZoneService
    {
        public ZoneType GetZoneType(int zone)
        {
            if (zone % GameConstants.SUPER_ZONE_INTERVAL == 0)
                return ZoneType.Super;

            if (zone % GameConstants.SAFE_ZONE_INTERVAL == 0)
                return ZoneType.Safe;

            return ZoneType.Normal;
        }

        public bool CanLeave(int zone)
        {
            ZoneType zoneType = GetZoneType(zone);

            return zoneType == ZoneType.Safe || zoneType == ZoneType.Super;
        }

        public int GetNextSafeZone(int zone)
        {
            return GetNextMultiple(zone, GameConstants.SAFE_ZONE_INTERVAL);
        }

        public int GetNextSuperZone(int zone)
        {
            return GetNextMultiple(zone, GameConstants.SUPER_ZONE_INTERVAL);
        }

        private static int GetNextMultiple(int value, int interval)
        {
            if (interval <= 0)
                return value;

            if (value % interval == 0)
                return value;

            return ((value / interval) + 1) * interval;
        }
    }
}
