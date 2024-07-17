using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;

namespace SanadDiP
{
    /// <CREDITS>
    /// Sanad Hajarat
    /// Data Science Intern
    /// ProgressSoft Corporation
    /// </CREDITS>
    
    // Simply a class used to alter images, either applying grayscale or adding and removing borders.
    public class ImageAlteration
    {
        public static Bitmap GrayScale(Bitmap b)  // Image's are converted to 8-Bit Per Pixel Grayscale
        {
            if (b.PixelFormat == PixelFormat.Format8bppIndexed) // return original image if already 8bpp
                return (Bitmap)b.Clone();
                        
            int bW = b.Width, bH = b.Height;                    // initialize variables & puxel formats instead of repetitively calling them
            Bitmap grayBitmap = new Bitmap(bW, bH, PixelFormat.Format8bppIndexed);             // This bitmap will hold the gray image we will return
            BitmapData coloredLock = b.LockBits(new Rectangle(0, 0, bW, bH), ImageLockMode.ReadOnly, b.PixelFormat);        // Read only info from original image
            BitmapData grayLock = grayBitmap.LockBits(new Rectangle(0, 0, bW, bH), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);  // Write on new 8-Bit image

            int pFsize = Image.GetPixelFormatSize(b.PixelFormat);  // returns 24 for 24bppRgb and 8 for 8bppIndexed etc..
            int bpp = pFsize / 8 > 1 ? pFsize / 8 : 1;  // stores int value in bpp that tells us how many bytes per pixel
                                                        //    ( < 1 =  1 bit )    (   3 = 24 bit )    (   4 = 32 bit )
            int strideColored = coloredLock.Stride;

            int offSetColored = strideColored - bW * bpp;   // offset is based on the bytes per pixel for original image
            int offSetGray = grayLock.Stride - bW;                // Grayscale so offset is determined for one channel

            ColorPalette palette = grayBitmap.Palette;
            for (int i = 0; i < 256; i++)
                palette.Entries[i] = Color.FromArgb(i, i, i);
            grayBitmap.Palette = palette;             // define color palette for grayscale image.

            unsafe
            {
                byte* coloredPixel = (byte*)coloredLock.Scan0.ToPointer(); // gets a pointer to the first pixel data in the original image.
                byte* grayPixel = (byte*)grayLock.Scan0.ToPointer(); // gets a pointer to the first pixel data in the gray image.
                
                if (bpp <= 1) // if pf = 1bppIndexed
                {
                    for (int y = 0; y < bH; y++, coloredPixel += strideColored, grayPixel += offSetGray)
                    {
                        for (int x = 0; x < bW; x++, grayPixel++)    // increment x for trace and ptr2 to get to next pixel.
                        {
                            byte pixelByte = *(coloredPixel + (x / 8));// finds byte responsible for pixel value e.x. (1000 0101)
                            int pixelValue = (pixelByte >> (7 - (x % 8))) & 0x01;   // results in 0 or 1 only which is pixel value from bit.
                            *grayPixel |= (byte)(pixelValue > 0 ? 255 : 0);            // sets new gray image value to 255 or 0.
                        }
                    }
                }
                else    //if not 1 bit
                {
                    for (int y = 0; y < bH; y++)
                    {
                        for (int x = 0; x < bW; x++, coloredPixel += bpp, grayPixel++)   // increments gray image and non-gray image according to its bytes per pixel.
                            *grayPixel = (byte)(.299 * coloredPixel[2] + .587 * coloredPixel[1] + .114 * coloredPixel[0]); // calculates pixel color value
                        coloredPixel += offSetColored;
                        grayPixel += offSetGray;
                    }
                }
            }

            b.UnlockBits(coloredLock);
            grayBitmap.UnlockBits(grayLock);
            
            return grayBitmap;
        }

        public static Bitmap AddBorder(Bitmap b, byte color, int borderWidth)    // Adds border of certain color. Helps with dilation and erosion.
        {
            int bitmapWidth = b.Width, bitmapHeight = b.Height, bW2 = bitmapWidth + (borderWidth*2), bH2 = bitmapHeight + (borderWidth*2); // define new width and height for bordered image.
            Bitmap greyB = GrayScale(b);                                        // grayscale transformation to make it easier to deal with.
            Bitmap newB = new Bitmap(bW2, bH2, PixelFormat.Format8bppIndexed);
            BitmapData grayImage = greyB.LockBits(new Rectangle(0, 0, bitmapWidth, bitmapHeight),
                ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);     
            BitmapData paddedImage = newB.LockBits(new Rectangle(0, 0, bW2, bH2),
                ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);  

            unsafe
            {
                byte* normal = (byte*)grayImage.Scan0.ToPointer(); // gets a pointer to the first pixel data in the normal image.
                byte* padded = (byte*)paddedImage.Scan0.ToPointer(); // gets a pointer to the first pixel data in the bordered image.

                int offSet1 = grayImage.Stride - bitmapWidth, offSet2 = paddedImage.Stride - bW2;
                
                for (int y = 0; y < bH2; y++, padded += offSet2)
                {
                    for (int x = 0; x < bW2; x++, padded++)
                    {
                        if (x < borderWidth || y < borderWidth || x > bitmapWidth + (borderWidth-1) || y > bitmapHeight + (borderWidth-1)) 
                            { *padded = color; }                                    // if on border make pixel = color
                        else { *padded = *normal; normal++; }                       // else pixel = normal pixel
                    }
                    if (y == 0)
                        continue;
                    normal += offSet1; // don't add offset if on first row
                }
            }
            greyB.UnlockBits(grayImage);
            newB.UnlockBits(paddedImage);

            return newB;
        }

