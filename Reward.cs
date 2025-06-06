using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apostador.Models
{
    class Reward
    {
        public int Amount { get; set; }
        public double Probability { get; set; }
        public string Description { get; set; }

        public int gainedTimes { get; private set; }

        public Reward(int amount, double probability, string description)
        {
            Description = description;
            Probability = probability;
            Amount = amount;
            gainedTimes = 0;
        }

        public int countGain()
        {
            gainedTimes++;

            return gainedTimes;
        }
        public int resetGainedTimes()
        {
            gainedTimes = 0; return gainedTimes;
        }

    }
}
