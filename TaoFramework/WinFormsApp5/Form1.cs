using Tao.OpenGl;           // для работы с библиотекой OpenGL
using Tao.Platform.Windows; // для реализации взаимодействия
using System.Runtime.InteropServices; // /для использования дополнительных API функций Windows

using System;
using OpenTK; // Рисование сферы
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;


// При инициализации OpenGL под Windows необходимо произвести связывание HDC и HGLRC
// HDC - идентификатор или дескриптор контекста устройства
// HGLRC - специальный тип для работы с RC

namespace WinFormsApp5
{
    public partial class Form1 : Form
    {
        IntPtr Handle3D;   // IntPtr - тип для указателя в C#
        IntPtr HDC3D;
        IntPtr HRC3D;
        public Form1() // конструктор 
        {
            InitializeComponent();  //метод инициализации компонентов, который был добавлен автоматически при создании проекта
            Handle3D = Handle;      // Выбираем для рисования саму форму. Если хотим, например, рисовать на панели Panel1,
                                    // то здесь необходимо указать Panel1.Handle
            HDC3D = User.GetDC(Handle3D);
            Gdi.PIXELFORMATDESCRIPTOR PFD =
                                        new Gdi.PIXELFORMATDESCRIPTOR(); // PFD - структура, содержащая описание формата                
                                                                         // пикселей, имеет 26 полей, мы заполним только
                                                                         // некоторые из них
            PFD.nVersion = 1;
            PFD.nSize = (short)Marshal.SizeOf(PFD);
            PFD.dwFlags = Gdi.PFD_DRAW_TO_WINDOW                         // набор флагов, рисуем в окне
                | Gdi.PFD_SUPPORT_OPENGL                                 // включаем поддержку OpenGL
                | Gdi.PFD_DOUBLEBUFFER;                                  // и поддержку двойной буферизации
            PFD.iPixelType = Gdi.PFD_TYPE_RGBA;                          // цвет в формате RGBA
                                                                         // R - красный, G - зеленый, B - синий
                                                                         // А - альфа-канал, отвечает за степень прозрачности
            PFD.cColorBits = 24;                                         // 24-х битный буфер цвета
            PFD.cDepthBits = 32;                                         // 32-х битный буфер глубины
            PFD.iLayerType = Gdi.PFD_MAIN_PLANE;
            int nPixelFormat = Gdi.ChoosePixelFormat(HDC3D, ref PFD);
            Gdi.SetPixelFormat(HDC3D, nPixelFormat, ref PFD);
            HRC3D = Wgl.wglCreateContext(HDC3D);
            Wgl.wglMakeCurrent(HDC3D, HRC3D);                            // делаем контекст воспроизведения текущим
        }

