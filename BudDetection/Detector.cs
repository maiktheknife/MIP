/*
 * @author Sebastian Lohmann
 */
namespace BudDetection {
	using System.Drawing;
    using System;
	using Glaukopis.SharpAccessoryIntegration;
	using SharpAccessory.Imaging.Classification.Features.Size;
    using SharpAccessory.Imaging.Classification.Features.Localization;
    using SharpAccessory.Imaging.Segmentation;

	public static class Detector {
        public static IResult<ObjectLayer> Execute(Bitmap bitmap) {
            var r = new Result<ObjectLayer>();
            var layer = createLayer(bitmap, 128, "BUD DetektorLayer");

            // hier aufrufen & berechnen
            ObjectPixels.ProcessLayer(layer);
            AxesOfCorrespondingEllipse.ProcessLayer(layer);
            PixelsAtLayerBorder.ProcessLayer(layer);

            // r.DebugLayers.Add(layer);
            r.Value = layer.CreateAbove((ImageObject io) => {
                // bei zoomstufe 0.5 

                // hier abrufen
                var minPixels = io.Features["ObjectPixels"].Value > 600;
                var maxPixels = io.Features["ObjectPixels"].Value < 14000;

                //if (!minPixels || !maxPixels) {
                //    return false;
                //}

                // Abstand bud zum Rand
                // wenn am Rand bzw abgeschnitten, dann nicht weiter betrachten               
                var distance = io.Features["PixelsAtLayerBorder"].Value < 5;

                // findet ähnlichkeiten zur einer ellipse
                //var axesMa = io.Features["MajorAxisOfCorrespondingEllipse"].Value < 3;
                //var axesMi = io.Features["MinorAxisOfCorrespondingEllipse"].Value < 3;

                var result = minPixels && maxPixels && distance; // && axesMa && axesMi;
                Console.WriteLine("Wert: " + result);
                return result;
                
                // markiere alles als Bud
                // return true;
            });

            r.DebugLayers.Add(r.Value);
            return r;
		}
        
		private static ObjectLayer createLayer(Bitmap bitmap, int threshold, string name){
			var map=new Map(bitmap.Width,bitmap.Height);
			using(var gsp=new SharpAccessory.Imaging.Filters.ColorDeconvolution().Get2ndStain(bitmap,SharpAccessory.Imaging.Filters.ColorDeconvolution.KnownStain.HaematoxylinDAB)){
				gsp.WriteBack=false;
				foreach(var grayscalePixel in gsp.Pixels()) map[grayscalePixel.X,grayscalePixel.Y]=grayscalePixel.V>threshold?1u:0u;
			}
			var layer=new ConnectedComponentCollector().Execute(map);
			layer.Name=name;
			return layer;
		}
	}
}
