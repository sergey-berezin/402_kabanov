using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace WpfAppDKab
{
    public class DataAction
    {
        private DBContext dbcon = new DBContext();
        public event Action DataChanged;
        //добавление
        public async Task AddAsync(Row item)
        { 
            if (isDub(item))
            { return; }
            await dbcon.Table.AddAsync(item);
            await dbcon.SaveChangesAsync();
            DataChanged?.Invoke();

        }
        //отчистка бд
        public void Reset()
        {
            dbcon.Database.EnsureDeleted();
            dbcon.Database.EnsureCreated();
            DataChanged?.Invoke();
        }
        //дедубликация
        private bool isDub(Row row)
        {
            //сравниваю рамки
            var dup = dbcon.Table.Where(a => (a.x == row.x && a.y == row.y && a.l == row.l && a.w == row.w));
            foreach (Row r in dup)
            {
                //сравниваю изображения
                if (r.Image.SequenceEqual(r.Image))
                {
                    return true;
                }
            }
            return false;
        }
        public IEnumerable<string> GetClasses()
        {
            IEnumerable<string> res;
            res = dbcon.Table.Select(a => a.Label).Distinct().Select(a => $"{a}  ({dbcon.Table.Where(b => b.Label == a).Count()}) ");
            return res;
        }

        public IEnumerable<byte[]> GetImages(string Label)
        {
            IEnumerable<byte[]> res;
            res = dbcon.Table.Where(a => a.Label == Label).Select(a => a.Image);
            return res;
        }
    }
    public abstract class BaseData : IEnumerable, INotifyCollectionChanged
    {
        public Comp comp { get; set; }
        public BaseData(Comp a)
        {
            comp = a;
            comp.DataAction.DataChanged += RaiseCollectionChanged;
        }
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public void RaiseCollectionChanged()
        {
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        //byte -> bitmap
        public static BitmapImage GetBitmapImageFromByte(byte[] arr)
        {
            using (var ms = new System.IO.MemoryStream(arr))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = ms;
                image.EndInit();
                return image;
            }
        }
        public abstract IEnumerator GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
    // источник для Listbox классов
    public class ClassContext : BaseData
    {
        public ClassContext(Comp comp) : base(comp) { }

        public override IEnumerator<string> GetEnumerator()
        {
            return comp.DataAction.GetClasses().GetEnumerator();
        }
    }
    // источник для ListView картинок
    public class ImageContext : BaseData
    {
        public ImageContext(Comp comp) : base(comp) { }
        private string str;
        public void Select(string select) 
        {
            str = select;
        }
        public override IEnumerator<BitmapImage> GetEnumerator()
        {
            return comp.DataAction.GetImages(str).Select(x => GetBitmapImageFromByte(x)).GetEnumerator();
        }
        
    }
    //столбцы бд
    public partial class Row
    {
        public int RowId { get; set; }
        public string Label { get; set; }
        public byte[] Image { get; set; }
        public float x { get; set; }
        public float y { get; set; }
        public float l { get; set; }
        public float w { get; set; }
    }
    //бд
    class DBContext : DbContext
    {
        /*public DBContext()
        {
            Database.EnsureDeleted();
            Database.EnsureCreated();
        }*/
        public DbSet<Row> Table { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
             modelBuilder.Entity<Row>().HasKey(x => x.RowId);
        }
        protected override void OnConfiguring(DbContextOptionsBuilder a)
        {
            a.UseSqlite(@"Data Source=C:\Users\Asus\source\repos\YOLOv4MLNet\WpfAppDKab\lib.db");
        }
    }
}
