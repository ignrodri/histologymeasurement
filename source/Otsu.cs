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

class Otsu {

	public static int[,] OtsuMask(Bitmap bi) {
		int i, j;
		int wi = bi.Width;
		int he = bi.Height;
		byte[,] b = new byte[wi, he];
		double[] histogram = new double[256];
		for (i = 0; i < 256; i++) {
			histogram[i] = 0;
		}
		for (i = 0; i < wi; i++) {
			for (j = 0; j < he; j++) {
				b[i, j] = bi.GetPixel(i, j).G;
				histogram[b[i, j]]++;
			}
		}
		/*
		b = Gauss(b, wi, he);
		for (i = 0; i < wi; i++) {
			for (j = 0; j < he; j++) {
				histogram[b[i, j]]++;
			}
		}
		*/
		int thres = 0;
		double max = 0;
		double v;
		for (i = 0; i < 256; i++) {
			v = Var(histogram, i);
			if (v > max) {
				thres = i;
				max = v;
			}
		}
		int[,] res = new int[wi, he];
		int c;
		for (i = 0; i < wi; i++) {
			for (j = 0; j < he; j++) {
				c = ((b[i, j] < thres) ? 1 : 0);
				res[i, j] = c;
			}
		}
		return res;
	//	return new ConnectOtsu(bi.Width, bi.Height, res).Connection();
	}
	
	static byte[,] Gauss(byte[,] b, int wi, int he) {
		byte[,] res = new byte[wi, he];
		int i, j, di, dj;
		double n, r;
		for (i = 0; i < wi; i++) {
			for (j = 0; j < he; j++) {
				n = 0;
				r = 0;
				for (di = (i - 2); di < (i + 3); di++) {
					for (dj = (j - 2); dj < (j + 3); dj++) {
						if ((di >= 0) && (di < wi) && (dj >= 0) && (dj < he)) {
							n++;
							r = r + b[di, dj];
						}
					}
					
				}
				res[i, j] = (byte) Math.Round(r / n);
			}
		}
		return res;
	}
	
	static double Var(double[] histo, int thres) {
		int i;
		double w0 = 0;
		double w1 = 0;
		double m0 = 0;
		double m1 = 0;
		for (i = 0; i < thres; i++) {
			w0 = w0 + histo[i];
			m0 = m0 + i * histo[i];
		}
		for (i = thres; i < histo.Length; i++) {
			w1 = w1 + histo[i];
			m1 = m1 + i * histo[i];
		}
		m0 = m0 / w0;
		m1 = m1 / w1;
		return w0 * w1 * (m0 - m1) * (m0 - m1);
	}
}