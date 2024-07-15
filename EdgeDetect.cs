using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace SanadDiP
{
    /// <CREDITS>
    /// Sanad Hajarat
    /// Data Science Intern
    /// ProgressSoft Corporation
    /// </CREDITS>
    
    public class EdgeDetect
    {
        public static Bitmap Sobel(Bitmap b, bool horizontal) 
        {
            int [,] kernel;
            if (horizontal)
                kernel = new int [,] { {-1, -2, -1}, {0, 0, 0}, {1, 2, 1} };
            else
                kernel = new int [,] { {-1, 0, -1}, {-2, 0, 2}, {-1, 0, 1} };

            int bW = b.Width, bH = b.Height;

            Bitmap gray = ImageAlteration.GrayScale(b);
            Bitmap final = new Bitmap(bW, bH, PixelFormat.Format8bppIndexed);

            BitmapData grayImg = gray.LockBits(new Rectangle(0, 0, bW, bH), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
            BitmapData finalImg = final.LockBits(new Rectangle(0, 0, bW, bH), ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);

            int stride = grayImg.Stride;

            int ptrOffset = stride - bW;
            int grayOffset = stride - bW + 2;
            
            unsafe
            {
                byte* grayPixel = (byte*)grayImg.Scan0.ToPointer();
                byte* ptr = (byte*)finalImg.Scan0.ToPointer();

                for (int y = 0; y < bH; y++)
                {
                    for (int x = 0; x < bW; x++, ptr++)
                    {
                        if (x < 1 || x >= bW - 1 || y < 1 || y >= bH - 1) 
                            *ptr = 0;
                        else
                        {
                            int sum = 0;
                            for (int i = 0; i < 3; i++)
                            {
                                for (int j = 0; j < 3; j++) 
                                {
                                    sum += kernel[i, j] * *(grayPixel + (i * stride) + j);
                                }
                            }

                            if (sum > 255)
                                sum = 255;
                            else if (sum < 0)
                                sum = 0;

                            *ptr = (byte)sum;

                            grayPixel++;
                        }
                    }
                    ptr += ptrOffset;
                    if (y < 1)
                        continue;
                    grayPixel += grayOffset;
                }
            }

            gray.UnlockBits(grayImg);
            final.UnlockBits(finalImg);

            return final;
        }

        public static Bitmap Laplace(Bitmap b) 
        {
            int [,] kernel = new int [,] { {0, 1, 0}, {1, -4, 1}, {0, 1, 0} };

            int bW = b.Width, bH = b.Height;

            Bitmap gray = ImageAlteration.GrayScale(b);
            Bitmap final = new Bitmap(bW, bH, PixelFormat.Format8bppIndexed);

            BitmapData grayImg = gray.LockBits(new Rectangle(0, 0, bW, bH), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
            BitmapData finalImg = final.LockBits(new Rectangle(0, 0, bW, bH), ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);

            int stride = grayImg.Stride;

            int ptrOffset = stride - bW;
            int grayOffset = stride - bW + 2;
            
            unsafe
            {
                byte* grayPixel = (byte*)grayImg.Scan0.ToPointer();
                byte* ptr = (byte*)finalImg.Scan0.ToPointer();

                for (int y = 0; y < bH; y++)
                {
                    for (int x = 0; x < bW; x++, ptr++)
                    {
                        if (x < 1 || x >= bW - 1 || y < 1 || y >= bH - 1) 
                            *ptr = 0;
                        else
                        {
                            int sum = 0;
                            for (int i = 0; i < 3; i++)
                            {
                                for (int j = 0; j < 3; j++) 
                                {
                                    sum += kernel[i, j] * (*(grayPixel + (i * stride) + j));
                                }
                            }

                            if (sum > 255)
                                sum = 255;
                            else if (sum < 0)
                                sum = 0;

                            *ptr = (byte)sum;

                            grayPixel++;
                        }
                    }
                    ptr += ptrOffset;
                    if (y < 1)
                        continue;
                    grayPixel += grayOffset;
                }
            }

            gray.UnlockBits(grayImg);
            final.UnlockBits(finalImg);

            return final;
        }

        public static Bitmap Combine(Bitmap b1, Bitmap b2)
        {
            int bW = b1.Width, bH = b1.Height;

            Bitmap newB = new Bitmap(bW, bH, PixelFormat.Format8bppIndexed);

            BitmapData bmd1 = b1.LockBits(new Rectangle(0, 0, bW, bH), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
            BitmapData bmd2 = b2.LockBits(new Rectangle(0, 0, bW, bH), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);

            BitmapData final = newB.LockBits(new Rectangle(0, 0, bW, bH), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);

            int offset = bmd1.Stride - bW;

            unsafe
            {
                byte* ptr1 = (byte*)bmd1.Scan0.ToPointer();
                byte* ptr2 = (byte*)bmd2.Scan0.ToPointer();
                byte* pixel = (byte*)final.Scan0.ToPointer();

                for (int i = 0; i < bH; i++, ptr1 += offset, ptr2 += offset, pixel += offset)
                    for (int j = 0; j < bW; j++, ptr1++, ptr2++, pixel++)
                    {
                        int sum = *ptr1 + *ptr2;
                        if (sum > 255)
                            sum = 255;
                        *pixel = (byte)sum;
                    }
            }
            b1.UnlockBits(bmd1);
            b2.UnlockBits(bmd2);
            newB.UnlockBits(final);

            return newB;
        }

    }
}