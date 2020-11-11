// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Linq;
using System.Numerics;
using GeoJSON.Net.Converters;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using Newtonsoft.Json;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Drawing.Shapes;
using SixLabors.ImageSharp.Drawing.Tests.TestUtilities.ImageComparison;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

using Xunit;

namespace SixLabors.ImageSharp.Drawing.Tests.Drawing
{
    [GroupOutput("Drawing")]
    public class FillPolygonTests
    {
        [Theory]
        [WithSolidFilledImages(8, 12, nameof(Color.Black), PixelTypes.Rgba32, 0)]
        [WithSolidFilledImages(8, 12, nameof(Color.Black), PixelTypes.Rgba32, 8)]
        [WithSolidFilledImages(8, 12, nameof(Color.Black), PixelTypes.Rgba32, 16)]
        public void FillPolygon_Solid_Basic<TPixel>(TestImageProvider<TPixel> provider, int antialias)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            PointF[] polygon1 = PolygonFactory.CreatePointArray((2, 2), (6, 2), (6, 4), (2, 4));
            PointF[] polygon2 = PolygonFactory.CreatePointArray((2, 8), (4, 6), (6, 8), (4, 10));

            var options = new GraphicsOptions { Antialias = antialias > 0, AntialiasSubpixelDepth = antialias };
            provider.RunValidatingProcessorTest(
                c => c.SetGraphicsOptions(options)
                    .FillPolygon(Color.White, polygon1)
                    .FillPolygon(Color.White, polygon2),
                appendPixelTypeToFileName: false,
                appendSourceFileOrDescription: false,
                testOutputDetails: $"aa{antialias}");
        }

        [Theory]
        [WithBasicTestPatternImages(250, 350, PixelTypes.Rgba32, "White", 1f, true)]
        [WithBasicTestPatternImages(250, 350, PixelTypes.Rgba32, "White", 0.6f, true)]
        [WithBasicTestPatternImages(250, 350, PixelTypes.Rgba32, "White", 1f, false)]
        [WithBasicTestPatternImages(250, 350, PixelTypes.Bgr24, "Yellow", 1f, true)]
        public void FillPolygon_Solid<TPixel>(TestImageProvider<TPixel> provider, string colorName, float alpha, bool antialias)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            PointF[] simplePath =
                {
                    new Vector2(10, 10), new Vector2(200, 150), new Vector2(50, 300)
                };
            Color color = TestUtils.GetColorByName(colorName).WithAlpha(alpha);

            var options = new GraphicsOptions { Antialias = antialias };

            string aa = antialias ? "" : "_NoAntialias";
            FormattableString outputDetails = $"{colorName}_A{alpha}{aa}";

