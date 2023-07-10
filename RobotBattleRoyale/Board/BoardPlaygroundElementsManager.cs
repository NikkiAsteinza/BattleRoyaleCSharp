using InterfazRBR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotBattleRoyale
{
    class PG_Elements_Manager
    {
        //Se diseña con un patrón singleton porque sino puede haber más de un tablero, no deberían existir multiples Managers
        #region Singleton
        private static PG_Elements_Manager _instance;

        public static PG_Elements_Manager Instance() 
        {
            if (_instance == null) 
            {
                _instance = new PG_Elements_Manager();
            }
            return _instance;
        }
        #endregion
        // Lista de todos los bots de la partida
        static List<Pawn> TotalPawns;
        public List<Pawn> Pawns { get { return TotalPawns; } }
        // Lista de todos los obstáculos de la partida
        static List<Obstacle> obstacles;
        //Lista de bots separados por equipos.
        static List<List<Pawn>> teams;
        //Estructura que define las especificaciones para cada elemento de tablero
        struct PlayGroundElementsSpecs
        {
            public ushort
                pawnLife, //Vida de los bots
                pawnDamage, //Daño que hacen los bots
                obstacleLife; // Vida de los obstáculos

        }
        //Variable que almacena las especificaciones del tablero actual.
        private PlayGroundElementsSpecs elementsSpecs;
        /// <summary>
        /// Inicializar las especificaciones del tablero
        /// </summary>
        /// <param name="board">Tablero</param>
        public void GenerateElementSpecs(Board board) 
        {
            //Se comprueba el tipo de tablero generado y en función del éste se asignan unos  u otros valores
            switch (board.CurrentSpecs.type)
            {
                case Board.BoardTypes.Rectangle:
                    elementsSpecs.pawnDamage = 30;
                    elementsSpecs.pawnLife = 90;
                    elementsSpecs.obstacleLife = 60;
                    Console.WriteLine("Total Pawns: " + board.CurrentSpecs.pawnNumber + " | " + "Obstacles: " + board.CurrentSpecs.obstacleNumber);
                    break;
                case Board.BoardTypes.Square:

                    elementsSpecs.pawnDamage = 30;
                    elementsSpecs.pawnLife = 60;
                    elementsSpecs.obstacleLife = 30;
                    Console.WriteLine("Total Pawns: " + board.CurrentSpecs.pawnNumber + " | " + "Obstacles: " + board.CurrentSpecs.obstacleNumber);
                    break;
            }
        }
        /// <summary>
        /// Separar los peones en dos equipos
        /// </summary>
        /// <param name="botsNumber">Número total de peones generados</param>
        /// <returns></returns>
        private  List<List<Pawn>> CreateTeams(int botsNumber)
        {
            TotalPawns = new List<Pawn>();
            ///Crear los bots y añadirlos a la lista de bots en partida.
            for (int i = 0; i < (botsNumber); i++)
            {

                if (i < botsNumber / 2)
                {
                    Pawn RedPawn = new Pawn(elementsSpecs.pawnLife, elementsSpecs.pawnDamage, Pawn.TeamColor.Red);
                    TotalPawns.Add(RedPawn);
                }
                else
                {
                    Pawn BluePawn = new Pawn(elementsSpecs.pawnLife, elementsSpecs.pawnDamage, Pawn.TeamColor.Blue);
                    TotalPawns.Add(BluePawn);
                }
            }
            //Crear la lista de peones del equipo Azul
            return DividirEquipos();
        }
        /// <summary>
        /// Creación de equipos a partir del número de peones establecido por el tablero
        /// </summary>
        /// <returns></returns>
        private static List<List<Pawn>> DividirEquipos()
        {
            List<Pawn> BluePawns = SetTeamList(TotalPawns, Pawn.TeamColor.Blue);
            List<Pawn> RedPawns = SetTeamList(TotalPawns, Pawn.TeamColor.Red);
            Console.WriteLine("Blue pawns:" + BluePawns.Count+ " || Red pawns:" + RedPawns.Count);
            List<List<Pawn>> equipos = new List<List<Pawn>>();
            equipos.Add(BluePawns);
            equipos.Add(RedPawns);
            return equipos;
        }
        /// <summary>
        /// Asigna el equipo indicado a los peones de la lista
        /// </summary>
        /// <param name="PawnList">Lista de peones generados</param>
        /// <param name="Team">Equipo</param>
        /// <returns>Lista de elementos de ese equipo</returns>
        static List<Pawn> SetTeamList(List<Pawn> PawnList, Pawn.TeamColor Team)
        {
            List<Pawn> newList = new List<Pawn>();
            foreach (Pawn pawn in PawnList)
            {
                if (pawn.Team == Team) newList.Add(pawn);
            }
            return newList;
        }
        /// <summary>
        /// Inicializar/Asignar las posiciones de los obstaculos en el tablero
        /// </summary>
        /// <param name="tablero"></param>
        private  void LocateObstacles(Board tablero)
        {
            for (int i = 0; i < tablero.CurrentSpecs.obstacleNumber; i++)
            {
                int half = tablero.CurrentSpecs.obstacleNumber / 2;
                switch (tablero.CurrentSpecs.type)
                {
                    case Board.BoardTypes.Rectangle:
                        int aux = tablero.CurrentSpecs.barrierX;
                        if (i < half) tablero.CurrentSpecs.board[aux, i] = obstacles[i];
                        else
                        {
                            int number = i - half;
                            tablero.CurrentSpecs.board[aux* 3, number] = obstacles[i];
                        }
                        break;
                    case Board.BoardTypes.Square:
                        if (i < half)
                        {
                            tablero.CurrentSpecs.board[ i,tablero.CurrentSpecs.barrierY] = obstacles[i];
                        }
                        else
                        {
                            int number =i-half;
                            tablero.CurrentSpecs.board[tablero.CurrentSpecs.barrierX,number] = obstacles[i];
                        }
                        break;
                }
            }
        }
        /// <summary>
        /// Inincializa/Asigna las posiciones de los peones del tablero.
        /// </summary>
        /// <param name="board">Tablero</param>
        private static void LocatePawns(Board board)
        {
            for (int i = 0; i < teams.Count; i++)
            {
                bool firstTeam = i == 0 ? true : false;

                for (int j = 0; j < teams[i].Count; j++)
                {
                    int xPosition;
                    int yPosition = j;
                    if (board.CurrentSpecs.type== Board.BoardTypes.Rectangle)
                    {
                        if (firstTeam) xPosition = 0;
                        else xPosition = board.CurrentSpecs.x - 1;

                        board.CurrentSpecs.board[xPosition, yPosition] = teams[i][j];
                    }
                    else
                    {
                        if (firstTeam) xPosition = 0;
                        else
                        {
                            xPosition = board.CurrentSpecs.x - 1;
                            yPosition = j;
                        }
                    }
                    board.CurrentSpecs.board[xPosition, yPosition] = teams[i][j];
                }
            }
        }
        /// <summary>
        /// Se generan los obstáculos del tablero tablero
        /// </summary>
        /// <param name="obstaclesNumber">Número de obstáculos a generar</param>
        /// <returns></returns>
        private List<Obstacle> CreateObstacles(int obstaclesNumber)
        {
            List<Obstacle> obstacles = new List<Obstacle>();
            for (int i = 0; i < (obstaclesNumber); i++)
            {
                Obstacle newObstacle = new Obstacle(elementsSpecs.obstacleLife);
                obstacles.Add(newObstacle);
            }
            return obstacles;
        }
        /// <summary>
        /// Inicializar los elementos del tablero
        /// </summary>
        /// <param name="tablero">Tablero o Grid</param>
        public void SetupPlaygroundElements(Board tablero) 
        {
            GenerateElementSpecs(tablero);
            teams = CreateTeams(tablero.CurrentSpecs.pawnNumber);
            obstacles = CreateObstacles(tablero.CurrentSpecs.obstacleNumber);
            LocateObstacles(tablero);
            LocatePawns(tablero);

        }

        public bool CheckIfATeamWasFulminated(Board tablero) 
        {
            int pawnsPerTeam = TotalPawns.Count / 2;
            int redPawns = 0;
            int bluePawns = 0;
            for (int x = 0; x < tablero.CurrentSpecs.x; x++) 
            {
                for (int y = 0; y < tablero.CurrentSpecs.y; y++) 
                {
                    Position position = new Position(x,y);
                    if (tablero.IsPawn(position)) 
                    {
                        if (tablero.GetPawnTeamColor(position) == Pawn.TeamColor.Blue)
                            bluePawns++;
                        if (tablero.GetPawnTeamColor(position) == Pawn.TeamColor.Red)
                            redPawns++;
                    }
                }
            }
            if (bluePawns == 0) 
            {
                Console.WriteLine("Los peones del equipo azul han sido exterminados cruelmente");
                Console.WriteLine("Gana el Equipo Rojo");
                return true;
            }
            if (redPawns == 0)
            {
                Console.WriteLine("Los peones del equipo rojo han sido exterminados cruelmente");
                Console.WriteLine("Gana el Equipo Azul");
                return true;
            }
            return false;
        }
    }
}
