using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FixedPointMath
{
    public const int LUT_SIZE = 3600;//定义查找表的大小 (0.1度精度，360度共3600个值)

    public const int DEG_0 = 0;    // 正右 (Positive X)
    public const int DEG_90 = 900;  // 正上 (Positive Y)
    public const int DEG_180 = 1800; // 正左 (Negative X)
    public const int DEG_270 = 2700; // 正下 (Negative Y)
    public const int DEG_360 = 3600; // 回到原点
}
