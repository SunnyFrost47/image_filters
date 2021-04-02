using System;
using System.Drawing;
using System.Windows.Forms;

namespace Program
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            
        }

        public static Bitmap image, image_orig;
        public static string full_name_of_image = "\0";
        public static UInt32[,] pixel;

        //открытие изображения
        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog open_dialog = new OpenFileDialog();
            open_dialog.Filter = "Image Files(*.BMP;*.JPG;*.GIF;*.PNG)|*.BMP;*.JPG;*.GIF;*.PNG|All files (*.*)|*.*";
            if (open_dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    full_name_of_image = open_dialog.FileName;
                    image_orig = new Bitmap(open_dialog.FileName);

                    // Clone a portion of the Bitmap object.
                    Rectangle cloneRect = new Rectangle(0, 0, image_orig.Width, image_orig.Height);
                    image = image_orig.Clone(cloneRect, image_orig.PixelFormat);

                    //this.pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                    //this.Width = image.Width + 40;
                    //this.Height = image.Height + 75;
                    //this.pictureBox1.Size = image.Size;
                    //pictureBox1.Image = image;
                    FromBitmapToScreen();
                    pictureBox1.Invalidate(); //????
                    //получение матрицы с пикселями
                    pixel = new UInt32[image.Height, image.Width];
                    for (int y = 0; y < image.Height; y++)
                        for (int x = 0; x < image.Width; x++)
                            pixel[y, x] = (UInt32)(image.GetPixel(x, y).ToArgb());
                }
                catch
                {
                    full_name_of_image = "\0";
                    DialogResult rezult = MessageBox.Show("Невозможно открыть выбранный файл",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        //сохранение изображения
        private void сохранитьКакToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                //string format = full_name_of_image.Substring(full_name_of_image.Length - 4, 4);
                SaveFileDialog savedialog = new SaveFileDialog();
                savedialog.Title = "Сохранить картинку как...";
                savedialog.OverwritePrompt = true;
                savedialog.CheckPathExists = true;
                savedialog.Filter = "Image Files(*.BMP)|*.BMP|Image Files(*.JPG)|*.JPG|Image Files(*.GIF)|*.GIF|Image Files(*.PNG)|*.PNG|All files (*.*)|*.*";
                savedialog.ShowHelp = true;
                if (savedialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        image.Save(savedialog.FileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                    }
                    catch
                    {
                        MessageBox.Show("Невозможно сохранить изображение", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        //яркость контрастность
        private void яркостьконтрастностьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2 BrightnessForm = new Form2(this);
            BrightnessForm.ShowDialog(); //just 'Show' for the control Form1
        }


        //преобразование из UINT32 to Bitmap
        public static void FromPixelToBitmap()
        {
            for (int y = 0; y < image.Height; y++)
                for (int x = 0; x < image.Width; x++)
                    image.SetPixel(x, y, Color.FromArgb((int)pixel[y, x]));
        }

        //преобразование из UINT32 to Bitmap по одному пикселю
        public static void FromOnePixelToBitmap(int x, int y, UInt32 pixel)
        {
            image.SetPixel(y, x, Color.FromArgb((int)pixel));
        }

        //вывод на экран
        public void FromBitmapToScreen()
        {
            pictureBox1.Image = image;
            pictureBox2.Image = Filter.CalculateBarChart(image, label18,label19,label20);
        }



        private void button3_Click(object sender, EventArgs e)
        {
            // Clone a portion of the Bitmap object.
            Rectangle cloneRect = new Rectangle(0, 0, image_orig.Width, image_orig.Height);
            image = image_orig.Clone(cloneRect, image_orig.PixelFormat);
            FromBitmapToScreen();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedState = comboBox1.SelectedItem.ToString();
            dataGridView1.Rows.Clear();
            dataGridView1.AutoResizeColumns();
            dataGridView1.Refresh();
            dataGridView2.Rows.Clear();
            dataGridView2.AutoResizeColumns();
            dataGridView2.Refresh();

            if (selectedState.Equals("2×2"))
            {
                dataGridView1.RowCount = 2;
                dataGridView1.ColumnCount = 2;
                dataGridView2.RowCount = 2;
                dataGridView2.ColumnCount = 2;
                dataGridView1.Focus();
          
            }

            else if (selectedState.Equals("3×3"))
            {
                dataGridView1.RowCount = 3;
                dataGridView1.ColumnCount = 3;
                dataGridView2.RowCount = 3;
                dataGridView2.ColumnCount = 3;
                dataGridView1.Focus();
            }

            else if (selectedState.Equals("5×5"))
            {
                dataGridView1.RowCount = 5;
                dataGridView1.ColumnCount = 5;
                dataGridView2.RowCount = 5;
                dataGridView2.ColumnCount = 5;
                dataGridView1.Focus();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            double[,] mygx = new double[dataGridView3.Rows.Count, dataGridView3.Rows.Count];

            for (int i = 0; i < dataGridView3.Rows.Count; i++)
            {
                for (int j = 0; j < dataGridView3.ColumnCount; j++)
                {
                    mygx[i,j] = double.Parse(dataGridView3.Rows[i].Cells[j].Value.ToString());
                }
            }

            double delimetr = 1;
            Filter myfilter = new Filter(dataGridView3.Rows.Count, mygx);

            delimetr = Convert.ToDouble(numericUpDown2.Value);
      
            image = myfilter.ApplyFilter(pictureBox1.Image, delimetr);
            FromBitmapToScreen();
        }




        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedState = comboBox2.SelectedItem.ToString();

            if (selectedState.Equals("Фильтр размытия 3х3"))
            {
                dataGridView3.RowCount = 3;
                dataGridView3.ColumnCount = 3;
                comboBox3.SelectedIndex = 1;
                numericUpDown2.Value = 9;
                for (int i = 0; i < dataGridView3.Rows.Count; i++)
                {
                    for (int j = 0; j < dataGridView3.ColumnCount; j++)
                    {
                         dataGridView3.Rows[i].Cells[j].Value = Filter.Kernel.gx_smooth[i, j];
                    }
                }

            }

            if (selectedState.Equals("Фильтр размытия 5х5"))
            {
                dataGridView3.RowCount = 5;
                dataGridView3.ColumnCount = 5;
                comboBox3.SelectedIndex = 2;
                numericUpDown2.Value = 25;
                for (int i = 0; i < dataGridView3.Rows.Count; i++)
                {
                    for (int j = 0; j < dataGridView3.ColumnCount; j++)
                    {
                        dataGridView3.Rows[i].Cells[j].Value = Filter.Kernel.gx_smooth2[i, j];
                    }
                }

            }

            else if (selectedState.Equals("Фильтр повышения резкости"))
            {
                dataGridView3.RowCount = 3;
                dataGridView3.ColumnCount = 3;
                comboBox3.SelectedIndex = 1;
                numericUpDown2.Value = 1;
                for (int i = 0; i < dataGridView3.Rows.Count; i++)
                {
                    for (int j = 0; j < dataGridView3.ColumnCount; j++)
                    {
                        dataGridView3.Rows[i].Cells[j].Value = Filter.Kernel.gx_sharpness[i, j];
                    }
                }
            }

            else if (selectedState.Equals("Фильтр Гаусса 1"))
            {
                dataGridView3.RowCount = 5;
                dataGridView3.ColumnCount = 5;
                comboBox3.SelectedIndex = 2;
                numericUpDown2.Value = Convert.ToDecimal(0.91);
                for (int i = 0; i < dataGridView3.Rows.Count; i++)
                {
                    for (int j = 0; j < dataGridView3.ColumnCount; j++)
                    {
                        dataGridView3.Rows[i].Cells[j].Value = Filter.Kernel.gx_gauss[i, j];
                    }
                }
            }

            else if (selectedState.Equals("Фильтр Гаусса 2"))
            {
                dataGridView3.RowCount = 5;
                dataGridView3.ColumnCount = 5;
                comboBox3.SelectedIndex = 2;
                numericUpDown2.Value = 1;
                for (int i = 0; i < dataGridView3.Rows.Count; i++)
                {
                    for (int j = 0; j < dataGridView3.ColumnCount; j++)
                    {
                        dataGridView3.Rows[i].Cells[j].Value = Filter.Kernel.gx_gauss2[i, j];
                    }
                }
            }

        }


        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedState = comboBox4.SelectedItem.ToString();

            if (selectedState.Equals("Фильтр Робертса"))
            {
                dataGridView1.RowCount = 2;
                dataGridView1.ColumnCount = 2;
                dataGridView2.RowCount = 2;
                dataGridView2.ColumnCount = 2;
                comboBox1.SelectedIndex = 0;
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    for (int j = 0; j < dataGridView1.ColumnCount; j++)
                    {
                        dataGridView1.Rows[i].Cells[j].Value = Filter.Kernel.gx_roberts[i, j];
                    }
                }
                for (int i = 0; i < dataGridView2.Rows.Count; i++)
                {
                    for (int j = 0; j < dataGridView2.ColumnCount; j++)
                    {
                        dataGridView2.Rows[i].Cells[j].Value = Filter.Kernel.gy_roberts[i, j];
                    }
                }

            }

            else if (selectedState.Equals("Фильтр Превитта"))
            {
                dataGridView1.RowCount = 3;
                dataGridView1.ColumnCount = 3;
                dataGridView2.RowCount = 3;
                dataGridView2.ColumnCount = 3;
                comboBox1.SelectedIndex = 1;
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    for (int j = 0; j < dataGridView1.ColumnCount; j++)
                    {
                        dataGridView1.Rows[i].Cells[j].Value = Filter.Kernel.gx_previtt[i, j];
                    }
                }
                for (int i = 0; i < dataGridView2.Rows.Count; i++)
                {
                    for (int j = 0; j < dataGridView2.ColumnCount; j++)
                    {
                        dataGridView2.Rows[i].Cells[j].Value = Filter.Kernel.gy_previtt[i, j];
                    }
                }
            }

            else if (selectedState.Equals("Фильтр Собеля"))
            {
                dataGridView1.RowCount = 3;
                dataGridView1.ColumnCount = 3;
                dataGridView2.RowCount = 3;
                dataGridView2.ColumnCount = 3;
                comboBox1.SelectedIndex = 1;
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    for (int j = 0; j < dataGridView1.ColumnCount; j++)
                    {
                        dataGridView1.Rows[i].Cells[j].Value = Filter.Kernel.gx_sobel[i, j];
                    }
                }
                for (int i = 0; i < dataGridView2.Rows.Count; i++)
                {
                    for (int j = 0; j < dataGridView2.ColumnCount; j++)
                    {
                        dataGridView2.Rows[i].Cells[j].Value = Filter.Kernel.gy_sobel[i, j];
                    }
                }
            }

            else if (selectedState.Equals("Фильтр Кэнни"))
            {
                dataGridView1.RowCount = 5;
                dataGridView1.ColumnCount = 5;
                comboBox1.SelectedIndex = 2;
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    for (int j = 0; j < dataGridView1.ColumnCount; j++)
                    {
                        dataGridView1.Rows[i].Cells[j].Value = Filter.Kernel.gx_canny[i, j];
                    }
                }
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (comboBox4.SelectedItem.ToString().Equals("Фильтр Кэнни"))
            {
                image = Canny.CannyEdgeDetection(pictureBox1.Image, (int)numericUpDown1.Value, checkBox1.Checked);
                FromBitmapToScreen();
                return;
            }

            double[,] mygx = new double[dataGridView1.Rows.Count, dataGridView1.Rows.Count];
            double[,] mygy = new double[dataGridView1.Rows.Count, dataGridView1.Rows.Count];

            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                for (int j = 0; j < dataGridView1.ColumnCount; j++)
                {
                    mygx[i, j] = double.Parse(dataGridView1.Rows[i].Cells[j].Value.ToString());
                }
            }

            for (int i = 0; i < dataGridView2.Rows.Count; i++)
            {
                for (int j = 0; j < dataGridView2.ColumnCount; j++)
                {
                    mygy[i, j] = double.Parse(dataGridView2.Rows[i].Cells[j].Value.ToString());
                }
            }
            Filter myfilter = new Filter(dataGridView1.Rows.Count, mygx, mygy);
            image = myfilter.ApplyDifferenceFilter(pictureBox1.Image, (int)numericUpDown1.Value, checkBox1.Checked);
            FromBitmapToScreen();
            
        }


        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedState = comboBox3.SelectedItem.ToString();
            dataGridView3.Rows.Clear();
            dataGridView3.AutoResizeColumns();
            dataGridView3.Refresh();
            numericUpDown2.Value = 1;

            if (selectedState.Equals("2×2"))
            {
                dataGridView3.RowCount = 2;
                dataGridView3.ColumnCount = 2;
                dataGridView3.Focus();

            }

            else if (selectedState.Equals("3×3"))
            {
                dataGridView3.RowCount = 3;
                dataGridView3.ColumnCount = 3;
                dataGridView3.Focus();
            }

            else if (selectedState.Equals("5×5"))
            {
                dataGridView3.RowCount = 5;
                dataGridView3.ColumnCount = 5;
                dataGridView3.Focus();
            }

        }

    }
}
