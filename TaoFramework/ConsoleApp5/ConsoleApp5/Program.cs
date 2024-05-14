using System;
using System.Drawing; // Color
using System.Drawing.Imaging;
using OpenTK;
using OpenTK.Graphics;
//using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL;
using System.Windows.Forms;
using OpenTK.Mathematics;
using OpenTK.Input;
using static OpenTK.GLControl;
using OpenTK.Windowing.GraphicsLibraryFramework;
using static System.Runtime.InteropServices.JavaScript.JSType;
//using Glut = OpenTK.Windowing.GraphicsLibraryFramework.GLFW;
using ImGuiNET;
using Ultz.SilkExtensions.ImGui;
using System.Text;
using Silk.NET.Input.Common;
//using SharpDX.Direct3D9;  // Checkbox

namespace LearnOpenTK_2
{
    class Program
    {
        // Создаем новый класс, отнаследовавшись от GameWindow
        public class Game : GameWindow
        {
            // Camera position
            Vector3 cameraPosition = new Vector3 (0, 0, 1);
            // Camera rotation angles
            private float pitch = 0, yaw = 0; // pitch - тангаж(относитльено X),yaw - рыскание(относительно Y) 
            // Для сохранения углов поворота камеры

            // Camera scale factor
            public float scaleFactor = 2.0f;

            bool showTexture = false; // Флаг вывода нормалей
            bool useMaterial = true;  // Флаг использования материала и источника света
            bool showNormals = false; // Флаг показа нормали
            bool flat = true;         // Флаг сглаживания
            static string path = "D:\\OpenGL\\TaoFramework\\ConsoleApp5\\ConsoleApp5\\earth3.png"; // Путь до пикчи
            static Bitmap bitmap = new Bitmap(path);
            BitmapData data;
            int texture;

            private float x0 = -0.7f, y0 = -0.7f, z0 = .0f, l = 0.2f, y_c = .0f, x_c = .0f, z_c = .0f, a = 0.4f; // Центр координат, длина осей координат, центр куба
            private float sinfactor = 0.0f, factor = 0.0f, frameTime = 0.0f; // Переменные для изменения цвета окна и подсчета fps
            private int fps = 0; // Просто вначале занулил fps

            private float sphR = 0.5f; // Радиус сферы
            int nx = 32, ny = 32; // Число разбиений (широт и долгот) сферы по X и Y
            // private int lightId = -1; - для очистки памяти при наложении текстур
            // Внутренний конструктор 
            public Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
                : base(gameWindowSettings, nativeWindowSettings)
            {
                KeyDown += Keyboard_KeyDown; // Обработчик нажатий на клавиши клавиатуры
                Console.WriteLine("VERSION OPENGL: " + GL.GetString(StringName.Version));
                Console.WriteLine("NAME OF VENDOR: " + GL.GetString(StringName.Vendor));
                Console.WriteLine("INFO ABOUT THE DRIVER: " + GL.GetString(StringName.Renderer));
                Console.WriteLine("THE VERSION OF SHADERS: " + GL.GetString(StringName.ShadingLanguageVersion));

                VSync = VSyncMode.On; // Вертикальная синхронизация
            }

