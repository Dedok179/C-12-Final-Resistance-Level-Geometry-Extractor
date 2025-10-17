using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using C12_Obj;

namespace C12_Lib
{
    public class C12
    {       
        class TextureInfo
        {
            public TextureInfo(short x, short y, short w, short h, long pix_ofs, short tex_index, short color_mode, short clut_ptr, short dat1, short dat2, short dat3)
            {
                VRAM_X = x;
                VRAM_Y = y;
                TEX_W = w;
                TEX_H = h;
                PIX_OFS = pix_ofs;
                TEX_INDEX = tex_index;
                COLOR_MODE = color_mode;
                CLUT_X = (short)(((clut_ptr >> 0) & 0x3F) * 16);
                CLUT_Y = (short)((clut_ptr >> 6) & 0x1FF);
                DAT1 = dat1;
                DAT2 = dat2;
                DAT3 = dat3;
            }

            public short VRAM_X;
            public short VRAM_Y;
            public short TEX_W;
            public short TEX_H;
            public long PIX_OFS;
            public short TEX_INDEX;
            public short COLOR_MODE;
            public short CLUT_X;
            public short CLUT_Y;
            public short DAT1;
            public short DAT2;
            public short DAT3;
        }

        class PaletteInfo
        {
            public PaletteInfo(short x, short y, short w, short h, long color_ofs)
            {
                VRAM_X = x;
                VRAM_Y = y;
                PAL_W = w;
                PAL_H = h;
                COLOR_OFS = color_ofs;
            }

            public short VRAM_X;
            public short VRAM_Y;
            public short PAL_W;
            public short PAL_H;
            public long COLOR_OFS;
        }

        public struct ClutVRAM
        {
            public ClutVRAM(short x, short y)
            {
                X = x;
                Y = y;
            }

            public short X;
            public short Y;
        }

        static int tF = 10000; //Vertices translate factor
        static int PTR_1B4300 = 0x1b4300; //For resolve flags
        static int PTR_D45DC = 0xd45dc; //Textures ptr area
        static int PTR_16A000 = 0x16a000; //Uv data
        
        static List<string> NewMat(string page)
        {
            List<string> t = new List<string>();

            t.Add($"newmtl MAT_{page.ToString()}");
            t.Add($"map_Kd PAGE_{page.ToString()}.png\n");

            return t;
        }

        public static void UnpackRGV(string filepath, byte[]rgv, int levelId, int sectionId)
        {            
            string pdir = Directory.GetParent(filepath).FullName;
            string fdir = Path.GetFileNameWithoutExtension(filepath);
            string odir = pdir + Path.DirectorySeparatorChar + $"LEVEL_ID{levelId}_{sectionId}";

            if (!Directory.Exists(odir))
            {
                Directory.CreateDirectory(odir);
            }

            Dictionary<int, TextureInfo> texInfo = new Dictionary<int, TextureInfo>();
            Dictionary<ClutVRAM, PaletteInfo> palInfo = new Dictionary<ClutVRAM, PaletteInfo>();

            int texCount;
            long texOfs;
            int palCount;
            long palOfs;

            using (BufferedBinaryReader r = new BufferedBinaryReader(rgv))
            {
                r.ReadBytes(0x4); //2GRV magic
                texCount = r.ReadInt32();
                texOfs = r.ReadInt32();
                palCount = r.ReadInt32();
                palOfs = r.ReadInt32();

                r.Seek(texOfs, SeekOrigin.Begin);
                GetTexInfo(r, texInfo, texCount);

                r.Seek(palOfs, SeekOrigin.Begin);
                GetPalInfo(r, palInfo, palCount);

                GetTextures(r, texInfo, palInfo, texCount, odir);
            }
        }

