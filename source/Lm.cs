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

class Lm {

	int wi;
	int he;
	int[,] mask;
	
	public Lm(int width, int height, int[,] m) {
		wi = width;
		he = height;
		mask = new int[wi, he];
		for (int i = 0; i < wi; i++) {
			for (int j = 0; j < he; j++) {
				mask[i, j] = ((m[i, j] == 1) ? 1 : 0);
			}
		}
	}
	
	public double[] Process() {
		double[] res = new double[2];
		res[0] = 0;
		res[1] = 0;
		Random rnd = new Random();
		int i = 0;
		int ntest = 1000;
		int x, y;
		while (i < ntest) {
			x = rnd.Next(wi);
			y = rnd.Next(he);
			if (!IsBorder(x, y)) {
				i++;
				res[0] = res[0] + GetX(x, x, y);
				res[1] = res[1] + GetY(x, y, y);
			}
		}
		res[0] = res[0] / ntest;
		res[1] = res[1] / ntest;
		return res;
	}
	
	bool IsBorder(int x, int y) {
		return ((x == 0) || (x == (wi - 1)) || (y == 0) || (y == (he - 1)) || (mask[x, y] == 1));
	}
	
	double GetX(int xmin, int xmax, int y) {
		if (IsBorder(xmin, y) && IsBorder(xmax, y)) {
			return (xmax - xmin);
		}
		if (IsBorder(xmin, y)) {
			return GetX(xmin, xmax + 1, y);
		}
		if (IsBorder(xmax, y)) {
			return GetX(xmin - 1, xmax, y);
		}
		return GetX(xmin - 1, xmax + 1, y);
	}
	
	double GetY(int x, int ymin, int ymax) {
		if (IsBorder(x, ymin) && IsBorder(x, ymax)) {
			return (ymax - ymin);
		}
		if (IsBorder(x, ymin)) {
			return GetY(x, ymin, ymax + 1);
		}
		if (IsBorder(x, ymax)) {
			return GetY(x, ymin - 1, ymax);
		}
		return GetY(x, ymin - 1, ymax + 1);
	}
}