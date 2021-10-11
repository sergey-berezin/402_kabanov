using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using YOLOv4MLNet;
using YOLOv4MLNet.DataStructures;

namespace task
{
    class Program
    {

        static public ConcurrentQueue<YoloV4Result> queueeue = new ConcurrentQueue<YoloV4Result>();
        static async Task Main(string[] args)
        {
            var cst = new CancellationTokenSource();
            var ct = cst.Token;


            YoloV4Result result;
            string s = @"C:\Users\Asus\source\repos\YOLOv4MLNet\YOLOv4MLNet\Assets\Images";
            _ = Task.Run(() =>
            {
                string cancel = Console.ReadLine();
                if (cancel == "c" || cancel == "с")
                    cst.Cancel();
            }
            );
            var task1 = Yolo.FunAsync(s, queueeue, ct);
            
            
            var task2 = Task.Run(() =>
            {
                while (true)
                {
                    while (queueeue.TryDequeue(out result))
                    {
                        // печать
                        var lab = result.Label;
                        var x1 = result.BBox[0];
                        var y1 = result.BBox[1];
                        var x2 = result.BBox[2];
                        var y2 = result.BBox[3];
                        Console.WriteLine($"{lab},   ({x1}  ,  {y1}) ; ({x2}  ,  {y2}) ");

                    }
                }
                
            });
            await Task.WhenAll(task1);
        }
    }
}
