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
    
    public class Corners
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
        public static List<List <Point>> FindConnectedComponents(Bitmap b)
        {
            List<List<Point>> components = new List<List<Point>>();
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
                            List<Point> component = new List<Point>();
                            TraceOutline(bmd, stride, x, y, visited, component, bW, bH);
                            components.Add(component);
                        }
                    }
                }
            }

            b.UnlockBits(bmd);

            return components;
        }   

        private static void TraceOutline(BitmapData bmd, int stride, int x, int y, bool [,] visited, List<Point> component, int bW, int bH)
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
                    component.Add(p);

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
                    if (myX < minX)
                        minX = myX;
                    if (myY > maxY)
                        maxY = myY;
                    if (myY < minY)
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
            }
            return splitShapes;
        }
    
        public static List<Point> DefineOneShape(Bitmap b)
        {
            List<Point> component = new List<Point>();

            Bitmap newB = ImageAlteration.RemoveWhiteBoundsWholeImage(b);
            int bW = newB.Width, bH= newB.Height;
            bool[,] visited = new bool[bW, bH];
            BitmapData bmd = newB.LockBits(new Rectangle(0, 0, bW, bH), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);

            int stride = bmd.Stride;
            int offSet = stride - bW;
            int [] yTraversal = new int[] {-1, 1, 0};

            unsafe
            {
                byte* ptr = (byte*)bmd.Scan0.ToPointer();

                Stack<Point> stack = new Stack<Point>();

                for (int x = 0; x < bW; x++)
                {
                    if (*(ptr+x) == 0)
                    {
                        stack.Push(new Point(x, 0));
                        break;
                    }
                }

                while (stack.Count > 0)
                {
                    Point p = stack.Pop();
                    int myX = p.X, myY = p.Y;
                    if (visited[myX, myY])
                        continue;

                    visited[myX, myY] = true;
                    component.Add(p);

                    for (int dy = 0; dy <= 2; dy++)
                    {
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            int newX = myX + dx;
                            int newY = myY + yTraversal[dy];

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

            newB.UnlockBits(bmd);

            return component;
        }

        public static int Count(List<Point> contour, double thresholdAngle = 0.2)
        {
            int corners = 0;
            int count = contour.Count;

            for (int i = 0; i < count - 1; i++)
            {
                Point prev = contour[(i - 1 + count) % count];
                Point current = contour[i];
                Point next = contour[(i + 1) % count];

                double angle = CalculateAngle(prev, current, next);

                if (!(angle < thresholdAngle || angle > (Math.PI - thresholdAngle)))
                {
                    corners++;
                    // Console.WriteLine($"Corner at: ({contour[i].X}, {contour[i].Y})");
                }
            }

            return corners;
        }

        private static double CalculateAngle(Point A, Point B, Point C)
        {
            double BAx = A.X - B.X;
            double BAy = A.Y - B.Y;

            double BCx = C.X - B.X;
            double BCy = C.Y - B.Y;

            double dotProduct = BAx * BCx + BAy * BCy;
            double magnitudeBA = Math.Sqrt(BAx * BAx + BAy * BAy);
            double magnitudeBC = Math.Sqrt(BCx * BCx + BCy * BCy);

            double cosTheta = dotProduct / (magnitudeBA * magnitudeBC);

            // Clamp the cosine value to the [-1, 1] range to avoid precision errors
            cosTheta = Math.Max(-1.0, Math.Min(1.0, cosTheta));

            double angle = Math.Acos(cosTheta);

            return angle;
        }
    }
}
