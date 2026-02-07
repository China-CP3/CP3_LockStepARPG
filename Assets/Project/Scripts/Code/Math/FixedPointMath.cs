
using System;

public static class FixedPointMath
{
    #region 三角函数
   //查找表的大小 (0.1度精度，360度共3600个值 28kb内存) 没必要36000 280kb内存 人类肉眼难以区分是否移动和旋转了0.01度

    private const int DEG_0 = 0;    // 正右 (Positive X)
    private const int DEG_90 = 900;  // 正上 (Positive Y)
    private const int DEG_180 = 1800; // 正左 (Negative X)
    private const int DEG_270 = 2700; // 正下 (Negative Y)
    private const int DEG_360 = 3600; // 回到原点
    private const int MaxAngle = 900;

    /// <summary>
    /// 将角度缩放到 [0, 3599] 范围内 (0.1度精度)
    /// 确保查表时不会越界
    /// </summary>
    public static int GetClampDegree(int angle)
    {
        angle %= DEG_360;
        if (angle < 0) angle += DEG_360;//-1度 = 359度
        return angle;
    }

    /// <summary>
    /// 获取 Sin 值
    /// </summary>
    public static long Sin(int angle)
    {
        int clampAngle = GetClampDegree(angle);//限制在360°以内
        bool isNegative = false;

        //第一象限:直接查表 第二：180 - x   第三 ： x - 180 第四：360 - x
        if (clampAngle > DEG_180)
        {
            isNegative = true;
            clampAngle = DEG_360 - clampAngle; // 359度变成1度，181度变成179度 x轴对称
        }else if (clampAngle == DEG_180)
        {
            return 0;
        }

        if (clampAngle > DEG_90)// 第二象限 (90.1° ~ 179.9°)与第一象限 y 对称
        {
            clampAngle = DEG_180 - clampAngle;
        }

        if (clampAngle < 0) clampAngle = 0;
        if (clampAngle > MaxAngle) clampAngle = MaxAngle;

        long value = sinTable90[clampAngle];
        return isNegative ? -value : value;
    }

    /// <summary>
    /// cos(x) = sin(x + 90.0°)
    /// </summary>
    public static long Cos(int angle)
    {
        return Sin(angle + DEG_90);
    }

    /// <summary>
    /// 获取 Tan 值：tan(x) = sin(x) / cos(x)
    /// 这里的乘 ScaleFactor 是为了保持定点数格式
    /// </summary>
    public static long Tan(int angle)
    {
        long sin = Sin(angle);
        long cos = Cos(angle);

        // 必须用绝对值判断，否则Cos 是负数。
        // 此时 c < FixedPoint.One.ScaledValue 永远成立（负数小于正数），这会导致所有的第二、三象限的 Tan 全部被错误地判定为 MaxValue。
        long abcCos = (cos < 0) ? -cos : cos;

        // 如果 cos 极小（不只是0），则视为无穷大，限制在定点数最大值范围内
        if (abcCos == 0 || abcCos < FixedPoint.One.ScaledValue)
        {
            return (sin > 0) ? FixedPoint.MaxValue.ScaledValue : FixedPoint.MinValue.ScaledValue;
        }

        Int128 numerator = new Int128(sin) << FixedPoint.ShiftBits;
        Int128 result = numerator / cos;
        return Int128.ClampInt128ToLong(result);
    }

