using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using YOLOv4MLNet;
using YOLOv4MLNet.DataStructures;
using System.Threading.Tasks.Dataflow;

namespace WpfAppDKab
{
    public class Comp : INotifyPropertyChanged
    {
        private  BufferBlock<IReadOnlyList<YoloV4Result>> resq; 
        public event PropertyChangedEventHandler PropertyChanged;
        CancellationTokenSource cst = new CancellationTokenSource();
        bool Procflg = false;
        public ClassContext cc { get; }
        public ImageContext ic { get; }
        public DataAction DataAction { get; } = new();
        string procstr = "Обработка не началась";
        string inputPath = "";
        string total = "0 / 0";
        int num = 0;
        
        public string Total
        {
            get => total;
            set
            {
                total = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Total"));
            }
        }
        public string InputPath
        {
            get => inputPath;
            set
            {
                inputPath = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("InputPath"));
            }
        }
        public Comp()
        {
            cc = new ClassContext(this);
            ic = new ImageContext(this);
        }
        
        public string ProcStr
        {
            get => procstr;
            set
            {
                procstr = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ProcStr"));
            }
        }
        public void StopProc()
        {
            cst.Cancel();
            Procflg = false;
            ProcStr = "Ручное Прерывание";
            
        }
        public void Reset()
        {
            DataAction.Reset();
        }
        public void StartProc()
        {
            if (Procflg)
            {
                return;
            }
            if (inputPath == "")
            {
                return;
            }
            num = 0;
            cst = new CancellationTokenSource();
            resq = new BufferBlock<IReadOnlyList<YoloV4Result>>();
            _ = TaskAsync();

        }
        public void SelectorHandler(string s)
        {
            if (s == null)
            {
                return;
            }
            ic.Select(s.Split(" ")[0]);
            
            ic.RaiseCollectionChanged();
        }

        private async Task TaskAsync()
        {
            num = 0;
            Procflg = true;
            ProcStr = "Работаем";
            var task1 = Yolo.FunAsync(inputPath, resq, cst.Token);
            await toDBAsync(resq);
            Procflg = false;
            ProcStr = "Работа завершена";
        }
        private async Task toDBAsync(ISourceBlock<IReadOnlyList<YoloV4Result>> irl)
        {
            while (await irl.OutputAvailableAsync() )
            {
                lock (Total)
                {
                    Total = (++num) + "/" + Yolo.num+ "--" + $" {100* num/Yolo.num}% " ;
                }
                var data = irl.Receive();
                foreach (YoloV4Result item in data)
                {
                    YoloItem yi = new YoloItem(item);
                    await DataAction.AddAsync(yi);  
                }
                await Task.Delay(1);
                cst.Token.ThrowIfCancellationRequested();
            }
            MessageBox.Show("aaa");
        }
       
       
    }
}
