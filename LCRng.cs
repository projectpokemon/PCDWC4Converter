namespace PCDWC4Converter;

public class LCRng
{
    private uint _seed;

    public LCRng(uint seed = 0) => _seed = seed;

    public uint Next() => _seed = _seed * 0x41C64E6Du + 0x00006073u;

    public uint NextH() => Next() >> 0x10;

    public uint Prev() => _seed = _seed * 0xEEB9EB65u + 0xA3561A1u;
    
    public uint PrevH() => Prev() >> 0x10;
}
