using VertigoDemo.WheelOfFortune.Data;

namespace VertigoDemo.WheelOfFortune.Economy
{
    /// <summary>
    /// Turns an authored wheel slice into the reward actually granted in a
    /// given zone. This is what makes 30 zones feel like 30 zones instead of
    /// three wheels repeated ten times.
    /// </summary>
    public interface IRewardScaler
    {
        RewardDraw Resolve(WheelSlice slice, int zone, ZoneType zoneType);
    }
}
