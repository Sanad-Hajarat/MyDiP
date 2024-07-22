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
    
    public class Contours
    {
        // really useless currently
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

        // Takes binarized image
        public static List<List <Point>> FindContours(Bitmap b)
        {
            List<List<Point>> contours = new List<List<Point>>();
            int bW = b.Width, bH = b.Height;

            bool[,] visited = new bool[bW, bH];
            BitmapData bmd = b.LockBits(new Rectangle(0, 0, bW, bH), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);

            int stride = bmd.Stride;
            int offSet = stride - bW;

            unsafe
            {
                byte* pixel = (byte*)bmd.Scan0.ToPointer();

                for (int y = 0; y < bH; y++, pixel += offSet)
                {
                    for (int x = 0; x < bW; x++, pixel++)
                    {
                        if (*pixel < 128 && !visited[x, y])
                        {
                            List<Point> contour = new List<Point>();
                            TraceOutline(bmd, stride, x, y, visited, contour, bW, bH);
                            contours.Add(contour);
                        }
                    }
                }
            }

            b.UnlockBits(bmd);

            return contours;
        }   

        private static void TraceOutline(BitmapData bmd, int stride, int x, int y, bool [,] visited, List<Point> contour, int bW, int bH)
        {
            Stack<Point> stack = new Stack<Point>();
            stack.Push(new Point(x, y));

            unsafe
            {
                byte* ptr = (byte*)bmd.Scan0.ToPointer();
                while (stack.Count > 0)
                {
                    Point p = stack.Pop();
                    int myX = p.X, myY = p.Y;
                    if (visited[myX, myY])
                        continue;

                    visited[myX, myY] = true;
                    contour.Add(p);

                    for (int dy = -1; dy <= 1; dy++)
                    {
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            int newX = myX + dx;
                            int newY = myY + dy;

                            if (newX >= 0 && newX < bW && newY >= 0 && newY < bH)
                            {
                                byte* pixel = ptr + (newY * stride) + newX;
                                if (*pixel == 0 && !visited[newX, newY])
                                    stack.Push(new Point(newX, newY));
                            }
                        }
                    }
                }
            }
        }


        public static Bitmap[] Split(Bitmap b, List<List <Point>> shapes)
        {
            int size = shapes.Count;
            int bW = b.Width, bH = b.Height;
            Bitmap [] splitShapes = new Bitmap[size];
            for (int i = 0; i < size; i++)
            {
                int minX = bW, minY = bH, maxX = 0, maxY = 0;
                foreach (Point p in shapes[i])
                {
                    int myX = p.X, myY = p.Y;
                    if (myX > maxX)
                        maxX = myX;
                    else if (myX < minX)
                        minX = myX;
                    if (myY > maxY)
                        maxY = myY;
                    else if (myY < minY)
                        minY = myY;
                }

                int width = maxX-minX+3, height = maxY-minY+3;
                Bitmap bm = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
                BitmapData bmd = bm.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);

                int stride = bmd.Stride;
                int offSet = stride - width;

                unsafe
                {
                    byte* ptr = (byte*)bmd.Scan0.ToPointer() + 1 + stride;

                    foreach (Point p in shapes[i])
                    {
                        int myX = p.X, myY = p.Y;
                        *(ptr + myX-minX + ((myY-minY)*stride)) = 255;
                    }

                    // inverting after
                    ptr = ptr - stride - 1;
                    for (int y = 0; y < height; y++, ptr += offSet)
                    {
                        for (int x = 0; x < width; x++, ptr++)
                        {
                            *ptr = (byte)(255 - *ptr);
                        }
                    }
                }

                bm.UnlockBits(bmd);
                
                splitShapes[i] = bm;

                //splitShapes[i] = (Bitmap)b.Clone(new Rectangle(minX-1, minY-1, maxX-minX+3, maxY-minY+3), PixelFormat.Format8bppIndexed);
            }
            return splitShapes;
        }
    }
}
