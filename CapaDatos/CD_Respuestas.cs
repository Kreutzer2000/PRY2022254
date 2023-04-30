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
    public class CD_Respuestas
    {
        /* LISTAR RESPUESTAS Y PREGUNTAS */
        public List<RptaPreguntas> ListarRespuesta(int idpregunta)
        {
            List<RptaPreguntas> lista = new List<RptaPreguntas>();
            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.CadenaConexion))
                {
                    using (SqlCommand _sqlcmd = new SqlCommand("sp_Lista_respuestas_preguntas", oconexion))
                    {
                        oconexion.Open();
                        _sqlcmd.CommandTimeout = 0;
                        _sqlcmd.CommandType = CommandType.StoredProcedure;

                        _sqlcmd.Parameters.Add("@idpregunta", SqlDbType.Int).Value = idpregunta;

                        SqlDataReader Reader = _sqlcmd.ExecuteReader();
                        while (Reader.Read())
                        {
                            //Preguntas pregunta = new Preguntas();
                            RptaPreguntas objRptaPreguntas = new RptaPreguntas()
                            {
                                idRptaPregunta = int.Parse(Reader["idRptaPreguntas"].ToString()),
                                respuesta = Reader["respuesta"].ToString(),
                                oPregunta = new Preguntas()
                                {
                                    idPregunta = Convert.ToInt32(Reader["idPregunta"]),
                                    pregunta = Reader["pregunta"].ToString(),
                                    oSubCategoria = new SubCategoriaNist()
                                    {
                                        idSubCategoria = Convert.ToInt32(Reader["idSubCategoria"]),
                                        subCategoria = Reader["subCategoria"].ToString(),
                                        oCategoria = new CategoriaNist()
                                        {
                                            idCategoria = Convert.ToInt32(Reader["idCategoria"]),
                                            categoria = Reader["categoria"].ToString(),
                                            oFuncion = new FuncionNist()
                                            {
                                                idFuncion = Convert.ToInt32(Reader["idFuncion"]),
                                                funcion = Reader["funcion"].ToString()
                                            }
                                        }
                                    }
                                },
                                oFuncionNist = new FuncionNist() { idFuncion = Convert.ToInt32(Reader["idFuncion"]), funcion = Reader["funcion"].ToString() }
                            };

                            lista.Add(objRptaPreguntas);
                        }
                        oconexion.Close();
                        return lista;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                return lista;
            }

        }

        /* LISTAR PREGUNTAS */
        public List<RptaPreguntas> ListarPreguntas()
        {
            List<RptaPreguntas> lista = new List<RptaPreguntas>();
            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.CadenaConexion))
                {
                    using (SqlCommand _sqlcmd = new SqlCommand("sp_Lista_preguntas", oconexion))
                    {
                        oconexion.Open();
                        _sqlcmd.CommandTimeout = 0;
                        _sqlcmd.CommandType = CommandType.StoredProcedure;

                        SqlDataReader Reader = _sqlcmd.ExecuteReader();
                        while (Reader.Read())
                        {
                            //Preguntas pregunta = new Preguntas();
                            RptaPreguntas objRptaPreguntas = new RptaPreguntas()
                            {
                                oPregunta = new Preguntas()
                                {
                                    idPregunta = Convert.ToInt32(Reader["idPregunta"]),
                                    pregunta = Reader["pregunta"].ToString(),
                                    oSubCategoria = new SubCategoriaNist()
                                    {
                                        idSubCategoria = Convert.ToInt32(Reader["idSubCategoria"]),
                                        subCategoria = Reader["subCategoria"].ToString(),
                                        oCategoria = new CategoriaNist()
                                        {
                                            idCategoria = Convert.ToInt32(Reader["idCategoria"]),
                                            categoria = Reader["categoria"].ToString(),
                                            oFuncion = new FuncionNist()
                                            {
                                                idFuncion = Convert.ToInt32(Reader["idFuncion"]),
                                                funcion = Reader["funcion"].ToString()
                                            }
                                        }
                                    }
                                },
                                oFuncionNist = new FuncionNist() { idFuncion = Convert.ToInt32(Reader["idFuncion"]), funcion = Reader["funcion"].ToString() }
                            };

                            lista.Add(objRptaPreguntas);
                        }
                        oconexion.Close();
                        return lista;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                return lista;
            }

        }

        /* LISTA LAS RESPUESTAS PARA EL CLIENTE */
        public List<RptaPreguntas> ListarRespuestas_Cliente()
        {
            List<RptaPreguntas> lista = new List<RptaPreguntas>();
            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.CadenaConexion))
                {
                    string query = "select rpta.idRptaPreguntas, rpta.respuesta, rpta.idPregunta, pr.pregunta, funN.idFuncion, funN.funcion from RptaPreguntas rpta inner join Preguntas pr on rpta.idPregunta = pr.idPregunta inner join SubCategoriaNist subN on pr.idSubCategoria = subN.idSubCategoria inner join CategoriaNist catN on subN.idCategoria = catN.idCategoria inner join FuncionNist funN on catN.idFuncion = funN.idFuncion\r\n";
                    SqlCommand cmd = new SqlCommand(query, oconexion);
                    cmd.CommandType = CommandType.Text;
                    oconexion.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(
                                new RptaPreguntas()
                                {
                                    idRptaPregunta = Convert.ToInt32(reader["idRptaPreguntas"]),
                                    respuesta = reader["respuesta"].ToString(),
                                    oPregunta = new Preguntas()
                                    {
                                        idPregunta = Convert.ToInt32(reader["idPregunta"]),
                                        pregunta = reader["pregunta"].ToString()
                                    },
                                    oFuncionNist = new FuncionNist()
                                    {
                                        idFuncion = Convert.ToInt32(reader["idFuncion"]),
                                        funcion = reader["funcion"].ToString()
                                    }
                                });
                        }
                    }
                }
            }
            catch
            {
                lista = new List<RptaPreguntas>();
            }
            return lista;
        }

        /* LISTA DE RESPUESTAS DE LOS CLIENTES PARA EL FILTRO -> 
         * SE UTILIZA CON EL PROPOSITO DE FILTRAR LAS PREGUNTAS EN EL CONTROLADOR CUESTIONARIO */

        public List<Respuesta> FiltroRespuesta_Cliente()
        {
            List<Respuesta> lista = new List<Respuesta>();
            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.CadenaConexion))
                {
                    string query = "select r.idRespuesta, r.idUsuario, u.email, r.idFuncion_ID, r.idFuncion_PR, r.idFuncion_DE, r.idFuncion_RS, r.idFuncion_RC, r.fechaRegistro \r\n\tfrom Respuesta r inner join Usuario u on r.idUsuario = u.idUsuario";
                    SqlCommand cmd = new SqlCommand(query, oconexion);
                    cmd.CommandType = CommandType.Text;
                    oconexion.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id, pr, de, rs, rc;
                            var idFuncion_ID = Convert.ToString(reader["idFuncion_ID"]);
                            var idFuncion_PR = Convert.ToString(reader["idFuncion_PR"]);
                            var idFuncion_DE = Convert.ToString(reader["idFuncion_DE"]);
                            var idFuncion_RS = Convert.ToString(reader["idFuncion_RS"]);
                            var idFuncion_RC = Convert.ToString(reader["idFuncion_RC"]);

                            if (idFuncion_ID == "")
                            {
                                id = 0;
                            }
                            else
                            {
                                id = Convert.ToInt32(reader["idFuncion_ID"]);
                            }

                            if (idFuncion_PR == "")
                            {
                                pr = 0;
                            }
                            else
                            {
                                pr = Convert.ToInt32(reader["idFuncion_PR"]);
                            }

                            if (idFuncion_DE == "")
                            {
                                de = 0;
                            }
                            else
                            {
                                de = Convert.ToInt32(reader["idFuncion_DE"]);
                            }

                            if (idFuncion_RS == "")
                            {
                                rs = 0;
                            }
                            else
                            {
                                rs = Convert.ToInt32(reader["idFuncion_RS"]);
                            }

                            if (idFuncion_RC == "")
                            {
                                rc = 0;
                            }
                            else
                            {
                                rc = Convert.ToInt32(reader["idFuncion_RC"]);
                            }

                            lista.Add(
                                new Respuesta()
                                {
                                    idRespuesta = Convert.ToInt32(reader["idRespuesta"]),
                                    oUsuario = new Usuario()
                                    {
                                        idUsuario = Convert.ToInt32(reader["idUsuario"]),
                                        email = Convert.ToString(reader["email"])
                                    },
                                    oFuncion_ID = new FuncionNist()
                                    {
                                        idFuncion = id,
                                    },
                                    oFuncion_PR = new FuncionNist()
                                    {
                                        idFuncion = pr,
                                    },
                                    oFuncion_DE = new FuncionNist()
                                    {
                                        idFuncion = de,
                                    },
                                    oFuncion_RS = new FuncionNist()
                                    {
                                        idFuncion = rs,
                                    },
                                    oFuncion_RC = new FuncionNist()
                                    {
                                        idFuncion = rc,
                                    },
                                    fechaRegistro = Convert.ToDateTime(reader["fechaRegistro"])
                                });
                        }
                    }
                }
            }
            catch
            {
                lista = new List<Respuesta>();
            }
            return lista;
        }


    }
}
