/*
 * @author Sebastian Lohmann
 */
namespace BudDetection {
	using System;
	using System.Drawing;
	using Glaukopis.SharpAccessoryIntegration;
	using SharpAccessory.Imaging.Processors;
	using SharpAccessory.Imaging.Segmentation;
	public static class ColorDeconvolution{
		private const int RedThreshold=128;
		private const int DabThreshold=128;

        // Pro Kachel 
        public static IResult<bool> Execute(Bitmap image) {
            // Console.WriteLine("ColorDeconvolution.Execute()");

            var r=new Result<bool>();
			using(var source=image.Clone() as Bitmap){
                /*
				var gspR=new GrayscaleProcessor(source.Clone() as Bitmap,RgbToGrayscaleConversion.JustReds);
				gspR.Dispose();
				r.DebugBitmaps.Add(Tuple.Create(gspR.Bitmap,"Red"));
				r.DebugLayers.Add(createLayer(gspR.Bitmap,RedThreshold,"Red"));
				r.DebugVariables.Add("RedThreshold",RedThreshold);

				var gpDAB = new SharpAccessory.Imaging.Filters.ColorDeconvolution().Get2ndStain(
                    source, 
                    SharpAccessory.Imaging.Filters.ColorDeconvolution.KnownStain.HaematoxylinDAB);

                gpDAB.Dispose();
                r.DebugBitmaps.Add(Tuple.Create(gpDAB.Bitmap,"DAB"));
                var dabLayer=createLayer(gpDAB.Bitmap,DabThreshold,"DAB");
                r.DebugLayers.Add(dabLayer);
                r.DebugVariables.Add("DabThreshold",DabThreshold);
                r.Value= 0 < dabLayer.Objects.Count;

                // sobald ein bissel (>1) hämoglobin drin sind, dann ist es betrachtungswürdig

                */
                
                // my version
                var gpDAB = new SharpAccessory.Imaging.Filters.ColorDeconvolution().Get2ndStain(
                    source,
                    SharpAccessory.Imaging.Filters.ColorDeconvolution.KnownStain.HaematoxylinDAB);

                // grauwert 0-255, ab 40 ist es wohl hämoglobin
                // tile ist 100x100 => 10.000 pixel, wenn es min 10 sind deren wert > schwellwert ist 
                // dann die kachel behalten
                int counter = 0;
                int schwellWert = 40;
                for (int i = 0; i < gpDAB.Height; i++) {
                    for (int j = 0; j < gpDAB.Width; j++) {
                        if (gpDAB[i,j] > schwellWert && ++counter > 10) {                            
                            r.Value = true;
                            return r;
                        }
                    }
                }
            }

            r.Value = false;
            return r;
		}

		private static ObjectLayer createLayer(Bitmap bitmap,int threshold,string name){
			var map=new Map(bitmap.Width,bitmap.Height);
			using(var gsp=new GrayscaleProcessor(bitmap,RgbToGrayscaleConversion.JustReds)){
				gsp.WriteBack=false;
				foreach(var grayscalePixel in gsp.Pixels()) map[grayscalePixel.X,grayscalePixel.Y]=grayscalePixel.V>threshold?1u:0u;
			}
			var layer=new ConnectedComponentCollector().Execute(map);
			layer.Name=name;
			return layer;
		}
	}
}
