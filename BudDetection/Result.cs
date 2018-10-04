/*
 * @author Sebastian Lohmann
 */
namespace BudDetection {
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using SharpAccessory.Imaging.Segmentation;
	public class Result<T>:IResult<T>{
		public T Value{get;set;}
		public List<Tuple<Bitmap,string>> DebugBitmaps=new List<Tuple<Bitmap,string>>();
		public List<ObjectLayer> DebugLayers=new List<ObjectLayer>();
		public Dictionary<string,double> DebugVariables=new Dictionary<string,double>();
		public IList<Tuple<Bitmap,string>> Bitmaps=>this.DebugBitmaps;
		public IList<ObjectLayer> ObjectLayers=>this.DebugLayers;
		public IDictionary<string,double> Variables=>this.DebugVariables;
	}
}
