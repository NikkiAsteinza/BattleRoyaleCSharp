namespace InterfazRBR
{
    /// <summary>
    /// Los elementos pertenecientes al tablero deben implementar este interfaz, donde se incluye un método
    /// que indica cómo se muestra el elemento en un caracter para su representación visual a la hora
    /// de imprimir el tablero entero.
    /// </summary>
    public interface IPlaygroundElement{ char ToChar(); };

    public class Pawn : IPlaygroundElement
    {

        public enum TeamColor : ushort
        {
            Red,
            Blue
        }

        private ushort life;
        public ushort Life { get => life; }
        public ushort Damage { get; }
        public TeamColor Team { get; }
        private bool isAlife = true;
        public bool IsAlife { get => isAlife; }
        public bool isShielded;

        public Pawn(ushort life, ushort damage, TeamColor team)
        {
            this.life = life;
            Damage = damage;
            Team = team;
        }

        public Pawn(Pawn other)
        {
            life = other.Life;
            Damage = other.Damage;
            Team = other.Team;
        }
        
        public bool DealDamage(ushort damage)
        {
            if (isShielded && damage > 0)
                damage--;

            if (Life < damage)
            {
                life = 0;
                isAlife = false;
                return true;
            }
            else
            {
                life -= damage;
                return false;
            }
        }

        public bool Heal(ushort lifePoints)
        {
            if (isAlife && life > 0)
            {
                life += lifePoints;
                return true;
            }

            return false;
        }

        public char ToChar()
        {
            if (!isAlife)
                return 'X';
            char teamChar = (Team == TeamColor.Red) ? 'r' : 'b';
            return (Life < 2 && Damage < 2) ? teamChar : char.ToUpper(teamChar);
        }

        public override string ToString() => "Pawn [Team: " + ToChar() + " | L: " + life + " | D: " + Damage + "]"; 
    }

    public class Obstacle : IPlaygroundElement
    {
        ushort life;
        public ushort Life { get => life; }

        public Obstacle(ushort life)
        {
            this.life = life;
        }

        public Obstacle(Obstacle other)
        {
            life = other.Life;
        }

        public bool DealDamage(ushort damage)
        {
            if (Life < damage)
            {
                life = 0;
                return true;
            }
            else
            {
                life -= damage;
                return false;
            }
        }

        public char ToChar() => (life > 0) ? '#':' ';
        public override string ToString() => "Obstacle [Life: " + life + "]";
    }
}
