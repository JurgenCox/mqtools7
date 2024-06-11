﻿using MqApi.Drawing.Image;
using System.Reflection;
namespace MqApi.Drawing{
	public class Bitmap2{
		/// <summary>
		/// Initializes a new instance of the <code>Bitmap2</code> class with the specified size.
		/// </summary>
		/// <param name="width">The width in pixels.</param>
		/// <param name="height">The height in pixels.</param>
		public Bitmap2(int width, int height){
			Data = new int[width, height];
		}
		public Bitmap2(int[,] data){
			Data = data;
		}
		/// <summary>
		/// Matrix with argb values of pixels which are assumed to be represented as int containing 
		/// one byte for a, r, g and b each.
		/// </summary>
		public int[,] Data{ get; }
		public int Width => Data.GetLength(0);
		public int Height => Data.GetLength(1);
		public void SetPixel(int i, int j, int argb){
			if (i < 0 || j < 0){
				return;
			}
			if (i >= Data.GetLength(0) || j >= Data.GetLength(1)){
				return;
			}
			Data[i, j] = argb;
		}
		public int GetPixel(int i, int j){
			return Data[i, j];
		}
		public void MirrorY(){
			for (int i = 0; i < Width; i++){
				for (int j = 0; j < Height / 2; j++){
					int p = GetPixel(i, j);
					SetPixel(i, j, GetPixel(i, Height - 1 - j));
					SetPixel(i, Height - 1 - j, p);
				}
			}
		}
		public void MirrorX(){
			for (int j = 0; j < Height; j++){
				for (int i = 0; i < Width / 2; i++){
					int p = GetPixel(i, j);
					SetPixel(i, j, GetPixel(Width - 1 - i, j));
					SetPixel(Width - 1 - i, j, p);
				}
			}
		}
		public Bitmap2 Transpose(){
			Bitmap2 result = new Bitmap2(Height, Width);
			for (int i = 0; i < Width; i++){
				for (int j = 0; j < Height; j++){
					result.SetPixel(j, i, GetPixel(i, j));
				}
			}
			return result;
		}
		public void DrawPath(int argb, int x, int y, int[] xpath, int[] ypath){
			for (int i = 0; i < xpath.Length; i++){
				SetPixel(x + xpath[i], y + ypath[i], argb);
			}
		}
		public void FillRectangle(int argb, int x, int y, int wid, int height){
			for (int i = x; i < x + wid; i++){
				for (int j = y; j < y + height; j++){
					SetPixel(i, j, argb);
				}
			}
		}
		public void DrawRectangle(int argb, int x, int y, int wid, int height){
			const int lw = 1;
			for (int j = x; j <= x + wid; j++){
				for (int w = 0; w < lw; w++){
					SetPixel(j, y + w, argb);
					SetPixel(j, y + height - w, argb);
				}
			}
			for (int i = y; i <= y + height; i++){
				for (int w = 0; w < lw; w++){
					SetPixel(x + w, i, argb);
					SetPixel(x + wid - w, i, argb);
				}
			}
		}
		public void DrawLine(int argb, int x1, int y1, int x2, int y2, bool dots){
			DrawLine(argb, x1, y1, x2, y2, dots, 1);
		}
		public void DrawLine(int argb, float x1, float y1, float x2, float y2, bool dots){
			DrawLine(argb, x1, y1, x2, y2, dots, 1);
		}
		public void DrawLine(int argb, float x1, float y1, float x2, float y2, bool dots, int width1){
			DrawLine(argb, (int) x1, (int) y1, (int) x2, (int) y2, dots, width1);
		}
		public void DrawLine(int argb, int x1, int y1, int x2, int y2, bool dots, int width1){
			float dx = x1 - x2;
			float dy = y1 - y2;
			if (dx == 0 && dy == 0){
				SetPixel(x1, y1, argb);
				return;
			}
			if (Math.Abs(dx) > Math.Abs(dy)){
				double a = (y1 - y2) / (double) (x1 - x2);
				for (int x = Math.Min(x1, x2); x <= Math.Max(x1, x2); x++){
					if (!dots || x % 2 == 1){
						int y = (int) Math.Round(y1 + a * (x - x1));
						for (int b = -((width1 - 1) / 2); b <= width1 / 2; b++){
							SetPixel(x, y + b, argb);
						}
					}
				}
			} else{
				double a = (x1 - x2) / (double) (y1 - y2);
				for (int y = Math.Min(y1, y2); y <= Math.Max(y1, y2); y++){
					if (!dots || y % 2 == 1){
						int x = (int) Math.Round(x1 + a * (y - y1));
						for (int b = -((width1 - 1) / 2); b <= width1 / 2; b++){
							SetPixel(x + b, y, argb);
						}
					}
				}
			}
		}
		public static Bitmap2 GetImage(string file){
			return GetImage("MqApi.img.", file);
		}
		public static Bitmap2 GetImage(string prefix, string file){
			Assembly thisExe = Assembly.GetExecutingAssembly();
			Stream file1 = thisExe.GetManifestResourceStream(prefix + file);
			if (file1 == null){
				return null;
			}
			Bitmap2 bm = Image2.ReadImage(file1);
			file1.Close();
			return bm;
		}
		public static Bitmap2 GetImage(Assembly exe, string path){
			Stream file1 = exe.GetManifestResourceStream(path);
			if (file1 == null){
				return null;
			}
			Bitmap2 bm = Image2.ReadImage(file1);
			file1.Close();
			return bm;
		}
		public static Bitmap2 ReadImage(Stream file1){
			return Image2.ReadImage(file1);
		}
	}
}