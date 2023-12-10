using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ProjectUpdateWF
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = openFile.FileName;
            }
        }


        private void button2_Click(object sender, EventArgs e)
        {
            var path = textBox1.Text; // Get đường dẫn file từ text box 1
            listView1.Clear(); // Xóa tất cả các cột và dữ liệu trong listView1
            listView1.Font = new Font(listView1.Font, FontStyle.Regular);

            if (File.Exists(path))
            {
                HTMLChecker checker = new HTMLChecker(path);
                checker.Start();

                List<Data> Errors = checker.Errors;
                // Hiển thị số lượng function ra text box 3
                textBox3.Text = Errors.Count().ToString();
                if (Errors.Count() > 0)
                {

                    // In dữ liệu ra list view với 2 columns
                    listView1.View = View.Details;
                    listView1.Columns.Add("Errors", 500); // Add column 1 với name và độ rộng
                    listView1.Columns.Add("Line Of Code", 160); // Add column 2 với name và độ rộng
                    listView1.Font = new Font(listView1.Font, FontStyle.Bold);

                    // Chèn dữ liệu vào list view
                    foreach (Data item in Errors)
                    {
                        ListViewItem listViewItem = new ListViewItem(new string[] { item.Content, item.Line.ToString() });// Add từng mảng giá trị tương ứng
                        listView1.Items.Add(listViewItem);
                        listViewItem.Font = new Font(listViewItem.Font, FontStyle.Regular);
                    }
                    listView1.GridLines = true;

                 
                }
            
                else
                {
                    listView1.View = View.Details;
                    listView1.Columns.Add("", 900); // Add column 1 với name và độ rộng


                    // Chèn dữ liệu vào list view
                    foreach (var item in checker.Results)
                    {
                        // Add từng mảng giá trị tương ứng
                        listView1.Items.Add(new ListViewItem(new string[] { item }));
                    }
                }
            }
            else
            {
                MessageBox.Show("File không tồn tại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}

