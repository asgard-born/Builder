namespace Shared
{
    public enum Resource : byte
    {
        None = 0,
        Gold = 10,
        Wood = 20,
        Metal = 30,
        Crystals = 255
    }
    
    public enum BuildCondition : byte
    {
        None = 0,
        BuildedPoint = 10,
        LevelAchieved = 20,
    }
}