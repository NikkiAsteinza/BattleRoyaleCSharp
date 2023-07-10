using InterfazRBR;
using System;
using System.Collections.Generic;

namespace MockPlugIn
{
    [RobotInfo("Nikki Asteinza", "KreeTeam", "MoridTerraneos")]
    public class MockPlugIn : RobotPlugin
    {
        //Listas de elementos de interes en el tablero
        List<Position> peones = new List<Position>();
        //Array de fases del juego
        PawnRolePhases[] phases;
        //Variable para almacenar la fase de juego actual
        int currentPhase = 0;

        public MockPlugIn(Pawn.TeamColor team, PawnRolePhases[] pawnRoleList, int boardDimensionX, int boardDimensionY) :
            base(team, pawnRoleList, boardDimensionX, boardDimensionY)
        {
            Console.WriteLine("Robot " + team); //... Si necesito hacer algo para inicializarlo
            phases = pawnRoleList;
        }


        #region Usefull methods
        /// <summary>
        /// Preguntar al usuario por un peón objetivo
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private Position GetPeonObjetivoPos_Input(string message)
        {
            Console.WriteLine(message);
            int num = int.Parse(Console.ReadLine());
            Position newPosition;
            while (num < 0 || num > (peones.Count-1))
            {
                Console.WriteLine("Introduce un número de peón válido");
                num = int.Parse(Console.ReadLine());
            }
            Position peonObjetivo = peones[num];
            newPosition = new Position(peonObjetivo.x, peonObjetivo.y);
            return newPosition;
        }
        /// <summary>
        /// Limpiar la lista existente de peones de nuestro equipo 
        /// </summary>
        private void CleanPlaygroundElementsList()
        {
            peones.Clear();
        }
        /// <summary>
        /// Actualizar la lista de nuestros peones
        /// </summary>
        /// <param name="boardData"></param>
        private void FindPlaygroundElements(IPlaygroundElement[,] boardData)
        {
            CleanPlaygroundElementsList();
            for (int j = 0; j < boardDimensionX; j++)
            {
                for (int k = 0; k < boardDimensionY; k++)
                {
                    var x = boardData[j, k];

                    if (x is Pawn)
                    {
                        Pawn pawn = (Pawn)boardData[j, k];
                       if (pawn.Team == team)
                            peones.Add(new Position(j, k));
                    }
                }
            }
        }
        private bool Obstacle(IPlaygroundElement playgroundElement)
        {
            return playgroundElement is Obstacle ? true : false;
        }
        private bool Pawn(IPlaygroundElement playgroundElement)
        {
            return playgroundElement is Pawn ? true : false;
        }
        /// <summary>
        /// Seleccionar uno de los peones de nuestro equipo
        /// </summary>
        /// <param name="boardData"></param>
        /// <param name="numActions"></param>
        /// <returns></returns>
        private List<Position> SelectOwnPawn(IPlaygroundElement[,] boardData, ushort numActions)
        {
            //Se buscan y separan por listas los peones y enemigos
            FindPlaygroundElements(boardData);
            //Se crea la lista de posiciones que se va a devolver
            List<Position> returnedList = new List<Position>();
            //Se pintan los peones por consola
            PintarPeones(boardData);
            // Se pregunta al usuario qué peón quiere currar y se añade la lista tantan veces como acciones hayan sido definidas
            for (int i = 0; i < numActions; ++i)
            {
                Position newPosition= GetPeonObjetivoPos_Input("Introduce el número del peón sobre el que deseas actuar");
                returnedList.Add(newPosition);
            }
            return returnedList;
        }
        /// <summary>
        /// Se comprueba que la posición esté dentro del tablero de juego
        /// </summary>
        /// <param name="damageTargetPosition"></param>
        /// <param name="boardDimensionX"></param>
        /// <param name="boardDimensionY"></param>
        /// <returns></returns>
        private bool OnBoardPosition(Position damageTargetPosition, int boardDimensionX, int boardDimensionY)
        {
            bool xInOnBoard = damageTargetPosition.x > 0 && damageTargetPosition.x < (boardDimensionX - 1);
            bool yInOnBoard = damageTargetPosition.y > 0 && damageTargetPosition.y < (boardDimensionY - 1);
            return (xInOnBoard && yInOnBoard);
        }
        /// <summary>
        /// Se comprueba que la casilla del tablero está vacía
        /// </summary>
        /// <param name="boardData"></param>
        /// <param name="damageTargetPosition"></param>
        /// <returns></returns>
        private bool OnBoardPosition_Empty(IPlaygroundElement[,] boardData, Position damageTargetPosition)
        {
            return boardData[damageTargetPosition.x, damageTargetPosition.y] == null;
        }
        /// <summary>
        /// Se comprueba que la casilla del tablero contenga un Pawn
        /// </summary>
        /// <param name="boardData"></param>
        /// <param name="damageTargetPosition"></param>
        /// <returns></returns>
        private bool OnBoardPosition_Pawn(IPlaygroundElement[,] boardData, Position damageTargetPosition)
        {
            return boardData[damageTargetPosition.x, damageTargetPosition.y] is Pawn;
        }
        /// <summary>
        /// Se comprueba que la casilla del tablero contenga un Obstacle
        /// </summary>
        /// <param name="boardData"></param>
        /// <param name="damageTargetPosition"></param>
        /// <returns></returns>
        private bool OnBoardPosition_Obstacle(IPlaygroundElement[,] boardData, Position damageTargetPosition)
        {
            return boardData[damageTargetPosition.x, damageTargetPosition.y] is Obstacle;
        }
        /// <summary>
        /// Se solicita al usuario el movimiento que desea realizar de un único desplazamiento
        /// </summary>
        /// <param name="boardData"></param>
        /// <param name="targetPosition"></param>
        /// <param name="initialPosition"></param>
        /// <param name="boardDimensionX"></param>
        /// <param name="boardDimensionY"></param>
        private void GetStepCrossTypeMovement(IPlaygroundElement[,] boardData, ref Position targetPosition, Position initialPosition, int boardDimensionX, int boardDimensionY)
        {
            Position UserInputPosition = new Position();
            bool correctPosition = false;
            while (!correctPosition)
            {
                Console.WriteLine("Introduce la dirección hacia la que quieres que se mueva o ataque tu peón");
                Console.WriteLine("Arriba (w) Abajo (s) Derecha (d) Izquierda (a)");
                char direccion = Console.ReadKey().KeyChar;
                //Se crea una variable por eje y se les asigna el valor de la posición desde la que se quiere inicar el movimiento
                int x = initialPosition.x;
                int y = initialPosition.y;
                //Peon inicial del movimiento
                Pawn initialPawn;
                // Asignamos sin comprobar porque desde la consola solo te permite seleccionar peones
                initialPawn = (Pawn)boardData[initialPosition.x, initialPosition.y];

                // Se realiza un switch sobre la respuesta del usuario y se le suman o restan valores a los ejes 
                switch (direccion)
                {
                    case 'd':
                        y += 1;
                        break;
                    case 'a':
                        y -= 1;
                        break;
                    case 'w':
                        x -= 1;
                        break;
                    case 's':
                        x += 1;
                        break;
                }
                // Se asignan los valores a de la nueva posicion:
                UserInputPosition.x = x;
                UserInputPosition.y = y;
                Console.WriteLine("\nPosicion elegida:"+UserInputPosition);
                // Si está dentro 
                if (OnBoardPosition(UserInputPosition, boardDimensionX, boardDimensionY))
                {
                    //Si la casilla contiene algo
                    if (!OnBoardPosition_Empty(boardData, UserInputPosition))
                    {
                        correctPosition = CheckIfPlaugroundElementIsValid(boardData, UserInputPosition, correctPosition);
                    }
                    else 
                    {
                        if (!correctPosition)
                        {
                            Console.WriteLine("La casilla está vacía, solo te moverás. Pulsa 1 para confirmar y cero para volver a empezar.");
                            int confirmacion = int.Parse(Console.ReadLine());
                            if (confirmacion == 1) correctPosition = true;

                        }
                    }
                }
                targetPosition.x = UserInputPosition.x;
                targetPosition.y = UserInputPosition.y;

                if (!correctPosition) Console.WriteLine("La posición de destino no es válida.");
            }
        }
        /// <summary>
        /// Se solicita al usuario la posición para realizar una acción lejana
        /// </summary>
        /// <param name="boardData"></param>
        /// <param name="targetPosition"></param>
        /// <param name="initialPosition"></param>
        /// <param name="boardDimensionX"></param>
        /// <param name="boardDimensionY"></param>
        private void GetFarAttackTarget(IPlaygroundElement[,] boardData, Position targetPosition, Position initialPosition, int boardDimensionX, int boardDimensionY)
        {
            Position UserInputPosition = new Position();
            bool correctPosition = false;
            while (!correctPosition)
            {
                Console.WriteLine("Introduce la dirección hacia la que quieres que ataque tu peón");
                Console.WriteLine("Arriba (w) Abajo (s) Derecha (d) Izquierda (a)");
                char direccion = Console.ReadKey().KeyChar;
                Console.WriteLine("Introduce a cuántas casillas en esa dirección quieres que lance la flecha tu peón");
                int distancia = int.Parse(Console.ReadLine());
                switch (direccion)
                {
                    case 'd':
                        UserInputPosition.y = initialPosition.y + 1;
                        break;
                    case 'a':
                        UserInputPosition.y = initialPosition.y - 1;
                        break;
                    case 'w':
                        UserInputPosition.x = initialPosition.x - 1;
                        break;
                    case 's':
                        UserInputPosition.x = initialPosition.x + 1;
                        break;
                }
                //Si la posición elegida se encuentra dentro del tablero
                if (OnBoardPosition(UserInputPosition, boardDimensionX, boardDimensionY))
                {
                    //Si la casilla contiene algo
                    if (!OnBoardPosition_Empty(boardData, UserInputPosition))
                    {
                        // Si ese algo que contiene la casilla es un pawn
                        correctPosition = CheckIfPlaugroundElementIsValid(boardData, UserInputPosition, correctPosition);

                    }
                }
            }

        }
        /// <summary>
       /// Se comprueba si la posición en el tablero corresponde a un iPlaygroundElement
       /// </summary>
       /// <param name="boardData"></param>
       /// <param name="UserInputPosition"></param>
       /// <param name="correctPosition"></param>
       /// <returns></returns>
        private bool CheckIfPlaugroundElementIsValid(IPlaygroundElement[,] boardData, Position UserInputPosition, bool correctPosition)
        {
            if (OnBoardPosition_Pawn(boardData, UserInputPosition))
            {
                // Se crea una variable para poder acceder a los métodos de la clase una vez casteado
                Pawn targetPawn = (Pawn)boardData[UserInputPosition.x, UserInputPosition.y];
                // Si el pawn no es de nuestro equipo
                if (targetPawn.Team != team)
                {
                    correctPosition = true;
                }
                else
                {
                    //Si es de nuestro equipo
                    Console.WriteLine("No deberías dañarte a ti mismo..Ejem..");
                }
            }
            else if (OnBoardPosition_Obstacle(boardData, UserInputPosition))
            {
                correctPosition = true;
                Console.WriteLine("No puedes caminar sobre un muro");
            }

            return correctPosition;
        }
        /// <summary>
        /// Muestra los peones por consola
        /// </summary>
        /// <param name="boardData"></param>
        private void PintarPeones(IPlaygroundElement[,] boardData)
        {
            for (int j = 0; j < peones.Count; j++)
            {
                int index = j;
                Position pos = peones[j];
                Pawn peon = (Pawn)boardData[pos.x, pos.y];
                Console.WriteLine("(" + index + ")" + pos + " Vida:" + peon.Life);
            }
        }
        #endregion

