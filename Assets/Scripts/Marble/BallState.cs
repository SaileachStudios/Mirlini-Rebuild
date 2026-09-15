namespace SaileachStudios.Mirlini.Marble
{
    public enum BallState
    {
        Idle,           // Waiting for level to start
        Playing,        // Normal rolling
        Falling,        // Dropping into hole
        Respawning,     // Growing back after wrong hole
        LevelComplete   // Fell in correct hole
    }

}
