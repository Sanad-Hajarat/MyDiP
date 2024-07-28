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
    
    public class Shapes
    {
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

            // boolean 2D array of size of the image. Tracks which pixels have been visited.
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
                            // New shape created called component which comes in the form of a list of points.
                            List<Point> component = new List<Point>();
                            GetPixels(bmd, stride, x, y, visited, component, bW, bH);

                            // components.Count = total number of shapes
                            components.Add(component);
                        }
                    }
                }
            }

            b.UnlockBits(bmd);

            return components;
        }   

        private static void GetPixels(BitmapData bmd, int stride, int x, int y, bool [,] visited, List<Point> component, int bW, int bH)
        {
            // All of the points being tracked that are directly neighbors.
            Stack<Point> stack = new Stack<Point>();
            stack.Push(new Point(x, y));

            unsafe
            {
                byte* ptr = (byte*)bmd.Scan0.ToPointer();
                while (stack.Count > 0)
                {
                    Point p = stack.Pop();
                    int myX = p.X, myY = p.Y;
                    
                    // skip the point if we visited it.
                    if (visited[myX, myY])
                        continue;

                    visited[myX, myY] = true;
                    component.Add(p);

                    // goes through 8-neighbors to find whole shape
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
            // makes an array of bitmaps depending on number of shapes.
            int size = shapes.Count;
            int bW = b.Width, bH = b.Height;
            Bitmap [] splitShapes = new Bitmap[size];

            for (int i = 0; i < size; i++)
            {
                // searches for minX, minY, maxX, maxY to find the Width and Height of the shapes.
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
                
                // Adding a padding to make it easier to show them in the images.
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
                        // Setting pixels in the right coordinates.
                        *(ptr + myX-minX + ((myY-minY)*stride)) = 255;
                    }

                    // inverting after.
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
            // Draws the shape in one image disregarding near shapes.
            List<Point> component = new List<Point>();

            Bitmap newB = ImageAlteration.RemoveWhiteBoundsWholeImage(b);
            int bW = newB.Width, bH = newB.Height;
            bool[,] visited = new bool[bW, bH];
            BitmapData bmd = newB.LockBits(new Rectangle(0, 0, bW, bH), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);

            int stride = bmd.Stride;
            int offSet = stride - bW;

            // traverses image to find directly bottom or above pixel instead of going diagonal first.
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

            GetPixels(bmd, stride, 0, 0, visited, component, bW, bH);

            newB.UnlockBits(bmd);

            return component;
        }

        public static int CalculateUniqueSlopes(List<Point> points)
        {
            // HashSet to prevent duplicates
            HashSet<double> slopes = new HashSet<double>();

            for (int i = 1; i < points.Count-3; i++)
            {
                Point p1 = points[(i - 1) % points.Count];
                Point p3 = points[(i + 1)];
                Point p5 = points[(i + 3) % points.Count];

                double slope13 = CalculateSlope(p1, p3);
                double slope35 = CalculateSlope(p3, p5);

                // If slope between 5 points is similar it means it's an edge of a shape. (measuring edges not corners)
                if (slope13 == slope35)
                    slopes.Add(slope13);
            }

            return slopes.Count;
        }

        private static double CalculateSlope(Point p1, Point p2) 
        { 
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;

            if (dx == 0)
                return double.PositiveInfinity;

            return dy / dx;
        }

        public static void Classify(Bitmap b, int dir)
        {
            Bitmap newB = ImageAlteration.GrayScale(b);
            newB = Binarization.ApplyStaticThreshold(b, 127);

            List<List <Point>> listInList = FindConnectedComponents(newB);
            Bitmap[] b2 = Split(newB, listInList);

            Console.WriteLine($"Shapes Detected = {listInList.Count}");

            int circle = 1, triangle = 1, square = 1, rectangle = 1;
            for (int i = 0; i < b2.Length; i++)
            {
                b2[i].Save($"ShapeDetection/Shapes{dir}Detected/Shape{i+1}.jpg", ImageFormat.Jpeg);

                Bitmap myB = ImageAlteration.Invert(Laplace(b2[i]));
                myB.Save($"ShapeDetection/Shapes{dir}Laplace/Shape{i+1}.jpg", ImageFormat.Jpeg);

                List<Point> outline = DefineOneShape(myB);

                int numOfSlopes = CalculateUniqueSlopes(outline);
        
                double aspectRatio = (double)myB.Width / myB.Height;
                if (numOfSlopes == 3)
                    b2[i].Save($"ShapeDetection/Shapes{dir}Final/Triangle{triangle++}.jpg", ImageFormat.Jpeg);
                else if (numOfSlopes == 2 && (aspectRatio >= 0.95 && aspectRatio <= 1.05))
                    b2[i].Save($"ShapeDetection/Shapes{dir}Final/Square{square++}.jpg", ImageFormat.Jpeg);
                else if (numOfSlopes == 2)
                    b2[i].Save($"ShapeDetection/Shapes{dir}Final/Rectangle{rectangle++}.jpg", ImageFormat.Jpeg);
                else
                    b2[i].Save($"ShapeDetection/Shapes{dir}Final/Circle{circle++}.jpg", ImageFormat.Jpeg);
            }
        }

    }
}
