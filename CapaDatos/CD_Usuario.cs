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
    public class CD_Usuario
    {
        /* LISTAR USUARIOS CLIENTES */
        public List<Usuario> Listar()
        {
            List<Usuario> lista = new List<Usuario>();

            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.CadenaConexion))
                {
                    string query = "select idUsuario,nombres,apellidos,cargoEmpleado,razonSocial,ruc,email,clave,restablecer,activo,r.idRol,r.rol from Usuario u inner join Rol r on u.idRol = r.idRol where u.idRol = 2";
                    
                    SqlCommand cmd = new SqlCommand(query, oconexion);
                    cmd.CommandType = CommandType.Text;

                    oconexion.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(
                                new Usuario()
                                {
                                    idUsuario = Convert.ToInt32(reader["idUsuario"]),
                                    nombres = reader["nombres"].ToString(),
                                    apellidos = reader["apellidos"].ToString(),
                                    cargoEmpleado = reader["cargoEmpleado"].ToString(),
                                    razonSocial = reader["razonSocial"].ToString(),
                                    ruc = reader["ruc"].ToString(),
                                    email = reader["email"].ToString(),
                                    clave = reader["clave"].ToString(),
                                    restablecer = Convert.ToBoolean(reader["restablecer"]),
                                    activo = Convert.ToBoolean(reader["activo"]),
                                    oRolc = new Rol() { idRol = Convert.ToInt32(reader["idRol"]), rol = reader["rol"].ToString() }
                                });
                        }
                    }
                }
            }
            catch 
            {
                lista = new List<Usuario>();
            }

            return lista;
        }

        /* LISTAR USUARIOS ADMIN */
        public List<Usuario> ListarAdmins()
        {
            List<Usuario> lista = new List<Usuario>();

            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.CadenaConexion))
                {
                    string query = "select idUsuario,nombres,apellidos,cargoEmpleado,razonSocial,ruc,email,clave,restablecer,activo,r.idRol,r.rol from Usuario u inner join Rol r on u.idRol = r.idRol where u.idRol = 1";

                    SqlCommand cmd = new SqlCommand(query, oconexion);
                    cmd.CommandType = CommandType.Text;

                    oconexion.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(
                                new Usuario()
                                {
                                    idUsuario = Convert.ToInt32(reader["idUsuario"]),
                                    nombres = reader["nombres"].ToString(),
                                    apellidos = reader["apellidos"].ToString(),
                                    cargoEmpleado = reader["cargoEmpleado"].ToString(),
                                    razonSocial = reader["razonSocial"].ToString(),
                                    ruc = reader["ruc"].ToString(),
                                    email = reader["email"].ToString(),
                                    clave = reader["clave"].ToString(),
                                    restablecer = Convert.ToBoolean(reader["restablecer"]),
                                    activo = Convert.ToBoolean(reader["activo"]),
                                    oRolc = new Rol() { idRol = Convert.ToInt32(reader["idRol"]), rol = reader["rol"].ToString() }
                                });
                        }
                    }
                }
            }
            catch
            {
                lista = new List<Usuario>();
            }

            return lista;
        }

        /* ENCONTRAR EL TIPO DE USUARIO Y LOGEARSE */
        public Usuario UsuarioLogeo(string correo)
        {
            Usuario usuario = null;

            SqlConnection oconexion = new SqlConnection(Conexion.CadenaConexion);
            SqlCommand cmd = default(SqlCommand);

            try
            {
                cmd = new SqlCommand();
                cmd.Connection = oconexion;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_usuarios_completos_por_correo";
                cmd.Parameters.Clear();

                cmd.Parameters.Add("@correo", SqlDbType.VarChar).Value = correo;

                cmd.Connection.Open();

                SqlDataReader Reader = cmd.ExecuteReader();
                if (Reader.HasRows)
                {
                    int idUsuario = Reader.GetOrdinal("idUsuario");
                    int nombres = Reader.GetOrdinal("nombres");
                    int apellidos = Reader.GetOrdinal("apellidos");
                    int cargoEmpleado = Reader.GetOrdinal("cargoEmpleado");
                    int razonSocial = Reader.GetOrdinal("razonSocial");
                    int ruc = Reader.GetOrdinal("ruc");
                    int email = Reader.GetOrdinal("email");
                    int clave = Reader.GetOrdinal("clave");
                    int confirmarClave = Reader.GetOrdinal("confirmarClave");
                    int restablecer = Reader.GetOrdinal("restablecer");
                    int activo = Reader.GetOrdinal("activo");
                    int idRol = Reader.GetOrdinal("idRol");
                    int rol = Reader.GetOrdinal("rol");

                    if (Reader.Read())
                    {
                        Rol identificador = new Rol();
                        usuario = new Usuario();
                        usuario.idUsuario = Reader.GetInt32(idUsuario);
                        usuario.nombres = Reader.GetString(nombres);
                        usuario.apellidos = Reader.GetString(apellidos);
                        usuario.cargoEmpleado = Reader.GetString(cargoEmpleado);
                        usuario.razonSocial = Reader.GetString(razonSocial);
                        usuario.ruc = Reader.GetString(ruc);
                        usuario.email = Reader.GetString(email);
                        usuario.clave = Reader.GetString(clave);
                        usuario.confirmarClave = Reader.GetString(confirmarClave);
                        usuario.restablecer = Reader.GetBoolean(restablecer);
                        usuario.activo = Reader.GetBoolean(activo);
                        //usuario.oRolc.idRol = Reader.GetInt32(idRol);
                        //usuario.oRolc.rol = Reader.GetString(rol);
                        usuario.oRolc = new Rol() { idRol = Reader.GetInt32(idRol), rol = Reader.GetString(rol) };
                    }
                }
            }
            catch (Exception e)
            {

                usuario = new Usuario();
            }
            return usuario;
        }


        /* REGISTRAR USUARIOS */

        public int RegistrarUsuario(Usuario obj, out string Mensaje)
        {
            int idautogenerado = 0;

            Mensaje = string.Empty;

            try
            {
                using (SqlConnection conexion = new SqlConnection(Conexion.CadenaConexion))
                {
                    SqlCommand cmd = new SqlCommand("sp_RegistrarUsuario", conexion);
                    cmd.Parameters.AddWithValue("nombres", obj.nombres);
                    cmd.Parameters.AddWithValue("apellidos", obj.apellidos);
                    cmd.Parameters.AddWithValue("cargoEmpleado", obj.cargoEmpleado);
                    cmd.Parameters.AddWithValue("razonSocial", obj.razonSocial);
                    cmd.Parameters.AddWithValue("ruc", obj.ruc);
                    cmd.Parameters.AddWithValue("email", obj.email);
                    cmd.Parameters.AddWithValue("clave", obj.clave);
                    cmd.Parameters.AddWithValue("confimar_clave", obj.confirmarClave);
                    cmd.Parameters.AddWithValue("activo", obj.activo);
                    cmd.Parameters.AddWithValue("idRol", obj.oRolc.idRol);
                    cmd.Parameters.Add("resultado", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("mensaje", SqlDbType.VarChar, 500).Direction = ParameterDirection.Output;
                    cmd.CommandType = CommandType.StoredProcedure;

                    conexion.Open();
                    cmd.ExecuteNonQuery();
                    idautogenerado = Convert.ToInt32(cmd.Parameters["resultado"].Value);
                    Mensaje = cmd.Parameters["mensaje"].Value.ToString();

                }
            }
            catch (Exception e)
            {
                idautogenerado = 0;
                Mensaje = e.Message;
            }
            return idautogenerado;
        }

        /* EDITAR USUARIOS */

        public bool EditarProducto(Usuario obj, out string Mensaje)
        {
            bool resultado = false;
            Mensaje = string.Empty;

            try
            {
                using (SqlConnection conexion = new SqlConnection(Conexion.CadenaConexion))
                {
                    SqlCommand cmd = new SqlCommand("sp_EditarUsuario", conexion);
                    cmd.Parameters.AddWithValue("Idusuario", obj.idUsuario);
                    cmd.Parameters.AddWithValue("nombres", obj.nombres);
                    cmd.Parameters.AddWithValue("apellidos", obj.apellidos);
                    cmd.Parameters.AddWithValue("cargoEmpleado", obj.cargoEmpleado);
                    cmd.Parameters.AddWithValue("razonSocial", obj.razonSocial);
                    cmd.Parameters.AddWithValue("ruc", obj.ruc);
                    cmd.Parameters.AddWithValue("email", obj.email);
                    cmd.Parameters.AddWithValue("activo", obj.activo);
                    cmd.Parameters.AddWithValue("idRol", obj.oRolc.idRol);
                    cmd.Parameters.Add("resultado", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("mensaje", SqlDbType.VarChar, 500).Direction = ParameterDirection.Output;
                    cmd.CommandType = CommandType.StoredProcedure;

                    conexion.Open();
                    cmd.ExecuteNonQuery();
                    resultado = Convert.ToBoolean(cmd.Parameters["resultado"].Value);
                    Mensaje = cmd.Parameters["mensaje"].Value.ToString();
                }
            }
            catch (Exception e)
            {
                resultado = false;
                Mensaje = e.Message;
            }
            return resultado;
        }

        /* ELIMINAR USUARIOS */

        public bool EliminarUsuario(int id, out string Mensaje)
        {
            bool resultado = false;
            Mensaje = string.Empty;
            try
            {
                using (SqlConnection conexion = new SqlConnection(Conexion.CadenaConexion))
                {
                    SqlCommand cmd = new SqlCommand("delete top (1) from Usuario where idUsuario = @id", conexion);
                    //SqlCommand cmd = new SqlCommand("update Usuario set activo = 0 where idUsuario = @id", conexion);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.CommandType = CommandType.Text;
                    conexion.Open();
                    //string q = cmd.ExecuteNonQuery().ToString();
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

        /* CAMBIAR CLAVE USUARIOS */

        public bool CambiarClave(int idusuario, string nuevaClave, out string Mensaje)
        {
            bool resultado = false;
            Mensaje = string.Empty;
            try
            {
                using (SqlConnection conexion = new SqlConnection(Conexion.CadenaConexion))
                {
                    SqlCommand cmd = new SqlCommand("update Usuario set clave = @nuevaClave, confirmarClave = @nuevaClaveConfirmar, restablecer = 0 where idUsuario = @id", conexion);
                    cmd.Parameters.AddWithValue("@id", idusuario);
                    cmd.Parameters.AddWithValue("@nuevaClave", nuevaClave);
                    cmd.Parameters.AddWithValue("@nuevaClaveConfirmar", nuevaClave);
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

        /* RESTABLECER CLAVE USUARIOS */

        public bool RestablecerClave(int idusuario, string clave, out string Mensaje)
        {
            bool resultado = false;
            Mensaje = string.Empty;
            try
            {
                using (SqlConnection conexion = new SqlConnection(Conexion.CadenaConexion))
                {
                    SqlCommand cmd = new SqlCommand("update Usuario set clave = @clave, confirmarClave = @claveConfirmar, restablecer = 1 where idUsuario = @id", conexion);
                    cmd.Parameters.AddWithValue("@id", idusuario);
                    cmd.Parameters.AddWithValue("@clave", clave);
                    cmd.Parameters.AddWithValue("@claveConfirmar", clave);
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
