using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic; 

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

            Bitmap b = new Bitmap(address + "/Milestone-examples/ShapeDetectionTest2.png");
            Console.WriteLine($"Width: {b.Width}, Height: {b.Height}");

            b = ImageAlteration.GrayScale(b);
            b = Binarization.ApplyStaticThreshold(b, 127);
            b.Save("Images/Shapes2Binary.jpg", ImageFormat.Jpeg);

            sw.Restart();
            List<List <Point>> listInList = Corners.FindContours(b);
            Bitmap[] b2 = Corners.Split(b, listInList);
            sw.Stop();
            Console.WriteLine($"Shapes Detected = {listInList.Count} | Time taken: {sw.ElapsedMilliseconds}ms\n");

            sw.Restart();

            int circle = 1, triangle = 1, square = 1, rectangle = 1;
            for (int i = 0; i < b2.Length; i++)
            {
                b2[i].Save($"Shapes2Detected/Shape{i+1}.jpg", ImageFormat.Jpeg);

                Bitmap myB = ImageAlteration.Invert(Corners.Laplace(b2[i]));
                List<Point> outline = Corners.DefineOneShape(myB);

                int corners = Corners.Count(outline);
                Console.WriteLine($"Shape {i+1} has {corners} corners with {outline.Count} points.\n");
                double aspectRatio = myB.Width / myB.Height;
                if (corners == 3)
                    b2[i].Save($"Shapes2Final/Triangle{triangle++}.jpg", ImageFormat.Jpeg);
                else if (corners == 4 && (aspectRatio >= 0.95 && aspectRatio <= 1.05))
                    b2[i].Save($"Shapes2Final/Square{square++}.jpg", ImageFormat.Jpeg);
                else if (corners == 4)
                    b2[i].Save($"Shapes2Final/Rectangle{rectangle++}.jpg", ImageFormat.Jpeg);
                else
                    b2[i].Save($"Shapes2Final/Circle{circle++}.jpg", ImageFormat.Jpeg);
                
                myB.Save($"Shapes2Laplace/Shape{i+1}.jpg", ImageFormat.Jpeg);
            }
            
            sw.Stop();
            Console.WriteLine($"\nTime taken for saving all shapes: {sw.ElapsedMilliseconds}ms\n");

        }  
    }
}