    // sinTable 0-90 只存了0-90度 901个数值 每0.1度对应1个数值 并且放大1024倍 对应定点数放大倍数
    private static readonly short[] sinTable90 = new short[] {
0, 2, 4, 5, 7, 9, 11, 13, 14, 16, 18, 20, 21, 23, 25,
27, 29, 30, 32, 34, 36, 38, 39, 41, 43, 45, 46, 48, 50, 52,
54, 55, 57, 59, 61, 63, 64, 66, 68, 70, 71, 73, 75, 77, 79,
80, 82, 84, 86, 87, 89, 91, 93, 95, 96, 98, 100, 102, 103, 105,
107, 109, 111, 112, 114, 116, 118, 119, 121, 123, 125, 127, 128, 130, 132,
134, 135, 137, 139, 141, 143, 144, 146, 148, 150, 151, 153, 155, 157, 158,
160, 162, 164, 165, 167, 169, 171, 173, 174, 176, 178, 180, 181, 183, 185,
187, 188, 190, 192, 194, 195, 197, 199, 201, 202, 204, 206, 208, 209, 211,
213, 215, 216, 218, 220, 222, 223, 225, 227, 229, 230, 232, 234, 236, 237,
239, 241, 243, 244, 246, 248, 249, 251, 253, 255, 256, 258, 260, 262, 263,
265, 267, 268, 270, 272, 274, 275, 277, 279, 281, 282, 284, 286, 287, 289,
291, 293, 294, 296, 298, 299, 301, 303, 305, 306, 308, 310, 311, 313, 315,
316, 318, 320, 322, 323, 325, 327, 328, 330, 332, 333, 335, 337, 338, 340,
342, 344, 345, 347, 349, 350, 352, 354, 355, 357, 359, 360, 362, 364, 365,
367, 369, 370, 372, 374, 375, 377, 379, 380, 382, 384, 385, 387, 389, 390,
392, 394, 395, 397, 398, 400, 402, 403, 405, 407, 408, 410, 412, 413, 415,
416, 418, 420, 421, 423, 425, 426, 428, 430, 431, 433, 434, 436, 438, 439,
441, 442, 444, 446, 447, 449, 450, 452, 454, 455, 457, 459, 460, 462, 463,
465, 466, 468, 470, 471, 473, 474, 476, 478, 479, 481, 482, 484, 485, 487,
489, 490, 492, 493, 495, 496, 498, 500, 501, 503, 504, 506, 507, 509, 510,
512, 514, 515, 517, 518, 520, 521, 523, 524, 526, 527, 529, 530, 532, 534,
535, 537, 538, 540, 541, 543, 544, 546, 547, 549, 550, 552, 553, 555, 556,
558, 559, 561, 562, 564, 565, 567, 568, 570, 571, 573, 574, 576, 577, 579,
580, 581, 583, 584, 586, 587, 589, 590, 592, 593, 595, 596, 598, 599, 600,
602, 603, 605, 606, 608, 609, 611, 612, 613, 615, 616, 618, 619, 621, 622,
623, 625, 626, 628, 629, 630, 632, 633, 635, 636, 637, 639, 640, 642, 643,
644, 646, 647, 649, 650, 651, 653, 654, 655, 657, 658, 660, 661, 662, 664,
665, 666, 668, 669, 670, 672, 673, 674, 676, 677, 679, 680, 681, 683, 684,
685, 687, 688, 689, 690, 692, 693, 694, 696, 697, 698, 700, 701, 702, 704,
705, 706, 707, 709, 710, 711, 713, 714, 715, 716, 718, 719, 720, 722, 723,
724, 725, 727, 728, 729, 730, 732, 733, 734, 735, 737, 738, 739, 740, 742,
743, 744, 745, 746, 748, 749, 750, 751, 753, 754, 755, 756, 757, 759, 760,
761, 762, 763, 765, 766, 767, 768, 769, 770, 772, 773, 774, 775, 776, 777,
779, 780, 781, 782, 783, 784, 786, 787, 788, 789, 790, 791, 792, 794, 795,
796, 797, 798, 799, 800, 801, 803, 804, 805, 806, 807, 808, 809, 810, 811,
812, 813, 815, 816, 817, 818, 819, 820, 821, 822, 823, 824, 825, 826, 827,
828, 829, 831, 832, 833, 834, 835, 836, 837, 838, 839, 840, 841, 842, 843,
844, 845, 846, 847, 848, 849, 850, 851, 852, 853, 854, 855, 856, 857, 858,
859, 860, 861, 862, 863, 864, 865, 866, 867, 867, 868, 869, 870, 871, 872,
873, 874, 875, 876, 877, 878, 879, 880, 880, 881, 882, 883, 884, 885, 886,
887, 888, 889, 889, 890, 891, 892, 893, 894, 895, 896, 896, 897, 898, 899,
900, 901, 902, 902, 903, 904, 905, 906, 907, 907, 908, 909, 910, 911, 912,
912, 913, 914, 915, 916, 916, 917, 918, 919, 920, 920, 921, 922, 923, 923,
924, 925, 926, 927, 927, 928, 929, 930, 930, 931, 932, 933, 933, 934, 935,
935, 936, 937, 938, 938, 939, 940, 940, 941, 942, 943, 943, 944, 945, 945,
946, 947, 947, 948, 949, 949, 950, 951, 951, 952, 953, 953, 954, 955, 955,
956, 957, 957, 958, 959, 959, 960, 960, 961, 962, 962, 963, 963, 964, 965,
965, 966, 966, 967, 968, 968, 969, 969, 970, 971, 971, 972, 972, 973, 973,
974, 974, 975, 976, 976, 977, 977, 978, 978, 979, 979, 980, 980, 981, 981,
982, 982, 983, 983, 984, 984, 985, 985, 986, 986, 987, 987, 988, 988, 989,
989, 990, 990, 990, 991, 991, 992, 992, 993, 993, 994, 994, 994, 995, 995,
996, 996, 997, 997, 997, 998, 998, 999, 999, 999, 1000, 1000, 1000, 1001, 1001,
1002, 1002, 1002, 1003, 1003, 1003, 1004, 1004, 1004, 1005, 1005, 1006, 1006, 1006, 1007,
1007, 1007, 1007, 1008, 1008, 1008, 1009, 1009, 1009, 1010, 1010, 1010, 1011, 1011, 1011,
1011, 1012, 1012, 1012, 1012, 1013, 1013, 1013, 1014, 1014, 1014, 1014, 1015, 1015, 1015,
1015, 1015, 1016, 1016, 1016, 1016, 1017, 1017, 1017, 1017, 1017, 1018, 1018, 1018, 1018,
1018, 1019, 1019, 1019, 1019, 1019, 1019, 1020, 1020, 1020, 1020, 1020, 1020, 1021, 1021,
1021, 1021, 1021, 1021, 1021, 1022, 1022, 1022, 1022, 1022, 1022, 1022, 1022, 1022, 1023,
1023, 1023, 1023, 1023, 1023, 1023, 1023, 1023, 1023, 1023, 1023, 1023, 1023, 1024, 1024,
1024, 1024, 1024, 1024, 1024, 1024, 1024, 1024, 1024, 1024, 1024, 1024, 1024, 1024, 1024,
1024
};

