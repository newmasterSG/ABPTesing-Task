using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABP.Application.Interfaces
{
    public interface IChanceBasedOutputService
    {
        string GetRandomValue(Dictionary<string, double> pairs);
    }
}
