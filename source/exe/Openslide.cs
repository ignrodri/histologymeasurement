using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Serialization;

class OpenslideLevel {

	public int level;
	public long h;
	public long w;
	
	public OpenslideLevel(int n) {
		level = n;
		h = 0;
		w = 0;
	}
}

class OpenslideImage {

	[System.Runtime.InteropServices.DllImport("libopenslide-0", EntryPoint = "openslide_open", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
	private static extern IntPtr OpenSlideOpen(IntPtr f); 

	[System.Runtime.InteropServices.DllImport("libopenslide-0", EntryPoint = "openslide_get_level_count", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
	private static extern int OpenSlideCount(IntPtr image); 

	[System.Runtime.InteropServices.DllImport("libopenslide-0", EntryPoint = "openslide_get_level_dimensions", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
	private static extern void OpenSlideDimensions(IntPtr image, int level, ref long w, ref long h); 

	[System.Runtime.InteropServices.DllImport("libopenslide-0", EntryPoint = "openslide_read_region", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
	private static extern void OpenSlideReadRegion(IntPtr image, [In, Out] int[] pix, long x, long y, int level, long w, long h); 

	IntPtr image;
	string sfile;
	OpenslideLevel[] levels;
	
	double x1;
	double x2;
	double y1;
	double y2;
	
	public OpenslideImage(string s) {
		x1 = 0;
		x2 = 1;
		y1 = 0;
		y2 = 1;
		sfile = s;
		image = OpenSlideOpen(Marshal.StringToHGlobalAnsi(sfile));
		int count = OpenSlideCount(image);
		levels = new OpenslideLevel[count];
		for (int i = 0; i < count; i++) {
			levels[i] = new OpenslideLevel(i);
			OpenSlideDimensions(image, i, ref levels[i].w, ref levels[i].h);
		}
	}
	
	public Bitmap GetImage(long x, long y, int level, long w, long h) {
		int[] pix = new int[w * h];
		OpenSlideReadRegion(image, pix, x, y, level, w, h);
		Bitmap bmp = new Bitmap((int)w, (int)h);
		for (int i = 0; i < w; i++) {
			for (int j = 0; j < h; j++) {
				bmp.SetPixel(i, j, Color.FromArgb(pix[j * w + i]));
			}
		}
		return bmp;
	}

	public Bitmap GetImage(int level) {
		return GetImage(0, 0, level, levels[level].w, levels[level].h);
	}
	
	public Bitmap Visualize() {
		int level = 0;
		while (!islevelvisual(level)) {
			level++;
		}
		long x = regular(x1, levels[0].w);
		long y = regular(y1, levels[0].h);
		long w = regular(x2 - x1, levels[level].w);
		long h = regular(y2 - y1, levels[level].h);
		return GetImage(x, y, level, w, h);
	}
	
	public Bitmap Save() {
		int level = 0;
		while (!islevelsave(level)) {
			level++;
		}
		long x = regular(x1, levels[0].w);
		long y = regular(y1, levels[0].h);
		long w = regular(x2 - x1, levels[level].w);
		long h = regular(y2 - y1, levels[level].h);
		MessageBox.Show(String.Format("Zoom {0} Pix {1} x {2}", level, w, h));
		return GetImage(x, y, level, w, h);
	}
	
	bool islevelvisual(int level) {
		long w = regular(x2 - x1, levels[level].w);
		if (w > 1000) {
			return false;
		}
		long h = regular(y2 - y1, levels[level].h);
		if (h > 1000) {
			return false;
		}
		return true;
	}
		
	bool islevelsave(int level) {
		long w = regular(x2 - x1, levels[level].w);
		if (w > 10000) {
			return false;
		}
		long h = regular(y2 - y1, levels[level].h);
		if (h > 10000) {
			return false;
		}
		return true;
	}
	
	long regular(double x, long l) {
		long res = (long) Math.Round(x * l);
		if (res < 0) {
			return 0;
		}
		if (res > l) {
			return l;
		}
		return res;
	}
	
	double linear(double a1, double a2, double t) {
		return a2 * t + a1 * (1 - t);
	}
	
	public void click(double xx1, double xx2, double yy1, double yy2) {
		double d1, d2;
		d1 = linear(x1, x2, xx1);
		d2 = linear(x1, x2, xx2);
		x1 = Math.Max(0, d1);
		x2 = Math.Min(1, d2);
		x2 = Math.Max(x2, x1 + 0.001);
		d1 = linear(y1, y2, yy1);
		d2 = linear(y1, y2, yy2);
		y1 = Math.Max(0, d1);
		y2 = Math.Min(1, d2);
		y2 = Math.Max(y2, y1 + 0.001);
	}
}
		

class Openslide : Form {

	OpenslideImage im;
	Panel pa;
	PictureBox pb;
	Button load, save, zoom;
	bool clicked;
	double x1, x2, y1, y2;
	
	public Openslide() {
		pa = new Panel();
		pb = new PictureBox();
		pb.SizeMode = PictureBoxSizeMode.Zoom;
		pb.Left = 0;
		pb.Top = 0;
		pa.Left = 0;
		pa.Top = 0;
	//	pa.AutoScroll = true;
		pb.MouseClick += new MouseEventHandler(pb_click);
		pb.MouseWheel += new MouseEventHandler(pb_wheel);
		pa.MouseWheel += new MouseEventHandler(pb_wheel);
		MouseWheel += new MouseEventHandler(pb_wheel);
		pa.Width = 700;
		pa.Height = 500;
		pb.Width = 700;
		pb.Height = 500;
		Width = pa.Width + 50;
		Height = pa.Height + 150;
		load = new Button();
		load.Top = pa.Height + 40;
		load.Left = 10;
		load.Text = "Cargar";
		load.Size = load.PreferredSize;
		load.Click += new EventHandler(load_click);
		save = new Button();
		save.Top = pa.Height + 40;
		save.Text = "Guardar";
		save.Size = save.PreferredSize;
		save.Click += new EventHandler(save_click);
		save.Left = pa.Left + (pa.Width - save.Width) / 2;
		zoom = new Button();
		zoom.Top = pa.Height + 40;
		zoom.Text = "x 2";
		zoom.Size = zoom.PreferredSize;
		zoom.Click += new EventHandler(zoom_click);
		zoom.Left = pa.Left + pa.Width - save.Width;
		Controls.AddRange(new Control[] {pa, load, save, zoom});
		pa.Controls.Add(pb);
		clicked = false;
		x1 = 0;
		x2 = 500;
		y1 = 0;
		y2 = 500;
		Icon = Icon.ExtractAssociatedIcon(Path.Combine(Application.StartupPath, "transformar.exe"));
	}
	
	void draw_im() {
		Bitmap bi = im.Visualize();
		double mult = Math.Min(((double)pa.Width) / ((double)bi.Width), ((double)pa.Height) / ((double)bi.Height));
		pb.Width = (int) Math.Round(mult * bi.Width);
		pb.Height = (int) Math.Round(mult * bi.Height);
		pb.Image = bi;
		clicked = false;
	}
	
	void load_click(Object sender, EventArgs e) {
		using (OpenFileDialog ofd = new OpenFileDialog()) {
			ofd.Filter = "BIF scan (*.bif)|*.bif";
			if (ofd.ShowDialog() == DialogResult.OK) {
				im = new OpenslideImage(ofd.FileName);
				draw_im();
			}
		}
	}
	
	void save_click(Object sender, EventArgs e) {
		using (SaveFileDialog sfd = new SaveFileDialog()) {
			sfd.Filter = "JPEG (*.jpg)|*.jpg";
			if (sfd.ShowDialog() == DialogResult.OK) {
				im.Save().Save(sfd.FileName);
			}
		}
	}
	
	void zoom_click(Object sender, EventArgs e) {
		im.click(-0.5, 1.5, -0.5, 1.5);
		draw_im();
	}

	void pb_click(Object sender, MouseEventArgs e) {
		if (!clicked) {
			x1 = e.X;
			y1 = e.Y;
			clicked = true;
		}
		else {
			x2 = e.X;
			y2 = e.Y;
			im.click(x1 / pb.Width, x2 / pb.Width, y1 / pb.Height, y2 / pb.Height);
			draw_im();
		}
	}

	void pb_wheel(Object sender, MouseEventArgs e) {
		if (e.Delta < 0) {
			im.click(-0.1, 1.1, -0.1, 1.1);
			draw_im();
		}
		if (e.Delta > 0) {
			im.click(0.1, 0.9, 0.1, 0.9);
			draw_im();
		}
	}

	[STAThreadAttribute]
	static void Main() {
		Application.Run(new Openslide());
	//	OpenslideImage im = new OpenslideImage("C:\\CNIC Data\\neumo_4_5_201602041257.bif");
	//	Bitmap bmp = im.GetImage(3);
	//	bmp.Save("C:\\CNIC Data\\neumo.transformada.jpg");
	//	bmp = im.GetImage(30000, 30000, 0, 6000, 6000);
	//	bmp.Save("C:\\CNIC Data\\neumo.parcial.jpg");
	}
}