            // Обработчик нажатий на клавиши клавиатуры
            private void Keyboard_KeyDown(KeyboardKeyEventArgs args)
            {
                // Camera movement speed, Camera rotation angles, Zoom speed 
                float cameraSpeed = 0.1f * scaleFactor, rotationSpeed = 0.1f, zoomSpeed = 0.01f;

                // Compute the direction vector based on pitch and yaw
                Vector3 direction = new Vector3(
                (float)(Math.Cos(yaw) * Math.Cos(pitch)),
                (float)Math.Sin(pitch),
                (float)(Math.Sin(yaw) * Math.Cos(pitch))
                ).Normalized(); // Нормировка вектора

                // Cross - векторное произведение [direction x {0, 1, 0}]
                Vector3 right = Vector3.Cross(direction, Vector3.UnitY).Normalized();
                // [right x direction]
                Vector3 up = Vector3.Cross(right, direction).Normalized();

                // Векторы right, up доджны быть единичной длины для согласованности движения камеры и ее поворота. 

                
                if (args.Key == Keys.Escape)
                {
                    Console.WriteLine(Keys.Escape.ToString());
                    Close();
                }

                // Handle keyboard input for camera movement
                if (args.Key == Keys.Up)
                    cameraPosition += cameraSpeed * up;
                if (args.Key == Keys.Down)
                    cameraPosition -= cameraSpeed * up;
                if (args.Key == Keys.Left)
                    cameraPosition -= cameraSpeed * right;
                if (args.Key == Keys.Right)
                    cameraPosition += cameraSpeed * right;

                // Handle keyboard input for camera rotation
                if (args.Key == Keys.W)
                    pitch += rotationSpeed;
                if (args.Key == Keys.S)
                    pitch -= rotationSpeed;
                if (args.Key == Keys.A)
                    yaw += rotationSpeed;
                if (args.Key == Keys.D)
                    yaw -= rotationSpeed;

                // Handle keyboard input for zooming Z - приблизить, X - отдалить
                if (args.Key == Keys.Z)
                    cameraPosition += zoomSpeed * direction; // Zoom in
                if (args.Key == Keys.X)
                    cameraPosition -= zoomSpeed * direction; // Zoom out

                // Ensure scaleFactor stays within valid range
                scaleFactor = MathHelper.Clamp(scaleFactor, 0.1f, 10.0f);

                // Clamp pitch to avoid flipping camera
                pitch = MathHelper.Clamp(pitch, -MathHelper.PiOver2 + 0.1f, MathHelper.PiOver2 - 0.1f);


                if (args.Key == Keys.Escape) Close(); // Escape - выход из окна
                if (args.Key == Keys.F11)
                    WindowState = (WindowState == WindowState.Fullscreen) ? WindowState.Normal : WindowState.Fullscreen;
                if (args.Key == Keys.F7)
                    if (showNormals)
                    {
                        showNormals = false;
                        showTexture = false;
                    }
                    else
                    {
                        showNormals = true;
                        showTexture = false;
                    }
                if (args.Key == Keys.F8)
                    if (flat)
                    {
                        flat = false;
                        useMaterial = true;
                        showTexture = false;
                    }
                    else
                    {
                        flat = true;
                        useMaterial = true;
                        showTexture = false;
                    }
                if (args.Key == Keys.F9)
                    if (showNormals)
                    {
                        showTexture = false;
                        showNormals = false;
                        useMaterial = true;
                    }
                    else
                    {
                        showTexture = true;
                        showNormals = false;
                        useMaterial = false;
                    }

                // Update the view matrix based on camera position and rotation
                Matrix4 viewMatrix = Matrix4.LookAt(cameraPosition, cameraPosition + direction, up);
                GL.MatrixMode(MatrixMode.Modelview);
                GL.LoadMatrix(ref viewMatrix);

            }

            // Это переопределенный метод который вызывается при инициализации окна
            // Тут:
            // base.OnLoad() Это гарантирует, что все задачи инициализации, определенные в базовом классе Game, будут выполнены до пользовательской логики инициализации в игровом классе.
            // Создание объекта Rect типа Rectangle 
            // data = LockBits() Это блокирует биты растрового изображения в памяти, обеспечивая прямой доступ к его пиксельным данным. Обычно это делается для эффективной обработки данных изображения или манипулирования ими.
            // PolygonMode - Он указывает, что полигоны, обращенные к передней части объекта, должны быть заполнены, что означает, что они будут отображаться как сплошные фигуры, а не просто контуры
            // CreateOrthographicOffCenter - определяет, как трехмерные координаты преобразуются в 2D-координаты на экране. В этом случае используется ортогональная проекция, которая сохраняет относительный размер объектов независимо от их расстояния до камеры.
            // Projection – проецирует глобальные координаты в пространство клипа (clip space); вы можете рассматривать это как своего рода линзу
            // Modelview - помещает геометрию объекта в глобальное непроецируемое пространство.
            // Матрица проекции загружается в стек матриц.
            // В чем отличие LoadMatrix от PushMatrix
            // Текущая матрица проекции заменяется на ту, которая была определена ранее, что гарантирует, что последующие операции рендеринга будут использовать эту матрицу проекции для преобразования координат.
            protected override void OnLoad()
            {
                base.OnLoad();
                Rectangle Rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                data = bitmap.LockBits(Rect, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill); // управляет интерпретацией полигонов для растеризации
                Matrix4 projection = Matrix4.CreateOrthographicOffCenter(-3.0f, 3.0f, -3.0f, 3.0f, -3.0f, 3.0f); // создание матрицы ортогональной проекции []x[]x[]
                GL.MatrixMode(MatrixMode.Projection);
                GL.LoadMatrix(ref projection);
            }

