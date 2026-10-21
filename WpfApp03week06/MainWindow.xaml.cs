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
        Polyline currentPolyline = null; // 用於紀錄目前正在繪製的折線

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
                currentPolyline = null; // 切換圖形時重置折線狀態
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
                ShapeLabel.Content = $"形狀：{shapeType}";
        }

        // 1. 按下滑鼠左鍵時的邏輯
        private void MyCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            start = e.GetPosition(MyCanvas);
            MyCanvas.CaptureMouse(); // 鎖定滑鼠焦點

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

                    case "Polyline":
                        // 若尚未開始繪製這條折線，建立新的 Polyline
                        if (currentPolyline == null)
                        {
                            currentPolyline = new Polyline
                            {
                                Stroke = strokeBrush,
                                StrokeThickness = strokeThickness
                            };
                            currentPolyline.Points.Add(start); // 第一點
                            currentPolyline.Points.Add(start); // 移動中的即時預覽點
                            MyCanvas.Children.Add(currentPolyline);
                        }
                        else
                        {
                            // 已經在繪製中，每次點擊加入一個新站點
                            currentPolyline.Points.Add(start);
                        }
                        break;
                }
            }
        }

        // 2. 移動滑鼠時的即時拉伸效果
        private void MyCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            end = e.GetPosition(MyCanvas);
            if (StatusLabel != null)
            {
                StatusLabel.Content = $"座標：({(int)end.X}, {(int)end.Y})";
            }

            if (actionType == "Draw")
            {
                // Polyline 無需按住左鍵即可移動預覽
                if (shapeType == "Polyline" && currentPolyline != null)
                {
                    currentPolyline.Points[currentPolyline.Points.Count - 1] = end;
                    return;
                }

                // 其他圖形需按住左鍵拖曳
                if (e.LeftButton == MouseButtonState.Pressed)
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
                    }
                }
            }
        }

        // 3. 放開滑鼠左鍵（Polyline可透過右鍵結束）
        private void MyCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (shapeType != "Polyline")
            {
                MyCanvas.ReleaseMouseCapture();
            }
        }

        // 滑鼠右鍵點擊：結束 Polyline 的繪製
        private void MyCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (shapeType == "Polyline" && currentPolyline != null)
            {
                currentPolyline = null; // 清除指標，下次點擊將開起新折線
                MyCanvas.ReleaseMouseCapture();
            }
        }
    }
}