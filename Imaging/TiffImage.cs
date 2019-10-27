using System;
using BitMiracle.LibTiff.Classic;

namespace DragonLib.Imaging
{
    public static class TiffImage
    {
        public static bool WriteTiff(string path, Span<byte> mip0, int width, int height)
        {
            using var tif = Tiff.Open(path, "w");
            if (tif == null) return false;

            tif.SetField(TiffTag.IMAGEWIDTH, width);
            tif.SetField(TiffTag.IMAGELENGTH, height);
            tif.SetField(TiffTag.BITSPERSAMPLE, 8);
            tif.SetField(TiffTag.SAMPLESPERPIXEL, 4);
            tif.SetField(TiffTag.PHOTOMETRIC, Photometric.RGB);
            tif.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
            tif.SetField(TiffTag.ROWSPERSTRIP, 1);
            var bpr = width * 4;
            for (var i = 0; i < height; ++i) tif.WriteScanline(mip0.Slice(i * bpr, bpr).ToArray(), i);
            tif.FlushData();
            tif.Close();
            return true;
        }
    }
}
