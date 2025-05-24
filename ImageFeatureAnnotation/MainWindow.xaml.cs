using ImageFeatureAnnotation.Domain;
using ImageFeatureAnnotation.Properties;
using MaterialDesignColors;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using Color = System.Drawing.Color;
using Pen = System.Drawing.Pen;
using Point = System.Drawing.Point;

namespace ImageFeatureAnnotation
{
    /// <summary>
    /// Interaction logic for MainWindoRWidth.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ThemeSettingsViewModel? tsv;
        private int themeId;
        private bool isDarkTheme;
        private List<Swatch> Swatches;
        private MainWindowViewModel? mwv;
        private string? curDir = "";
        private Bitmap? sourceBitmap;
        private Bitmap? OrignalBitmap;
        private bool MousePress;
        private System.Drawing.Pen gPen = new System.Drawing.Pen(System.Drawing.Color.Maroon, 1f);
        private System.Drawing.Pen rPen = new System.Drawing.Pen(System.Drawing.Color.Red, 2f);
        private string FileName = "";
        private bool drawPoly;
        private bool hovarSP;
        private List<System.Drawing.Point>? curPS;
        private bool close;
        public int[,] Map;
        private static object _object = new object();
        private StringFormat sf = new StringFormat();
        private int margin = 40;
        private float rate = 15f;
        private float wratio = 1f;
        private float hratio = 1f;
        private StringBuilder sb = new StringBuilder();
        private int MaxF = 20;
        private Bitmap? displayBitmap;
        private bool closed;
        private int HoverF = -1;
        private int SelectedF = -1;
        private int HoverFP = -1;
        private int mousePx = -1;
        private int mousePy = -1;

        public MainWindow()
        {
            this.InitializeComponent();
            this.tsv = new ThemeSettingsViewModel();
            this.Swatches = this.tsv.Swatches.ToList<Swatch>();
            this.Closing += new CancelEventHandler(this.MainWindow_Closing);
            this.Loaded += new RoutedEventHandler(this.MainWindow_Loaded);
            this.themeId = Settings.Default.ThemeId;
            this.isDarkTheme = Settings.Default.DarkTheme;
            this.Swatches = this.tsv.Swatches.ToList<Swatch>();
            this.tsv.ApplyPrimaryCommand.Execute((object)this.Swatches[this.themeId]);
            this.tsv.IsDarkTheme = this.isDarkTheme;
            colors[0] = this.Swatches[this.themeId].PrimaryHues[7].Color;
            this.DataContext = (object)new MainWindowViewModel();
            this.mwv = (MainWindowViewModel)this.DataContext;
            this.AddHotKeys();
            try
            {
                if ((double)Settings.Default.w != 0.0)
                    this.RWidth.Value = new double?((double)Settings.Default.w);
                if ((double)Settings.Default.h != 0.0)
                    this.RHeight.Value = new double?((double)Settings.Default.h);
            }
            catch
            {
            }
            this.curDir = Settings.Default.InitDir;
            this.SaveD.IsChecked = new bool?(Settings.Default.IsSaveDir);
            this.SaveSubD.IsChecked = new bool?(!Settings.Default.IsSaveDir);
            this.SketchImg.IsChecked = new bool?(Settings.Default.IsSketchImg);
            this.mwv.FeatureList = new ObservableCollection<SelectableFeature>();
            this.TXT.IsChecked = new bool?(Settings.Default.Save2Txt);
            this.JSON.IsChecked = new bool?(!Settings.Default.Save2Txt);
            this.IsAutoSave.IsChecked = new bool?(Settings.Default.AutoSave);
            if (!(this.curDir != ""))
                return;
            this.OpenFolder();
        }

        private System.Drawing.Color GetAlphaColor(System.Drawing.Color color, int alpha) => System.Drawing.Color.FromArgb(alpha, color);

        private void MainWindow_Loaded(object sender, RoutedEventArgs e) => this.DataGrid_SelectionChanged((object)null, (SelectionChangedEventArgs)null);

