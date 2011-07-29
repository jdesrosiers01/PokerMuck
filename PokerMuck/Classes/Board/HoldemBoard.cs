﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace PokerMuck
{
    public class HoldemBoard : Board
    {
        public HoldemBoard(Card first, Card second, Card third)
            : base("Board")
        {
            AddCard(first);
            AddCard(second);
            AddCard(third);
        }

        public HoldemBoard(Card first, Card second, Card third, Card fourth)
            : this(first, second, third)
        {
            AddCard(fourth);
        }

        public HoldemBoard(Card first, Card second, Card third, Card fourth, Card fifth) : 
            this(first, second, third, fourth)
        {
            AddCard(fifth);
        }

        public HoldemBoard GetBoardAt(HoldemGamePhase phase)
        {
            switch (phase)
            {
                case HoldemGamePhase.Flop:
                    return new HoldemBoard(cards[0], cards[1], cards[2]);
                case HoldemGamePhase.Turn:
                    Debug.Assert(cards.Count >= 4, "Trying to get turn board when only flop cards are available");
                    return new HoldemBoard(cards[0], cards[1], cards[2], cards[3]);
                case HoldemGamePhase.River:
                    Debug.Assert(cards.Count >= 5, "Trying to get river board when only flop or cards are available");
                    return new HoldemBoard(cards[0], cards[1], cards[2], cards[3], cards[4]);
                default:
                    Debug.Assert(false, "Trying to get a board for an invalid game phase");
                    return null;
            }
        }
    }
}
