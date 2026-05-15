
public struct CardDroppedEvent  { public int Row; public bool IsPlayerSlot; }
public struct CardRemovedEvent  { public int Row; public bool IsPlayerSlot; }
public struct TurnEndedEvent    { }
public struct PassTurnEvent     { }
public struct GameOverEvent     { public string Winner; public int P1Points; public int P2Points; }