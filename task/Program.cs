using System;
using System.Threading;
using YOLOv4MLNet;
using YOLOv4MLNet.DataStructures;

namespace task
{
    class Program
    {
        

        static void Main(string[] args)
        {
            string s = @"C:\Users\Asus\source\repos\YOLOv4MLNet\YOLOv4MLNet\Assets\Images";
            var res = Yolo.FunAsync(s);
            YoloV4Result result;
            
            while(res.Result.Count != 0)
            {
                res.Result.TryDequeue(out result);
                var lab = result.Label;
                var x1 = result.BBox[0];
                var y1 = result.BBox[1];
                var x2 = result.BBox[2];
                var y2 = result.BBox[3];
                Console.WriteLine($"{lab},   ({x1}  ,  {y1}) ; ({x2}  ,  {y2}) ");
            }

        }
    }
}
