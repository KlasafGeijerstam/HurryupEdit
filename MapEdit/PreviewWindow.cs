using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MapEdit
{
    public partial class PreviewWindow : Form
    {
        public PreviewWindow()
        {
            InitializeComponent();
            Paint += PreviewWindow_Paint;
        }

        private void PreviewWindow_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            if(Data != null)
            {
                for (int x = 0; x < Data.GetLength(0); x++)
                {
                    for (int y = 0; y < Data.GetLength(1); y++)
                    {
                        g.FillRectangle(new Pen(Data[x, y].TileColor).Brush, 20*x, 20 * y, 20, 20);
                    }
                }
            }    
        }

        public void Draw()
        {
            Invalidate();
        }

        private void PreviewWindow_Load(object sender, EventArgs e)
        {

        }

        public Tile[,] Data { get; set; }

    }
}
