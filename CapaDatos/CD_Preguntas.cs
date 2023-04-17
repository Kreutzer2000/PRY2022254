using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.SqlClient;
using System.Data;

namespace CapaDatos
{
    public class CD_Preguntas
    {
        public List<Preguntas> ListarPreguntas()
        {
            List<Preguntas> lista = new List<Preguntas>();
            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.CadenaConexion))
                {
                    string query = "select idPregunta, pregunta from Preguntas";
                    SqlCommand cmd = new SqlCommand(query, oconexion);
                    cmd.CommandType = CommandType.Text;
                    oconexion.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(
                                new Preguntas()
                                {
                                    idPregunta = Convert.ToInt32(reader["idPregunta"]),
                                    pregunta = reader["pregunta"].ToString()
                                });
                        }
                    }
                }
            }
            catch
            {
                lista = new List<Preguntas>();
            }
            return lista;
        }
    }
}
