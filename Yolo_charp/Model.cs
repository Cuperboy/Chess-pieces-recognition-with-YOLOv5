using System;
using SixLabors.ImageSharp; // Из одноимённого пакета NuGet
using SixLabors.ImageSharp.PixelFormats;
using System.Linq;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.OnnxRuntime;
using System.Collections.Generic;
using System.Diagnostics;
using SixLabors.Fonts;
using static System.Formats.Asn1.AsnWriter;
using System.Collections;
using System.ComponentModel;
using System.Net.NetworkInformation;
//using static System.Net.Mime.MediaTypeNames;

namespace YOLO_csharp
{
    public interface IModelParams
    {
        string imagePath { get; set; }
        string saveDir { get; set; }
        public double precision { get; set; }
        //public Image<Rgba32> result { get; set; }
    }
    public class Model
    {
        IModelParams parameters;
        public Model(IModelParams images)
        {
            parameters = images;
        }
        public void YoloProcess() //Image<Rgba32>
        {
            var imagePath = parameters.imagePath;
            var image = Image.Load<Rgba32>(imagePath);

            float Sigmoid(float value)
            {
                var e = (float)Math.Exp(value);
                return e / (1.0f + e);
            }

            // Размер 
            const int TargetWidth = 416;
            const int TargetHeight = 416;

            //Изменяем размер изображения до 416 x 416
            var resized = image.Clone(x =>
            {
                x.Resize(new ResizeOptions
                {
                    Size = new Size(TargetWidth, TargetHeight),
                    Mode = ResizeMode.Pad // Дополнить изображение до указанного размера
                });
            });
            image.Dispose();

            // Перевод пикселов в тензор и нормализация
            var input = new DenseTensor<float>(new[] { 1, 3, TargetHeight, TargetWidth });
            for (int y = 0; y < TargetHeight; y++)
            {
                Span<Rgba32> pixelSpan = resized.GetPixelRowSpan(y);
                for (int x = 0; x < TargetWidth; x++)
                {
                    input[0, 0, y, x] = pixelSpan[x].R / 255f;
                    input[0, 1, y, x] = pixelSpan[x].G / 255f;
                    input[0, 2, y, x] = pixelSpan[x].B / 255f;
                }
            }

            // Подготавливаем входные данные нейросети. Имя input задано в файле модели
            var inputs = new List<NamedOnnxValue>
                {
                    NamedOnnxValue.CreateFromTensor("images", input),
                };

            // Вычисляем предсказание нейросетью
            using var session = new InferenceSession("best.onnx");
            using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = session.Run(inputs);

            // Получаем результаты
            var outputs = results.First().AsTensor<float>();

            //Находим лучший objectness
            double objectness = Double.MinValue;
            List<int> BestIndexes = new List<int>();
            List<float> confs = new List<float>();

            for (int i = 0; i < 3549; i++)
            {
                var ClassConf = Double.MinValue;
                var conf = outputs[0, 4, i];

                for (int j = 1; j < 12; j++)
                {
                    if (outputs[0, 4 + j, i] > ClassConf) ClassConf = outputs[0, 4 + j, i];
                }

                if (Sigmoid(conf) * Sigmoid((float)ClassConf) > parameters.precision)
                {
                    BoundingBoxes(resized, i);
                }
            }
            //parameters.result = resized.Clone();
            resized.Save("F:\\ML\\PracticalML2024\\new\\result.jpg");
            resized.Dispose();
            //return parameters.result;


            void BoundingBoxes(Image<Rgba32> im, int i)
            {
                var X = outputs[0, 0, i];
                var Y = outputs[0, 1, i];
                var width = outputs[0, 2, i];
                var height = outputs[0, 3, i];

                im.Mutate(
                ctx => ctx.DrawPolygon(
                    Pens.Dash(Color.Red, 2),
                    new PointF[] {
                               new PointF(X - width / 2, Y - height / 2),
                               new PointF(X + width / 2, Y - height / 2),
                               new PointF(X + width / 2, Y + height / 2),
                               new PointF(X - width / 2, Y + height / 2)
                    }));
            }
        }
    }
}
