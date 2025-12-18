using System;
using System.Diagnostics;

public readonly struct FixedPoint:IEquatable<FixedPoint>
{
    //最大值 9000万亿
    //小数部分占10位，精度为 1/1000 = 0.001 实际可以保证有效精度2位
    //最大平方根 9500万

    private readonly long scaledValue;//放大后的数
    public readonly long ScaledValue
    {
        get
        {
            return scaledValue;
        }
    }

    private const int ShiftBits = 10;//位移数
    private const long ScaleFactor = 1L << ShiftBits;//乘法放大的倍数 1024

    private const long MaxLongParam = long.MaxValue >> ShiftBits;//能传入构造函数的最大值 大概是
    private const long MinLongParam = long.MinValue >> ShiftBits;
    private const double MaxDoubleParam = (double)long.MaxValue / ScaleFactor;//能传入构造函数的最大值
    private const double MinDoubleParam = (double)long.MinValue / ScaleFactor;//不用位移是因为要丢失小数部分

    public static readonly FixedPoint Zero = new FixedPoint(0);
    public static readonly FixedPoint One = new FixedPoint(ScaleFactor);
    public static readonly FixedPoint MaxValue = new FixedPoint(long.MaxValue);//用于溢出钳制
    public static readonly FixedPoint MinValue = new FixedPoint(long.MinValue);

    #region 构造函数和工厂方法

    /*
     * 假如规定保留1位小数  比如1.25  那么四舍五入就是1.3  放大10倍是12.5  再四舍五入是13  
     * 本质上是对需要保留的最后1位小数四舍五入  也就是0.2
     * 放大后 需要保留的最后1位小数 变成了整数中的个位 也就是2
     * 所以直接加0.5 完全没问题
     */
    public static FixedPoint CreateByFloat(float value)
    {
        return CreateByDouble(value);
    }

    public static FixedPoint CreateByDouble(double value)
    {
        if (value > MaxDoubleParam)
        {
            return MaxValue;
        }
        if (value < MinDoubleParam)
        {
            return MinValue;
        }

        return new FixedPoint((long)Math.Round(value * ScaleFactor));
    }

    //public FixedPoint(int value)
    //{   
    //    scaledValue = (long)value << ShiftBits;//C#规则 先位运算的值 决定容器大小 假如这里不(long)的话 value就是int 容器大小是32位 后位运算的值位移数会按32取模 导致long的值丢失
    //}

    public static FixedPoint CreateByInt(int value)
    {
        if (value > MaxLongParam)//避免未来修改了ShiftBits又来改这里 一劳永逸
        {
            return MaxValue;
        }
        if (value < MinLongParam)
        {
            return MinValue;
        }

        return new FixedPoint((long)value << ShiftBits);
    }

    public static FixedPoint CreateByLong(long value)
    {
        if (value > MaxLongParam)
        {
            return MaxValue;
        }
        if (value < MinLongParam)
        {
            return MinValue;
        }

        return new FixedPoint(value << ShiftBits);
    }

    /// <summary>
    /// 用FixedPoint的ScaledValue创建1个新的FixedPoint
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static FixedPoint CreateByScaledValue(long scaledValue)
    {
        return new FixedPoint(scaledValue);
    }

    /// <summary>
    /// 已经放大过的 long 值创建一个 FixedPoint 实例。
    /// </summary>
    private FixedPoint(long scaledValue)
    {
        this.scaledValue = scaledValue;
    }

    #endregion

    #region 常用接口

    public override string ToString()
    {
        return ((double)this).ToString();
    }

    public bool Equals(FixedPoint other)
    {
        return this.scaledValue == other.scaledValue;
    }

    //根本原因  如果结构体不重写就会走笨重的反射  如果类不重写 会走默认的equals fp参数的equals 进行引用比较
    public override bool Equals(object other)
    {
        if(other is FixedPoint)
        {
            return this.Equals((FixedPoint)other);
        }
        return false;
    }

    public override int GetHashCode()
    {
        return scaledValue.GetHashCode();
    }

    #endregion

    #region 四则运算 重载+ - * /
    public static FixedPoint operator +(FixedPoint a, FixedPoint b)
    {
        long valA = a.scaledValue;
        long valB = b.scaledValue;

        // 如果两个数符号相同，才可能溢出
        if ((valA > 0 && valB > 0 && valA > long.MaxValue - valB) ||
            (valA < 0 && valB < 0 && valA < long.MinValue - valB))
        {
            return valA > 0 ? MaxValue : MinValue;
        }

        return new FixedPoint(valA + valB);
    }

    public static FixedPoint operator -(FixedPoint a, FixedPoint b)
    {
        // 溢出只可能在 a 和 b 符号相反时发生

        long valA = a.scaledValue;
        long valB = b.scaledValue;

        // 正数 - 负数 (等同于相加)
        if (valA > 0 && valB < 0 && valA > long.MaxValue + valB)// valB是负数，所以+valB等同于减去一个正数
        {
            return MaxValue;
        }
        // 负数 - 正数 (等同于相减)
        if (valA < 0 && valB > 0 && valA < long.MinValue + valB)// valB是正数，所以+valB等同于负数减去一个正数
        {
            return MinValue;
        }

        return new FixedPoint(valA - valB);
    }

    public static FixedPoint operator -(FixedPoint a)
    {
        return new FixedPoint(-a.scaledValue);
    }

    //* A* B
    //= (A_real * ScaleFactor) * (B_real * ScaleFactor)
    //= (A_real * B_real) * ScaleFactor * ScaleFactor

    //结果被放大了 两次 ScaleFactor！
    //正确结果应该是(A_real* B_real) * ScaleFactor。
    //所以，在计算完 a.scaledValue * b.scaledValue 之后，我们必须除以一个 ScaleFactor 来把它“拉回”到正确的放大倍数。
    //也就是 a * b / c 但是2个long相乘容易溢出 调换顺序 a / c * b 先缩小后放大也不行 会丢失精度 哪怕它俩数学上相等

    public static FixedPoint operator *(FixedPoint a, FixedPoint b)
    {
        decimal temp = (decimal)a.scaledValue * b.scaledValue;//decimal 不支持位运算
        temp = temp / ScaleFactor;

        if(temp > long.MaxValue)
        {
            return MaxValue;
        }

        if(temp < long.MinValue)
        {
            return MinValue;
        }

        return new FixedPoint((long)Math.Round(temp));
    }

    //A / B
    //= (A_real* ScaleFactor) / (B_real* ScaleFactor)
    //= A_real / B_real
    //两个 ScaleFactor 公倍数直接被约掉了 所以需要乘以1个ScaleFactor
    //A_real / B_real * ScaleFactor 
    //先除法的话 long会丢失精度 比如7/2=3 所以调换顺序  A_real * ScaleFactor / B_real //小心也会有乘法溢出问题
    //A_real * ScaleFactor / B_real 乘法以后再除法 会不会产生小数？会的 但是毫不影响 举个例子
    //10 / 3 * 10 = 3 * 10 = 30 丢失了0.3
    // 10 * 10 / 3 = 33.3 这时候33就是10/3放大10倍后的结果 0.3是产生的小数 因为是long会自动丢失 完全没影响 
    /*
     * 10 * 10 / 3 = 33.3 为什么要乘以10  假设规则定为保留1位小数  不管结果在小数点后面有多少位 只放大10倍 只保留1位 
     * 对分子乘以10等于是把结果乘以10倍 
     * 原本是 10/3 = 3.3 会丢失0.3变成3
     * 放大10倍 10 * 10 / 3 = 33.3 原本3.3变成了33.3 小数的0.3已经变成了整数 此时小数点后面的数 全是之前本身就要摒弃 不需要的  
     */
    public static FixedPoint operator /(FixedPoint a, FixedPoint b)
    {
        if (b.scaledValue == 0)//分母不能为0
        {
            throw new DivideByZeroException("FixedPoint operation a/b b is Zero!!!");
        }

        decimal temp = (decimal)a.scaledValue * ScaleFactor;
        temp = temp / b.scaledValue;//小数不能位移运算  负数位移运算会向负无穷取整 干脆统一用除法

        if (temp > long.MaxValue)
        {
            return MaxValue;
        }

        if (temp < long.MinValue)
        {
            return MinValue;
        }

        return new FixedPoint((long)Math.Round(temp));
    }

    #endregion

    #region 强制转换为 int float double long 
    public static explicit operator int(FixedPoint fixedPoint)
    {
        //这里不用位运算是因为会对负数向负无穷取整 /号是向0取整
        return (int)(fixedPoint.scaledValue / ScaleFactor);

        //优化为位移运算 //后来发现可能是反向优化 编译器会检查 把能够转换为位运算的情况自动转换 比如乘除的是2的次幂
        //但是有了ifesle cpu速度会慢一点 遇到分支时CPU只能“猜测”走哪条路，一旦猜错，之前做的预处理就全部作废，导致严重的时间浪费。
        //if(fixedPoint.scaledValue >= 0)
        //{
        //    return (int)fixedPoint.scaledValue >> ShiftBits;
        //}
        //else
        //{
        //    return (int) -(-fixedPoint.scaledValue >> ShiftBits);
        //}
    }

    public static explicit operator long(FixedPoint fixedPoint)
    {
        //这里不用位运算是因为会对负数向负无穷取整 /号是向0取整
        return fixedPoint.scaledValue / ScaleFactor;

        //优化为位移运算 //后来发现可能是反向优化 编译器会检查 把能够转换为位运算的情况自动转换 比如乘除的是2的次幂
        //但是有了ifesle cpu速度会慢一点 遇到分支时CPU只能“猜测”走哪条路，一旦猜错，之前做的预处理就全部作废，导致严重的时间浪费。
        //if (fixedPoint.scaledValue >= 0)
        //{
        //    return (long)fixedPoint.scaledValue >> ShiftBits;
        //}
        //else
        //{
        //    return (long)-(-fixedPoint.scaledValue >> ShiftBits);
        //}
    }

    public static explicit operator float(FixedPoint fixedPoint)
    {
        return (float)((double)fixedPoint.scaledValue / ScaleFactor);//这里不用位运算是因为会丢失小数
    }

    public static explicit operator double(FixedPoint fixedPoint)
    {
        return (double)fixedPoint.scaledValue / ScaleFactor;
    }
    #endregion

    #region 重载运算符 == != > < >= <=
    public static bool operator ==(FixedPoint a, FixedPoint b)
    {
        return a.scaledValue == b.scaledValue;
    }

    public static bool operator !=(FixedPoint a, FixedPoint b)
    {
        return a.scaledValue != b.scaledValue;
    }

    public static bool operator >(FixedPoint a, FixedPoint b)
    {
        return a.scaledValue > b.scaledValue;
    }

    public static bool operator <(FixedPoint a, FixedPoint b)
    {
        return a.scaledValue < b.scaledValue;
    }

    public static bool operator <=(FixedPoint a, FixedPoint b)
    {
        return a.scaledValue <= b.scaledValue;
    }

    public static bool operator >=(FixedPoint a, FixedPoint b)
    {
        return a.scaledValue >= b.scaledValue;
    }
    #endregion
}
