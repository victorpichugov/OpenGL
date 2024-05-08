using Tao.OpenGl;                     // Пространство имен для работы с библиотекой OpenGL
using Tao.Platform.Windows;           // Пространство имен для реализации взаимодействия
using System.Runtime.InteropServices; // Пространство имен для использования дополнительных API функций Windows
using System.Drawing.Imaging;         // Пространство имен для работы с 2D текстурами

using System;
using OpenTK;                         // Пространство имен для рисование сферы
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using static OpenTK.Graphics.OpenGL.GL;



///
/// Задачи:
/// 1) Нарисовать векторы;
/// 2) Нарисовать сферу;
/// 
///


// Схема работы:
// 1) Выбрать визуальный компонент для рисования (форма, панель, должен иметь свойсво Handle)
// 2) Получить контекст устройства HDC
// 3) Установить формат пикселей
// 4) Получить контекст воспроизведения (HGLRC)
// 5) Подготовка к рисованию 
// 6) Рисование средствами OpenGL
// 7) Удалить HGLRC (почему?)
// 8) Удалить HDC (почему?)

// Процедуры загрузки и создания текстуры в виде методов формы
static uint LoadTexture(string filename)
// Загрузка текстуры из файла filename
{
    // Идентификатор текстурного объекта
    uint texObject = 0;
    try
    {
        // Считываем изображение из файла
        Bitmap bmp = new Bitmap(filename);
        // Переворачиваем по оси Y
        bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
        // Создаем промежуточный объект для получения массива
        // цветов, при этом блокируем изображение в памяти
        BitmapData bmpdata = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), 
            ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
        //создаем текстуру, Scan0 - получаем указатель на первый
        //элемент массива цветов в объекте bmpdata
        texObject = MakeGlTexture(bmpdata.Scan0, bmp.Width, bmp.Height);
        //разблокировка изображения в памяти
        bmp.UnlockBits(bmpdata);
    }
    catch { }
    return texObject;
}

static uint MakeGlTexture(IntPtr pixels, int w, int h)
// Создание текстуры в памяти
{
    // Идентификатор текстурного объекта
    uint texObject;
    // Генерируем текстурный объект
    Gl.glGenTextures(1, out texObject);
    // Устанавливаем режим упаковки пикселей
    Gl.glPixelStorei(Gl.GL_UNPACK_ALIGNMENT, 1);
    // Создаем привязку к только что созданной текстуре
    Gl.glBindTexture(Gl.GL_TEXTURE_2D, texObject);
    // Устанавливаем режим повторения текстуры
    Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_S, Gl.GL_REPEAT);
    Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_T, Gl.GL_REPEAT);
    // Устанавливаем режим фильтрации текстуры
    Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR); // GL_LINEAR / GL_NEAREST
    Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR);
    // Создаем RGB текстуру
    Gl.glTexImage2D(Gl.GL_TEXTURE_2D, 0, Gl.GL_RGB, w, h, 0,
        Gl.GL_BGR, Gl.GL_UNSIGNED_BYTE, pixels);
    // Возвращаем идентификатор текстурного объекта
    return texObject;
}




// WinFormsApp5 - пространство имен по умолчанию
namespace WinFormsApp5
{
    public partial class Form1 : Form
    {
        // Объявление перенной - идентификатор текстуры
        uint Texture;
        int Font3D = 0;

        // Объявление переменных
        float r = 10.0f, fi = 30.0f, psi = 30.0f; // отвечают за положение камеры относительно начала мировых координат
        int R = 0, G = 0, B = 0;

