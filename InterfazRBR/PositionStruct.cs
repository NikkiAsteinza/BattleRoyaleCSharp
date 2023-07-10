namespace InterfazRBR
{
    public struct Position
    {
        public int x;
        public int y;

        public Position(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        public static Position operator +(Position lhs, Position rhs)
        {
            return new Position(lhs.x + rhs.x, lhs.y + rhs.y);
        }
        public static Position operator -(Position lhs, Position rhs)
        {
            return new Position(lhs.x - rhs.x, lhs.y - rhs.y);
        }
        public override string ToString()
        {
            return "P(" + x + ", " + y + ")";
        }
    }
}
