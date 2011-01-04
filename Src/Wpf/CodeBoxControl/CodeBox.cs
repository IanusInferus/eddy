using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.ComponentModel;
using CodeBoxControl.Decorations;

namespace CodeBoxControl
{
    /// <summary>
    ///  A control to view or edit styled text<kssksk> 
    /// </summary>
    public partial class CodeBox : TextBox
    {
        /// <summary>
        /// Used to cached the render in case of invalid textbox properties.
        /// </summary>
        private CodeBoxRenderInfo renderinfo = new CodeBoxRenderInfo();


        /// <summary>
        /// Has the scroll event on the scrollviewer been enabled.
        /// </summary>
        bool mScrollingEventEnabled;

        public CodeBox()
        {

            this.TextChanged += new TextChangedEventHandler(txtTest_TextChanged);
            this.Background = new SolidColorBrush(Colors.Transparent);
            this.Foreground = new SolidColorBrush(Colors.Transparent);
            this.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            this.TextWrapping = TextWrapping.Wrap;
            InitializeComponent();
            this.AcceptsReturn = true;
        }

        public static DependencyProperty BaseForegroundProperty = DependencyProperty.Register("BaseForeground", typeof(Brush), typeof(CodeBox),
   new FrameworkPropertyMetadata(new SolidColorBrush(Colors.Black), FrameworkPropertyMetadataOptions.AffectsRender));

        [Bindable(true)]
        public Brush BaseForeground
        {
            get { return (Brush)GetValue(BaseForegroundProperty); }
            set { SetValue(BaseForegroundProperty, value); }
        }

        public static DependencyProperty CodeBoxBackgroundProperty = DependencyProperty.Register("CodeBoxBackground", typeof(Brush), typeof(CodeBox),
   new FrameworkPropertyMetadata(new SolidColorBrush(Colors.White), FrameworkPropertyMetadataOptions.AffectsRender));

        [Bindable(true)]
        public Brush CodeBoxBackground
        {
            get { return (Brush)GetValue(CodeBoxBackgroundProperty); }
            set { SetValue(CodeBoxBackgroundProperty, value); }
        }

        void txtTest_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.InvalidateVisual();
        }

        private List<Decoration> mDecorations = new List<Decoration>();
        /// <summary>
        /// List of the Decorative attributes assigned to the text
        /// </summary>
        public List<Decoration> Decorations
        {
            get { return mDecorations; }
            set { mDecorations = value; }
        }

        public static DependencyProperty DecorationSchemeProperty = DependencyProperty.Register("DecorationScheme", typeof(DecorationScheme), typeof(CodeBox),
   new FrameworkPropertyMetadata(new DecorationScheme(), FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// The DecorationScheme used for the CodeBox
        /// </summary>
        /// 
        public DecorationScheme DecorationScheme
        {
            get { return (DecorationScheme)GetValue(DecorationSchemeProperty); }
            set { SetValue(DecorationSchemeProperty, value); }

        }

        FormattedText formattedText;
        int previousFirstChar = -1;
        #region OnRender

        /// <summary>
        /// Overrides render and divides into the designer and nondesigner cases.
        /// </summary>
        /// <param name="drawingContext"></param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            EnsureScrolling();
            base.OnRender(drawingContext);

            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                OnRenderDesigner(drawingContext);
            }
            else
            {
                if (this.LineCount == 0)
                {
                    ReRenderLastRuntimeRender(drawingContext);
                    Action a = () => this.InvalidateVisual();
                    this.Dispatcher.BeginInvoke(a);
                }
                else
                {
                    OnRenderRuntime(drawingContext);
                }
            }
        }

