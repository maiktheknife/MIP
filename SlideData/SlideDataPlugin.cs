/*
 * @author Sebastian Lohmann
 */
namespace Glaukopis.CognitionMasterPlugins{
	using System;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using CognitionMasterIntegration;
	using SlideProcessing;
	using SharpAccessory.CognitionMaster.Plugging;
	using SharpAccessory.CognitionMaster.WholeSlideImageSupport;
	using Debug=SharpAccessory.CognitionMaster.DefaultPlugins.Debug;
	[PluginDefaultEnabled(true)]
	public class SlideDataPlugin:GlaukopisPlugin {
		private SlideDataProvider slideDataProvider;
		private HeatMapDisplay heatMapDisplay;
		protected override void OnLoad(EventArgs e){
			base.OnLoad(e);
			this.CreateTabContainer("Slide Data");
			this.TabContainer.Enabled=true;
			this.heatMapDisplay=new HeatMapDisplay { Parent=this.TabContainer,Dock=DockStyle.Fill,SelectorIsVisible = true};
			new Button{Text="get layers",Parent=this.TabContainer,Dock=DockStyle.Top}.Click+=delegate{this.setLayers();};
			new Button{Text="get annotations",Parent=this.TabContainer,Dock=DockStyle.Top}.Click+=delegate{this.setAnnotationLayer();};
			new Button{Text="get heat maps",Parent=this.TabContainer,Dock=DockStyle.Top}.Click+=delegate{this.setHeatMapLayers();};
			new Button{Text="get all",Parent=this.TabContainer,Dock=DockStyle.Top}.Click+=delegate{this.setAllLayers();};
		}
		private async void setLayers(){
			if(null==this.slideDataProvider) return;
			await Task.Run(()=>this.addLayer(this.slideDataProvider.GetLayers(WsiInterop.Navigation),false));
		}
		private async void setAnnotationLayer(){
			if(null==this.slideDataProvider) return;
			await Task.Run(()=>this.addLayer(this.slideDataProvider.GetAnnotationLayers(WsiInterop.Navigation),true));
		}
		private async void setHeatMapLayers(){
			if(null==this.slideDataProvider) return;
			await Task.Run(()=>this.addLayer(this.slideDataProvider.GetHeatMapLayers(WsiInterop.Navigation),true));
		}
		private async void setAllLayers(){
			if(null==this.slideDataProvider) return;
			await Task.Run(()=> {
				this.addLayer(this.slideDataProvider.GetLayers(WsiInterop.Navigation),true);
				this.addLayer(this.slideDataProvider.GetAnnotationLayers(WsiInterop.Navigation),true);
				this.addLayer(this.slideDataProvider.GetHeatMapLayers(WsiInterop.Navigation),true);
			});
		}
		protected override void OnReady(EventArgs e){
			base.OnReady(e);
			this.heatMapDisplay.BindNavigation();
		}
		protected override void OnImageFileOpened(EventArgs e){
			base.OnImageFileOpened(e);
			var slide=this.ImageFile as IWholeSlideImageFile;
			if(null==slide) return;
			this.slideDataProvider?.Dispose();
			this.slideDataProvider=new SlideDataProvider(slide);
			foreach(var heatMap in this.slideDataProvider.HeatMapDescriptions) this.heatMapDisplay.AddImage(heatMap.Name,heatMap.FileName);
		}
		protected override void OnImageFileClosed(EventArgs e){
			base.OnImageFileClosed(e);
			this.slideDataProvider?.Dispose();
			this.slideDataProvider=null;
			this.heatMapDisplay.Reset();
		}
	}
}
