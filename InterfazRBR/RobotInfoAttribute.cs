using System;

namespace InterfazRBR
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class RobotInfoAttribute : Attribute
    {
        public readonly string StudentName; //Nombre y apellidos del alumno (max 80 caracteres).
        public readonly string TeamName;    //Pseudónimo del alumno o nombre del equipo (max 25 caracteres).
        public readonly string BattleCry;   //Grito de batalla (max 80 caracteres).

        public RobotInfoAttribute(string studentName, string teamName, string battleCry = "")
        {
            StudentName = (studentName.Length > 80) ? studentName.Substring(0, 80) : studentName;
            TeamName = (teamName.Length > 25) ? teamName.Substring(0, 25) : teamName;
            BattleCry = (battleCry.Length > 80) ? battleCry.Substring(0, 80) : battleCry;
        }
    }
}
