using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;

namespace SanadDiP
{
    /// <CREDITS>
    /// Sanad Hajarat
    /// Data Science Intern
    /// ProgressSoft Corporation
    /// </CREDITS>

    /////// look at width & stride call cost
    /////// why static everything get to the core of it
    
    public class Run
    {
        // commands to run in /DIPME/MyDiP to run program, always save first and dotnet build
        // cd /DIPME/MyDiP
        // dotnet publish -c Release -r win-x64 --self-contained
        // wine /home/sanad/DIPME/MyDiP/bin/Release/net6.0/win-x64/publish/MyDiP.exe
        
        static void Main(string[] args)
        {
            Console.WriteLine();
            Stopwatch sw = new Stopwatch();
            string address = "/home/sanad/Desktop/My Files";

            // Going through tasks:

            // Task 1: Convert gray scale images to black and white (binarization) using static threshold, mean threshold.

            Bitmap bmp = new Bitmap(address + "/8BitImages/HSOTHER8BIT.bmp");
            sw.Start();
            Bitmap staticBinary = Binarization.ApplyStaticThreshold(bmp, 128);
            sw.Stop();
            Console.WriteLine($"Time taken for a {bmp.PixelFormat} image of size ({bmp.Width}, {bmp.Height}) for static thresholding: {sw.ElapsedMilliseconds}ms");
            sw.Restart();
            Bitmap meanBinary = Binarization.ApplyMeanThreshold(bmp);
            sw.Stop();
            Console.WriteLine($"Time taken for a {bmp.PixelFormat} image of size ({bmp.Width}, {bmp.Height}) for mean thresholding: {sw.ElapsedMilliseconds}ms");
            staticBinary.Save("Images/ThreshStatic.jpg", ImageFormat.Jpeg);
            meanBinary.Save("Images/ThreshMean.jpg", ImageFormat.Jpeg);
            Console.WriteLine();

            // Task 2: Concatenate two gray scale images horizontally and vertically.

            bmp = new Bitmap(address + "/8BitImages/lena_gray.bmp");
            Bitmap bmp2 = new Bitmap(address + "/8BitImages/CHEQUE8bit.bmp");
            sw.Restart();
            Bitmap vertical = Concatenate.Concat(bmp, bmp2, false);
            sw.Stop();
            Console.WriteLine($"Time taken for two {bmp.PixelFormat} images of sizes ({bmp.Width}, {bmp.Height}) & ({bmp2.Width}, {bmp2.Height}) for Vertical Concatenation: {sw.ElapsedMilliseconds}ms");
            sw.Restart();
            Bitmap horizontal = Concatenate.Concat(bmp, bmp2, true);
            sw.Stop();
            Console.WriteLine($"Time taken for two {bmp2.PixelFormat} images of sizes ({bmp.Width}, {bmp.Height}) & ({bmp2.Width}, {bmp2.Height}) for Horizontal Concatenation: {sw.ElapsedMilliseconds}ms");
            vertical.Save("Images/ConcatVerticalLenaCheque.jpg", ImageFormat.Jpeg);
            horizontal.Save("Images/ConcatHorizontalLenaCheque.jpg", ImageFormat.Jpeg);
            Console.WriteLine();

            // Tasks 3 & 4: Convert 24 bits color & 1-bit binary images to 8-bit grayscale image. 
            
            bmp = new Bitmap(address + "/1BitImages/2ChequesBW.bmp");
            bmp2 = new Bitmap(address + "/24BitImages/Headshot.jpg");
            sw.Restart();
            Bitmap binaryToGray = ImageAlteration.GrayScale(bmp);
            sw.Stop();
            Console.WriteLine($"Time taken for a binary (1-bit image) of size ({bmp.Width}, {bmp.Height}) to grayscale transformation: {sw.ElapsedMilliseconds}ms");
            binaryToGray.Save("Images/BITtoGRAYcheques.jpg", ImageFormat.Jpeg);
            sw.Restart();
            Bitmap rgbToGray = ImageAlteration.GrayScale(bmp2);
            sw.Stop();
            Console.WriteLine($"Time taken for a colored (24-bit image) of size ({bmp2.Width}, {bmp2.Height}) to grayscale transformation: {sw.ElapsedMilliseconds}ms");
            rgbToGray.Save("Images/RGBtoGRAYme.jpg", ImageFormat.Jpeg);
            Console.WriteLine();

            // Task 5: Apply morphological dilation and erosion (3X3) to a binary image.

            bmp = new Bitmap(address + "/1BitImages/bwImage.bmp");
            int[,] circle = new int[,] { { 0, 0, 0, 1, 0, 0, 0 }, { 0, 0, 1, 1, 1, 0, 0 }, { 0, 1, 1, 1, 1, 1, 0 }, { 1, 1, 1, 1, 1, 1, 1 }, { 0, 1, 1, 1, 1, 1, 0 }, { 0, 0, 1, 1, 1, 0, 0 }, { 0, 0, 0, 1, 0, 0, 0 } };
            sw.Restart();
            Bitmap dilated = Morphology.Dilate(bmp, circle);
            sw.Stop();
            Console.WriteLine($"Time taken for a binary image of size ({bmp.Width}, {bmp.Height}) to apply dilation: {sw.ElapsedMilliseconds}ms");
            sw.Restart();
            Bitmap eroded = Morphology.Erode(bmp, circle);
            sw.Stop();
            Console.WriteLine($"Time taken for a binary image of size ({bmp.Width}, {bmp.Height}) to apply erosion: {sw.ElapsedMilliseconds}ms");
            dilated.Save("Images/SignDilatedDiamond7x7.jpg", ImageFormat.Jpeg);
            eroded.Save("Images/SignErodedDiamond7x7.jpg", ImageFormat.Jpeg);
            Console.WriteLine();

            // Task 6: Remove white boundaries from an image.

            bmp = new Bitmap(address + "/24BitImages/shoe.jpeg");
            
            sw.Restart();
            bmp2 = ImageAlteration.RemoveWhiteBounds(bmp);
            sw.Stop();
            Console.WriteLine($"Image Before size = ({bmp.Width}, {bmp.Height}), Image After size = ({bmp2.Width}, {bmp2.Height})");
            Console.WriteLine($"Time taken: {sw.ElapsedMilliseconds}ms");
            bmp2.Save("Images/ShoeNoWhiteBounds.jpg", ImageFormat.Jpeg);
            Console.WriteLine();

            // Task 7: Rescale image to best fit (either horizontally or vertically)

            bmp.Save("Images/ShoeNormal.jpg", ImageFormat.Jpeg);

            sw.Restart();
            bmp2 = ImageAlteration.Rescale(bmp, 0.2);
            sw.Stop();
            Console.WriteLine($"Image Before size = ({bmp.Width}, {bmp.Height}), Image After size = ({bmp2.Width}, {bmp2.Height})");
            Console.WriteLine($"Time taken: {sw.ElapsedMilliseconds}ms");
            Console.WriteLine();
            bmp2.Save("Images/ShoeRescaledFifth.jpg", ImageFormat.Jpeg);

            bmp = ImageAlteration.GrayScale(bmp);
            bmp.Save("Images/ShoeGray.jpg", ImageFormat.Jpeg);

        }   
    }
}