            // Переопределенный метод, вызывается всякий раз, когда изменяется размер окна.
            // base.OnResize() - Это гарантирует, что любые задачи по изменению размера, будут выполнены до того, как пользовательская логика изменения размера будет реализована в игровом классе.
            // Viewport задает область просмотра, которая является областью окна, в которой выполняется рендеринг
            protected override void OnResize(ResizeEventArgs e)
            {
                base.OnResize(e);
                GL.Viewport(0, 0, Size.X, Size.Y);   
            }

            // Переопределенный метод, вызывается один раз за кадр во время игрового цикла для обновления игровой логики.
            // base.OnUpdateFrame Это гарантирует, что любая логика обновления, определенная в базовом классе, выполняется после пользовательской логики обновления в игровом классе.
            protected override void OnUpdateFrame(FrameEventArgs args)
            {
                // Подсчет FPS - Frames Per Second (Количество кадров в секунду)
                frameTime += (float)args.Time;
                fps++; // Количество кадров
                // Когда frameTime - превышает 1 секунду, мы сбрасываем fps и считаем заново
                if (frameTime >= 1.0f)
                {
                    Title = $"LearnOpenTK FPS - {fps}";
                    frameTime = 0.0f;
                    fps = 0;
                }

                // Подсчеты
                factor += 0.0005f;
                sinfactor = .03f * (float)Math.Sin(factor);

                base.OnUpdateFrame(args);
            }

            // Это переопределенный метод, который отвечает за рендеринг сцены путем рисования различных объектов и применения дополнительных текстур и световых эффектов на основе заданных настроек.
            protected override void OnRenderFrame(FrameEventArgs e)
            {
                // Подготовка сцены
                // Она определяет цвет, который OpenGL использует для очистки цветового буфера при вызове GL.Clear.
                GL.ClearColor(.1f, .2f + sinfactor, .3f, 1.0f);
                // Очистка как буфера цвета, так и глубины 
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                // Отключение расчетов освещения
                GL.Disable(EnableCap.Lighting);

                // Включение проверки глубины. Это метод, используемый для определения того, какие объекты видны в сцене, на основе их глубины
                GL.Enable(EnableCap.DepthTest);


                
                //float sphR = 0.5f; // Радиус сферы
                //int nx = 32, ny = 32; // Число разбиений (широт и долгот) сферы по X и Y
                if (showTexture || useMaterial)
                {
                    if (useMaterial && flat) // Добавляем интерполяцию (сглаживание)
                    {
                        nx = 24;
                        ny = 24;
                    }
                }
                else
                {
                    if (showNormals) // Добавляем нормали
                    {
                        nx = 16;
                        ny = 16;
                    }
                }

                // Вращения вокруг вектора (X, Y, Z) на угол alpha
                //GL.Rotate(.6f, x0, y0, z0);
                GL.PointSize(7.0f);
                
                // Источник освещения 
                GL.Begin(PrimitiveType.Points);
                GL.Color3(Color.Yellow);
                GL.Vertex3(.0f, 1.5f, 1.0f);
                GL.End();

                axes(x0, y0, z0, l);
                cube(x_c, y_c, z_c, a);

                if (showTexture) makeTxtr();
                // Сооздаем материал и источник света
                // Добавить CleaUP
                if (useMaterial) makeMatAndLight();
 
                // Выводим сферу
                sphere(sphR, 1.0, 0.0, 0.0, nx, ny);
                // Отображение текстур отключается, чтобы предотвратить применение текстур к последующим объектам.
                if (showTexture) GL.Disable(EnableCap.Texture2D);
                // Вычисления освещения отключаются, чтобы предотвратить применение световых эффектов к последующим объектам.
                if (useMaterial) GL.Disable(EnableCap.Lighting);
                // Свапнуть передний и задний буферы, отображая отрисованный кадр на экране.
                SwapBuffers(); 
                base.OnRenderFrame(e);
            }

