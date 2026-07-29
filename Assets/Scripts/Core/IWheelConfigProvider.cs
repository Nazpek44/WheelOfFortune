using VertigoDemo.WheelOfFortune.Data;

namespace VertigoDemo.WheelOfFortune.Core
{
    public interface IWheelConfigProvider
    {
        WheelConfig GetConfig(ZoneType zoneType);
    }
}
