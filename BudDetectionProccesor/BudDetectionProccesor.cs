/*
 * @author Sebastian Lohmann
 */
namespace BudDetectionProccesor {
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.IO;
	using System.Linq;
	using Glaukopis.Core.Analysis;
	using Glaukopis.SharpAccessoryIntegration;
	using Glaukopis.SlideProcessing;
	using VMscope.InteropCore.VirtualMicroscopy;
	internal class BudDetectionProccesor {
		private static void Main(string[] args){
			foreach(var slideName in Util.GetSlideFilenames(args)){
				using(var slideCache=new SlideCache(slideName)){
					// var scale = 0.1f; 
                    var scale = 0.5f; // besser so laut 19.07.2018
                    var tileSize = 100;
					var slidePartitionerFileName=slideCache.DataPath+"BudDetection.Buds.xml";
					var slidePartitioner=File.Exists(slidePartitionerFileName)?SlidePartitioner<int?>.Load(slidePartitionerFileName):new SlidePartitioner<int?>(slideCache.Slide,scale,new Size(tileSize,tileSize));

					var heatMapHelper=new HeatMapHelper(slideCache.GetImage("tissueHeatMap"));
					var budCountRange=new Range<int>();
                    var counter = 0;

                    foreach (var tile in slidePartitioner.Values){

                        // test, ob schon was für die kackel berechnet wurde
						if(tile.Data.HasValue){
							Console.WriteLine(slideCache.SlideName+"-"+tile.Index+":"+tile.Data.Value+" skipped");
							budCountRange.Add(tile.Data.Value);
							continue;
						}

                        // soll kachel bearbeitet werden
						var colors=heatMapHelper.GetAffectedColors(slideCache.Slide.Size,tile.SourceArea);
						var values=heatMapHelper.GetAffectedValues(slideCache.Slide.Size,tile.SourceArea);
						if(values.All(v=> 255==v)){ // 255 weiß
							Console.WriteLine(slideCache.SlideName+"-"+tile.Index+": no tissue");
							continue;
						}

						using(var tileImage=slideCache.Slide.GetImagePart(tile)){
                            counter = counter + 1;

                            if (false){
								var tileCache=slideCache.GetTileCache(tile.Index);
								tileCache.SetImage("rgb",tileImage);//speichert unter C:\ProgramData\processingRepository\[slideCache.SlideName]\[Index]\... ansonsten tileImage.Save("[uri].png")
							}

							var r = BudDetection.Detector.Execute(tileImage); // importent call 
							var layer = r.Value;

							tile.Data=layer.Objects.Count;
							budCountRange.Add(layer.Objects.Count);

                            // für alle gefundene objekte wird eine annotation angelegt
							foreach(var io in layer.Objects){
								//var bb=io.Contour.FindBoundingBox();
								var offset=tile.SourceArea.Location;
								var sourcePoints=new List<Point>();
								foreach(var contourPoint in io.Contour.GetPoints()){
									double x=Math.Round(contourPoint.X/scale+offset.X);
									double y=Math.Round(contourPoint.Y/scale+offset.Y);
									var sourcePoint=new Point((int)x,(int)y);
									sourcePoints.Add(sourcePoint);
								}
								var annotation=slideCache.Slide.CreateAnnotation(AnnotationType.PolygonLine);
								foreach(var point in sourcePoints){
									annotation.AppendPoints(point);
								}
								annotation.Color=Color.PaleVioletRed;
								annotation.Name=null==io.Class?"bud":io.Class.Name;
							}
						}
                        if (counter % 100 == 0)
                        {
                            Console.WriteLine(slideCache.SlideName + "-" + tile.Index + " done");
                        }
                        
						if(Console.KeyAvailable) break;
					}

                    slidePartitioner.Save(slidePartitionerFileName);

                    // normalisierung
					using(var heatMap = slidePartitioner.GenerateHeatMap(b => {
						if(!b.HasValue) return Color.LightSeaGreen;
						var c = (int) Math.Round(budCountRange.Normalize(b.Value) * 255d);
						return Color.FromArgb(c,c,c); // grauwert(hell == viele butts)
					}))

					slideCache.SetImage("budHeatMap",heatMap);
				}
			}
		}
	}
}