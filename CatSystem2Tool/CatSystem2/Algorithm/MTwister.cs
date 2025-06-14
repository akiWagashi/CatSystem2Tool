namespace CatSystem2.Algorithm;

internal class MTwister
{
    private const uint MTLength = 0x270;
    private uint[] MT { get; set; } = new uint[MTLength];

    public MTwister(uint seed)
    {
        this.Init(seed);
    }

    private void Init(uint seed)
    {
        const uint calcFactor = 0x10DCD;

        for (uint i = 0; i < MTLength; ++i)
        {
            var temp = seed * calcFactor + 1;
            MT[i] = (temp >> 0x10) ^ (seed & 0xFFFF0000);
            seed = temp * calcFactor + 1;
        }
    }

    private void Twist()
    {

        for (uint i = 0; i < 0xE3; ++i)
        {
            uint temp = (MT[i + 1] ^ MT[i]) & 0x7FFFFFFF ^ MT[i];

            temp = temp >> 1 ^ (temp % 2 == 0 ? 0 : 0x9908B0DF);

            MT[i] = temp ^ MT[397 + i];

        }

        for (uint i = 0xE3; i < MTLength - 1; ++i)
        {
            uint temp = (MT[i + 1] ^ MT[i]) & 0x7FFFFFFF ^ MT[i];

            temp = temp >> 1 ^ (temp % 2 == 0 ? 0 : 0x9908B0DF);

            MT[i] = temp ^ MT[i - 0xE3];
        }

        {
            uint temp = (MT[0] ^ MT[MTLength - 1]) & 0x7FFFFFFF ^ MT[MTLength - 1];

            temp = temp >> 1 ^ (temp % 2 == 0 ? 0 : 0x9908B0DF);

            MT[MTLength - 1] = temp ^ MT[396];

        }

    }

    public uint Rand()
    {
        Twist();

        uint result = MT[0] >> 0xB ^ MT[0];
        result = (result & 0xFF3A58AD) << 7 ^ result;
        result = (result & 0xFFFFDF8C) << 0xF ^ result;
        result = result >> 0x12 ^ result;

        return result;
    }
}

