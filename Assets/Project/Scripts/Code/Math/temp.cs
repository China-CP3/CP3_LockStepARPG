//using System;
//using System.Globalization;
//using System.Runtime.CompilerServices;
//using UnityEngine;

///// <summary>
///// 工程级有符号128位整数类型（Unity兼容，IL2CPP/mono均支持）
///// 基于双Int64存储：High（高64位） + Low（低64位）
///// 符号位：High的第63位（最高位）
///// </summary>
//[Serializable]
//public readonly struct Int128 : IEquatable<Int128>, IComparable<Int128>
//{
//    #region 核心字段
//    /// <summary>高64位（包含符号位）</summary>
//    public readonly long High;
//    /// <summary>低64位</summary>
//    public readonly long Low;
//    #endregion

//    #region 常量定义
//    /// <summary>最小值：-2^127</summary>
//    public static readonly Int128 MinValue = new Int128(unchecked((long)0x8000000000000000), 0);
//    /// <summary>最大值：2^127 - 1</summary>
//    public static readonly Int128 MaxValue = new Int128(unchecked((long)0x7FFFFFFFFFFFFFFF), unchecked((long)0xFFFFFFFFFFFFFFFF));
//    /// <summary>零</summary>
//    public static readonly Int128 Zero = new Int128(0, 0);
//    /// <summary>一</summary>
//    public static readonly Int128 One = new Int128(0, 1);
//    /// <summary>负一</summary>
//    public static readonly Int128 NegativeOne = new Int128(-1, -1);
//    #endregion

//    #region 构造函数
//    /// <summary>从高64位和低64位构建Int128</summary>
//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    public Int128(long high, long low)
//    {
//        High = high;
//        Low = low;
//    }

//    /// <summary>从Int64构建Int128</summary>
//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    public Int128(long value)
//    {
//        High = value < 0 ? -1 : 0; // 符号扩展
//        Low = value;
//    }

//    /// <summary>从UInt64构建Int128</summary>
//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    public Int128(ulong value)
//    {
//        High = 0;
//        Low = (long)value;
//    }

//    /// <summary>从Int32构建Int128</summary>
//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    public Int128(int value) : this((long)value) { }
//    #endregion

//    #region 基础属性
//    /// <summary>是否为负数</summary>
//    public bool IsNegative => (High & 0x8000000000000000) != 0;

//    /// <summary>是否为零</summary>
//    public bool IsZero => High == 0 && Low == 0;

//    /// <summary>符号：-1（负）、0（零）、1（正）</summary>
//    public int Sign
//    {
//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        get
//        {
//            if (IsNegative) return -1;
//            return IsZero ? 0 : 1;
//        }
//    }
//    #endregion

//    #region 算术运算（核心）
//    // 加法（处理进位）
//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    public static Int128 operator +(Int128 a, Int128 b)
//    {
//        ulong lowSum = (ulong)a.Low + (ulong)b.Low;
//        long carry = lowSum > ulong.MaxValue ? 1 : 0;
//        long highSum = a.High + b.High + carry;
//        return new Int128(highSum, (long)lowSum);
//    }

//    // 减法（转换为加相反数）
//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    public static Int128 operator -(Int128 a, Int128 b) => a + (-b);

//    // 一元负号（取反加1）
//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    public static Int128 operator -(Int128 value)
//    {
//        if (value == MinValue)
//            throw new OverflowException("Int128最小值无法取反");

//        ulong low = ~(ulong)value.Low + 1;
//        long carry = low == 0 ? 1 : 0;
//        long high = ~value.High + carry;
//        return new Int128(high, (long)low);
//    }

//    // 一元正号
//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    public static Int128 operator +(Int128 value) => value;

//    // 乘法（128位乘法，基于64位分段计算）
//    public static Int128 operator *(Int128 a, Int128 b)
//    {
//        // 拆分64位为高32位和低32位，避免溢出
//        ulong aLow = (ulong)a.Low;
//        ulong aHigh = (ulong)a.High;
//        ulong bLow = (ulong)b.Low;
//        ulong bHigh = (ulong)b.High;

