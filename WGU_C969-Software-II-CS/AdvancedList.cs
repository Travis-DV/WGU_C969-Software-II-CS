using System.Runtime.CompilerServices;

namespace WGU_C969_Software_II_CS;

public class AdvancedList<T> : List<T>
{
    private App.UpdateComboBox ComboBoxUpdater { get; }

    public AdvancedList(App.UpdateComboBox comboBoxUpdater)
    {
        this.ComboBoxUpdater = comboBoxUpdater;
    }

    public T this[int index]
    {
        get => base[index];
        set => base[index] = value;
    }

    public new void Add(T item)
    {
        base.Add(item);
        if (this.ComboBoxUpdater == null)
        {
            Console.WriteLine("delegate null");
            return;
        }
        this.ComboBoxUpdater();
    }
    
    public static implicit operator List<string>(AdvancedList<T> list)
    {
        return list.Select(item => item?.ToString() ?? "").ToList();
    }
}