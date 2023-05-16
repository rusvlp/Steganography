using System;
using System.Collections;

namespace Steganography.Util;

public static class Functions
{
    public static BitArray ByteToBit(byte src) {
        BitArray bitArray = new BitArray(8);
        bool st = false;
        for (int i = 0; i < 8; i++)
        {
            if ((src >> i & 1) == 1) {
                st = true;
            } else st = false;
            bitArray[i] = st;
        }
        return bitArray;
    }

    public static byte BitToByte (BitArray src) {
        byte num = 0;
        for (int i = 0; i < src.Count; i++)
            if (src[i] == true)
                num += (byte)Math.Pow(2, i);
        return num;
    }
    
    
}