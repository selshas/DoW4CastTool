[System.Flags]
public enum MatchMode : int
{
    None        = 0,

    OneOnOne      = 1 << 0,  // 1v1
    TwoOnTwo      = 1 << 1,  // 2v2
    ThreeOnThree  = 1 << 2,  // 3v3
    FreeForAll    = 1 << 3,  // FFA
}
