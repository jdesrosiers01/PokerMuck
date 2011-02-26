﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace PokerMuck
{
    /* A holdem player has certain statistics that a five card draw player might not have */
    class HoldemPlayer : Player
    {

        private int limps;
        private bool HasLimpedThisRound;

        private int voluntaryPutMoneyPreflop;
        private bool HasVoluntaryPutMoneyPreflopThisRound;

        private int preflopRaises;
        private bool HasPreflopRaisedThisRound;

        /* Each table is set this way:
         key => value
         GamePhase => value
         Ex. calls[flop] == 4 --> player has flat called 4 times during the flop
         */
        private Hashtable calls;
        private Hashtable bets;
        private Hashtable folds;
        private Hashtable raises;

        public bool IsBigBlind { get; set; }
        public bool IsSmallBlind { get; set; }



        public HoldemPlayer(String playerName)
            : base(playerName)
        {
            calls = new Hashtable(5);
            bets = new Hashtable(5);
            folds = new Hashtable(5);
            raises = new Hashtable(5);

            ResetAllStatistics();
        }

        /* Returns the limp ratio (1.0 to 0) */
        public float GetLimpRatio()
        {
            return (float)limps / (float)totalHandsPlayed;
        }

        /* How many times has the player put money in preflop? */
        public float GetVPFRatio()
        {
            return (float)voluntaryPutMoneyPreflop / (float)totalHandsPlayed;
        }

        /* How many times has the player raised preflop? */
        public float GetPFRRatio()
        {
            return (float)preflopRaises / (float)totalHandsPlayed;
        }
        
        /* This player has raised, increment the stats */
        public void HasRaised(HoldemGamePhase gamePhase){
            if (gamePhase == HoldemGamePhase.Preflop)
            {
                IncrementVoluntaryPutMoneyPreflop();
                IncrementPreflopRaise();
            }

            IncrementStatistics(raises, gamePhase);
        }

        /* Has bet */
        public void HasBet(HoldemGamePhase gamePhase)
        {
            if (gamePhase == HoldemGamePhase.Preflop)
            {
                IncrementVoluntaryPutMoneyPreflop();
            }

            IncrementStatistics(bets, gamePhase);
        }

        /* Has limped */
        public void HasLimped()
        {
            /* If he's not the small or big blind, this is also a limp */
            if (!IsSmallBlind && !IsBigBlind && !HasLimpedThisRound)
            {
                limps += 1;
                HasLimpedThisRound = true;
            }
        }

        /* Has called */
        public void HasCalled(HoldemGamePhase gamePhase)
        {
            if (gamePhase == HoldemGamePhase.Preflop)
            {
                IncrementVoluntaryPutMoneyPreflop();
            }


            IncrementStatistics(calls, gamePhase);
        }

        /* Folded */
        public void HasFolded(HoldemGamePhase gamePhase)
        {
            IncrementStatistics(folds, gamePhase);
        }


        /* Helper function to increment the VPF stat */
        private void IncrementVoluntaryPutMoneyPreflop()
        {
            if (!HasVoluntaryPutMoneyPreflopThisRound)
            {
                HasVoluntaryPutMoneyPreflopThisRound = true;
                voluntaryPutMoneyPreflop += 1;
            }
        }


        /* Helper function to increment the PFR stat */
        private void IncrementPreflopRaise()
        {
            if (!HasPreflopRaisedThisRound)
            {
                HasPreflopRaisedThisRound = true;
                preflopRaises += 1;
            }
        }

        /* Helper function to increment the value in one of the hash tables (calls, raises, folds, etc.) */
        private void IncrementStatistics(Hashtable table, HoldemGamePhase gamePhase)
        {
            table[gamePhase] = (int)table[gamePhase] + 1;
        }

        /* Certain statistics are round specific (for example a person can only limp once per round)
         * This function should get called at the beginning of a new round */
        public override void PrepareStatisticsForNewRound()
        {
            base.PrepareStatisticsForNewRound();

            IsBigBlind = false;
            IsSmallBlind = false;
            HasLimpedThisRound = false;
            HasVoluntaryPutMoneyPreflopThisRound = false;
            HasPreflopRaisedThisRound = false;
        }

        
        /* Resets all statistics counters */
        public override void ResetAllStatistics()
        {
            base.ResetAllStatistics();

            ResetStatistics(calls);
            ResetStatistics(bets);
            ResetStatistics(folds);
            ResetStatistics(raises);
            limps = 0;
            voluntaryPutMoneyPreflop = 0;
            totalHandsPlayed = 0;
            preflopRaises = 0;

            PrepareStatisticsForNewRound();
        }

        /* Reset the stats for a particular hash table set */
        private void ResetStatistics(Hashtable table)
        {
            table[HoldemGamePhase.Preflop] = 0;
            table[HoldemGamePhase.Flop] = 0;
            table[HoldemGamePhase.Turn] = 0;
            table[HoldemGamePhase.River] = 0;
        }

        /* Returns the statistics of the player */
        public override Statistics GetStatistics()
        {
            Statistics result =  base.GetStatistics();

            result.Set("VPF", GetVPFRatio());
            result.Set("Limp", GetLimpRatio());
            result.Set("PFR", GetPFRRatio());

            return result;
        }
    }
}