            // Переопределенный метод, выгружает контекст. 
            // Этот метод отвечает за высвобождение любых ресурсов OpenGL, которые были выделены во время существования контекста.
            protected override void OnUnload()
            {
                base.OnUnload();
            }

            // Создает 2D-текстуру на базе растрового образа, загруженного в data
            public void makeTxtr()
            {
                // Активация режима вывода текстур
                GL.Enable(EnableCap.Texture2D);
                // Генерация идентификатора тестуры 
                GL.GenTextures(1, out texture);
                // Связывание текстуры с идентификатором 
                GL.BindTexture(TextureTarget.Texture2D, texture);
                // Параметры текстуры 
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                // Создаем текстуру
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            }

            // Метод, отвечающий за освещение
            private void makeMatAndLight()
            {
                // Check if a light has been previously allocated
                //if (lightId != -1)
                //{
                //    GL.Disable(EnableCap.Light0); // Отключить ранее выделенный источник света
                //    GL.DeleteLight(lightId); // Удалить свет
                //}
                // 
                float[] light_position = [.0f, 1.5f, 1.0f, 0.0f];               // Координаты источника света
                float[] lghtClr = [ 1, 1, 1, 0 ];                     // Источник излучает белый цвет
                float[] mtClr = [ 1, 1, 0, 0 ];                       // Материал зеленого цвета
                //lightId = GL.GenLight();
                
                if (flat)
                    GL.ShadeModel(ShadingModel.Flat);                 // Вывод без интерполяции цветов
                else
                    GL.ShadeModel(ShadingModel.Smooth);               // Вывод с интерполяцией цветов
                GL.Enable(EnableCap.Lighting);                        // Будем рассчитывать освещенность

                // GL.Light - настройка параметров источника света
                // light - количество источников света, поддерживается (можно включить), как минимум 8 источников Light0-Light7
                GL.Light(LightName.Light0, LightParameter.Position, light_position);

                GL.Light(LightName.Light0, LightParameter.Diffuse, lghtClr); // Рассеивание
                GL.Enable(EnableCap.Light0);                          // Включаем в уравнение освещенности источник GL_LIGHT0
                                                                      // Диффузионная компонента цвета материала
                GL.Material(MaterialFace.Front, MaterialParameter.Diffuse, mtClr);
            }

            // Метод, рисующий координатные оси

            private void axes(double x0, double y0, double z0, double l)
            {
                GL.PushMatrix();

                // X - Red line
                GL.Color3(1.0f, 0.0f, 0.0f);
                GL.Begin(PrimitiveType.Lines);
                GL.Vertex3(x0, y0, z0);
                GL.Vertex3(x0 + l, y0, z0);
                GL.End();
                //DrawText("X", 1.1f, 0.0f, 0.0f);

                // Y - Green line
                GL.Color3(0.0f, 1.0f, 0.0f);
                GL.Begin(PrimitiveType.Lines);
                GL.Vertex3(x0, y0, z0);
                GL.Vertex3(x0, y0 + l, z0);
                GL.End();
                //DrawText("Y", 0.0f, 1.1f, 0.0f);

                // Z - Blue line
                GL.Color3(0.0f, 0.0f, 1.0f);
                GL.Begin(PrimitiveType.Lines);
                GL.Vertex3(x0, y0, z0);
                GL.Vertex3(x0, y0, z0 + l);
                GL.End();
                //DrawText("Z", 0.0f, 0.0f, 1.1f);

                // O - начало координат
                GL.Begin(PrimitiveType.Points);
                GL.Color3(Color.White);
                GL.Enable(EnableCap.PointSmooth);
                GL.Hint(HintTarget.PointSmoothHint, HintMode.Nicest); // Fastest / Nicest
                GL.PointSize(5.0f);
                GL.Vertex3(x0, y0, z0);
                GL.End();

                GL.PopMatrix();
            }

