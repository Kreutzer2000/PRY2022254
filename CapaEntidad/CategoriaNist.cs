using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEntidad
{
    public class CategoriaNist
    {
        public int idCategoria { get; set; }
        public FuncionNist oFuncion { get; set; }
        public string codigo { get; set; }
        public string categoria { get; set; }
        public string comentario { get; set; }
    }
}
