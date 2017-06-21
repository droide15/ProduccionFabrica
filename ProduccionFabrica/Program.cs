using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace ProduccionFabrica
{
    class Program
    {
        static void Main(string[] args)
        {
            string cs = @"server=mysql4.gear.host;userid=programamestadb;
            password=Ot0S2-PVBA~A;database=programamestadb";

            MySqlConnection conn = null;

            //lista de tablas
            List<string> tablas = new List<string>();

            connect(cs, ref conn);
            leerTablas(ref conn, tablas);

            //###valores de ejemplo###
            List<string> atribs = new List<string>();
            List<string> tipos = new List<string>();
            string tabla;

            //pantalla de bienvenida y menu de opciones

            //menu de acceso

            //menu de consultas
            atribs.Clear();
            tipos.Clear();

            Console.WriteLine("---seleccione tipo de consulta---");
            Console.WriteLine("1) consultar estructura de la base de datos");
            Console.WriteLine("2) consultar datos de una tabla");
            char opcion_consultas = Console.ReadKey().KeyChar;
            Console.WriteLine("");
            switch (opcion_consultas)
            {
                case '1':
                    mostrarTablas(ref conn, tablas);
                    break;
                case '2':
                    Console.WriteLine("---seleccione una tabla---");
                    for (int i = 0; i < tablas.Count; i++)
                    {
                        Console.WriteLine(i+1 + ") " + tablas.ElementAt(i));
                    }
                    int opcion_tabla = (int)Char.GetNumericValue(Console.ReadKey().KeyChar) - 1;
                    Console.WriteLine("");
                    tabla = tablas.ElementAt(opcion_tabla);
                    leerAtribs(ref conn, tabla, atribs, tipos);
                    desplegarTabla(ref conn, tabla, atribs, tipos);
                    break;
            }

            //menu de registros
            atribs.Clear();
            tipos.Clear();

            tabla = "produccion";
            //string tabla = "productos";
            List<string> datos = new List<string>();
            leerAtribs(ref conn, tabla, atribs, tipos);
            Console.WriteLine("---seleccione un tipo de operacion---");
            Console.WriteLine("1) insertar datos");
            Console.WriteLine("2) borrar datos");
            char opcion_operacion = Console.ReadKey().KeyChar;
            Console.WriteLine("");
            switch (opcion_operacion)
            {
                case '1':
                    for (int i = 0; i < atribs.Count; i++)
                    {
                        imprimirDato(atribs.ElementAt(i), 20);
                        imprimirDato(tipos.ElementAt(i), 15);
                        Console.Write(":");
                        datos.Add(Console.ReadLine());
                    }
                    insertarDatos(ref conn, tabla, atribs, datos);
                    break;
                case '2':
                    Console.Write("id de " + tabla + ": ");
                    borrarDatos(ref conn, tabla, "id", Console.ReadLine());
                    break;
            }

            close(ref conn);

            Console.ReadKey();
        }

        //abriendo conexion
        protected static bool connect(string cs, ref MySqlConnection conn)
        {
            try
            {
                conn = new MySqlConnection(cs);
                conn.Open();

                string stm = "SELECT VERSION()";
                MySqlCommand cmd = new MySqlCommand(stm, conn);
                string version = Convert.ToString(cmd.ExecuteScalar());
                Console.WriteLine("MySQL version : {0}", version);
                return true;
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error: {0}", ex.ToString());
                close(ref conn);
            }
            return false;
        }

        //cerrando conexion
        protected static void close(ref MySqlConnection conn)
        {
            if (conn != null)
            {
                conn.Close();
            }
        }

        //desplegando datos de una tabla
        protected static void desplegarTabla(ref MySqlConnection conn, string tabla, List<string> atribs, List<string> tipos)
        {
            MySqlDataReader rdr = null;

            //consultando los registros de la tabla
            try
            {
                string stm = "select * from " + tabla;
                MySqlCommand cmd = new MySqlCommand(stm, conn);
                rdr = cmd.ExecuteReader();

                //mostrando los registros de la tabla
                Console.WriteLine("---" + tabla + "---");
                imprimirDato(atribs.ElementAt(0), 10);
                for (int i = 1; i < rdr.FieldCount; i++)
                {
                    imprimirDato(atribs.ElementAt(i), 15);
                }
                Console.WriteLine("");
                imprimirDato(tipos.ElementAt(0), 10);
                for (int i = 1; i < rdr.FieldCount; i++)
                {
                    imprimirDato(tipos.ElementAt(i), 15);
                }
                Console.WriteLine("");
                while (rdr.Read())
                {
                    imprimirDato(rdr[0].ToString(), 10);
                    for (int i = 1; i < rdr.FieldCount; i++)
                    {
                        string value = rdr[i].ToString();

                        string[] format = new string[] { "d/M/yyyy hh:mm:ss tt" };
                        DateTime datetime;
                        if (DateTime.TryParseExact(value, format, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.NoCurrentDateDefault, out datetime))
                            value = datetime.ToShortDateString();

                        imprimirDato(value, 15);
                    }
                    Console.WriteLine("");
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error: {0}", ex.ToString());
            }
            finally
            {
                if (rdr != null)
                {
                    rdr.Close();
                }
            }
        }

        //insertando datos en una tabla
        protected static void insertarDatos(ref MySqlConnection conn, string tabla, List<string> atribs, List<string> datos)
        {
            try
            {
                MySqlCommand cmd = new MySqlCommand();
                List<string> campos = new List<string>();
                for (int i = 0; i < atribs.Count; i++)
                {
                    campos.Add("@" + atribs.ElementAt(i));
                }
                string lista_atribs = string.Join(",", atribs.ToArray());
                string lista_campos = string.Join(",", campos.ToArray());

                cmd.Connection = conn;
                cmd.CommandText = "INSERT INTO " + tabla + "(" + lista_atribs + ") VALUES(" + lista_campos + ")";
                cmd.Prepare();
                for (int i = 0; i < atribs.Count; i++)
                {
                    cmd.Parameters.AddWithValue(campos.ElementAt(i), datos.ElementAt(i));
                }
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error: {0}", ex.ToString());
            }
        }

        //borrando datos de una tabla
        protected static void borrarDatos(ref MySqlConnection conn, string tabla, string campo, string valor)
        {
            try
            {
                MySqlCommand cmd = new MySqlCommand();

                cmd.Connection = conn;
                cmd.CommandText = "DELETE FROM " + tabla + " WHERE " + campo + " = @valor";
                cmd.Prepare();
                cmd.Parameters.AddWithValue("@valor", valor);
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error: {0}", ex.ToString());
            }
        }

        //mostrando las tablas de la base de datos
        protected static void mostrarTablas(ref MySqlConnection conn, List<string> tablas)
        {
            MySqlDataReader rdr = null;

            //mostrando las tablas
            Console.WriteLine("---tablas---");
            foreach (var tabla in tablas)
            {
                Console.WriteLine(tabla);
            }
            
            //iterando sobre las tablas
            foreach (var tabla in tablas)
            {
                //describiendo la tabla
                try
                {
                    string stm = "describe " + tabla;
                    MySqlCommand cmd = new MySqlCommand(stm, conn);
                    rdr = cmd.ExecuteReader();

                    //mostrando la descripcion
                    Console.WriteLine("---" + tabla + "---");
                    while (rdr.Read())
                    {
                        imprimirDato(rdr[0].ToString(), 20);
                        imprimirDato(rdr[1].ToString(), 15);
                        for (int i = 2; i < rdr.FieldCount; i++)
                        {
                            imprimirDato(rdr[i].ToString(), 5);
                        }
                        Console.WriteLine("");
                    }
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine("Error: {0}", ex.ToString());
                }
                finally
                {
                    if (rdr != null)
                    {
                        rdr.Close();
                    }
                }
            }
        }

        //leyendo lista de tablas
        protected static void leerTablas(ref MySqlConnection conn, List<string> tablas)
        {
            MySqlDataReader rdr = null;
            
            //consultando las tablas
            try
            {
                string stm = "show tables";
                MySqlCommand cmd = new MySqlCommand(stm, conn);
                rdr = cmd.ExecuteReader();

                //guardando las tablas
                while (rdr.Read())
                {
                    tablas.Add(rdr[0].ToString());
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error: {0}", ex.ToString());
            }
            finally
            {
                if (rdr != null)
                {
                    rdr.Close();
                }
            }
        }

        //leer atributos y tipos de una tabla
        protected static void leerAtribs(ref MySqlConnection conn, string tabla, List<string> atribs, List<string> tipos)
        {
            MySqlDataReader rdr = null;

            //consultando los atributos de la tabla
            try
            {
                string stm = "describe " + tabla;
                MySqlCommand cmd = new MySqlCommand(stm, conn);
                rdr = cmd.ExecuteReader();

                //guardando atributos y tipos
                while (rdr.Read())
                {
                    atribs.Add(rdr[0].ToString());
                    tipos.Add(rdr[1].ToString());
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error: {0}", ex.ToString());
            }
            finally
            {
                if (rdr != null)
                {
                    rdr.Close();
                }
            }
        }

        //imprimiendo un dato forzando que ocupe tamtmp espacios de caracter
        protected static void imprimirDato(string strtmp, int tamtmp)
        {
            while (strtmp.Length < tamtmp)
                strtmp = strtmp + " ";
            Console.Write(strtmp);
        }
    }
}
