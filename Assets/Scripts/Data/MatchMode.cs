[System.Flags]
public enum MatchMode : int
{
    None            = 0,

    OneOnOne     = 1 << 0,  // 1v1
    TwoOnTwo     = 1 << 1,  // 2v2
    ThreeOnThree = 1 << 2,  // 3v3
    FourOnFour     = 1 << 3,  // 3v3
    NvN          = 0b1111,  // NvN

    FreeForAll   = 1 << 4,  // FFA
}