//        // 分段乘法：(aH*2^32 + aL) * (bH*2^32 + bL) = aH*bH*2^64 + (aH*bL + aL*bH)*2^32 + aL*bL
//        ulong aLbL = aLow & 0xFFFFFFFF * (bLow & 0xFFFFFFFF);
//        ulong aLbH = aLow & 0xFFFFFFFF * (bLow >> 32);
//        ulong aHbL = (aLow >> 32) * (bLow & 0xFFFFFFFF);
//        ulong aHbH = (aLow >> 32) * (bLow >> 32);

//        ulong sum0 = aLbL;
//        ulong sum1 = aLbH + aHbL + (sum0 >> 32);
//        ulong sum2 = aHbH + (sum1 >> 32);
//        ulong sum3 = (sum2 >> 32);

//        // 处理高64位部分
//        Int128 highPart = new Int128((long)(aHigh * bLow + aLow * bHigh) + (long)sum2, (long)(sum1 << 32 | sum0 & 0xFFFFFFFF));
//        highPart += new Int128((long)sum3, 0);

//        // 符号修正
//        if (a.IsNegative != b.IsNegative && highPart != Zero)
//            highPart = -highPart;

//        return highPart;
//    }

//    // 除法（简化版：仅支持Int128 / Int64，工程常用场景）
//    public static Int128 operator /(Int128 a, long b)
//    {
//        if (b == 0) throw new DivideByZeroException();
//        if (a == MinValue && b == -1) throw new OverflowException("Int128最小值除以-1溢出");

//        bool negative = a.IsNegative != (b < 0);
//        Int128 absA = a.IsNegative ? -a : a;
//        ulong absB = (ulong)Math.Abs(b);

//        ulong remainder = 0;
//        ulong high = (ulong)absA.High;
//        ulong low = (ulong)absA.Low;

//        // 逐位计算商（128位除法转64位迭代）
//        ulong quotientHigh = 0;
//        ulong quotientLow = 0;
//        for (int i = 0; i < 128; i++)
//        {
//            remainder = (remainder << 1) | ((high >> 63) & 1);
//            high = (high << 1) | (low >> 63);
//            low <<= 1;

//            quotientLow = (quotientLow << 1) | (remainder >= absB ? 1UL : 0UL);
//            if (remainder >= absB)
//                remainder -= absB;
//        }

//        Int128 result = new Int128((long)quotientHigh, (long)quotientLow);
//        return negative ? -result : result;
//    }

//    // 取模
//    public static Int128 operator %(Int128 a, Int128 b)
//    {
//        if (b.IsZero) throw new DivideByZeroException();
//        Int128 quotient = a / b;
//        return a - quotient * b;
//    }
//    #endregion

//    #region 位移运算
//    // 左移（算术左移，补0）
//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    public static Int128 operator <<(Int128 value, int shift)
//    {
//        if (shift < 0 || shift >= 128) return Zero;
//        if (shift == 0) return value;

//        if (shift <= 63)
//        {
//            long newLow = value.Low << shift;
//            long newHigh = (value.High << shift) | (value.Low >> (64 - shift));
//            return new Int128(newHigh, newLow);
//        }
//        else
//        {
//            long newHigh = value.Low << (shift - 64);
//            return new Int128(newHigh, 0);
//        }
//    }

//    // 右移（算术右移，补符号位）
//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    public static Int128 operator >>(Int128 value, int shift)
//    {
//        if (shift < 0 || shift >= 128) return value.IsNegative ? NegativeOne : Zero;
//        if (shift == 0) return value;

//        if (shift <= 63)
//        {
//            long newHigh = value.High >> shift;
//            long newLow = (value.Low >> shift) | (value.High << (64 - shift));
//            // 符号位填充
//            if (value.IsNegative)
//                newLow |= (1L << (64 - shift)) - 1;
//            return new Int128(newHigh, newLow);
//        }
//        else
//        {
//            long newHigh = value.IsNegative ? -1 : 0;
//            return new Int128(newHigh, newHigh);
//        }
//    }
//    #endregion

