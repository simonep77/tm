using System.Collections.Generic;

namespace TaskInterfaceLib
{
    public class ParametroItem
    {
        public string Chiave { get; }
        public string Valore { get; }
        public bool IsVisibile { get; }

        public ParametroItem(string chiave, string valore, bool visibile)
        {
            this.Chiave = chiave;
            this.Valore = valore;
            this.IsVisibile = visibile;
        }
    }


    public class ListaParametri : Dictionary<string, ParametroItem>
    {
    }
}