﻿namespace MqUtil.Api.Image{
	public interface IImageSession : ICloneable{
		string Name { get; }
		int AnatCount { get; }
		int FuncCount { get; }
		int DwiCount { get; }
		int ParMapCount { get; }
		IImageSeries GetAnatAt(int index);
		IImageSeries GetFuncAt(int index);
		IImageSeries GetDwiAt(int index);
		IImageSeries GetParMapAt(int index);
		IImageSeries GetAt(MriType type, int index);
		void AddAnat(IImageSeries anat);
		void AddFunc(IImageSeries func);
		void AddDwi(IImageSeries dwi);
		void AddParMap(IImageSeries parMap);
		void Write(BinaryWriter writer);
	}
}