using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Media;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GUI;

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
            Main();
        }

        public class ConsoleClass
        {
            public bool CursorVisible { get; set; }
            public int BufferWidth { get; set; }

            private const int Scale = 50;

            public ConsoleClass(Form1 form)
            {
                Form = form;
            }

            Form1 Form;

            public string Title
            {
                get => Form.Name;
                set => Form.Name = value;
            }

            private int _cursorPositionX = 0;
            private int _cursorPositionY = 0;

            public void SetWindowSize(int conMaxWidth, int conMaxHeight)
            {
                Form.Bitmap = new Bitmap(conMaxWidth * Scale, conMaxHeight * Scale);
                {
                    var bmp2 = new Bitmap(Scale, Scale);
                    using (var g = Graphics.FromImage(bmp2))
                    {
                        g.InterpolationMode = InterpolationMode.NearestNeighbor;
                        g.DrawImage(PngBullet, new Rectangle(Point.Empty, bmp2.Size));
                        BulletBitmap = bmp2;
                    }
                }
                {
                    var bmp2 = new Bitmap(Scale, Scale);
                    using (var g = Graphics.FromImage(bmp2))
                    {
                        g.InterpolationMode = InterpolationMode.NearestNeighbor;
                        g.DrawImage(PngInvader, new Rectangle(Point.Empty, bmp2.Size));
                        InvaderBitmap = bmp2;
                    }
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
            }

            public void SetCursorPosition(int x, int y)
            {
                _cursorPositionX = x;
                _cursorPositionY = y;
            }

            public void Write(char c)
            {
                Bitmap bmp;
                switch (c)
                {
                    case '-':
                        bmp = (Bitmap)InvaderBitmap.Clone();
                        bmp.RotateFlip(RotateFlipType.Rotate270FlipNone);
                        break;
                    case '\\':
                        bmp = (Bitmap)InvaderBitmap.Clone();
                        break;
                    case '|':
                        bmp = (Bitmap)InvaderBitmap.Clone();
                        bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);
                        break;
                    case '/':
                        bmp = (Bitmap)InvaderBitmap.Clone();
                        bmp.RotateFlip(RotateFlipType.Rotate180FlipNone);
                        break;
                    case '&':
                        bmp = (Bitmap)ShipBitmap.Clone();
                        break;
                    default:
                        bmp = new Bitmap(Scale, Scale);
                        break;
                }

                Bitmap fbmp = (Bitmap)Form.Bitmap.Clone();
                Graphics grph = Graphics.FromImage(fbmp);
                grph.DrawImage(bmp, _cursorPositionX, _cursorPositionY);
                Form.Bitmap = fbmp;
            }

            public (ConsoleKey Key, bool b) ReadKey(bool b)
            {
                throw new NotImplementedException();
            }

            public void Beep()
            {
                SystemSounds.Beep.Play();
            }

            public static Bitmap PngBullet;
            public static Bitmap PngInvader;
            public static Bitmap PngShip;

            public Bitmap BulletBitmap;
            public Bitmap InvaderBitmap;
            public Bitmap ShipBitmap;
        }


        Form1 Form;
        public ConsoleClass Console;

        Thread bdg; //поток создания врагов
        EventWaitHandle startevt; //события начала партии
        Mutex screenlock = new Mutex(); //запрещает одновременую отрисовку
        Semaphore bulletsem = new Semaphore(0, 3); //запрещает боьше 3х пуль
        object gameover = new object(); //заглушка критической секции
        List<Thread> bg = new List<Thread>(); //потоки врагов
        List<Thread> bullets = new List<Thread>(); //потоки пуль

        bool end = false; //статус конца игры (стоп потоков врагов и пуль)
        long hit = 0, miss = 0; //попадания\промахи
        static readonly char[] badchar = {'-', '\\', '|', '/'}; //моделька врага
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
            bdg.Start(); //запускаем этот поток
            while (true)
            {
                //цикл управление пушкой
                writeat(x, y, '&'); //отрисовка пушки
                switch (Console.ReadKey(true).Key)
                {
                    //считывает нажатые клавиши без записи в консоль
                    case ConsoleKey.Spacebar: //нажат пробел
                        startevt.Set(); //ивент начала игры
                        bx = x;
                        by = y - 1; //-1 тк пуля отрисовывется не на месте пушки
                        bullets.Add(new Thread(new ThreadStart(bullet))); //создание потока пуль
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
                    Console.Title = $"Война потоков - Попаданий: {hit}, Промахов: {miss}";
                    bdg.Abort(); //заканчивает поток спавна врагов
                    end = true; //изменяет статус конца игры
                    MessageBox(IntPtr.Zero, "Игра окончена!", "Thread War", 0);
                    Environment.Exit(0); //завершает программу
                }
            }
            else Console.Title = $"Война потоков - Попаданий: {hit}, Промахов: {miss}";
        }

        int random(int n0, int n1) //метод генерации случайных чисел
        {
            return new Random().Next(n0, n1); //n0-минимальное значение n1-максимальное
        }
    }
}