using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

using System.Threading;

using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using YOLOv4MLNet.DataStructures;
using static Microsoft.ML.Transforms.Image.ImageResizingEstimator;
using System.Collections.Concurrent;

namespace YOLOv4MLNet
{
    public class Yolo
    {
        const string modelPath = @"C:\Users\Asus\Desktop\2021-1-cs-tasks\yolov4.onnx"; 
        const string imageFolder = @"Assets\Images";
        public static int num = 0;
        const string imageOutputFolder = @"Assets\Output";

        static readonly string[] classesNames = new string[] { "person", "bicycle", "car", "motorbike", "aeroplane", "bus", "train", "truck", "boat", "traffic light", "fire hydrant", "stop sign", "parking meter", "bench", "bird", "cat", "dog", "horse", "sheep", "cow", "elephant", "bear", "zebra", "giraffe", "backpack", "umbrella", "handbag", "tie", "suitcase", "frisbee", "skis", "snowboard", "sports ball", "kite", "baseball bat", "baseball glove", "skateboard", "surfboard", "tennis racket", "bottle", "wine glass", "cup", "fork", "knife", "spoon", "bowl", "banana", "apple", "sandwich", "orange", "broccoli", "carrot", "hot dog", "pizza", "donut", "cake", "chair", "sofa", "pottedplant", "bed", "diningtable", "toilet", "tvmonitor", "laptop", "mouse", "remote", "keyboard", "cell phone", "microwave", "oven", "toaster", "sink", "refrigerator", "book", "clock", "vase", "scissors", "teddy bear", "hair drier", "toothbrush" };

        static async Task<YoloV4Prediction> GetPredictionAsync(PredictionEngine<YoloV4BitmapData, YoloV4Prediction> predictionEngine, Bitmap bitmap)
        {
            return await Task<YoloV4Prediction>.Factory.StartNew(() =>
            {
                return predictionEngine.Predict(new YoloV4BitmapData() { Image = bitmap });
            });
        }
        
        public static async Task FunAsync(string s, BufferBlock<IReadOnlyList<YoloV4Result>> output, CancellationToken ct)
        {
            /////////////////////////////////////////////////////////////////////////////////////////////
            ///not my stuff
            /////////////////////////////////////////////////////////////////////////////////////////////
            Directory.CreateDirectory(imageOutputFolder);
            MLContext mlContext = new MLContext();
            var pipeline = mlContext.Transforms.ResizeImages(inputColumnName: "bitmap", outputColumnName: "input_1:0", imageWidth: 416, imageHeight: 416, resizing: ResizingKind.IsoPad)
                .Append(mlContext.Transforms.ExtractPixels(outputColumnName: "input_1:0", scaleImage: 1f / 255f, interleavePixelColors: true))
                .Append(mlContext.Transforms.ApplyOnnxModel(
                    shapeDictionary: new Dictionary<string, int[]>()
                    {
                        { "input_1:0", new[] { 1, 416, 416, 3 } },
                        { "Identity:0", new[] { 1, 52, 52, 3, 85 } },
                        { "Identity_1:0", new[] { 1, 26, 26, 3, 85 } },
                        { "Identity_2:0", new[] { 1, 13, 13, 3, 85 } },
                    },
                    inputColumnNames: new[]
                    {
                        "input_1:0"
                    },
                    outputColumnNames: new[]
                    {
                        "Identity:0",
                        "Identity_1:0",
                        "Identity_2:0"
                    },
                    modelFile: modelPath, recursionLimit: 100));

            // Fit on empty list to obtain input data schema
            var model = pipeline.Fit(mlContext.Data.LoadFromEnumerable(new List<YoloV4BitmapData>()));


            var sw = new Stopwatch();
            sw.Start();


            // image name extractor

            DirectoryInfo dir = new DirectoryInfo(s);
            string[] extensions = new[] { ".jpg", ".tiff", ".bmp", ".png" };
            FileInfo[] files = dir.GetFiles().Where(f => extensions.Contains(f.Extension.ToLower())).ToArray();
            var images = new List<string>();
            foreach (FileInfo f in files)
            {
                images.Add(f.FullName);
            }
            num = images.Count();
            
            /////////////////////////////////////////////////////////////////////////////////////////////
            ///my stuff
            /////////////////////////////////////////////////////////////////////////////////////////////

            
            var tmp = new ConcurrentDictionary<string, int>();
            int val;

            var Tblock = new ActionBlock<string>(async imageName =>
            {
                using (var bitmap = new Bitmap(Image.FromFile(Path.Combine(s, imageName))))
                {
                   
                    var predictionEngine = mlContext.Model.CreatePredictionEngine<YoloV4BitmapData, YoloV4Prediction>(model);
                    var predict = await GetPredictionAsync(predictionEngine, bitmap);
                    
                    IReadOnlyList<YoloV4Result> irl = predict.GetResults(classesNames, 0.3f, 0.7f);
                    

                    foreach (YoloV4Result item in irl)
                    {
                        item.GiveName(imageName);
                    }

                    _ = output.Post(irl);
                   
                }
            },
                new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = 2,
                    CancellationToken = ct
                }

            );

            Parallel.ForEach(images, imageName => Tblock.Post(imageName));
            Tblock.Complete();
            
            try
            {
                await Tblock.Completion;
                output.Complete();
                await output.Completion;
                Console.WriteLine("                                                        end");  
            }
            catch
            {
                Console.WriteLine("+++++++++++++++++++++РУЧНОЕ ПРЕРЫВАНИЕ++++++++++++++++++++++++++ ");

            }
            
        }
    }
}
