using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace MapEdit
{
    public partial class Form1 : Form
    {
        private Point cellSize = new Point(30, 30);
        private Dictionary<int,Tile> tiles = new Dictionary<int, Tile>();
        private int currentId = 1;
        private Connection curConnection;

        public Form1()
        {
            InitializeComponent();
            Load += Form1_Load;
            dataGridView1.SelectionChanged += DataGridView1_SelectionChanged;
            KeyDown += Form1_KeyDown;
            KeyPreview = true;
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.W && comboBox1.SelectedIndex < comboBox1.Items.Count - 1)
                comboBox1.SelectedIndex++;
            else if (e.KeyCode == Keys.S && comboBox1.SelectedIndex > 0)
                comboBox1.SelectedIndex--;
            else if (e.KeyCode == Keys.Space && !connectionMode)
                button2.PerformClick();
            else if (e.KeyCode == Keys.A)
                connectionValueUpDown.Value++;
            else if (e.KeyCode == Keys.D && connectionValueUpDown.Value > 0)
                connectionValueUpDown.Value--;
        }

        
        private void DataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedCells.Count > 0)
            {
                if (!connectionMode)
                {
                    foreach (DataGridViewCell c in dataGridView1.SelectedCells)
                    {
                        if (comboBox1.SelectedItem == null)
                            return;
                        var s = ((Tile)comboBox1.SelectedItem);
                        var nt = Tile.Copy(s);
                        if (nt.IsLogic)
                            nt.ID = currentId++;
                        c.Value = new CellItem() { Type = s.Type, Tile = nt };
                    }
                }
                else
                {
                    var tile = ((CellItem)dataGridView1.SelectedCells[0].Value).Tile;
                    if (!curConnection.First)
                    {

                        if (tile.IsLogic)
                        {
                            curConnection.From = tile.ID;
                            curConnection.First = true;
                        }
                    }
                    else
                    {
                        if (tile.IsLogic)
                        {
                            curConnection.To = tile.ID;
                            curConnection.ConnectionValue = (int)connectionValueUpDown.Value;
                            connectionsListBox.Items.Add(curConnection);
                            curConnection = null;
                            connectionMode = false;
                            checkBox1.Checked = false;
                        }
                    }
                } 
            }
            
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.ColumnHeadersVisible = false;
            dataGridView1.RowTemplate.Height = cellSize.X;
            dataGridView1.AllowUserToResizeRows = false;
            dataGridView1.AllowUserToResizeColumns = false;
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            LoadTileTypes();
            comboBox1.SelectedIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            dataGridView1.RowCount = (int)heightNumeric.Value;
            dataGridView1.ColumnCount = (int)widthNumeric.Value;
            foreach (DataGridViewColumn c in dataGridView1.Columns)
            {
                c.Width = cellSize.Y;
            }
            
        }

        private void LoadTileTypes()
        {
            XDocument xdoc = XDocument.Load("tiles.xml");

            xdoc.Root.Elements().Where(x => x.Name == "object").ToList().ForEach(x => tiles.Add(int.Parse(x.Attribute("id").Value), new Tile(int.Parse(x.Attribute("id").Value), x.Attribute("type").Value) { IsLogic = bool.Parse(x.Attribute("logic").Value), TileColor = PC(x.Attribute("pcol").Value)}));

            tiles.Values.ToList().ForEach(x => comboBox1.Items.Add(x));

        }
        
        class CellItem
        {
            public int Type { get; set; } = 0;
            public Tile Tile { get; set; }

            public int ID
            {
                get
                {
                    return this.Tile.ID;
                }

            }

            public override string ToString()
            {
                if (Type == 0)
                    return "";
                else
                    return Type.ToString() + (Tile.ID != 0 ? ":"+Tile.ID.ToString() : "");
            }

            public string GetValue()
            {
                return Type.ToString() + (Tile.ID != 0 ? ":" + Tile.ID.ToString() : "");
            }

        }

        private Color PC(string s)
        {
            var sa = s.Split(',');
            return Color.FromArgb(int.Parse(sa[0]), int.Parse(sa[1]), int.Parse(sa[2]));
        }

        class Connection
        {
            public int To { get; set; }
            public int From { get; set; }
            public int ConnectionValue { get; set; }

            public bool First { get; set; } = false;

            public Connection()
            {

            }

            public override string ToString()
            {
                return $"From: {From} To: {To} CV: {ConnectionValue}";
            }

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            dataGridView1.MultiSelect = checkBox1.Checked;
        }

        bool connectionMode = false;

        private void button2_Click(object sender, EventArgs e)
        {
            checkBox1.Checked = true;
            connectionMode = true;
            curConnection = new Connection() { ConnectionValue = (int)connectionValueUpDown.Value };
            dataGridView1.ClearSelection();
        }

        private void connectionsListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(connectionsListBox.SelectedItems.Count > 0)
            {
                //idNumeric.Value = ((Connection)connectionsListBox.SelectedItem).ConnectionValue;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (connectionsListBox.SelectedItems.Count > 0)
            {
                connectionsListBox.Items.Remove(connectionsListBox.SelectedItem);
            }
        }

        private void saveBtn_Click(object sender, EventArgs e)
        {
            XDocument xdoc = new XDocument();
            var root = new XElement("hurryupgame");

            StringBuilder sb = new StringBuilder();
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    CellItem t;
                    if (cell.Value != null)
                        t = ((CellItem)cell.Value);
                    else
                        t = new CellItem() { Tile = new Tile(0, "normal") };

                    sb.Append(t.GetValue());
                    if (cell.ColumnIndex != (int)widthNumeric.Value -1)
                        sb.Append(",");
                }
                sb.Append("\n");
            }
            var map = new XElement("map");
            map.Value = sb.ToString();

            var con = new XElement("logic");

            foreach (Connection connection in connectionsListBox.Items.Cast<Connection>())
            {
                var c = new XElement("connection");
                c.SetAttributeValue("connectionvalue", connection.ConnectionValue);
                c.SetAttributeValue("from", connection.From);
                c.SetAttributeValue("to", connection.To);
                con.Add(c);
            }
            root.Add(map);
            root.Add(con);

            xdoc.Add(root);
            xdoc.Save("map.xml");
            

        }

        private void previewBtn_Click(object sender, EventArgs e)
        {
            var d = new PreviewWindow();
            var tiles = new Tile[(int)widthNumeric.Value,(int)heightNumeric.Value];
            
            int y = 0;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                int x = 0;
                foreach (DataGridViewCell cell in row.Cells)
                {
                    Tile t;
                    if (cell.Value != null)
                        t = ((CellItem)cell.Value).Tile;
                    else
                        t = new Tile(0, "normal");

                    tiles[x, y] = t;
                    x++;
                    
                }
                y++;
            }

            d.Data = tiles;
            d.Draw();
            d.ShowDialog();

        }


        private void button4_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "(map)|*.xml";
            if(ofd.ShowDialog() == DialogResult.OK)
            {
                var doc = XDocument.Load(ofd.FileName);
                //Map
                var sa = doc.Root.Element("map").Value.Split('\n');
                dataGridView1.RowCount = sa.Length;
                dataGridView1.ColumnCount = sa[0].Split(',').Length;

                for (int i = 0; i < sa.Length; i++)
                {
                    var sub = sa[i].Split(',');
                    for (int x = 0; x < sub.Length; x++)
                    {
                        var item = sub[x].Split(':');
                        if (item[0] == string.Empty)
                            continue;
                        var tile = Tile.Copy(tiles[int.Parse(item[0])]);
                        dataGridView1.Rows[i].Cells[x].Value = new CellItem() { Tile = tile , Type = tile.Type };
                    }
                }

                foreach (DataGridViewColumn c in dataGridView1.Columns)
                {
                    c.Width = cellSize.Y;
                }

            } 
        }

    }

    public class Tile
    {
        public int ID { get; set; } = 0;
        public string Name { get; set; }

        public bool IsLogic { get; set; }

        public int Type { get; set; }

        public Color TileColor { get; set; }

        public Tile(int t, string name)
        {
            Type = t;
            Name = name;
        }

        public override string ToString()
        {
            return $"{Type}: {Name}";
        }

        public static Tile Copy(Tile s)
        {
            var t = new Tile(s.Type, s.Name);
            t.IsLogic = s.IsLogic;
            t.TileColor = s.TileColor;
            return t;
        }

    }


}