        #region Action Methods
        private void DoDamage(IPlaygroundElement[,] boardData, ushort numActions, List<Tuple<Position, Position>> returnedList)
        {
            //Seleccionar el peón que atacará
            Position initialPosition = GetPeonObjetivoPos_Input("Introduce el número de peón que realizará esta acción");
            //Seleccionar la casilla hacia la que atacará
            Position targetPosition = new Position();
            GetStepCrossTypeMovement(boardData, ref targetPosition, initialPosition, boardDimensionX, boardDimensionY);
            Tuple<Position, Position> tupla = new Tuple<Position, Position>(initialPosition, targetPosition);
            returnedList.Add(tupla);
        }
        private void DoFarAwayAction(IPlaygroundElement[,] boardData, ushort numActions, List<Tuple<Position, Position>> returnedList)
        {
            //Seleccionar el peón que atacará
            Position initialPosition = GetPeonObjetivoPos_Input("Introduce el número de peón que realizará esta acción");
            //Seleccionar la casilla hacia la que atacará
            Position targetPosition = new Position();
            GetFarAttackTarget(boardData, targetPosition, initialPosition, boardDimensionX, boardDimensionY);
            Tuple<Position, Position> tupla = new Tuple<Position, Position>(initialPosition, targetPosition);
            returnedList.Add(tupla);
        }
        #endregion

