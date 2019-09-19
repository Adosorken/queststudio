﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuestStudio
{
    class Util
    {
        private static uint[] CryptTable = {
	        0x697a5,
	        0x6045c,
	        0xab4e2,
	        0x409e4,
	        0x71209,
	        0x32392,
	        0xa7292,
	        0xb09fc,
	        0x4b658,
	        0xaaad5,
	        0x9b9cf,
	        0xa326a,
	        0x8dd12,
	        0x38150,
	        0x8e14d,
	        0x2eb7f,
	        0xe0a56,
	        0x7e6fa,
	        0xdfc27,
	        0xb1301,
	        0x8b4f7,
	        0xa7f70,
	        0xaa713,
	        0x6cc0f,
	        0x6fedf,
	        0x2ec87,
	        0xc0f1c,
	        0x45ca4,
	        0x30df8,
	        0x60e99,
	        0xbc13e,
	        0x4e0b5,
	        0x6318b,
	        0x82679,
	        0x26ef2,
	        0x79c95,
	        0x86ddc,
	        0x99bc0,
	        0xb7167,
	        0x72532,
	        0x68765,
	        0xc7446,
	        0xda70d,
	        0x9d132,
	        0xe5038,
	        0x2f755,
	        0x9171f,
	        0xcb49e,
	        0x6f925,
	        0x601d3,
	        0x5bd8a,
	        0x2a4f4,
	        0x9b022,
	        0x706c3,
	        0x28c10,
	        0x2b24b,
	        0x7cd55,
	        0xca355,
	        0xd95f4,
	        0x727bc,
	        0xb1138,
	        0x9ad21,
	        0xc0aca,
	        0xcd928,
	        0x953e5,
	        0x97a20,
	        0x345f3,
	        0xbdc03,
	        0x7e157,
	        0x96c99,
	        0x968ef,
	        0x92aa9,
	        0xc2276,
	        0xa695d,
	        0x6743b,
	        0x2723b,
	        0x58980,
	        0x66e08,
	        0x51d1b,
	        0xb97d2,
	        0x6caee,
	        0xcc80f,
	        0x3ba6c,
	        0xb0bf5,
	        0x9e27b,
	        0xd122c,
	        0x48611,
	        0x8c326,
	        0xd2af8,
	        0xbb3b7,
	        0xded7f,
	        0x4b236,
	        0xd298f,
	        0xbe912,
	        0xdc926,
	        0xc873f,
	        0xd0716,
	        0x9e1d3,
	        0x48d94,
	        0x9bd91,
	        0x5825d,
	        0x55637,
	        0xb2057,
	        0xbcc6c,
	        0x460de,
	        0xae7fb,
	        0x81b03,
	        0x34d8f,
	        0xc0528,
	        0xc9b59,
	        0x3d260,
	        0x6051d,
	        0x93757,
	        0x8027f,
	        0xb7c34,
	        0x4a14e,
	        0xb12b8,
	        0xe4945,
	        0x28203,
	        0xa1c0f,
	        0xaa382,
	        0x46abb,
	        0x330b9,
	        0x5a114,
	        0xa754b,
	        0xc68d0,
	        0x9040e,
	        0x6c955,
	        0xbb1ef,
	        0x51e6b,
	        0x9ff21,
	        0x51bca,
	        0x4c879,
	        0xdff70,
	        0x5b5ee,
	        0x29936,
	        0xb9247,
	        0x42611,
	        0x2e353,
	        0x26f3a,
	        0x683a3,
	        0xa1082,
	        0x67333,
	        0x74eb7,
	        0x754ba,
	        0x369d5,
	        0x8e0bc,
	        0xabafd,
	        0x6630b,
	        0xa3a7e,
	        0xcdbb1,
	        0x8c2de,
	        0x92d32,
	        0x2f8ed,
	        0x7ec54,
	        0x572f5,
	        0x77461,
	        0xcb3f5,
	        0x82c64,
	        0x35fe0,
	        0x9203b,
	        0xada2d,
	        0xbaebd,
	        0xcb6af,
	        0xc8c9a,
	        0x5d897,
	        0xcb727,
	        0xa13b3,
	        0xb4d6d,
	        0xc4929,
	        0xb8732,
	        0xcce5a,
	        0xd3e69,
	        0xd4b60,
	        0x89941,
	        0x79d85,
	        0x39e0f,
	        0x6945b,
	        0xc37f8,
	        0x77733,
	        0x45d7d,
	        0x25565,
	        0xa3a4e,
	        0xb9f9e,
	        0x316e4,
	        0x36734,
	        0x6f5c3,
	        0xa8ba6,
	        0xc0871,
	        0x42d05,
	        0x40a74,
	        0x2e7ed,
	        0x67c1f,
	        0x28be0,
	        0xe162b,
	        0xa1c0f,
	        0x2f7e5,
	        0xd505a,
	        0x9fcc8,
	        0x78381,
	        0x29394,
	        0x53d6b,
	        0x7091d,
	        0xa2fb1,
	        0xbb942,
	        0x29906,
	        0xc412d,
	        0x3fcd5,
	        0x9f2eb,
	        0x8f0cc,
	        0xe25c3,
	        0x7e519,
	        0x4e7d9,
	        0x5f043,
	        0xbba1b,
	        0x6710a,
	        0x819fb,
	        0x9a223,
	        0x38e47,
	        0xe28ad,
	        0xb690b,
	        0x42328,
	        0x7cf7e,
	        0xae108,
	        0xe54ba,
	        0xba5a1,
	        0xa09a6,
	        0x9cab7,
	        0xdb2b3,
	        0xa98cc,
	        0x5ceba,
	        0x9245d,
	        0x5d083,
	        0x8ea21,
	        0xae349,
	        0x54940,
	        0x8e557,
	        0x83efd,
	        0xdc504,
	        0xa6059,
	        0xb85c9,
	        0x9d162,
	        0x7aeb6,
	        0xbed34,
	        0xb4963,
	        0xe367b,
	        0x4c891,
	        0x9e42c,
	        0xd4304,
	        0x96eaa,
	        0xd5d69,
	        0x866b8,
	        0x83508,
	        0x7baec,
	        0xd03fd,
	        0xda122
};

        public static uint StrToHashKey(string str)
        {
            uint lSeed1 = 0xdeadc0de; // result
            uint lSeed2 = 0x7fed7fed; // tkey
            byte ch = 0;

            str = str.ToUpper();

            for (int i = 0; i < str.Length; i++)
            {
                ch = (byte)str[i];
                lSeed1 = CryptTable[(((0x9C) << 8) + ch) & 0xFF] ^ (lSeed1 + lSeed2);
                lSeed2 = ch + lSeed1 + lSeed2 + (lSeed2 << 5) + 3;
            }
            return lSeed1;
        }
    }
}
