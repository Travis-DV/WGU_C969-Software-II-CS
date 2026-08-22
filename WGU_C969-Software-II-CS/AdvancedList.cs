/*namespace WGU_C969_Software_II_CS;

public class AdvancedList<T>(App.UpdateComboBox comboBoxUpdater) : List<T>
{
    private App.UpdateComboBox ComboBoxUpdater { get; } = comboBoxUpdater;

    public new T this[int index]
    {
        get => base[index];
        set => base[index] = value;
    }

    public new void Add(T item)
    {
        base.Add(item);
        this.ComboBoxUpdater();
    }
    
    /*public static implicit operator List<string>(AdvancedList<T> list)
    {
        return list.Select(item => item?.ToString() ?? "").ToList();
    }#1#
}*/