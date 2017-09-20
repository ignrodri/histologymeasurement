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

class Palette {

	static Color[] c1 = new Color[] {Color.Black, Color.White, Color.Black};
	static Color[] c2 = new Color[125];
	
	static Palette() {
	
		Random r = new Random();
		int i;
		int ca, cb, cc;
		for (i = 0; i < c2.Length; i++) {
			ca = r.Next(256);
			cb = r.Next(256);
			cc = r.Next(256);
			c2[i] = Color.FromArgb(ca, cb, cc);
		}
	}
			
			
//	static Color[] c2 = new Color[] {Color.Red, Color.Blue, Color.Yellow, Color.Green, Color.Brown, Color.Orange, Color.Aqua, Color.DeepPink, Color.Firebrick};

	public static Color GetColor(int i) {
		if (i < 0) {
			return Color.Black;
		}
		if (i < 3) {
			return c1[i];
		}
		return c2[(i - 3) % c2.Length];
	}
	
	static int Comb(int ix, int iy) {
		return (int) Math.Round(0.8 * ix + 0.2 * iy);
	}
	
	public static Color Combine(Color cx, Color cy) {
		return Color.FromArgb(Comb(cx.R, cy.R), Comb(cx.G, cy.G), Comb(cx.B, cy.B));
	}
}