        public static Bitmap RemoveWhiteBounds(Bitmap b) // Remove's white boundary from image in an equal manner.
        {
            Bitmap newB = GrayScale(b);                                     // Applies grayscale 
            Bitmap final = Binarization.ApplyStaticThreshold(newB, 200);    // Apply high threshold to define pure white from shadow
            int bW = b.Width, bH = b.Height;
            BitmapData bmd = final.LockBits(new Rectangle(0, 0, bW, bH), 
                ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed); // working on same image and calculating pixels to be removed
            
            bool breakTop = false, breakLeft = false, breakRight = false, breakBot = false;

            int skipY   = 0;         // how many pixels we are skipping from final image because of white border.
            int skipX  = 0;
            int finalW = 0;
            int finalH   = 0;

            int stride = bmd.Stride;    // NON-CHANGING stride value.

            unsafe
            {  
                byte* ptr = (byte*)bmd.Scan0.ToPointer();                        // First pixel from top-left corner of image
                while (skipY < bH)
                {
                    byte* ptrTop = ptr + stride*skipY;
                    for (int i = 0; i < bW; i++)
                    {
                        if (*ptrTop < 255) // if either top or bot pixels not white
                        { 
                            breakTop = true;
                            break;
                        }
                        ptrTop++;
                    }
                    if (breakTop)
                        break;

                    skipY++;
                }

                while (skipX < bW)
                {
                    byte* ptrLeft = ptr + stride*skipY + skipX;
                    for (int i = 0; i < bH - skipY; i++)
                    {
                        if (*ptrLeft < 255) // if either top or bot pixels not white
                        { 
                            breakLeft = true;
                            break;
                        }
                        ptrLeft += stride;
                    }
                    if (breakLeft)
                        break;

                    skipX++;
                }

                while (skipX + finalW < bW)
                {
                    byte* ptrRight = ptr + stride*skipY + bW - 1 - finalW;
                    for (int i = 0; i < bH - skipY; i++)
                    {
                        if (*ptrRight < 255) // if either top or bot pixels not white
                        { 
                            breakRight = true;
                            break;
                        }
                        ptrRight += stride;
                    }
                    if (breakRight)
                        break;

                    finalW++;
                }

                while (skipY + finalH < bH)
                {
                    byte* ptrBot = ptr + stride*(bH-1-finalH) + skipX;
                    for (int i = 0; i < bW - finalW - skipX; i++)
                    {
                        if (*ptrBot < 255) // if either top or bot pixels not white
                        { 
                            breakBot = true;
                            break;
                        }
                        ptrBot++;
                    }
                    if (breakBot)
                        break;

                    finalH++;
                }

                // shorter and more readable method
            }

            final.UnlockBits(bmd);


            return b.Clone(new Rectangle(skipX, skipY, bW-finalW-skipX, bH-finalH-skipY), b.PixelFormat);   

        }   // Returns same Image cropped by exactly n rows and columns equally

        public static Bitmap RemoveWhiteBoundsWholeImage(Bitmap b) // Remove's white boundary from image in an equal manner.
        {
            Bitmap newB = GrayScale(b);                                     // Applies grayscale 
            Bitmap final = Binarization.ApplyStaticThreshold(newB, 200);    // Apply high threshold to define pure white from shadow
            int bW = b.Width, bH = b.Height;
            BitmapData bmd = final.LockBits(new Rectangle(0, 0, bW, bH), 
                ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed); // working on same image and calculating pixels to be removed

            int minX = bW - 1, minY = bH - 1, maxX = 0, maxY = 0; // keep changing

            int offSet = bmd.Stride - bW;    // NON-CHANGING stride value.

            unsafe
            {  
                byte* ptr = (byte*)bmd.Scan0.ToPointer();                        // First pixel from top-left corner of image
                
                for (int y = 0; y < bH; y++, ptr += offSet) 
                {
                    for (int x = 0; x < bW; x++, ptr++) 
                    {
                        if  (*ptr < 255) 
                        {
                            if (x < minX)
                                minX = x;
                            if (y < minY)
                                minY = y;
                            if (x > maxX)
                                maxX = x;
                            if (y > maxY)
                                maxY = y;
                        }
                    }
                }
            }

            final.UnlockBits(bmd);


            return b.Clone(new Rectangle(minX, minY, maxX-minX+1, maxY-minY+1), b.PixelFormat);   

        }   // Returns same Image cropped by exactly n rows and columns equally


