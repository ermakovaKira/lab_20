using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace GameEngineApp
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

    public class UnitType
    {
        public string Name;
        public Color UnitColor;
        public string ModelName;

        public UnitType(string name, Color color, string modelName)
        {
            Name = name;
            UnitColor = color;
            ModelName = modelName;
        }
    }

    public static class UnitTypeFactory
    {
        private static Dictionary<string, UnitType> _types = new Dictionary<string, UnitType>();

        public static UnitType GetType(string name, Color color, string modelName)
        {
            if (!_types.ContainsKey(name))
            {
                _types[name] = new UnitType(name, color, modelName);
            }
            return _types[name];
        }

        public static int SharedTypesCount => _types.Count;
    }

    public class Unit
    {
        public int X, Y;
        public int CurrentHp;
        public UnitType Type; 

        public Unit(int x, int y, int hp, UnitType type)
        {
            X = x;
            Y = y;
            CurrentHp = hp;
            Type = type;
        }

        public void Draw(Graphics g)
        {
            using (Brush b = new SolidBrush(Type.UnitColor))
            {
                g.FillEllipse(b, X - 12, Y - 12, 24, 24);
                g.DrawEllipse(Pens.Black, X - 12, Y - 12, 24, 24);
            }
            g.DrawString($"{Type.Name} ({CurrentHp} HP)", SystemFonts.DefaultFont, Brushes.Black, X + 15, Y - 6);
        }
    }

    public interface IArmyFactory
    {
        Unit CreateWarrior(int x, int y);
        Unit CreateArcher(int x, int y);
    }

    public class OrcFactory : IArmyFactory
    {
        public Unit CreateWarrior(int x, int y)
        {
            UnitType sharedType = UnitTypeFactory.GetType("Орк-Воин", Color.DarkSeaGreen, "HeavyOrcModel");
            return new Unit(x, y, 120, sharedType);
        }

        public Unit CreateArcher(int x, int y)
        {
            UnitType sharedType = UnitTypeFactory.GetType("Орк-Лучник", Color.OliveDrab, "LightOrcModel");
            return new Unit(x, y, 75, sharedType);
        }
    }
    public class ElfFactory : IArmyFactory
    {
        public Unit CreateWarrior(int x, int y)
        {
            UnitType sharedType = UnitTypeFactory.GetType("Эльф-Мечник", Color.SkyBlue, "ElfWarriorModel");
            return new Unit(x, y, 90, sharedType);
        }

        public Unit CreateArcher(int x, int y)
        {
            UnitType sharedType = UnitTypeFactory.GetType("Эльф-Лучник", Color.Gold, "ElfArcherModel");
            return new Unit(x, y, 60, sharedType);
        }
    }

    public class MainForm : Form
    {
        private IArmyFactory currentFactory; 
        private List<Unit> units = new List<Unit>();
        private Random rnd = new Random();

        private PictureBox pbMap = new PictureBox { Location = new Point(12, 12), Size = new Size(500, 420), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
        private Label lblStats = new Label { Location = new Point(525, 12), Size = new Size(240, 70), Font = new Font("Arial", 9, FontStyle.Bold) };
        private ListBox lbUnits = new ListBox { Location = new Point(525, 85), Size = new Size(240, 160) };

        private Button btnFactionOrcs = new Button { Text = "Орки", Location = new Point(525, 255), Size = new Size(240, 30), BackColor = Color.DarkSeaGreen };
        private Button btnFactionElves = new Button { Text = "Эльфы", Location = new Point(525, 290), Size = new Size(240, 30), BackColor = Color.SkyBlue };
        private Button btnSpawnWarrior = new Button { Text = "Создать Воина", Location = new Point(525, 330), Size = new Size(115, 35) };
        private Button btnSpawnArcher = new Button { Text = "Создать Лучника", Location = new Point(650, 330), Size = new Size(115, 35) };
        private Button btnSpawnSquad = new Button { Text = "Создать отряд", Location = new Point(525, 370), Size = new Size(240, 32), BackColor = Color.LightYellow };
        private Button btnClear = new Button { Text = "Очистить карту", Location = new Point(525, 405), Size = new Size(240, 27) };

        public MainForm()
        {
            this.Text = "армия армия";
            this.Size = new Size(795, 485);
            this.StartPosition = FormStartPosition.CenterScreen;

            pbMap.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                units.ForEach(u => u.Draw(e.Graphics));
            };

            btnFactionOrcs.Click += (s, e) => SetFaction(new OrcFactory(), "Орки");
            btnFactionElves.Click += (s, e) => SetFaction(new ElfFactory(), "Эльфы");

            btnSpawnWarrior.Click += (s, e) => SpawnUnit(true);
            btnSpawnArcher.Click += (s, e) => SpawnUnit(false);
            btnSpawnSquad.Click += (s, e) => SpawnSquad();
            btnClear.Click += (s, e) => { units.Clear(); UpdateUI(); };

            this.Controls.AddRange(new Control[] {
                pbMap, lblStats, lbUnits,
                btnFactionOrcs, btnFactionElves,
                btnSpawnWarrior, btnSpawnArcher, btnSpawnSquad, btnClear
            });

            SetFaction(new OrcFactory(), "Орки");
        }

        private void SetFaction(IArmyFactory factory, string factionName)
        {
            currentFactory = factory;
            UpdateUI();
        }

        private void SpawnUnit(bool isWarrior)
        {
            int x = rnd.Next(30, pbMap.Width - 120);
            int y = rnd.Next(30, pbMap.Height - 30);

            Unit newUnit = isWarrior
                ? currentFactory.CreateWarrior(x, y)
                : currentFactory.CreateArcher(x, y);

            units.Add(newUnit);
            UpdateUI();
        }

        private void SpawnSquad()
        {
            for (int i = 0; i < 5; i++)
            {
                SpawnUnit(rnd.Next(2) == 0);
            }
        }

        private void UpdateUI()
        {
            lblStats.Text = $"Воинов на карте: {units.Count}\n" +
                            $"Уникальных типов в памяти: {UnitTypeFactory.SharedTypesCount}\n";

            lbUnits.Items.Clear();
            foreach (var u in units)
            {
                lbUnits.Items.Add($"{u.Type.Name} [{u.X}, {u.Y}] (HP: {u.CurrentHp})");
            }

            pbMap.Refresh();
        }
    }
}