        /// <summary>
        ///The main render code
        /// </summary>
        /// <param name="drawingContext"></param>
        protected void OnRenderRuntime(DrawingContext drawingContext)
        {
            drawingContext.PushClip(new RectangleGeometry(new Rect(0, 0, this.ActualWidth, this.ActualHeight)));//restrict drawing to textbox
            drawingContext.DrawRectangle(CodeBoxBackground, null, new Rect(0, 0, this.ActualWidth, this.ActualHeight));//Draw Background
            if (this.Text == "") return;

            int firstLineIndex = GetFirstVisibleLineIndex();// GetFirstLine();
            int firstCharIndex = GetCharacterIndexFromLineIndex(firstLineIndex);// GetFirstChar();
            string visibleText = this.VisibleText;
            if (visibleText == null) return;

            Double leftMargin = 4.0 + this.BorderThickness.Left;
            Double topMargin = 2.0 + this.BorderThickness.Top;

            formattedText = new FormattedText(
                    visibleText,
                    CultureInfo.CurrentUICulture,
                    FlowDirection.LeftToRight,
                    new Typeface(this.FontFamily.Source),
                    this.FontSize,
                    BaseForeground);  //Text that matches the textbox's
            formattedText.Trimming = TextTrimming.None;

            ApplyTextWrapping(formattedText);

            Pair visiblePair = new Pair(firstCharIndex, visibleText.Length);
            double top = GetRenderTop(firstLineIndex);
            if (Double.IsInfinity(top)) { return; }
            Point renderPoint = new Point(leftMargin, top);
            renderinfo.RenderPoint = renderPoint;

            //Generates the prepared decorations for the BaseDecorations
            Dictionary<EDecorationType, Dictionary<Decoration, List<Geometry>>> basePreparedDecorations
                = GeneratePreparedDecorations(visiblePair, DecorationScheme.BaseDecorations);
            //Displays the prepared decorations for the BaseDecorations
            DisplayPreparedDecorations(drawingContext, basePreparedDecorations, renderPoint);

            //Generates the prepared decorations for the Decorations
            Dictionary<EDecorationType, Dictionary<Decoration, List<Geometry>>> preparedDecorations
                = GeneratePreparedDecorations(visiblePair, mDecorations);
            //Displays the prepared decorations for the Decorations
            DisplayPreparedDecorations(drawingContext, preparedDecorations, renderPoint);

            ColorText(firstCharIndex, DecorationScheme.BaseDecorations);//Colors According to Scheme
            ColorText(firstCharIndex, mDecorations);//Colors Acording to Decorations
            drawingContext.DrawText(formattedText, renderPoint);

            //Cache information for possible rerender
            renderinfo.BoxText = formattedText;
            renderinfo.BasePreparedDecorations = basePreparedDecorations;
            renderinfo.PreparedDecorations = preparedDecorations;
        }

        /// <summary>
        /// Render logic for the designer
        /// </summary>
        /// <param name="drawingContext"></param>
        protected void OnRenderDesigner(DrawingContext drawingContext)
        {

            int firstCharIndex = 0;

            Double leftMargin = 4.0 + this.BorderThickness.Left;
            Double topMargin = 2.0 + this.BorderThickness.Top;


            string visibleText = VisibleText;

            formattedText = new FormattedText(
                   this.Text,
                    CultureInfo.CurrentUICulture,
                    FlowDirection.LeftToRight,
                    new Typeface(this.FontFamily.Source),
                    this.FontSize,
                    BaseForeground);  //Text that matches the textbox's
            formattedText.Trimming = TextTrimming.None;

            previousFirstChar = firstCharIndex;
            Pair visiblePair = new Pair(firstCharIndex, Text.Length);

            drawingContext.PushClip(new RectangleGeometry(new Rect(0, 0, this.ActualWidth, this.ActualHeight)));//restrict text to textbox
            Point renderPoint = new Point(leftMargin, topMargin);

            drawingContext.DrawRectangle(CodeBoxBackground, null, new Rect(0, 0, this.ActualWidth, this.ActualHeight));

            Dictionary<Decoration, List<Geometry>> hilightgeometryDictionary = PrepareGeometries(visiblePair, formattedText, mDecorations, EDecorationType.Hilight, HilightGeometryMaker);
            DisplayGeometry(drawingContext, hilightgeometryDictionary, renderPoint);

            Dictionary<Decoration, List<Geometry>> strikethroughGeometryDictionary = PrepareGeometries(visiblePair, formattedText, mDecorations, EDecorationType.Strikethrough, StrikethroughGeometryMaker);
            DisplayGeometry(drawingContext, strikethroughGeometryDictionary, renderPoint);

            Dictionary<Decoration, List<Geometry>> underlineGeometryDictionary = PrepareGeometries(visiblePair, formattedText, mDecorations, EDecorationType.Underline, UnderlineGeometryMaker);
            DisplayGeometry(drawingContext, underlineGeometryDictionary, renderPoint);
            ColorText(firstCharIndex, mDecorations);
            drawingContext.DrawText(formattedText, renderPoint);
        }


        /// <summary>
        /// Performs the last successful render again.
        /// </summary>
        /// <param name="drawingContext"></param>
        protected void ReRenderLastRuntimeRender(DrawingContext drawingContext)
        {
            drawingContext.DrawText(renderinfo.BoxText, renderinfo.RenderPoint);
            DisplayPreparedDecorations(drawingContext, renderinfo.PreparedDecorations, renderinfo.RenderPoint);
            DisplayPreparedDecorations(drawingContext, renderinfo.BasePreparedDecorations, renderinfo.RenderPoint);
        }


