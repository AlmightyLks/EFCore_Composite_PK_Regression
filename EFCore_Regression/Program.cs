using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

Setup();

using var ctx = new Ctx();
var table = ctx.Tables.First();
table.Rows.Remove(table.Rows.First());
table.Rows.Add(new UIRow() { Table = table });

ctx.SaveChanges();

void Setup()
{
    using var ctx = new Ctx();
    ctx.Database.EnsureDeleted();
    ctx.Database.EnsureCreated();
    var table = new UITable();
    ctx.Tables.Add(table);
    ctx.SaveChanges();

    var row = new UIRow() { Table = table };
    table.Rows.Add(row);
    ctx.SaveChanges();
}


public class UITable : INotifyPropertyChanged, INotifyPropertyChanging
{
    private long _id;

    public long Id
    {
        get => _id;
        set
        {
            if (_id != value)
            {
                this.OnPropertyChanging();
                this._id = value;
                this.OnPropertyChanged();
            }
        }
    }

    private ICollection<UIRow> _rows = null!;
    public ICollection<UIRow> Rows { get => lazyLoader.Load(this, ref _rows); }



    protected readonly Action<object, string> lazyLoader;

    public UITable()
    {

    }

    public UITable(Action<object, string> lazyLoader)
    {
        this.lazyLoader = lazyLoader;
    }

    public event PropertyChangingEventHandler PropertyChanging;
    public event PropertyChangedEventHandler PropertyChanged;

    public virtual void OnPropertyChanging([CallerMemberName] string? propertyName = null)
    {
        if (PropertyChanging != null)
        {
            PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
        }
    }
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        if (PropertyChanged != null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

public class UIRow : INotifyPropertyChanged, INotifyPropertyChanging
{
    private int _tableId;
    private int _position;

    public int TableId
    {
        get => _tableId;
        set
        {

            if (_tableId != value)
            {
                this.OnPropertyChanging();
                this._tableId = value;
                this.OnPropertyChanged();
            }
        }
    }
    public int Position
    {
        get => _position;
        set
        {

            if (_tableId != value)
            {
                this.OnPropertyChanging();
                this._position = value;
                this.OnPropertyChanged();
            }
        }
    }

    private UITable _table = null!;

    public UITable Table
    {
        get => lazyLoader.Load(this, ref _table);
        set
        {

            if (_table != value)
            {
                this.OnPropertyChanging();
                this._table = value;
                this.OnPropertyChanged();
            }
        }
    }



    protected readonly Action<object, string> lazyLoader;

    public UIRow()
    {

    }

    public UIRow(Action<object, string> lazyLoader)
    {
        this.lazyLoader = lazyLoader;
    }

    public event PropertyChangingEventHandler? PropertyChanging;
    public event PropertyChangedEventHandler? PropertyChanged;

    public virtual void OnPropertyChanging([CallerMemberName] string? propertyName = null)
    {
        if (PropertyChanging != null)
        {
            PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
        }
    }
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        if (PropertyChanged != null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

public class Ctx : DbContext
{
    public DbSet<UITable> Tables { get; set; }
    public DbSet<UIRow> Rows { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=data.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotifications);

        modelBuilder.Entity<UITable>()
            .HasKey(x => x.Id);

        modelBuilder.Entity<UITable>()
            .HasMany(x => x.Rows)
            .WithOne(x => x.Table)
            .IsRequired();

        modelBuilder.Entity<UIRow>()
            .HasKey(x => new { x.TableId, x.Position });
    }
}

public static class PocoLoadingExtensions
{
    public static TEntity Load<TEntity>(
        this Action<object, string> loader,
        object entity,
        ref TEntity navigationField,
        [CallerMemberName] string navigationName = null)
        where TEntity : class
    {
        loader?.Invoke(entity, navigationName);

        return navigationField;
    }
    public static ICollection<TRelated> Load<TRelated>(
        this Action<object, string> loader,
        object entity,
        ref ICollection<TRelated> navigationField,
        [CallerMemberName] string navigationName = null)
        where TRelated : class
    {
        loader?.Invoke(entity, navigationName);
        navigationField ??= new ObservableCollection<TRelated>();

        return navigationField;
    }
}