using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;


namespace template
{
    public partial class Form1 : Form
    {
        private readonly Graphics _graphics;
        private readonly Random _rand;
        private IEnumerator _generator;
        private int[,] _maze; //0:道　1:壁 x:列　y:行
        private List<(int src, int dst)> _graph;
        private List<int> _visited;
        private (int x, int y) start, goal;
        private IEnumerator _searcher;
        private int MazeWidth => _maze.GetLength(0);
        private int MazeHeight => _maze.GetLength(1);
        private float CellWidth => (float)pictureBox1.Width / MazeWidth;
        private float CellHeight => (float)pictureBox1.Height / MazeHeight;
        private Stopwatch sw = new Stopwatch();

        public Form1()
        {
            InitializeComponent();
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            pictureBox1.Image = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            comboBox1.SelectedIndex = 0;
            _graphics = Graphics.FromImage(pictureBox1.Image);
            _rand = new Random();
            _visited = new List<int>();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var width = int.Parse(textBox1.Text);
            var height = int.Parse(textBox2.Text);
            label3.Text = "Elapsed Time : ";
            if (width * height % 2 == 0) MessageBox.Show("幅・高さともに奇数を入力してください。", "初期値エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            _visited = new List<int>();
            sw.Reset();
            switch (comboBox1.SelectedIndex)
            {
                default:
                    _generator = StickDownGenerator(width, height);
                    break;
                case 1:
                    _generator = DiggingGenerator(width, height);
                    break;
                case 2:
                    _generator = BranchingGenerator(width, height);
                    break;
            }
            timer1.Start();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            _visited = new List<int>();
            MazeToGraph();
            _searcher = DepthFirstSearcher(start, goal);
        }

        private IEnumerator StickDownGenerator(int width, int height)
        {
            sw.Start();
            _maze = new int[width, height];
            start = (1, 1);
            goal = (width - 2, height - 2);

            for (var x = 0; x < MazeWidth; x++)
            {
                for (var y = 0; y < MazeHeight; y++)
                {
                    if (x == 0 || x == MazeWidth - 1 || y == 0 || y == MazeHeight - 1)
                    {
                        _maze[x, y] = 1;
                    }
                    else if (((x + 1) * (y + 1)) % 2 == 1)
                    {
                        _maze[x, y] = 1;

                        int r;
                        if (y == 2)
                        {
                            r = _rand.Next(3);
                            switch (r)
                            {
                                case 0:
                                    _maze[x - 1, y] = 1;
                                    break;
                                case 1:
                                    _maze[x, y - 1] = 1;
                                    break;
                                case 2:
                                    _maze[x + 1, y] = 1;
                                    break;
                                default:
                                    break;
                            }
                        }
                        else
                        {
                            r = _rand.Next(4);
                            switch (r)
                            {
                                case 0:
                                    _maze[x - 1, y] = 1;
                                    break;
                                case 1:
                                    _maze[x, y - 1] = 1;
                                    break;
                                case 2:
                                    _maze[x + 1, y] = 1;
                                    break;
                                case 3:
                                    _maze[x, y + 1] = 1;
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
                yield return null;
            }
            sw.Stop();
            label3.Text = "Elapsed Time : " + (Double)sw.ElapsedMilliseconds / 1000 + " [s]";
        }

        private IEnumerator DiggingGenerator(int width, int height)
        {
            // 穴掘り法
            sw.Start();
            _maze = new int[width + 2, height + 2];
            start = (2, 2);
            goal = (MazeWidth - 3, MazeHeight - 3);

            for (var x = 0; x < MazeWidth; x++)
            {
                for (var y = 0; y < MazeHeight; y++)
                {
                    if (x != 0 && x != MazeWidth - 1 && y != 0 && y != MazeHeight - 1)
                    {
                        _maze[x, y] = 1;
                    }
                }
            }
            List<int> xL = new List<int> { _rand.Next(1, (MazeWidth + 1) / 2 - 2) * 2 };
            List<int> yL = new List<int> { _rand.Next(1, (MazeHeight + 1) / 2 - 2) * 2 };
            int cnt = 0;
            _maze[xL[cnt], yL[cnt]] = 0;
            while (true)
            {
                if (_maze[xL[cnt], yL[cnt] - 2] == 0 && _maze[xL[cnt] - 2, yL[cnt]] == 0 && _maze[xL[cnt], yL[cnt] + 2] == 0 && _maze[xL[cnt] + 2, yL[cnt]] == 0)
                {
                    xL.RemoveAt(cnt);
                    yL.RemoveAt(cnt);
                    if (xL.Count == 0) break;
                    cnt = _rand.Next(xL.Count - 1);
                    continue;
                }

                int r = _rand.Next(4);
                switch (r)
                {
                    case 0:
                        if (_maze[xL[cnt], yL[cnt] - 2] == 1)
                        {
                            _maze[xL[cnt], yL[cnt] - 1] = 0;
                            _maze[xL[cnt], yL[cnt] - 2] = 0;
                            xL.Add(xL[cnt]);
                            yL.Add(yL[cnt] - 2);
                            cnt = xL.Count - 1;
                            yield return null;
                            continue;
                        }
                        break;
                    case 1:
                        if (_maze[xL[cnt] - 2, yL[cnt]] == 1)
                        {
                            _maze[xL[cnt] - 1, yL[cnt]] = 0;
                            _maze[xL[cnt] - 2, yL[cnt]] = 0;
                            xL.Add(xL[cnt] - 2);
                            yL.Add(yL[cnt]);
                            cnt = xL.Count - 1;
                            yield return null;
                            continue;
                        }
                        break;
                    case 2:
                        if (_maze[xL[cnt], yL[cnt] + 2] == 1)
                        {
                            _maze[xL[cnt], yL[cnt] + 1] = 0;
                            _maze[xL[cnt], yL[cnt] + 2] = 0;
                            xL.Add(xL[cnt]);
                            yL.Add(yL[cnt] + 2);
                            cnt = xL.Count - 1;
                            yield return null;
                            continue;
                        }
                        break;
                    case 3:
                        if (_maze[xL[cnt] + 2, yL[cnt]] == 1)
                        {
                            _maze[xL[cnt] + 1, yL[cnt]] = 0;
                            _maze[xL[cnt] + 2, yL[cnt]] = 0;
                            xL.Add(xL[cnt] + 2);
                            yL.Add(yL[cnt]);
                            cnt = xL.Count - 1;
                            yield return null;
                            continue;
                        }
                        break;
                    default:
                        break;
                }
                cnt = _rand.Next(xL.Count - 1);
            }
            sw.Stop();
            label3.Text = "Elapsed Time : " + (Double)sw.ElapsedMilliseconds / 1000 + " [s]";
            yield return null;
        }

        private IEnumerator BranchingGenerator(int width, int height)
        {
            sw.Start();
            _maze = new int[width + 4, height + 4];
            start = (3, 3);
            goal = (MazeWidth - 4, MazeHeight - 4);
            List<int> xL = new List<int>();
            List<int> yL = new List<int>();
            int cnt = 0;

            for (int x = 0; x < MazeWidth; x++)
            {
                for (int y = 0; y < MazeHeight; y++)
                {
                    if (x < 3 || x > MazeWidth - 4 || y < 3 || y > MazeHeight - 4) _maze[x, y] = 1;
                    if ((x == 2 || x == MazeWidth - 3) && y > 3 && y < MazeHeight - 4 && y % 2 == 0)
                    {
                        xL.Add(x);
                        yL.Add(y);

                    }
                    else if (x > 3 && x < MazeWidth - 4 && (y == 2 || y == MazeHeight - 3) && x % 2 == 0)
                    {
                        xL.Add(x);
                        yL.Add(y);
                    }
                }
            }
            cnt = _rand.Next(xL.Count - 1);
            while (true)
            {
                if (_maze[xL[cnt], yL[cnt] - 2] == 1 && _maze[xL[cnt] - 2, yL[cnt]] == 1 && _maze[xL[cnt], yL[cnt] + 2] == 1 && _maze[xL[cnt] + 2, yL[cnt]] == 1)
                {
                    xL.RemoveAt(cnt);
                    yL.RemoveAt(cnt);
                    if (xL.Count == 0) break;
                    cnt = _rand.Next(xL.Count - 1);
                    continue;
                }
                switch (_rand.Next(4))
                {
                    case 0:
                        if (_maze[xL[cnt], yL[cnt] - 2] == 0)
                        {
                            _maze[xL[cnt], yL[cnt] - 1] = 1;
                            _maze[xL[cnt], yL[cnt] - 2] = 1;
                            xL.Add(xL[cnt]);
                            yL.Add(yL[cnt] - 2);
                            cnt = xL.Count - 1;
                            yield return null;
                            continue;
                        }
                        break;
                    case 1:
                        if (_maze[xL[cnt] - 2, yL[cnt]] == 0)
                        {
                            _maze[xL[cnt] - 1, yL[cnt]] = 1;
                            _maze[xL[cnt] - 2, yL[cnt]] = 1;
                            xL.Add(xL[cnt] - 2);
                            yL.Add(yL[cnt]);
                            cnt = xL.Count - 1;
                            yield return null;
                            continue;
                        }
                        break;
                    case 2:
                        if (_maze[xL[cnt], yL[cnt] + 2] == 0)
                        {
                            _maze[xL[cnt], yL[cnt] + 1] = 1;
                            _maze[xL[cnt], yL[cnt] + 2] = 1;
                            xL.Add(xL[cnt]);
                            yL.Add(yL[cnt] + 2);
                            cnt = xL.Count - 1;
                            yield return null;
                            continue;
                        }
                        break;
                    case 3:
                        if (_maze[xL[cnt] + 2, yL[cnt]] == 0)
                        {
                            _maze[xL[cnt] + 1, yL[cnt]] = 1;
                            _maze[xL[cnt] + 2, yL[cnt]] = 1;
                            xL.Add(xL[cnt] + 2);
                            yL.Add(yL[cnt]);
                            cnt = xL.Count - 1;
                            yield return null;
                            continue;
                        }
                        break;
                    default:
                        break;
                }
                cnt = _rand.Next(xL.Count - 1);
            }
            sw.Stop();
            label3.Text = "Elapsed Time : " + (Double)sw.ElapsedMilliseconds / 1000 + " [s]";
            yield return null;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            // 迷路を実際に生成する部分。
            _generator.MoveNext();
            _searcher?.MoveNext();

            // _maze の内容が PictureBox に反映されるようにしている部分。
            _graphics.Clear(Color.White);
            for (var x = 0; x < MazeWidth; x++)
            {
                for (var y = 0; y < MazeHeight; y++)
                {
                    if (_maze[x, y] == 1)
                    {
                        _graphics.FillRectangle(Brushes.Black, x * CellWidth, y * CellHeight, CellWidth, CellHeight);
                    }
                    else if (_visited.Contains(x + y * MazeWidth))
                    {
                        _graphics.FillRectangle(Brushes.Blue, x * CellWidth, y * CellHeight, CellWidth, CellHeight);
                    }

                }
            }
            pictureBox1.Refresh();
        }

        private void MazeToGraph()
        {
            _graph = new List<(int src, int dst)>();
            // 道の番号 = 左上から→に
            // 辺リスト
            for (int x = 1; x < MazeWidth - 1; x++)
            {
                for (int y = 1; y < MazeHeight - 1; y++)
                {
                    if (_maze[x, y] == 0)
                    {
                        if (_maze[x + 1, y] == 0) _graph.Add((x + MazeWidth * y, (x + 1) + MazeWidth * y));
                        if (_maze[x, y + 1] == 0) _graph.Add((x + MazeWidth * y, x + MazeWidth * (y + 1)));
                    }
                }
            }
        }

        private IEnumerator DepthFirstSearcher((int x, int y) start, (int x, int y) goal)
        {
            _visited = new List<int> { start.x + start.y * MazeWidth };
            List<int> stack = new List<int>() { start.x + start.y * MazeWidth };
            int num = 0;
            // 深さ優先探索の実装。
            while (stack.Count != 0)
            {
                num = stack[stack.Count - 1];
                stack.RemoveAt(stack.Count - 1);
                foreach (var g in _graph)
                {
                    if (g.src == num && !_visited.Contains(g.dst))
                    {
                        stack.Add(g.dst);
                        _visited.Add(g.dst);
                    }

                    if (g.dst == num && !_visited.Contains(g.src))
                    {
                        stack.Add(g.src);
                        _visited.Add(g.src);
                    }
                }
                if (_visited.Contains(goal.x + goal.y * MazeWidth)) break;
                yield return null;
            }
        }
    }
}