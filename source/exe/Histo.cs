using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Serialization;

using ExcelLibrary.SpreadSheet;

class Histo : Form {

	Panel pa;
	PictureBox pb;
	Button load, air, wall, viewm, viewp, proc;
	Bitmap orig;
	Bitmap[] bi;
	int index;
	int imWidth;
	int imHeight;
	double mult;
	int[,] mask;
	List<Point> polygon;
	
	public Histo() {
		pa = new Panel();
		pb = new PictureBox();
		pb.SizeMode = PictureBoxSizeMode.Zoom;
		pb.Left = 0;
		pb.Top = 0;
		pa.Left = 0;
		pa.Top = 0;
		pb.MouseClick += new MouseEventHandler(pb_click);
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
		air = new Button();
		air.Top = pa.Height + 40;
		air.Text = "Aire";
		air.Size = air.PreferredSize;
		air.Click += new EventHandler(air_click);
		air.Left = pa.Left + pa.Width / 4 - air.Width / 2;
		wall = new Button();
		wall.Top = pa.Height + 40;
		wall.Text = "Pared";
		wall.Size = wall.PreferredSize;
		wall.Click += new EventHandler(wall_click);
		wall.Left = pa.Left + (pa.Width - wall.Width) / 2;
		viewm = new Button();
		viewm.Top = pa.Height + 40;
		viewm.Text = "<";
		viewm.Size = viewm.PreferredSize;
		viewm.Click += new EventHandler(viewm_click);
		viewm.Left = pa.Left + 3 * pa.Width / 4 - viewm.Width;
		viewp = new Button();
		viewp.Top = pa.Height + 40;
		viewp.Text = ">";
		viewp.Size = viewp.PreferredSize;
		viewp.Click += new EventHandler(viewp_click);
		viewp.Left = pa.Left + 3 * pa.Width / 4;
		proc = new Button();
		proc.Top = pa.Height + 40;
		proc.Text = "Procesar";
		proc.Size = proc.PreferredSize;
		proc.Click += new EventHandler(proc_click);
		proc.Left = pa.Left + pa.Width - proc.Width;
		Controls.AddRange(new Control[] {pa, load, air, wall, viewm, viewp, proc});
		pa.Controls.Add(pb);
		Icon = Icon.ExtractAssociatedIcon(Path.Combine(Application.StartupPath, "histo.exe"));
		index = 0;
		mult = 0;
		imWidth = 0;
		imHeight = 0;
		bi = new Bitmap[4];
		polygon = new List<Point>();
		air.Enabled = false;
		wall.Enabled = false;
		viewm.Enabled = false;
		viewp.Enabled = false;
		proc.Enabled = false;
	}
	
	void load_click(Object sender, EventArgs e) {
		using (OpenFileDialog ofd = new OpenFileDialog()) {
			ofd.Filter = "JPEG (*.jpg)|*.jpg";
			if (ofd.ShowDialog() == DialogResult.OK) {
				orig = new Bitmap(ofd.FileName);
				imWidth = orig.Width;
				imHeight = orig.Height;
				mult = Math.Min(((double)pa.Width) / ((double)imWidth), ((double)pa.Height) / ((double)imHeight));
				pb.Width = (int) Math.Round(mult * imWidth);
				pb.Height = (int) Math.Round(mult * imHeight);
				pb.Image = orig;
				mask = Otsu.OtsuMask(orig);
				mask = new Connect(imWidth, imHeight, mask).Connection();
				index = 0;
				ImageMask();
				viewm.Enabled = true;
				viewp.Enabled = true;
				proc.Enabled = true;
			}
		}
	}
	
	void FillPolygon(int v) {
		air.Enabled = false;
		wall.Enabled = false;
		int i, j;
		int xmin = imWidth;
		int xmax = 0;
		int ymin = imHeight;
		int ymax = 0;
		for (i = 0; i < polygon.Count; i++) {
			xmin = Math.Min(xmin, polygon[i].X);
			xmax = Math.Max(xmax, polygon[i].X);
			ymin = Math.Min(ymin, polygon[i].Y);
			ymax = Math.Max(ymax, polygon[i].Y);
		}
		for (i = xmin; i < xmax; i++) {
			for (j = ymin; j < ymax; j++) {
				if (InPolygon(i, j)) {
					mask[i, j] = v;
				}
			}
		}
		mask = new Connect(imWidth, imHeight, mask).Connection();
		polygon = new List<Point>();
		ImageMask();
		pb.Image = bi[index];
	}
	
	void air_click(Object sender, EventArgs e) {
		FillPolygon(0);
	}
	
	void wall_click(Object sender, EventArgs e) {
		FillPolygon(1);
	}

