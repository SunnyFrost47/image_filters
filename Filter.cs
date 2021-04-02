using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Program
{
    public class Filter
    {
        public Kernel kernel;
        public static double sum;

        public class Kernel
        {
            public int size;
            public double[,] gx;
            public double[,] gy;

            public static double[,] gx_roberts = new double[,] { { 1, 0 }, { 0, -1 } };
            public static double[,] gy_roberts = new double[,] { { 0, 1 }, { -1, 0 } };

            public static double[,] gx_previtt = new double[,] { { -1, 0, 1 }, { -1, 0, 1 }, { -1, 0, 1 } };
            public static double[,] gy_previtt = new double[,] { { -1, -1, -1 }, { 0, 0, 0 }, { 1, 1, 1 } };

            public static double[,] gx_sobel = new double[,] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
            public static double[,] gy_sobel = new double[,] { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } };

            public static double[,] gx_canny = new double[,] { { 2, 4, 5, 4, 2 }, { 4, 9, 12, 9, 4 }, { 5, 12, 15, 12, 5 }, { 4, 9, 12, 9, 4 }, { 2, 4, 5, 4, 2 } };

            public static double[,] gx_smooth = new double[,] { { 1, 1, 1 }, { 1, 1, 1 }, { 1, 1, 1 } };

            public static double[,] gx_smooth2 = new double[,] { { 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1 } };

            public static double[,] gx_sharpness = new double[,] { { -1, -1, -1 }, { -1, 9, -1 }, { -1, -1, -1 } };

            public static double[,] gx_gauss = new double[,] { { 0.003, 0.013, 0.022, 0.013, 0.003 }, { 0.013, 0.059, 0.097, 0.059, 0.013 }, { 0.022, 0.097, 0.159, 0.097, 0.022 }, { 0.013, 0.059, 0.097, 0.059, 0.013 }, { 0.003, 0.013, 0.022, 0.013, 0.003 } };
            public static double[,] gx_gauss2 = new double[,] { { 0.000789, 0.006581, 0.013347, 0.006581, 0.000789 }, { 0.006581, 0.054901, 0.111345, 0.054901, 0.006581 }, { 0.013347, 0.111345, 0.225821, 0.111345, 0.013347 }, { 0.006581, 0.054901, 0.111345, 0.054901, 0.006581 }, { 0.000789, 0.006581, 0.013347, 0.006581, 0.000789 } };

            public Kernel(int Size, double[,] Gx, double[,] Gy)
            {
                size = Size;
                gx = Copy(Gx);
                gy = Copy(Gy);
            }

            public Kernel(int Size, double[,] Gx)
            {
                sum = 0;
                size = Size;
                gx = Copy(Gx);
                for (int i = 0; i < gx.GetLength(0); i++)
                    for (int j = 0; j < gx.GetLength(1); j++)
                        sum += gx[i, j];

            }


            static T[,] Copy<T>(T[,] array)
            {
                T[,] newArray = new T[array.GetLength(0), array.GetLength(1)];
                for (int i = 0; i < array.GetLength(0); i++)
                    for (int j = 0; j < array.GetLength(1); j++)
                        newArray[i, j] = array[i, j];
                return newArray;
            }

        }


        public Filter(int Size, double[,] Gx, double[,] Gy)
        {
            kernel = new Kernel(Size, Gx, Gy);
        }

        public Filter(int Size, double[,] Gx)
        {
            kernel = new Kernel(Size, Gx);
        }


        public Bitmap ApplyDifferenceFilter(Image image1, int limit, bool bordcolor)
        {
            Bitmap b = new Bitmap(image1);
            Bitmap bb = new Bitmap(image1);

            // Turn image into gray scale image
            //Bitmap grayScaleBP = MakeGrayscale3(new Bitmap(pictureBox1.Image));

            int width = b.Width;
            int height = b.Height;

            int[,] allPixR = new int[width, height];
            int[,] allPixG = new int[width, height];
            int[,] allPixB = new int[width, height];

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    allPixR[i, j] = b.GetPixel(i, j).R;
                    allPixG[i, j] = b.GetPixel(i, j).G;
                    allPixB[i, j] = b.GetPixel(i, j).B;
                }
            }

            int new_rx = 0, new_ry = 0, new_rmag = 0;
            int new_gx = 0, new_gy = 0, new_gmag = 0;
            int new_bx = 0, new_by = 0, new_bmag = 0;
            int rc, gc, bc;

            if (this.kernel.size == 2)
            {
                for (int i = 0; i < b.Width - 1; i++)
                {
                    for (int j = 0; j < b.Height - 1; j++)
                    {

                        new_rx = 0;
                        new_ry = 0;
                        new_gx = 0;
                        new_gy = 0;
                        new_bx = 0;
                        new_by = 0;

                        for (int wi = 0; wi < 2; wi++)
                        {
                            for (int hw = 0; hw < 2; hw++)
                            {
                                rc = allPixR[i + hw, j + wi];
                                new_rx += (int)(this.kernel.gx[wi, hw] * rc);
                                new_ry += (int)(this.kernel.gy[wi, hw] * rc);

                                gc = allPixG[i + hw, j + wi];
                                new_gx += (int)(this.kernel.gx[wi, hw] * gc);
                                new_gy += (int)(this.kernel.gy[wi, hw] * gc);

                                bc = allPixB[i + hw, j + wi];
                                new_bx += (int)(this.kernel.gx[wi, hw] * bc);
                                new_by += (int)(this.kernel.gy[wi, hw] * bc);
                            }
                        }

                        new_rmag = (int)Math.Sqrt(new_rx * new_rx + new_ry * new_ry);
                        new_gmag = (int)Math.Sqrt(new_gx * new_gx + new_gy * new_gy);
                        new_bmag = (int)Math.Sqrt(new_bx * new_bx + new_by * new_by);


                        if ((new_rmag > limit) || (new_gmag > limit) || (new_bmag > limit))
                        {
                            if (bordcolor == true)
                                bb.SetPixel(i, j, Color.White);
                            else
                                bb.SetPixel(i, j, Color.Black);
                        }

                        else
                        {
                            if (bordcolor == true)
                                bb.SetPixel(i, j, Color.Black);
                            else
                                bb.SetPixel(i, j, Color.White);
                        }
                            
                    }
                }
            }


            else if (this.kernel.size == 3)
            {
                for (int i = 1; i < b.Width - 1; i++)
                {
                    for (int j = 1; j < b.Height - 1; j++)
                    {

                        new_rx = 0;
                        new_ry = 0;
                        new_gx = 0;
                        new_gy = 0;
                        new_bx = 0;
                        new_by = 0;

                        for (int wi = -1; wi < 2; wi++)
                        {
                            for (int hw = -1; hw < 2; hw++)
                            {
                                rc = allPixR[i + hw, j + wi];
                                new_rx += (int)(this.kernel.gx[wi + 1, hw + 1] * rc);
                                new_ry += (int)(this.kernel.gy[wi + 1, hw + 1] * rc);

                                gc = allPixG[i + hw, j + wi];
                                new_gx += (int)(this.kernel.gx[wi + 1, hw + 1] * gc);
                                new_gy += (int)(this.kernel.gy[wi + 1, hw + 1] * gc);

                                bc = allPixB[i + hw, j + wi];
                                new_bx += (int)(this.kernel.gx[wi + 1, hw + 1] * bc);
                                new_by += (int)(this.kernel.gy[wi + 1, hw + 1] * bc);
                            }
                        }

                        new_rmag = (int)Math.Sqrt(new_rx * new_rx + new_ry * new_ry);
                        new_gmag = (int)Math.Sqrt(new_gx * new_gx + new_gy * new_gy);
                        new_bmag = (int)Math.Sqrt(new_bx * new_bx + new_by * new_by);

                        if ((new_rmag > limit) || (new_gmag > limit) || (new_bmag > limit))
                        {
                            if (bordcolor == true)
                                bb.SetPixel(i, j, Color.White);
                            else
                                bb.SetPixel(i, j, Color.Black);
                        }

                        else
                        {
                            if (bordcolor == true)
                                bb.SetPixel(i, j, Color.Black);
                            else
                                bb.SetPixel(i, j, Color.White);
                        }

                        /*
                        if (mode == 1)
                        {
                            if ((new_rmag > (limit * 0.11)) || (new_gmag > (limit * 0.59)) || (new_bmag > (limit * 0.3)))
                                bb.SetPixel(i, j, Color.White);

                            else
                                bb.SetPixel(i, j, Color.Black);
                        }
                        */
                    }
                }
            }

            else if (this.kernel.size == 5)
            {
                for (int i = 2; i < b.Width - 2; i++)
                {
                    for (int j = 2; j < b.Height - 2; j++)
                    {

                        new_rx = 0;
                        new_ry = 0;
                        new_gx = 0;
                        new_gy = 0;
                        new_bx = 0;
                        new_by = 0;

                        for (int wi = -2; wi < 3; wi++)
                        {
                            for (int hw = -2; hw < 3; hw++)
                            {
                                rc = allPixR[i + hw, j + wi];
                                new_rx += (int)(this.kernel.gx[wi + 2, hw + 2] * rc);
                                new_ry += (int)(this.kernel.gy[wi + 2, hw + 2] * rc);

                                gc = allPixG[i + hw, j + wi];
                                new_gx += (int)(this.kernel.gx[wi + 2, hw + 2] * gc);
                                new_gy += (int)(this.kernel.gy[wi + 2, hw + 2] * gc);

                                bc = allPixB[i + hw, j + wi];
                                new_bx += (int)(this.kernel.gx[wi + 2, hw + 2] * bc);
                                new_by += (int)(this.kernel.gy[wi + 2, hw + 2] * bc);
                            }
                        }



                        if ((new_rmag > limit) || (new_gmag > limit) || (new_bmag > limit))
                        {
                            if (bordcolor == true)
                                bb.SetPixel(i, j, Color.White);
                            else
                                bb.SetPixel(i, j, Color.Black);
                        }

                        else
                        {
                            if (bordcolor == true)
                                bb.SetPixel(i, j, Color.Black);
                            else
                                bb.SetPixel(i, j, Color.White);
                        }
                    }

                }
            }


            return bb;
        }

        public Bitmap ApplyFilter(Image image1, double div)
        {
            Bitmap b = new Bitmap(image1);
            Bitmap bb = new Bitmap(image1);

            // Turn image into gray scale image
            //Bitmap grayScaleBP = MakeGrayscale3(new Bitmap(pictureBox1.Image));

            int width = b.Width;
            int height = b.Height;

            int[,] allPixR = new int[width, height];
            int[,] allPixG = new int[width, height];
            int[,] allPixB = new int[width, height];

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    allPixR[i, j] = b.GetPixel(i, j).R;
                    allPixG[i, j] = b.GetPixel(i, j).G;
                    allPixB[i, j] = b.GetPixel(i, j).B;
                }
            }

            int new_rx = 0, new_rmag = 0;
            int new_gx = 0, new_gmag = 0;
            int new_bx = 0, new_bmag = 0;
            int rc, gc, bc;

            if (this.kernel.size == 2)
            {
                for (int i = 0; i < b.Width - 1; i++)
                {
                    for (int j = 0; j < b.Height - 1; j++)
                    {

                        new_rx = 0;
                        new_gx = 0;
                        new_bx = 0;

                        for (int wi = 0; wi < 2; wi++)
                        {
                            for (int hw = 0; hw < 2; hw++)
                            {
                                rc = allPixR[i + hw, j + wi];
                                new_rx += (int)(this.kernel.gx[wi, hw] * rc);

                                gc = allPixG[i + hw, j + wi];
                                new_gx += (int)(this.kernel.gx[wi, hw] * gc);

                                bc = allPixB[i + hw, j + wi];
                                new_bx += (int)(this.kernel.gx[wi, hw] * bc);
                            }
                        }

                        new_rmag = (int)(new_rx / div);
                        if (new_rmag < 0)
                            new_rmag = 0;
                        if (new_rmag > 255)
                            new_rmag = 255;
                        new_gmag = (int)(new_gx / div);
                        if (new_gmag < 0)
                            new_gmag = 0;
                        if (new_gmag > 255)
                            new_gmag = 255;
                        new_bmag = (int)(new_bx / div);
                        if (new_bmag < 0)
                            new_bmag = 0;
                        if (new_bmag > 255)
                            new_bmag = 255;

                        bb.SetPixel(i, j, Color.FromArgb(new_rmag, new_gmag, new_bmag));
                    }
                }
            }

            else if (this.kernel.size == 3)
            {
                for (int i = 1; i < b.Width - 1; i++)
                {
                    for (int j = 1; j < b.Height - 1; j++)
                    {

                        new_rx = 0;
                        new_gx = 0;
                        new_bx = 0;

                        for (int wi = -1; wi < 2; wi++)
                        {
                            for (int hw = -1; hw < 2; hw++)
                            {
                                rc = allPixR[i + hw, j + wi];
                                new_rx += (int)(this.kernel.gx[wi + 1, hw + 1] * rc);

                                gc = allPixG[i + hw, j + wi];
                                new_gx += (int)(this.kernel.gx[wi + 1, hw + 1] * gc);

                                bc = allPixB[i + hw, j + wi];
                                new_bx += (int)(this.kernel.gx[wi + 1, hw + 1] * bc);
                            }
                        }


                        new_rmag = (int)(new_rx / div);
                        if (new_rmag < 0)
                            new_rmag = 0;
                        if (new_rmag > 255)
                            new_rmag = 255;
                        new_gmag = (int)(new_gx / div);
                        if (new_gmag < 0)
                            new_gmag = 0;
                        if (new_gmag > 255)
                            new_gmag = 255;
                        new_bmag = (int)(new_bx / div);
                        if (new_bmag < 0)
                            new_bmag = 0;
                        if (new_bmag > 255)
                            new_bmag = 255;

                        bb.SetPixel(i, j, Color.FromArgb(new_rmag, new_gmag, new_bmag));
                    }

                }
            }


            else if (this.kernel.size == 5)
            {
                for (int i = 2; i < b.Width - 2; i++)
                {
                    for (int j = 2; j < b.Height - 2; j++)
                    {

                        new_rx = 0;
                        new_gx = 0;
                        new_bx = 0;

                        for (int wi = -2; wi < 3; wi++)
                        {
                            for (int hw = -2; hw < 3; hw++)
                            {
                                rc = allPixR[i + hw, j + wi];
                                new_rx += (int)(this.kernel.gx[wi + 2, hw + 2] * rc);

                                gc = allPixG[i + hw, j + wi];
                                new_gx += (int)(this.kernel.gx[wi + 2, hw + 2] * gc);

                                bc = allPixB[i + hw, j + wi];
                                new_bx += (int)(this.kernel.gx[wi + 2, hw + 2] * bc);
                            }
                        }


                        new_rmag = (int)(new_rx / div);
                        if (new_rmag < 0)
                            new_rmag = 0;
                        if (new_rmag > 255)
                            new_rmag = 255;
                        new_gmag = (int)(new_gx / div);
                        if (new_gmag < 0)
                            new_gmag = 0;
                        if (new_gmag > 255)
                            new_gmag = 255;
                        new_bmag = (int)(new_bx / div);
                        if (new_bmag < 0)
                            new_bmag = 0;
                        if (new_bmag > 255)
                            new_bmag = 255;

                        bb.SetPixel(i, j, Color.FromArgb(new_rmag, new_gmag, new_bmag));
                    }

                }
            }

            return bb;
        }


        public static Bitmap MakeGrayscale3(Bitmap original)
        {

            //Gray  = Green * 0.59 + Blue * 0.30 + Red * 0.11;

            //create a blank bitmap the same size as original
            Bitmap newBitmap = new Bitmap(original.Width, original.Height);

            //get a graphics object from the new image
            using (Graphics g = Graphics.FromImage(newBitmap))
            {

                //create the grayscale ColorMatrix
                ColorMatrix colorMatrix = new ColorMatrix(
                   new float[][]
                   {
             new float[] {.3f, .3f, .3f, 0, 0},
             new float[] {.59f, .59f, .59f, 0, 0},
             new float[] {.11f, .11f, .11f, 0, 0},
             new float[] {0, 0, 0, 1, 0},
             new float[] {0, 0, 0, 0, 1}
                   });

                //create some image attributes
                using (ImageAttributes attributes = new ImageAttributes())
                {

                    //set the color matrix attribute
                    attributes.SetColorMatrix(colorMatrix);

                    //draw the original image on the new image
                    //using the grayscale color matrix
                    g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
                                0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);
                }
            }
            return newBitmap;
        }

        

        public static Image CalculateBarChart(Bitmap bmp, Label l1, Label l2, Label l3)
        {
            int[] histogram_r = new int[256];
            int[] histogram_g = new int[256];
            int[] histogram_b = new int[256];
            float max_r = 0;
            float max_g = 0;
            float max_b = 0;

            int redValue, greenValue, blueValue;

            for (int i = 0; i < bmp.Width; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    redValue = bmp.GetPixel(i, j).R;
                    greenValue = bmp.GetPixel(i, j).G;
                    blueValue = bmp.GetPixel(i, j).B;

                    histogram_r[redValue]++;
                    if (max_r < histogram_r[redValue])
                        max_r = histogram_r[redValue];
                    histogram_g[greenValue]++;
                    if (max_g < histogram_g[greenValue])
                        max_g = histogram_g[greenValue];
                    histogram_b[blueValue]++;
                    if (max_b < histogram_b[blueValue])
                        max_b = histogram_b[blueValue];
                }
            }

            int histHeight = 128;
            Bitmap img = new Bitmap(256, histHeight*3 + 70);

            using (Graphics g = Graphics.FromImage(img))
            {
                l1.Text = max_r.ToString();
                for (int i = 0; i < histogram_r.Length; i++)
                {
                    float pct = histogram_r[i] / max_r;   // What percentage of the max is this value?
                    g.DrawLine(Pens.Red,
                        new Point(i, img.Height - 10),
                        new Point(i, img.Height - 10 - (int)(pct * histHeight))  // Use that percentage of the height
                        );
                }
                l2.Text = max_g.ToString();
                for (int i = 0; i < histogram_g.Length; i++)
                {
                    float pct = histogram_g[i] / max_g;   // What percentage of the max is this value?
                    g.DrawLine(Pens.Green,
                        new Point(i, img.Height - 35 - 128),
                        new Point(i, img.Height - 35 - 128 - (int)(pct * histHeight))  // Use that percentage of the height
                        );
                }
                l3.Text = max_b.ToString();
                for (int i = 0; i < histogram_b.Length; i++)
                {
                    float pct = histogram_b[i] / max_b;   // What percentage of the max is this value?
                    g.DrawLine(Pens.Blue,
                        new Point(i, img.Height - 60 - 256),
                        new Point(i, img.Height - 60 - 256 - (int)(pct * histHeight))  // Use that percentage of the height
                        );
                }
            }
            
            return img;
        }
    }
}
