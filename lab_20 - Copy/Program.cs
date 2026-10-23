using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace GeoMapApp
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.Run(new MainForm());
        }
    }

    public interface IMapItem
    {
        void Draw(Graphics g);
        void Move(int dx, int dy);
        IMapItem Clone();
        void Print(ListBox lb, int level);
    }

    public class House : IMapItem
    {
        public string Name;
        public int X, Y;

        public House(string name, int x, int y) { Name = name; X = x; Y = y; }
        public void Draw(Graphics g)
        {
            g.FillRectangle(Brushes.SandyBrown, X, Y, 50, 40);
            g.DrawRectangle(Pens.Black, X, Y, 50, 40);
            g.DrawString(Name, SystemFonts.DefaultFont, Brushes.Black, X + 2, Y + 2);
        }
        public void Move(int dx, int dy) { X += dx; Y += dy; }
        public IMapItem Clone() => new House(Name, X, Y);
        public void Print(ListBox lb, int level) =>
            lb.Items.Add(new string('-', level * 2) + " " + Name);
    }

    public class Street : IMapItem
    {
        public string Name;
        public int X, Y;
        public List<IMapItem> Houses = new List<IMapItem>();

        public Street(string name, int x, int y) { Name = name; X = x; Y = y; }
        public void Add(IMapItem house) => Houses.Add(house);

        public void Draw(Graphics g)
        {
            g.DrawString(Name + ":", SystemFonts.DefaultFont, Brushes.DarkSlateGray, X, Y + 12);
            Houses.ForEach(h => h.Draw(g));
        }

        public void Move(int dx, int dy)
        {
            X += dx; Y += dy;
            Houses.ForEach(h => h.Move(dx, dy));
        }

        public IMapItem Clone()
        {
            var copy = new Street(Name, X, Y);
            Houses.ForEach(h => copy.Add(h.Clone()));
            return copy;
        }

        public void Print(ListBox lb, int level)
        {
            lb.Items.Add(new string('-', level * 2) + " " + Name);
            Houses.ForEach(h => h.Print(lb, level + 1));
        }
    }

    public class City : IMapItem
    {
        public string Name;
        public int X, Y;
        public List<IMapItem> Streets = new List<IMapItem>();

        public City(string name, int x, int y) { Name = name; X = x; Y = y; }
        public void Add(IMapItem street) => Streets.Add(street);

        public void Draw(Graphics g)
        {
            g.DrawRectangle(Pens.SteelBlue, X, Y, 470, 390);
            g.DrawString(Name, new Font("Arial", 10, FontStyle.Bold), Brushes.SteelBlue, X + 5, Y + 5);
            Streets.ForEach(s => s.Draw(g));
        }

        public void Move(int dx, int dy)
        {
            X += dx; Y += dy;
            Streets.ForEach(s => s.Move(dx, dy));
        }

        public IMapItem Clone()
        {
            var copy = new City(Name, X, Y);
            Streets.ForEach(s => copy.Add(s.Clone()));
            return copy;
        }

        public void Print(ListBox lb, int level)
        {
            lb.Items.Add(new string('-', level * 2) + " " + Name);
            Streets.ForEach(s => s.Print(lb, level + 1));
        }
    }

    public class MainForm : Form
    {
        private City city;
        private PictureBox pb = new PictureBox { Location = new Point(10, 10), Size = new Size(500, 420), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
        private ListBox lb = new ListBox { Location = new Point(520, 10), Size = new Size(240, 260) };

        private Button btnCloneHouse = new Button { Text = "Клонировать дом", Location = new Point(520, 290), Size = new Size(240, 35) };
        private Button btnCloneStreet = new Button { Text = "Клонировать улицу", Location = new Point(520, 335), Size = new Size(240, 35) };
        private Button btnMove = new Button { Text = "Сдвинуть город", Location = new Point(520, 380), Size = new Size(240, 35) };

        public MainForm()
        {
            this.Text = "Город - Улицы - Дома";
            this.Size = new Size(790, 480);
            this.StartPosition = FormStartPosition.CenterScreen;

            pb.Paint += (s, e) => city?.Draw(e.Graphics);

            btnCloneHouse.Click += (s, e) => AddHouse();
            btnCloneStreet.Click += (s, e) => AddStreet();
            btnMove.Click += (s, e) => { city?.Move(15, 10); UpdateUI(); };

            this.Controls.AddRange(new Control[] { pb, lb, btnCloneHouse, btnCloneStreet, btnMove });

            city = new City("Город", 15, 15);
            var st = new Street("Улица 1", 30, 45);
            st.Add(new House("Дом 1", 100, 45));
            st.Add(new House("Дом 2", 160, 45));
            city.Add(st);

            UpdateUI();
        }

        private void UpdateUI()
        {
            lb.Items.Clear();
            city?.Print(lb, 0);
            pb.Refresh();
        }

        private void AddHouse()
        {
            if (city.Streets.Count == 0) return;
            var curStreet = (Street)city.Streets[city.Streets.Count - 1];

            int count = curStreet.Houses.Count;
            int lastX = count > 0 ? ((House)curStreet.Houses[count - 1]).X + 60 : curStreet.X + 70;
            int lastY = count > 0 ? ((House)curStreet.Houses[count - 1]).Y : curStreet.Y;

            var newHouse = new House("Дом " + (count + 1), lastX, lastY);
            curStreet.Add(newHouse);
            UpdateUI();
        }

        private void AddStreet()
        {
            if (city.Streets.Count == 0) return;
            var firstStreet = (Street)city.Streets[0];

            var newStreet = (Street)firstStreet.Clone();
            newStreet.Name = "Улица " + (city.Streets.Count + 1);
            newStreet.Move(0, city.Streets.Count * 60);

            city.Add(newStreet);
            UpdateUI();
        }
    }
}