        IntPtr Handle3D;   // IntPtr - тип для указателя в C#
        IntPtr HDC3D;
        IntPtr HRC3D;
        public Form1() // конструктор 
        {
            InitializeComponent();  //метод инициализации компонентов, который был добавлен автоматически при создании проекта
            // Шаг 1
            Handle3D = Handle;      // Выбираем для рисования саму форму. Если хотим, например, рисовать на панели Panel1,
                                    // то здесь необходимо указать Panel1.Handle
            // Шаг 2
            HDC3D = User.GetDC(Handle3D);
            // Шаг 3
            Gdi.PIXELFORMATDESCRIPTOR PFD = new Gdi.PIXELFORMATDESCRIPTOR(); // PFD - это структура, содержащая описание формата пикселей,
                                                                             // содержит 26 полей, мы заполним только некоторые из них
            // Вставить отсечение после операторов старта?
            Gl.glEnable(Gl.GL_DEPTH_TEST);
            // Font - текущий шрифт формы
            // CreateFont3D(Font);

            PFD.nVersion = 0;       // что это за поле?
            PFD.nSize = (short)Marshal.SizeOf(PFD); // что это за поле?
            PFD.dwFlags =
                    Gdi.PFD_DRAW_TO_WINDOW // рисование в окне
                  | Gdi.PFD_SUPPORT_OPENGL // включаем поддержку OpenGL
                  | Gdi.PFD_DOUBLEBUFFER;  // включаем поддержку двойной буфферизации
            PFD.cColorBits = 24;           // 24-х битный буфер цвета
            PFD.cDepthBits = 32;           // 32-х битный буфер глубины
            PFD.iLayerType = Gdi.PFD_MAIN_PLANE; // что это за поле?
            int nPixelFormat = Gdi.ChoosePixelFormat(HDC3D, ref PFD); // что это за переменная?
            Gdi.SetPixelFormat(HDC3D, nPixelFormat, ref PFD); // что это за поле?
            // Шаг 4
            HRC3D = Wgl.wglCreateContext(HDC3D);
            Wgl.wglMakeCurrent(HDC3D, HRC3D); // делаем контекст воспроизведения текущим
            
            // Для того, чтобы подготовка к рисованию выполнялась при запуске приложения
            Form1_Resize(null, null);
        }

        // Обработчик события - изменения размера окна
        // Шаг 5 - первая часть 
        // определение типа проекции
        // опредение окна вывода
        private void Form1_Resize(object sender, EventArgs e)
        {
            // Делает текущей одну из нескольких основных 
            // матриц библиотеки
            // GL_PROJECTION - матрица проекций
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            
            // Преобразование проекций
            // Загрузка единичной матрицы
            Gl.glLoadIdentity();
            int w = ClientRectangle.Width;
            int h = ClientRectangle.Height;
            
            // Реализует перспективную проекцию, реализует механизм реального человеческого зрения
            Glu.gluPerspective(30.0, (double)w / h, 2.0, 200000.0);
            
            // Оператов области вывода на экран
            Gl.glViewport(0, 0, w, h);
            // (0, 0) - координаты левого нижнего угла в экранных координатах
            // w - ширина, h - высота области видимости
            
        }

        // Обработчик события - перерисовки окна
        // Шаг 5 - вторая часть + Шаг 6
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            // параметры очистки буферов глубины и цвета
            Gl.glClearColor(R, G, B, 1); // почему 4 параметра?  0 <= R, G, B <= 1
            // очистка буферов
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);

            // видовое преобразование, определяющее положение камеры
            // GL_MODELVIEW - видовая матрица
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            
            // Единичная матрица
            Gl.glLoadIdentity();
            // Перенос в направлении вектора с координатами (x, y, z) - глобальные
            Gl.glTranslatef(0, 0, -r);
            // Поровот на угол fi против часовой стрелки относительно оси,
            // проходящей через начало координат в направление (x, y, z)
            // сначала поворот на fi в направление x
            Gl.glRotatef(fi, 1.0f, 0, 0);
            // сначала поворот на fi в направление y
            Gl.glRotatef(psi, 0, 1.0f, 0);

            // Шаг 5
            // подготовка к рисованию

            // Шаг 6
            // Рисование средствами OpenGL
            // Прочитать главу 5


            Gl.glFinish(); // заканчиваем рисование
            Gl.SwapBuffers(HDC3D); // меняем местами передний и задний буферы при двойной буферизации

        }

        // Обработчик события - закрытия приложения
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Wgl.wglMakeCurrent(IntPtr.Zero, IntPtr.Zero);
            
            // Шаг 7
            Wgl.wglDeleteContext(HRC3D); 
            // Шаг 8
            User.ReleaseDC(Handle3D, HDC3D); 
        }

        // Добавим методы перехвата сообщения EraseBackground и вызова отложенной перерисовки Windows

        // переопределение процедуры перехвата сообщений Windows для обработки сообщения EraseBackground
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == MyGL.WM_ERASEBKGND)
            {
                m.Result = IntPtr.Zero;
                InvalidateRect();
            }
        }

        // вызов отложенной перерисовки трехмерной сцены
        void InvalidateRect()
        {
            MyGL.InvalidateRect(Handle, IntPtr.Zero, false);
        }
        // Метод InvalidateRect() необходимо будет вызывать каждый раз, когда нужно
        // перерисовывовать трехмерную сцену


        // добавление перехвата сообщения о необходимости перерисовки EraseBackground
        class MyGL
        {
            // идентификатор события EraseBackground
            internal const int WM_ERASEBKGND = 0x0014; // шестнадцатиразрядное число
            // вызов процедуры отложенной перерисовки Windows
            [DllImport("user32.dll")]
            internal static extern bool InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase);
        }

        // Создание 3D шрифта
        void CreateFont3D(Font font)
        {
            Gdi.SelectObject(HDC3D, font.ToHfont());
            Font3D = Gl.glGenLists(256);
            Wgl.wglUseFontBitmapsA(HDC3D, 0, 256, Font3D);
        }

        // Для загрузки и создания текстуры
        // Texture = LoadTexture("pic.bmp");
    }
}