        static void GetTextures(BufferedBinaryReader r, Dictionary<int, TextureInfo> tex_info, Dictionary<ClutVRAM, PaletteInfo> pal_info, int count, string odir)
        {            
            Dictionary<string, Bitmap> pages = new Dictionary<string, Bitmap>();
            List<string> pageKeys = new List<string>();

            for (int i = 0; i < count; i++)
            {
                Bitmap texture = new Bitmap(tex_info[i].TEX_W, tex_info[i].TEX_H);
                Color[] palette = new Color[16];
                ClutVRAM clut = new ClutVRAM(tex_info[i].CLUT_X, tex_info[i].CLUT_Y);

                int colorBit = ((tex_info[i].COLOR_MODE >> 7) & 3);
                int pageX = ((tex_info[i].COLOR_MODE >> 0) & 0xF) * 64;
                int pageY = ((tex_info[i].COLOR_MODE >> 4) & 0x1) * 256;
                string pageId = $"{pageX}x{pageY}";
                int shiftX = tex_info[i].VRAM_X - pageX;
                int shiftY = tex_info[i].VRAM_Y - pageY;

                if (shiftY > 255)
                    shiftY -= 256;               

                if (!pages.ContainsKey(pageId))
                {
                    pageKeys.Add(pageId);
                    pages.Add(pageId, new Bitmap(256, 256));
                }

                switch (colorBit)
                {
                    case 0: //4-bit
                        texture = new Bitmap(tex_info[i].TEX_W * 4, tex_info[i].TEX_H);

                        r.Seek(pal_info[clut].COLOR_OFS, SeekOrigin.Begin);
                        palette = GetPalette(r);

                        r.Seek(tex_info[i].PIX_OFS, SeekOrigin.Begin);

                        for (int y = 0; y < tex_info[i].TEX_H; y++)
                        {
                            for (int x = 0; x < (tex_info[i].TEX_W); x++)
                            {
                                var b = r.ReadUInt16();

                                var p1 = (b & 0xF);
                                var p2 = (b & 0xF0) >> 4;
                                var p3 = (b & 0xF00) >> 8;
                                var p4 = (b & 0xF000) >> 12;

                                texture.SetPixel(x * 4, y, palette[p1]);
                                texture.SetPixel((x * 4) + 1, y, palette[p2]);
                                texture.SetPixel((x * 4) + 2, y, palette[p3]);
                                texture.SetPixel((x * 4) + 3, y, palette[p4]);

                                pages[pageId].SetPixel(((x + shiftX) * 4), y + shiftY, palette[p1]);
                                pages[pageId].SetPixel(((x + shiftX) * 4) + 1, y + shiftY, palette[p2]);
                                pages[pageId].SetPixel(((x + shiftX) * 4) + 2, y + shiftY, palette[p3]);
                                pages[pageId].SetPixel(((x + shiftX) * 4) + 3, y + shiftY, palette[p4]);
                            }
                        }

                        break;
                    case 1: //8-bit
                        texture = new Bitmap(tex_info[i].TEX_W * 2, tex_info[i].TEX_H);

                        r.Seek(pal_info[clut].COLOR_OFS, SeekOrigin.Begin);
                        palette = GetPalette(r);

                        r.Seek(tex_info[i].PIX_OFS, SeekOrigin.Begin);

                        for (int y = 0; y < tex_info[i].TEX_H; y++)
                        {
                            for (int x = 0; x < (tex_info[i].TEX_W); x++)
                            {
                                var b = r.ReadUInt16();

                                var p1 = (b & 0xFF);
                                var p2 = (b >> 0x8);

                                texture.SetPixel(x * 2, y, palette[p1]);
                                texture.SetPixel((x * 2) + 1, y, palette[p2]);

                                pages[pageId].SetPixel(((x + shiftX) * 2), y + shiftY, palette[p1]);
                                pages[pageId].SetPixel(((x + shiftX) * 2) + 1, y + shiftY, palette[p2]);
                            }
                        }

                        break;
                    case 2: //16-bit
                        texture = new Bitmap(tex_info[i].TEX_W, tex_info[i].TEX_H);

                        r.Seek(tex_info[i].PIX_OFS, SeekOrigin.Begin);

                        for (int y = 0; y < tex_info[i].TEX_H; y++)
                        {
                            for (int x = 0; x < (tex_info[i].TEX_W); x++)
                            {
                                var b = r.ReadUInt16();

                                texture.SetPixel(x, y, GetColor16(b));

                                pages[pageId].SetPixel(((x + shiftX)), y + shiftY, GetColor16(b));
                            }
                        }

                        break;
                }

                texture.Save(odir + Path.DirectorySeparatorChar + $"TEX_{tex_info[i].TEX_INDEX}.png", ImageFormat.Png);          
            }

            for (int t = 0; t < pageKeys.Count; t++)
                pages[pageKeys[t]].Save(odir + Path.DirectorySeparatorChar + $"PAGE_{pageKeys[t]}.png", ImageFormat.Png);
        }

