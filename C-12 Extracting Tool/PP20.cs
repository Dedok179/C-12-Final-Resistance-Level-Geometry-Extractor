public class PP20
{
    private const int INTEGER_SIZE = 4;
    private const int BIT_TRUE = 1;
    private const int BIT_FALSE = 0;
    private const int BITS_PER_BYTE = 8;

    private const int OFFSET_BIT_OPTIONS = 4;
    private const int HAS_RAW_DATA_BIT = BIT_FALSE;
    private const int INPUT_BIT_LENGTH = 2;
    private const int INPUT_CONTINUE_WRITING_BITS = 3;
    private const int OFFSET_BIT_LENGTH = 3;
    private const int SAFETY_MARGIN_CONSTANT = 4;
    private const int OFFSET_CONTINUE_WRITING_BITS = 7;
    private const int OPTIONAL_BITS_SMALL_OFFSET = 7;
    private const int MINIMUM_DECODE_DATA_LENGTH = 2;
    private const int COMPRESSION_LEVEL_BITS = 2;

    public static UnpackResult UnpackData(byte[] data)
    {
        int[] offsetBitLengths = GetOffsetBitLengths(data);
        int skip = data[data.Length - 1] & 0xFF;
        byte[] outBuffer = new byte[GetDecodedDataSize(data)];
        int outPos = outBuffer.Length;

        BufferedBitReader r = new BufferedBitReader(data, 4);

        r.SetReverseBytes(true);
        r.ReadBits(skip);

        int byteMargin = 0;

        while (outPos > 0)
            outPos = DecodeSegment(r, outBuffer, outPos, offsetBitLengths, byteMargin);

        return new UnpackResult(outBuffer, byteMargin);
    }

    public class UnpackResult
    {
        public UnpackResult(byte[] buffer, int margin)
        {
            unpackedBytes = buffer;
            minimumByteMargin = margin;
        }

        public byte[] unpackedBytes;
        public int minimumByteMargin;

        public int GetSafetyMarginWordCount()
        {
            return 2 + (minimumByteMargin / INTEGER_SIZE);
        }
    }

    private static int[] GetOffsetBitLengths(byte[] data)
    {
        int[] a = new int[OFFSET_BIT_OPTIONS];

        for (int i = 0; i < OFFSET_BIT_OPTIONS; i++)
            a[i] = data[i + OFFSET_BIT_OPTIONS];

        return a;
    }

    private static int GetDecodedDataSize(byte[] data)
    {
        int i = data.Length - 2;

        return (data[i - 2] & 0xFF) << 16 | (data[i - 1] & 0xFF) << 8 | data[i] & 0xFF;
    }

    private static int DecodeSegment(BufferedBitReader r, byte[] outBuffer, int outPos, int[] offsetBitLengths, int byteMargin)
    {
        if (r.ReadBit() == HAS_RAW_DATA_BIT)
            outPos = CopyFromInput(r, outBuffer, outPos, byteMargin);

        if (outPos > 0)
            outPos = CopyFromDecoded(r, outBuffer, outPos, offsetBitLengths, byteMargin);

        return outPos;
    }

    private static int CopyFromInput(BufferedBitReader r, byte[] outBuffer, int bytePos, int byteMargin)
    {
        int count = 1, countInc;

        while ((countInc = r.ReadBits(INPUT_BIT_LENGTH)) == INPUT_CONTINUE_WRITING_BITS)
            count += INPUT_CONTINUE_WRITING_BITS;

        UpdateByteMargin(r, outBuffer, bytePos, byteMargin);

        for (count += countInc; count > 0; count--)
            outBuffer[--bytePos] = (byte)r.ReadBits(BITS_PER_BYTE);

        return bytePos;
    }

    private static int CopyFromDecoded(BufferedBitReader r, byte[] outBuffer, int bytePos, int[] offsetBitLengths, int byteMargin)
    {
        int compressionLevel = r.ReadBits(COMPRESSION_LEVEL_BITS);
        bool extraLengthData = (compressionLevel == INPUT_CONTINUE_WRITING_BITS);
        int offBits = extraLengthData && r.ReadBit() == BIT_FALSE ? OPTIONAL_BITS_SMALL_OFFSET : offsetBitLengths[compressionLevel];
        int off = r.ReadBits(offBits);

        int copyLength = compressionLevel + MINIMUM_DECODE_DATA_LENGTH;

        if (extraLengthData)
        {
            int lastLengthBits;

            do
            {
                lastLengthBits = r.ReadBits(OFFSET_BIT_LENGTH);
                copyLength += lastLengthBits;
            } while (lastLengthBits == OFFSET_CONTINUE_WRITING_BITS);
        }

        UpdateByteMargin(r, outBuffer, bytePos, byteMargin);

        for (int i = 0; i < copyLength; i++, bytePos--)
            outBuffer[bytePos - 1] = outBuffer[bytePos + off];

        return bytePos;
    }

    private static void UpdateByteMargin(BufferedBitReader r, byte[] outBuffer, int bytePos, int byteMargin)
    {
        int writerPosFromOutputBufferEnd = outBuffer.Length - bytePos;
        int readerPosFromOutputBufferEnd = outBuffer.Length - (r.GetData().Length - SAFETY_MARGIN_CONSTANT - 1) + r.GetBytePos();
        int currentByteMargin = writerPosFromOutputBufferEnd - readerPosFromOutputBufferEnd;

        if (currentByteMargin > byteMargin)
            byteMargin = currentByteMargin;
    }
}