        #region Robot Plugin Overrides
        override public List<Position> RoundInitialization(IPlaygroundElement[,] boardData, ushort numActions)
        {
            currentPhase = currentPhase == 3 ? currentPhase = 0 : currentPhase;
            return SelectOwnPawn(boardData, numActions);
        }
        override public List<Tuple<Position, Position>> RoundActions(IPlaygroundElement[,] boardData, ushort numActions)
        {
           
            //Se actualiza la lista de peones de nuestro equipo
            FindPlaygroundElements(boardData);
            //Se crea la lista de posiciones que se devolverá
            List<Tuple<Position, Position>> returnedList = new List<Tuple<Position, Position>>();
            //Se comprueba la fase y se resetea el contador al llegar al final de la lista
            if (currentPhase > 3) currentPhase = 0;
            //Por cada una de las acciones pasadas por parámetro
            for (int i = 0; i < numActions; i++)
            {
                //En funcion de la fase del juego que esté teniendo lugar
                switch (pawnRoleList[currentPhase])
                {
                    case PawnRolePhases.Archery:
                        DoFarAwayAction(boardData, numActions, returnedList);
                        break;
                    case PawnRolePhases.Damage:
                        DoDamage(boardData, numActions, returnedList);
                        break;
                    case PawnRolePhases.Harakiri:
                        returnedList = null;
                        break;
                    case PawnRolePhases.Heal:
                        DoFarAwayAction(boardData, numActions, returnedList);
                        break;
                }
            }
            currentPhase++;
            return returnedList;
        }
        override public List<Position> RoundEnding(IPlaygroundElement[,] boardData, ushort numActions)
        {
            //Se actualiza la lista de peones de nuestro equipo
            FindPlaygroundElements(boardData);
            currentPhase += 1;
            Console.WriteLine("Siguiente fase"+pawnRoleList[currentPhase]);
            return SelectOwnPawn(boardData, numActions);
            
        }
        #endregion
    }
}

