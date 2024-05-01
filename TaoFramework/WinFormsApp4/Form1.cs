using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tao.OpenGl;
using Tao.Platform.Windows;
using System.Runtime.InteropServices;
//������� ������ ��� ���������, �������� ������ ���������� � ����������� �������� ���������� ������� � ������� �����
namespace OpenGL
{
    public partial class Form1 : Form
    {
        IntPtr Handle3D; //IntPtr - ��� ��� ��������� � C#
        IntPtr HDC3D;
        IntPtr HRC3D;
        float r = 10f;
        public Form1()
        {
            InitializeComponent(); // ����� �������������
                                   // �����������, ������� ��� ��������
                                   // ������������� ��� �������� �������
            Handle3D = Handle; // �������� ��� ��������� ���� �����
                               // ���� �����, ��������, �������� �� ������ Panel1,
                               // �� ����� ���������� ������� Panel1.Handle
            HDC3D = User.GetDC(Handle3D);
            Gdi.PIXELFORMATDESCRIPTOR PFD = new Gdi.PIXELFORMATDESCRIPTOR();
            PFD.nVersion = 1;
            PFD.nSize = (short)Marshal.SizeOf(PFD);
            PFD.dwFlags = Gdi.PFD_DRAW_TO_WINDOW | Gdi.PFD_SUPPORT_OPENGL | Gdi.PFD_DOUBLEBUFFER;
            PFD.iPixelType = Gdi.PFD_TYPE_RGBA;
            PFD.cColorBits = 24; //24-� ������ ����� �����
            PFD.cDepthBits = 32; //32-� ������ ����� �������
            PFD.iLayerType = Gdi.PFD_MAIN_PLANE;
            int nPixelFormat = Gdi.ChoosePixelFormat(HDC3D, ref PFD);
            Gdi.SetPixelFormat(HDC3D, nPixelFormat, ref PFD);
            HRC3D = Wgl.wglCreateContext(HDC3D);
            Wgl.wglMakeCurrent(HDC3D, HRC3D);
        }



        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Wgl.wglMakeCurrent(IntPtr.Zero, IntPtr.Zero);
            Wgl.wglDeleteContext(HRC3D);
            User.ReleaseDC(Handle3D, HDC3D);
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            int R = 0;
            int G = 0;
            int B = 0;
            //��������� ������� ������� ������� � �����
            Gl.glClearColor(R, G, B, 1); // 0 <= R,G,B <= 1
                                         //������� �������
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
            //��������� �����
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glLoadIdentity();
            Gl.glTranslatef(0, 0, -r);
            Gl.glRotatef(45, 0, 1, 1);
            Gl.glBegin(Gl.GL_LINES);
            Gl.glColor3f(1f, 0, 0);
            Gl.glVertex3f(0, 0, 0);
            Gl.glVertex3f(0, 0, 10);
            Gl.glBegin(Gl.GL_LINES);
            Gl.glColor3f(2f, 0, 0);
            Gl.glVertex3f(1, 1, 0);
            Gl.glVertex3f(1, 1, 10);

            Gl.glEnd();
            Gl.glFinish(); //����������� ���������
            Gdi.SwapBuffers(HDC3D);


        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            //����������� ��������
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glLoadIdentity();
            int w = ClientRectangle.Width;
            int h = ClientRectangle.Height;
            Glu.gluPerspective(30.0, (double)w / h, 2.0, 200000.0);
            //����������� ������� ������
            Gl.glViewport(0, 0, w, h);
        }
        class MyGL
        {
            //������������� ������� EraseBackground
            internal const int WM_ERASEBKGND = 0x0014;
            //����� ��������� ���������� ����������� Windows
            [DllImport("user32.dll")]
            internal static extern bool InvalidateRect(IntPtr hWnd,
            IntPtr lpRect, bool bErase);
        }
        void InvalidateRect()
        {
            MyGL.InvalidateRect(Handle, IntPtr.Zero, false);
        }
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == MyGL.WM_ERASEBKGND)
            {
                m.Result = IntPtr.Zero;
                InvalidateRect();
            }
        }
    }
}
