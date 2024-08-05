using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic; 
using System.IO;

namespace SanadDiP
{
    /// <CREDITS>
    /// Sanad Hajarat
    /// Data Science Intern
    /// ProgressSoft Corporation
    /// </CREDITS>

    /////// look at width & stride call cost
    /////// why static everything get to the core of it
    /////// Challenge yourself ( EXTRA ): what if there is noise (speckles) in image how to remove white bounds?
    
    public class Run
    {
        // commands to run in /DIPME/MyDiP to run program, always save first and dotnet build
        // cd /DIPME/MyDiP
        // ~/.dotnet/dotnet publish -c Release -r win-x64 --self-contained
        //////// dotnet publish -c Release -r win-x64 --self-contained
        // wine /home/sanad/DIPME/MyDiP/bin/Release/net6.0/win-x64/publish/MyDiP.exe
        
        static void Main(string[] args)
        {
            Console.WriteLine();
            Stopwatch sw = new Stopwatch();
            string address = "/home/sanad/Desktop/My Files";

/*

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

            Bitmap bmp = new Bitmap(address + "/8BitImages/lena_gray.bmp");
            Bitmap bmp2 = new Bitmap(address + "/8BitImages/CHEQUE8bit.bmp");
            sw.Restart();
            Bitmap vertical = Concatenate.Concat(bmp, bmp2, false);
            sw.Stop();
            Console.WriteLine($"Time taken for two {bmp.PixelFormat} images of sizes ({bmp.Width}, {bmp.Height}) & ({bmp2.Width}, {bmp2.Height}) for Vertical Concatenation: {sw.ElapsedMilliseconds}ms");
            sw.Restart();
            Bitmap horizontal = Concatenate.Concat(bmp, bmp2, true);
            sw.Stop();
            Console.WriteLine($"Time taken for two {bmp2.PixelFormat} images of sizes ({bmp.Width}, {bmp.Height}) & ({bmp2.Width}, {bmp2.Height}) for Horizontal Concatenation: {sw.ElapsedMilliseconds}ms");

            sw.Restart();
            vertical = Concatenate.ConcatOne(bmp, bmp2, false);
            sw.Stop();
            Console.WriteLine($"Time taken for two {bmp.PixelFormat} images of sizes ({bmp.Width}, {bmp.Height}) & ({bmp2.Width}, {bmp2.Height}) for Vertical Concatenation (ONE): {sw.ElapsedMilliseconds}ms");
            sw.Restart();
            horizontal = Concatenate.ConcatOne(bmp, bmp2, true);
            sw.Stop();
            Console.WriteLine($"Time taken for two {bmp2.PixelFormat} images of sizes ({bmp.Width}, {bmp.Height}) & ({bmp2.Width}, {bmp2.Height}) for Horizontal Concatenation (ONE): {sw.ElapsedMilliseconds}ms");

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

            Bitmap bmp = new Bitmap(address + "/24BitImages/Husky.jpg");
            Bitmap bmp2;

            sw.Restart();
            bmp2 = ImageAlteration.RemoveWhiteBoundsWholeImage(bmp);
            sw.Stop();
            Console.WriteLine($"Image Before size = ({bmp.Width}, {bmp.Height}), Image After size = ({bmp2.Width}, {bmp2.Height})");
            Console.WriteLine($"Time taken for whole image method: {sw.ElapsedMilliseconds}ms");
            bmp2.Save("Images/HuskyNoWhiteBounds.jpg", ImageFormat.Jpeg);
            Console.WriteLine();

            // Task 7: Rescale image to best fit (either horizontally or vertically)

            bmp = new Bitmap(address + "/8BitImages/lena_gray.bmp");

            sw.Restart();
            bmp2 = ImageAlteration.RescaleGray(bmp, 0.33);
            sw.Stop();
            Console.WriteLine($"Image Before size = ({bmp.Width}, {bmp.Height}), Image After size = ({bmp2.Width}, {bmp2.Height})");
            Console.WriteLine($"Time taken: {sw.ElapsedMilliseconds}ms");
            Console.WriteLine();
            bmp2.Save("Images/LenaRescaledThirdGray.jpg", ImageFormat.Jpeg);
*/
            //// Tests
            
            // Best image is using Laplace.

            string[] filepaths = new string[] { "/home/sanad/DIPME/MyDiP/ShapeDetection/Shapes1Final", "/home/sanad/DIPME/MyDiP/ShapeDetection/Shapes2Final", "/home/sanad/DIPME/MyDiP/ShapeDetection/Shapes3Final",
                                                "/home/sanad/DIPME/MyDiP/ShapeDetection/Shapes1Laplace"," /home/sanad/DIPME/MyDiP/ShapeDetection/Shapes2Laplace", "/home/sanad/DIPME/MyDiP/ShapeDetection/Shapes3Laplace",
                                                "/home/sanad/DIPME/MyDiP/ShapeDetection/Shapes1Detected", "/home/sanad/DIPME/MyDiP/ShapeDetection/Shapes2Detected", "/home/sanad/DIPME/MyDiP/ShapeDetection/Shapes3Detected",
                                                "/home/sanad/DIPME/MyDiP/ShapeDetection/Shapes4Final", "/home/sanad/DIPME/MyDiP/ShapeDetection/Shapes4Laplace", "/home/sanad/DIPME/MyDiP/ShapeDetection/Shapes4Detected",
                                                "/home/sanad/DIPME/MyDiP/ShapeDetection/Shapes1Filled", "/home/sanad/DIPME/MyDiP/ShapeDetection/Shapes2Filled", "/home/sanad/DIPME/MyDiP/ShapeDetection/Shapes3Filled", "/home/sanad/DIPME/MyDiP/ShapeDetection/Shapes4Filled",
                                                "/home/sanad/DIPME/MyDiP/ShapeDetection/Shapes5Final", "/home/sanad/DIPME/MyDiP/ShapeDetection/Shapes5Laplace", "/home/sanad/DIPME/MyDiP/ShapeDetection/Shapes5Detected", "/home/sanad/DIPME/MyDiP/ShapeDetection/Shapes5Filled"}; 

            foreach (string filepath in filepaths)
                if (Directory.Exists(filepath))
                    foreach (string file in Directory.GetFiles(filepath))
                        File.Delete(file);

            Bitmap b = new Bitmap(address + "/Milestone-examples/ShapeDetectionTest.png");
            
            sw.Restart();
            Shapes.Classify(b, 1);
            sw.Stop();

            Console.WriteLine($"\nTime taken for saving all shapes: {sw.ElapsedMilliseconds}ms\n");

            b = new Bitmap(address + "/Milestone-examples/ShapeDetectionTest2.png");

            sw.Restart();
            Shapes.Classify(b, 2);
            sw.Stop();

            Console.WriteLine($"\nTime taken for saving all shapes: {sw.ElapsedMilliseconds}ms\n");

            b = new Bitmap(address + "/Milestone-examples/MyShapes.png");
            b.Save("ShapeDetection/Shapes3.jpg", ImageFormat.Jpeg);
            b = ImageAlteration.GrayScale(b);
            b.Save("ShapeDetection/Shapes3Gray.jpg", ImageFormat.Jpeg);
            b = Binarization.ApplyStaticThreshold(b, 250);
            b.Save("ShapeDetection/Shapes3Binary.jpg", ImageFormat.Jpeg);

            sw.Restart();
            Shapes.Classify(b, 3);
            sw.Stop();

            Console.WriteLine($"\nTime taken for saving all shapes: {sw.ElapsedMilliseconds}ms\n");

            b = new Bitmap(address + "/Milestone-examples/MyShapes2.png");
            b.Save("ShapeDetection/Shapes4.jpg", ImageFormat.Jpeg);
            b = ImageAlteration.GrayScale(b);
            b.Save("ShapeDetection/Shapes4Gray.jpg", ImageFormat.Jpeg);
            b = Binarization.ApplyStaticThreshold(b, 250);
            b.Save("ShapeDetection/Shapes4Binary.jpg", ImageFormat.Jpeg);

            sw.Restart();
            Shapes.Classify(b, 4);
            sw.Stop();

            Console.WriteLine($"\nTime taken for saving all shapes: {sw.ElapsedMilliseconds}ms\n");

            b = new Bitmap(address + "/Milestone-examples/LastTest.png");
            b.Save("ShapeDetection/Shapes5.jpg", ImageFormat.Jpeg);
            b = ImageAlteration.GrayScale(b);
            b.Save("ShapeDetection/Shapes5Gray.jpg", ImageFormat.Jpeg);
            b = Binarization.ApplyStaticThreshold(b, 250);
            b.Save("ShapeDetection/Shapes5Binary.jpg", ImageFormat.Jpeg);

            sw.Restart();
            Shapes.Classify(b, 5);
            sw.Stop();

            Console.WriteLine($"\nTime taken for saving all shapes: {sw.ElapsedMilliseconds}ms\n");
        }  
    }
}

