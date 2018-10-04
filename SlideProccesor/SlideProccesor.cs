/*
 * @author Sebastian Lohmann
 */
namespace SlideProccesor {
	using System;
	using System.Drawing;
	using System.IO;
	using Glaukopis.SlideProcessing;
	internal class SlideProccesor {
		private static void Main(string[] args){
            Console.WriteLine("Start SlideProzessor");
            Console.WriteLine(args);

            foreach (var slideName in Util.GetSlideFilenames(args)){
                Console.WriteLine("progress: " + slideName);
                
                using (var slideCache=new SlideCache(slideName)){
                    // Skalierung des Bildes
                    var scale = 0.5f; // 0.1
					var tileSize = 500; // 100

                    // Partionierung und speichern
                    var slidePartitionerFileName =slideCache.DataPath+"BudDetection.Tissue.xml";
					var slidePartitioner=File.Exists(slidePartitionerFileName)?SlidePartitioner<bool?>.Load(slidePartitionerFileName):new SlidePartitioner<bool?>(slideCache.Slide,scale,new Size(tileSize,tileSize));
					using(var overViewImage=slideCache.Slide.GetImagePart(0,0,slideCache.Slide.Size.Width,slideCache.Slide.Size.Height,slidePartitioner.Columns,slidePartitioner.Rows)){
						slideCache.SetImage("overview",overViewImage);//speichert unter C:\ProgramData\processingRepository\[slideCache.SlideName]\...
						//ggf. heatmap hier erstellen
					}

                    // über alle Kacheln
                    var counter = 0;
					foreach(var tile in slidePartitioner.Values){
                        counter = counter + 1;

                        // ist bereits ein wert vorhanden, dann überspringen
                        if (tile.Data.HasValue){
                            if (counter % 100 == 0) {
                                Console.WriteLine(slideCache.SlideName + "-" + tile.Index + ":" + tile.Data.Value + " skipped");
                            }
							continue;                            
                        }

                        // bitmap erzeugen aus der Kachel
                        using (var tileImage=slideCache.Slide.GetImagePart(tile)){

                            // für debugging zwecke
                            if (false){
								var tileCache=slideCache.GetTileCache(tile.Index);
								tileCache.SetImage("rgb",tileImage);//speichert unter C:\ProgramData\processingRepository\[slideCache.SlideName]\[Index]\... ansonsten tileImage.Save("[uri].png")
							}

                            // Wert Berechnung
                            var r =BudDetection.ColorDeconvolution.Execute(tileImage);
							tile.Data=r.Value; //Wert zur Kachel speichern
						}

                        if (counter % 100 == 0) {
                            Console.WriteLine(slideCache.SlideName + "-" + tile.Index + " done");
                        }

                        // unterbrechbar via key eingabe
                        if (Console.KeyAvailable) break;
					}

                    // zustand der bearbeitng speichern
                    slidePartitioner.Save(slidePartitionerFileName);
					using(var heatMap=slidePartitioner.GenerateHeatMap(b=>b.HasValue?(b.Value?Color.Green:Color.White):Color.Black)) slideCache.SetImage("tissueHeatMap",heatMap);
				}
			}
		}
	}
}