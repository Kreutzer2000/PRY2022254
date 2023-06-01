using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CapaEntidad;

namespace CapaDatos
{
    public class CD_Sesiones
    {
        /* LISTAR SESIONES DE CLIENTES ACTIVOS */
        public List<ActiveSession> ListarSesiones()
        {
            List<ActiveSession> lista = new List<ActiveSession>();

            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.CadenaConexion))
                {
                    string query = "select correo, llave from Sesiones";

                    SqlCommand cmd = new SqlCommand(query, oconexion);
                    cmd.CommandType = CommandType.Text;

                    oconexion.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(
                                new ActiveSession()
                                {
                                    correoUsuario = reader["correo"].ToString(),
                                    key = reader["llave"].ToString(),
                                });
                        }
                    }
                }
            }
            catch
            {
                lista = new List<ActiveSession>();
            }

            return lista;
        }

        /* LISTAR SESIONES DE CLIENTES ACTIVOS POR EL CORREO */
        public ActiveSession ListarSesiones_Correo(string email)
        {
            ActiveSession sesion = new ActiveSession();

            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.CadenaConexion))
                {
                    using (SqlCommand _sqlcmd = new SqlCommand("sp_ListarSesiones_Correo", oconexion))
                    {
                        oconexion.Open();
                        _sqlcmd.CommandTimeout = 0;
                        _sqlcmd.CommandType = CommandType.StoredProcedure;

                        _sqlcmd.Parameters.Add("@email", SqlDbType.VarChar).Value = email;

                        SqlDataReader Reader = _sqlcmd.ExecuteReader();
                        while (Reader.Read())
                        {
                            //Preguntas pregunta = new Preguntas();
                            ActiveSession objSesion = new ActiveSession()
                            {
                                correoUsuario = Reader["correo"].ToString(),
                                key = Reader["llave"].ToString(),
                            };
                            sesion = objSesion;
                        }
                        oconexion.Close();
                        return sesion;
                    }
                }
            }
            catch
            {
                sesion = new ActiveSession();
            }

            return sesion;
        }

        /* FUNCION PARA GUARDAR LA SESION ACTIVA DEL CLIENTE */
        public int RegistrarSesion(string email, string llave, out string Mensaje)
        {
            int idautogenerado = 0;
            Mensaje = string.Empty;

            try
            {
                using (SqlConnection con = new SqlConnection(Conexion.CadenaConexion))
                {
                    SqlCommand cmd = new SqlCommand("sp_RegistroSesiones", con);
                    cmd.Parameters.AddWithValue("email", email);
                    cmd.Parameters.AddWithValue("llave", llave);
                    cmd.Parameters.Add("mensaje", SqlDbType.VarChar, 500).Direction = ParameterDirection.Output;
                    cmd.CommandType = CommandType.StoredProcedure;

                    con.Open();
                    cmd.ExecuteNonQuery();
                    
                    Mensaje = cmd.Parameters["mensaje"].Value.ToString();
                    idautogenerado = 1;
                }
            }
            catch (Exception e)
            {
                idautogenerado = 0;
                Mensaje = e.Message;
            }
            return idautogenerado;
        }

        /* ELIMINAR SESIONES DE CLIENTES ACTIVOS */
        public bool EliminarSesion(string email, out string Mensaje)
        {
            bool resultado = false;
            Mensaje = string.Empty;
            try
            {
                using (SqlConnection conexion = new SqlConnection(Conexion.CadenaConexion))
                {
                    SqlCommand cmd = new SqlCommand("delete from Sesiones where correo = @correo", conexion);
                    cmd.Parameters.AddWithValue("@correo", email);
                    cmd.CommandType = CommandType.Text;
                    conexion.Open();
                    
                    resultado = cmd.ExecuteNonQuery() > 0 ? true : false;
                }
            }
            catch (Exception e)
            {
                resultado = false;
                Mensaje = e.Message;
            }
            return resultado;
        }

    }
}
