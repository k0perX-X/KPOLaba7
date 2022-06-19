using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Media;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GUI;
using Timer = System.Threading.Timer;

namespace TheWarofThreads
{
    class Game
    {
        public Game(Form1 form)
        {
            Form = form;
            Console = new ConsoleClass(Form);
            conMaxWidth = Console.BufferWidth;
            conmass = new char[conMaxHeight + 1, conMaxWidth + 1]; //матрица поля
            mainThread = new Thread(new ThreadStart(Main));
            mainThread.IsBackground = true;
            mainThread.Start();
        }

        public Thread mainThread;

        public class ConsoleClass
        {
            public bool CursorVisible { get; set; }
            public int BufferWidth = 26;

            private const int Scale = 50;

            private char[,] _matrix; //матрица поля
            private char[,] _previousMatrix; //старая матрица поля

            private (int x, int y, bool chenged) ShipPosition = (0, 0, false);

            private Thread _generateImageThread;

            public ConsoleClass(Form1 form)
            {
                _form = form;
                // _generateImageThread = new Thread(new ThreadStart(GenerateImage));
            }

            readonly Form1 _form;

            public string Title
            {
                get => _form.Text;
                set
                {
                    try
                    {
                        _form.Invoke(new MethodInvoker(delegate { _form.Text = value; }));
                    }
                    catch (Exception e)
                    {
                    }
                }
            }

            private int _cursorPositionX = 0;
            private int _cursorPositionY = 0;

            private int _conMaxWidth = 0;
            private int _conMaxHeight = 0;

            private bool init = false;

            public void SetWindowSize(int conMaxWidth, int conMaxHeight)
            {
                _conMaxWidth = conMaxWidth;
                _conMaxHeight = conMaxHeight;
                _matrix = new char[_conMaxWidth, _conMaxHeight];
                _previousMatrix = new char[_conMaxWidth, _conMaxHeight];
                //MainBitmap = new Bitmap(conMaxWidth * Scale - Scale, conMaxHeight * Scale - Scale * 2);
                {
                    var bmp2 = new Bitmap(Scale, Scale);
                    using (var g = Graphics.FromImage(bmp2))
                    {
                        g.InterpolationMode = InterpolationMode.NearestNeighbor;
                        g.DrawImage(PngBullet, new Rectangle(Point.Empty, bmp2.Size));
                        BulletBitmap = bmp2;
                    }
                }
                int i = 0;
                foreach (var rotateFlip in new[]
                         {
                             RotateFlipType.Rotate270FlipNone, RotateFlipType.RotateNoneFlipNone,
                             RotateFlipType.Rotate90FlipNone, RotateFlipType.Rotate180FlipNone
                         })
                {
                    var bmp2 = new Bitmap(Scale, Scale);
                    using (var g = Graphics.FromImage(bmp2))
                    {
                        g.InterpolationMode = InterpolationMode.NearestNeighbor;
                        g.DrawImage(PngInvader, new Rectangle(Point.Empty, bmp2.Size));
                        bmp2.RotateFlip(rotateFlip);
                        InvaderBitmap[i] = bmp2;
                    }

                    i++;
                }

                {
                    var bmp2 = new Bitmap(Scale, Scale);
                    using (var g = Graphics.FromImage(bmp2))
                    {
                        g.InterpolationMode = InterpolationMode.NearestNeighbor;
                        g.DrawImage(PngShip, new Rectangle(Point.Empty, bmp2.Size));
                        ShipBitmap = bmp2;
                    }
                }
                BlackBitmap = new Bitmap(Scale, Scale);
                using (var g = Graphics.FromImage(BlackBitmap))
                    g.Clear(Color.Black);
                Sky = new Bitmap(conMaxWidth * Scale - Scale, conMaxHeight * Scale - Scale);
                using (Graphics g2 = Graphics.FromImage(Sky))
                    g2.Clear(Color.Black);
                // Timer _renderTimer = new Timer(new TimerCallback((object o) => { GenerateImage();}), 0, 0, 100);
                renderThread = new Thread(() =>
                {
                    while (true)
                    {
                        // var t = new Thread(() =>
                        // {
                        //     GenerateImage();
                        // });
                        // t.Start();
                        GenerateImage();
                        Thread.Sleep(100);
                        // t.Join();
                    }
                });
                renderThread.IsBackground = true;
            }

            private Thread renderThread;

            public void SetCursorPosition(int x, int y)
            {
                _cursorPositionX = x;
                _cursorPositionY = y;
            }

