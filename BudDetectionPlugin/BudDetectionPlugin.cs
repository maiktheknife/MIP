/*
 * @author Sebastian Lohmann
 */
namespace BudDetectionPlugin{
	using System;
	using System.Windows.Forms;
    using BudDetection;
	using Glaukopis.CognitionMasterIntegration;
	using SharpAccessory.CognitionMaster.DefaultPlugins;
	using SharpAccessory.CognitionMaster.Plugging;
	using SharpAccessory.CognitionMaster.WholeSlideImageSupport;
    
	[PluginDefaultEnabled(true)]
	public class BudDetectionPlugin:GlaukopisPlugin{
		private ImageListBox deconvolutedImageBox;
		private NumericUpDown zoomNumericUpDown;

		protected override void OnLoad(EventArgs e){
			base.OnLoad(e);
			this.CreateTabContainer("BudDetection");
			this.TabContainer.Enabled=true;
			this.deconvolutedImageBox=new ImageListBox(()=>this.DisplayedImage,this.SetDisplayedImage){Parent=this.TabContainer,Height=100};
			(new Button{Parent=this.TabContainer, Text= "ColorDeconvolution", Dock=DockStyle.Top}).Click+=delegate {
				this.processResult(BudDetection.ColorDeconvolution.Execute(this.DisplayedImage));            
            };
            (new Button{Parent=this.TabContainer, Text = "BudDetection", Dock = DockStyle.Top }).Click+=delegate {
                this.processResult(BudDetection.Detector.Execute(this.DisplayedImage));
            };
            new Button{Parent=this.TabContainer, Text = "goto zoom", Dock =DockStyle.Top}.Click+=delegate {
                WsiInterop.Navigation.Goto((float)this.zoomNumericUpDown.Value);
            };

			this.zoomNumericUpDown=new NumericUpDown(){Parent=this.TabContainer,Dock=DockStyle.Top,Minimum=0,Maximum=1,Value=0.5M,DecimalPlaces=1,Increment=0.1M};
		}

		private void processResult<T>(IResult<T> result){
			this.deconvolutedImageBox.Init();
			foreach(var objectLayer in result.ObjectLayers) this.addLayer(objectLayer,true);
			foreach(var bitmap in result.Bitmaps) this.deconvolutedImageBox.Add(bitmap.Item1,bitmap.Item2);
			foreach(var variable in result.Variables){
				Debug.WriteLine(variable.Key+"="+variable.Value);
			}
		}
	}
}