            provider.RunValidatingProcessorTest(
                c => c.SetGraphicsOptions(options).FillPolygon(color, simplePath),
                outputDetails,
                appendSourceFileOrDescription: false);
        }

        public static TheoryData<bool, IntersectionRule> FillPolygon_Complex_Data =
            new TheoryData<bool, IntersectionRule>()
            {
                {false, IntersectionRule.OddEven},
                {false, IntersectionRule.Nonzero},
                {true, IntersectionRule.OddEven},
                {true, IntersectionRule.Nonzero},
            };

        [Theory]
        [WithBasicTestPatternImages(nameof(FillPolygon_Complex_Data), 100, 100, PixelTypes.Rgba32)]
        public void FillPolygon_Complex<TPixel>(TestImageProvider<TPixel> provider, bool reverse, IntersectionRule intersectionRule)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            PointF[] contour = PolygonFactory.CreatePointArray((20, 20), (80, 20), (80, 80), (20, 80));
            PointF[] hole = PolygonFactory.CreatePointArray((40, 40), (40, 60), (60, 60), (60, 40));

            if (reverse)
            {
                Array.Reverse(contour);
                Array.Reverse(hole);
            }

            ComplexPolygon polygon = new ComplexPolygon(
                new Path(new LinearLineSegment(contour)),
                new Path(new LinearLineSegment(hole)));
            
            provider.RunValidatingProcessorTest(
                c =>
                {
                    c.SetShapeOptions(new ShapeOptions()
                    {
                        OrientationHandling = OrientationHandling.KeepOriginal,
                        IntersectionRule = intersectionRule
                    });
                    c.Fill(Color.White, polygon);
                },
                testOutputDetails: $"Reverse({reverse})_IntersectionRule({intersectionRule})",
                comparer: ImageComparer.TolerantPercentage(0.01f),
                appendSourceFileOrDescription: false,
                appendPixelTypeToFileName: false);
        }

        [Theory]
        [WithBasicTestPatternImages(200, 200, PixelTypes.Rgba32, false)]
        [WithBasicTestPatternImages(200, 200, PixelTypes.Rgba32, true)]
        public void FillPolygon_Concave<TPixel>(TestImageProvider<TPixel> provider, bool reverse)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            var points = new PointF[]
                             {
                                 new Vector2(8, 8),
                                 new Vector2(64, 8),
                                 new Vector2(64, 64),
                                 new Vector2(120, 64),
                                 new Vector2(120, 120),
                                 new Vector2(8, 120)
                             };
            if (reverse)
            {
                Array.Reverse(points);
            }

            var color = Color.LightGreen;

            provider.RunValidatingProcessorTest(
                c =>
                {
                    c.SetShapeOptions(new ShapeOptions()
                    {
                        OrientationHandling = OrientationHandling.KeepOriginal
                    });
                    c.FillPolygon(color, points);
                },
                testOutputDetails: $"Reverse({reverse})",
                comparer: ImageComparer.TolerantPercentage(0.01f),
                appendSourceFileOrDescription: false,
                appendPixelTypeToFileName: false);
        }

        [Theory]
        [WithSolidFilledImages(30, 30, "Black", PixelTypes.Rgba32)]
        public void FillPolygon_Reverse<TPixel>(TestImageProvider<TPixel> provider)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            PointF[] points = PolygonFactory.CreatePointArray(
                (2.5644531f, 2.6796875f),
                (2.9794922f, 10.443359f),
                (3.6386719f, 23.382812f),
                (24.634766f, 23.382812f));

            using var image = provider.GetImage();
            image.Mutate(c =>
            {
                c.SetShapeOptions(new ShapeOptions()
                {
                    OrientationHandling = OrientationHandling.KeepOriginal
                });
                c.FillPolygon(Color.White, points);
            });
            image.DebugSave(provider);
        }

        [Theory]
        [WithBasicTestPatternImages(250, 350, PixelTypes.Rgba32)]
        public void FillPolygon_Pattern<TPixel>(TestImageProvider<TPixel> provider)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            PointF[] simplePath =
                {
                    new Vector2(10, 10), new Vector2(200, 150), new Vector2(50, 300)
                };
            var color = Color.Yellow;

            var brush = Brushes.Horizontal(color);

            provider.RunValidatingProcessorTest(
                c => c.FillPolygon(brush, simplePath),
                appendSourceFileOrDescription: false);
        }

        [Theory]
        [WithBasicTestPatternImages(250, 350, PixelTypes.Rgba32, TestImages.Png.Ducky)]
        [WithBasicTestPatternImages(250, 350, PixelTypes.Rgba32, TestImages.Bmp.Car)]
        public void FillPolygon_ImageBrush<TPixel>(TestImageProvider<TPixel> provider, string brushImageName)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            PointF[] simplePath =
                {
                    new Vector2(10, 10), new Vector2(200, 50), new Vector2(50, 200)
                };

            using (var brushImage = Image.Load<TPixel>(TestFile.Create(brushImageName).Bytes))
            {
                var brush = new ImageBrush(brushImage);

                provider.RunValidatingProcessorTest(
                    c => c.FillPolygon(brush, simplePath),
                    System.IO.Path.GetFileNameWithoutExtension(brushImageName),
                    appendSourceFileOrDescription: false);
            }
        }

        [Theory]
        [WithBasicTestPatternImages(250, 250, PixelTypes.Rgba32)]
        public void Fill_RectangularPolygon<TPixel>(TestImageProvider<TPixel> provider)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            var polygon = new RectangularPolygon(10, 10, 190, 140);
            var color = Color.White;

            provider.RunValidatingProcessorTest(
                c => c.Fill(color, polygon),
                appendSourceFileOrDescription: false);
        }

        [Theory]
        [WithBasicTestPatternImages(200, 200, PixelTypes.Rgba32, 3, 50, 0f)]
        [WithBasicTestPatternImages(200, 200, PixelTypes.Rgba32, 3, 60, 20f)]
        [WithBasicTestPatternImages(200, 200, PixelTypes.Rgba32, 3, 60, -180f)]
        [WithBasicTestPatternImages(200, 200, PixelTypes.Rgba32, 5, 70, 0f)]
        [WithBasicTestPatternImages(200, 200, PixelTypes.Rgba32, 7, 80, -180f)]
        public void Fill_RegularPolygon<TPixel>(TestImageProvider<TPixel> provider, int vertices, float radius, float angleDeg)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            float angle = GeometryUtilities.DegreeToRadian(angleDeg);
            var polygon = new RegularPolygon(100, 100, vertices, radius, angle);
            var color = Color.Yellow;

            FormattableString testOutput = $"V({vertices})_R({radius})_Ang({angleDeg})";
            provider.RunValidatingProcessorTest(
                c => c.Fill(color, polygon),
                testOutput,
                appendSourceFileOrDescription: false,
                appendPixelTypeToFileName: false);
        }

        public static readonly TheoryData<bool, IntersectionRule> Fill_EllipsePolygon_Data =
            new TheoryData<bool, IntersectionRule>()
            {
                { false, IntersectionRule.OddEven},
                { false, IntersectionRule.Nonzero},
                { true, IntersectionRule.OddEven},
                { true, IntersectionRule.Nonzero},
            };

        [Theory]
        [WithBasicTestPatternImages(nameof(Fill_EllipsePolygon_Data), 200, 200, PixelTypes.Rgba32)]
        public void Fill_EllipsePolygon<TPixel>(TestImageProvider<TPixel> provider, bool reverse, IntersectionRule intersectionRule)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            IPath polygon = new EllipsePolygon(100, 100, 80, 120);
            if (reverse)
            {
                polygon = polygon.Reverse();
            }
            
            var color = Color.Azure;

            provider.RunValidatingProcessorTest(
                c =>
                {
                    c.SetShapeOptions(new ShapeOptions()
                    {
                        IntersectionRule = intersectionRule,
                        OrientationHandling = OrientationHandling.KeepOriginal
                    });
                    c.Fill(color, polygon);
                },
                testOutputDetails: $"Reverse({reverse})_IntersectionRule({intersectionRule})",
                appendSourceFileOrDescription: false,
                appendPixelTypeToFileName: false);
        }

        [Theory]
        [WithSolidFilledImages(60, 60, "Blue", PixelTypes.Rgba32)]
        public void Fill_IntersectionRules_OddEven<TPixel>(TestImageProvider<TPixel> provider)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            using (var img = provider.GetImage())
            {

                var poly = new Polygon(new LinearLineSegment(
                    new PointF(10, 30),
                    new PointF(10, 20),
                    new PointF(50, 20),
                    new PointF(50, 50),
                    new PointF(20, 50),
                    new PointF(20, 10),
                    new PointF(30, 10),
                    new PointF(30, 40),
                    new PointF(40, 40),
                    new PointF(40, 30),
                    new PointF(10, 30)));

                img.Mutate(c => c.Fill(
                    new ShapeGraphicsOptions
                    {
                        ShapeOptions = { IntersectionRule = IntersectionRule.OddEven },
                    },
                    Color.HotPink,
                    poly));

                provider.Utility.SaveTestOutputFile(img);

                Assert.Equal(Color.Blue.ToPixel<TPixel>(), img[25, 25]);
            }
        }

        [Theory]
        [WithSolidFilledImages(60, 60, "Blue", PixelTypes.Rgba32)]
        public void Fill_IntersectionRules_Nonzero<TPixel>(TestImageProvider<TPixel> provider)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            Configuration.Default.MaxDegreeOfParallelism = 1;
            using (var img = provider.GetImage())
            {
                var poly = new Polygon(new LinearLineSegment(
                    new PointF(10, 30),
                    new PointF(10, 20),
                    new PointF(50, 20),
                    new PointF(50, 50),
                    new PointF(20, 50),
                    new PointF(20, 10),
                    new PointF(30, 10),
                    new PointF(30, 40),
                    new PointF(40, 40),
                    new PointF(40, 30),
                    new PointF(10, 30)));
                img.Mutate(c => c.Fill(
                    new ShapeGraphicsOptions
                    {
                        ShapeOptions = { IntersectionRule = IntersectionRule.Nonzero },
                    },
                    Color.HotPink,
                    poly));

                provider.Utility.SaveTestOutputFile(img);
                Assert.Equal(Color.HotPink.ToPixel<TPixel>(), img[25, 25]);
            }
        }
    }
}