        /// <summary>
        /// Performs the EDecorationType.TextColor decorations in the formattted text.
        /// </summary>
        /// <param name="firstChar"></param>
        /// <param name="decorations"></param>
        private void ColorText(int firstChar, List<Decoration> decorations)
        {
            if (decorations != null)
            {
                foreach (Decoration dec in decorations)
                {
                    if (dec.DecorationType == EDecorationType.TextColor)
                    {
                        List<Pair> ranges = dec.Ranges(this.Text);
                        foreach (Pair p in ranges)
                        {
                            if (p.End > firstChar && p.Start < firstChar + formattedText.Text.Length)
                            {
                                int adjustedStart = Math.Max(p.Start - firstChar, 0);
                                int adjustedLength = Math.Min(p.Length + Math.Min(p.Start - firstChar, 0), formattedText.Text.Length - adjustedStart);
                                formattedText.SetForegroundBrush(dec.Brush, adjustedStart, adjustedLength);
                            }
                        }
                    }
                }
            }
        }


        public void ApplyTextWrapping(FormattedText formattedText)
        {
            switch (this.TextWrapping)
            {
                case TextWrapping.NoWrap:
                    break;
                case TextWrapping.Wrap:
                    formattedText.MaxTextWidth = this.ViewportWidth; //Used with Wrap only
                    break;
                case TextWrapping.WrapWithOverflow:
                    formattedText.SetMaxTextWidths(VisibleLineWidthsIncludingTrailingWhitespace());
                    break;
            }

        }

        /// <summary>
        /// Displays the Decorations for a List of Decorations
        /// </summary>
        /// <param name="drawingContext">The drawing Context from the OnRender</param>
        /// <param name="visiblePair">The pair representing the first character of the Visible text with respect to the whole text</param>
        /// <param name="renderPoint">The Point representing the offset from (0,0) for rendering</param>
        /// <param name="decorations">The List of Decorations</param>
        private void DisplayDecorations(DrawingContext drawingContext, Pair visiblePair, Point renderPoint, List<Decoration> decorations)
        {
            Dictionary<Decoration, List<Geometry>> hilightgeometryDictionary = PrepareGeometries(visiblePair, formattedText, decorations, EDecorationType.Hilight, HilightGeometryMaker);
            DisplayGeometry(drawingContext, hilightgeometryDictionary, renderPoint);

            Dictionary<Decoration, List<Geometry>> strikethroughGeometryDictionary = PrepareGeometries(visiblePair, formattedText, decorations, EDecorationType.Strikethrough, StrikethroughGeometryMaker);
            DisplayGeometry(drawingContext, strikethroughGeometryDictionary, renderPoint);

            Dictionary<Decoration, List<Geometry>> underlineGeometryDictionary = PrepareGeometries(visiblePair, formattedText, decorations, EDecorationType.Underline, UnderlineGeometryMaker);
            DisplayGeometry(drawingContext, underlineGeometryDictionary, renderPoint);

        }

        /// <summary>
        /// The first part of the split version of Display decorations.
        /// </summary>
        /// <param name="visiblePair">The pair representing the first character of the Visible text with respect to the whole text</param>
        /// <param name="decorations">The List of Decorations</param>
        /// <returns></returns>
        private Dictionary<EDecorationType, Dictionary<Decoration, List<Geometry>>> GeneratePreparedDecorations(Pair visiblePair, List<Decoration> decorations)
        {
            Dictionary<EDecorationType, Dictionary<Decoration, List<Geometry>>> preparedDecorations = new Dictionary<EDecorationType, Dictionary<Decoration, List<Geometry>>>();
            Dictionary<Decoration, List<Geometry>> hilightgeometryDictionary = PrepareGeometries(visiblePair, formattedText, decorations, EDecorationType.Hilight, HilightGeometryMaker);
            preparedDecorations.Add(EDecorationType.Hilight, hilightgeometryDictionary);
            Dictionary<Decoration, List<Geometry>> strikethroughGeometryDictionary = PrepareGeometries(visiblePair, formattedText, decorations, EDecorationType.Strikethrough, StrikethroughGeometryMaker);
            preparedDecorations.Add(EDecorationType.Strikethrough, strikethroughGeometryDictionary);
            Dictionary<Decoration, List<Geometry>> underlineGeometryDictionary = PrepareGeometries(visiblePair, formattedText, decorations, EDecorationType.Underline, UnderlineGeometryMaker);
            preparedDecorations.Add(EDecorationType.Underline, underlineGeometryDictionary);
            return preparedDecorations;
        }

