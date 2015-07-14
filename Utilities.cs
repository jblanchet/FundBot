using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FundBot
{
  public static class Utilities
  {
    public static float AmountChanged(float first, float second)
    {
      return ((second - first)/first) * 100.0f;
    }
  }
}