        public static Bitmap RescaleGray(Bitmap b, double factor) // Changes image resolution by resizing/rescaling
        {
            Bitmap copy = GrayScale(b);
            if (factor == 1)
                return copy;
            
            int originalWidth = copy.Width, originalHeight = copy.Height;
            int rescaledW = (int)(originalWidth * factor);
            int rescaledH = (int)(originalHeight * factor);  // using factor to change bW and bH.

            Bitmap newB = new Bitmap(rescaledW, rescaledH, PixelFormat.Format8bppIndexed);
            newB.Palette = copy.Palette;

            BitmapData original = copy.LockBits(new Rectangle(0, 0, originalWidth, originalHeight), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
            BitmapData rescaled = newB.LockBits(new Rectangle(0, 0, rescaledW, rescaledH), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

            int strideOriginal = original.Stride;
            int offSetRescaled = rescaled.Stride - rescaledW;

            double stepX = 1.0/factor, stepY = 1.0/factor;

            unsafe
            {
                byte* originalPixel = (byte*)original.Scan0.ToPointer();  
                byte* rescaledPixel = (byte*)rescaled.Scan0.ToPointer();  

                for (int y = 0; y < rescaledH; y++, rescaledPixel += offSetRescaled) 
                {
                    byte* rowPtr = originalPixel;
                    for (int x = 0; x < rescaledW; x++, rescaledPixel++)
                    {
                        *rescaledPixel = *rowPtr;
                        if (stepX >= 1)
                        {
                            rowPtr += (int) stepX;
                            stepX = 1.0/factor;
                        }
                        else
                            stepX += 1.0/factor;
                    }
                    if (stepY >= 1)
                    {
                        originalPixel += (int) stepY * strideOriginal;
                        stepY = 1.0/factor;
                    }
                    else
                        stepY += 1.0/factor;
                }
            }
            
            copy.UnlockBits(original);
            newB.UnlockBits(rescaled);
            
            return newB;
        }

        public static Bitmap Rescale(Bitmap b, double factor)     // Changes image resolution by resizing/rescaling
        {
            Bitmap copy = (Bitmap)b.Clone();
            copy.Palette = b.Palette;
            if (factor == 1)
                return copy;
            if ((copy.PixelFormat != PixelFormat.Format8bppIndexed && copy.PixelFormat != PixelFormat.Format24bppRgb))
                copy = GrayScale(b);
            
            int originalWidth = copy.Width, originalHeight = copy.Height;
            int rescaledW = (int)(originalWidth * factor);
            int rescaledH = (int)(originalHeight * factor);  // using factor to change bW and bH.

            Bitmap newB = new Bitmap(rescaledW, rescaledH, copy.PixelFormat);
            newB.Palette = b.Palette;

            BitmapData original = copy.LockBits(new Rectangle(0, 0, originalWidth, originalHeight), ImageLockMode.ReadOnly, copy.PixelFormat);
            BitmapData rescaled = newB.LockBits(new Rectangle(0, 0, rescaledW, rescaledH), ImageLockMode.WriteOnly, copy.PixelFormat);

            int strideOriginal = original.Stride;
            int bpp = Image.GetPixelFormatSize(copy.PixelFormat) / 8;
            int offSetRescaled = rescaled.Stride - rescaledW * bpp;

            unsafe
            {
                byte* originalPixel = (byte*)original.Scan0.ToPointer();  
                byte* rescaledPixel = (byte*)rescaled.Scan0.ToPointer();  

                for (int y = 0; y < rescaledH; y++) 
                {
                    int pixelY = (int) (y / factor);
                    byte* rowPtr = originalPixel + (pixelY * strideOriginal);

                    for (int x = 0; x < rescaledW * bpp; x+= bpp, rescaledPixel+= bpp)
                    {
                        int pixelX = (int) (x / factor);
                        for (int i = 0; i < bpp; i++)
                            rescaledPixel[i] = (rowPtr + pixelX)[i];
                    }
                    rescaledPixel += offSetRescaled;
                }
            }
            
            copy.UnlockBits(original);
            newB.UnlockBits(rescaled);
            
            return newB;
        }

    } 
}