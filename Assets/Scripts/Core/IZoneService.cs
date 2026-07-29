using VertigoDemo.WheelOfFortune.Data;

namespace VertigoDemo.WheelOfFortune.Core
{
    public interface IZoneService
    {
        ZoneType GetZoneType(int zone);

        /// <summary>Leaving with the run's rewards is only allowed on safe and super zones.</summary>
        bool CanLeave(int zone);

        int GetNextSafeZone(int zone);

        int GetNextSuperZone(int zone);
    }
}
