using Tao.OpenGl;                     // ������������ ���� ��� ������ � ����������� OpenGL
using Tao.Platform.Windows;           // ������������ ���� ��� ���������� ��������������
using System.Runtime.InteropServices; // ������������ ���� ��� ������������� �������������� API ������� Windows
using System.Drawing.Imaging;         // ������������ ���� ��� ������ � 2D ����������

using System;
using OpenTK;                         // ������������ ���� ��� ��������� �����
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using static OpenTK.Graphics.OpenGL.GL;



///
/// ������:
/// 1) ���������� �������;
/// 2) ���������� �����;
/// 
///


// ����� ������:
// 1) ������� ���������� ��������� ��� ��������� (�����, ������, ������ ����� ������� Handle)
// 2) �������� �������� ���������� HDC
// 3) ���������� ������ ��������
// 4) �������� �������� ��������������� (HGLRC)
// 5) ���������� � ��������� 
// 6) ��������� ���������� OpenGL
// 7) ������� HGLRC (������?)
// 8) ������� HDC (������?)

// ��������� �������� � �������� �������� � ���� ������� �����
static uint LoadTexture(string filename)
// �������� �������� �� ����� filename
{
    // ������������� ����������� �������
    uint texObject = 0;
    try
    {
        // ��������� ����������� �� �����
        Bitmap bmp = new Bitmap(filename);
        // �������������� �� ��� Y
        bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
        // ������� ������������� ������ ��� ��������� �������
        // ������, ��� ���� ��������� ����������� � ������
        BitmapData bmpdata = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), 
            ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
        //������� ��������, Scan0 - �������� ��������� �� ������
        //������� ������� ������ � ������� bmpdata
        texObject = MakeGlTexture(bmpdata.Scan0, bmp.Width, bmp.Height);
        //������������� ����������� � ������
        bmp.UnlockBits(bmpdata);
    }
    catch { }
    return texObject;
}

static uint MakeGlTexture(IntPtr pixels, int w, int h)
// �������� �������� � ������
{
    // ������������� ����������� �������
    uint texObject;
    // ���������� ���������� ������
    Gl.glGenTextures(1, out texObject);
    // ������������� ����� �������� ��������
    Gl.glPixelStorei(Gl.GL_UNPACK_ALIGNMENT, 1);
    // ������� �������� � ������ ��� ��������� ��������
    Gl.glBindTexture(Gl.GL_TEXTURE_2D, texObject);
    // ������������� ����� ���������� ��������
    Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_S, Gl.GL_REPEAT);
    Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_T, Gl.GL_REPEAT);
    // ������������� ����� ���������� ��������
    Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR); // GL_LINEAR / GL_NEAREST
    Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR);
    // ������� RGB ��������
    Gl.glTexImage2D(Gl.GL_TEXTURE_2D, 0, Gl.GL_RGB, w, h, 0,
        Gl.GL_BGR, Gl.GL_UNSIGNED_BYTE, pixels);
    // ���������� ������������� ����������� �������
    return texObject;
}




// WinFormsApp5 - ������������ ���� �� ���������
namespace WinFormsApp5
{
    public partial class Form1 : Form
    {
        // ���������� �������� - ������������� ��������
        uint Texture;
        int Font3D = 0;

        // ���������� ����������
        float r = 10.0f, fi = 30.0f, psi = 30.0f; // �������� �� ��������� ������ ������������ ������ ������� ���������
        int R = 0, G = 0, B = 0;

