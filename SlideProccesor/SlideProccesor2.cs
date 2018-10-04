/*
 * @author Sebastian Lohmann
 */
/*
 * @author Sebastian Lohmann
 */
namespace SlideProccesor
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using Glaukopis.Core.Analysis;
    using Glaukopis.SharpAccessoryIntegration;
    using Glaukopis.SlideProcessing;
    using VMscope.InteropCore.VirtualMicroscopy;
    internal class BudDetectionProccesor
    {
        private static void Main(string[] args)
        {
            foreach (var slideName in Util.GetSlideFilenames(args))
            {
                using (var slideCache = new SlideCache(slideName))
                {
                    var scale = 0.5f;
                    var tileSize = 500;
                    var slidePartitionerFileName = slideCache.DataPath + "BudDetection.Buds.xml";
                    var slidePartitioner = File.Exists(slidePartitionerFileName) ? SlidePartitioner<int?>.Load(slidePartitionerFileName) : new SlidePartitioner<int?>(slideCache.Slide, scale, new Size(tileSize, tileSize));

                    var heatMapHelper = new HeatMapHelper(slideCache.GetImage("tissueHeatMap"));
                    var budCountRange = new Range<int>();

                    foreach (var tile in slidePartitioner.Values)
                    {
                        if (tile.Data.HasValue)
                        {
                            Console.WriteLine(slideCache.SlideName + "-" + tile.Index + ":" + tile.Data.Value + " skipped");
                            budCountRange.Add(tile.Data.Value);
                            continue;
                        }

                        var colors = heatMapHelper.GetAffectedColors(slideCache.Slide.Size, tile.SourceArea);
                        var values = heatMapHelper.GetAffectedValues(slideCache.Slide.Size, tile.SourceArea);
                        if (values.All(v => 255 == v))
                        {
                            Console.WriteLine(slideCache.SlideName + "-" + tile.Index + ": no tissue");
                            continue;
                        }

                        using (var tileImage = slideCache.Slide.GetImagePart(tile))
                        {
                            if (false)
                            {
                                var tileCache = slideCache.GetTileCache(tile.Index);
                                tileCache.SetImage("rgb", tileImage);//speichert unter C:\ProgramData\processingRepository\[slideCache.SlideName]\[Index]\... ansonsten tileImage.Save("[uri].png")
                            }
                            var r = BudDetection.Detector.Execute(tileImage);
                            
                            var layer = r.Value;
                            tile.Data = layer.Objects.Count;
                            budCountRange.Add(layer.Objects.Count);
                            foreach (var io in layer.Objects)
                            {
                                //var bb=io.Contour.FindBoundingBox();
                                var offset = tile.SourceArea.Location;
                                var sourcePoints = new List<Point>();
                                foreach (var contourPoint in io.Contour.GetPoints())
                                {
                                    double x = Math.Round(contourPoint.X / scale + offset.X);
                                    double y = Math.Round(contourPoint.Y / scale + offset.Y);
                                    var sourcePoint = new Point((int)x, (int)y);
                                    sourcePoints.Add(sourcePoint);
                                }
                                var annotation = slideCache.Slide.CreateAnnotation(AnnotationType.PolygonLine);
                                foreach (var point in sourcePoints)
                                {
                                    annotation.AppendPoints(point);
                                }
                                annotation.Color = Color.OrangeRed;
                                annotation.Name = null == io.Class ? "bud" : io.Class.Name;
                            }
                        }
                        Console.WriteLine(slideCache.SlideName + "-" + tile.Index + " done");
                        if (Console.KeyAvailable) break;
                    }
                    slidePartitioner.Save(slidePartitionerFileName);
                    using (var heatMap = slidePartitioner.GenerateHeatMap(b => {
                        if (!b.HasValue) return Color.LightSeaGreen;
                        var c = (int)Math.Round(budCountRange.Normalize(b.Value) * 255d);
                        return Color.FromArgb(c, c, c);
                    }))
                        slideCache.SetImage("budHeatMap", heatMap);
                }
            }
        }
    }
}

/*
 * if(false){
								var tileCache=slideCache.GetTileCache(tile.Index);
								tileCache.SetImage("rgb",tileImage);//speichert unter C:\ProgramData\processingRepository\[slideCache.SlideName]\[Index]\... ansonsten tileImage.Save("[uri].png")
							}



    Bud tips:
    PIXEL COUNT!
    Kontur Rund!
    Elipse um Bud legen

*/
