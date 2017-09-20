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

class ConnectOtsu {

	int wi;
	int he;
	int[,] mask;
	PixelCollection li;
	List<Region> reg;
	double minsize;
	
	public ConnectOtsu(int width, int height, int[,] m) {
		wi = width;
		he = height;
		mask = new int[wi, he];
		for (int i = 0; i < wi; i++) {
			for (int j = 0; j < he; j++) {
				mask[i, j] = m[i, j];
			}
		}
		li = new PixelCollection();
		reg = new List<Region>();
		minsize = ((double)wi) * ((double)he) / 10000;
	}
	
	void Process() {
		reg = new List<Region>();
		li = new PixelCollection();
		int numcolor = 2;
		Region r = new Region(2);
		int i, j;
		for (i = 0; i < wi; i++) {
			AddToQueue(i, 0, 2);
			AddToQueue(i, he - 1, 2);
		}
		for (i = 0; i < he; i++) {
			AddToQueue(0, i, 2);
			AddToQueue(wi - 1, i, 2);
		}
		ProcessQueue(r);
		for (i = 0; i < wi; i++) {
			for (j = 0; j < he; j++) {
				if (mask[i, j] == 0) {
					numcolor++;
					r = new Region(2 * numcolor);
					AddToQueue(i, j, 2 * numcolor);
					ProcessQueue(r);
				}
				if (mask[i, j] == 1) {
					numcolor++;
					r = new Region(2 * numcolor + 1);
					AddToQueue(i, j, 2 * numcolor + 1);
					ProcessQueue(r);
				}
			}
		}
	}
	
	public int[,] Connection() {
		Process();
		foreach (Region r in reg) {
			if (r.total < minsize) {
				MaskSet(r.label, 1 - (r.label % 2));
			}
			else {
				MaskSet(r.label, r.label % 2);
			}
		}
		return mask;
	}
	
	void ProcessQueue(Region r) {
		int pi, x, y;
		while (li.Count > 0) {
			pi = li.Take();
			x = pi % wi;
			y = pi / wi;
			r.total++;
			AddToQueue(x - 1, y, r.label);
			AddToQueue(x + 1, y, r.label);
			AddToQueue(x, y - 1, r.label);
			AddToQueue(x, y + 1, r.label);
		}
		reg.Add(r);
	}
	
	void AddToQueue(int x, int y, int v) {
		if ((x >= 0) && (x < wi) && (y >= 0) && (y < he)) {
			if (mask[x, y] == 0) {
				mask[x, y] = v;
				li.Add(x + wi * y);
			}
		}
	}

	void MaskSet(int o, int n) {
		for (int i = 0; i < wi; i++) {
			for (int j = 0; j < he; j++) {
				if (mask[i, j] == o) {
					mask[i, j] = n;
				}
			}
		}
	}			
}