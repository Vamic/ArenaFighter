using System;
using System.Collections.Generic;

namespace Util
{
    class Match
    {
        string matchId;
        Character player;
        Character opponent;
        List<Round> rounds;
        Round currentRound;
        bool playerGoesFirst;
        int playerHPLeft;
        int opponentHPLeft;

        static int InstanceCount { get; set; }

        public Match(Character player, Character opponent)
        {
            this.player = player;
            this.opponent = opponent;
            rounds = new List<Round>();
            InstanceCount++;
            matchId = "Match-" + InstanceCount;
            RollInitiative();
        }

        private void RollInitiative()
        {
            int pInit = player.Stats.Dexterity + new Dice(DiceSize.Ten).Roll();
            int oInit = opponent.Stats.Dexterity + new Dice(DiceSize.Ten).Roll();
            if(pInit == oInit)
            {
                if (Game.RNG.Next(2) == 0)
                    pInit++;
                else
                    oInit++;
            }
            playerGoesFirst = pInit > oInit;
        }

        public Round NextRound()
        {
            if(currentRound != null)
                rounds.Add(currentRound);
            if(playerGoesFirst)
                currentRound = new Round(player, opponent);
            else
                currentRound = new Round(opponent, player);
            currentRound.DoRound();
            return currentRound;
        }

        public void Finish()
        {
            playerHPLeft = player.CurrentHealth;
            opponentHPLeft = opponent.CurrentHealth;
        }

        public override string ToString()
        {
            return matchId + ": " + player + "("+ playerHPLeft + " HP) vs " + opponent + "(" + opponentHPLeft + " HP)";
        }
    }
}
