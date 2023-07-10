using System;
using System.Linq;
using System.Reflection;
using InterfazRBR;

namespace RobotBattleRoyale
{
    /// <summary>
    /// Clase que se encarga de gestionar la carga de los pulgins. Estos deben heredar de RobotPlugin para ser cargados.
    /// Así mismo, deben implementar los métodos abstractos pertinentes para permitir funcionar la lógica del juego.
    /// </summary>
    static class GestorPlugIns
    {
        public static bool Verbose { get; set; } = false;

        public static RobotPlugin LoadPlayerDll(string str, Pawn.TeamColor team, RobotPlugin.PawnRolePhases[] pawnRoleList, int boardDimensionX, int boardDimensionY)
        {
            Assembly asm = null;
            Type tipo = null;

            try
            {
                //Cargamos una librería externa al proyecto con LoadFrom.
                asm = Assembly.LoadFrom(str);
            }
            catch { Console.WriteLine("La dll " + str + " no fue encontrada en el directorio de ejecución."); }

            try
            {

                foreach (Type t in asm.GetTypes())
                {
                    //Buscamos una clase de tipo RobotPlugin
                    //Si hay varias, nos quedamos con la primera.
                    //Queda como responsabilidad del alumno no entregar una dll con más de una clase RobotPlugin.
                    if (t.IsSubclassOf(typeof(RobotPlugin)))
                    {
                        tipo = t;
                        break;
                    }
                }
            }
            catch { Console.WriteLine("No se ha encontrado una clase que implemente al interfaz IRobotPlugin en la dll."); }

            //Si encontramos esa clase:
            if (tipo != null)
            {
                //Si está en modo Verbose, imprimimos todos los datos del atributo de clase, si no solo el pseoudónimo.
                try
                {
                    foreach (Attribute atrb in tipo.GetCustomAttributes())
                    {
                        RobotInfoAttribute robotInfo = atrb as RobotInfoAttribute;
                        if (robotInfo != null)
                        {
                            if (!Verbose)
                                Console.WriteLine("El equipo " + robotInfo.TeamName + " grita:\n" + robotInfo.BattleCry);
                            else
                                Console.WriteLine("El equipo " + robotInfo.TeamName + ", del alumno " + robotInfo.StudentName + ", grita:\n" + robotInfo.BattleCry);
                        }
                    }
                }
                catch { Console.WriteLine("Error en la carga de los atributos de la clase del plug-in"); }
                //Se crea una instancia al objeto, necesaria para llamar al método en un contexto.
                try
                {
                    //Crear instancia pasandole los parámetros del constructor.
                    return Activator.CreateInstance(tipo, new object[] { team, pawnRoleList, boardDimensionX, boardDimensionY }) as RobotPlugin;
                }
                catch { Console.WriteLine("Error al crear la instancia de la clase. Los parámetros no son correctos."); }
            }

            return null;
        }
    }
}