            public void Write(char c)
            {
                // Bitmap bmp;
                // switch (c)
                // {
                //     case '-':
                //         bmp = InvaderBitmap[0];
                //         break;
                //     case '\\':
                //         bmp = InvaderBitmap[1];
                //         break;
                //     case '|':
                //         bmp = InvaderBitmap[2];
                //         break;
                //     case '/':
                //         bmp = InvaderBitmap[3];
                //         break;
                //     case '&':
                //         bmp = ShipBitmap;
                //         break;
                //     case '*':
                //         bmp = BulletBitmap;
                //         break;
                //     default:
                //         bmp = BlackBitmap;
                //         break;
                // }
                _matrix[_cursorPositionX, _cursorPositionY] = c;
                if (c == '&')
                    ShipPosition = (_cursorPositionX, _cursorPositionY, true);
                // Debug.Print($"{_cursorPositionX} {_cursorPositionY} {_conMaxWidth} {_conMaxHeight}");
                if (_cursorPositionY == _conMaxHeight - 2 && _cursorPositionX == _conMaxWidth - 1)
                    renderThread.Start();
                //     init = true;
                // if (init) GenerateImage(true);
            }

            private void GenerateImage(bool tf = false)
            {
                void Case(int i, int j, Graphics grph, bool spaceOn = true)
                {
                    Bitmap bmp;
                    var c = _matrix[i, j];
                    switch (c)
                    {
                        case '-':
                            bmp = InvaderBitmap[0];
                            break;
                        case '\\':
                            bmp = InvaderBitmap[1];
                            break;
                        case '|':
                            bmp = InvaderBitmap[2];
                            break;
                        case '/':
                            bmp = InvaderBitmap[3];
                            break;
                        case '&':
                            bmp = ShipBitmap;
                            break;
                        case '*':
                            bmp = BulletBitmap;
                            break;
                        default:
                            if (spaceOn)
                            {
                                bmp = BlackBitmap;
                                break;
                            }
                            else return;
                    }

                    grph.DrawImage(bmp, i * Scale, j * Scale);
                }

                // void PreviousMatrixCase(int i, int j, Graphics grph)
                // {
                //     Bitmap bmp;
                //     var c = _previousMatrix[i, j];
                //     switch (c)
                //     {
                //         case '-':
                //             bmp = BlackBitmap;
                //             break;
                //         case '\\':
                //             bmp = BlackBitmap;
                //             break;
                //         case '|':
                //             bmp = BlackBitmap;
                //             break;
                //         case '/':
                //             bmp = BlackBitmap;
                //             break;
                //         case '&':
                //             bmp = BlackBitmap;
                //             break;
                //         case '*':
                //             bmp = BlackBitmap;
                //             break;
                //         default:
                //             return;
                //     }
                //
                //     grph.DrawImage(bmp, i * Scale, j * Scale);
                // }

                Bitmap fbmp = Sky;
                Graphics g = Graphics.FromImage(fbmp);
                if (tf)
                {
                    Case(_cursorPositionX, _cursorPositionY, g);
                }
                else
                {
                    for (int i = 0; i < _conMaxWidth; i++)
                    for (int j = 0; j < _conMaxHeight; j++)
                    {
                        if (!(i == ShipPosition.x && j == ShipPosition.y) || ShipPosition.chenged)
                        {
                            Case(i, j, g);
                        }
                    }

                    ShipPosition.chenged = false;
                }

                _form.SetImage(fbmp);
                if (!tf)
                {
                    _previousMatrix = _matrix;
                    _matrix = new char[_conMaxWidth, _conMaxHeight];
                }
                // GenerateImage();
            }

            public (ConsoleKey? Key, bool b) ReadKey(bool b)
            {
                while (Keys.Count == 0) ;
                if (Keys.Count == 0) return (null, false);
                else return (Keys.Dequeue(), true);
            }

            public Queue<ConsoleKey> Keys = new Queue<ConsoleKey>();

            public void Beep()
            {
                SystemSounds.Beep.Play();
            }

            public static Bitmap PngBullet;
            public static Bitmap PngInvader;
            public static Bitmap PngShip;

            public Bitmap BulletBitmap;
            public Bitmap ShipBitmap;
            public Bitmap BlackBitmap;

            public Bitmap[] InvaderBitmap = new[]
            {
                new Bitmap(Scale, Scale), //270
                new Bitmap(Scale, Scale), //0
                new Bitmap(Scale, Scale), //90
                new Bitmap(Scale, Scale), //180
            };

            public Bitmap Sky;

            public Bitmap MainBitmap;
        }


        Form1 Form;
        public ConsoleClass Console;