        static Color[] GetPalette(BufferedBinaryReader r)
        {
            Color[] pal = new Color[16];

            for (int p = 0; p < 16; p++)
            {
                ushort color16 = r.ReadUInt16();

                pal[p] = GetColor16(color16);
            }

            return pal;
        }

        static Color GetColor16(ushort color16)
        {
            var r0 = (color16 & 0x1F);
            var g0 = (color16 & 0x3E0) >> 5;
            var b0 = (color16 & 0x7C00) >> 10;

            var r8 = (byte)(r0 << 3);
            var g8 = (byte)(g0 << 3);
            var b8 = (byte)(b0 << 3);
            byte a = 255;

            if (color16 >> 15 == 0)
            {
                if (r8 == 0 && g8 == 0 && b8 == 0)
                    a = 0;
                else
                    a = 255;
            }

            return Color.FromArgb(a, r8, g8, b8);
        }

        static void GetTexInfo(BufferedBinaryReader r, Dictionary<int, TextureInfo> tex_info, int count)
        {
            short vramX;
            short vramY;
            short texW;
            short texH;
            long pixOfs;
            short texIndex;
            short colorMode;
            short clutPtr;
            short dat1;
            short dat2;
            short dat3;

            for (int i = 0; i < count; i++)
            {
                vramX = r.ReadInt16();
                vramY = r.ReadInt16();
                texW = r.ReadInt16();
                texH = r.ReadInt16();
                pixOfs = r.ReadInt32();
                texIndex = r.ReadInt16();
                colorMode = r.ReadInt16();
                clutPtr = r.ReadInt16();
                dat1 = r.ReadInt16();
                dat2 = r.ReadInt16();
                dat3 = r.ReadInt16();

                tex_info.Add(i, new TextureInfo(vramX, vramY, texW, texH, pixOfs, texIndex, colorMode, clutPtr, dat1, dat2, dat3));
            }
        }

        static void GetPalInfo(BufferedBinaryReader r, Dictionary<ClutVRAM, PaletteInfo> pal_info, int count)
        {
            short vramX;
            short vramY;
            short palW;
            short palH;
            long palOfs;

            for (int i = 0; i < count; i++)
            {
                vramX = r.ReadInt16();
                vramY = r.ReadInt16();
                palW = r.ReadInt16();
                palH = r.ReadInt16();
                palOfs = r.ReadInt32();

                pal_info.Add (new ClutVRAM(vramX, vramY), new PaletteInfo(vramX, vramY, palW, palH, palOfs));
            }
        }

