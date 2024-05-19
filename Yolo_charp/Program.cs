using System;
using SixLabors.ImageSharp; // Из одноимённого пакета NuGet
using SixLabors.ImageSharp.PixelFormats;
using System.Linq;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.OnnxRuntime;
using System.Collections.Generic;

namespace YOLO_csharp
{
    class Program
    {
        static void Main(string[] args)
        {
            using var image = Image.Load<Rgba32>(args.FirstOrDefault() ?? "c4943d83c06a12ad5e0399d19514a4ca_jpg.rf.99b2d7e1faa204e71fdc71676040c4d6.jpg");

            // Размер изображения
            const int TargetWidth = 416;
            const int TargetHeight = 416;

            // Изменяем размер изображения до 416 x 416
            var resized = image.Clone(x =>
            {
                x.Resize(new ResizeOptions
                {
                    Size = new Size(TargetWidth, TargetHeight),
                    Mode = ResizeMode.Pad // Дополнить изображение до указанного размера
                });
            });

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
                var conf = outputs[0, 4, i];
                confs.Add(conf);
                if (objectness < conf)
                {
                    BestIndexes.Add(i);
                    objectness = conf;
                }
            }

            BestIndexes.Reverse();

            string[] labels = new string[]
               {
                       "black-bishop", "black-king", "black-knight", "black-pawn", "black-queen", "black-rook", "white-bishop", "white-king", "white-knight", "white-pawn", "white-queen", "white-rook"
               };

            //Находим класс
            int bestclassIndex = 0;
            double bestScore = Double.MinValue;
            for (int j = 1; j < 12; j++)
            {
                if (outputs[0, 4 + j, BestIndexes[0]] > bestScore)
                {
                    bestScore = outputs[0, 4 + j, BestIndexes[0]];
                    bestclassIndex = j;
                }
            }

            Console.WriteLine($"Best objectness {objectness}; Best class {labels[bestclassIndex - 1]}; Class conf {bestScore}");
            Console.WriteLine(
                   String.Join(',',
                       confs
                       .OrderByDescending(c => c)
                       .Take(10)
                       .Select(c => c.ToString())));


            void BoundingBoxes(Image<Rgba32> im, int index, string save)
            {
                for (int i = 0; i < Math.Min(index, BestIndexes.Count); i++)
                {

                    var X = outputs[0, 0, BestIndexes[i]];
                    var Y = outputs[0, 1, BestIndexes[i]];
                    var width = outputs[0, 2, BestIndexes[i]];
                    var height = outputs[0, 3, BestIndexes[i]];

                    Console.WriteLine($"{X} {Y} {width} {height}");

                    im.Mutate(
                    ctx => ctx.DrawPolygon(
                        Pens.Dash(Color.Red, 2),
                        new PointF[] {
                               new PointF(X - width / 2, Y - height / 2),
                               new PointF(X + width / 2, Y - height / 2),
                               new PointF(X + width / 2, Y + height / 2),
                               new PointF(X - width / 2, Y + height / 2)
                        }));
                    im.Save(save);
                }
            }
            BoundingBoxes(resized, 1, "result.jpg");
        }
    }
}