//    #region 比较运算
//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    public static bool operator ==(Int128 a, Int128 b) => a.High == b.High && a.Low == b.Low;

//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    public static bool operator !=(Int128 a, Int128 b) => !(a == b);

//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    public static bool operator <(Int128 a, Int128 b)
//    {
//        if (a.IsNegative != b.IsNegative) return a.IsNegative;
//        if (a.High != b.High) return a.High < b.High;
//        return a.Low < b.Low;
//    }

//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    public static bool operator >(Int128 a, Int128 b) => b < a;

//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    public static bool operator <=(Int128 a, Int128 b) => !(a > b);

//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    public static bool operator >=(Int128 a, Int128 b) => !(a < b);
//    #endregion

//    #region 类型转换
//    // 隐式转换：Int64 -> Int128
//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    public static implicit operator Int128(long value) => new Int128(value);

//    // 隐式转换：Int32 -> Int128
//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    public static implicit operator Int128(int value) => new Int128(value);

//    // 显式转换：Int128 -> Int64（可能溢出）
//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    public static explicit operator long(Int128 value)
//    {
//        if (value.High != 0 && !(value.High == -1 && value.Low < 0))
//            throw new OverflowException("Int128值超出Int64范围");
//        return value.Low;
//    }

//    // 显式转换：Int128 -> Int32（可能溢出）
//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    public static explicit operator int(Int128 value) => (int)(long)value;
//    #endregion

//    #region 字符串转换
//    /// <summary>转换为十进制字符串</summary>
//    public override string ToString()
//    {
//        if (IsZero) return "0";
//        if (this == MinValue) return "-170141183460469231731687303715884105728"; // MinValue的固定值

//        Int128 absValue = IsNegative ? -this : this;
//        char[] digits = new char[40]; // 128位最大十进制数约39位
//        int index = digits.Length;

//        while (absValue > Zero)
//        {
//            Int128 remainder = absValue % 10;
//            absValue = absValue / 10;
//            digits[--index] = (char)('0' + (long)remainder);
//        }

//        string result = new string(digits, index, digits.Length - index);
//        return IsNegative ? "-" + result : result;
//    }

//    /// <summary>从字符串解析Int128</summary>
//    public static Int128 Parse(string s)
//    {
//        if (string.IsNullOrEmpty(s)) throw new ArgumentNullException(nameof(s));
//        if (!TryParse(s, out var result))
//            throw new FormatException("无效的Int128字符串格式");
//        return result;
//    }

//    /// <summary>尝试从字符串解析Int128</summary>
//    public static bool TryParse(string s, out Int128 result)
//    {
//        result = Zero;
//        if (string.IsNullOrEmpty(s)) return false;

//        // 处理符号
//        int startIndex = 0;
//        bool isNegative = false;
//        if (s[0] == '-')
//        {
//            isNegative = true;
//            startIndex = 1;
//        }
//        else if (s[0] == '+')
//        {
//            startIndex = 1;
//        }

//        // 逐位解析
//        for (int i = startIndex; i < s.Length; i++)
//        {
//            if (!char.IsDigit(s[i])) return false;
//            int digit = s[i] - '0';
//            result = result * 10 + digit;
//        }

//        result = isNegative ? -result : result;
//        return true;
//    }
//    #endregion

//    #region 接口实现
//    public bool Equals(Int128 other) => this == other;
//    public override bool Equals(object obj) => obj is Int128 other && Equals(other);
//    public override int GetHashCode() => HashCode.Combine(High, Low);

//    public int CompareTo(Int128 other)
//    {
//        if (this < other) return -1;
//        if (this > other) return 1;
//        return 0;
//    }
//    #endregion

//    #region 辅助方法
//    /// <summary>安全转换为Int64（溢出时返回默认值）</summary>
//    public long ToInt64OrDefault(long defaultValue = 0)
//    {
//        try { return (long)this; }
//        catch { return defaultValue; }
//    }

//    /// <summary>检查是否在Int64范围内</summary>
//    public bool IsInInt64Range()
//    {
//        return High == 0 || (High == -1 && Low < 0);
//    }
//    #endregion
//}