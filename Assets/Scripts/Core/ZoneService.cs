using VertigoDemo.WheelOfFortune.Data;
using VertigoDemo.WheelOfFortune.Utilities;

namespace VertigoDemo.WheelOfFortune.Core
{
    public interface IZoneService
    {
        ZoneType GetZoneType(int zone);
        bool CanLeave(int zone);
    }

    public sealed class ZoneService : IZoneService
    {
        public ZoneType GetZoneType(int zone)
        {
            if (zone % GameConstants.SuperZoneInterval == 0)
                return ZoneType.Super;

            if (zone % GameConstants.SafeZoneInterval == 0)
                return ZoneType.Safe;

            return ZoneType.Normal;
        }

        public bool CanLeave(int zone)
        {
            ZoneType zoneType = GetZoneType(zone);
            return zoneType == ZoneType.Safe || zoneType == ZoneType.Super;
        }
    }
}