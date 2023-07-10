using System;
using System.Collections.Generic;

namespace InterfazRBR
{
    /// <summary>
    /// Clase de la cual deben heredar todos los plug-ins asociados con el RobotBattleRoyale.
    /// </summary>
    public abstract class RobotPlugin
    {
        public enum PawnRolePhases: ushort
        {
            Damage,     //Daña al enemigo al caminar sobre el.
            Heal,       //Cura al enemigo al caminar sobre el.
            Archery,    //El movimiento a más de 1 de distancia se considera disparo de flecha.
            Harakiri    //El movimiento se considera daño auto-infligido.
        }

        public readonly Pawn.TeamColor team;                //Color de equipo que controla esta inteligencia.
        protected readonly PawnRolePhases[] pawnRoleList;   //Listado de roles que tomará la partida cada ronda de manera cíclica.
        protected readonly int boardDimensionX;             //Dimensión en X del tablero (Primera coordenada del array)
        protected readonly int boardDimensionY;             //Dimensión en Y del tablero (Segunda coordenada del array)

        /// <summary>
        /// Este constructor almacena los datos necesarios no modificables durante la partida, de tal manera
        /// que en llamadas posteriores se pasarán los datos que sí cambian cada ronda.
        /// </summary>
        /// <param name="team">Equipo al que pertenece esta inteligencia</param>
        /// <param name="pawnRoleList">Los roloes que existirán cada ronda de manera cíclica</param>
        /// <param name="boardDimensionX">Dimensión en el primer eje de la matriz que compone el tablero</param>
        /// <param name="boardDimensionY">Dimensión del segundo eje de la matriz que compone el tablero</param>
        public RobotPlugin(Pawn.TeamColor team, PawnRolePhases[] pawnRoleList, int boardDimensionX, int boardDimensionY)
        {
            this.team = team;
            this.pawnRoleList = pawnRoleList;
            this.boardDimensionX = boardDimensionX;
            this.boardDimensionY = boardDimensionY;
        }

        /// <summary>
        /// Fase en la que las acciones curan en área: 2 en la casilla 1 alrededor.
        /// </summary>
        /// <param name="boardData">Datos del tablero en este momento</param>
        /// <param name="numActions">Indica el numero de acciones de curación disponibles para esta ronda</param>
        /// <returns>Las posiciones donde se va a aplicar la curación de la fase de inicio</returns>
        public abstract List<Position> RoundInitialization(IPlaygroundElement[,] boardData, ushort numActions);

        /// <summary>
        /// Fase en la que se mueven los peones (Teniendo en cuenta el pawnRole de esta ronda).
        /// Damage      //Daña al enemigo al caminar sobre el.
        /// Heal        //Cura al enemigo al caminar sobre el.
        /// Archery     //El movimiento a más de 1 de distancia se considera disparo de flecha.
        /// Harakiri    //El movimiento se considera daño auto-inflingido.        
        /// </summary>
        /// <param name="boardData">Datos del tablero en este momento</param>
        /// <param name="numActions">Se indica un número de movimientos para esta IA en esta ronda</param>
        /// <returns>Se retorna una lista que indica los movimientos. (De la posición A a la B)</returns>
        public abstract List<Tuple<Position, Position>> RoundActions(IPlaygroundElement[,] boardData, ushort numActions);

        /// <summary>
        /// Fase en la que se activa el escudo para determinados peones.
        /// </summary>
        /// <param name="boardData">Datos del tablero en este momento</param>
        /// <param name="numActions">Numero de escudos que se pueden indicar esta ronda</param>
        /// <returns>Retorna la lista de posiciones donde la IA indica que se lanzan el número de escudos disponibles</returns>
        public abstract List<Position> RoundEnding(IPlaygroundElement[,] boardData, ushort numActions);
    }
}
