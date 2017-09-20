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

class Region {

	public int label;
	public int total;
	
	public Region(int l) {
		label = l;
		total = 0;
	}
}

class RegionCollection {

	int[] val;
	
	public RegionCollection() {
		val = new int[0];
	}
	
	public void Add(int r) {
		if (r >= 0) {
			if (r < val.Length) {
				val[r]++;
			}
			else {
				int[] nv = new int[r + 1];
				for (int i = 0; i < val.Length; i++) {
					nv[i] = val[i];
				}
				for (int i = val.Length; i < r; i++) {
					nv[i] = 0;
				}
				nv[r] = 1;
				val = nv;
			}
		}
	}
	
	public int GetVal(int r) {
		if ((r >= 0) && (r < val.Length)) {
			return val[r];
		}
		return 0;
	}
	
	public int Count {
		get {
			return val.Length;
		}
	}
}

class PixelCollection {

	public int Count;
	int[] val;
	
	public PixelCollection() {
		Count = 0;
		val = new int[1000];
	}
	
	public void Add(int pi) {
		Count = Count + 1;
		if (Count > val.Length) {
			int[] nv = new int[2 * val.Length];
			Array.Copy(val, 0, nv, 0, val.Length);
			val = nv;
		}
		val[Count - 1] = pi;
	}
	
	public int Take() {
		if (Count > 0) {
			Count = Count - 1;
			return val[Count];
		}
		return 0;
	}
}
		

class Connect {

	int wi;
	int he;
	int[,] mask;
	PixelCollection li;
	List<Region> reg;
	double minsize;
	
	public Connect(int width, int height, int[,] m) {
		wi = width;
		he = height;
		mask = new int[wi, he];
		for (int i = 0; i < wi; i++) {
			for (int j = 0; j < he; j++) {
				mask[i, j] = ((m[i, j] == 1) ? 1 : 0);
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
					r = new Region(numcolor);
					AddToQueue(i, j, numcolor);
					ProcessQueue(r);
				}
			}
		}
	}
	
	public int[,] Connection() {
		Process();
		foreach (Region r in reg) {
			if (r.total < minsize) {
				MaskSet(r.label, 1);
			}
			else {
				MaskSet(r.label, 0);
			}
		}
		Process();
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