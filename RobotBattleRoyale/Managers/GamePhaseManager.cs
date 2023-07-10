using InterfazRBR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotBattleRoyale
{
    public class GamePhaseManager
    {
        //Se diseña con un patrón singleton porque sino puede haber más de un tablero, no deberían existir multiples Managers
        #region Singleton
        private static GamePhaseManager _instance;

        public static GamePhaseManager Instance()
        {
            if (_instance == null)
            {
                _instance = new GamePhaseManager();
            }
            return _instance;
        }
        #endregion
        List<Position> shieldedPositionListRed = new List<Position>();
        List<Position> shieldedPositionListBlue = new List<Position>();

        public bool CheckPawnShielded(Pawn initialPawn, Position targetPosition)
        {
            bool isShielded = false;
            if (initialPawn.Team == Pawn.TeamColor.Blue)
            {
                if (shieldedPositionListBlue != null)
                {
                    isShielded = shieldedPositionListBlue.Contains(targetPosition);
                    shieldedPositionListBlue.Clear();
                }
            }
            else if (initialPawn.Team == Pawn.TeamColor.Red)
            {
                if (shieldedPositionListRed != null)
                {
                    isShielded = shieldedPositionListRed.Contains(targetPosition);
                    shieldedPositionListRed.Clear();
                }
            }
            return isShielded;
        }
        public void AddElementsToShield(Board tablero, IPlaygroundElement[,] board,Position position)
        {
            if (tablero.ValidPosition(position))
            {
                if (tablero.IsPawn(position))
                {
                    Pawn pawn = (Pawn)board[position.x, position.y];
                    if (pawn.Team == Pawn.TeamColor.Blue)
                        shieldedPositionListBlue.Add(position);
                    else if (pawn.Team == Pawn.TeamColor.Red)
                        shieldedPositionListRed.Add(position);
                }
                else
                {
                    Console.WriteLine("Acción invalidada, el elemento no es un peón");
                }
            }
        }

        public void ExecRoundInitialization(Board tablero, IPlaygroundElement[,] board ,Position position) 
        {
            
            if (tablero.ValidPosition(position))
            {
                if (tablero.IsPawn(position))
                {
                    Pawn pawn = (Pawn)board[position.x, position.y];
                    Console.WriteLine("Robot " + pawn.Team + " - vida:" + pawn.Life);
                    pawn.Heal(2);
                    Console.WriteLine("Robot " + pawn.Team + " - vida:" + pawn.Life);
                }
                else
                {
                    Console.WriteLine("Acción invalidada, el elemento no es un peón");
                }
            }
        }
    }
}
