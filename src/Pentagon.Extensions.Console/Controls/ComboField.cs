namespace Pentagon.Extensions.Console.Controls {
    public class ComboField<T>
    {
        public T Content { get; set; }

        public bool IsSelected { get; set; }

        internal int RowSpan { get; set; }
    }
}