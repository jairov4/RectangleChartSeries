using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using ReactiveUI;
using TReal = System.Single;

namespace PipeView
{
	public class MainWindow2 : MainWindow
	{
		protected override void CreateSeries(DataStream dataset)
		{
			var xI = dataset.FieldNames.IndexOf("x");
			var yI = dataset.FieldNames.IndexOf("y");
			var wI = dataset.FieldNames.IndexOf("w");
			var hI = dataset.FieldNames.IndexOf("h");
			
			var nameIndices = Enumerable.Range(0, dataset.FieldNames.Count).Except(new[] { xI, yI, wI, hI }).ToArray();
			var names = nameIndices.Select(i => dataset.FieldNames[i]).ToArray();
			var types = nameIndices.Select(i => dataset.FieldTypes[i]).ToArray();

			// README Check series
			var series = new Series4(names, types);
			
			const int chunkSize = 10000;
			var x = new List<TReal>(chunkSize); var y = new List<TReal>(chunkSize);
			var h = new List<TReal>(chunkSize); var w = new List<TReal>(chunkSize);
			var atts = new List<object>(chunkSize*nameIndices.Length);

			VisualStateManager.GoToState(this, "Loading", true);
			
			dataset.Stream.ToObservable().Buffer(chunkSize).SubscribeOn(Scheduler.Default)
				.Subscribe(dataStreamValues =>
			{
				x.Clear(); y.Clear(); w.Clear(); h.Clear(); atts.Clear();
				foreach (var dataStreamValue in dataStreamValues)
				{
					atts.AddRange(from c in nameIndices select dataStreamValue.Values[c]);
					x.Add((TReal)dataStreamValue.Values[xI]);
					y.Add((TReal)dataStreamValue.Values[yI]);
					w.Add((TReal)dataStreamValue.Values[wI]);
					h.Add((TReal)dataStreamValue.Values[hI]);
				}
				series.Append(x, y, w, h, atts);
				UpdateStatus();
			}, () => VisualStateManager.GoToState(this, "Idle", true));

			// README Check adapter
			var adapter = new RectangleRenderableSeries3(series);
			chart.RenderableSeries.Add(adapter);
		}
		
	}
}