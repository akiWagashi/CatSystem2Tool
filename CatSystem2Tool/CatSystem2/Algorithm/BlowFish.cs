
namespace CatSystem2.Algorithm;
internal class BlowFish
{
    public static readonly int PArraySize = 18;

    public static readonly int SBoxCount = 4;

    public static readonly int SBoxSize = 256;

    private uint[] PArray { get; set; } = new uint[PArraySize];

    private uint[,] SBox  { get; init; } = new uint[SBoxCount, SBoxSize];

    public BlowFish(uint[] key)
    {
        for (uint i = 0; i < 18; i++)
        {
            var bytes = BitConverter.GetBytes(key[i % key.Length]);
            Array.Reverse<byte>(bytes);
            PArray[i] = BlowFishORIG.PArray[i] ^ BitConverter.ToUInt32(bytes, 0);

        }

        SBox  = (uint[,])BlowFishORIG.SBox.Clone();

        uint dataL = 0;
        uint dataR = 0;

        for (uint i = 0; i < PArraySize;)
        {
            Encrypt(ref dataL, ref dataR);
            PArray[i++] = dataR;
            PArray[i++] = dataL;
        }

        for (uint x = 0; x < SBoxCount; ++x)
        {
            for (uint y = 0; y < SBoxSize;)
            {
                Encrypt(ref dataL, ref dataR);
                SBox [x, y++] = dataR;
                SBox [x, y++] = dataL;
            }

        }
    }

    public void Encrypt(ref uint dataL, ref uint dataR)
    {
        uint[] temp = new uint[PArraySize];

        temp[0] = dataL;

        temp[1] = dataR ^ PArray[0];

        for (uint i = 2; i < temp.Length; ++i)
        {
            var bytes = BitConverter.GetBytes(temp[i - 1]);
            temp[i] = ((SBox [0, bytes[3]] + SBox [1, bytes[2]]) ^ SBox [2, bytes[1]]) + SBox [3, bytes[0]];
            temp[i] ^= PArray[i - 1];
            temp[i] ^= temp[i - 2];

        }

        dataL = temp[17];
        dataR = temp[16] ^ PArray[17];

        return;
    }

    public void Decrypt(ref uint dataL, ref uint dataR)
    {
        uint[] temp = new uint[PArraySize];

        temp[0] = dataR;

        temp[1] = dataL ^ PArray[17];

        for (uint i = 2; i < temp.Length; ++i)
        {
            var bytes = BitConverter.GetBytes(temp[i - 1]);
            temp[i] = ((SBox [0, bytes[3]] + SBox [1, bytes[2]]) ^ SBox [2, bytes[1]]) + SBox [3, bytes[0]];
            temp[i] ^= PArray[18 - i];
            temp[i] ^= temp[i - 2];
        }

        dataL = temp[16] ^ PArray[0];
        dataR = temp[17];

    }
}

