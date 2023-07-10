using System;
using System.Collections.Generic;
using System.Configuration;
using InterfazRBR;

namespace RobotBattleRoyale
{
    class Program
    {
        /// <summary>
        /// Inicializar la variable pawn role phases.
        /// </summary>
        /// <param name="pawnRoleList"></param>
        private static  void InitGamePhases(RobotPlugin.PawnRolePhases[] pawnRoleList)
        {
            pawnRoleList[0] = RobotPlugin.PawnRolePhases.Damage;
            pawnRoleList[1] = RobotPlugin.PawnRolePhases.Harakiri;
            pawnRoleList[2] = RobotPlugin.PawnRolePhases.Heal;
            pawnRoleList[3] = RobotPlugin.PawnRolePhases.Archery;
        }

        static void Main(string[] args)
        {    
            //Array de fases del juego
            RobotPlugin.PawnRolePhases[] pawnRoleList = new RobotPlugin.PawnRolePhases[4];
            //Fase actual del juego
            RobotPlugin.PawnRolePhases currentPhase;
            //Se inicializa el array de fases del juego
            InitGamePhases(pawnRoleList);
            //Creación del tablero aleatorio y asignacion de dimensiones a través de la clase Board
            Board tablero = new Board();
            // Inicialización del tablero con dimensiones aleatorias
            tablero.RandomBoardDimensions();
            // Creación del grid del tablero (la tabla)
            IPlaygroundElement[,] board = tablero.CurrentSpecs.board;
            // Se instacia un PlayGround Elements Manager
            PG_Elements_Manager elementsManager = new PG_Elements_Manager();
            // Se rellena el tablero con las configuracion adecuada según el tipo de board generado aleatoriamente
            elementsManager.SetupPlaygroundElements(tablero);
            // Se pinta el tablero
            tablero.PrintBoard();
            // Se instancia un GamePhase Manager para gestionar los distintos elementos de cada fase de la ronda.
            GamePhaseManager gamePhaseManager = new GamePhaseManager();
            // Se instancia un PawnRolePhaseManager para gestionar los movimientos de la fase de acción.
            PawnRolePhaseManager pawnRolePhaseManager = new PawnRolePhaseManager();
            //Se cargan los plugins de los jugadores
            RobotPlugin playerIA1 = GestorPlugIns.LoadPlayerDll("MyPlugIn.dll", Pawn.TeamColor.Red, pawnRoleList, tablero.CurrentSpecs.x, tablero.CurrentSpecs.y);
            RobotPlugin playerIA2 = GestorPlugIns.LoadPlayerDll("", Pawn.TeamColor.Blue, pawnRoleList, tablero.CurrentSpecs.x, tablero.CurrentSpecs.y);

            // Se crea una lista de jugadores
            List<RobotPlugin> players = new List<RobotPlugin>();
            players.Add(playerIA1);
            players.Add(playerIA2);

            //Se comprueba que el plugin se ha cargado correctamente
            if (playerIA1 == null || playerIA2 == null)
            {
                Console.WriteLine("No se pudo cargar el plugin correctamente");
                return;
            }
            ////////////////////////////////////////
            //Búcle principal - Lógica del programa
            //--------------------------------------

            Random rnd = new Random();
            ushort roundAction = (ushort)rnd.Next(1, 5);
            ushort healingAction = (ushort)rnd.Next(1, roundAction);// Curaciones de dos puntos en local y a distancia //puede curar a
            ushort shieldAction = (ushort)rnd.Next(1, 3);

            int roundCount = 0;
            bool someOneTeamDied = elementsManager.CheckIfATeamWasFulminated(tablero);
            while (!someOneTeamDied)
            {
                //Controlando el flujo de las pawnrole phases, si es la fase cinco se reinicia al cero para no salirnos del array y completar el ciclo
                roundCount = roundCount == 3 ? 0 : roundCount;
                currentPhase = pawnRoleList[roundCount];
                bool shieldedPawn = false;
                // Se repite el ciclo para cada uno de los jugadores
                foreach (RobotPlugin player in players)
                {
                    Console.WriteLine("--------------------------------------------");
                    Console.WriteLine("\bFASE DE INICIALIZACION: elige peones que curar");
                    Console.WriteLine("--------------------------------------------");
                    Console.WriteLine("ROBOT " + player.team+" - Acciones:" + healingAction);
                    #region Round Initialization
                    foreach (Position position in player.RoundInitialization(board, healingAction))
                    {
                        gamePhaseManager.ExecRoundInitialization(tablero, board, position);
                    }
                    #endregion
                    Console.WriteLine("--------------------------------------------");
                    Console.WriteLine("\bFASE ACCION " + currentPhase);
                    Console.WriteLine("--------------------------------------------");
                    Console.WriteLine("ROBOT " + player.team + " - Acciones:" + roundAction + " | Fase: " + currentPhase);
                    switch (currentPhase)
                    {
                        case RobotPlugin.PawnRolePhases.Damage:
                            Console.WriteLine("DAMAGE: puedes dañar a un peon a UNA CASILLA  de distancia no se atraviesan obstáculos");
                            break;
                        case RobotPlugin.PawnRolePhases.Archery:
                            Console.WriteLine("ARCHERY: puedes dañar a un peon a distancia");
                            break;
                        case RobotPlugin.PawnRolePhases.Harakiri:
                            Console.WriteLine("HARAKIRI: si devuelves un valor te haces daño");
                            break;
                        case RobotPlugin.PawnRolePhases.Heal:
                            Console.WriteLine("HEAL: puedes curar a un peon a distancia (no se atraviesan obstáculos)");
                            break;

                    }
                    Console.WriteLine("____________________________________________");
                    #region Round Actions
                    List<Tuple<Position, Position>> actionPositionList = player.RoundActions(board, roundAction);
                    //Se itera dentro de la lista de las posiciones para realizar la accion correspondiente en cada caso
                    if (actionPositionList != null)
                    {
                        foreach (Tuple<Position, Position> TuplaAccion in actionPositionList)
                        {
                            pawnRolePhaseManager.ExecutePhaseAction(gamePhaseManager, currentPhase, tablero, TuplaAccion, actionPositionList);
                        }
                    }
                    else { Console.WriteLine("El pugin no ha indicado posiciones de acción"); }

                    #endregion
                    Console.WriteLine("--------------------------------------------");
                    Console.WriteLine("\bFASE FINAL: elige peones a los que escudar");
                    Console.WriteLine("ROBOT " + player.team + " - Acciones:" + shieldAction);
                    Console.WriteLine("--------------------------------------------");
                    #region End Action
                    List<Position> shieldedPositionList = playerIA1.RoundEnding(board, roundAction);
                    //Se itera dentro de la lista de las posiciones para realizar la accion correspondiente en cada caso
                    foreach (Position position in shieldedPositionList)
                    {
                        gamePhaseManager.AddElementsToShield(tablero,board, position);
                    }
                    #endregion
                }
            }
        }
    }
}