	void viewm_click(Object sender, EventArgs e) {
		index = (index + bi.Length - 1) % bi.Length;
		pb.Image = bi[index];
	}
	
	void viewp_click(Object sender, EventArgs e) {
		index = (index + 1) % bi.Length;
		pb.Image = bi[index];
	}
	
	void proc_click(Object sender, EventArgs e) {
		int i, j;
		Workbook workbook = new Workbook();
		Worksheet warea = new Worksheet("Area");
		RegionCollection rc = new RegionCollection();
		for (i = 0; i < imWidth; i++) {
			for (j = 0; j < imHeight; j++) {
				rc.Add(mask[i, j]);
			}
		}
		j = 3;
		warea.Cells[0, 0] = new Cell("Region");
		warea.Cells[0, 1] = new Cell("Area");
		warea.Cells[1, 0] = new Cell("Pared");
		warea.Cells[1, 1] = new Cell(rc.GetVal(1));
		warea.Cells[2, 0] = new Cell("Exterior");
		warea.Cells[2, 1] = new Cell(rc.GetVal(2));
		for (i = 3; i < rc.Count; i++) {
			if (rc.GetVal(i) > 0) {
				warea.Cells[j, 0] = new Cell("Alveolo");
				warea.Cells[j, 1] = new Cell(rc.GetVal(i));
				j++;
			}
		}
		for (i = 0; i < 100; i++) {
			warea.Cells[i + j + 1, 0] = new Cell("");
		}
		double[] lm = new Lm(imWidth, imHeight, mask).Process();
		Worksheet wlm = new Worksheet("Lm");
		wlm.Cells[0, 0] = new Cell("Lm X");
		wlm.Cells[0, 1] = new Cell(lm[0]);
		wlm.Cells[1, 0] = new Cell("Lm y");
		wlm.Cells[1, 1] = new Cell(lm[1]);
		wlm.Cells[2, 0] = new Cell("Lm medio");
		wlm.Cells[2, 1] = new Cell((lm[0] + lm[1]) / 2);
		workbook.Worksheets.Add(warea);
		workbook.Worksheets.Add(wlm);
		string f = "C:\\histo\\histo.xls";
		workbook.Save(f);
		Process.Start(f);
	}

	void pb_click(Object sender, MouseEventArgs e) {
		if (e.Button == MouseButtons.Left) {
			double dx = e.X;
			double dy = e.Y;
			Point pi = new Point((int) Math.Round(dx / mult), (int) Math.Round(dy / mult));
			polygon.Add(pi);
			if (polygon.Count > 1) {
				Graphics g = Graphics.FromImage(bi[index]);
				g.DrawLine(Pens.Red, polygon[polygon.Count - 2], polygon[polygon.Count - 1]);
				pb.Image = bi[index];
			}
		}
		if (e.Button == MouseButtons.Right) {
			if (polygon.Count > 1) {
				Graphics g = Graphics.FromImage(bi[index]);
				g.DrawLine(Pens.Red, polygon[polygon.Count - 1], polygon[0]);
				pb.Image = bi[index];
				air.Enabled = true;
				wall.Enabled = true;
			}
		}
	}
	
	void ImageMask() {
		bi[0] = new Bitmap(imWidth, imHeight);
		bi[1] = new Bitmap(imWidth, imHeight);
		bi[2] = new Bitmap(imWidth, imHeight);
		bi[3] = new Bitmap(imWidth, imHeight);
		Color c0, c1, c2, c3;
		for (int i = 0; i < imWidth; i++) {
			for (int j = 0; j < imHeight; j++) {
				c0 = orig.GetPixel(i, j);
				c1 = Palette.GetColor((mask[i, j] == 1) ? 1 : 0);
				c2 = Palette.GetColor(mask[i, j]);
				c3 = Palette.Combine(c0, c2);
				bi[0].SetPixel(i, j, c0);
				bi[1].SetPixel(i, j, c1);
				bi[2].SetPixel(i, j, c2);
				bi[3].SetPixel(i, j, c3);
			}
		}
	}
	
	bool InPolygon(int x, int y) {
		int cn = 0;
		int i, j;
		double vt;
		for (i = 0; i < polygon.Count; i++) {
			j = (i + 1) % polygon.Count;
			if (((polygon[i].Y <= y) && (polygon[j].Y > y)) || ((polygon[i].Y > y) && (polygon[j].Y <= y))) {
				vt = ((double)(y - polygon[i].Y)) / (polygon[j].Y - polygon[i].Y);
				if (x < polygon[i].X + vt * (polygon[j].X - polygon[i].X)) {
					cn++;
				}
			}
		}
		return ((cn % 2) != 0);
	}
	
	[STAThreadAttribute]
	static void Main() {
		Application.Run(new Histo());
	}
}