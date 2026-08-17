using System.Linq;

namespace AzureTools.Models
{
    public class Option<T> : IOption
    {
        public Option(T value, string displayName, bool disabled = false)
        {
            Value = value;
            DisplayName = displayName;
            Disabled = disabled;
        }

        public T Value { get; set; }
        public bool Disabled { get; set; }

        public string DisplayName { get; set; }

        public static Option<T>[] Create(params System.Tuple<T, string>[] options)
        {
            return options.Select(x => new Option<T>(x.Item1, x.Item2)).ToArray();
        }
    }
}
