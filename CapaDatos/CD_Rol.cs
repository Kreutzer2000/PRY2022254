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
    public class CD_Rol
    {
        public List<Rol> ListarRol()
        {
            List<Rol> lista = new List<Rol>();
            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.CadenaConexion))
                {
                    string query = "select idRol, rol from Rol";

                    SqlCommand cmd = new SqlCommand(query, oconexion);
                    cmd.CommandType = CommandType.Text;

                    oconexion.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(
                                new Rol()
                                {
                                    idRol = Convert.ToInt32(reader["idRol"]),
                                    rol = reader["rol"].ToString()
                                });
                        }
                    }
                }
            }
            catch
            {
                lista = new List<Rol>();
            }

            return lista;
        }
    }
}