        private void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            Settings.Default.ThemeId = this.themeId;
            Settings.Default.InitDir = this.curDir;
            Settings.Default.IsSaveDir = this.SaveD.IsChecked.Value;
            Settings.Default.w = (float)(int)this.RWidth.Value.Value;
            Settings.Default.h = (float)(int)this.RHeight.Value.Value;
            Settings.Default.AutoSave = this.IsAutoSave.IsChecked.Value;
            Settings.Default.Save2Txt = this.TXT.IsChecked.Value;
            Settings.Default.IsSketchImg = this.SketchImg.IsChecked.Value;
            Settings.Default.Save();
            GC.Collect();
            System.Windows.Application.Current.Shutdown();
            Environment.Exit(0);
        }

        private void AddHotKeys()
        {
            try
            {
                RoutedCommand Settings1 = new RoutedCommand();
                Settings1.InputGestures.Add(new KeyGesture(Key.Q, ModifierKeys.Control));
                CommandBindings.Add(new CommandBinding(Settings1, My_first_event_handler));

                RoutedCommand Settings2 = new RoutedCommand();
                Settings2.InputGestures.Add(new KeyGesture(Key.A, ModifierKeys.Control));
                CommandBindings.Add(new CommandBinding(Settings2, My_second_event_handler));
                RoutedCommand Settings3 = new RoutedCommand();
                Settings3.InputGestures.Add(new KeyGesture(Key.D, ModifierKeys.Control));
                CommandBindings.Add(new CommandBinding(Settings3, My_third_event_handler));
                RoutedCommand Settings6 = new RoutedCommand();
                //Settings6.InputGestures.Add(new KeyGesture(Key.M, ModifierKeys.Control));
                //CommandBindings.Add(new CommandBinding(Settings6, My_event_handler6));

            }
            catch (Exception err)
            {
                throw new Exception(err.ToString());
            }
        }
        private void My_first_event_handler(object sender, ExecutedRoutedEventArgs e)
        {
            themeId++;
            themeId = themeId >= Swatches.Count ? 0 : themeId;
            tsv?.ApplyPrimaryCommand.Execute(Swatches[themeId]);
            Properties.Settings.Default.ThemeId = themeId;
            Properties.Settings.Default.Save();
            //var mvm = DataContext as MainWindowViewModel;
            //Home? c = mvm?.MenuItems[0].Content as Home;
            //home = c;
            ////home?.Render();
        }

        private void My_second_event_handler(object sender, ExecutedRoutedEventArgs e)
        {
            themeId--;
            themeId = themeId < 0 ? Swatches.Count - 1 : themeId;
            tsv?.ApplyPrimaryCommand.Execute(Swatches[themeId]);
            Properties.Settings.Default.ThemeId = themeId;
            Properties.Settings.Default.Save();
        }
        private void My_third_event_handler(object sender, ExecutedRoutedEventArgs e)
        {
            if (isDarkTheme == true)
                isDarkTheme = false;
            else
                isDarkTheme = true;
            tsv!.IsDarkTheme = isDarkTheme;
            Properties.Settings.Default.DarkTheme = tsv!.IsDarkTheme;
            Properties.Settings.Default.Save();
        }
        public static System.Windows.Media.Color[]? colors { get; set; } = new System.Windows.Media.Color[3]
        {
      (System.Windows.Media.Color) System.Windows.Media.ColorConverter.ConvertFromString("#FF3F51B5"),
      (System.Windows.Media.Color) System.Windows.Media.ColorConverter.ConvertFromString("#FF3A7E00"),
      (System.Windows.Media.Color) System.Windows.Media.ColorConverter.ConvertFromString("#FFB00020")
        };

        private void BRect_Checked(object sender, RoutedEventArgs e)
        {
            if (this.BRect.IsChecked.GetValueOrDefault())
            {
                this.RectPanel.Visibility = Visibility.Visible;
            }
            else
            {
                this.RectPanel.Visibility = Visibility.Hidden;
                if (!this.BPolygon.IsChecked.GetValueOrDefault() || this.SelectedF < 0)
                    return;
                if (this.mwv.FeatureList[this.SelectedF].Shape == 0)
                    this.SelectedF = -1;
                this.DrawF();
            }
        }

        public static ObservableCollection<SelectableFiles> FileList { set; get; } = new ObservableCollection<SelectableFiles>();

        private ObservableCollection<string> CatList { set; get; } = new ObservableCollection<string>();

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if (this.curDir == "")
                this.curDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            folderBrowserDialog.InitialDirectory = this.curDir;
            if (folderBrowserDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;
            this.sourceBitmap = (Bitmap)null;
            this.curDir = folderBrowserDialog.SelectedPath;
            this.OpenFolder();
        }

        private void OpenFolder()
        {
            string curDir = this.curDir;
            try
            {
                MainWindow.FileList.Clear();
                foreach (string file in Directory.GetFiles(curDir))
                {
                    string lower = new FileInfo(file).Extension.ToLower();
                    if (lower == ".jpg" || lower == ".bmp" || lower == ".png")
                    {
                        FileInfo fileInfo = new FileInfo(file);
                        MainWindow.FileList.Add(new SelectableFiles()
                        {
                            FileName = fileInfo.Name,
                            Directory = fileInfo.Directory.FullName
                        });
                    }
                }
                this.mwv.FileList = MainWindow.FileList;
                if (MainWindow.FileList.Count <= 0)
                    return;
                this.FileListGrid.SelectedIndex = 0;
                if (!this.TXT.IsChecked.GetValueOrDefault())
                    return;
                for (int index = 0; index < MainWindow.FileList.Count; ++index)
                {
                    string path1 = this.SaveD.IsChecked.GetValueOrDefault() ? this.curDir : this.curDir + "\\" + this.SavePath.Text;
                    if (Directory.Exists(path1))
                    {
                        string path2 = path1 + "\\" + MainWindow.FileList[index].FileName.Replace(new FileInfo(MainWindow.FileList[index].FileName).Extension.ToLower(), ".txt");
                        if (File.Exists(this.curDir + "\\" + MainWindow.FileList[index].FileName) && File.Exists(path2))
                            this.mwv.FileList[index].IsSelected = true;
                    }
                }
            }
            catch
            {
            }
        }

        private void DrawF(int x = 0, int y = 0)
        {
            if (this.sourceBitmap == null)
                return;
            lock (MainWindow._object)
            {
                Bitmap bm = new Bitmap((System.Drawing.Image)this.displayBitmap);
                using (Graphics graphics1 = Graphics.FromImage((System.Drawing.Image)bm))
                {
                    System.Drawing.Point fpoint;
                    if (this.drawPoly && x != 0 && y != 0)
                    {
                        if (this.curPS.Count == 0)
                        {
                            this.drawPoly = false;
                            return;
                        }
                        if (Math.Abs(x - this.curPS[0].X) < 15 && Math.Abs(y - this.curPS[0].Y) < 15 && this.curPS.Count > 2 && !this.closed)
                        {
                            fpoint = this.curPS[0];
                            x = fpoint.X;
                            fpoint = this.curPS[0];
                            y = fpoint.Y;
                            graphics1.FillEllipse((System.Drawing.Brush)new SolidBrush(System.Drawing.Color.Yellow), new Rectangle(x - 15, y - 15, 30, 30));
                            graphics1.DrawEllipse(new System.Drawing.Pen(System.Drawing.Color.Red, 1f), new Rectangle(x - 15, y - 15, 30, 30));
                            this.close = true;
                        }
                        else
                            this.close = false;
                        if (this.closed)
                        {
                            this.closed = false;
                            this.drawPoly = false;
                            this.curPS = (List<Point>)null;
                        }
                        else
                        {
                            Point[] pointArray = new Point[this.curPS.Count + 1];
                            Array.Copy((Array)this.curPS.ToArray(), (Array)pointArray, this.curPS.Count);
                            pointArray[this.curPS.Count] = new Point(x, y);
                            graphics1.DrawLines(this.rPen, pointArray);
                            for (int index = 0; index < pointArray.Length; ++index)
                                graphics1.FillRectangle((System.Drawing.Brush)new SolidBrush(System.Drawing.Color.DarkRed), new RectangleF((float)(pointArray[index].X - 4), (float)(pointArray[index].Y - 4), 8f, 8f));
                        }
                    }
                    for (int index1 = 0; index1 < this.mwv.FeatureList.Count; ++index1)
                    {
                        List<Point> fpoints = this.mwv.FeatureList[index1].FPoints;
                        Graphics graphics2 = graphics1;
                        int? cat = this.mwv.FeatureList[index1].Cat;
                        int index2 = cat.Value % PrimaryColor.Length;
                        System.Drawing.Pen pen = new System.Drawing.Pen(PrimaryColor[index2], 3f);
                        Point[] array1 = this.mwv.FeatureList[index1].FPoints.ToArray();
                        graphics2.DrawPolygon(pen, array1);
                        for (int index3 = 0; index3 < this.mwv.FeatureList[index1].FPoints.Count; ++index3)
                        {
                            Graphics graphics3 = graphics1;
                            cat = this.mwv.FeatureList[index1].Cat;
                            int index4 = cat.Value % PrimaryColor.Length;
                            SolidBrush solidBrush = new SolidBrush(PrimaryColor[index4]);
                            fpoint = this.mwv.FeatureList[index1].FPoints[index3];
                            double x1 = (double)(fpoint.X - 4);
                            fpoint = this.mwv.FeatureList[index1].FPoints[index3];
                            double y1 = (double)(fpoint.Y - 4);
                            RectangleF rect = new RectangleF((float)x1, (float)y1, 8f, 8f);
                            graphics3.FillRectangle((System.Drawing.Brush)solidBrush, rect);
                        }
                        if (this.mwv.FeatureList[index1].Shape == 0)
                        {
                            Graphics graphics4 = graphics1;
                            cat = this.mwv.FeatureList[index1].Cat;
                            int index5 = cat.Value % PrimaryColor.Length;
                            SolidBrush solidBrush = new SolidBrush(PrimaryColor[index5]);
                            fpoint = this.mwv.FeatureList[index1].FPoints[0];
                            int x2 = fpoint.X;
                            fpoint = this.mwv.FeatureList[index1].FPoints[1];
                            int x3 = fpoint.X;
                            double x4 = (double)((x2 + x3) / 2 - 4);
                            fpoint = this.mwv.FeatureList[index1].FPoints[0];
                            int y2 = fpoint.Y;
                            fpoint = this.mwv.FeatureList[index1].FPoints[3];
                            int y3 = fpoint.Y;
                            double y4 = (double)((y2 + y3) / 2 - 4);
                            RectangleF rect = new RectangleF((float)x4, (float)y4, 8f, 8f);
                            graphics4.FillRectangle((System.Drawing.Brush)solidBrush, rect);
                        }
                        if (this.SelectedF == index1)
                        {
                            Graphics graphics5 = graphics1;
                            cat = this.mwv.FeatureList[index1].Cat;
                            int index6 = cat.Value % PrimaryColor.Length;
                            SolidBrush solidBrush = new SolidBrush(this.GetAlphaColor(PrimaryColor[index6], 100));
                            Point[] array2 = this.mwv.FeatureList[this.SelectedF].FPoints.ToArray();
                            graphics5.FillPolygon((System.Drawing.Brush)solidBrush, array2);
                        }
                        if (this.HoverF == index1)
                        {
                            if (this.HoverF != this.SelectedF)
                            {
                                Graphics graphics6 = graphics1;
                                cat = this.mwv.FeatureList[index1].Cat;
                                int index7 = cat.Value % PrimaryColor.Length;
                                SolidBrush solidBrush = new SolidBrush(this.GetAlphaColor(PrimaryColor[index7], 100));
                                Point[] array3 = this.mwv.FeatureList[this.HoverF].FPoints.ToArray();
                                graphics6.FillPolygon((System.Drawing.Brush)solidBrush, array3);
                            }
                            if (this.HoverFP >= 0)
                            {
                                Graphics graphics7 = graphics1;
                                SolidBrush solidBrush = new SolidBrush(System.Drawing.Color.Yellow);
                                fpoint = this.mwv.FeatureList[this.HoverF].FPoints[this.HoverFP];
                                double x5 = (double)(fpoint.X - 6);
                                fpoint = this.mwv.FeatureList[this.HoverF].FPoints[this.HoverFP];
                                double y5 = (double)(fpoint.Y - 6);
                                RectangleF rect = new RectangleF((float)x5, (float)y5, 12f, 12f);
                                graphics7.FillRectangle((System.Drawing.Brush)solidBrush, rect);
                                if (this.mwv.FeatureList[this.HoverF].Shape == 0)
                                {
                                    this.BRect.IsChecked = new bool?(true);
                                    this.RWidth.ValueChanged -= new RoutedPropertyChangedEventHandler<double?>(this.RWidth_ValueChanged);
                                    this.RHeight.ValueChanged -= new RoutedPropertyChangedEventHandler<double?>(this.RWidth_ValueChanged);
                                    MaterialDesignThemes.Wpf.NumericUpDown rwidth = this.RWidth;
                                    fpoint = this.mwv.FeatureList[this.HoverF].FPoints[0];
                                    int x6 = fpoint.X;
                                    fpoint = this.mwv.FeatureList[this.HoverF].FPoints[2];
                                    int x7 = fpoint.X;
                                    double? nullable1 = new double?((double)Math.Abs(x6 - x7));
                                    rwidth.Value = nullable1;
                                    MaterialDesignThemes.Wpf.NumericUpDown rheight = this.RHeight;
                                    fpoint = this.mwv.FeatureList[this.HoverF].FPoints[0];
                                    int y6 = fpoint.Y;
                                    fpoint = this.mwv.FeatureList[this.HoverF].FPoints[2];
                                    int y7 = fpoint.Y;
                                    double? nullable2 = new double?((double)Math.Abs(y6 - y7));
                                    rheight.Value = nullable2;
                                    this.RWidth.ValueChanged += new RoutedPropertyChangedEventHandler<double?>(this.RWidth_ValueChanged);
                                    this.RHeight.ValueChanged += new RoutedPropertyChangedEventHandler<double?>(this.RWidth_ValueChanged);
                                }
                            }
                        }
                    }
                }
                this.DrawToGraphics(bm);
            }
            GC.Collect();
        }

        private void DrawToGraphics(Bitmap bm)
        {
            lock (GWpf.Lock)
            {
                this.GWpf.GFX.SmoothingMode = SmoothingMode.AntiAlias;
                this.GWpf.GFX.SmoothingMode = SmoothingMode.HighQuality;
                this.GWpf.GFX.CompositingQuality = CompositingQuality.HighQuality;
                this.GWpf.GFX.PixelOffsetMode = PixelOffsetMode.HighQuality;
                Graphics gfx = this.GWpf.GFX;
                if (bm == null)
                    return;
                gfx.Clear(System.Drawing.Color.White);
                gfx.DrawImage((System.Drawing.Image)bm, 0, 0);
                this.GWpf.Paint();
            }
        }

        private void RWidth_ValueChanged(object sender, EventArgs e)
        {
            if (this.sourceBitmap == null || this.mwv.FeatureList == null || this.mwv.FeatureList.Count == 0 || this.SelectedF == -1)
                return;
            SelectableFeature feature = this.mwv.FeatureList[this.SelectedF];
            if (this.SelectedF >= 0 && feature.Shape == 0)
            {
                int num1 = Math.Min(feature.FPoints[0].X, feature.FPoints[2].X);
                int num2 = Math.Min(feature.FPoints[0].Y, feature.FPoints[2].Y);
                Point fpoint1 = feature.FPoints[0];
                int x1 = fpoint1.X;
                fpoint1 = feature.FPoints[2];
                int x2 = fpoint1.X;
                int num3 = Math.Max(x1, x2);
                Point fpoint2 = feature.FPoints[0];
                int y1 = fpoint2.Y;
                fpoint2 = feature.FPoints[2];
                int y2 = fpoint2.Y;
                int num4 = Math.Max(y1, y2);
                for (int index = 0; index < feature.FPoints.Count; ++index)
                {
                    Point fpoint3 = feature.FPoints[index];
                    double? nullable;
                    if (feature.FPoints[index].X == num3)
                    {
                        ref Point local = ref fpoint3;
                        int num5 = num1;
                        nullable = this.RWidth.Value;
                        int num6 = (int)nullable.Value;
                        int num7 = num5 + num6;
                        local.X = num7;
                    }
                    if (feature.FPoints[index].Y == num4)
                    {
                        ref Point local = ref fpoint3;
                        int num8 = num2;
                        nullable = this.RHeight.Value;
                        int num9 = (int)nullable.Value;
                        int num10 = num8 + num9;
                        local.Y = num10;
                    }
                    feature.FPoints[index] = fpoint3;
                }
            }
            this.DrawF();
            this.DrawMap();
        }

        private unsafe void DrawMap()
        {
            if (sourceBitmap == null)
                return;
            //sb.Clear();
            lock (_object)
            {
                Bitmap bm = new Bitmap(sourceBitmap.Width, sourceBitmap.Height);
                using (Graphics g = Graphics.FromImage(bm))
                {
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.Clear(Color.Black);
                    for (int i = 0; i < mwv!.FeatureList.Count; i++)
                    {
                        var s = mwv!.FeatureList[i].FPoints.ToArray();
                        int cv = (i + 1) * 10;
                        var cl = Color.FromArgb(255, cv, cv, cv);
                        g.FillPolygon(new SolidBrush(cl), s);
                        g.DrawPolygon(new Pen(cl, 2), s);
                    }
                }
                int height = bm.Height;
                int width = bm.Width;
                BitmapData sourceData = bm.LockBits(new Rectangle(0, 0,
                                         width, height),
                                                           ImageLockMode.ReadOnly,
                                                     bm.PixelFormat);
                byte* src = (byte*)sourceData.Scan0.ToPointer();
                int stride = sourceData.Stride;

                int channels = System.Drawing.Image.GetPixelFormatSize(bm.PixelFormat) / 8;
                int offset = stride - width * channels;
                int r = stride;
                int r2 = 2 * stride;
                int r3 = 3 * stride;
                int c = channels;
                int c2 = 2 * channels;
                int c3 = 3 * channels;
                int[] v = new int[33];
                Map = new int[height, width];
                try
                {
                    unsafe
                    {
                        fixed (int* dstPtr = Map)
                        {
                            int* dst = dstPtr;
                            for (int i = 0; i < height; i++)
                            {
                                for (int j = 0; j < width; j++, src += channels, dst++)
                                {
                                    byte* s = src;
                                    if ((s[0] > 0 || s[1] > 0 || s[2] > 0) && (i >= 3 && i < height - 3 && j >= 3 && j < width - 3))
                                    {
                                        //int cp=
                                        v[0] = s[-c3 - r3 + 1];
                                        v[1] = s[-c2 - r3 + 1];
                                        v[2] = s[-c - r3 + 1];
                                        v[3] = s[-r3 + 1];
                                        v[4] = s[c - r3 + 1];
                                        v[5] = s[c2 - r3 + 1];
                                        v[6] = s[c2 - r3 + 1];

                                        v[7] = s[-c3 - r2 + 1];
                                        v[8] = s[-c3 - r + 1];
                                        v[9] = s[-c3 + 1];
                                        v[10] = s[-c3 + r + 1];
                                        v[11] = s[-c3 + r2 + 1];

                                        v[12] = s[-r2 + 1];
                                        v[13] = s[-r + 1];
                                        v[14] = s[1];
                                        v[15] = s[r + 1];
                                        v[16] = s[r2 + 1];

                                        v[17] = s[-c2 + 1];
                                        v[18] = s[-c + 1];
                                        v[19] = s[c + 1];
                                        v[20] = s[c2 + 1];

                                        v[21] = s[c3 - r2 + 1];
                                        v[22] = s[c3 - r + 1];
                                        v[23] = s[c3 + 1];
                                        v[24] = s[c3 + r + 1];
                                        v[25] = s[c3 + r2 + 1];
                                        v[26] = s[-c3 + r3 + 1];
                                        v[27] = s[-c2 + r3 + 1];
                                        v[28] = s[-c + r3 + 1];
                                        v[29] = s[r3 + 1];
                                        v[30] = s[c + r3 + 1];
                                        v[31] = s[c2 + r3 + 1];
                                        v[32] = s[c3 + r3 + 1];
                                        *dst = v.Max() / 10;
                                    }
                                }
                                src += offset;
                            }
                        }
                    }
                    bm.UnlockBits(sourceData);
                }
                catch { }
            }
            GC.Collect();
        }

        private int GetPolyIndex(int x, int y)
        {
            if (this.Map == null || x <= 0 || y <= 0 || x >= this.sourceBitmap.Width - 1 || y >= this.sourceBitmap.Height - 1)
                return 0;
            int val2 = Math.Max(Math.Max(this.Map[y - 1, x], this.Map[y + 1, x]), Math.Max(this.Map[y, x + 1], this.Map[y, x - 1]));
            return Math.Max(this.Map[y, x], val2);
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.FileListGrid.SelectedIndex <= -1)
                return;
            try
            {
                this.FileName = this.mwv.FileList[this.FileListGrid.SelectedIndex].FileName;
                StreamReader streamReader1 = new StreamReader(this.curDir + "\\" + this.FileName);
                this.OrignalBitmap = (Bitmap)System.Drawing.Image.FromStream(streamReader1.BaseStream);
                streamReader1.Close();
                int width = this.GWpf.Width;
                int height = this.GWpf.Height;
                if (width < 0)
                    return;
                this.wratio = 1f;
                this.hratio = 1f;
                if (this.SketchImg.IsChecked.GetValueOrDefault())
                {
                    this.wratio = (float)this.OrignalBitmap.Width / (float)width;
                    this.hratio = (float)this.OrignalBitmap.Height / (float)height;
                    this.sourceBitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                    this.sourceBitmap.SetResolution(this.OrignalBitmap.HorizontalResolution, this.OrignalBitmap.VerticalResolution);
                    Graphics graphics = Graphics.FromImage((System.Drawing.Image)this.sourceBitmap);
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.DrawImage((System.Drawing.Image)this.OrignalBitmap, new Rectangle(0, 0, width, height));
                    graphics.Dispose();
                }
                else
                    this.sourceBitmap = this.OrignalBitmap;
                this.displayBitmap = MainWindow.BitmapAdjust(this.sourceBitmap, (float)this.Brightness.Value / 100f, (float)this.Contrast.Value / 100f);
                this.richtextBox1.Document.Blocks.Clear();
                System.Windows.Controls.RichTextBox richtextBox1_1 = this.richtextBox1;
                string[] strArray1 = new string[6]
                {
          "原图：",
          this.OrignalBitmap.Width.ToString(),
          " X ",
          this.OrignalBitmap.Height.ToString(),
          " X ",
          null
                };
                int num1 = System.Drawing.Image.GetPixelFormatSize(this.OrignalBitmap.PixelFormat) / 8;
                strArray1[5] = num1.ToString();
                string textData1 = string.Concat(strArray1);
                richtextBox1_1.AppendText(textData1);
                System.Windows.Controls.RichTextBox richtextBox1_2 = this.richtextBox1;
                string[] strArray2 = new string[6];
                strArray2[0] = "\r\n现图：";
                num1 = this.sourceBitmap.Width;
                strArray2[1] = num1.ToString();
                strArray2[2] = " X ";
                num1 = this.sourceBitmap.Height;
                strArray2[3] = num1.ToString();
                strArray2[4] = " X ";
                num1 = System.Drawing.Image.GetPixelFormatSize(this.sourceBitmap.PixelFormat) / 8;
                strArray2[5] = num1.ToString();
                string textData2 = string.Concat(strArray2);
                richtextBox1_2.AppendText(textData2);
                if (this.mwv.FeatureList.Count > 0)
                {
                    bool flag = false;
                    for (int index1 = 0; index1 < this.mwv.FeatureList.Count; ++index1)
                    {
                        for (int index2 = 0; index2 < this.mwv.FeatureList[index1].FPoints.Count; ++index2)
                        {
                            if (this.mwv.FeatureList[index1].FPoints[index2].X > this.sourceBitmap.Width - 6 || this.mwv.FeatureList[index1].FPoints[index2].Y > this.sourceBitmap.Height - 6)
                                flag = true;
                        }
                    }
                    if (flag)
                        this.mwv.FeatureList.Clear();
                }
                int selectedIndex = this.FileListGrid.SelectedIndex;
                string path = (this.SaveD.IsChecked.GetValueOrDefault() ? this.curDir : this.curDir + "\\" + this.SavePath.Text) + "\\" + MainWindow.FileList[selectedIndex].FileName.Replace(new FileInfo(MainWindow.FileList[selectedIndex].FileName).Extension.ToLower(), ".txt");
                if (File.Exists(path))
                {
                    this.mwv.FeatureList.Clear();
                    using (StreamReader streamReader2 = File.OpenText(path))
                    {
                        string str;
                        while ((str = streamReader2.ReadLine()) != null)
                        {
                            string[] strArray3 = str.Split(' ');
                            if (strArray3.Length == 9)
                            {
                                int num2 = int.Parse(strArray3[0]);
                                Point[] source = new Point[4]
                                {
                  new Point((int) (double.Parse(strArray3[1]) * (double) this.OrignalBitmap.Width / (double) this.wratio), (int) (double.Parse(strArray3[2]) * (double) this.OrignalBitmap.Height / (double) this.hratio)),
                  new Point((int) (double.Parse(strArray3[3]) * (double) this.OrignalBitmap.Width / (double) this.wratio), (int) (double.Parse(strArray3[4]) * (double) this.OrignalBitmap.Height / (double) this.hratio)),
                  new Point((int) (double.Parse(strArray3[5]) * (double) this.OrignalBitmap.Width / (double) this.wratio), (int) (double.Parse(strArray3[6]) * (double) this.OrignalBitmap.Height / (double) this.hratio)),
                  new Point((int) (double.Parse(strArray3[7]) * (double) this.OrignalBitmap.Width / (double) this.wratio), (int) (double.Parse(strArray3[8]) * (double) this.OrignalBitmap.Height / (double) this.hratio))
                                };
                                this.mwv.FeatureList.Add(new SelectableFeature()
                                {
                                    FPoints = ((IEnumerable<Point>)source).ToList<Point>(),
                                    Shape = 0,
                                    Cat = new int?(num2),
                                    Description = "Rectangle"
                                });
                            }
                            else
                            {
                                int num3 = int.Parse(strArray3[0]);
                                Point[] source = new Point[(strArray3.Length - 1) / 2];
                                int num4 = 0;
                                for (int index = 1; index < strArray3.Length - 1; index += 2)
                                {
                                    int x = (int)(double.Parse(strArray3[index]) * (double)this.OrignalBitmap.Width / (double)this.wratio);
                                    int y = (int)(double.Parse(strArray3[index + 1]) * (double)this.OrignalBitmap.Height / (double)this.hratio);
                                    source[num4++] = new Point(x, y);
                                }
                                this.mwv.FeatureList.Add(new SelectableFeature()
                                {
                                    FPoints = ((IEnumerable<Point>)source).ToList<Point>(),
                                    Shape = 1,
                                    Cat = new int?(num3),
                                    Description = "Polygon"
                                });
                            }
                        }
                        this.FeaturesList.SelectedIndex = 0;
                    }
                }
                this.DrawF();
                this.DrawMap();
            }
            catch
            {
                if (this.FileListGrid.SelectedIndex >= this.FileListGrid.Items.Count - 1)
                    return;
                ++this.FileListGrid.SelectedIndex;
            }
        }

        private void IsAutoSave_CheckedChanged(object sender, EventArgs e)
        {
            bool? isChecked = this.IsAutoSave.IsChecked;
            if ((isChecked.HasValue ? new bool?(!isChecked.GetValueOrDefault()) : new bool?()).GetValueOrDefault())
                this.MSave.Visibility = Visibility.Visible;
            else
                this.MSave.Visibility = Visibility.Hidden;
        }

        private void MSave_Click(object sender, RoutedEventArgs e) => this.SaveFeatureFile();

        private void SaveFeatureFile()
        {
            if (this.sourceBitmap == null || this.mwv.FeatureList.Count == 0 || this.FileName == "")
                return;
            bool? isChecked = this.SaveD.IsChecked;
            string path1 = isChecked.GetValueOrDefault() ? this.curDir : this.curDir + "\\" + this.SavePath.Text;
            if (!Directory.Exists(path1))
                Directory.CreateDirectory(path1);
            string str1 = path1;
            string fileName = this.FileName;
            string lower = new FileInfo(this.FileName).Extension.ToLower();
            isChecked = this.TXT.IsChecked;
            string newValue = isChecked.GetValueOrDefault() ? ".txt" : ".Json";
            string str2 = fileName.Replace(lower, newValue);
            string path2 = str1 + "\\" + str2;
            this.sb = new StringBuilder();
            isChecked = this.TXT.IsChecked;
            if (isChecked.GetValueOrDefault())
            {
                for (int index1 = 0; index1 < this.mwv.FeatureList.Count; ++index1)
                {
                    this.sb.Append(this.mwv.FeatureList[index1].Cat.ToString() + " ");
                    for (int index2 = 0; index2 < this.mwv.FeatureList[index1].FPoints.Count; ++index2)
                    {
                        this.sb.Append(((float)this.mwv.FeatureList[index1].FPoints[index2].X * this.wratio / (float)this.OrignalBitmap.Width).ToString() + " " + ((float)this.mwv.FeatureList[index1].FPoints[index2].Y * this.hratio / (float)this.OrignalBitmap.Height).ToString());
                        if (index2 != this.mwv.FeatureList[index1].FPoints.Count - 1)
                            this.sb.Append(" ");
                    }
                    if (index1 != this.mwv.FeatureList.Count - 1)
                        this.sb.Append("\n");
                }
                using (TextWriter textWriter = (TextWriter)new StreamWriter((Stream)new FileStream(path2, FileMode.Create, FileAccess.Write, FileShare.Write, 4096, true), Encoding.Default))
                    textWriter.Write(this.sb.ToString());
                this.mwv.FileList[this.FileListGrid.SelectedIndex].IsSelected = true;
            }
            else
            {
                for (int index3 = 0; index3 < this.mwv.FeatureList.Count; ++index3)
                {
                    this.sb.Append("{ \n\" label\":\"" + this.mwv.FeatureList[index3].Cat.ToString() + "\",\n");
                    this.sb.Append("\" points\":[\n");
                    for (int index4 = 0; index4 < this.mwv.FeatureList[index3].FPoints.Count; ++index4)
                    {
                        this.sb.Append("  [\n");
                        StringBuilder sb = this.sb;
                        string[] strArray = new string[5]
                        {
              "   ",
              null,
              null,
              null,
              null
                        };
                        float num = (float)this.mwv.FeatureList[index3].FPoints[index4].X * this.wratio / (float)this.sourceBitmap.Width;
                        strArray[1] = num.ToString();
                        strArray[2] = ",\n   ";
                        num = (float)this.mwv.FeatureList[index3].FPoints[index4].Y * this.hratio / (float)this.sourceBitmap.Height;
                        strArray[3] = num.ToString();
                        strArray[4] = "\n";
                        string str3 = string.Concat(strArray);
                        sb.Append(str3);
                        if (index4 != this.mwv.FeatureList[index3].FPoints.Count - 1)
                            this.sb.Append("  ],\n");
                        else
                            this.sb.Append("  ]\n");
                    }
                    if (index3 != this.mwv.FeatureList.Count - 1)
                        this.sb.Append(" ],\n      \"group_id\": null,\n      \"description\": \"\",\n      \"shape_type\": \"polygon\",\n      \"flags\": {},\n      \"mask\": null\n },\n");
                    else
                        this.sb.Append(" ],\n      \"group_id\": null,\n      \"description\": \"\",\n      \"shape_type\": \"polygon\",\n      \"flags\": {},\n      \"mask\": null\n}\n");
                }
                string[] strArray1 = new string[8]
                {
          "{\n\"shapes\": [\n" + this.sb.ToString() + "  ],\n \"imagePath\": \"" + this.FileName + "\",\n\"imageData\":\n",
          "\"",
          this.ImageToBase64(this.curDir + "\\" + this.FileName),
          "\",\n \"imageHeight\":",
          null,
          null,
          null,
          null
                };
                int num1 = this.OrignalBitmap.Height;
                strArray1[4] = num1.ToString();
                strArray1[5] = ",\n  \"imageWidth\":";
                num1 = this.OrignalBitmap.Width;
                strArray1[6] = num1.ToString();
                strArray1[7] = "\n}";
                string str4 = string.Concat(strArray1);
                using (TextWriter textWriter = (TextWriter)new StreamWriter((Stream)new FileStream(path2, FileMode.Create, FileAccess.Write, FileShare.Write, 4096, true), Encoding.Default))
                    textWriter.Write(str4);
                this.mwv.FileList[this.FileListGrid.SelectedIndex].IsSelected = true;
            }
        }

        public string ImageToBase64(string imgpath)
        {
            using (System.Drawing.Image image = System.Drawing.Image.FromFile(imgpath))
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    image.Save((Stream)memoryStream, image.RawFormat);
                    return Convert.ToBase64String(memoryStream.ToArray());
                }
            }
        }

        private static string ConvertImageToBase64(string imagePath) => Convert.ToBase64String(File.ReadAllBytes(imagePath));

        public static string ToPixelBuffer2(string FilePath)
        {
            Bitmap bitmap = (Bitmap)System.Drawing.Image.FromFile(FilePath);
            BitmapData bitmapdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            byte[] numArray = new byte[bitmapdata.Stride * bitmapdata.Height];
            Marshal.Copy(bitmapdata.Scan0, numArray, 0, numArray.Length);
            bitmap.UnlockBits(bitmapdata);
            return Convert.ToBase64String(numArray);
        }

        public static unsafe byte[] ToPixelBuffer(string FilePath)
        {
            Bitmap bitmap = (Bitmap)System.Drawing.Image.FromFile(FilePath);
            BitmapData bitmapdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
            int num1 = System.Drawing.Image.GetPixelFormatSize(bitmap.PixelFormat) / 8;
            byte[] pixelBuffer = new byte[bitmapdata.Width * num1 * bitmapdata.Height];
            byte* pointer = (byte*)bitmapdata.Scan0.ToPointer();
            fixed (byte* numPtr1 = pixelBuffer)
            {
                int stride = bitmapdata.Stride;
                for (int index1 = 0; index1 < bitmapdata.Height; ++index1)
                {
                    byte* numPtr2 = pointer + index1 * stride;
                    byte* numPtr3 = numPtr1 + index1 * bitmapdata.Width * num1;
                    int num2 = 0;
                    while (num2 < bitmapdata.Width)
                    {
                        for (int index2 = 0; index2 < num1; ++index2)
                            numPtr3[index2] = numPtr2[index2];
                        ++num2;
                        numPtr2 += num1;
                        numPtr3 += num1;
                    }
                }
            }
            bitmap.UnlockBits(bitmapdata);
            return pixelBuffer;
        }

        private void pictureBox1_SizeChanged(object sender, EventArgs e) => this.DataGrid_SelectionChanged((object)null, (SelectionChangedEventArgs)null);

        private void FeaturesList_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void Form_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (curPS != null && curPS.Count > 0)
                { curPS.Clear(); DrawF(); }
                else if (SelectedF >= 0 || HoverFP >= 0)
                {
                    SelectedF = -1;
                    HoverFP = -1;
                    DrawF();
                }
            }
            else if (e.Key == Key.Delete)
            {
                if (mwv!.FeatureList != null && mwv!.FeatureList.Count > 0)
                {
                    mwv!.FeatureList.RemoveAt(SelectedF);
                    DrawF();
                    DrawMap();
                }
            }
        }
        private void GWpf_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (this.sourceBitmap == null || e.Button == MouseButtons.Right)
                return;
            int num1 = (int)this.RWidth.Value.Value / 2;
            int num2 = (int)this.RHeight.Value.Value / 2;
            bool? isChecked = this.BRect.IsChecked;
            if (isChecked.GetValueOrDefault() && (e.X < num1 / 2 + 8 || e.Y < num2 / 2 + 8 || e.X + num1 / 2 > this.sourceBitmap.Width - 8 || e.Y + num2 / 2 > this.sourceBitmap.Height - 8) || e.X < 5 || e.X >= this.sourceBitmap.Width - 5 || e.Y < 5 || e.Y >= this.sourceBitmap.Height - 5)
                return;
            if (this.HoverF >= 0)
                this.SelectedF = this.HoverF;
            isChecked = this.BRect.IsChecked;
            bool flag = false;
            if (isChecked.GetValueOrDefault() == flag & isChecked.HasValue && this.HoverF < 0)
            {
                if (this.curPS == null || this.curPS.Count == 0)
                {
                    this.curPS = new List<Point>();
                    this.curPS.Add(new Point(e.X, e.Y));
                    this.drawPoly = true;
                }
                else if (this.close && this.mwv.FeatureList.Count < this.MaxF)
                {
                    this.mwv.FeatureList.Add(new SelectableFeature()
                    {
                        FPoints = this.curPS,
                        Shape = 1,
                        Cat = new int?(2),
                        Description = "Polygon"
                    });
                    this.DrawMap();
                    this.closed = true;
                }
                else
                    this.curPS.Add(new Point(e.X, e.Y));
            }
            else if (this.HoverF == -1)
            {
                if (this.mwv.FeatureList.Count >= this.MaxF)
                    return;
                Point[] source = new Point[4]
                {
          new Point(e.X - num1, e.Y - num2),
          new Point(e.X + num1, e.Y - num2),
          new Point(e.X + num1, e.Y + num2),
          new Point(e.X - num1, e.Y + num2)
                };
                this.mwv.FeatureList.Add(new SelectableFeature()
                {
                    FPoints = ((IEnumerable<Point>)source).ToList<Point>(),
                    Shape = 0,
                    Cat = new int?(this.mwv.FeatureList.Count),
                    Description = "Rectangle"
                });
                this.FeaturesList.SelectedIndex = this.mwv.FeatureList.Count - 1;
                this.DrawMap();
                this.DrawF();
            }
            else
            {
                if (this.SelectedF < 0)
                    return;
                this.mousePx = e.X;
                this.mousePy = e.Y;
                this.MousePress = true;
                this.FeaturesList.SelectedIndex = this.SelectedF;
            }
        }

        private void GWpf_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (this.sourceBitmap == null)
                return;
            if (this.BRect.IsChecked.GetValueOrDefault())
            {
                double x1 = (double)e.X;
                double num1 = 5.0;
                double? nullable1 = this.RWidth.Value;
                double num2 = 2.0;
                double? nullable2 = nullable1.HasValue ? new double?(nullable1.GetValueOrDefault() / num2) : new double?();
                double? nullable3 = nullable2.HasValue ? new double?(num1 + nullable2.GetValueOrDefault()) : new double?();
                double valueOrDefault1 = nullable3.GetValueOrDefault();
                if (x1 < valueOrDefault1 & nullable3.HasValue)
                {
                    double x2 = (double)e.X;
                    double num3 = (double)(this.sourceBitmap.Width - 5);
                    double? nullable4 = this.RWidth.Value;
                    double num4 = 2.0;
                    double? nullable5 = nullable4.HasValue ? new double?(nullable4.GetValueOrDefault() / num4) : new double?();
                    double? nullable6 = nullable5.HasValue ? new double?(num3 - nullable5.GetValueOrDefault()) : new double?();
                    double valueOrDefault2 = nullable6.GetValueOrDefault();
                    if (x2 >= valueOrDefault2 & nullable6.HasValue)
                    {
                        double y1 = (double)e.Y;
                        double num5 = 5.0;
                        double? nullable7 = this.RHeight.Value;
                        double num6 = 2.0;
                        double? nullable8 = nullable7.HasValue ? new double?(nullable7.GetValueOrDefault() / num6) : new double?();
                        nullable6 = nullable8.HasValue ? new double?(num5 + nullable8.GetValueOrDefault()) : new double?();
                        double valueOrDefault3 = nullable6.GetValueOrDefault();
                        if (y1 < valueOrDefault3 & nullable6.HasValue)
                        {
                            double y2 = (double)e.Y;
                            double num7 = (double)(this.sourceBitmap.Height - 5);
                            double? nullable9 = this.RHeight.Value;
                            double num8 = 2.0;
                            double? nullable10 = nullable9.HasValue ? new double?(nullable9.GetValueOrDefault() / num8) : new double?();
                            nullable6 = nullable10.HasValue ? new double?(num7 - nullable10.GetValueOrDefault()) : new double?();
                            double valueOrDefault4 = nullable6.GetValueOrDefault();
                            if (y2 >= valueOrDefault4 & nullable6.HasValue)
                                return;
                        }
                    }
                }
            }
            bool? isChecked = this.BRect.IsChecked;
            bool flag1 = false;
            if (isChecked.GetValueOrDefault() == flag1 & isChecked.HasValue && e.X < 5 && e.X >= this.sourceBitmap.Width - 5 && e.Y < 5 && e.Y >= this.sourceBitmap.Height - 5)
                return;
            if (this.drawPoly)
                this.DrawF(e.X, e.Y);
            else if (this.MousePress && e.Button == MouseButtons.Left)
            {
                if (this.SelectedF >= 0 && this.HoverFP < 0)
                {
                    this.GWpf.Cursor = System.Windows.Input.Cursors.SizeAll;
                    SelectableFeature feature = this.mwv.FeatureList[this.SelectedF];
                    int num9 = e.X - this.mousePx;
                    int num10 = e.Y - this.mousePy;
                    bool flag2 = false;
                    for (int index = 0; index < feature.FPoints.Count; ++index)
                    {
                        Point fpoint = feature.FPoints[index];
                        int num11 = fpoint.X + num9;
                        int num12 = fpoint.Y + num10;
                        if (num11 < 5 || num12 < 5 || num11 >= this.sourceBitmap.Width - 5 || num12 >= this.sourceBitmap.Height - 5)
                        {
                            flag2 = true;
                            break;
                        }
                    }
                    if (!flag2)
                    {
                        for (int index = 0; index < feature.FPoints.Count; ++index)
                        {
                            Point fpoint = feature.FPoints[index];
                            fpoint.X += num9;
                            fpoint.Y += num10;
                            feature.FPoints[index] = fpoint;
                        }
                    }
                }
                else if (this.SelectedF >= 0 && this.HoverFP >= 0)
                {
                    this.GWpf.Cursor = System.Windows.Input.Cursors.Hand;
                    SelectableFeature feature = this.mwv.FeatureList[this.SelectedF];
                    int num13 = e.X - this.mousePx;
                    int num14 = e.Y - this.mousePy;
                    if (feature.Shape == 0)
                    {
                        Point fpoint1 = feature.FPoints[this.HoverFP];
                        int index1 = this.HoverFP == 0 ? 3 : this.HoverFP - 1;
                        Point fpoint2 = feature.FPoints[index1];
                        int index2 = this.HoverFP == 3 ? 0 : this.HoverFP + 1;
                        Point fpoint3 = feature.FPoints[index2];
                        int index3 = this.HoverFP > 1 ? this.HoverFP - 2 : this.HoverFP + 2;
                        Point fpoint4 = feature.FPoints[index3];
                        int num15 = fpoint1.X + num13;
                        int num16 = fpoint1.Y + num14;
                        if (num15 < 5)
                            num15 = 5;
                        if (num16 < 5)
                            num16 = 5;
                        if (num15 >= this.sourceBitmap.Width - 5)
                            num15 = this.sourceBitmap.Width - 6;
                        if (num16 >= this.sourceBitmap.Height - 5)
                            num16 = this.sourceBitmap.Height - 6;
                        if (fpoint2.X == fpoint1.X)
                        {
                            fpoint1.X = num15;
                            fpoint2.X = num15;
                            fpoint1.Y = num16;
                            fpoint3.Y = num16;
                        }
                        else
                        {
                            fpoint1.X = num15;
                            fpoint3.X = num15;
                            fpoint1.Y = num16;
                            fpoint2.Y = num16;
                        }
                        if (Math.Abs(fpoint1.X - fpoint4.X) <= 15 || Math.Abs(fpoint1.Y - fpoint4.Y) <= 15)
                            return;
                        feature.FPoints[this.HoverFP] = fpoint1;
                        feature.FPoints[index1] = fpoint2;
                        feature.FPoints[index2] = fpoint3;
                    }
                    else
                    {
                        Point fpoint = feature.FPoints[this.HoverFP];
                        fpoint.X += num13;
                        fpoint.Y += num14;
                        if (fpoint.X < 5)
                            fpoint.X = 5;
                        if (fpoint.Y < 5)
                            fpoint.Y = 5;
                        if (fpoint.X >= this.sourceBitmap.Width - 5)
                            fpoint.X = this.sourceBitmap.Width - 6;
                        if (fpoint.Y >= this.sourceBitmap.Height - 5)
                            fpoint.Y = this.sourceBitmap.Height - 6;
                        feature.FPoints[this.HoverFP] = fpoint;
                    }
                }
                this.mousePx = e.X;
                this.mousePy = e.Y;
                this.DrawF();
                this.DrawMap();
            }
            else
            {
                this.GWpf.Cursor = System.Windows.Input.Cursors.None;
                int polyIndex = this.GetPolyIndex(e.X, e.Y);
                if (polyIndex > 0)
                {
                    this.HoverF = polyIndex - 1;
                    this.GWpf.Cursor = System.Windows.Input.Cursors.SizeAll;
                }
                for (int index4 = 0; index4 < this.mwv.FeatureList.Count; ++index4)
                {
                    SelectableFeature feature = this.mwv.FeatureList[index4];
                    for (int index5 = 0; index5 < feature.FPoints.Count; ++index5)
                    {
                        if (Math.Abs(feature.FPoints[index5].X - e.X) < 8 && Math.Abs(feature.FPoints[index5].Y - e.Y) < 8)
                        {
                            this.HoverF = index4;
                            this.HoverFP = index5;
                            this.GWpf.Cursor = System.Windows.Input.Cursors.Hand;
                            this.DrawF();
                            return;
                        }
                    }
                }
                this.HoverF = polyIndex - 1;
                this.HoverFP = -1;
                this.DrawF();
            }
        }

        private void GWpf_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (this.drawPoly)
                return;
            this.MousePress = false;
            this.DrawF();
        }

        public static Bitmap BitmapAdjust(Bitmap bmp, float brightness, float contrast)
        {
            Bitmap bitmap = new Bitmap(bmp.Width, bmp.Height, bmp.PixelFormat);
            ImageAttributes imageAttr = new ImageAttributes();
            ColorMatrix newColorMatrix = new ColorMatrix(new float[5][]
            {
        new float[5]{ contrast, 0.0f, 0.0f, 0.0f, 0.0f },
        new float[5]{ 0.0f, contrast, 0.0f, 0.0f, 0.0f },
        new float[5]{ 0.0f, 0.0f, contrast, 0.0f, 0.0f },
        new float[5]{ 0.0f, 0.0f, 0.0f, 1f, 0.0f },
        new float[5]{ brightness, brightness, brightness, 0.0f, 1f }
            });
            using (Graphics graphics = Graphics.FromImage((System.Drawing.Image)bitmap))
            {
                imageAttr.SetColorMatrix(newColorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                graphics.DrawImage((System.Drawing.Image)bmp, new Rectangle(0, 0, bitmap.Width, bitmap.Height), 0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel, imageAttr);
            }
            return bitmap;
        }

        private void GWpf_SizeChanged(object sender, SizeChangedEventArgs e) => this.DataGrid_SelectionChanged((object)null, (SelectionChangedEventArgs)null);

        private void Contrast_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.displayBitmap == null)
                return;
            this.displayBitmap = MainWindow.BitmapAdjust(this.sourceBitmap, (float)this.Brightness.Value / 100f, (float)this.Contrast.Value / 100f);
            this.DrawF();
        }

        private void SketchImg_Checked(object sender, RoutedEventArgs e) => this.DataGrid_SelectionChanged((object)null, (SelectionChangedEventArgs)null);

        private void MNext_Click(object sender, RoutedEventArgs e)
        {
            if (this.IsAutoSave.IsChecked.GetValueOrDefault())
                this.SaveFeatureFile();
            else if (this.mwv.FeatureList != null && this.mwv.FeatureList.Count > 0)
            {
                string path = (this.SaveD.IsChecked.GetValueOrDefault() ? this.curDir : this.curDir + "\\" + this.SavePath.Text) + "\\" + this.FileName.Replace(new FileInfo(this.FileName).Extension, this.TXT.IsChecked.GetValueOrDefault() ? ".txt" : ".Json");
                if ((!Directory.Exists(path) || !File.Exists(path)) && System.Windows.Forms.MessageBox.Show("特征未保存，是否保存特征文件?", "提示框", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk) == System.Windows.Forms.DialogResult.OK)
                    this.SaveFeatureFile();
            }
            if (this.FileListGrid.SelectedIndex >= this.FileListGrid.Items.Count - 1)
                return;
            ++this.FileListGrid.SelectedIndex;
        }

        private void IsAutoSave_Checked(object sender, RoutedEventArgs e)
        {
            bool? isChecked = this.IsAutoSave.IsChecked;
            bool flag = false;
            if (isChecked.GetValueOrDefault() == flag & isChecked.HasValue)
            {
                this.MSave.Visibility = Visibility.Visible;
            }
            else
            {
                if (this.MSave == null)
                    return;
                this.MSave.Visibility = Visibility.Hidden;
            }
        }

        private void FeaturesList_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.mwv.FeatureList.Count > 0 && this.FeaturesList.SelectedItems.Count > 0)
            {
                this.SelectedF = this.FeaturesList.SelectedIndex;
                if (this.mwv.FeatureList[this.SelectedF].Shape == 0)
                    this.BRect.IsChecked = new bool?(true);
                else
                    this.BPolygon.IsChecked = new bool?(true);
            }
            else
                this.SelectedF = -1;
            this.DrawF();
        }

        private void GWpf_GdiContextDraw(int e)
        {
            switch (e)
            {
                case 0:
                    if (this.curPS == null || this.curPS.Count <= 0)
                        break;
                    this.curPS.Clear();
                    this.DrawF();
                    break;
                case 1:
                    this.SelectedF = -1;
                    break;
                case 2:
                    if (this.HoverF < 0 || this.mwv.FeatureList == null || this.mwv.FeatureList.Count <= 0)
                        break;
                    this.mwv.FeatureList.RemoveAt(this.HoverF);
                    this.DrawF();
                    this.DrawMap();
                    break;
            }
        }

        private void ContextMenuFPoints_Click(object sender, RoutedEventArgs e)
        {
            if (this.curPS == null || this.curPS.Count <= 0)
                return;
            this.curPS.Clear();
            this.DrawF();
        }

        private void ContextMenuCat_Click(object sender, RoutedEventArgs e)
        {
            if (this.SelectedF < 0 || this.mwv.FeatureList == null || this.mwv.FeatureList.Count <= 0 || this.SelectedF < 0)
                return;
            this.mwv.FeatureList.RemoveAt(this.SelectedF);
            this.FeaturesList.Items.RemoveAt(this.SelectedF);
            this.DrawF();
            this.DrawMap();
        }

        private void ContextMenuSharp_Click(object sender, RoutedEventArgs e) => this.SelectedF = -1;

        Color[] PrimaryColor => new Color[]
        {
      System.Drawing.Color.Red,
      System.Drawing.Color.Lime,
      System.Drawing.Color.Purple,
      System.Drawing.Color.Maroon,
      System.Drawing.Color.DarkOrange,
      System.Drawing.Color.Indigo,
      System.Drawing.Color.Blue,
      System.Drawing.Color.LightBlue,
      System.Drawing.Color.LightGreen,
      System.Drawing.Color.Cyan,
      System.Drawing.Color.Teal,
      System.Drawing.Color.Yellow,
      System.Drawing.Color.Green,
      System.Drawing.Color.Orange,
      System.Drawing.Color.Brown,
      System.Drawing.Color.Chocolate,
      System.Drawing.Color.DarkSalmon,
      System.Drawing.Color.Aquamarine,
      System.Drawing.Color.DarkSeaGreen,
      System.Drawing.Color.Pink
        };
    }
}