    #endregion
    /// <summary>
    /// 求绝对值
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static FixedPoint Abs(FixedPoint value)
    {
        if (value.ScaledValue == long.MinValue) return FixedPoint.MaxValue;
        //long result = value.scaledValue < 0 ? -value.scaledValue : value.scaledValue;//尽量用位运算替换分支判断 性能稍好些
        long mask = value.ScaledValue >> 63;//得到符号位 正数全是0 负数全是1
        //细节操作 用4位举例 假如是0101 右移3位得到0000
        //0101 + 0000 = 0101 ^ 0000 = 0101 正数完全没影响 值不改变 
        //假如是1010 右移得到1111
        //1010 + 1111 = 1001 ^ 1111 = 0110 = 1010补码
        long result = (value.ScaledValue + mask) ^ mask;
        //return new FixedPoint(result);
        return FixedPoint.CreateByScaledValue(result);
        /*
         * CPU 为了提升效率，采用了指令流水线（Pipeline）技术。当遇到 if 分支时，CPU 会利用分支预测器尝试预判结果并提前执行后面的指令。
           如果猜对了：流水线满载运行，效率最高。
           如果猜错了：CPU 必须扔掉已经算了一半的指令，重新回到分支点。会导致几十个时钟周期的浪费。
         */
    }

