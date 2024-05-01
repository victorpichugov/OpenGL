using Tao.OpenGl;           // ��� ������ � ����������� OpenGL
using Tao.Platform.Windows; // ��� ���������� ��������������
using System.Runtime.InteropServices; // /��� ������������� �������������� API ������� Windows

using System;
using OpenTK; // ��������� �����
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;


// ��� ������������� OpenGL ��� Windows ���������� ���������� ���������� HDC � HGLRC
// HDC - ������������� ��� ���������� ��������� ����������
// HGLRC - ����������� ��� ��� ������ � RC

namespace WinFormsApp5
{
    public partial class Form1 : Form
    {
        IntPtr Handle3D;   // IntPtr - ��� ��� ��������� � C#
        IntPtr HDC3D;
        IntPtr HRC3D;
        public Form1() // ����������� 
        {
            InitializeComponent();  //����� ������������� �����������, ������� ��� �������� ������������� ��� �������� �������
            Handle3D = Handle;      // �������� ��� ��������� ���� �����. ���� �����, ��������, �������� �� ������ Panel1,
                                    // �� ����� ���������� ������� Panel1.Handle
            HDC3D = User.GetDC(Handle3D);
            Gdi.PIXELFORMATDESCRIPTOR PFD =
                                        new Gdi.PIXELFORMATDESCRIPTOR(); // PFD - ���������, ���������� �������� �������                
                                                                         // ��������, ����� 26 �����, �� �������� ������
                                                                         // ��������� �� ���
            PFD.nVersion = 1;
            PFD.nSize = (short)Marshal.SizeOf(PFD);
            PFD.dwFlags = Gdi.PFD_DRAW_TO_WINDOW                         // ����� ������, ������ � ����
                | Gdi.PFD_SUPPORT_OPENGL                                 // �������� ��������� OpenGL
                | Gdi.PFD_DOUBLEBUFFER;                                  // � ��������� ������� �����������
            PFD.iPixelType = Gdi.PFD_TYPE_RGBA;                          // ���� � ������� RGBA
                                                                         // R - �������, G - �������, B - �����
                                                                         // � - �����-�����, �������� �� ������� ������������
            PFD.cColorBits = 24;                                         // 24-� ������ ����� �����
            PFD.cDepthBits = 32;                                         // 32-� ������ ����� �������
            PFD.iLayerType = Gdi.PFD_MAIN_PLANE;
            int nPixelFormat = Gdi.ChoosePixelFormat(HDC3D, ref PFD);
            Gdi.SetPixelFormat(HDC3D, nPixelFormat, ref PFD);
            HRC3D = Wgl.wglCreateContext(HDC3D);
            Wgl.wglMakeCurrent(HDC3D, HRC3D);                            // ������ �������� ��������������� �������
        }

        // ���������� �������
        private void Form1_Resize(object sender, EventArgs e)
        {
            // ����������� ��������
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glLoadIdentity();
            int w = ClientRectangle.Width;
            int h = ClientRectangle.Height;
            Glu.gluPerspective(30.0, (double)w / h, 2.0, 200000.0);
            // ����������� ������� ������
            Gl.glViewport(0, 0, w, h);
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            int R = 0;
            int G = 0;
            int B = 0;
            float r = 5.0f;
            //Gl.glEnable(Gl.GL_DEPTH_TEST);
            // ��������� ������� ������� ������� � �����
            Gl.glClearColor(R, G, B, 1); // 0 <= R,G,B <= 1
            // ������� �������
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
            Gl.glMatrixMode(Gl.GL_MODELVIEW); // ������ ������� ���� �� ���������� �������� ������ ����������
                                              // GL_MODELVIEW - ������� �������
            Gl.glLoadIdentity();              // �������� ��������� ������� � �������

            // ������� ��������������
            Gl.glTranslatef(0, 0, -r);        // ������� � ����������� ������� � ������������ (x, y, z). 
                                              //Gl.glScalef(a, b, c);           // ��������������� � �������������� (a, b, c) �� ���� ��������� (x, y, z) ��������������     
            
            GL.glBegin(GL.GL_QUADS);
            GL.glColor3f(1.0f, 1.0f, 1.0f);
            DrawSphere(1.0f, 20, 20);        // radius - ������, slices - ������� (���������), stacks - ������ (�����������)
            // ������� ������������ ������ ������� ������� ���������
            //Gl.glRotatef(fi, 1.0f, 0, 0);     //  ������� �� ���� alpha (� ��������) ������ ������� ������� ������������ 
            //Gl.glRotatef(psi, 0, 1.0f, 0);

            // First line GL_LINES - ��������� ������� 
            //            GL_LINE_STRIP � ������� �������
            //            GL_LINE_LOOP � ��������� �����
            //            glEnable(Gl.GL_LINE_SMOOTH) - ��������� ������ ����������� �����
            //Gl.glBegin(Gl.GL_LINES);
                //Gl.glColor3f(0, 0, 1f);
                //Gl.glVertex3f(0, 0, 0);           // Vertex - �������
                //Gl.glVertex3f(0, 0, 10);

            // Second line 
            //Gl.glBegin(Gl.GL_LINES);
                //Gl.glColor3f(0, 0, 1f);
                //Gl.glVertex3f(1, 1, 0);
                //Gl.glVertex3f(1, 1, 10);

            // �������������� GL_TRIANGLES - �� ������� ������������
            //                GL_TRIANGLE_STRIP - ������� ������������
            //                GL_TRIANGLE_FAN - ���� ������������
            //                GL_QUADS - �� ������� ����������������
            //                GL_POLYGON - �������������

            //Gl.glBegin(Gl.GL_TRIANGLES);
                //Gl.glColor3f(0, 0, 1f);
                //Gl.glVertex3f(1f, 0, 0);
                //Gl.glVertex3f(1f, 2f, 0);
                //Gl.glVertex3f(1f, 3f, 0);


            Gl.glEnd();
            Gl.glFinish();           // ����������� ���������
            Gdi.SwapBuffers(HDC3D);  // ������ ������� �������� � ������ ������ ��� ������� �����������

        }

        // ������� ��������� ����������� ���������� ������ OpenGL
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Wgl.wglMakeCurrent(IntPtr.Zero, IntPtr.Zero);
            Wgl.wglDeleteContext(HRC3D); // 7
            User.ReleaseDC(Handle3D, HDC3D); // 8
        }

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

        // ���������� ��������� ��������� � ������������� ����������� EraseBackground
        class MyGL
        {
            // ������������� ������� EraseBackground
            internal const int WM_ERASEBKGND = 0x0014;
            // ����� ��������� ���������� ����������� Windows
            [DllImport("user32.dll")]
            internal static extern bool InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase);
        }

        // ����� ���������� ����������� ���������� �����
        void InvalidateRect()
        {
            MyGL.InvalidateRect(Handle, IntPtr.Zero, false);
        }
    }
}