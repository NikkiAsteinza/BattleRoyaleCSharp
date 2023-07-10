using InterfazRBR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotBattleRoyale
{
    class PawnRolePhaseManager
    {
        //Se diseña con un patrón singleton porque sino puede haber más de un tablero, no deberían existir multiples Managers
        #region Singleton
        private static PawnRolePhaseManager _instance;

        public static PawnRolePhaseManager Instance()
        {
            if (_instance == null)
            {
                _instance = new PawnRolePhaseManager();
            }
            return _instance;
        }
        #endregion

        //Se declaran las posiciones inicales y objetivo para cada movimiento
        Position initialPosition;
        Position targetPosition;

        #region Private Methods
        /// <summary>
        /// Se comprueba que las posiciones que conforman la tupla estén a un máximo de una casilla de distancia.
        /// </summary>
        /// <param name="TuplaAccion">Tupla con la posición de partida y la posición objetivo</param>
        /// <returns></returns>
        private bool ComprobarUnaCasillaDeDistancia(Tuple<Position, Position> TuplaAccion)
        {
            bool xStep = Math.Abs(targetPosition.x - TuplaAccion.Item1.x) <= 1;
            bool yStep = Math.Abs(targetPosition.y - TuplaAccion.Item1.y) <= 1;
            if (!xStep & yStep)
            {
                Console.WriteLine("MOVIMIENTO PERDIDO: no puedes moverte a más de una casilla de distancia");
            }
            return (xStep & yStep);
        }
        /// <summary>
        /// Hacer daño a un peón
        /// </summary>
        /// <param name="gamePhaseManager">El manager de las fases, se obtienen las listas de elementos escudados</param>
        /// <param name="board">Grid del tablero donde se encuentran los Pawns</param>
        /// <param name="TuplaAccion">Tupla con la posición de partida y la posición objetivo</param>
        private void SetDamage(GamePhaseManager gamePhaseManager, IPlaygroundElement[,] board, Tuple<Position, Position> TuplaAccion)
        {
            Pawn initialPawn = (Pawn)board[TuplaAccion.Item1.x, TuplaAccion.Item1.y];
            Pawn targetPawn = (Pawn)board[TuplaAccion.Item2.x, TuplaAccion.Item2.y];

            bool shieldedPawn=gamePhaseManager.CheckPawnShielded(initialPawn,targetPosition);
            
            int shieldedDamage = initialPawn.Damage - 1;
            ushort Damage = shieldedPawn ? (ushort)shieldedDamage : initialPawn.Damage;
            if (shieldedPawn) Console.WriteLine("El pawn estaba siendo escudado, pasa menos daño");
            targetPawn.DealDamage(initialPawn.Damage);
        }
        /// <summary>
        /// Hacer daño a un peón a una casilla de distancia
        /// </summary>
        /// <param name="gamePhaseManager">El manager de las fases, se obtienen las listas de elementos escudados</param>
        /// <param name="tablero">Objeto que contiene el grid donde se encuentran los Pawns</param>
        /// <param name="TuplaAccion">Tupla con la posición de partida y la posición objetivo</param>
        private void OneStepDamage(GamePhaseManager gamePhaseManager,Board tablero, Tuple<Position, Position> TuplaAccion)
        {
            Pawn initialPawn = (Pawn)tablero.CurrentSpecs.board[initialPosition.x, initialPosition.y];
            Pawn targetPawn = (Pawn)tablero.CurrentSpecs.board[targetPosition.x, targetPosition.y];
            if (targetPawn.Life > 0)
            {
                //Extraemos la informacion del grid del tablero
                IPlaygroundElement[,] board = tablero.CurrentSpecs.board;
                //Comprobar que solo se avanza una casilla
                bool oneStep = ComprobarUnaCasillaDeDistancia(TuplaAccion);
                if (oneStep)
                {
                    //Comprobar si la casilla no está vacía
                    if (!tablero.EmptyCell(targetPosition))
                    {
                        //Comprobar si la casilla está ocupada por un peón
                        if (tablero.IsPawn(targetPosition))
                        {
                            //Comprobar si los peones de ambas casillas son de distintos equipos
                            if (tablero.GetPawnTeamColor(targetPosition) != tablero.GetPawnTeamColor(initialPosition))
                            {
                                // Comprobar si el pawnlife es mayor que cero
                                if (targetPawn.Life > 0)
                                {
                                     SetDamage(gamePhaseManager,board, TuplaAccion);
                                }
                            }
                        }
                        // Comprobar si la casilla está ocupada por un obstáculo
                        if (tablero.IsObstacle(targetPosition))
                        {
                            //Creación de variable de tipo Obstacle para poder acceder a los métodos tras el casteo
                            Obstacle targetObstacle = (Obstacle)tablero.CurrentSpecs.board[targetPosition.x, targetPosition.y];
                            //Comprobar si el obstáculo está vivo
                            if (targetObstacle.Life == 0)
                            {

                                targetObstacle.DealDamage(initialPawn.Damage);
                            }
                        }
                    }
                    // Si la casilla no contiene nada
                    else
                    {
                        board[targetPosition.x, targetPosition.y] = board[TuplaAccion.Item1.x, TuplaAccion.Item1.y];
                        board[TuplaAccion.Item1.x, TuplaAccion.Item1.y] = null;
                        //Se actualiza el tablero 
                        tablero.PrintBoard();
                    }

                }
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Ejecutar la acción correspondiente a la PawnRolePhase actual
        /// </summary>
        /// <param name="gamePhaseManager">Manager que gestiona las fases del juego</param>
        /// <param name="currentPhase">PawnRolePhase actual</param>
        /// <param name="tablero">Elemento que contiene el gris de posiciones, propiedades y métodos</param>
        /// <param name="TuplaAccion">Tupla con la posición de partida y la posición objetivo</param>
        /// <param name="actionPositionList">Lista de tuplas de acción proporcionada por el plugin</param>
        public void ExecutePhaseAction(GamePhaseManager gamePhaseManager,RobotPlugin.PawnRolePhases currentPhase, Board tablero, Tuple<Position, Position> TuplaAccion, List<Tuple<Position, Position>> actionPositionList) 
        {
            //Posición de inicio del movimiento
            initialPosition = TuplaAccion.Item1;
            //Posición de destino
            targetPosition = TuplaAccion.Item2;
            //Peon que ejecuta el movimiento
            Pawn initialPawn;
            //Extraemos la informacion del grid del tablero
            IPlaygroundElement[,] board = tablero.CurrentSpecs.board;
            //Comprobar si la posición de inicio del movimiento es una casilla Pawn
            if (!tablero.IsPawn(initialPosition))
            {
                Console.WriteLine("MOVIEMIENTO PERDIDO: La posición inicial del movimiento tiene que corresponder a un peón.");
                //Early exit. Sin no lo es se pierde movimiento
                return;
            }
            // Si es una casilla pawn le asignamos su valor a la variable initialPawn
            else
            {
                initialPawn = (Pawn)tablero.CurrentSpecs.board[initialPosition.x, initialPosition.y];
            }
            //Comprobar que la posición de destino se encuentra dentro del tablero
            if (tablero.ValidPosition(targetPosition))
            {
                //Switch según la fase en la que estemos
                switch (currentPhase)
                {
                    case RobotPlugin.PawnRolePhases.Archery:
                        // Si las posiciones están alineadas en alguno de sus ejes (no diagonales)
                        if (tablero.CheckRectLineBetween(initialPosition, targetPosition))
                        {
                            //Se crea la variable que contiene el peon obetivo para poder acceder a los métodos de la clase Pawn
                            Pawn archeryTargetPawn = (Pawn)tablero.CurrentSpecs.board[targetPosition.x, targetPosition.y];
                            //Si está vivo
                            if (archeryTargetPawn.Life > 0)
                            {
                                //Hacemos daño
                                SetDamage(gamePhaseManager,board,TuplaAccion);
                                Console.WriteLine("El peón "+targetPosition+" del equipo:"+archeryTargetPawn.Team+"ha sido dañado");
                            }
                        }
                        break;
                    case RobotPlugin.PawnRolePhases.Damage:
                        //Hacemos daño a una casilla de distanca
                        if (board[targetPosition.x, targetPosition.y] is Pawn) OneStepDamage(gamePhaseManager, tablero, TuplaAccion);
                        else  tablero.DoMovePawn(initialPosition, targetPosition);
                        break;
                    case RobotPlugin.PawnRolePhases.Harakiri:
                        //Si la lista de posiciones no está vacía
                        if (actionPositionList != null)
                        {
                            //Se crea la variable que contiene el peon objetivo para poder acceder a los métodos de la clase Pawn
                            Pawn harakiriTargetPawn = (Pawn)board[initialPosition.x, initialPosition.y];
                            //Se hace daño
                            Console.WriteLine("El peón " + targetPosition + " del equipo:" + harakiriTargetPawn.Team + "ha sido dañado");
                            harakiriTargetPawn.DealDamage(harakiriTargetPawn.Damage);
                        }
                        break;
                    case RobotPlugin.PawnRolePhases.Heal:
                        //Si las posiciones están alineadas en alguno de sus ejes (no diagonales)
                        if (tablero.CheckRectLineBetween(initialPosition, targetPosition))
                        {
                            //Se crea la variable que contiene el peon objetivo para poder acceder a los métodos de la clase Pawn
                            Pawn healingTargetPawn = (Pawn)tablero.CurrentSpecs.board[targetPosition.x, targetPosition.y];
                            //Se cura
                            ushort healingValue = 10;
                            healingTargetPawn.Heal(healingValue);
                            Console.WriteLine("El peón " + targetPosition + " del equipo:" + healingTargetPawn.Team + "ha sido curado");
                        }
                        break;
                }
            }
          
        }
        #endregion
    }

}
