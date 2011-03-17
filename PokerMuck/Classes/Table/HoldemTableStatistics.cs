﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PokerMuck
{
    class HoldemTableStatistics : TableStatistics
    {
        /* Information about the blinds */
        public float BigBlindAmount { get; set; }
        public float SmallBlindAmount { get; set; }

        /* Somebody bet the flop */
        private bool PlayerBetTheFlopThisRound;

        /* Somebody cbet the flop (which is different than simple betting) */
        private bool PlayerCBetThisRound;

        /* Somebody raised the flop */
        private bool PlayerRaisedTheFlopThisRound;

        public HoldemTableStatistics(Table table)
            : base(table)
        {
            PrepareStatisticsForNewRound();
        }

        public override void PrepareStatisticsForNewRound()
        {
            base.PrepareStatisticsForNewRound();

            PlayerBetTheFlopThisRound = false;
            PlayerRaisedTheFlopThisRound = false;
            PlayerCBetThisRound = false;
        }

        public override void RegisterParserHandlers(HHParser parser)
        {
            ((HoldemHHParser)parser).FoundBigBlind += new HoldemHHParser.FoundBigBlindHandler(handHistoryParser_FoundBigBlind);
            ((HoldemHHParser)parser).FoundSmallBlind += new HoldemHHParser.FoundSmallBlindHandler(handHistoryParser_FoundSmallBlind);
            ((HoldemHHParser)parser).PlayerBet += new HoldemHHParser.PlayerBetHandler(handHistoryParser_PlayerBet);
            ((HoldemHHParser)parser).PlayerCalled += new HoldemHHParser.PlayerCalledHandler(handHistoryParser_PlayerCalled);
            ((HoldemHHParser)parser).PlayerFolded += new HoldemHHParser.PlayerFoldedHandler(handHistoryParser_PlayerFolded);
            ((HoldemHHParser)parser).PlayerRaised += new HoldemHHParser.PlayerRaisedHandler(handHistoryParser_PlayerRaised);
            ((HoldemHHParser)parser).PlayerChecked += new HoldemHHParser.PlayerCheckedHandler(handHistoryParser_PlayerChecked);
            ((HoldemHHParser)parser).FoundButton += new HoldemHHParser.FoundButtonHandler(handHistoryParser_FoundButton);

        }

        void handHistoryParser_FoundButton(int seatNumber)
        {
            // The player that is seating at seatNumber gets the button
            foreach (HoldemPlayer p in table.PlayerList)
            {
                if (p.SeatNumber == seatNumber)
                {
                    p.IsButton = true;
                    break;
                }            
            }
        }

        void handHistoryParser_PlayerRaised(string playerName, float initialPot, float raiseAmount, HoldemGamePhase gamePhase)
        {
            HoldemPlayer p = FindPlayer(playerName);
            
            if (gamePhase == HoldemGamePhase.Flop)
            {
                // Has somebody cbet?
                if (!PlayerRaisedTheFlopThisRound && PlayerCBetThisRound)
                {
                    p.IncrementRaiseToACBet();
                }

                PlayerRaisedTheFlopThisRound = true;
            }

            
            p.HasRaised(gamePhase);
        }

        void handHistoryParser_PlayerFolded(string playerName, HoldemGamePhase gamePhase)
        {
            HoldemPlayer p = FindPlayer(playerName);

            // Has somebody cbet?
            if (gamePhase == HoldemGamePhase.Flop && !PlayerRaisedTheFlopThisRound && PlayerCBetThisRound)
            {
                p.IncrementFoldToACBet();
            }

            p.HasFolded(gamePhase);
        }

        void handHistoryParser_PlayerChecked(string playerName, HoldemGamePhase gamePhase)
        {
            HoldemPlayer p = FindPlayer(playerName);

            // Flop
            if (gamePhase == HoldemGamePhase.Flop && !PlayerBetTheFlopThisRound && p.HasPreflopRaisedThisRound())
            {
                p.IncrementOpportunitiesToCBet(false);
            }

            p.HasChecked(gamePhase);
        }

        void handHistoryParser_PlayerCalled(string playerName, float amount, HoldemGamePhase gamePhase)
        {
            HoldemPlayer p = FindPlayer(playerName);

            // If we are preflop
            if (gamePhase == HoldemGamePhase.Preflop)
            {
                // If the call is the same amount as the big blind, this is also a limp
                if (amount == BigBlindAmount)
                {
                    // HasLimped() makes further checks to avoid duplicate counts and whether the player is the big blind or small blind
                    p.CheckForLimp();
                }
            }
            else if (gamePhase == HoldemGamePhase.Flop)
            {
                // Has somebody cbet?
                if (!PlayerRaisedTheFlopThisRound && PlayerCBetThisRound)
                {
                    p.IncrementCallToACBet();
                }
            }

            p.HasCalled(gamePhase);
        }

        void handHistoryParser_PlayerBet(string playerName, float amount, HoldemGamePhase gamePhase)
        {
            HoldemPlayer p = FindPlayer(playerName);

            // Flop
            if (gamePhase == HoldemGamePhase.Flop && !PlayerBetTheFlopThisRound)
            {
                // Did he raised preflop?
                if (p.HasPreflopRaisedThisRound())
                {
                    p.IncrementOpportunitiesToCBet(true);
                    PlayerCBetThisRound = true;
                }

                PlayerBetTheFlopThisRound = true;
            }


            p.HasBet(gamePhase);
        }

        void handHistoryParser_FoundBigBlind(String playerName, float amount)
        {
            // Save current blind
            BigBlindAmount = amount;

            // Keep track of who is the big blind
            HoldemPlayer p = FindPlayer(playerName);
            p.IsBigBlind = true;
        }

        void handHistoryParser_FoundSmallBlind(String playerName, float amount)
        {
            // Save current blind
            SmallBlindAmount = amount;

            // Keep track of who is the small blind
            HoldemPlayer p = FindPlayer(playerName);
            p.IsSmallBlind = true;
        }


        /* Helper function to find a player */
        private HoldemPlayer FindPlayer(String playerName)
        {
            return (HoldemPlayer)table.FindPlayer(playerName);
        }
    }
}