using ABP.Application.Interfaces;

namespace ABP.Application.Services
{
    public class ChanceBasedOutputService : IChanceBasedOutputService
    {
        public string GetRandomValue<TKey, TValue>(Dictionary<TKey, TValue> pairs) where TValue : struct
        {
            double totalProbability = 0;

            if (typeof(TValue) == typeof(IEnumerable<decimal>))
            {
                var valueList = pairs.Values;
                totalProbability = (double)valueList.Cast<decimal>().Sum();
            }

            if (totalProbability > 1)
                throw new InvalidOperationException("The sum of probabilities can't be more then 1");

            double randomNumber = new Random().NextDouble() * totalProbability;

            double cumulativeProbability = 0;
            foreach (KeyValuePair<TKey, TValue> entry in pairs)
            {
                if (entry.Value is double value)
                {
                    cumulativeProbability += value;
                    if (randomNumber <= cumulativeProbability)
                    {
                        if(entry.Key is string str)
                        {
                            return str;
                        }
                    }
                }
            }

            throw new InvalidOperationException("The dictionary is empty or the probabilities are incorrect.");
        }
    }
}