        /// <summary>
        /// The second half of the  DisplayDecorations.
        /// </summary>
        /// <param name="drawingContext">The drawing Context from the OnRender</param>
        /// <param name="preparedDecorations">The previously prepared decorations</param>
        /// <param name="renderPoint">The Point representing the offset from (0,0) for rendering</param>
        private void DisplayPreparedDecorations(DrawingContext drawingContext, Dictionary<EDecorationType, Dictionary<Decoration, List<Geometry>>> preparedDecorations, Point renderPoint)
        {
            DisplayGeometry(drawingContext, preparedDecorations[EDecorationType.Hilight], renderPoint);
            DisplayGeometry(drawingContext, preparedDecorations[EDecorationType.Strikethrough], renderPoint);
            DisplayGeometry(drawingContext, preparedDecorations[EDecorationType.Underline], renderPoint);
        }
        #endregion




        /// <summary>
        /// Gets the Renderpoint, the top left corner of the first character displayed. Note that this can 
        /// have negative vslues when the textbox is scrolling.
        /// </summary>
        /// <param name="firstLineIndex">The first visible line index</param>
        /// <returns></returns>
        private double GetRenderTop(int firstLineIndex)
        {
            int firstCharIndex = GetCharacterIndexFromLineIndex(firstLineIndex);
            int firstLineLength = GetLineLength(firstLineIndex);

            double Top = double.MaxValue;

            for (int i = 0; i < firstLineLength; i += 1)
            {
                Rect cRect = GetRectFromCharacterIndex(firstCharIndex + i);
                Top = Math.Min(Top, cRect.Top);
            }
            return Top;
        }

        private void DisplayGeometry(DrawingContext drawingContext, Dictionary<Decoration, List<Geometry>> geometryDictionary, Point renderPoint)
        {
            foreach (Decoration dec in geometryDictionary.Keys)
            {
                List<Geometry> GeomList = geometryDictionary[dec];
                foreach (Geometry g in GeomList)
                {
                    g.Transform = new System.Windows.Media.TranslateTransform(renderPoint.X, renderPoint.Y);
                    drawingContext.DrawGeometry(dec.Brush, null, g);
                }
            }
        }


        private Dictionary<Decoration, List<Geometry>> PrepareGeometries(Pair pair, FormattedText visibleFormattedText, List<Decoration> decorations, EDecorationType decorationType, GeometryMaker gMaker)
        {
            Dictionary<Decoration, List<Geometry>> geometryDictionary = new Dictionary<Decoration, List<Geometry>>();
            foreach (Decoration dec in decorations)
            {
                List<Geometry> geomList = new List<Geometry>();
                if (dec.DecorationType == decorationType)
                {
                    List<Pair> ranges = dec.Ranges(this.Text);
                    foreach (Pair p in ranges)
                    {
                        if (p.End > pair.Start && p.Start < pair.Start + VisibleText.Length)
                        {
                            int adjustedStart = Math.Max(p.Start - pair.Start, 0);
                            int adjustedLength = Math.Min(p.Length + Math.Min(p.Start - pair.Start, 0), pair.Length - adjustedStart);
                            Geometry geom = gMaker(visibleFormattedText, new Pair(adjustedStart, adjustedLength));
                            geomList.Add(geom);
                        }
                    }
                }
                geometryDictionary.Add(dec, geomList);
            }
            return geometryDictionary;
        }

        /// <summary>
        ///Delegate used with the PrepareGeomeries method.
        /// </summary>
        /// <param name="text">The FormattedText used for the decoration</param>
        /// <param name="p">The pair defining the begining character and the length of the character range</param>
        /// <returns></returns>
        private delegate Geometry GeometryMaker(FormattedText text, Pair p);

        /// <summary>
        /// Creates the Geometry for the Hilight decoration, used with the GeometryMakerDelegate.
        /// </summary>
        /// <param name="text">The FormattedText used for the decoration</param>
        /// <param name="p">The pair defining the begining character and the length of the character range</param>
        /// <returns></returns>
        private Geometry HilightGeometryMaker(FormattedText text, Pair p)
        {
            return text.BuildHighlightGeometry(new Point(0, 0), p.Start, p.Length);
        }

