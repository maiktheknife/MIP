/*
 * @author Sebastian Lohmann
 */
namespace BudDetection {
	using Glaukopis.SharpAccessoryIntegration;
	using SharpAccessory.Imaging.Classification.Features.Size;
	using SharpAccessory.Imaging.Segmentation;
	public static class AxesOfCorrespondingEllipse {
		public static void ProcessLayer(ObjectLayer layer){
			foreach(var io in layer.Objects){
				var correspondingEllipse=CorrespondingEllipse.FromContour(io.Contour);
				io.SetFeature("MajorAxisOfCorrespondingEllipse",new MajorAxisOfCorrespondingEllipse(correspondingEllipse).Value);
				io.SetFeature("MinorAxisOfCorrespondingEllipse",new MinorAxisOfCorrespondingEllipse(correspondingEllipse).Value);
			}
		}
	}
}