        IntPtr Handle3D;   // IntPtr - ��� ��� ��������� � C#
        IntPtr HDC3D;
        IntPtr HRC3D;
        public Form1() // ����������� 
        {
            InitializeComponent();  //����� ������������� �����������, ������� ��� �������� ������������� ��� �������� �������
            // ��� 1
            Handle3D = Handle;      // �������� ��� ��������� ���� �����. ���� �����, ��������, �������� �� ������ Panel1,
                                    // �� ����� ���������� ������� Panel1.Handle
            // ��� 2
            HDC3D = User.GetDC(Handle3D);
            // ��� 3
            Gdi.PIXELFORMATDESCRIPTOR PFD = new Gdi.PIXELFORMATDESCRIPTOR(); // PFD - ��� ���������, ���������� �������� ������� ��������,
                                                                             // �������� 26 �����, �� �������� ������ ��������� �� ���
            // �������� ��������� ����� ���������� ������?
            Gl.glEnable(Gl.GL_DEPTH_TEST);
            // Font - ������� ����� �����
            // CreateFont3D(Font);

            PFD.nVersion = 0;       // ��� ��� �� ����?
            PFD.nSize = (short)Marshal.SizeOf(PFD); // ��� ��� �� ����?
            PFD.dwFlags =
                    Gdi.PFD_DRAW_TO_WINDOW // ��������� � ����
                  | Gdi.PFD_SUPPORT_OPENGL // �������� ��������� OpenGL
                  | Gdi.PFD_DOUBLEBUFFER;  // �������� ��������� ������� ������������
            PFD.cColorBits = 24;           // 24-� ������ ����� �����
            PFD.cDepthBits = 32;           // 32-� ������ ����� �������
            PFD.iLayerType = Gdi.PFD_MAIN_PLANE; // ��� ��� �� ����?
            int nPixelFormat = Gdi.ChoosePixelFormat(HDC3D, ref PFD); // ��� ��� �� ����������?
            Gdi.SetPixelFormat(HDC3D, nPixelFormat, ref PFD); // ��� ��� �� ����?
            // ��� 4
            HRC3D = Wgl.wglCreateContext(HDC3D);
            Wgl.wglMakeCurrent(HDC3D, HRC3D); // ������ �������� ��������������� �������
            
            // ��� ����, ����� ���������� � ��������� ����������� ��� ������� ����������
            Form1_Resize(null, null);
        }

        // ���������� ������� - ��������� ������� ����
        // ��� 5 - ������ ����� 
        // ����������� ���� ��������
        // ��������� ���� ������
        private void Form1_Resize(object sender, EventArgs e)
        {
            // ������ ������� ���� �� ���������� �������� 
            // ������ ����������
            // GL_PROJECTION - ������� ��������
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            
            // �������������� ��������
            // �������� ��������� �������
            Gl.glLoadIdentity();
            int w = ClientRectangle.Width;
            int h = ClientRectangle.Height;
            
            // ��������� ������������� ��������, ��������� �������� ��������� ������������� ������
            Glu.gluPerspective(30.0, (double)w / h, 2.0, 200000.0);
            
            // �������� ������� ������ �� �����
            Gl.glViewport(0, 0, w, h);
            // (0, 0) - ���������� ������ ������� ���� � �������� �����������
            // w - ������, h - ������ ������� ���������
            
        }

        // ���������� ������� - ����������� ����
        // ��� 5 - ������ ����� + ��� 6
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            // ��������� ������� ������� ������� � �����
            Gl.glClearColor(R, G, B, 1); // ������ 4 ���������?  0 <= R, G, B <= 1
            // ������� �������
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);

            // ������� ��������������, ������������ ��������� ������
            // GL_MODELVIEW - ������� �������
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            
            // ��������� �������
            Gl.glLoadIdentity();
            // ������� � ����������� ������� � ������������ (x, y, z) - ����������
            Gl.glTranslatef(0, 0, -r);
            // ������� �� ���� fi ������ ������� ������� ������������ ���,
            // ���������� ����� ������ ��������� � ����������� (x, y, z)
            // ������� ������� �� fi � ����������� x
            Gl.glRotatef(fi, 1.0f, 0, 0);
            // ������� ������� �� fi � ����������� y
            Gl.glRotatef(psi, 0, 1.0f, 0);

            // ��� 5
            // ���������� � ���������

            // ��� 6
            // ��������� ���������� OpenGL
            // ��������� ����� 5


            Gl.glFinish(); // ����������� ���������
            Gl.SwapBuffers(HDC3D); // ������ ������� �������� � ������ ������ ��� ������� �����������

        }

        // ���������� ������� - �������� ����������
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Wgl.wglMakeCurrent(IntPtr.Zero, IntPtr.Zero);
            
            // ��� 7
            Wgl.wglDeleteContext(HRC3D); 
            // ��� 8
            User.ReleaseDC(Handle3D, HDC3D); 
        }

        // ������� ������ ��������� ��������� EraseBackground � ������ ���������� ����������� Windows

        // ��������������� ��������� ��������� ��������� Windows ��� ��������� ��������� EraseBackground
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == MyGL.WM_ERASEBKGND)
            {
                m.Result = IntPtr.Zero;
                InvalidateRect();
            }
        }

        // ����� ���������� ����������� ���������� �����
        void InvalidateRect()
        {
            MyGL.InvalidateRect(Handle, IntPtr.Zero, false);
        }
        // ����� InvalidateRect() ���������� ����� �������� ������ ���, ����� �����
        // ���������������� ���������� �����


        // ���������� ��������� ��������� � ������������� ����������� EraseBackground
        class MyGL
        {
            // ������������� ������� EraseBackground
            internal const int WM_ERASEBKGND = 0x0014; // �������������������� �����
            // ����� ��������� ���������� ����������� Windows
            [DllImport("user32.dll")]
            internal static extern bool InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase);
        }

        // �������� 3D ������
        void CreateFont3D(Font font)
        {
            Gdi.SelectObject(HDC3D, font.ToHfont());
            Font3D = Gl.glGenLists(256);
            Wgl.wglUseFontBitmapsA(HDC3D, 0, 256, Font3D);
        }

        // ��� �������� � �������� ��������
        // Texture = LoadTexture("pic.bmp");
    }
}