        Thread bdg; //поток создания врагов
        EventWaitHandle startevt; //события начала партии
        Mutex screenlock = new Mutex(); //запрещает одновременую отрисовку
        Semaphore bulletsem = new Semaphore(0, 3); //запрещает боьше 3х пуль
        Semaphore badguyBulletsSem = new Semaphore(0, 10); //запрещает больше 10 пуль
        object gameover = new object(); //заглушка критической секции
        List<Thread> bg = new List<Thread>(); //потоки врагов
        List<Thread> bullets = new List<Thread>(); //потоки пуль
        List<Thread> badguyBullets = new List<Thread>(); //потоки пуль

        bool end = false; //статус конца игры (стоп потоков врагов и пуль)
        public long hit = 0, miss = 0, hp = 5; //попадания\промахи
        static readonly char[] badchar = {'-', '\\', '|', '/'}; //моделька врага
        private const char badguyBulletChar = '+';
        private int conMaxHeight = 24; //размер матрицы\консоли
        private int conMaxWidth;
        char[,] conmass; // = new char[conMaxHeight + 1, conMaxWidth + 1]; //матрица поля
        int bx, by; //стартовые координаты пуль

        private const int MF_BYCOMMAND = 0x00000000;
        public const int SC_CLOSE = 0xF060;
        public const int SC_MINIMIZE = 0xF020;
        public const int SC_MAXIMIZE = 0xF030;
        public const int SC_SIZE = 0xF000;

        [DllImport("user32.dll")]
        public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();

        //message box
        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int MessageBox(IntPtr hWnd, string lpText, string lpCaption, uint uType);

        void Main()
        {
            IntPtr handle = GetConsoleWindow();
            IntPtr sysMenu = GetSystemMenu(handle, false);
            if (handle != IntPtr.Zero)
            {
                DeleteMenu(sysMenu, SC_MINIMIZE, MF_BYCOMMAND);
                DeleteMenu(sysMenu, SC_MAXIMIZE, MF_BYCOMMAND);
                DeleteMenu(sysMenu, SC_SIZE, MF_BYCOMMAND);
            }

            Console.CursorVisible = false; //убрать курсор
            Console.SetWindowSize(conMaxWidth + 1, conMaxHeight + 1); //изменить размер матрицы
            startevt = new EventWaitHandle(false, EventResetMode.ManualReset); //инициализируем событие старта
            score(); //обновление счета
            for (int i = 0; i < conMaxHeight; i++) //циклы для заполнения массива и поля пробелами
            for (int j = 1; j <= conMaxWidth; j++)
            {
                Console.SetCursorPosition(j, i); //курсор в точку
                Console.Write(' '); //вставляем на позицию курсора пробел
                conmass[i, j] = ' '; //аналогичную позицию в массиве приравниваем к пробелу
            }

            int x = conMaxWidth / 2; //середина консоли (позиция пушки)
            int y = conMaxHeight - 1; //нижний ряд в консоли
            bdg = new Thread(new ThreadStart(badguys)); //инициализируем поток создания врагов
            bdg.IsBackground = true;
            bdg.Start(); //запускаем этот поток
            while (true)
            {
                //цикл управление пушкой
                writeat(x, y, '&'); //отрисовка пушки
                var c = Console.ReadKey(true);
                if (c.b)
                    switch (c.Key)
                    {
                        //считывает нажатые клавиши без записи в консоль
                        case ConsoleKey.Spacebar: //нажат пробел
                            startevt.Set(); //ивент начала игры
                            bx = x;
                            by = y - 1; //-1 тк пуля отрисовывется не на месте пушки
                            bullets.Add(new Thread(new ThreadStart(bullet))); //создание потока пуль
                            bullets[bullets.Count - 1].IsBackground = true;
                            bullets[bullets.Count - 1].Start(); //запуск потока пули
                            break;
                        case ConsoleKey.LeftArrow: //нажата левая стрелка
                            if (x - 1 >= 2)
                            {
                                //если есть место для движения влево
                                startevt.Set();
                                writeat(x, y, ' '); //удаляем фигурку пушки
                                x--; //изменяем координаты
                            }

                            break;
                        case ConsoleKey.RightArrow: //нажата правая стрелка
                            if (x + 1 <= conMaxWidth - 2)
                            {
                                startevt.Set();
                                writeat(x, y, ' ');
                                x++;
                            }

                            break;
                        case ConsoleKey.Enter: //(используется для тестов)
                            miss = 30; //сразу 30 пропусков
                            score();
                            break;
                    }
            }
        }

