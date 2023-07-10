using InterfazRBR;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RobotBattleRoyale
{
    public class Board
    {
        //Patrón singleton para evitar el instanciamiento de múltiples tableros. Solo hay uno por partida
        #region  Singleton
        private static Board _instance;
        public static Board Instance()
        {
            if (_instance == null)
            {
                _instance = new Board();
            };
            return _instance;
        }
        #endregion
        // Enumerado que almacena los posibles tipos de tableros a generar
        public enum BoardTypes { Rectangle, Square };
        /// <summary>
        /// /Struct que almacena tanto el grid del tablero como sus especificaciones más importantes.
        /// </summary>
        public struct BoardSpecs
        {
            public IPlaygroundElement[,] board;
            public int x, y, barrierX, barrierY, pawnNumber, obstacleNumber;
            public BoardTypes type;

        }
        // Especificaciones de mi tablero
        private static BoardSpecs currentSpecs;
        // Getter de mis especificaciones, no interesa que se puedan cambiar.
        public BoardSpecs CurrentSpecs
        {
            get { return currentSpecs; }
        }
        // Se genera un grid de dimensiones aleatorias
        public void RandomBoardDimensions()
        {
            //Se recoge el minuto actual de la hora
            int t = DateTime.Now.Minute;
            //Se divide ese minuto entre dos para saber si es par 
            int time = (int)t % 2;
            // Se establecen las dimensiones de tablero (esto a veces entra en STACK OVERFLOW)
            CalculateX();
            //La dimensión Y depende de si el programa se ejecuta en un minuto par o impar y  de la dimension X 
            currentSpecs.y = time > 0 ? currentSpecs.x : currentSpecs.x - 2;
            //Creación del tablero
            IPlaygroundElement[,] board = new IPlaygroundElement[currentSpecs.x, currentSpecs.y];
            //Check si es un rectángulo y asignación del tipo de tablero creado a las especificaciones del mismo
            currentSpecs.type = currentSpecs.x != currentSpecs.y ? BoardTypes.Rectangle : BoardTypes.Square;
            // Switch según el tipo de rectángulo para asignar el resto de especificaciones correcpondientes al tablero generado
            switch (currentSpecs.type)
            {
                case BoardTypes.Rectangle:
                    //Se define la total cantidad de peones
                    currentSpecs.pawnNumber = 20;
                    // Se define la cantidad de obstáculo que se generarán en función de la altura del grid
                    currentSpecs.obstacleNumber = 2 * currentSpecs.y;
                    // Se establece  la altura de la barrera en X
                    currentSpecs.barrierX = currentSpecs.x / 4;
                    break;
                case BoardTypes.Square:
                    //Se define la total cantidad de peones
                    currentSpecs.pawnNumber = 16;
                    // Se define la cantidad de obstáculos que se generarán en función de la algura del grid
                    currentSpecs.obstacleNumber = currentSpecs.x + currentSpecs.y;
                    // Se establece la barrera horizontal
                    currentSpecs.barrierX = currentSpecs.x / 2;
                    // Se establece la barrera vertical
                    currentSpecs.barrierY = currentSpecs.y / 2;
                    break;
            }
            //Debug
            Console.WriteLine("(Minute " + t + ")--BOARD CREATED--(" + currentSpecs.type + " = " + currentSpecs.x + "X" + currentSpecs.y + ")");
            currentSpecs.board = board;
        }
        /// Calcular la dimension en X del grid en la generación aleatoria de tablero.
        private void CalculateX()
        {
            Random rnd = new Random();
            currentSpecs.x = (int)rnd.Next(10, 30);
            if (currentSpecs.x % 2 != 0) CalculateX();
        }
        //Métodos publicos
        #region Public Methods
        /// <summary>
        /// Pintar tablero: una vez relleno, se pinta el tablero.
        /// </summary>
        /// 
        public void PrintBoard()
        {
            int contadorDeLinea = 0;
            for (int i = 0; i < currentSpecs.x; i++)
            {
                for (int j = 0; j < currentSpecs.y; j++)
                {
                    IPlaygroundElement cell = currentSpecs.board[i, j];
                    bool emptyCell = cell != null ? false : true;

                    if (!emptyCell)
                    {
                        if (cell.GetType().Name == "Obstacle") Console.Write("O");
                        else if (cell.GetType().Name == "Pawn")
                        {
                            Pawn pawn = (Pawn)cell;
                            if (pawn.Team == Pawn.TeamColor.Blue) Console.Write("B");
                            else if (pawn.Team == Pawn.TeamColor.Red) Console.Write("R");
                        }
                    }
                    else Console.Write("_");

                    if (contadorDeLinea < currentSpecs.y - 1)
                        contadorDeLinea++;
                    else
                    {
                        contadorDeLinea = 0;
                        Console.WriteLine();
                    }
                }
            }
        }
        //Comprobar que la posición se encuentra dentro del tablero generado
        public bool ValidPosition(Position pos)
        {
            bool xInBoard = pos.x > 0 && pos.x < currentSpecs.x;
            bool yInBoard = pos.y > 0 && pos.y < currentSpecs.y;
            bool validPosition = (xInBoard && yInBoard);
            if (!validPosition) Console.WriteLine("MOVIMIENTO PERDIDO: La posición está fuera del tablero");
            return (xInBoard && yInBoard);
        }
        //Comprobar que la posición corresponde a una casilla vacía dentro del tablero generado
        public bool EmptyCell(Position pos)
        {
            bool emptyCell = currentSpecs.board[pos.x, pos.y] == null ? true : false;
            if (emptyCell) Console.WriteLine("La casilla elegida está vacía, moviendo peón");
            return emptyCell;
        }
        //Comprobar que la posición corresponde a una casilla con un Peón dentro del tablero generado
        public bool IsPawn(Position pos)
        {
            return currentSpecs.board[pos.x, pos.y] is Pawn;
        }
        //Obtener el equipo que corresponde al peon que se encuentra en la posición facilitada dentro del tablero generado
        public Pawn.TeamColor GetPawnTeamColor(Position pos)
        {
            Pawn targetPawn = (Pawn)currentSpecs.board[pos.x, pos.y];
            return targetPawn.Team;
        }
        //Obtener la vida que corresponde al peon que se encuentra en la posición facilitada dentro del tablero generado
        public ushort GetPawnLife(Position pos)
        {
            Pawn targetPawn = (Pawn)currentSpecs.board[pos.x, pos.y];
            return targetPawn.Life;
        }
        //Comprobar que la posición corresponde a una casilla con un Obstáculo dentro del tablero generado
        public bool IsObstacle(Position pos)
        {
            bool isObstacle = currentSpecs.board[pos.x, pos.y] is Obstacle;
            if (isObstacle) Console.WriteLine("LA casilla elegida es un obstáculo");
            return currentSpecs.board[pos.x, pos.y] is Obstacle ? true : false;
        }
        // Actualizar la posición de un peón en el tablero
        public void DoMovePawn(Position initialPosition, Position targetPosition) 
        {
            currentSpecs.board[targetPosition.x, targetPosition.y] = currentSpecs.board[initialPosition.x, initialPosition.y];
            currentSpecs.board[initialPosition.x, initialPosition.y] = null;
            //Se pinta tablero con posición actualizada
            PrintBoard();
        }
        public bool CheckRectLineBetween(Position initialPosition, Position targetPosition) 
        {
            string alignedAxis;
            int stepsBetween;
            if (initialPosition.x != targetPosition.x && initialPosition.y != targetPosition.y) 
            {
                Console.WriteLine("MOVIMIENTO PERDIDO: los peones no están alineados en ninún eje");
            }
            else
            {
                alignedAxis = initialPosition.x - targetPosition.x == 0 ? "x" : "y";
                switch (alignedAxis) 
                {
                    case "x":
                        stepsBetween = targetPosition.y - initialPosition.y;
                        for (int i = 0; i < stepsBetween; i++)
                        {
                            if (currentSpecs.board[targetPosition.x, targetPosition.y + (i + 1)] is Obstacle)
                            {
                                Console.WriteLine("MOVIMIENTO PERDIDO: se ha encontrado un obstáculo en el camino");
                                return false;
                            }
                            else return true;
                        }

                        break;
                    case "y":
                        stepsBetween = targetPosition.x - initialPosition.x;
                        for (int i = 0; i < stepsBetween; i++)
                        {
                            if (currentSpecs.board[targetPosition.x+(i+1), targetPosition.y] is Obstacle)
                            {
                                Console.WriteLine("MOVIMIENTO PERDIDO: se ha encontrado un obstáculo en el camino");
                                return false;
                            }
                            else return true;
                        }
                        break;
                }
                
            }
            return true;
        }
        #endregion
    }
}