// ��������� ����������
// CommandName[1,2,3,4][b,s,i,f,d,ub,us,ui][v](<���������>);
// ��������: gl (���������� OpenGL), glu (���������� GLU), wgl (������������� OpenGL � ���������� Windows)
// [1, 2, 3, 4] - ���������� ����������
// b - char, s - short int, i - int , etc
// [v] - � �������� ��������� ������������ ������ �� ������, � �������� ����� ��������� � �� ��� ������������
// ����������� ������ � �������� 
// <���������> - ���� �������� ����� ���� ������

// Gl.glEnable(...) Gl.glDisable(...)
// Gl.glEnable(Gl.GL_DEPTH_TEST); - ����� ��������� ��������� �����, ������������
// ��� ���� ������� ����� ���������� ���� ������ ������� � ������� ���������� ����������


// ������� ���������
// ��������� (x, y, z) - ����������, �������������. � ��� �������� ���������� ������ 
// ��������������� ��������
// �������   (x_{e}, y_{e}, z_{e}) - ���������, �������������. ��� ������� � ������ ���������� (�������)
// ��� x_{e}, y_{e} ����������� ������ � ������ �� �����������, z_{e} �� ����������� ����������
// ��������  (x_{s}, y_{s}) - ������� � �������� ������ �� ������.

// ������� ������: ������������� ������� � �������� �� ����������� �� ������.
// �� ��� �������� ������� �������������� OpenGL


// ����������� ��������� - ��� ����� ������� ������. ��� ������������ ������� ������ (Vetrex), � ������ ��������
// ������������� �������������� ��������: Color, Normal, RasterPos, TexCoord
// ������ ���������:
// Gl.glBegin{<��� ���������>};
// <�������� �������> (glColor, glNormal, ...)
// <���������� �������> (glVertex)
// <�������� �������>
// < ���������� ������� >
// Gl.glEnd();

// ���� ��� ������� ����� ���� � �� �� ��������, �� �� ����������� ����� ������� ����� ���������� ��������,
// � ������ ��� ���������� ������ ���������� ������
// Gl.glColor3ub(R,G,B) - 0-255 �������������
// Gl.glColor3f(R,G,B)
// Gl.glColor3f(0,0,1f) � ����� ����;

// ���������� ������ �������� � ������� ������� ��������� ����������� Gl.glVertex3[f,d](x,y,z), Gl.glVertex2[f,d](x,y) (2D)

// ����� Gl.GL_POINTS() - ����������� ���������� ����� � ������� 1. ����������, ����� ���������� GL_POINT_SMOOTH
// �����:
// Gl.GL_LINES - ��������� �������;
// Gl.GL_LINE_STRIP � ������� �������;
// Gl.GL_LINE_LOOP � ��������� �����.
// ��������������:
// Gl.GL_TRIANGLES - ��������� ������������; ����������� ���������� ������ 3
// Gl.GL_TRIANGLE_STRIP - ������� ������������;
// Gl.GL_TRIANGLE_FAN - ���� ������������;
// Gl.GL_QUADS - �� ������� ����������������; ����������� ���������� ������ 4   
// � �������������� ��������� �������� � ������ �������. �� ��������� �������� ������� (FRONT) ��,
// ��� ������� ����� ����� ����������� ������ ������� �������. ����� �� ��������� ����� �������������� ���������:
// Gl.glFrontFace(Gl.GL_CCW) - �������� ������� ������ ������� �������

// Gl.glPolygonMode(face,mode) - �������� ��� ���������� ���������, face - ������� ��������������, mode - ��� ����������
// Gl.glPolygonMode(Gl.GL_FRONT_AND_BACK,Gl.GL_FILL);
// GL_FRONT_AND_BACK - �������� ����� �������� � ����� ��������, GL_FILL - ������ �������

// ��������� ��������
// ��� ����������� �������� ���������� ������� ����������� �������� �
// � ���� ��������� ���������� ����� �������� ������ ����������