    /// <summary>
    /// 计算定点数的平方根 牛顿迭代法
    /// </summary>
    /// <param name="targetFp">一个非负的定点数 对它开方</param>
    /// <returns>该定点数的平方根</returns>
    public static FixedPoint Sqrt(FixedPoint fixedPoint)
    {
        //负数没有实数平方根
        if (fixedPoint.ScaledValue < 0)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.LogError("FixedPoint Sqrt() param fp < 0 !");
#endif
            return FixedPoint.Zero;
        }

        if (fixedPoint == FixedPoint.Zero)//0的平方根就是0
        {
            return FixedPoint.Zero;
        }

        //本质原因是 放大倍数也被开方了 所以这里要再放大1次 才能保证结果正确
        //比如放大100倍，结果被开方后变成了放大10倍，所以再放大1次 100 * 100 开方后不就刚好是100了吗
        Int128 targetScaledValue = new Int128(fixedPoint.ScaledValue) << FixedPoint.ShiftBits;

        //1个数的二进制位数 大约是 它的平方根的二进制位数的2倍 
        //比如 n = 10000 (二进制 10 0111 0001 0000，长度14位) sqrt(n) = 100(二进制 110 0100，长度7位) 注意只是大约 也有14对比6或者8的情况
        int mostBitPos = FindMostSignificantBitPositionForInt128(targetScaledValue);//小心这里得到的值较大 超过long的最大位数64位
        //注意细节 1是long 避免因为int位运算 产生32位的容器 导致后面的long只能在32位容器上计算 丢失数值
        //+1是为了避免向下取整丢失精度 导致首次猜测值过小 距离真实平方根更遥远 导致后续牛顿迭代法要多循环更多次
        //比如 sqrt(60) 约等于 7.74。
        //N = 60。二进制是 111100。最高有效位在第5位，所以 mostBitPos = 5。
        //没有 +1 的计算：
        //5 >> 1 = 2 (向下取整，丢失了精度) 初始猜测值 = 1L << 2 = 4  这个猜测值 4 和真实值 7.74 相比，误差很大。
        //有 +1 的计算：
        //(5 >> 1) + 1 = 2 + 1 = 3   初始猜测值 = 1L << 3 = 8
        //+1也保证了初始猜测值总是大于真实的平方根

        //强制限制它最大为 62，确保 1L << shiftAmount 永远是个正数。
        int shiftAmount = (mostBitPos >> 1) + 1;
        if (shiftAmount > 62) shiftAmount = 62;
        long currentGuess = 1L << shiftAmount;
        //long currentGuess = 1L << ((mostBitPos >> 1) + 1);

        const int MAX_ITERATIONS = 12;
        for (int i = 0; i < MAX_ITERATIONS; i++)
        {
            if (currentGuess == 0)
            {
                break;
            }

            Int128 nextGuessValue = ((Int128)currentGuess + targetScaledValue / currentGuess) >> 1;
            long nextGuessValueLong = (long)nextGuessValue;
#if UNITY_EDITOR

            //UnityEngine.Debug.Log(string.Format("Times:{0},currentGuess:{1},nextGuessValue:{2},Math.Sqrt:{3}", i, currentGuess / 1024, nextGuessValue / 1024, Math.Sqrt(fixedPoint.ScaledValue / 1024)));
#endif
            if (nextGuessValueLong >= currentGuess)//下次猜测会小于当前猜测 猜测值每次循环从大到小越来越逼近结果 如果下次猜测的值大于了当前猜测 说明已经越过了结果
            {
                return FixedPoint.CreateByScaledValue(currentGuess);
            }

            currentGuess = nextGuessValueLong;
        }