        // Обработчик события
        private void Form1_Resize(object sender, EventArgs e)
        {
            // определение проекции
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glLoadIdentity();
            int w = ClientRectangle.Width;
            int h = ClientRectangle.Height;
            Glu.gluPerspective(30.0, (double)w / h, 2.0, 200000.0);
            // определение области вывода
            Gl.glViewport(0, 0, w, h);
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            int R = 0;
            int G = 0;
            int B = 0;
            float r = 5.0f;
            //Gl.glEnable(Gl.GL_DEPTH_TEST);
            // параметры очистки буферов глубины и цвета
            Gl.glClearColor(R, G, B, 1); // 0 <= R,G,B <= 1
            // очистка буферов
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
            Gl.glMatrixMode(Gl.GL_MODELVIEW); // делает текущей одну из нескольких основных матриц библиотеки
                                              // GL_MODELVIEW - видовая матрица
            Gl.glLoadIdentity();              // загрузку единичной матрицы в текущую

            // Видовое преобразование
            Gl.glTranslatef(0, 0, -r);        // перенос в направлении вектора с координатами (x, y, z). 
                                              //Gl.glScalef(a, b, c);           // масштабирование с коэффициентами (a, b, c) по осям координат (x, y, z) соответственно     
            
            GL.glBegin(GL.GL_QUADS);
            GL.glColor3f(1.0f, 1.0f, 1.0f);
            DrawSphere(1.0f, 20, 20);        // radius - радиус, slices - долготы (вертикаль), stacks - широты (горизонталь)
            // Поворот относительно начала мировой системы координат
            //Gl.glRotatef(fi, 1.0f, 0, 0);     //  поворот на угол alpha (в градусах) против часовой стрелки относительно 
            //Gl.glRotatef(psi, 0, 1.0f, 0);

            // First line GL_LINES - несвязные отрезки 
            //            GL_LINE_STRIP – связные отрезки
            //            GL_LINE_LOOP – замкнутая линия
            //            glEnable(Gl.GL_LINE_SMOOTH) - включение режима сглаживания линий
            //Gl.glBegin(Gl.GL_LINES);
                //Gl.glColor3f(0, 0, 1f);
                //Gl.glVertex3f(0, 0, 0);           // Vertex - вершина
                //Gl.glVertex3f(0, 0, 10);

            // Second line 
            //Gl.glBegin(Gl.GL_LINES);
                //Gl.glColor3f(0, 0, 1f);
                //Gl.glVertex3f(1, 1, 0);
                //Gl.glVertex3f(1, 1, 10);

            // Многоугольники GL_TRIANGLES - не связные треугольники
            //                GL_TRIANGLE_STRIP - связные треугольники
            //                GL_TRIANGLE_FAN - веер треугольники
            //                GL_QUADS - не связные четырехугольники
            //                GL_POLYGON - многоугольник

            //Gl.glBegin(Gl.GL_TRIANGLES);
                //Gl.glColor3f(0, 0, 1f);
                //Gl.glVertex3f(1f, 0, 0);
                //Gl.glVertex3f(1f, 2f, 0);
                //Gl.glVertex3f(1f, 3f, 0);


            Gl.glEnd();
            Gl.glFinish();           // заканчиваем рисование
            Gdi.SwapBuffers(HDC3D);  // меняем местами передний и задний буферы при двойной буферизации

        }

        // вставим операторы корректного завершения работы OpenGL
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Wgl.wglMakeCurrent(IntPtr.Zero, IntPtr.Zero);
            Wgl.wglDeleteContext(HRC3D); // 7
            User.ReleaseDC(Handle3D, HDC3D); // 8
        }

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

        private void DrawSphere(float radius, int slices, int stacks)
        {
            for (float phi = -90.0f; phi < 90.0f; phi += 180.0f / stacks)
            {

                GL.glBegin(GL.GL_QUAD_STRIP);
                for (float theta = 0.0f; theta <= 360.0f; theta += 360.0f / slices)
                {
                    Vector3d vertex;
                    vertex.X = radius * Math.Cos(Math.PI / 180.0 * phi) * Math.Cos(Math.PI / 180.0 * theta);
                    vertex.Y = radius * Math.Cos(Math.PI / 180.0 * phi) * Math.Sin(Math.PI / 180.0 * theta);
                    vertex.Z = radius * Math.Sin(Math.PI / 180.0 * phi);
                    GL.Normal3(vertex);
                    GL.Vertex3(vertex);

                    vertex.X = radius * Math.Cos(Math.PI / 180.0 * (phi + 180.0f / stacks)) * Math.Cos(Math.PI / 180.0 * theta);
                    vertex.Y = radius * Math.Cos(Math.PI / 180.0 * (phi + 180.0f / stacks)) * Math.Sin(Math.PI / 180.0 * theta);
                    vertex.Z = radius * Math.Sin(Math.PI / 180.0 * (phi + 180.0f / stacks));
                    GL.Normal3(vertex);
                    GL.Vertex3(vertex);
                }
                GL.End();
                Gl.glFinish();
                Gdi.SwapBuffers(HDC3D);
            }
        }

        // добавление перехвата сообщения о необходимости перерисовки EraseBackground
        class MyGL
        {
            // идентификатор события EraseBackground
            internal const int WM_ERASEBKGND = 0x0014;
            // вызов процедуры отложенной перерисовки Windows
            [DllImport("user32.dll")]
            internal static extern bool InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase);
        }

        // вызов отложенной перерисовки трехмерной сцены
        void InvalidateRect()
        {
            MyGL.InvalidateRect(Handle, IntPtr.Zero, false);
        }
    }
}