public class BufferedBitReader
{
    private byte[] data;
    private int bytePos;
    private int bitPos = 0;
    private bool reverseBits;
    private bool reverseBytes;

    private const int MAX_BIT = 7;

    public bool isReverseBits(){ return reverseBits; }

    public void SetReverseBits(bool reverse_bits) { reverseBits = reverse_bits; }

    public bool isReverseBytes() { return reverseBytes; }

    public void SetReverseBytes(bool reverse_bytes) { reverseBytes = reverse_bytes; }

    public byte[] GetData() { return data; }

    public int GetBytePos() { return bytePos; }

    public BufferedBitReader(byte[] data, int pos)
    {
        this.data = data;
        this.bytePos = pos;
    }

    public int ReadBit()
    {
        int readBitPos = isReverseBits() ? (MAX_BIT - this.bitPos) : this.bitPos;
        int readBytePos = isReverseBytes() ? (this.data.Length - 1 - this.bytePos) : this.bytePos;

        if (this.bitPos == MAX_BIT)
        {
            this.bitPos = 0;
            this.bytePos++;
        }
        else
        {
            this.bitPos++;
        }

        return (this.data[readBytePos] >> readBitPos) & 0x01;
    }

    public int ReadBits(int count)
    {
        int num = 0;

        for (int i = 0; i < count; i++)
            num = (num << 1) | ReadBit();

        return num;
    }

    public bool HasRemaining()
    {
        return this.data.Length > this.bytePos;
    }

    public int GetRemainingBits()
    {
        if (this.bytePos >= this.data.Length)
            return 0;

        return (0x08 * (this.data.Length - this.bytePos - 1)) + (MAX_BIT - this.bitPos);
    }
}
