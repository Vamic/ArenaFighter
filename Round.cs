namespace Util
{
    class Round
    {
        Character firstCharacter;
        Character secondCharacter;
        string summary;

        public bool IsFinal { get; set; }

        public Round(Character initiativeCharacter, Character opponent)
        {
            firstCharacter = initiativeCharacter;
            secondCharacter = opponent;
        }

        public override string ToString()
        {
            return summary;
        }

        internal void DoRound()
        {
            //Name missed / Name hit for X damage
            summary += firstCharacter + " " +  firstCharacter.Attack(secondCharacter);
            if (!secondCharacter.IsAlive)
                summary += " ^*KILLING BLOW*^";
            else
                summary += "\n" + secondCharacter + " " + secondCharacter.Attack(firstCharacter);

            if (!firstCharacter.IsAlive)
                summary += " ^*KILLING BLOW*^";
        }
    }
}
