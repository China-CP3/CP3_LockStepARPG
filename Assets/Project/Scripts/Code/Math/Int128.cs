using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Numerics; 
using System.Text;    

public struct Int128
{
    public ulong low64;
    public ulong high64;

    public Int128(ulong high64, ulong low64)
    {
        this.high64 = high64;
        this.low64 = low64;
    }

    public static Int128 operator +(Int128 a, Int128 b)
    {
        ulong tempLow = a.low64 + b.low64;
        byte lowToHighCarry = 0;
        if (tempLow < a.low64)
        {
            lowToHighCarry = 1;
        }
        ulong tempHigh = a.high64 + b.high64 + lowToHighCarry;

        return new Int128(tempHigh, tempLow);
    }

    public override string ToString()
    {

        byte[] bytes = new byte[17]; 
        System.Buffer.BlockCopy(System.BitConverter.GetBytes(low64), 0, bytes, 0, 8);
        System.Buffer.BlockCopy(System.BitConverter.GetBytes(high64), 0, bytes, 8, 8);

        BigInteger bigInt = new BigInteger(bytes);

        return bigInt.ToString();
    }
}
