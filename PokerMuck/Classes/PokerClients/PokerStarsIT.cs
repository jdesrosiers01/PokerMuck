﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace PokerMuck
{
    class PokerStarsIT : PokerClient
    {
        public PokerStarsIT(){
            // Other init stuff?
        }

        public PokerStarsIT(String language)
        {
            InitializeLanguage(language);
        }

        protected override void InitializeData()
        {
            if (CurrentLanguage == "English")
            {
                /* To recognize the Game ID given the table window (including any prefixes such as T for tournament)
                 * ex. T1234567990 from €2.60+€0.40 EUR Hold'em No Limit [Heads-up Turbo] - Tournament 1234567990 - 1 on 1 - ...*/
                regex.Add("game_window_title_to_recognize_tournament_game_id", @"]? - (?<tournamentId>[^ ]+ [0-9]+) [^$]+$");
                regex.Add("game_window_title_to_recognize_play_money_game_description", @"(?<gameDescription>[^-]+)-[^-]+ Play Money");
                
                /* Recognize the Hand History game phases */
                regex.Add("hand_history_begin_preflop_phase_token", @"\*\*\* HOLE CARDS \*\*\*");
                regex.Add("hand_history_begin_flop_phase_token", @"\*\*\* FLOP \*\*\*");
                regex.Add("hand_history_begin_turn_phase_token", @"\*\*\* TURN \*\*\*");
                regex.Add("hand_history_begin_river_phase_token", @"\*\*\* RIVER \*\*\*");
                regex.Add("hand_history_begin_showdown_phase_token", @"\*\*\* SHOW DOWN \*\*\*");
                regex.Add("hand_history_begin_summary_phase_token", @"\*\*\* SUMMARY \*\*\*");
                

                /* Recognize the Hand History gameID and table ID */
                regex.Add("hand_history_game_id_token", @"PokerStars Game #(?<gameId>[\d]+):");
                regex.Add("hand_history_table_id_token", @"Table '(?<tableId>[\d ]+)'");

                /* Recognize game type (Hold'em, Omaha, No-limit, limit, etc.) 
                   Note that for PokerStars.it the only valid currency is EUR, but this might be different 
                   on other clients. This regex works for both play money and tournaments */
                regex.Add("hand_history_game_type_token", @"(EUR (?<gameType>[^-]+) -)|(PokerStars Game #[\d]+:  (?<gameType>[^(]+) \([\d]+/[\d]+\))");

                /* Recognize players 
                 Ex. Seat 1: stallion089 (2105 in chips) => "stallion089" 
                 */
                regex.Add("hand_history_detect_player_in_game", @"Seat [\d]+: (?<playerName>[^(]+) .*\([\d]+ in chips");

                /* Recognize mucked hands
                 Ex. Seat 1: stallion089 (button) (small blind) mucked [5d 5s]*/
                regex.Add("hand_history_detect_mucked_hand", @"Seat [\d]+: (?<playerName>[^(]+) .*(showed|mucked) \[(?<cards>[\d\w ]+)\]");

                /* Recognize the final board */
                regex.Add("hand_history_detect_final_board", @"Board \[(?<cards>[\d\w ]+)\]");

                /* Detect who is the small/big blind
                   Ex. stallion089: posts small blind 15 */
                regex.Add("hand_history_detect_small_blind", @"(?<playerName>[^:]+): posts small blind (?<smallBlindAmount>[\d]+)");
                regex.Add("hand_history_detect_big_blind", @"(?<playerName>[^:]+): posts big blind (?<bigBlindAmount>[\d]+)");


                /* Detect calls
                 * ex. stallion089: calls 10 */
                regex.Add("hand_history_detect_player_call", @"(?<playerName>[^:]+): calls (?<amount>[\d]+)");

                /* Detect bets 
                   ex. stallion089: bets 20 */
                regex.Add("hand_history_detect_player_bet", @"(?<playerName>[^:]+): bets (?<amount>[\d]+)");

                /* Detect folds
                 * ex. preferiti90: folds */
                regex.Add("hand_history_detect_player_fold", @"(?<playerName>[^:]+): folds");

                /* Detect raises 
                 * ex. zanzara za: raises 755 to 1155 and is all-in */
                regex.Add("hand_history_detect_player_raise", @"(?<playerName>[^:]+): raises (?<initialPot>[\d]+) to (?<raiseAmount>[\d]+)");

                /* Recognize end of round character sequence (in PokerStars.it it's
                 * a blank line */
                regex.Add("hand_history_detect_end_of_round", @"^$");

                /* Hand history file format. Example: HH20111216 T123456789 ... .txt */
                config.Add("hand_history_tournament_filename_format", "HH[0-9]+ {0}{1}");
                config.Add("hand_history_play_money_filename_format", "HH[0-9]+ {0}");

                /* Game description (as shown in the hand history) */
                config.Add("game_description_no_limit_holdem", "Hold'em No Limit");

            }

            /* Number of sequences required to raise the OnRoundHasTerminated event.
             * This refers to the hand_history_detect_end_of_round regex, on PokerStars.it
             * a round is over after 3 blank lines. Most clients might have only one line */
            config.Add("hand_history_end_of_round_number_of_tokens_required", 3);

            /* Hud display configuration */

            /* If you take the game window width, what's the factor to multiply by to obtain
             * the relative X position of the center of the table */
            config.Add("hud_table_center_x_factor_related_to_window_width", 0.5f);

            /* Since the Y position of the center of the table is always (assume) proportional
             * to the relative X position, we can obtain the Y position of the center of the table
             * by multiplying by this factor */
            config.Add("hud_table_center_y_factor_related_to_table_center_x", 0.6f);

            /* The distance in the X direction of a player from the center is proportional
             * to the position of the center (X) */
            config.Add("hud_player_x_distance_from_center_factor_related_to_center_x", 0.8f);

            /* Same for Y */
            config.Add("hud_player_y_distance_from_center_factor_related_to_center_y", 0.7f); 

            /* The drawing of the huds begins from the top center and goes clockwise
             * (if you think of a clock, it starts at noon). We need two angles: skip angle
             * which tells us the angle at which the first seat is drawn, and distance between players
             * which tells us the angle that exists between two players (in radians) 
             * The skip angle is obviously different depending on how many seats are available at
             * the table */
            // TODO 10 seats?
            config.Add("hud_9_seats_skip_angle", 0.65f);
            config.Add("hud_6_seats_skip_angle", 1.186f);
            config.Add("hud_2_seats_skip_angle", (float)Math.PI/2);

            config.Add("hud_distance_between_players_angle", (float)Math.PI/6);

        }

        /* Given a game description, returns the corresponding PokerGameType */
        public override PokerGameType GetPokerGameTypeFromGameDescription(string gameDescription)
        {
            if (gameDescription == (String)config["game_description_no_limit_holdem"]) return PokerGameType.Holdem;

            return PokerGameType.Unknown; //Default
        }

        /**
         * This function matches an open window title with patterns to recognize which hand history
         * the current window refers to (if it is even a poker game window). It will return an empty
         * string if it cannot match any pattern */
        public override String GetHandHistoryFilenameRegexPatternFromWindowTitle(String windowTitle)
        {
            /* Tricky, title format is significantly different for tournaments and play money on PokerStars.it
             * so we need to make two checks */
            Regex regex = GetRegex("game_window_title_to_recognize_tournament_game_id");
            Match match = regex.Match(windowTitle);
            if (match.Success)
            {
                // We matched a tournament game window
                // tournamentID = Tournament 123456789, we need T12345689
                String tournamentID = match.Groups["tournamentId"].Value;
                String[] parts = tournamentID.Split(' ');

                String prefix = parts[0].Substring(0,1); //T
                String gameID = parts[1];

                return String.Format(GetConfigString("hand_history_tournament_filename_format"), prefix, gameID);
            }
            else
            {
                // No luck, try with play money
                regex = GetRegex("game_window_title_to_recognize_play_money_game_description");
                match = regex.Match(windowTitle);
                if (match.Success)
                {
                    string gameDescription = match.Groups["gameDescription"].Value;
                    
                    // We matched a play money game window, need to convert the description into a filename friendly format
                    return String.Format(GetConfigString("hand_history_play_money_filename_format"),gameDescription);
                }
                else
                {
                    return String.Empty; //Could not find any valid match... must be a title we're not interested into
                }
            }
        }

        /* Cards on PokerStars.it are represented by two chars, the first indicating the face
         * and the second indicating the suit. Ex. Ks, Ah, etc. */
        public override Card GenerateCardFromString(String card)
        {
            // This should never be different than 2
            Debug.Assert(card.Length == 2, "A string representation of a card was found to be of invalid length: " + card.Length + " instead of 2");

            // Uppercase to simplify checks
            String cardValues = card.ToUpper();

            // Extract components
            Char faceComponent = cardValues[0];
            Char suitComponent = cardValues[1];

            CardFace face = CharToCardFace(faceComponent);
            CardSuit suit = CharToCardSuit(suitComponent);

            Card result = new Card(face, suit);
            return result;
        }

        /* Helper method to convert a char into a CardFace enum value */
        private CardFace CharToCardFace(Char c)
        {
            switch (c)
            {
                case 'A': return CardFace.Ace;
                case '2': return CardFace.Two;
                case '3': return CardFace.Three;
                case '4': return CardFace.Four;
                case '5': return CardFace.Five;
                case '6': return CardFace.Six;
                case '7': return CardFace.Seven;
                case '8': return CardFace.Eight;
                case '9': return CardFace.Nine;
                case 'T': return CardFace.Ten;
                case 'J': return CardFace.Jack;
                case 'Q': return CardFace.Queen;
                case 'K': return CardFace.King;
                default: 
                    Debug.Assert(false,"Invalid char detected during conversion to CardFace: " + c);
                    return CardFace.Ace; // Never to be executed
            }
        }

        /* Helper method to convert a char into a CardSuit enum value */
        private CardSuit CharToCardSuit(Char c)
        {
            switch (c)
            {
                case 'S': return CardSuit.Spades;
                case 'C': return CardSuit.Clubs;
                case 'D': return CardSuit.Diamonds;
                case 'H': return CardSuit.Hearts;
                default:
                    Debug.Assert(false, "Invalid char detected during conversion to CardSuit: " + c);
                    return CardSuit.Hearts; // Never to be executed
            }
        }

        public override String Name {
            get
            {
                return "PokerStars.IT";
            }
        }

        public override ArrayList SupportedLanguages
        {
            get
            {
                ArrayList languages = new ArrayList();
                languages.Add("English");
                return languages;
            }
        }

        public override ArrayList SupportedGameModes
        {
            get {
                ArrayList supportedGameModes = new ArrayList();
                supportedGameModes.Add("No Limit Hold'em"); //TODO CHECK

                return supportedGameModes;
            }
        }

        protected override RegexOptions regexOptions{
            get{
                return RegexOptions.IgnoreCase | RegexOptions.Compiled;
            }
        }
    }
}