        /// <summary>
        /// Creates the Geometry for the Underline decoration, used with the GeometryMakerDelegate.
        /// </summary>
        /// <param name="text">The FormattedText used for the decoration</param>
        /// <param name="p">The pair defining the begining character and the length of the character range</param>
        /// <returns></returns>
        private Geometry UnderlineGeometryMaker(FormattedText text, Pair p)
        {
            Geometry geom = text.BuildHighlightGeometry(new Point(0, 0), p.Start, p.Length);
            if (geom != null)
            {
                StackedRectangleGeometryHelper srgh = new StackedRectangleGeometryHelper(geom);
                return srgh.BottomEdgeRectangleGeometry();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Creates the Geometry for the Strikethrough decoration, used with the GeometryMakerDelegate.
        /// </summary>
        /// <param name="text">The FormattedText used for the decoration</param>
        /// <param name="p">The pair defining the begining character and the length of the character range</param>
        /// <returns></returns>
        private Geometry StrikethroughGeometryMaker(FormattedText text, Pair p)
        {
            Geometry geom = text.BuildHighlightGeometry(new Point(0, 0), p.Start, p.Length);
            if (geom != null)
            {
                StackedRectangleGeometryHelper srgh = new StackedRectangleGeometryHelper(geom);
                return srgh.CenterLineRectangleGeometry();
            }
            else
            {
                return null;
            }
        }



        /// <summary>
        /// Makes sure that the scrolling event is being listended to.
        /// </summary>
        private void EnsureScrolling()
        {
            if (!mScrollingEventEnabled)
            {
                try
                {
                    DependencyObject dp = VisualTreeHelper.GetChild(this, 0);
                    ScrollViewer sv = VisualTreeHelper.GetChild(dp, 0) as ScrollViewer;
                    sv.ScrollChanged += new ScrollChangedEventHandler(ScrollChanged);
                    mScrollingEventEnabled = true;
                }
                catch { }
            }
        }

        private void ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            this.InvalidateVisual();
        }

        /// <summary>
        /// Gets the Text that is visible in the textbox. Please note that it depends on
        ///  GetFirstVisibleLineIndex and 
        /// </summary>
        private string VisibleText
        {
            get
            {
                if (this.Text == "") { return ""; }
                string visibleText = "";
                try
                {
                    int textLength = Text.Length;
                    int firstLine = GetFirstVisibleLineIndex();
                    int lastLine = GetLastVisibleLineIndex();

                    int lineCount = this.LineCount;
                    int firstChar = (firstLine == 0) ? 0 : GetCharacterIndexFromLineIndex(firstLine);

                    int lastChar = GetCharacterIndexFromLineIndex(lastLine) + GetLineLength(lastLine) - 1;
                    int length = lastChar - firstChar + 1;
                    int maxlenght = textLength - firstChar;
                    string text = Text.Substring(firstChar, Math.Min(maxlenght, length));
                    if (text != null)
                    {
                        visibleText = text;
                    }
                }
                catch
                {
                    Debug.WriteLine("GetVisibleText failure");
                }
                return visibleText;
            }
        }

        /// <summary>
        /// Returns the line widths for use with the wrap with overflow.
        /// </summary>
        /// <returns></returns>
        private Double[] VisibleLineWidthsIncludingTrailingWhitespace()
        {

            int firstLine = this.GetFirstVisibleLineIndex();
            int lastLine = Math.Max(this.GetLastVisibleLineIndex(), firstLine);
            Double[] lineWidths = new Double[lastLine - firstLine + 1];
            if (lineWidths.Length == 1)
            {
                lineWidths[0] = MeasureString(this.Text);
            }
            else
            {
                for (int i = firstLine; i <= lastLine; i++)
                {
                    string lineString = this.GetLineText(i);
                    lineWidths[i - firstLine] = MeasureString(lineString);
                }
            }
            return lineWidths;
        }


        /// <summary>
        /// Returns the width of the string in the font and fontsize of the textbox including the trailing white space.
        /// Used for wrap with overflow.
        /// </summary>
        /// <param name="str">The string to measure</param>
        /// <returns></returns>
        private double MeasureString(string str)
        {
            FormattedText formattedText = new FormattedText(
                 str,
                   CultureInfo.CurrentUICulture,
                   FlowDirection.LeftToRight,
                   new Typeface(this.FontFamily.Source),
                   this.FontSize,
                  new SolidColorBrush(Colors.Black));
            if (str == "")
            {
                return formattedText.WidthIncludingTrailingWhitespace;
            }
            else if (str.Substring(0, 1) == "\t")
            {
                return formattedText.WidthIncludingTrailingWhitespace;
            }
            else
            {
                return formattedText.WidthIncludingTrailingWhitespace;
            }
        }
    }
}

