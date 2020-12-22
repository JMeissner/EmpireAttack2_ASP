namespace EmpireAttack2_ASP.Game.TileMap
{
    public enum TileType
    {
        Normal, Forest, Hills, Urban, Water, Capital
    };

    public enum Coin
    {
        None, Bronze, Silver, Gold
    };

    public class Tile
    {
        #region Public Properties
        public Point Coordinates { get; set; }
        public Faction Faction { get; set; }
        public bool IsConnected { get; set; }
        public bool IsVisited { get; set; }
        public int Population { get; set; }
        public TileType Type { get; set; }
        public Coin Coin { get; set; }

        #endregion Public Properties

        #region Public Constructors

        public Tile(TileType type)
        {
            this.Faction = Faction.NONE;
            this.Population = 1;
            this.Type = type;
            this.Coin = Coin.None;
        }
        public Tile(TileType type, int i, int j)
        {
            this.Coordinates = new Point(i, j);
            this.Faction = Faction.NONE;
            this.Population = 1;
            this.Type = type;
            this.Coin = Coin.None;
        }

        public Tile(TileType type, int i, int j, bool random)
        {
            System.Random r = new System.Random();

            this.Coordinates = new Point(i, j);
            this.Faction = Faction.NONE;
            this.Type = type;
            this.Coin = Coin.None;

            switch (type)
            {
                case TileType.Normal: this.Population = 1; break;
                case TileType.Capital: this.Population = 1; break;
                case TileType.Urban: this.Population = r.Next(500, 3500); break;
                case TileType.Hills: this.Population = r.Next(1250, 5000); break;
                case TileType.Water: this.Population = r.Next(750, 2000); break;
                case TileType.Forest: this.Population = r.Next(50, 150); break;
                default: this.Population = 1; break;
            }
        }

        public Tile(Faction faction, int population, TileType type, Coin coin)
        {
            this.Faction = faction;
            this.Population = population;
            this.Type = type;
            this.Coin = coin;
        }

        #endregion Public Constructors

        #region Public Methods

        public string GetShortType()
        {
            string stype = "";
            switch (Type)
            {
                case TileType.Normal: stype = "N"; break;
                case TileType.Water: stype = "W"; break;
                case TileType.Forest: stype = "F"; break;
                case TileType.Hills: stype = "H"; break;
                case TileType.Urban: stype = "U"; break;
                case TileType.Capital: stype = "C"; break;
                default: stype = "E"; break;
            }
            return stype;
        }

        #endregion Public Methods
    }
}