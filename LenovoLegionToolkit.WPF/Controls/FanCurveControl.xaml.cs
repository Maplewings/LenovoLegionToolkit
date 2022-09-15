﻿using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using LenovoLegionToolkit.Lib;
using LenovoLegionToolkit.Lib.Extensions;
using LenovoLegionToolkit.Lib.System;

namespace LenovoLegionToolkit.WPF.Controls
{
    public partial class FanCurveControl
    {
        private readonly List<Slider> _sliders = new();

        private FanTableData[]? _tableData;

        public FanCurveControl()
        {
            InitializeComponent();
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            var size = base.ArrangeOverride(arrangeBounds);

            DrawGraph();

            return size;
        }

        public void SetFanTableInfo(FanTableInfo fanTableInfo)
        {
            _sliders.Clear();
            _slidersGrid.Children.Clear();

            var tableValues = fanTableInfo.Table.GetTable();

            for (var i = 0; i < tableValues.Length; i++)
            {
                var slider = GenerateSlider(i, 0, 10);
                slider.Value = tableValues[i];
                _sliders.Add(slider);
                _slidersGrid.Children.Add(slider);
            }

            _tableData = fanTableInfo.Data;
        }

        public FanTableInfo? GetFanTableInfo()
        {
            if (_tableData is null)
                return null;

            var fanTable = _sliders.Select(s => (ushort)s.Value).ToArray();
            return new(_tableData, new FanTable(fanTable));
        }

        private void Slider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_sliders.Count < 2)
                return;

            if (sender is not Slider { IsMouseCaptureWithin: true } currentSlider)
                return;

            if (currentSlider.Value < 1)
            {
                currentSlider.Value = 1;
                return;
            }

            VerifyValues(currentSlider);
            DrawGraph();
        }

        private void VerifyValues(Slider currentSlider)
        {
            var currentIndex = _sliders.IndexOf(currentSlider);
            if (currentIndex < 0)
                return;

            var currentValue = currentSlider.Value;
            var slidersBefore = _sliders.Take(currentIndex);
            var slidersAfter = _sliders.Skip(currentIndex + 1);

            foreach (var slider in slidersBefore)
            {
                if (slider.Value > currentValue)
                    slider.Value = currentValue;
            }

            foreach (var slider in slidersAfter)
            {
                if (slider.Value < currentValue)
                    slider.Value = currentValue;
            }
        }

        private void DrawGraph()
        {
            var color = (SolidColorBrush)Application.Current.Resources["ControlFillColorDefaultBrush"];

            _canvas.Children.Clear();

            var points = _sliders
                .Select(GetThumbLocation)
                .Select(p => new Point(p.X, p.Y))
                .ToArray();

            if (points.IsEmpty())
                return;

            // Line

            var pathSegmentCollection = new PathSegmentCollection();
            foreach (var point in points.Skip(1))
                pathSegmentCollection.Add(new LineSegment { Point = point });
            var pathFigure = new PathFigure { StartPoint = points[0], Segments = pathSegmentCollection };

            var path = new Path
            {
                StrokeThickness = 2,
                Stroke = color,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                Data = new PathGeometry { Figures = new PathFigureCollection { pathFigure } },
            };
            _canvas.Children.Add(path);

            // Fill

            var pointCollection = new PointCollection { new(points[0].X, _canvas.ActualHeight) };
            foreach (var point in points)
                pointCollection.Add(point);
            pointCollection.Add(new(points[^1].X, _canvas.ActualHeight));

            var polygon = new Polygon
            {
                Fill = color,
                Points = pointCollection
            };
            _canvas.Children.Add(polygon);
        }

        private Slider GenerateSlider(int index, int minimum, int maximum)
        {
            var slider = new Slider
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Orientation = Orientation.Vertical,
                IsSnapToTickEnabled = true,
                TickFrequency = 1,
                Maximum = maximum,
                Minimum = minimum,
                Tag = index
            };
            slider.ValueChanged += Slider_OnValueChanged;
            Grid.SetColumn(slider, index + 1);
            return slider;
        }

        private Point GetThumbLocation(Slider slider)
        {
            var ratio = slider.Value / (slider.Maximum - slider.Minimum);
            var y = slider.ActualHeight - (slider.ActualHeight * ratio);
            var x = slider.ActualWidth * 0.5;
            var point = slider.TranslatePoint(new(x, y), _canvas);
            return point;
        }
    }
}