        void bullet() //выстрел
        {
            int x = bx;
            int y = by;
            if (getat(x, y) == '*') return; //если перед пушкой уже есть пуля
            try
            {
                bulletsem.Release();
            } //если меньше 3х-ок, больше завершаем поток
            catch
            {
                return;
            }

            while (y != -1)
            {
                //пока в консоли
                writeat(x, y, '*'); //отрисовка пули
                Thread.Sleep(100);
                if (end) return; //если конец - стоп поток пули
                writeat(x, y, ' '); //отрис пробела на старом месте пули
                y--;
            }

            bulletsem.WaitOne(); //ждет 1 из 3 пуль
        }

        void badguyBullet(int bx, int by)
        {
            int x = bx;
            int y = by;
            if (getat(x, y) == badguyBulletChar) return; //если перед пушкой уже есть пуля
            try
            {
                badguyBulletsSem.Release();
            } 
            catch
            {
                return;
            }

            while (y != conMaxHeight)
            {
                //пока в консоли
                writeat(x, y, badguyBulletChar); //отрисовка пули
                Thread.Sleep(100);
                if (end) return; //если конец - стоп поток пули
                writeat(x, y, ' '); //отрис пробела на старом месте пули
                y++;
            }

            badguyBulletsSem.WaitOne(); 
        }

        void badguy() //враг
        {
            bool hitme = false; //статус удара по врагу
            Random rnd = new Random();
            int y = rnd.Next(0, 10); //случайная высота спавна
            int x = random(0, 100) % 2 == 0 ? 2 : conMaxWidth - 2; //четные спавняться слева нечетные справа
            int dir = x == 2 ? 1 : -1; //направление движения врагов
            while ((dir == 1 && x != conMaxWidth - 2 && !hitme) || (dir == -1 && x != 2 && !hitme))
            {
                writeat(x, y, badchar[x % badchar.Length]); //отрисовка врагов
                // bullet 

                for (int i = 0; i < 10; i++)
                {
                    Thread.Sleep(5 / ((int) hit + (int) miss + 1) + 10); //ускорение
                    if (getat(x, y) == '*')
                    {
                        //если "пуля" попала
                        hitme = true; //изменить статус врага
                        Console.Beep(); //бип
                        break;
                    }
                }

                if (end) return;
                writeat(x, y, ' '); //отрисовать пустоту на старых координатах врага
                if (hitme)
                {
                    Interlocked.Increment(ref hit);
                    score();
                    return;
                } //если во врага попали то изменяем счет и завершаем поток
                else x += dir; //иначе изменяем координаты врага
            }

            if (!hitme)
            {
                Interlocked.Increment(ref miss);
                score();
            } //если во врага так и не попали то изменяем счет
        }

        void badguys() //метод спавна врагов
        {
            startevt.WaitOne(15000); //ждем 15сек до спавна или срабатывания ивента
            while (true)
            {
                if ((random(0, 100) < (hit + miss) / 25 + 20))
                {
                    bg.Add(new Thread(new ThreadStart(badguy)));
                    bg[bg.Count - 1].IsBackground = true;
                    bg[bg.Count - 1].Start();
                }

                Thread.Sleep(1000); //ждем 1сек до спавна нового врага
            }
        }

        char getat(int x, int y) //определяет символ в заданной позиции экрана
        {
            screenlock.WaitOne(); //только 1 поток может использовать этот ресурс
            char res = conmass[y, x];
            screenlock.ReleaseMutex();
            return res;
        }

        void writeat(int x, int y, char res) //отрисовка объектов
        {
            screenlock.WaitOne(); //закрываем мьютекс изолируя ресурс для других потоков
            WriteObject(x, y, res); //сама отрисовка
            conmass[y, x] = res; //изменяем массив с объектами
            screenlock.ReleaseMutex(); //открываем мьютекс
        }

        void WriteObject(int x, int y, char res) //метод отрисовки
        {
            Console.SetCursorPosition(x, y); //ставим курсор в координаты отрисовки
            Console.Write(res); //отрисовываем объект
        }

        void score() //метод обновления покозателей счета
        {
            if (miss > 29)
            {
                //если количество промахов больше или равно 30
                lock (gameover)
                {
                    Console.Title = $"Война потоков - Попаданий: {hit}, Промахов: {miss}, Здоровье: {hp}";
                    bdg.Abort(); //заканчивает поток спавна врагов
                    end = true; //изменяет статус конца игры
                    MessageBox(IntPtr.Zero, "Игра окончена!", "Thread War", 0);
                    Environment.Exit(0); //завершает программу
                }
            }
            else Console.Title = $"Война потоков - Попаданий: {hit}, Промахов: {miss}, Здоровье: {hp}";
        }

        int random(int n0, int n1) //метод генерации случайных чисел
        {
            return new Random().Next(n0, n1); //n0-минимальное значение n1-максимальное
        }
    }
}