// Синтаксис операторов
// CommandName[1,2,3,4][b,s,i,f,d,ub,us,ui][v](<аргументы>);
// Префиксы: gl (библиотека OpenGL), glu (библиотека GLU), wgl (взаимодейстие OpenGL с платформой Windows)
// [1, 2, 3, 4] - количество аргументов
// b - char, s - short int, i - int , etc
// [v] - в качестве аргумента используется ссылка на массив, у которого число элементов и их тип определяется
// предыдущими цифрой и символом 
// <аргументы> - либо заданные числа либо ссылки

// Gl.glEnable(...) Gl.glDisable(...)
// Gl.glEnable(Gl.GL_DEPTH_TEST); - режим отсечения невидимых линий, поверхностей
// без него объекты будут рисоваться один поверх другого в порядке выполнения операторов


// Системы координат
// Мирования (x, y, z) - глобальная, фиксированная. В ней задаются координаты вершин 
// визуализируемых объектов
// Видовая   (x_{e}, y_{e}, z_{e}) - подвижная, левосторонняя. Она связана с точкой наблюдения (камерой)
// оси x_{e}, y_{e} направленны вправо и наверх от наблюдателя, z_{e} по направлению наблюдения
// Экранная  (x_{s}, y_{s}) - связана с областью вывода на экране.

// Главная задача: Преобразовать мировые в экранные их изображений на экране.
// За это отвечает конвеер преобразований OpenGL


// Графические примитивы - это самые простые фигуры. Они определяются набором вершин (Vetrex), с каждой вершиной
// ассоциированы дополнительные свойства: Color, Normal, RasterPos, TexCoord
// Пример примитивы:
// Gl.glBegin{<тип примитива>};
// <свойства вершины> (glColor, glNormal, ...)
// <координаты вершины> (glVertex)
// <свойства вершины>
// < координаты вершины >
// Gl.glEnd();

// Если все вершины имеют один и те же свойства, то их определение можно вынести перед командными скобками,
// а внутри них определить только координаты вершин
// Gl.glColor3ub(R,G,B) - 0-255 интенсивность
// Gl.glColor3f(R,G,B)
// Gl.glColor3f(0,0,1f) – синий цвет;

// Координаты вершин задаются в мировой системе координат операторами Gl.glVertex3[f,d](x,y,z), Gl.glVertex2[f,d](x,y) (2D)

// Точки Gl.GL_POINTS() - минимальное количество точек в скобках 1. Изначально, точки квадратные GL_POINT_SMOOTH
// Линии:
// Gl.GL_LINES - несвязные отрезки;
// Gl.GL_LINE_STRIP – связные отрезки;
// Gl.GL_LINE_LOOP – замкнутая линия.
// Многоугольники:
// Gl.GL_TRIANGLES - несвязные треугольники; минимальное количество вершин 3
// Gl.GL_TRIANGLE_STRIP - связные треугольники;
// Gl.GL_TRIANGLE_FAN - веер треугольники;
// Gl.GL_QUADS - не связные четырехугольники; минимальное количество вершин 4   
// У многоугольника различают переднюю и заднюю стороны. По умолчанию передняя сторона (FRONT) та,
// для которой обход точек выполняется против часовой стрелки. Обход по умолчанию можно переопределить командами:
// Gl.glFrontFace(Gl.GL_CCW) - передняя сторона против часовой стрелки

// Gl.glPolygonMode(face,mode) - задается вид прорисовки полигонов, face - сторона многоугольника, mode - вид прорисовки
// Gl.glPolygonMode(Gl.GL_FRONT_AND_BACK,Gl.GL_FILL);
// GL_FRONT_AND_BACK - оператор будет применен к обеим сторонам, GL_FILL - полная заливка

// Рисование текстуры
// Для отображения текстуры необходимо выбрать закрашенный примитив и
// в коде рисования трехмерной сцены добавить группу операторов