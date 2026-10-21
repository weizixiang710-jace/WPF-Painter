using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace _2026_WpfApp3
{
    public partial class MainWindow : Window
    {
        Color strokeColor = Colors.Black;
        Color fillColor = Colors.Transparent;
        int strokeThickness = 1;
        string shapeType = "Line";
        string actionType = "Draw";

        Point start;
        Point end;
        Polyline currentPolyline = null; // 用於紀錄目前正在畫的隨手畫線條

        public MainWindow()
        {
            InitializeComponent();
            StrokeColorPicker.SelectedColor = strokeColor;
            FillColorPicker.SelectedColor = fillColor;
        }

        private void ShapeButton_Checked(object sender, RoutedEventArgs e)
        {
            var targetShapeButton = sender as RadioButton;
            if (targetShapeButton != null && targetShapeButton.Tag != null)
            {
                shapeType = targetShapeButton.Tag.ToString();
                currentPolyline = null;
                DisplayStatus();
            }
        }

        private void StrokeColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (StrokeColorPicker.SelectedColor.HasValue)
            {
                strokeColor = StrokeColorPicker.SelectedColor.Value;
                DisplayStatus();
            }
        }

        private void FillColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (FillColorPicker.SelectedColor.HasValue)
            {
                fillColor = FillColorPicker.SelectedColor.Value;
                DisplayStatus();
            }
        }

        private void ThicknessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            strokeThickness = (int)ThicknessSlider.Value;
            DisplayStatus();
        }

        private void DisplayStatus()
        {
            if (ColorLabel != null)
                ColorLabel.Content = $"筆刷色彩:{strokeColor}   填充色彩:{fillColor}   筆刷粗細：{strokeThickness}";
            if (ShapeLabel != null)
                ShapeLabel.Content = $"目前模式：{shapeType}";
        }

        // 清空畫布按鈕
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            MyCanvas.Children.Clear();
            currentPolyline = null;
        }

        // 1. 滑鼠按下左鍵
        private void MyCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            start = e.GetPosition(MyCanvas);
            MyCanvas.CaptureMouse(); // 鎖定滑鼠焦點

            // 橡皮擦模式：點擊到的圖形直接刪除
            if (shapeType == "Eraser")
            {
                if (e.OriginalSource is Shape targetShape)
                {
                    MyCanvas.Children.Remove(targetShape);
                }
                return;
            }

            if (actionType == "Draw")
            {
                SolidColorBrush strokeBrush = new SolidColorBrush(strokeColor);
                SolidColorBrush fillBrush = new SolidColorBrush(fillColor);

                switch (shapeType)
                {
                    case "Line":
                        Line line = new Line
                        {
                            X1 = start.X,
                            Y1 = start.Y,
                            X2 = start.X,
                            Y2 = start.Y,
                            Stroke = strokeBrush,
                            StrokeThickness = strokeThickness
                        };
                        MyCanvas.Children.Add(line);
                        break;

                    case "Rectangle":
                        Rectangle rect = new Rectangle
                        {
                            Stroke = strokeBrush,
                            Fill = fillBrush,
                            StrokeThickness = strokeThickness,
                            Width = 0,
                            Height = 0
                        };
                        Canvas.SetLeft(rect, start.X);
                        Canvas.SetTop(rect, start.Y);
                        MyCanvas.Children.Add(rect);
                        break;

                    case "Ellipse":
                        Ellipse ellipse = new Ellipse
                        {
                            Stroke = strokeBrush,
                            Fill = fillBrush,
                            StrokeThickness = strokeThickness,
                            Width = 0,
                            Height = 0
                        };
                        Canvas.SetLeft(ellipse, start.X);
                        Canvas.SetTop(ellipse, start.Y);
                        MyCanvas.Children.Add(ellipse);
                        break;

                    case "Polyline": // 【隨手畫畫筆】按下滑鼠時建立一條新的 Polyline，並放入起點
                        currentPolyline = new Polyline
                        {
                            Stroke = strokeBrush,
                            StrokeThickness = strokeThickness,
                            StrokeLineJoin = PenLineJoin.Round, // 讓線條轉折處平滑
                            StrokeStartLineCap = PenLineCap.Round, // 線條開頭圓角
                            StrokeEndLineCap = PenLineCap.Round  // 線條結尾圓角
                        };
                        currentPolyline.Points.Add(start);
                        MyCanvas.Children.Add(currentPolyline);
                        break;
                }
            }
        }

        // 2. 滑鼠移動
        private void MyCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            end = e.GetPosition(MyCanvas);
            if (StatusLabel != null)
            {
                StatusLabel.Content = $"座標：({(int)end.X}, {(int)end.Y})";
            }

            // 橡皮擦模式：拖曳劃過時連續擦除圖形
            if (shapeType == "Eraser" && e.LeftButton == MouseButtonState.Pressed)
            {
                if (e.OriginalSource is Shape targetShape)
                {
                    MyCanvas.Children.Remove(targetShape);
                }
                return;
            }

            if (actionType == "Draw" && e.LeftButton == MouseButtonState.Pressed)
            {
                switch (shapeType)
                {
                    case "Line":
                        var line = MyCanvas.Children.OfType<Line>().LastOrDefault();
                        if (line != null)
                        {
                            line.X2 = end.X;
                            line.Y2 = end.Y;
                        }
                        break;

                    case "Rectangle":
                        var rect = MyCanvas.Children.OfType<Rectangle>().LastOrDefault();
                        if (rect != null)
                        {
                            Canvas.SetLeft(rect, Math.Min(start.X, end.X));
                            Canvas.SetTop(rect, Math.Min(start.Y, end.Y));
                            rect.Width = Math.Abs(end.X - start.X);
                            rect.Height = Math.Abs(end.Y - start.Y);
                        }
                        break;

                    case "Ellipse":
                        var ellipse = MyCanvas.Children.OfType<Ellipse>().LastOrDefault();
                        if (ellipse != null)
                        {
                            Canvas.SetLeft(ellipse, Math.Min(start.X, end.X));
                            Canvas.SetTop(ellipse, Math.Min(start.Y, end.Y));
                            ellipse.Width = Math.Abs(end.X - start.X);
                            ellipse.Height = Math.Abs(end.Y - start.Y);
                        }
                        break;

                    case "Polyline": // 【隨手畫畫筆】按住左鍵移動時，持續將滑鼠軌跡點加入 Polyline
                        if (currentPolyline != null)
                        {
                            currentPolyline.Points.Add(end);
                        }
                        break;
                }
            }
        }

        // 3. 放開滑鼠左鍵
        private void MyCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            currentPolyline = null; // 結束目前筆畫
            MyCanvas.ReleaseMouseCapture();
        }

        // 右鍵（預留）
        private void MyCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
        }
    }
}