        return FixedPoint.CreateByScaledValue(currentGuess);//循环12次后 实在不行就返回当前值 基本上不会触发这一行
    }

    /// <summary>
    /// 二分法查找 某个值的二进制最左边的1具体在哪一位
    /// </summary>
    /// <returns></returns>
    private static int FindMostSignificantBitPositionForULong(ulong value)
    {
        //思路 用8位举例 0001 0000 先检查高4位(最大位数的一半) !=0 说明值在高4位 丢弃低4位多余的值 返回值+4
        //现在是0001 检查高2位 == 0 说明值不在高2位 
        //现在是0001 检查高1位 == 0 说明值不在高1位 已经可以得出在第0位 返回值+0 最终返回值是4

        if (value <= 0)
        {
            return -1;
        }

        int postion = 0;
        for (int bit = 32; bit > 0; bit >>= 1)
        {
            if (value >> bit != 0)
            {
                postion += bit;
                value >>= bit;
            }
        }

        return postion;
    }

    private static int FindMostSignificantBitPositionForInt128(Int128 value)
    {
        if (value == Int128.Zero) return -1;

        long high = value.high64;
        if (high != 0)
        {
            // 如果 high 是负数，最高位一定是第 63 位 (符号位)
            if (high < 0) return 64 + 63;

            return 64 + FindMostSignificantBitPositionForULong((ulong)high);// 如果高 64 位不为 0，说明最高位在 64-127 之间
        }
        return FindMostSignificantBitPositionForULong(value.low64);//// 如果高位为 0，则最高位在 0-63 之间
    }

    public static FixedPoint Max(FixedPoint value1, FixedPoint value2)
    {
        return value1 > value2 ? value1 : value2;
    }

    public static FixedPoint Min(FixedPoint value1, FixedPoint value2)
    {
        return value1 < value2 ? value1 : value2;
    }

    public static FixedPoint Clamp(FixedPoint value, FixedPoint min, FixedPoint max)
    {
        return value < min ? min : value > max ? max : value;
    }

    /// <summary>
    /// 反余弦函数：根据 Cos 值反推角度 (0.1度单位)
    /// 输入范围：FixedPoint [-1, 1]
    /// 输出范围：[0, 1800] (0.1度单位)
    /// </summary>
    public static int Acos01(FixedPoint dot)
    {
        // 1. 限制输入范围在 [-1, 1]，防止点积微溢出导致查表失败
        FixedPoint clampedDot = Clamp(dot, -FixedPoint.One, FixedPoint.One);

        // 2. 转换成sinTable 对应的量级 (放大 1024 倍)
        // 注意：Cos(x) = Sin(90 - x)
        // 反查的是正弦表，所以处理的是 Sin 轴。
        long targetValue = clampedDot.ScaledValue;

        // 3. 处理符号。Cos 在 90-180 度是负的
        bool isNegative = targetValue < 0;
        long absValue = isNegative ? -targetValue : targetValue;

        // 4. 在 sinTable90 中进行二分查找 (查找对应 Cos 值的角度)
        // 因为 Cos(x) = sinTable[900 - x]，直接查这个 absValue 对应的角度索引
        int low = 0;
        int high = 900;
        int angleIndex = 0;

        while (low <= high)
        {
            int mid = (low + high) >> 1;
            if (sinTable90[mid] <= absValue)
            {
                angleIndex = mid; // 暂存最接近的索引
                low = mid + 1;
            }
            else
            {
                high = mid - 1;
            }
        }

        // 5. 根据 Cos 函数特性转换索引为最终 0.1 度角度
        // 如果是正数 (0~90度)：angle = 900 - angleIndex
        // 如果是负数 (90~180度)：angle = 900 + angleIndex
        if (isNegative)
        {
            return DEG_90 + angleIndex;
        }
        else
        {
            return DEG_90 - angleIndex;
        }
    }
}