            // Метод, рисующий куб
            private void cube(double x_c, double y_c, double z_c, double a)
            {
                GL.PushMatrix();

                // First edge  
                GL.Begin(PrimitiveType.Quads);
                GL.Color3(Color.Red);
                GL.Vertex3(x_c + a, y_c - a, z_c - a); // 1
                GL.Vertex3(x_c + a, y_c + a, z_c - a); // 4
                GL.Vertex3(x_c - a, y_c + a, z_c - a); // 3
                GL.Vertex3(x_c - a, y_c - a, z_c - a); // 2
                GL.End();

                // Second edge
                GL.Begin(PrimitiveType.Quads);
                GL.Color3(Color.Green);
                GL.Vertex3(x_c + a, y_c - a, z_c - a); // 1
                GL.Vertex3(x_c + a, y_c + a, z_c - a); // 4
                GL.Vertex3(x_c + a, y_c + a, z_c + a); // 6
                GL.Vertex3(x_c + a, y_c - a, z_c + a); // 5
                GL.End();

                // Trird edge
                GL.Begin(PrimitiveType.Quads);
                GL.Color3(Color.Yellow);
                GL.Vertex3(x_c + a, y_c + a, z_c - a); // 4
                GL.Vertex3(x_c - a, y_c + a, z_c - a); // 3
                GL.Vertex3(x_c - a, y_c + a, z_c + a); // 7
                GL.Vertex3(x_c + a, y_c + a, z_c + a); // 6
                GL.End();

                // Fourth edge
                GL.Begin(PrimitiveType.Quads);
                GL.Color3(Color.Blue);
                GL.Vertex3(x_c - a, y_c - a, z_c - a); // 2
                GL.Vertex3(x_c - a, y_c + a, z_c - a); // 3
                GL.Vertex3(x_c - a, y_c + a, z_c + a); // 7
                GL.Vertex3(x_c - a, y_c - a, z_c + a); // 8
                GL.End();

                // Fifth edge
                GL.Begin(PrimitiveType.Quads);
                GL.Color3(Color.Orange);
                GL.Vertex3(x_c + a, y_c - a, z_c - a); // 1
                GL.Vertex3(x_c - a, y_c - a, z_c - a); // 2
                GL.Vertex3(x_c - a, y_c - a, z_c + a); // 8
                GL.Vertex3(x_c + a, y_c - a, z_c + a); // 5
                GL.End();

                // Sixth edge
                GL.Begin(PrimitiveType.Quads);
                GL.Color3(Color.Purple);
                GL.Vertex3(x_c + a, y_c - a, z_c + a); // 5
                GL.Vertex3(x_c + a, y_c + a, z_c + a); // 6
                GL.Vertex3(x_c - a, y_c + a, z_c + a); // 7
                GL.Vertex3(x_c - a, y_c - a, z_c + a); // 8
                GL.End();

                GL.PopMatrix();
            }

