﻿namespace MqUtil.Api.Image{
	public interface IImageMetadata : ICloneable{
		double[,,] RigidBodyTransformation{ get; set; }
		string[,] Events{ get; set; }
		float[,][,,] DeformationField { get; set; }
		string GetBIDSEntity(string entityName);
		void Write(BinaryWriter writer);
	}
}