        public static void ExtractLevelByID(byte[] exeBuffer, byte[] mwdBuffer, int levelId, int sectionId, string path)
        {
            using (BufferedBinaryReader exe = new BufferedBinaryReader(exeBuffer))
            {
                using (BufferedBinaryReader mwd = new BufferedBinaryReader(mwdBuffer))
                {
                    //Get level info
                    int levelShift = levelId << 0x1;

                    levelShift += levelId;
                    levelShift = levelShift << 0x3;
                    levelShift -= levelId;
                    levelShift = levelShift << 0x2;

                    int levelPtr = (int)(0x800d1f78 + levelShift);
                    int sectionShift = sectionId << 0x1;

                    sectionShift += sectionId;
                    sectionShift = sectionShift << 0x3;
                    sectionShift -= sectionId;
                    sectionShift = sectionShift << 0x2;

                    exe.Seek(PS1OffsetConvert(levelPtr), SeekOrigin.Begin);

                    byte sectionMax = exe.ReadByte(0xF);

                    if (sectionId >= sectionMax)
                    {
                        MessageBox.Show($"Level section out of range! Maximum count - {sectionMax}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    exe.Seek(PS1OffsetConvert(exe.ReadInt32(0x10) + sectionShift), SeekOrigin.Begin);

                    short resourcesId = exe.ReadInt16(0x0);
                    short texturesId = exe.ReadInt16(0x2);
                    int tablePtr = PS1OffsetConvert(exe.ReadInt32(0x4));

                    //Get files from MWD
                    byte findTextures = 0;
                    byte findResources = 0;
                    int num = 0;

                    exe.Seek(0xB0830, SeekOrigin.Begin);

                    int filename = -1;
                    int flag;
                    int fileType;
                    int fileOfs;
                    int currentSize;
                    int realSize;

                    byte[] texturesBuffer = new byte[] {};
                    byte[] resourcesBuffer = new byte[] {};

                    while ((findTextures + findResources) != 2)
                    {
                        filename = exe.ReadInt32();
                        flag = exe.ReadInt32();
                        fileType = exe.ReadInt32();
                        fileOfs = exe.ReadInt32() * 0x800;
                        exe.ReadInt32(); //zero
                        exe.ReadInt32(); //zero
                        currentSize = exe.ReadInt32();
                        realSize = exe.ReadInt32();
                        exe.ReadInt32(); //unk

                        if (flag != 0x2 & flag != 0x8)
                        {
                            if (num == texturesId)
                            {
                                mwd.Seek(fileOfs, SeekOrigin.Begin);

                                texturesBuffer = mwd.ReadBytes(currentSize);

                                findTextures = 1;
                            }

                            if (num == resourcesId)
                            {
                                mwd.Seek(fileOfs, SeekOrigin.Begin);

                                resourcesBuffer = mwd.ReadBytes(currentSize);

                                if (flag == 0x14 & fileType == -1)
                                    resourcesBuffer = PP20.UnpackData(resourcesBuffer).unpackedBytes;

                                findResources = 1;
                            }                              
                        }

                        num++;
                    }

                    //Operation with data
                    using (BufferedBinaryReader r = new BufferedBinaryReader(resourcesBuffer))
                    {
                        byte[] forgBuffer = new byte[] { };

                        if (r.ReadInt32(0) == 0x47524f46) //FORG
                        {
                            forgBuffer = resourcesBuffer;

                            goto FORG_PROCESSING;
                        }

                        int resIdx;
                        int resType;
                        int resSize;
                        int resCount = r.ReadInt32(0xC);
                                               
                        
                        for (int i = 0; i < resCount; i++)
                        {
                            resIdx = r.ReadInt32();
                            resType = r.ReadInt32();
                            resSize = r.ReadInt32();
                            resCount = r.ReadInt32();
                        
                            if (r.ReadInt32(0) == 0x47524f46) //FORG                       
                                forgBuffer = r.ReadBytes(resSize);
                            else
                                r.ReadBytes(resSize);
                        }

                    FORG_PROCESSING:
                        using (BufferedBinaryReader forg = new BufferedBinaryReader(forgBuffer))
                        {
                            int itemsCount = forg.ReadInt32(0xC);
                            int namesPtr = forg.ReadInt32(0x14);
                            bool polyFound = false;

                            for (int i = 0; i < itemsCount; i++)
                            {
                                if (forg.ReadInt32(namesPtr + (i * 4)) == 0x594c4f50) //POLY
                                    polyFound = true;
                            }

                            if (polyFound != true)
                                return;

                            GetLevelData(exe, forgBuffer, texturesBuffer, tablePtr, levelId, sectionId, path);
                            UnpackRGV(path, texturesBuffer, levelId, sectionId);
                        }
                    }
                }
            }
        }

        private static int PS1OffsetConvert(int offset)
        {
            return (int)(offset - 0x80010000 + 0x800);
        }

        private static void GetLevelData(BufferedBinaryReader exe, byte[] forg, byte[] rgv, int tablePtr, int levelId, int sectionId, string path)
        {
            string filepath = path;
            string pdir = Directory.GetParent(filepath).FullName;
            string fdir = Path.GetFileNameWithoutExtension(filepath);
            string odir = pdir + Path.DirectorySeparatorChar + $"LEVEL_ID{levelId}_{sectionId}" + Path.DirectorySeparatorChar;

            if (!Directory.Exists(odir))
            {
                Directory.CreateDirectory(odir);
            }

            BufferedBinaryReader mem = new BufferedBinaryReader(new byte[0x200000]);

            exe.Seek(0, SeekOrigin.Begin);

            using (BufferedBinaryReader r = new BufferedBinaryReader(rgv))
            {
                r.ReadBytes(0x4); //2GRV magic

                int texCount = r.ReadInt32();
                int texOfs = r.ReadInt32();

                r.Seek(texOfs, SeekOrigin.Begin);

                int uvArea = PTR_16A000;
                int tableArea = PTR_D45DC;

                for (int i = 0; i < texCount; i++)
                {
                    r.ReadInt32(); //VRAM XY
                    r.ReadInt32(); //TEX WH
                    r.ReadInt32(); //PIX OFS
                    short id = r.ReadInt16();
                    short tpage = r.ReadInt16();
                    short clut = r.ReadInt16();
                    short dat = r.ReadInt16();
                    short uv = r.ReadInt16();
                    short sz = r.ReadInt16();

                    mem.WriteInt32(id * 0x4 + tableArea, uvArea);

                    mem.WriteInt16(uvArea, dat);
                    mem.WriteInt16(uvArea + 0x2, sz);
                    mem.WriteInt16(uvArea + 0x4, uv);
                    mem.WriteInt16(uvArea + 0x6, clut);
                    mem.WriteInt16(uvArea + 0x8, (short)(((uv >> 8) << 0x8) | (uv & 0xff) + (sz & 0xff)));
                    mem.WriteInt16(uvArea + 0xA, tpage);
                    mem.WriteInt16(uvArea + 0xC, (short)(((uv >> 8) + (sz >> 0x8) << 0x8) | (uv & 0xff)));
                    mem.WriteInt16(uvArea + 0xE, (short)(((uv >> 8) + (sz >> 0x8) << 0x8) | (uv & 0xff) + (sz & 0xff)));

                    uvArea += 0x10;
                }
            }

            using (BufferedBinaryReader r = new BufferedBinaryReader(forg))
            {
                r.ReadInt32(); //FORG magic
                int dataPtr = r.ReadInt32();
                r.ReadInt32(); //FORG size
                int itemsCount = r.ReadInt32();

                r.Seek(dataPtr, SeekOrigin.Begin);

                int polyPtr = 0;

                for (int i = 0; i < itemsCount; i++)
                {
                    if (r.ReadInt32(0) == 0x594c4f50) //POLY
                        polyPtr = (int)r.Position + 0x8;

                    r.Seek(r.ReadInt32(4), SeekOrigin.Current);
                }

                r.Seek(polyPtr, SeekOrigin.Begin);

                int facesCount = r.ReadInt16();
                int vertsCount = r.ReadInt16();
                int uvCount = r.ReadInt16();
                r.ReadInt16(); //null
                int facesPtr = r.ReadInt32();
                int vertsPtr = r.ReadInt32();
                int uvsPtr = r.ReadInt32();

                r.Seek(0, SeekOrigin.Begin);

                short tempIdx;
                int tempTex;
                ushort flag;
                ushort resolveIdx;
                int resolvePtr;
                byte tempUv;
                int tempFaces = facesPtr;

                for (int i = 0; i < facesCount; i++)
                {
                    tempIdx = exe.ReadInt16(tablePtr + (r.ReadInt16(tempFaces + 0x8) * 0x2));
                    r.WriteInt16(tempFaces + 0x8, tempIdx);
                    tempTex = mem.ReadInt32(tempIdx * 4 + PTR_D45DC);

                    flag = (ushort)(r.ReadInt16(tempFaces + 0xA) & 0x8000);
                    resolveIdx = (ushort)(r.ReadInt16(tempFaces + 0xA) & 0x7fff);

                    if ((flag != 0) && (mem.ReadInt32(PTR_1B4300 + (resolveIdx * 4)) == 0))
                    {
                        mem.WriteInt32(PTR_1B4300 + (resolveIdx * 4), 0x1);

                        resolvePtr = uvsPtr + resolveIdx * 0x8;

                        tempUv = (byte)((r.ReadByte(resolvePtr) * mem.ReadByte(tempTex + 0x2)) / 0xff);
                        r.WriteByte(resolvePtr, (byte)(tempUv + mem.ReadByte(tempTex + 0x4)));
                        tempUv = (byte)((r.ReadByte(resolvePtr + 0x1) * mem.ReadByte(tempTex + 0x3)) / 0xff);
                        r.WriteByte(resolvePtr + 0x1, (byte)(tempUv + mem.ReadByte(tempTex + 0x5)));

                        tempUv = (byte)((r.ReadByte(resolvePtr + 0x2) * mem.ReadByte(tempTex + 0x2)) / 0xff);
                        r.WriteByte(resolvePtr + 0x2, (byte)(tempUv + mem.ReadByte(tempTex + 0x4)));
                        tempUv = (byte)((r.ReadByte(resolvePtr + 0x3) * mem.ReadByte(tempTex + 0x3)) / 0xff);
                        r.WriteByte(resolvePtr + 0x3, (byte)(tempUv + mem.ReadByte(tempTex + 0x5)));

                        tempUv = (byte)((r.ReadByte(resolvePtr + 0x4) * mem.ReadByte(tempTex + 0x2)) / 0xff);
                        r.WriteByte(resolvePtr + 0x4, (byte)(tempUv + mem.ReadByte(tempTex + 0x4)));
                        tempUv = (byte)((r.ReadByte(resolvePtr + 0x5) * mem.ReadByte(tempTex + 0x3)) / 0xff);
                        r.WriteByte(resolvePtr + 0x5, (byte)(tempUv + mem.ReadByte(tempTex + 0x5)));

                        tempUv = (byte)((r.ReadByte(resolvePtr + 0x6) * mem.ReadByte(tempTex + 0x2)) / 0xff);
                        r.WriteByte(resolvePtr + 0x6, (byte)(tempUv + mem.ReadByte(tempTex + 0x4)));
                        tempUv = (byte)((r.ReadByte(resolvePtr + 0x7) * mem.ReadByte(tempTex + 0x3)) / 0xff);
                        r.WriteByte(resolvePtr + 0x7, (byte)(tempUv + mem.ReadByte(tempTex + 0x5)));
                    }

                    tempFaces += 0x10;
                }

                Obj model = new Obj();
                List<string> matData = new List<string>();

                int uvIdx = 1;
                int tS = 256;

                model.matFile = $"mtllib LEVEL{levelId}_{sectionId}_MAT.mtl";
                model.name = $"LEVEL{levelId}_{sectionId}_GEOMETRY";

                List<string> matID = new List<string>();
                Dictionary<string, List<string>> facesDict = new Dictionary<string, List<string>>();

                for (int i = 0; i < vertsCount; i++)
                {
                    var x = (float)(r.ReadInt16(vertsPtr) << 8) / tF;
                    var y = (float)(r.ReadInt16(vertsPtr + 0x2) << 8) / tF;
                    var z = (float)(r.ReadInt16(vertsPtr + 0x4) << 8) / tF;

                    model.vertices.Add($"v {x} {y} {z}");

                    vertsPtr += 0x8;
                }

                byte u1, v1, u2, v2, u3, v3, u4, v4;
                int uv1, uv2, uv3, uv4;

                for (int i = 0; i < facesCount; i++)
                {
                    short[] idx = new short[4];

                    for (int f = 0; f < 4; f++)
                        idx[f] = r.ReadInt16(facesPtr + (f * 0x2));

                    int texPtr = mem.ReadInt32(r.ReadInt16(facesPtr + 0x8) * 0x4 + PTR_D45DC);
                    short uvFlag = r.ReadInt16(facesPtr + 0xA);

                    int pageX = ((mem.ReadInt16(texPtr + 0xA) >> 0) & 0xF) * 64;
                    int pageY = ((mem.ReadInt16(texPtr + 0xA) >> 4) & 0x1) * 256;
                    int typeFlag = r.ReadInt16(facesPtr + 0xE);
                    string pageId = $"{pageX}x{pageY}";

                    if ((uvFlag & 0x8000) == 0)
                    {
                        u1 = (byte)(mem.ReadInt16(texPtr + ((uvFlag & 0x7) * 2)) & 0xFF);
                        v1 = (byte)(mem.ReadInt16(texPtr + ((uvFlag & 0x7) * 2)) >> 0x8);
                        u2 = (byte)(mem.ReadInt16(texPtr + ((uvFlag >> 3 & 0x7) * 2)) & 0xFF);
                        v2 = (byte)(mem.ReadInt16(texPtr + ((uvFlag >> 3 & 0x7) * 2)) >> 0x8);
                        u3 = (byte)(mem.ReadInt16(texPtr + ((uvFlag >> 6 & 0x7) * 2)) & 0xFF);
                        v3 = (byte)(mem.ReadInt16(texPtr + ((uvFlag >> 6 & 0x7) * 2)) >> 0x8);
                        u4 = (byte)(mem.ReadInt16(texPtr + ((uvFlag >> 9 & 0x7) * 2)) & 0xFF);
                        v4 = (byte)(mem.ReadInt16(texPtr + ((uvFlag >> 9 & 0x7) * 2)) >> 0x8);
                    }
                    else
                    {
                        resolvePtr = (int)(uvsPtr + (uvFlag & 0x7fff) * 0x8);

                        uv1 = r.ReadInt32(resolvePtr);
                        uv3 = r.ReadInt32(resolvePtr + 0x4);
                        uv2 = uv1 >> 0x10;
                        uv1 = uv1 & 0xffff;
                        uv4 = uv3 >> 0x10;
                        uv3 = uv3 & 0xffff;

                        u1 = (byte)(uv1 & 0xFF);
                        v1 = (byte)(uv1 >> 0x8);
                        u2 = (byte)(uv2 & 0xFF);
                        v2 = (byte)(uv2 >> 0x8);
                        u3 = (byte)(uv3 & 0xFF);
                        v3 = (byte)(uv3 >> 0x8);
                        u4 = (byte)(uv4 & 0xFF);
                        v4 = (byte)(uv4 >> 0x8);
                    }

                    if (!facesDict.ContainsKey(pageId))
                    {
                        matID.Add(pageId);
                        facesDict.Add(pageId, new List<string>());
                        matData.AddRange(NewMat(pageId));

                        facesDict[pageId].Add($"usemtl MAT_{pageId.ToString()}");
                    }

                    if (typeFlag != 0x4400) //0400
                    {
                        facesDict[pageId].Add($"f {idx[1] + 1}/{uvIdx + 1} {idx[0] + 1}/{uvIdx} {idx[2] + 1}/{uvIdx + 2}");
                        facesDict[pageId].Add($"f {idx[1] + 1}/{uvIdx + 1} {idx[2] + 1}/{uvIdx + 2} {idx[3] + 1}/{uvIdx + 3}");

                        model.uvs.Add($"vt {(float)u1 / tS} {1f - (float)v1 / tS}");
                        model.uvs.Add($"vt {(float)u2 / tS} {1f - (float)v2 / tS}");
                        model.uvs.Add($"vt {(float)u3 / tS} {1f - (float)v3 / tS}");
                        model.uvs.Add($"vt {(float)u4 / tS} {1f - (float)v4 / tS}");

                        uvIdx += 4;
                    }

                    facesPtr += 0x10;
                }

                for (int i = 0; i < matID.Count; i++)
                {
                    model.faces.AddRange(facesDict[matID[i]]);
                }

                File.WriteAllLines(odir + $"LEVEL{levelId}_{sectionId}.obj", model.GetObj());
                File.WriteAllLines(odir + $"LEVEL{levelId}_{sectionId}_MAT.mtl", matData);

                mem.Dispose();
            }
        }
    }
}