            // Метод, рисующий сферу
            private void sphere(double r, double x0, double y0, double z0, int nx, int ny)
            {
                int ix, iy;
                double x, y, z, sy, cy, sy1, cy1, sx, cx, piy, pix, ay, ay1, ax, tx, ty, ty1, dnx, dny, diy;
                dnx = 1.0 / nx; // Шаг по широтам
                dny = 1.0 / ny; // Шаг по долготам

                // Перемещает текущий матричный стек на единицу вниз, дублируя текущую матрицу
                GL.PushMatrix(); // Это очень важный шаг
                
                // Перенос центра сферы в точку (x0, y0, z0)
                GL.Translate(x0, y0, z0);

                // Рисуем полигональную модель сферы, формируем нормали и задаем коодинаты текстуры
                // Каждый полигон - это трапеция. Трапеции верхнего и нижнего слоев вырождаются в треугольники
                GL.Begin(PrimitiveType.QuadStrip);
                GL.Color3(Color.Aqua);
                piy = Math.PI * dny;
                pix = Math.PI * dnx;
                for (iy = 0; iy < ny; iy++)
                {
                    diy = iy;
                    ay = diy * piy;
                    sy = Math.Sin(ay);
                    cy = Math.Cos(ay);
                    ty = diy * dny;
                    ay1 = ay + piy;
                    sy1 = Math.Sin(ay1);
                    cy1 = Math.Cos(ay1);
                    ty1 = ty + dny;
                    for (ix = 0; ix <= nx; ix++)
                    {
                        ax = 2.0 * ix * pix;
                        sx = Math.Sin(ax);
                        cx = Math.Cos(ax);
                        x = r * sy * cx;
                        y = r * sy * sx;
                        z = r * cy;
                        tx = ix * dnx;
                        // Координаты нормали в текущей вершине
                        GL.Normal3(x, y, z); // Нормаль направлена от центра
                                             // Координаты текстуры в текущей вершине
                        GL.TexCoord2(tx, ty);
                        GL.Vertex3(x, y, z);
                        x = r * sy1 * cx;
                        y = r * sy1 * sx;
                        z = r * cy1;
                        GL.Normal3(x, y, z);
                        GL.TexCoord2(tx, ty1);
                        GL.Vertex3(x, y, z);
                    }
                }
                GL.End();
                // Показываем нормали
                if (showNormals)
                {
                    double rv = 1.15 * r;
                    GL.LineWidth(2); // Толщина линии, отображающей нормаль
                    GL.Color3(Color.White);
                    GL.Begin(PrimitiveType.Lines);
                    piy = Math.PI * dny;
                    pix = Math.PI * dnx;
                    for (iy = 0; iy < ny; iy++)
                    {
                        diy = (double)iy;
                        ay = diy * piy;
                        sy = Math.Sin(ay);
                        cy = Math.Cos(ay);
                        ay1 = ay + piy;
                        //sy1 = Math.Sin(ay1);
                        //cy1 = Math.Cos(ay1);
                        for (ix = 0; ix <= nx; ix++)
                        {
                            ax = 2.0 * ix * pix;
                            sx = Math.Sin(ax);
                            cx = Math.Cos(ax);
                            x = r * sy * cx;
                            y = r * sy * sx;
                            z = r * cy;
                            GL.Vertex3(x, y, z);
                            x = rv * sy * cx;
                            y = rv * sy * sx;
                            z = rv * cy;
                            GL.Vertex3(x, y, z);
                        }
                    }
                    GL.End();
                }

                GL.PopMatrix(); // извлекает текущий матричный стек LIFO
            }
        }

        static void Main(string[] args)
        {

            var nativeWinSettings = new NativeWindowSettings() // Специальный класс в OpenTK, почему он?
            {
                ClientSize = new Vector2i(1000, 850),             // Начальный размер окна
                Location = new Vector2i(370, 300),          // Место расположение окна
                WindowBorder = WindowBorder.Resizable,      // Возможность изменения размера окна
                WindowState = WindowState.Normal,           // Определяет, будет ли окно свернуто, развернуто или восстановлено
                Title = "LearnOpenTK - Creating a Window", 
                Flags = ContextFlags.Default,               // Текущие флаги графического поля
                APIVersion = new Version(3, 3),             // Возвращает или задает значение, представляющее текущую версию графического API.
                Profile = ContextProfile.Compatability,     // Текущий профиль графического API
                API = ContextAPI.OpenGL
            };


            // Правильное использование удаленных объектов
            // При объвлении using создается удаленный экземпляр game типа(класса) Game, после прохождения using экземпляр удаляется 
            using Game game = new Game(GameWindowSettings.Default, nativeWinSettings);

            game.Run();
        }
    }
}