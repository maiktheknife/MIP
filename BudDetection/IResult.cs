/*
 * @author Sebastian Lohmann
 */
namespace BudDetection {
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using SharpAccessory.Imaging.Segmentation;
	public interface IResult<out T> {
		T Value{get;}
		IList<Tuple<Bitmap,string>> Bitmaps{get;}
		IList<ObjectLayer> ObjectLayers{get;}
		IDictionary<string,double> Variables{get;}
	}

    // TODO hier das interface, impl für buddetetion

    // "zerschnittene" bud ignorieren
}
