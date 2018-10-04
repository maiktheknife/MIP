/*
 * @author Sebastian Lohmann
 */
namespace DeconvolutionPlugin{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Windows.Forms;
	using Glaukopis.CognitionMasterIntegration;
	using Glaukopis.SharpAccessoryIntegration;
	using SharpAccessory.CognitionMaster.Plugging;
	using SharpAccessory.CognitionMaster.WholeSlideImageSupport;
	using SharpAccessory.Imaging.Filters;
	using SharpAccessory.Imaging.Processors;
	using SharpAccessory.Imaging.Segmentation;
	[PluginDefaultEnabled(true)]
	public class DeconvolutionPlugin:GlaukopisPlugin{
		private ImageListBox deconvolutedImageBox;
		private NumericUpDown zoomNumericUpDown;
		protected override void OnLoad(EventArgs e){
			base.OnLoad(e);
			this.CreateTabContainer("Deconvolution");
			this.TabContainer.Enabled=true;
			this.deconvolutedImageBox=new ImageListBox(()=>this.DisplayedImage,this.SetDisplayedImage){Parent=this.TabContainer,Height=100};
			(new Button{Parent=this.TabContainer,Text="deconvolute",Dock=DockStyle.Top}).Click+=delegate{
				this.deconvolutedImageBox.Init();
				foreach(var tuple in this.process()){
					var map=new Map(tuple.Item1.Width,tuple.Item1.Height);
					using(var gsp=new GrayscaleProcessor(tuple.Item1,RgbToGrayscaleConversion.JustReds)){
						gsp.WriteBack=false;
						foreach(var grayscalePixel in gsp.Pixels()) map[grayscalePixel.X,grayscalePixel.Y]=grayscalePixel.V>tuple.Item3?1u:0u;
					}
					var layer=new ConnectedComponentCollector().Execute(map);
					layer.Name=tuple.Item2;
					this.addLayer(layer,true);
					this.deconvolutedImageBox.Add(tuple.Item1,tuple.Item2);
				}
			};
			new Button{Text="goto zoom",Parent=this.TabContainer,Dock=DockStyle.Top}.Click+=delegate{WsiInterop.Navigation.Goto((float)this.zoomNumericUpDown.Value);};
			this.zoomNumericUpDown=new NumericUpDown(){Parent=this.TabContainer,Dock=DockStyle.Top,Minimum=0,Maximum=1,Value=0.5M,DecimalPlaces=1,Increment=0.1M};
		}
		private IEnumerable<Tuple<Bitmap,string,int>> process(){
			using(var source=this.DisplayedImage.Clone() as Bitmap){
				var gspR=new GrayscaleProcessor(source.Clone() as Bitmap,RgbToGrayscaleConversion.JustReds);
				gspR.Dispose();
				yield return Tuple.Create(gspR.Bitmap,"Red",128);
				var gpDAB=new ColorDeconvolution().Get2ndStain(source,ColorDeconvolution.KnownStain.HaematoxylinDAB);
				gpDAB.Dispose();
				yield return Tuple.Create(gpDAB.Bitmap,"DAB",128);
			}
		}
	}
}